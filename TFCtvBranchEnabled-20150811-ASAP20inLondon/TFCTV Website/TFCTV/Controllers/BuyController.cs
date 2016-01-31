using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DevTrends.MvcDonutCaching;
using Gigya.Socialize.SDK;
using GOMS_TFCtv;
using IPTV2_Model;
using Newtonsoft.Json.Linq;
using TFCTV.Helpers;
using TFCTV.Models;
using StackExchange.Profiling;

namespace TFCTV.Controllers
{
    public class BuyController : Controller
    {
        //
        // GET: /Buy/

        [HttpPost]
        public ActionResult _Wallet(int? id, string wid, int? cpid)
        {
            string errorMessage = "";
            if (id == null)
                return PartialView("PaymentTemplates/_BuyErrorPartial");

            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("errorCode", (int)ErrorCodes.UnknownError);
            collection.Add("errorMessage", MyUtility.getErrorMessage(ErrorCodes.UnknownError));
            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product == null)
                {
                    collection = MyUtility.setError(ErrorCodes.ProductIsNull);
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                if (!product.IsForSale)
                {
                    collection = MyUtility.setError(ErrorCodes.ProductIsNotPurchaseable);
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                string productName = product.Name;
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                User recipient = null;
                //Get Recipient
                if (!String.IsNullOrEmpty(wid))
                {
                    GSArray gsarray = GigyaMethods.GetWishlistDetails(wid);
                    if (gsarray != null)
                    {
                        JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                        System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                        recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                        ViewBag.RecipientEmail = recipient.EMail;
                        collection.Add("recipient", String.Format("{0} {1}", recipient.FirstName, recipient.LastName));
                    }

                    if (subscriptionType == SubscriptionProductType.Package)
                    {
                        //Check if user has a current subscription
                        if (!IsProductGiftable(context, product, recipient == null ? user.UserId : recipient.UserId))
                        {
                            string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                            errorMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                            if (recipient != null)
                            {
                                if (recipient.UserId != user.UserId) // same user
                                    errorMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                            }
                            collection = MyUtility.setError(ErrorCodes.ProductIsNotPurchaseable, errorMessage);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                    }
                }

                if (subscriptionType == SubscriptionProductType.Package)
                {
                    if (!IsProductRestricted(context, product, recipient == null ? user.UserId : recipient.UserId))
                    {
                        //string productName = ContextHelper.GetProductName(context, product);
                        errorMessage = String.Format("Sorry! The ({0}) subscription is not available in your country.", productName);
                        if (recipient != null)
                        {
                            if (recipient.UserId != user.UserId) // same user
                                errorMessage = String.Format("Sorry! The ({0}) subscription is not available in the country where {1} {2} is. You are not allowed to send this gift.", productName, recipient.FirstName, recipient.LastName);
                        }
                        collection = MyUtility.setError(ErrorCodes.ProductIsNotAllowedInCountry, errorMessage);
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }
                    if (String.IsNullOrEmpty(wid))
                    {
                        if (!IsProductPurchaseable(context, product, recipient == null ? user.UserId : recipient.UserId))
                        {
                            string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                            errorMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                            if (recipient != null)
                            {
                                if (recipient.UserId != user.UserId) // same user
                                    errorMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                            }
                            collection = MyUtility.setError(ErrorCodes.ProductIsNotPurchaseable, errorMessage);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                    }
                }

                if (user != null)
                {
                    if (offering != null)
                    {
                        if (user.HasPendingGomsChangeCountryTransaction(offering))
                        {
                            errorMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                            collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }

                        var registDt = DateTime.Now;
                        if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                        {
                            errorMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                            collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                    }

                    ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                    UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                    ErrorCodes errorCode = ErrorCodes.UnknownError;
                    if (wallet.Balance >= priceOfProduct.Amount)
                    {
                        ErrorResponse response = PaymentHelper.PayViaWallet(context, userId, product.ProductId, subscriptionType, recipient == null ? userId : recipient.UserId, cpid);
                        errorCode = (ErrorCodes)response.Code;

                        string CountryCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                        Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CountryCode);
                        collection["errorCode"] = (int)errorCode;
                        if (errorCode == ErrorCodes.Success)
                        {
                            if (!String.IsNullOrEmpty(wid))
                            {
                                GigyaMethods.DeleteWishlist(wid); // Delete from Wishlist
                                SendGiftShareToSocialNetwork(product, recipient);
                                ReceiveGiftShareToSocialNetwork(product, recipient, user);
                            }

                            string balance;
                            if (currency.IsLeft)
                                balance = String.Format("Your new wallet balance is {0}{1}.", currency.Symbol, wallet.Balance.ToString("F"));
                            else
                                balance = String.Format("Your new wallet balance is {0}{1}.", wallet.Balance.ToString("F"), currency.Symbol);
                            balance = String.Format("Your new wallet balance is {0} {1}.", currency.Code, wallet.Balance.ToString("F"));

                            collection["errorMessage"] = balance;
                            collection.Add("wid", wid);
                        }
                        else
                            collection.Add("errorMessage", MyUtility.getErrorMessage(errorCode));
                    }
                    else
                    {
                        errorCode = ErrorCodes.InsufficientWalletLoad;
                        collection["errorCode"] = (int)errorCode;
                        collection["errorMessage"] = MyUtility.getErrorMessage(errorCode);
                    }
                }
            }
            else
            {
                collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
                collection["errorMessage"] = "Your session has expired. Please login again.";
            }

            return Content(MyUtility.buildJson(collection), "application/json");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Wallet(int? id, string wid, int? cpid)
        {
            if (!GlobalConfig.IsEWalletPaymentModeEnabled)
                return PartialView("PaymentTemplates/_BuyErrorPartial");
            if (id == null)
                return PartialView("PaymentTemplates/_BuyErrorPartial");

            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                var registDt = DateTime.Now;
                if (user != null)
                {

                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                        return PartialView("PaymentTemplates/_BuyPendingChangeCountryErrorPartial");

                    if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                        return PartialView("PaymentTemplates/_BuyErrorExceededMaximumThresholdPartial");

                    Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
                    if (product == null)
                        return PartialView("PaymentTemplates/_BuyErrorPartial");
                    if (!product.IsForSale)
                        return PartialView("PaymentTemplates/_BuyErrorPartial");

                    ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                    if (priceOfProduct == null) // not available on your country
                        return PartialView("PaymentTemplates/_BuyErrorPartial");
                    UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                    ViewBag.ProductId = product.ProductId;
                    //Check if wallet has bigger amount than product then tick isBuyable if condition is true.
                    ViewBag.isBuyable = (wallet.Balance >= priceOfProduct.Amount) ? true : false;
                    string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                    Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                    ViewBag.WalletBalance = wallet.Balance.ToString("F");
                    ViewBag.Currency = currency.Code;
                    ViewBag.WishlistId = wid;
                    ViewBag.CurrentProductId = cpid;
                    ViewBag.User = user;
                    if (currency.IsLeft)
                        ViewBag.WalletBalanceFormat = String.Format("Your TFC.tv wallet has a balance of {0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
                    else
                        ViewBag.WalletBalanceFormat = String.Format("Your TFC.tv wallet has a balance of {0}{1}", wallet.Balance.ToString("F"), currency.Symbol);

                    ViewBag.IsSubscriptionProduct = false;
                    if (product is SubscriptionProduct)
                        ViewBag.IsSubscriptionProduct = true;
                    if (GlobalConfig.IsRecurringBillingEnabled)
                    {
                        var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                        if (checkIfEnrolled.value)
                            return PartialView("PaymentTemplates/_IsEnrolledToRecurringPartial", checkIfEnrolled.container);
                    }
                    return PartialView("PaymentTemplates/_BuyViaWalletPartial");
                }
            }
            return PartialView("PaymentTemplates/_BuyAuthenticationErrorPartial");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Process(int? id, string wid, int? cpid)
        {
            bool isFirstTimeSubscriber = false; /** ADDED JAN 2 2013 **/

            ViewBag.Tab = "0";


            if (id == null)
                return RedirectToAction("Index", "Home");

            if (!MyUtility.isUserLoggedIn())
                return PartialView("PaymentTemplates/_BuyAuthenticationErrorPartial");

            var context = new IPTV2Entities();
            string CountryCode = GlobalConfig.DefaultCountry;
            CountryCode = MyUtility.GetCurrentCountryCodeOrDefault();

            DateTime registDt = DateTime.Now;


            try
            {
                User user = null;
                if (MyUtility.isUserLoggedIn())
                {
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        CountryCode = user.CountryCode;
                }

                int productId = (int)id;

                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                if (product == null)
                    return RedirectToAction("Index", "Home");
                SubscriptionProductType type = ContextHelper.GetProductType(product);

                if (!product.IsForSale)
                    return PartialView("PaymentTemplates/_BuyErrorPartial");

                if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                    return PartialView("PaymentTemplates/_BuyErrorExceededMaximumThresholdPartial");

                User recipient = null;
                string productName = product.Name;
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                if (!String.IsNullOrEmpty(wid))
                {
                    GSArray gsarray = GigyaMethods.GetWishlistDetails(wid);
                    if (gsarray != null)
                    {
                        JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                        productId = Convert.ToInt32(o["ProductId_i"].ToString());
                        System.Guid userId = new System.Guid(o["UID_s"].ToString());
                        recipient = context.Users.FirstOrDefault(u => u.UserId == userId);
                        ViewBag.Recipient = recipient;
                        ViewBag.WishlistId = wid;
                        if (MyUtility.isUserLoggedIn())
                        {
                            if (type == SubscriptionProductType.Package)
                            {
                                //Check if user has a current subscription
                                if (!IsProductGiftable(context, product, recipient == null ? user.UserId : recipient.UserId))
                                {
                                    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                                    ViewBag.ErrorMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                    if (recipient != null)
                                    {
                                        if (recipient.UserId != user.UserId) // same user
                                            ViewBag.ErrorMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                                    }
                                }
                            }
                        }
                    }
                }

                switch (type)
                {
                    case SubscriptionProductType.Package:
                        if (MyUtility.isUserLoggedIn())
                        {
                            if (!IsProductRestricted(context, product, recipient == null ? user.UserId : recipient.UserId))
                            {
                                //string productName = ContextHelper.GetProductName(context, product);
                                ViewBag.ErrorMessage = String.Format("Sorry! The ({0}) subscription is not available in your country.", productName);
                                if (recipient != null)
                                {
                                    if (recipient.UserId != user.UserId) // same user
                                        ViewBag.ErrorMessage = String.Format("Sorry! The ({0}) subscription is not available in the country where {1} {2} is. You are not allowed to send this gift.", productName, recipient.FirstName, recipient.LastName);
                                }
                            }
                            if (String.IsNullOrEmpty(wid))
                            {
                                if (!IsProductPurchaseable(context, product, recipient == null ? user.UserId : recipient.UserId))
                                {
                                    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                                    ViewBag.ErrorMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                    if (recipient != null)
                                    {
                                        if (recipient.UserId != user.UserId) // same user
                                            ViewBag.ErrorMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                                    }
                                }
                            }


                            /** ADDED JAN 2 2013 **/
                            if (GlobalConfig.IsEarlyBirdEnabled)
                            {
                                if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                                {
                                    int convertedDays = ContextHelper.GetConvertedDaysFromFreeTrial(user);
                                    ViewBag.FreeTrialConvertedDays = convertedDays;

                                    var freeTrialPackageIds = MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds);
                                    var freeTrialEntitlement = user.PackageEntitlements.FirstOrDefault(p => freeTrialPackageIds.Contains(p.PackageId) && p.EndDate > registDt);
                                    if (freeTrialEntitlement != null)
                                        ViewBag.FreeTrialEndDate = freeTrialEntitlement.EndDate;

                                    isFirstTimeSubscriber = true;

                                }
                            }
                        }
                        //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                        //isFirstTimeSubscriber = false;
                        ViewBag.IsFirstTimeSubscriber = isFirstTimeSubscriber;

                        ProductPackage pp = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId); ViewBag.PackageName = pp.Package.PackageName;
                        var PackageSubscriptionEndDate = GetSubscriptionEndDate(user, pp.PackageId);
                        ViewBag.SubscriptionEndDate = PackageSubscriptionEndDate;

                        if (isFirstTimeSubscriber)
                            ViewBag.PackageToBeBoughtEntitlementEndDate = MyUtility.getEntitlementEndDate(pp.Product.Duration, pp.Product.DurationType, registDt);

                        DateTime RenewalSubscriptionEndDate = registDt;
                        if (PackageSubscriptionEndDate != null)
                        {
                            RenewalSubscriptionEndDate = MyUtility.getEntitlementEndDate(pp.Product.Duration, pp.Product.DurationType, (DateTime)PackageSubscriptionEndDate);
                            ViewBag.RenewalSubscriptionEndDate = RenewalSubscriptionEndDate;
                        }

                        if (cpid != null && cpid != 0)
                        {
                            ProductPackage currentPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == cpid);
                            ViewBag.CurrentPackageName = currentPackage.Package.PackageName;
                            var currentPackageSubscriptionEndDate = GetSubscriptionEndDate(user, currentPackage.PackageId);
                            ViewBag.CurrentSubscriptionEndDate = PackageSubscriptionEndDate;
                            ViewBag.CurrentProductId = cpid;
                            if (currentPackageSubscriptionEndDate != null)
                            {
                                //Get remaining number of days left on your current subscription
                                TimeSpan remainingTsofCurrentSubscription = ((DateTime)currentPackageSubscriptionEndDate).Subtract(registDt);
                                int remainingDaysofCurrentSubscription = (int)remainingTsofCurrentSubscription.TotalDays;
                                ViewBag.remainingDaysofCurrentSubscription = remainingDaysofCurrentSubscription;
                                ViewBag.CurrentSubscriptionName = currentPackage.Package.PackageName;
                                ViewBag.NewSubscriptionName = pp.Package.PackageName;
                                ViewBag.NewSubscriptionProductName = pp.Product.Name;
                                //
                                TimeSpan newSubscriptionConvertedToDays = MyUtility.getEntitlementEndDate(pp.Product.Duration, pp.Product.DurationType, registDt).Subtract(registDt);
                                ViewBag.NewSubscriptionTotalDays = (int)newSubscriptionConvertedToDays.TotalDays;

                                var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == currentPackage.PackageId);
                                var EquivalentPremiumDuration = GetEquivalentPremiumDuration(context, user.Country.CurrencyCode, entitlement);
                                ViewBag.ConvertedDays = (int)EquivalentPremiumDuration;

                                ViewBag.CombinedSubscriptionTotalDays = (int)newSubscriptionConvertedToDays.TotalDays + (int)EquivalentPremiumDuration;
                            }

                            //Get the converted number of  days
                        }

                        break;
                    case SubscriptionProductType.Show: ProductShow ps = context.ProductShows.FirstOrDefault(p => p.ProductId == product.ProductId); ViewBag.PackageName = ps.Show.Description;
                        var ShowSubscriptionEndDate = GetSubscriptionEndDate(user, ps.CategoryId);
                        ViewBag.SubscriptionEndDate = ShowSubscriptionEndDate;
                        if (ShowSubscriptionEndDate != null)
                            ViewBag.RenewalSubscriptionEndDate = MyUtility.getEntitlementEndDate(ps.Product.Duration, ps.Product.DurationType, (DateTime)ShowSubscriptionEndDate);

                        /** ADDED JAN 09 2013 **/
                        if (MyUtility.isUserLoggedIn())
                        {
                            if (GlobalConfig.IsEarlyBirdEnabled)
                            {
                                if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                                {
                                    int convertedDays = ContextHelper.GetConvertedDaysFromFreeTrial(user);
                                    ViewBag.FreeTrialConvertedDays = convertedDays;

                                    var freeTrialPackageIds = MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds);
                                    var freeTrialEntitlement = user.PackageEntitlements.FirstOrDefault(p => freeTrialPackageIds.Contains(p.PackageId) && p.EndDate > registDt);
                                    if (freeTrialEntitlement != null)
                                        ViewBag.FreeTrialEndDate = freeTrialEntitlement.EndDate;

                                    isFirstTimeSubscriber = true;

                                }
                            }
                            //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                            //isFirstTimeSubscriber = false;
                            ViewBag.IsFirstTimeSubscriber = isFirstTimeSubscriber;

                            if (isFirstTimeSubscriber)
                                ViewBag.PackageToBeBoughtEntitlementEndDate = MyUtility.getEntitlementEndDate(ps.Product.Duration, ps.Product.DurationType, registDt);
                        }

                        break;
                    case SubscriptionProductType.Episode: ProductEpisode pe = context.ProductEpisodes.FirstOrDefault(p => p.ProductId == product.ProductId);
                        EpisodeCategory category = pe.Episode.EpisodeCategories.FirstOrDefault(e => e.EpisodeId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible);
                        ViewBag.PackageName = String.Format("{0}, {1}", category.Show.Description, pe.Episode.DateAired.Value.ToString("MMM. dd, yyyy"));
                        var EpisodeSubscriptionEndDate = GetSubscriptionEndDate(user, pe.EpisodeId);
                        ViewBag.SubscriptionEndDate = EpisodeSubscriptionEndDate;
                        if (EpisodeSubscriptionEndDate != null)
                            ViewBag.RenewalSubscriptionEndDate = MyUtility.getEntitlementEndDate(pe.Product.Duration, pe.Product.DurationType, (DateTime)EpisodeSubscriptionEndDate);
                        break;
                }

                ViewBag.Product = product;

                var price = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(CountryCode));
                if (price == null)
                    price = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);

                if (MyUtility.isUserLoggedIn())
                {
                    //Check which tab opens first
                    ViewBag.Tab = 1; //Prepaid Card
                    UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                    if (wallet != null)
                    {
                        if (wallet.Balance >= price.Amount)
                            ViewBag.Tab = 0;
                    }

                    //ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                    //UPDATED  05/07/2013
                    if (GlobalConfig.IsMECreditCardEnabled)
                    {
                        try
                        {
                            if (user.Country.GomsSubsidiaryId != GlobalConfig.MiddleEastGomsSubsidiaryId)
                                ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                            else
                            {
                                var listOfMiddleEastAllowedCountries = GlobalConfig.MECountriesAllowedForCreditCard.Split(','); //get list of middle east countries allowed for credit card payment
                                if (listOfMiddleEastAllowedCountries.Contains(user.Country.Code))
                                    ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                                else
                                    ViewBag.CreditCardList = null;
                            }
                        }
                        catch (Exception) { ViewBag.CreditCardList = null; }
                    }
                    else
                        ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();

                }
                var currency = price.Currency;
                if (currency.IsLeft)
                    ViewBag.ProductAmountFormat = String.Format("{0}{1}", currency.Symbol, price.Amount.ToString("F"));
                else
                    ViewBag.ProductAmountFormat = String.Format("{0}{1}", price.Amount.ToString("F"), currency.Symbol);
                ViewBag.ProductAmountFormat = String.Format("{0} {1}", currency.Code, price.Amount.ToString("F"));
                if (!currency.IsPayPalSupported)
                {
                    price = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);
                    ViewBag.ProductAmountFormat += String.Format(" / {0} {1} (Paypal only)", price.CurrencyCode, price.Amount.ToString("F"));

                }

                ViewBag.IsSubscriptionProduct = false;
                if (product is SubscriptionProduct)
                    ViewBag.IsSubscriptionProduct = true;

                //var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                //if (checkIfEnrolled.value)
                //    return PartialView("PaymentTemplates/_IsEnrolledToRecurringPartial", checkIfEnrolled.container);

                return PartialView("PaymentTemplates/_BuyProcessPartial");
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                return PartialView("PaymentTemplates/_BuyErrorEncounteredPartial");
            }

        }

        private DateTime? GetSubscriptionEndDate(User user, int id)
        {
            if (MyUtility.isUserLoggedIn())
            {
                foreach (var entitlement in user.Entitlements.Where(e => DateTime.Now < e.EndDate).ToList())
                {
                    if (entitlement is PackageEntitlement)
                    {
                        var pe = (PackageEntitlement)entitlement;
                        if (pe.PackageId == id)
                            return pe.EndDate;
                        //return TimeZone.CurrentTimeZone.ToLocalTime(pe.EndDate);
                    }
                    else if (entitlement is ShowEntitlement)
                    {
                        var se = (ShowEntitlement)entitlement;
                        if (se.CategoryId == id)
                            return se.EndDate;
                        //return TimeZone.CurrentTimeZone.ToLocalTime(se.EndDate);
                    }
                    else if (entitlement is EpisodeEntitlement)
                    {
                        var ee = (EpisodeEntitlement)entitlement;
                        if (ee.EpisodeId == id)
                            return ee.EndDate;
                        //return TimeZone.CurrentTimeZone.ToLocalTime(ee.EndDate);
                    }
                }
            }
            return null;
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult PrepaidCard(int? id, string wid)
        {
            if (!GlobalConfig.IsPpcPaymentModeEnabled)
                return PartialView("PaymentTemplates/_BuyErrorPartial");
            if (id == null)
                return PartialView("PaymentTemplates/_BuyErrorPartial");

            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    var registDt = DateTime.Now;
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                        return PartialView("PaymentTemplates/_BuyPendingChangeCountryErrorPartial");

                    if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                        return PartialView("PaymentTemplates/_BuyErrorExceededMaximumThresholdPartial");

                    Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
                    if (product == null)
                        return PartialView("PaymentTemplates/_BuyErrorPartial");
                    if (!product.IsForSale)
                        return PartialView("PaymentTemplates/_BuyErrorPartial");

                    ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                    ViewBag.ProductId = product.ProductId;
                    ViewBag.User = user;
                    ViewBag.WishlistId = wid;

                    ViewBag.IsSubscriptionProduct = false;
                    if (product is SubscriptionProduct)
                        ViewBag.IsSubscriptionProduct = true;
                    if (GlobalConfig.IsRecurringBillingEnabled)
                    {
                        var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                        if (checkIfEnrolled.value)
                            return PartialView("PaymentTemplates/_IsEnrolledToRecurringPartial", checkIfEnrolled.container);
                    }
                    return PartialView("PaymentTemplates/_BuyViaPrepaidCardPartial");
                }
            }
            return PartialView("PaymentTemplates/_BuyAuthenticationErrorPartial");
        }

        [HttpPost]
        public ActionResult _PrepaidCard(int? id, string wid, int? cpid, FormCollection fc)
        {
            if (id == null)
                return PartialView("PaymentTemplates/_BuyErrorPartial");

            Dictionary<string, object> collection = new Dictionary<string, object>();
            Ppc.ErrorCode errorCode = Ppc.ErrorCode.UnknownError;
            string errorMessage = MyUtility.getPpcError(errorCode);
            collection.Add("errorCode", (int)errorCode);
            collection.Add("errorMessage", errorMessage);

            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product == null)
                {
                    collection = MyUtility.setError(ErrorCodes.ProductIsNull);
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                if (!product.IsForSale)
                {
                    collection = MyUtility.setError(ErrorCodes.ProductIsNotPurchaseable);
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                string productName = product.Name;
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                if (user != null)
                {
                    if (offering != null)
                    {
                        if (user.HasPendingGomsChangeCountryTransaction(offering))
                        {
                            errorMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                            collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }

                        var registDt = DateTime.Now;
                        if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                        {
                            errorMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                            collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                    }

                    string serial = fc["serialnumber"];
                    string pin = fc["pin"];
                    serial = serial.Replace(" ", "");
                    pin = pin.Replace(" ", "");
                    //Get Recipient
                    User recipient = null;
                    if (!String.IsNullOrEmpty(wid))
                    {
                        GSArray gsarray = GigyaMethods.GetWishlistDetails(wid);
                        if (gsarray != null)
                        {
                            JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                            System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                            recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                            ViewBag.RecipientEmail = recipient.EMail;
                            collection.Add("recipient", String.Format("{0} {1}", recipient.FirstName, recipient.LastName));
                        }

                        if (subscriptionType == SubscriptionProductType.Package)
                        {
                            //Check if user has a current subscription
                            if (!IsProductGiftable(context, product, recipient == null ? user.UserId : recipient.UserId))
                            {
                                string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                                errorMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                if (recipient != null)
                                {
                                    if (recipient.UserId != user.UserId) // same user
                                        errorMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                                }
                                collection = MyUtility.setError(ErrorCodes.ProductIsNotPurchaseable, errorMessage);
                                return Content(MyUtility.buildJson(collection), "application/json");
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(pin))
                        pin = MyUtility.GetSHA1(pin);
                    ErrorResponse response = PaymentHelper.PayViaPrepaidCard(context, userId, product.ProductId, subscriptionType, serial, pin, recipient == null ? userId : recipient.UserId, cpid);
                    errorCode = (Ppc.ErrorCode)response.Code;

                    switch (errorCode)
                    {
                        case Ppc.ErrorCode.Success:
                            if (!String.IsNullOrEmpty(wid))
                            {
                                GigyaMethods.DeleteWishlist(wid); // Delete from Wishlist
                                SendGiftShareToSocialNetwork(product, recipient);
                                ReceiveGiftShareToSocialNetwork(product, recipient, user);
                            }
                            collection.Add("wid", wid);
                            errorMessage = String.Format("Well done! You have successfully subscribed to {0}", product.Description); break;
                        default: errorMessage = MyUtility.getPpcError(errorCode); break;
                    }
                    collection["errorCode"] = (int)errorCode;
                    collection["errorMessage"] = errorMessage;
                }
            }
            else
            {
                collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
                collection.Add("errorMessage", "Your session has expired. Please login again.");
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        //[HttpPost]
        //public ActionResult _CreditCard(int? id, string wid, FormCollection fc)
        //{
        //    if (id == null)
        //        return PartialView("PaymentTemplates/_BuyErrorPartial");

        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    ErrorCodes errorCode = ErrorCodes.UnknownError;
        //    string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
        //    collection.Add("errorCode", 1);
        //    collection.Add("errorMessage", errorMessage);

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var context = new IPTV2Entities();
        //        System.Guid userId = new System.Guid(User.Identity.Name);
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);

        //        Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
        //        if (product == null)
        //            return PartialView("PaymentTemplates/_BuyErrorPartial");
        //        SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);

        //        if (user != null)
        //        {
        //            decimal amount = Convert.ToDecimal(fc["amount"]);
        //            int cctype = Convert.ToInt32(fc["CreditCard"]);
        //            string name = fc["cardholdername"];
        //            string cardnumber = fc["cardnumber"];
        //            string securitycode = fc["securitycode"];
        //            string expirymonth = fc["ExpiryMonth"];
        //            string expiryyear = fc["ExpiryYear"];
        //            string address = fc["address"];
        //            string city = fc["city"];
        //            string zip = fc["zip"];

        //            CreditCardInfo info = new CreditCardInfo()
        //            {
        //                CardType = (IPTV2_Model.CreditCardType)cctype,
        //                Name = name,
        //                Number = cardnumber,
        //                CardSecurityCode = securitycode,
        //                ExpiryMonth = Convert.ToInt32(expirymonth),
        //                ExpiryYear = Convert.ToInt32(expiryyear),
        //                PostalCode = zip,
        //                StreetAddress = String.Format("{0}{1}", address, String.IsNullOrEmpty(city) ? "" : (", " + city))
        //            };

        //            //Get Recipient
        //            User recipient = null;
        //            if (!String.IsNullOrEmpty(wid))
        //            {
        //                GSArray gsarray = GigyaMethods.GetWishlistDetails(wid);
        //                if (gsarray != null)
        //                {
        //                    JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
        //                    System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
        //                    recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
        //                    ViewBag.RecipientEmail = recipient.EMail;
        //                    collection.Add("recipient", String.Format("{0} {1}", recipient.FirstName, recipient.LastName));
        //                }
        //            }

        //            errorCode = PaymentHelper.PayViaCreditCard(context, userId, info, product.ProductId, subscriptionType, recipient == null ? userId : recipient.UserId);
        //            switch (errorCode)
        //            {
        //                case ErrorCodes.IsProductIdInvalidPpc: errorMessage = "Please use proper card for this product."; break;
        //                case ErrorCodes.IsReloadPpc: errorMessage = "Card is invalid. Type is for reloading wallet."; break;
        //                case ErrorCodes.IsInvalidCombinationPpc: errorMessage = "Invalid serial/pin combination."; break;
        //                case ErrorCodes.IsExpiredPpc: errorMessage = "Prepaid card is expired."; break;
        //                case ErrorCodes.IsInvalidPpc: errorMessage = "Serial does not exist."; break;
        //                case ErrorCodes.IsUsedPpc: errorMessage = "Ppc is already used."; break;
        //                case ErrorCodes.IsNotValidInCountryPpc: errorMessage = "Ppc not valid in your country."; break;
        //                case ErrorCodes.CreditCardHasExpired: errorMessage = "Your credit card has already expired."; break;
        //                case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
        //                case ErrorCodes.Success:
        //                    {
        //                        if (!String.IsNullOrEmpty(wid))
        //                        {
        //                            GigyaMethods.DeleteWishlist(wid); // Delete from Wishlist
        //                            SendGiftShareToSocialNetwork(product, recipient);
        //                            ReceiveGiftShareToSocialNetwork(product, recipient, user);
        //                        }
        //                        collection.Add("wid", wid);
        //                        errorMessage = "Congratulations! You have now bought this product."; break;
        //                    }
        //                default: errorMessage = "Unknown error."; break;
        //            }
        //            collection["errorCode"] = (int)errorCode;
        //            collection["errorMessage"] = errorMessage;
        //        }
        //    }
        //    else
        //    {
        //        collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
        //        collection.Add("errorMessage", "Your session has expired. Please login again.");
        //    }
        //    return Content(MyUtility.buildJson(collection), "application/json");
        //}

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult CreditCard(int? id, string wid)
        {
            if (!GlobalConfig.IsCreditCardPaymentModeEnabled)
                return PartialView("PaymentTemplates/_CreditCardDownTimePartial");
            ArrayList list = new ArrayList();
            for (int i = 0; i < 12; i++)
            {
                SelectListItem item = new SelectListItem()
                {
                    Value = (i + 1).ToString(),
                    Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(i + 1)
                };

                list.Add(item);
            }
            ViewBag.Months = list;

            list = new ArrayList();
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i <= currentYear + 20; i++)
            {
                SelectListItem item = new SelectListItem()
                {
                    Value = i.ToString(),
                    Text = i.ToString()
                };

                list.Add(item);
            }
            ViewBag.Years = list;
            var registDt = DateTime.Now;

            if (id == null)
                return PartialView("PaymentTemplates/_BuyErrorPartial");
            var context = new IPTV2Entities();
            Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
                return PartialView("PaymentTemplates/_BuyErrorPartial");
            if (!product.IsForSale)
                return PartialView("PaymentTemplates/_BuyErrorPartial");
            ViewBag.ProductId = product.ProductId;

            ViewBag.WishlistId = wid;

            User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(User.Identity.Name));
            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            if (user != null)
            {
                if (user.HasPendingGomsChangeCountryTransaction(offering))
                    return PartialView("PaymentTemplates/_BuyPendingChangeCountryErrorPartial");

                if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                    return PartialView("PaymentTemplates/_BuyErrorExceededMaximumThresholdPartial");
            }
            ViewBag.User = user;
            var ccTypes = user.Country.GetGomsCreditCardTypes();
            List<TFCTV.Helpers.CreditCard> clist = new List<TFCTV.Helpers.CreditCard>();
            foreach (var item in ccTypes)
                clist.Add(new TFCTV.Helpers.CreditCard() { value = ((int)item).ToString(), text = item.ToString().Replace('_', ' ') });
            ViewBag.CreditCardList = clist;

            if (product == null)
                return PartialView("PaymentTemplates/_BuyErrorPartial");

            ViewBag.IsSubscriptionProduct = false;
            //if (product is SubscriptionProduct)
            if (product is PackageSubscriptionProduct)
                ViewBag.IsSubscriptionProduct = true;

            ViewBag.HasCreditCardEnrolled = false;
            if (GlobalConfig.IsRecurringBillingEnabled)
            {
                var UserCreditCard = user.CreditCards.FirstOrDefault(c => c.StatusId == GlobalConfig.Visible && c.OfferingId == offering.OfferingId);
                if (UserCreditCard != null)
                {
                    ViewBag.HasCreditCardEnrolled = true;
                    ViewBag.UserCreditCard = UserCreditCard;
                }

                var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                if (checkIfEnrolled.value)
                    return PartialView("PaymentTemplates/_IsEnrolledToRecurringPartial", checkIfEnrolled.container);
            }
            return PartialView("PaymentTemplates/_BuyViaCreditCardPartial");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Paypal(int? id, string wid, int? cpid)
        {
            if (!GlobalConfig.IsPaypalPaymentModeEnabled)
                return PartialView("PaymentTemplates/_BuyErrorPartial");
            if (id == null)
                return PartialView("PaymentTemplates/_BuyErrorPartial");

            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                var registDt = DateTime.Now;
                if (user != null)
                {
                    Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
                    if (product == null)
                        return PartialView("PaymentTemplates/_BuyErrorPartial");
                    if (!product.IsForSale)
                        return PartialView("PaymentTemplates/_BuyErrorPartial");

                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                        return PartialView("PaymentTemplates/_BuyPendingChangeCountryErrorPartial");

                    if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                        return PartialView("PaymentTemplates/_BuyErrorExceededMaximumThresholdPartial");

                    string subscriptionType = ContextHelper.GetProductType(product).ToString();

                    ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                    ViewBag.WishlistId = wid;
                    ViewBag.ProductId = product.ProductId;
                    ViewBag.productName = product.Name;
                    ViewBag.ProductPrice = priceOfProduct.Amount.ToString("F");
                    ViewBag.Currency = priceOfProduct.CurrencyCode;
                    ViewBag.subscriptionType = subscriptionType;
                    ViewBag.cpId = cpid;
                    ViewBag.User = user;

                    if (!priceOfProduct.Currency.IsPayPalSupported)
                    {
                        priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);
                        ViewBag.ProductPrice = priceOfProduct.Amount.ToString("F");
                        ViewBag.Currency = priceOfProduct.CurrencyCode;
                    }

                    ViewBag.returnURL = GlobalConfig.PayPalReturnUrl; // String.Format("http://{0}{1}/Subscribe/PayPalHandler", Request.Url.Host, (Request.Url.Port != 80) ? ":" + Request.Url.Port.ToString() : "");

                    ViewBag.IsSubscriptionProduct = false;
                    if (product is SubscriptionProduct)
                        ViewBag.IsSubscriptionProduct = true;
                    if (GlobalConfig.IsRecurringBillingEnabled)
                    {
                        var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                        if (checkIfEnrolled.value)
                            return PartialView("PaymentTemplates/_IsEnrolledToRecurringPartial", checkIfEnrolled.container);
                    }
                    return PartialView("PaymentTemplates/_BuyViaPaypalPartial");
                }
            }
            return PartialView("PaymentTemplates/_BuyAuthenticationErrorPartial");
        }

        public ActionResult PayPalHandler()
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection.Add("errorCode", 1);
            collection.Add("errorMessage", errorMessage);

            string PDTpostData = "";

            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                string tx = Request["tx"];
                string st = Request["st"];
                string amt = Request["amt"];
                string cc = Request["cc"];

                //Call PDT of Paypal

                PDTpostData = PaymentHelper.PDTHandler(tx);
                if (String.IsNullOrEmpty(PDTpostData))
                    return RedirectToAction("Index", "Home");
                Custom custom = PDTHolder.Parse(PDTpostData);

                int productId = custom.productId;
                //string subscriptionType = custom.subscriptionType;

                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                if (product == null)
                    return PartialView("PaymentTemplates/_BuyErrorPartial");
                SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                string productName = product.Name;
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                if (user != null)
                {
                    if (offering != null)
                    {
                        //if (user.HasPendingGomsChangeCountryTransaction(offering))
                        //{
                        //    errorMessage = "You are not allowed to purchase as of this time.";
                        //    collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                        //    return Content(MyUtility.buildJson(collection), "application/json");
                        //}
                    }

                    //Get Recipient
                    User recipient = null;
                    if (!String.IsNullOrEmpty(custom.WishlistId))
                    {
                        GSArray gsarray = GigyaMethods.GetWishlistDetails(custom.WishlistId);

                        if (gsarray != null)
                        {
                            JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                            System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                            recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                            ViewBag.RecipientEmail = recipient.EMail;
                            ViewBag.RecipientName = String.Format("{0} {1}", recipient.FirstName, recipient.LastName);
                            collection.Add("recipient", String.Format("{0} {1}", recipient.FirstName, recipient.LastName));
                        }
                    }

                    int? cpId = null;
                    if (!String.IsNullOrEmpty(custom.cpId))
                        cpId = Convert.ToInt32(custom.cpId);

                    ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));

                    /** Begin Commented out due to doubling of record ***/
                    //errorCode = PaymentHelper.PayViaPayPal(context, userId, product.ProductId, subscriptionType, custom.TransactionId, recipient == null ? userId : recipient.UserId, cpId);
                    /** End Commented out due to doubling of record ***/
                    if (GlobalConfig.IsPDTEnabled)
                        errorCode = PaymentHelper.PayViaPayPal(context, userId, product.ProductId, subscriptionType, custom.TransactionId, recipient == null ? userId : recipient.UserId, cpId, custom.subscr_id);
                    else
                        errorCode = ErrorCodes.Success;

                    switch (errorCode)
                    {
                        case ErrorCodes.IsProcessedPayPalTransaction:
                            errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", tx);
                            //errorMessage = "This transaction has already been processed.";
                            break;
                        case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                        case ErrorCodes.Success:
                            {
                                ViewBag.WishlistId = custom.WishlistId;

                                if (!String.IsNullOrEmpty(custom.WishlistId))
                                {
                                    GigyaMethods.DeleteWishlist(custom.WishlistId); // Delete from Wishlist
                                    SendGiftShareToSocialNetwork(product, recipient);
                                    ReceiveGiftShareToSocialNetwork(product, recipient, user);
                                }
                                errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", tx); break;
                            }
                        default: errorMessage = "Unknown error."; break;
                    }
                    collection["errorCode"] = (int)errorCode;
                    collection["errorMessage"] = errorMessage;
                }
            }
            else
            {
                collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
                collection["errorMessage"] = "Your session has expired. Please login again.";
            }

            ViewBag.errorCode = collection["errorCode"];
            ViewBag.errorMessage = collection["errorMessage"];

            return PartialView("PaymentTemplates/_PayPalPaymentCallbackUrlPartial");
        }

        public ActionResult Authenticate()
        {
            return PartialView("PaymentTemplates/_BuyAuthenticationErrorPartial");
        }

        private void SendGiftShareToSocialNetwork(Product product, User recipient)
        {
            //Publish user action
            List<ActionLink> actionlinks = new List<ActionLink>();
            actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.sgift_actionlink_href) });
            List<MediaItem> mediaItems = new List<MediaItem>();
            mediaItems.Add(new MediaItem() { type = SNSTemplates.sgift_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.sgift_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.sgift_mediaitem_href, User.Identity.Name)) });
            UserAction action = new UserAction()
            {
                actorUID = User.Identity.Name,
                userMessage = String.Format(SNSTemplates.sgift_usermessage_external, product.Description, String.Format("{0} {1}", recipient.FirstName, recipient.LastName)),
                title = SNSTemplates.sgift_title,
                subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.sgift_subtitle),
                linkBack = String.Format(SNSTemplates.sgift_linkback, User.Identity.Name),
                description = SNSTemplates.sgift_description,
                actionLinks = actionlinks,
                mediaItems = mediaItems
            };

            var userId = new Guid(User.Identity.Name);
            var userData = MyUtility.GetUserPrivacySetting(userId);
            if (userData.IsExternalSharingEnabled.Contains("true"))
                GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "external");
            //Modify action to suit Internal feed needs
            mediaItems.Clear();
            mediaItems.Add(new MediaItem() { type = SNSTemplates.sgift_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.sgift_mediaitem_src_internal), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.sgift_mediaitem_href, User.Identity.Name)) });
            action.userMessage = String.Format(SNSTemplates.sgift_usermessage, product.Description, recipient.UserId, String.Format("{0} {1}", recipient.FirstName, recipient.LastName));
            action.description = String.Format(SNSTemplates.sgift_description_internal, product.Description, recipient.UserId, String.Format("{0} {1}", recipient.FirstName, recipient.LastName));
            action.mediaItems = mediaItems;
            if (userData.IsInternalSharingEnabled.Contains("true"))
                GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "internal");
        }

        private void ReceiveGiftShareToSocialNetwork(Product product, User recipient, User user)
        {
            //Publish user action
            List<ActionLink> actionlinks = new List<ActionLink>();
            actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.rgift_actionlink_href) });
            List<MediaItem> mediaItems = new List<MediaItem>();
            mediaItems.Add(new MediaItem() { type = SNSTemplates.rgift_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.rgift_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.rgift_mediaitem_href, recipient.UserId.ToString())) });
            UserAction action = new UserAction()
            {
                actorUID = recipient.UserId.ToString(),
                userMessage = String.Format(SNSTemplates.rgift_usermessage_external, product.Description, String.Format("{0} {1}", user.FirstName, user.LastName)),
                title = SNSTemplates.rgift_title,
                subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.rgift_subtitle),
                linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.rgift_linkback, recipient.UserId.ToString())),
                description = SNSTemplates.rgift_description,
                actionLinks = actionlinks,
                mediaItems = mediaItems
            };
            var userData = MyUtility.GetUserPrivacySetting(recipient.UserId);
            if (userData.IsExternalSharingEnabled.Contains("true"))
                GigyaMethods.PublishUserAction(action, recipient.UserId, "external");
            //Modify action to suit Internal feed needs
            mediaItems.Clear();
            mediaItems.Add(new MediaItem() { type = SNSTemplates.rgift_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.rgift_mediaitem_src_internal), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.rgift_mediaitem_href, recipient.UserId.ToString())) });
            action.userMessage = String.Format(SNSTemplates.rgift_usermessage, product.Description, user.UserId, String.Format("{0} {1}", user.FirstName, user.LastName));
            action.description = String.Format(SNSTemplates.rgift_description_internal, product.Description, user.UserId, String.Format("{0} {1}", user.FirstName, user.LastName));
            action.mediaItems = mediaItems;
            if (userData.IsInternalSharingEnabled.Contains("true"))
                GigyaMethods.PublishUserAction(action, recipient.UserId, "internal");
        }

        private bool IsProductPurchaseable(IPTV2Entities context, Product product, System.Guid userId)
        {
            Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
            PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

            if (user != null)
            {
                var subscribedProducts = user.GetSubscribedProductGroups(offering);

                foreach (var item in subscribedProducts)
                {
                    //checks if product to be bought and subscribed product belong to the same group
                    if (item.ProductGroupId != subscription.ProductGroupId) //does not belong to same group
                    {
                        if (item.UpgradeableFromProductGroups().Contains(subscription.ProductGroup))
                            return false;
                    }
                }
            }
            return true;
        }

        private bool IsProductGiftable(IPTV2Entities context, Product product, System.Guid userId)
        {
            Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
            PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

            if (user != null)
            {
                var subscribedProducts = user.GetSubscribedProductGroups(offering);
                if (subscribedProducts.Count == 0)
                    return true;
                foreach (var item in subscribedProducts)
                {
                    //checks if product to be bought and subscribed product belong to the same group
                    if (item.ProductGroupId != subscription.ProductGroupId) //does not belong to same group
                    {
                        if (item.UpgradeableFromProductGroups().Contains(subscription.ProductGroup))
                            return false;
                        else
                        { // added this line
                            if (item.UpgradeableToProductGroups().Contains(subscription.ProductGroup))
                                return false;
                            else
                                return true;
                        }
                    }
                    else
                        return true;
                }
            }
            return false;
        }

        private bool IsProductRestricted(IPTV2Entities context, Product product, System.Guid userId)
        {
            if (product is PackageSubscriptionProduct)
            {
                PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    if (user.Country != null)
                    {
                        if (product.IsAllowed(user.Country))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Product to be bought is TFC.tv Lite
        /// PackageId 48
        /// </summary>
        /// <param name="context"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        private bool isAllowedToBuyLite(IPTV2Entities context, Product product, System.Guid userId)
        {
            if (product is PackageSubscriptionProduct)
            {
                PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;
                foreach (var item in subscription.Packages)
                {
                    if (item.PackageId == GlobalConfig.LitePackageId) // If Product contains a Lite package, check for Premium
                    {
                        DateTime registDt = DateTime.Now;
                        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        var PremiumEntitlement = user.PackageEntitlements.FirstOrDefault(pe => pe.PackageId == GlobalConfig.PremiumPackageId && pe.EndDate > registDt);
                        return PremiumEntitlement != null ? false : true;
                    }
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// Product to be bought is TFC.tv Lite
        /// PackageId 48
        /// </summary>
        /// <param name="context"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        private bool isAllowedToBuyPremium(IPTV2Entities context, Product product, System.Guid userId)
        {
            if (product is PackageSubscriptionProduct)
            {
                PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;
                foreach (var item in subscription.Packages)
                {
                    if (item.PackageId == GlobalConfig.PremiumPackageId) // If Product contains a Lite package, check for Premium
                    {
                        DateTime registDt = DateTime.Now;
                        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        var LiteEntitlement = user.PackageEntitlements.FirstOrDefault(pe => pe.PackageId == GlobalConfig.LitePackageId && pe.EndDate > registDt);
                        return LiteEntitlement != null ? false : true;
                    }
                    return true;
                }
            }
            return true;
        }

        private bool isLiteAllowedInCountry(IPTV2Entities context, Product product, System.Guid userId)
        {
            if (product is PackageSubscriptionProduct)
            {
                PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;
                foreach (var item in subscription.Packages)
                {
                    if (item.PackageId == GlobalConfig.LitePackageId)
                    {
                        //Check users country
                        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                            if (user.Country != null)
                                if (user.Country.Code == GlobalConfig.DefaultCountry)
                                    return false;
                    }
                }
            }
            return true;
        }

        [HttpPost]
        public ActionResult _CreditCard(int? id, string wid, int? cpid, FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            DateTime registDt = DateTime.Now;
            try
            {
                if (id == null)
                    return PartialView("PaymentTemplates/_BuyErrorPartial");

                string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
                collection = MyUtility.setError(ErrorCodes.UnknownError);

                if (User.Identity.IsAuthenticated)
                {
                    var context = new IPTV2Entities();
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                    Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
                    if (product == null)
                    {
                        collection = MyUtility.setError(ErrorCodes.ProductIsNull);
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }
                    if (!product.IsForSale)
                    {
                        collection = MyUtility.setError(ErrorCodes.ProductIsNotPurchaseable);
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }
                    SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);

                    string productName = product.Name;
                    Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                    if (user != null)
                    {
                        if (offering != null)
                        {
                            if (user.HasPendingGomsChangeCountryTransaction(offering))
                            {
                                errorMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                                return Content(MyUtility.buildJson(collection), "application/json");
                            }

                            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                            {
                                errorMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                                return Content(MyUtility.buildJson(collection), "application/json");
                            }
                        }

                        //decimal amount = Convert.ToDecimal(fc["amount"]);
                        int cctype = Convert.ToInt32(fc["CreditCard"]);
                        string name = fc["cardholdername"];
                        string cardnumber = fc["cardnumber"];
                        string securitycode = fc["securitycode"];
                        string expirymonth = fc["ExpiryMonth"];
                        string expiryyear = fc["ExpiryYear"];
                        string address = fc["address"];
                        string city = fc["city"];
                        string zip = fc["zip"];

                        var recur = fc["recur"];
                        //decimal amount = Convert.ToDecimal("4.99");
                        //int cctype = 2;
                        //string name = "123";
                        //string cardnumber = "4111111111111111";
                        //string securitycode = "123";
                        //string expirymonth = "1";
                        //string expiryyear = "2014";
                        //string address = "123";
                        //string city = "123";
                        //string zip = "123";

                        CreditCardInfo info = new CreditCardInfo()
                        {
                            CardType = (IPTV2_Model.CreditCardType)cctype,
                            Name = name,
                            Number = cardnumber,
                            CardSecurityCode = securitycode,
                            ExpiryMonth = Convert.ToInt32(expirymonth),
                            ExpiryYear = Convert.ToInt32(expiryyear),
                            PostalCode = zip,
                            StreetAddress = String.Format("{0}{1}", address, String.IsNullOrEmpty(city) ? "" : (", " + city))
                        };

                        //Get Recipient
                        User recipient = null;
                        if (!String.IsNullOrEmpty(wid))
                        {
                            GSArray gsarray = GigyaMethods.GetWishlistDetails(wid);
                            if (gsarray != null)
                            {
                                JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                                System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                                recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                                ViewBag.RecipientEmail = recipient.EMail;
                                collection.Add("recipient", String.Format("{0} {1}", recipient.FirstName, recipient.LastName));
                            }

                            if (subscriptionType == SubscriptionProductType.Package)
                            {
                                //Check if user has a current subscription
                                if (!IsProductGiftable(context, product, recipient == null ? user.UserId : recipient.UserId))
                                {
                                    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                                    errorMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                    if (recipient != null)
                                    {
                                        if (recipient.UserId != user.UserId) // same user
                                            errorMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                                    }
                                    collection = MyUtility.setError(ErrorCodes.ProductIsNotPurchaseable, errorMessage);
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }
                            }
                        }

                        var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                        if (checkIfEnrolled.value)
                        {
                            errorMessage = "You are currently automatically renewing a similar subscription product through credit card.";
                            collection = MyUtility.setError(ErrorCodes.ProductIsNotPurchaseable, errorMessage);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }

                        ErrorResponse response;
                        if (!String.IsNullOrEmpty(recur))
                        {
                            if (recur.ToLowerInvariant().Contains("on"))
                                response = PaymentHelper.PayViaCreditCardWithRecurringBilling(context, userId, info, product.ProductId, subscriptionType, recipient == null ? userId : recipient.UserId, cpid);
                            else
                                response = PaymentHelper.PayViaCreditCard2(context, userId, info, product.ProductId, subscriptionType, recipient == null ? userId : recipient.UserId, cpid);
                        }
                        else
                            response = PaymentHelper.PayViaCreditCard2(context, userId, info, product.ProductId, subscriptionType, recipient == null ? userId : recipient.UserId, cpid);

                        switch (response.Code)
                        {
                            case (int)ErrorCodes.IsProductIdInvalidPpc: errorMessage = "Please use proper card for this product."; break;
                            case (int)ErrorCodes.IsReloadPpc: errorMessage = "Card is invalid. Type is for reloading wallet."; break;
                            case (int)ErrorCodes.IsInvalidCombinationPpc: errorMessage = "Invalid serial/pin combination."; break;
                            case (int)ErrorCodes.IsExpiredPpc: errorMessage = "Prepaid card is expired."; break;
                            case (int)ErrorCodes.IsInvalidPpc: errorMessage = "Serial does not exist."; break;
                            case (int)ErrorCodes.IsUsedPpc: errorMessage = "Ppc is already used."; break;
                            case (int)ErrorCodes.IsNotValidInCountryPpc: errorMessage = "Ppc not valid in your country."; break;
                            case (int)ErrorCodes.CreditCardHasExpired: errorMessage = "Your credit card has already expired."; break;
                            case (int)ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                            case (int)ErrorCodes.Success:
                                {
                                    //Insert to Recurring
                                    if (!String.IsNullOrEmpty(recur))
                                    {
                                        if (recur.ToLowerInvariant().Contains("on"))
                                        {
                                            ////Check if product is subscription product
                                            //if (product is SubscriptionProduct)
                                            //{
                                            //    //check if there are any recurring products that have the same productgroup
                                            //    SubscriptionProduct subscriptionProduct = (SubscriptionProduct)product;

                                            //    //Get user's recurring productGroups
                                            //    var recurringProductGroups = user.GetRecurringProductGroups(offering);
                                            //    if (!recurringProductGroups.Contains(subscriptionProduct.ProductGroup))
                                            //    {
                                            //        var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId);
                                            //        if (productPackage != null)
                                            //        {
                                            //            var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == productPackage.PackageId);
                                            //            if (entitlement != null)
                                            //            {
                                            //                var billing = new RecurringBilling()
                                            //                {
                                            //                    CreatedOn = registDt,
                                            //                    Product = product,
                                            //                    User = user,
                                            //                    UpdatedOn = registDt,
                                            //                    EndDate = entitlement.EndDate,
                                            //                    NextRun = entitlement.EndDate.AddDays(-1).Date, // Run day before expiry
                                            //                    StatusId = GlobalConfig.Visible,
                                            //                    Offering = offering,
                                            //                    Package = (Package)productPackage.Package
                                            //                };
                                            //                context.RecurringBillings.Add(billing);
                                            //                context.SaveChanges();
                                            //            }
                                            //        }
                                            //    }
                                            //}
                                        }
                                    }

                                    if (!String.IsNullOrEmpty(wid))
                                    {
                                        GigyaMethods.DeleteWishlist(wid); // Delete from Wishlist
                                        SendGiftShareToSocialNetwork(product, recipient);
                                        ReceiveGiftShareToSocialNetwork(product, recipient, user);
                                    }
                                    collection.Add("wid", wid);
                                    errorMessage = "Congratulations! You have now bought this product.";
                                    if (!String.IsNullOrEmpty(response.CCEnrollmentStatusMessage))
                                    {
                                        //errorMessage = String.Format("{0} {1}", errorMessage, response.CCEnrollmentStatusMessage);                                        
                                        //collection.Add("cErrorMessage", "This product will be renewed using the credit card you previously enrolled in your account.");
                                        collection.Add("cErrorMessage", "Auto-renewal was not successful. Please contact Customer Support.");
                                    }

                                    break;
                                }
                            default: errorMessage = response.Message; break;
                        }
                        collection["errorCode"] = response.Code;
                        collection["errorMessage"] = errorMessage;
                    }
                }
                else
                {
                    collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
                    collection.Add("errorMessage", "Your session has expired. Please login again.");
                }
            }
            catch (Exception e)
            {
                collection = MyUtility.setError(ErrorCodes.UnknownError, e.Message);
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        private static decimal GetEquivalentPremiumDuration(IPTV2Entities context, string CurrenyCode, PackageEntitlement entitlement)
        {
            DateTime registDt = DateTime.Now;
            if (entitlement != null)
            {
                //Get remaining hours of current LITE subscription (LD)
                var remainingTs = entitlement.EndDate.Subtract(registDt);
                var remainingDuration = remainingTs.Hours;
                remainingDuration = remainingTs.Days;

                //Get price of 1 month LITE subscription (1ML)
                var Lite1Month = context.ProductPackages.FirstOrDefault(p => p.PackageId == GlobalConfig.LitePackageId && p.Product.OfferingId == GlobalConfig.offeringId && p.Product.Duration == 1 && p.Product.DurationType == "m");
                if (Lite1Month == null)
                    return 0;

                var LiteProductPrice = Lite1Month.Product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == CurrenyCode);
                if (LiteProductPrice == null)
                    return 0;

                //Lite Daily Rate (LDR)
                var LiteDailyRate = LiteProductPrice.Amount / 30;

                //Balance in Currency (BIC) = LDR * LD
                var BalanceInCurrency = LiteDailyRate * remainingDuration;

                //Get price of 1 month PREMIUM (1MP)
                var Premium1Month = context.ProductPackages.FirstOrDefault(p => p.PackageId == GlobalConfig.PremiumPackageId && p.Product.OfferingId == GlobalConfig.offeringId && p.Product.Duration == 1 && p.Product.DurationType == "m");
                if (Premium1Month == null)
                    return 0;

                var PremiumProductPrice = Premium1Month.Product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == CurrenyCode);
                if (PremiumProductPrice == null)
                    return 0;

                //Premium Daily Rate (PDR)
                var PremiumDailyRate = PremiumProductPrice.Amount / 30;

                //Equivalent Premium Duration
                return BalanceInCurrency / PremiumDailyRate;
            }
            return 0;
        }

        private static RecurringBillingReturnValue CheckIfUserIsEnrolledToSameRecurringProductGroup(IPTV2Entities context, Offering offering, User user, Product product)
        {
            RecurringBillingReturnValue returnValue = new RecurringBillingReturnValue()
            {
                container = null,
                value = false
            };
            var profiler = MiniProfiler.Current;
            using (profiler.Step("Check Recurring Enrolment"))
            {
                try
                {

                    if (product is SubscriptionProduct)
                    {
                        // check if user is part of recurring
                        var subscriptionProduct = (SubscriptionProduct)product;
                        //Get user's recurring productGroups
                        var recurringProductGroups = user.GetRecurringProductGroups(offering);
                        if (recurringProductGroups.Contains(subscriptionProduct.ProductGroup))
                        {
                            var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId);
                            if (productPackage != null)
                            {
                                var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == productPackage.PackageId);
                                if (entitlement != null)
                                {
                                    var container = new RecurringBillingContainer()
                                    {
                                        user = user,
                                        product = product,
                                        entitlement = entitlement,
                                        package = (Package)productPackage.Package
                                    };
                                    returnValue.value = true;
                                    returnValue.container = container;
                                }
                            }
                        }
                    }
                }
                catch (Exception e) { MyUtility.LogException(e); }
            }
            return returnValue;
        }


    }
}