using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Globalization;
using IPTV2_Model;
using TFCTV.Helpers;
using Gigya.Socialize.SDK;
using Newtonsoft.Json.Linq;
using System.Web.Security;
using StackExchange.Profiling;
using TFCTV.Models;
using GOMS_TFCtv;

namespace TFCTV.Controllers
{
    public class PaymentController : Controller
    {
        //
        // GET: /Payment/

        [RequireHttps]
        public ActionResult EWallet(int? id, string wid, int? cpid)
        {
            return RedirectToAction("Details", "Subscribe");
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");
            if (id == null)
                return RedirectToAction("Index", "Home");

            DateTime registDt = DateTime.Now;
            ViewBag.HasError = false;
            var context = new IPTV2Entities();
            var UserId = new Guid(User.Identity.Name);

            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var userWallet = user.UserWallets.FirstOrDefault(u => u.Currency == user.Country.CurrencyCode && u.IsActive == true);
            if (userWallet == null)
                return RedirectToAction("Index", "Home");

            if (!GlobalConfig.IsEWalletPaymentModeEnabled)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.EWALLET_PAYMENT_IS_DISABLED;
            }

            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            if (user.HasPendingGomsChangeCountryTransaction(offering))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.PENDING_GOMS_CHANGE_COUNTRY;
            }

            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED;
            }

            var product = context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                if (product.StatusId != GlobalConfig.Visible)
                    return RedirectToAction("Index", "Home");
                if (!product.IsForSale)
                    return RedirectToAction("Index", "Home");
                if (!ContextHelper.IsProductViewableInUserCountry(product))
                    return RedirectToAction("Index", "Home");

                var UserCurrencyCode = user.Country.CurrencyCode;
                var productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == UserCurrencyCode);
                if (productPrice == null)
                    productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);
                ViewBag.ProductPrice = productPrice;
                if (productPrice.Amount > userWallet.Balance)
                {
                    ViewBag.HasError = true;
                    ViewBag.ErrorEncountered = PaymentError.INSUFFICIENT_WALLET_LOAD;
                }

                ViewBag.IsSubscriptionProduct = false;
                if (product is SubscriptionProduct)
                    ViewBag.IsSubscriptionProduct = true;
                if (GlobalConfig.IsRecurringBillingEnabled)
                {
                    var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                    if (checkIfEnrolled.value)
                    {
                        ViewBag.HasError = true;
                        ViewBag.ErrorEncountered = PaymentError.USER_ENROLLED_IN_SAME_RECURRING_GROUP_PRODUCT;
                        ViewBag.ErrorEncounteredData = checkIfEnrolled.container;
                    }
                }
            }
            ViewBag.User = user;
            ViewBag.Product = product;
            ViewBag.UserWallet = userWallet;
            ViewBag.CurrentProductId = cpid;
            ViewBag.WishlistId = wid;

            return View();
        }

        [RequireHttps]
        public ActionResult CreditCard(int? id, string wid, int? cpid)
        {
            return RedirectToAction("Details", "Subscribe");
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

            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");
            if (id == null)
                return RedirectToAction("Index", "Home");

            DateTime registDt = DateTime.Now;
            ViewBag.HasError = false;
            var context = new IPTV2Entities();
            var UserId = new Guid(User.Identity.Name);
            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            if (!GlobalConfig.IsCreditCardPaymentModeEnabled)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.CREDIT_CARD_PAYMENT_IS_DISABLED;
            }

            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            if (user.HasPendingGomsChangeCountryTransaction(offering))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.PENDING_GOMS_CHANGE_COUNTRY;
            }

            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED;
            }

            //Get Credit Card List
            var ccTypes = user.Country.GetGomsCreditCardTypes();
            if (ccTypes == null)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.CREDIT_CARD_IS_NOT_AVAILABLE_IN_YOUR_AREA;
                return View();
            }
            List<TFCTV.Helpers.CreditCard> clist = new List<TFCTV.Helpers.CreditCard>();
            foreach (var item in ccTypes)
                clist.Add(new TFCTV.Helpers.CreditCard() { value = ((int)item).ToString(), text = item.ToString().Replace('_', ' ') });
            ViewBag.CreditCardList = clist;

            var product = context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                if (!ContextHelper.IsProductViewableInUserCountry(product))
                    return RedirectToAction("Index", "Home");

                var UserCurrencyCode = user.Country.CurrencyCode;
                var productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == UserCurrencyCode);
                if (productPrice == null)
                    productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);
                ViewBag.ProductPrice = productPrice;

                ViewBag.IsSubscriptionProduct = false;
                if (product is PackageSubscriptionProduct)
                {
                    if (((PackageSubscriptionProduct)product).ProductGroup.ProductSubscriptionTypeId == null)
                        ViewBag.IsSubscriptionProduct = true;
                }

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
                    {
                        ViewBag.HasError = true;
                        ViewBag.ErrorEncountered = PaymentError.USER_ENROLLED_IN_SAME_RECURRING_GROUP_PRODUCT;
                        ViewBag.ErrorEncounteredData = checkIfEnrolled.container;
                    }
                }
            }
            ViewBag.User = user;
            ViewBag.Product = product;
            return View();
        }

        [RequireHttps]
        public ActionResult Paypal(int? id, string wid, int? cpid)
        {
            return RedirectToAction("Details", "Subscribe");
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");
            if (id == null)
                return RedirectToAction("Index", "Home");

            DateTime registDt = DateTime.Now;
            ViewBag.HasError = false;
            var context = new IPTV2Entities();
            var UserId = new Guid(User.Identity.Name);
            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            if (!GlobalConfig.IsPaypalPaymentModeEnabled)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.PAYPAL_PAYMENT_IS_DISABLED;
            }

            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            if (user.HasPendingGomsChangeCountryTransaction(offering))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.PENDING_GOMS_CHANGE_COUNTRY;
            }

            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED;
            }

            var product = context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                if (!ContextHelper.IsProductViewableInUserCountry(product))
                    return RedirectToAction("Index", "Home");

                var UserCurrencyCode = user.Country.CurrencyCode;
                var productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == UserCurrencyCode);
                if (productPrice == null)
                    productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);
                if (!productPrice.Currency.IsPayPalSupported)
                    productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);
                ViewBag.ProductPrice = productPrice;

                ViewBag.IsSubscriptionProduct = false;
                ViewBag.SubscriptionDuration = 0;
                ViewBag.SubscriptionDurationType = String.Empty;
                if (product is SubscriptionProduct)
                {
                    var subscriptionProduct = (SubscriptionProduct)product;
                    ViewBag.SubscriptionDuration = subscriptionProduct.Duration;
                    ViewBag.SubscriptionDurationType = subscriptionProduct.DurationType.ToUpper();
                    if (product is PackageSubscriptionProduct)
                        if (((PackageSubscriptionProduct)product).ProductGroup.ProductSubscriptionTypeId == null)
                            ViewBag.IsSubscriptionProduct = true;
                }

                if (GlobalConfig.IsRecurringBillingEnabled)
                {
                    var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                    if (checkIfEnrolled.value)
                    {
                        ViewBag.HasError = true;
                        ViewBag.ErrorEncountered = PaymentError.USER_ENROLLED_IN_SAME_RECURRING_GROUP_PRODUCT;
                        ViewBag.ErrorEncounteredData = checkIfEnrolled.container;
                    }
                }
            }
            else
                return RedirectToAction("Index", "Home");
            ViewBag.User = user;
            ViewBag.Product = product;
            ViewBag.WishlistId = wid;
            ViewBag.SubscriptionType = ContextHelper.GetProductType(product).ToString();
            ViewBag.CurrentProductId = cpid;
            ViewBag.User = user;
            return View();
        }

        [RequireHttps]
        public ActionResult PrepaidCard(int? id, string wid, int? cpid)
        {
            return RedirectToAction("Details", "Subscribe");
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");
            if (id == null)
                return RedirectToAction("Index", "Home");

            DateTime registDt = DateTime.Now;
            ViewBag.HasError = false;
            var context = new IPTV2Entities();
            var UserId = new Guid(User.Identity.Name);
            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            if (!GlobalConfig.IsPpcPaymentModeEnabled)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.PREPAID_CARD_PAYMENT_IS_DISABLED;
            }

            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            if (user.HasPendingGomsChangeCountryTransaction(offering))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.PENDING_GOMS_CHANGE_COUNTRY;
            }

            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = PaymentError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED;
            }

            var product = context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                if (!ContextHelper.IsProductViewableInUserCountry(product))
                    return RedirectToAction("Index", "Home");

                var UserCurrencyCode = user.Country.CurrencyCode;
                var productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == UserCurrencyCode);
                if (productPrice == null)
                    productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);
                ViewBag.ProductPrice = productPrice;

                ViewBag.IsSubscriptionProduct = false;
                if (product is SubscriptionProduct)
                    ViewBag.IsSubscriptionProduct = true;
                if (GlobalConfig.IsRecurringBillingEnabled)
                {
                    var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                    if (checkIfEnrolled.value)
                    {
                        ViewBag.HasError = true;
                        ViewBag.ErrorEncountered = PaymentError.USER_ENROLLED_IN_SAME_RECURRING_GROUP_PRODUCT;
                        ViewBag.ErrorEncounteredData = checkIfEnrolled.container;
                    }
                }
            }
            ViewBag.User = user;
            ViewBag.Product = product;
            ViewBag.CurrentProductId = cpid;
            ViewBag.WishlistId = wid;
            return View();
        }

        [RequireHttp]
        public ActionResult Confirmation(string id)
        {
            return RedirectToAction("Details", "Subscribe");
            if (!Request.IsLocal)
                if (String.Compare(id, "callback", true) != 0)
                    if (TempData["PaymentMode"] == null || TempData["StatusMessage"] == null)
                        return RedirectToAction("Index", "Home");

            ViewBag.PaymentMode = TempData["PaymentMode"] != null ? (string)TempData["PaymentMode"] : "";
            ViewBag.StatusMessage = TempData["StatusMessage"] != null ? (string)TempData["StatusMessage"] : "";

            if (String.Compare(id, "callback", true) == 0)
            {
                ViewBag.PaymentMode = "PayPal";
                return View("PaypalIPN");
            }

            try
            {
                if (MyUtility.isUserLoggedIn())
                {
                    var context = new IPTV2Entities();
                    var UserId = new Guid(User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                    if (user != null)
                    {
                        Transaction transaction = null;
                        if (String.Compare(id, "callback", true) == 0)
                            transaction = user.Transactions.LastOrDefault(t => t is PaypalPaymentTransaction);
                        else
                            transaction = user.Transactions.LastOrDefault(t => t is PaymentTransaction);

                        if (transaction != null)
                        {
                            var paymentTransaction = (PaymentTransaction)transaction;
                            if (paymentTransaction.Purchase != null)
                                if (paymentTransaction.Purchase.PurchaseItems.Count() > 0)
                                {
                                    var SubscriptionProd = paymentTransaction.Purchase.PurchaseItems.First().SubscriptionProduct;
                                    ViewBag.Product = SubscriptionProd;

                                    //Get ProductPackage
                                    if (SubscriptionProd is PackageSubscriptionProduct)
                                    {
                                        var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == SubscriptionProd.ProductId);
                                        if (productPackage != null)
                                        {
                                            var PkgEntitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == productPackage.PackageId);
                                            if (PkgEntitlement != null)
                                            {
                                                ViewBag.PackageName = productPackage.Package.PackageName;
                                                ViewBag.EntitlementEndDate = PkgEntitlement.EndDate;
                                            }
                                        }
                                    }
                                    else if (SubscriptionProd is ShowSubscriptionProduct)
                                    {
                                        var productShows = context.ProductShows.FirstOrDefault(p => p.ProductId == SubscriptionProd.ProductId);
                                        if (productShows != null)
                                        {
                                            var ShwEntitlement = user.ShowEntitlements.FirstOrDefault(p => p.CategoryId == productShows.CategoryId);
                                            if (ShwEntitlement != null)
                                            {
                                                ViewBag.PackageName = String.Format("{0} subscription", productShows.Show.Description);
                                                ViewBag.EntitlementEndDate = ShwEntitlement.EndDate;
                                            }
                                        }
                                    }
                                }
                            ViewBag.Transaction = paymentTransaction;
                            return View();
                        }
                        else
                            return View("PaypalIPN");
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return View("PaypalIPN");
        }

        public ActionResult SummaryOfOrder(int id, User user, Product product, ProductPrice productPrice)
        {
            try
            {
                var registDt = DateTime.Now;
                var ExpirationDate = registDt;
                //Get entitlement end date for user
                SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                switch (subscriptionType)
                {
                    case SubscriptionProductType.Package:
                        PackageSubscriptionProduct PackageSubscription = (PackageSubscriptionProduct)product;
                        var package = PackageSubscription.Packages.FirstOrDefault();
                        if (package != null)
                        {
                            ViewBag.ListOfDescription = ContextHelper.GetPackageFeatures(user.CountryCode, package);

                            PackageEntitlement UserPackageEntitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);
                            if (UserPackageEntitlement != null)
                                ExpirationDate = MyUtility.getEntitlementEndDate(PackageSubscription.Duration, PackageSubscription.DurationType, ((UserPackageEntitlement.EndDate > registDt) ? UserPackageEntitlement.EndDate : registDt));
                            else
                                ExpirationDate = MyUtility.getEntitlementEndDate(PackageSubscription.Duration, PackageSubscription.DurationType, registDt);
                        }
                        break;
                    case SubscriptionProductType.Show:
                        ShowSubscriptionProduct ShowSubscription = (ShowSubscriptionProduct)product;
                        var show = ShowSubscription.Categories.FirstOrDefault();
                        if (show != null)
                        {
                            ViewBag.ShowDescription = show.Show.Blurb;
                            ShowEntitlement UserShowEntitlement = user.ShowEntitlements.FirstOrDefault(s => s.CategoryId == show.CategoryId);
                            if (UserShowEntitlement != null)
                                ExpirationDate = MyUtility.getEntitlementEndDate(ShowSubscription.Duration, ShowSubscription.DurationType, ((UserShowEntitlement.EndDate > registDt) ? UserShowEntitlement.EndDate : registDt));
                            else
                                ExpirationDate = MyUtility.getEntitlementEndDate(ShowSubscription.Duration, ShowSubscription.DurationType, registDt);
                        }
                        break;
                    case SubscriptionProductType.Episode:
                        EpisodeSubscriptionProduct EpisodeSubscription = (EpisodeSubscriptionProduct)product;
                        var episode = EpisodeSubscription.Episodes.FirstOrDefault();
                        if (episode != null)
                        {
                            EpisodeEntitlement UserEpisodeEntitlement = user.EpisodeEntitlements.FirstOrDefault(e => e.EpisodeId == episode.EpisodeId);
                            if (UserEpisodeEntitlement != null)
                                ExpirationDate = MyUtility.getEntitlementEndDate(EpisodeSubscription.Duration, EpisodeSubscription.DurationType, ((UserEpisodeEntitlement.EndDate > registDt) ? UserEpisodeEntitlement.EndDate : registDt));
                            else
                                ExpirationDate = MyUtility.getEntitlementEndDate(EpisodeSubscription.Duration, EpisodeSubscription.DurationType, registDt);
                        }
                        break;
                }
                ViewBag.ExpirationDate = ExpirationDate;
                ViewBag.ProductPrice = productPrice;
                return PartialView(product);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return null;
        }

        public ActionResult Reminder()
        {
            return PartialView();
        }

        public ActionResult PaymentSelection(string pselection, int id)
        {
            bool IsPayPalSupported = false;
            string UserCountryCode = GlobalConfig.DefaultCountry;
            try
            {
                if (MyUtility.isUserLoggedIn())
                {
                    var context = new IPTV2Entities();
                    var UserId = new Guid(User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                    if (user != null)
                    {
                        IsPayPalSupported = user.Country.Currency.IsPayPalSupported;
                        UserCountryCode = user.CountryCode;
                    }
                    //Get CreditCard List
                    if (GlobalConfig.IsMECreditCardEnabled)
                    {
                        try
                        {
                            if (user.Country.GomsSubsidiaryId != GlobalConfig.MiddleEastGomsSubsidiaryId)
                                ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                            else
                            {
                                //get list of middle east countries allowed for credit card payment
                                var listOfMiddleEastAllowedCountries = GlobalConfig.MECountriesAllowedForCreditCard.Split(',');
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
            }
            catch (Exception e) { MyUtility.LogException(e); }
            ViewBag.UserCountryCode = UserCountryCode;
            ViewBag.IsPayPalSupported = IsPayPalSupported;
            ViewBag.Selection = pselection;
            ViewBag.Id = id;
            return PartialView();
        }

        public ActionResult ErrorEncountered(PaymentError error)
        {
            var context = new IPTV2Entities();
            var UserId = new Guid(User.Identity.Name);
            var userWallet = context.UserWallets.FirstOrDefault(u => u.UserId == UserId);

            string message = String.Empty;
            switch (error)
            {
                case PaymentError.CREDIT_CARD_PAYMENT_IS_DISABLED:
                    message = "Credit card payment is currenty disabled."; break;
                case PaymentError.EWALLET_PAYMENT_IS_DISABLED:
                    message = "E-Wallet payment is currently disabled."; break;
                case PaymentError.INSUFFICIENT_WALLET_LOAD:
                    if (GlobalConfig.IsMopayReloadModeEnabled && GlobalConfig.MopayCountryWhitelist.Contains(userWallet.User.CountryCode))
                    {
                        message = String.Format("You don't have enough credits to purchase this product. Your current balance is {0}. Reload <a href=\"/Load/Mopay\">here</a>.", userWallet.Balance.ToString("0.00"));
                    }
                    else message = String.Format("You don't have enough credits to purchase this product. Your current balance is {0}. Reload <a href=\"/Load/CreditCard\">here</a>.", userWallet.Balance.ToString("0.00")); break;
                case PaymentError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED:
                    message = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day.", GlobalConfig.paymentTransactionMaximumThreshold); break;
                case PaymentError.PAYPAL_PAYMENT_IS_DISABLED:
                    message = "Paypal payment is currently disabled."; break;
                case PaymentError.PENDING_GOMS_CHANGE_COUNTRY:
                    message = "We are still processing your recent change in location. Please try again later."; break;
                case PaymentError.PREPAID_CARD_PAYMENT_IS_DISABLED:
                    message = "Prepaid card payment is currently disabled."; break;
                case PaymentError.USER_ENROLLED_IN_SAME_RECURRING_GROUP_PRODUCT:
                    message = "You are currently automatically renewing a similar subscription product through Paypal/Credit card."; break;
                case PaymentError.CREDIT_CARD_IS_NOT_AVAILABLE_IN_YOUR_AREA:
                    message = "Credit card payment is not available in your country."; break;
                default:
                    message = "The system encountered an unspecified error. Please contact Customer Support."; break;
            }
            var ReturnCode = new TransactionReturnType() { StatusMessage = message };
            return PartialView(ReturnCode);
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

        private RecurringBillingReturnValue CheckIfUserIsEnrolledToSameRecurringProductGroup(IPTV2Entities context, Offering offering, User user, Product product)
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

        private void ProcessWishlist(string wid, Product product, User recipient, User user)
        {
            if (!String.IsNullOrEmpty(wid))
            {
                GigyaMethods.DeleteWishlist(wid); // Delete from Wishlist
                SendGiftShareToSocialNetwork(product, recipient);
                ReceiveGiftShareToSocialNetwork(product, recipient, user);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _EWallet(FormCollection fc)
        {
            Response.ContentType = "application/json";
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };

            try
            {
                if (String.IsNullOrEmpty(fc["id"]))
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);

                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Your request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (!GlobalConfig.IsEWalletPaymentModeEnabled)
                {
                    ReturnCode.StatusCode = (int)PaymentError.EWALLET_PAYMENT_IS_DISABLED;
                    ReturnCode.StatusMessage = "E-Wallet payment is currenty disabled.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (User.Identity.IsAuthenticated)
                {
                    int? id = Convert.ToInt32(fc["id"]); //Get ID

                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
                    if (product == null)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNull;
                        ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNull);
                        return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                    }
                    if (product.StatusId != GlobalConfig.Visible)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                        ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                        return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                    }
                    if (!product.IsForSale)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                        ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                        return Json(ReturnCode, JsonRequestBehavior.AllowGet);
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
                                ReturnCode.StatusCode = (int)ErrorCodes.HasPendingChangeCountryTransaction;
                                ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }

                            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.MaximumTransactionsExceeded;
                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (GlobalConfig.IsRecurringBillingEnabled)
                        {
                            var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                            if (checkIfEnrolled.value)
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.IsCurrentlyEnrolledInRecurringBilling;
                                ReturnCode.StatusMessage = "You are currently automatically renewing a similar subscription product through credit card.";
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                        }

                        /**************** CHECK FOR WISHLISTING, PRODUCT RESTRICTION **********************/
                        User recipient = null;
                        //Get Recipient
                        string wid = fc["wid"];
                        if (!String.IsNullOrEmpty(wid))
                        {
                            GSArray gsarray = GigyaMethods.GetWishlistDetails(wid);
                            if (gsarray != null)
                            {
                                JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                                System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                                recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                                //ViewBag.RecipientEmail = recipient.EMail;
                                if (recipient != null)
                                    ReturnCode.Recipient = String.Format("{0} {1}", recipient.FirstName, recipient.LastName);
                            }

                            if (subscriptionType == SubscriptionProductType.Package)
                            {
                                //Check if user has a current subscription
                                if (!IsProductGiftable(context, product, recipient == null ? user.UserId : recipient.UserId))
                                {
                                    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                                    ReturnCode.StatusMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                    if (recipient != null)
                                    {
                                        if (recipient.UserId != user.UserId) // same user
                                            ReturnCode.StatusMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                                    }
                                    ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }

                        if (subscriptionType == SubscriptionProductType.Package)
                        {
                            if (!IsProductRestricted(context, product, recipient == null ? user.UserId : recipient.UserId))
                            {
                                //string productName = ContextHelper.GetProductName(context, product);
                                ReturnCode.StatusMessage = String.Format("Sorry! The ({0}) subscription is not available in your country.", productName);
                                if (recipient != null)
                                {
                                    if (recipient.UserId != user.UserId) // same user
                                        ReturnCode.StatusMessage = String.Format("Sorry! The ({0}) subscription is not available in the country where {1} {2} is. You are not allowed to send this gift.", productName, recipient.FirstName, recipient.LastName);
                                }
                                ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotAllowedInCountry;
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                            if (String.IsNullOrEmpty(wid))
                            {
                                if (!IsProductPurchaseable(context, product, recipient == null ? user.UserId : recipient.UserId))
                                {
                                    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                                    ReturnCode.StatusMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                    if (recipient != null)
                                    {
                                        if (recipient.UserId != user.UserId) // same user
                                            ReturnCode.StatusMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                                    }
                                    ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        /**************** END OF CHECK FOR WISHLISTING, PRODUCT RESTRICTION ***************/

                        int? cpid = null;
                        if (!String.IsNullOrEmpty(fc["cpid"]))
                            Convert.ToInt32(fc["cpid"]);

                        ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                        UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                        ErrorCodes StatusCode = ErrorCodes.UnknownError;
                        if (wallet.Balance >= priceOfProduct.Amount)
                        {
                            ErrorResponse response = PaymentHelper.PayViaWallet(context, userId, product.ProductId, subscriptionType, recipient == null ? userId : recipient.UserId, cpid);
                            StatusCode = (ErrorCodes)response.Code;
                            string CountryCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                            Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CountryCode);
                            ReturnCode.StatusCode = (int)StatusCode;
                            if (StatusCode == ErrorCodes.Success)
                            {
                                ProcessWishlist(wid, product, recipient, user);

                                string balance = String.Format("Your new wallet balance is {0} {1}.", currency.Code, wallet.Balance.ToString("F"));
                                ReturnCode.StatusMessage = balance;

                                //To be used in the Thank You page
                                //TempData["Product"] = product;
                                TempData["PaymentMode"] = "WALLET";
                                TempData["StatusMessage"] = balance;
                                //TempData["Transaction"] = user.Transactions.Last();

                                ReturnCode.WId = wid;
                                ReturnCode.HtmlUri = "/Payment/Confirmation";
                                if (!String.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
                                    ReturnCode.HtmlUri = String.Format("{0}{1}", ReturnCode.HtmlUri, String.Format("?ReturnUrl={0}", Request.QueryString["ReturnUrl"]));
                            }
                            else
                            {
                                ReturnCode.StatusCode = (int)StatusCode;
                                ReturnCode.StatusMessage = MyUtility.getErrorMessage(StatusCode);
                            }
                        }
                        else
                        {
                            ReturnCode.StatusCode = (int)ErrorCodes.InsufficientWalletLoad;
                            ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.InsufficientWalletLoad);
                        }
                    }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.NotAuthenticated;
                    ReturnCode.StatusMessage = "Your session has expired. Please login again.";
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                ReturnCode.StatusMessage = e.Message;
            }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _PrepaidCard(FormCollection fc)
        {
            Response.ContentType = "application/json";
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };

            try
            {
                if (String.IsNullOrEmpty(fc["id"]))
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);

                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Your request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (!GlobalConfig.IsPpcPaymentModeEnabled)
                {
                    ReturnCode.StatusCode = (int)PaymentError.PREPAID_CARD_PAYMENT_IS_DISABLED;
                    ReturnCode.StatusMessage = "Prepaid card payment is currenty disabled.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (User.Identity.IsAuthenticated)
                {
                    int? id = Convert.ToInt32(fc["id"]); //Get ID

                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                    Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
                    if (product == null)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNull;
                        ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNull);
                        return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                    }
                    if (product.StatusId != GlobalConfig.Visible)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                        ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                        return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                    }
                    if (!product.IsForSale)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                        ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                        return Json(ReturnCode, JsonRequestBehavior.AllowGet);
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
                                ReturnCode.StatusCode = (int)ErrorCodes.HasPendingChangeCountryTransaction;
                                ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }

                            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.MaximumTransactionsExceeded;
                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (GlobalConfig.IsRecurringBillingEnabled)
                        {
                            var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                            if (checkIfEnrolled.value)
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.IsCurrentlyEnrolledInRecurringBilling;
                                ReturnCode.StatusMessage = "You are currently automatically renewing a similar subscription product through credit card.";
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                        }

                        Ppc.ErrorCode StatusCode = Ppc.ErrorCode.UnknownError;
                        string serial = fc["serialnumber"];
                        string pin = fc["pin"];
                        serial = serial.Replace(" ", "");
                        pin = pin.Replace(" ", "");

                        /**************** CHECK FOR WISHLISTING, PRODUCT RESTRICTION **********************/
                        User recipient = null;
                        //Get Recipient
                        string wid = fc["wid"];
                        if (!String.IsNullOrEmpty(wid))
                        {
                            GSArray gsarray = GigyaMethods.GetWishlistDetails(wid);
                            if (gsarray != null)
                            {
                                JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                                System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                                recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                                //ViewBag.RecipientEmail = recipient.EMail;
                                if (recipient != null)
                                    ReturnCode.Recipient = String.Format("{0} {1}", recipient.FirstName, recipient.LastName);
                            }

                            if (subscriptionType == SubscriptionProductType.Package)
                            {
                                //Check if user has a current subscription
                                if (!IsProductGiftable(context, product, recipient == null ? user.UserId : recipient.UserId))
                                {
                                    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                                    ReturnCode.StatusMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                    if (recipient != null)
                                    {
                                        if (recipient.UserId != user.UserId) // same user
                                            ReturnCode.StatusMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                                    }
                                    ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }

                        if (subscriptionType == SubscriptionProductType.Package)
                        {
                            if (!IsProductRestricted(context, product, recipient == null ? user.UserId : recipient.UserId))
                            {
                                //string productName = ContextHelper.GetProductName(context, product);
                                ReturnCode.StatusMessage = String.Format("Sorry! The ({0}) subscription is not available in your country.", productName);
                                if (recipient != null)
                                {
                                    if (recipient.UserId != user.UserId) // same user
                                        ReturnCode.StatusMessage = String.Format("Sorry! The ({0}) subscription is not available in the country where {1} {2} is. You are not allowed to send this gift.", productName, recipient.FirstName, recipient.LastName);
                                }
                                ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotAllowedInCountry;
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                            if (String.IsNullOrEmpty(wid))
                            {
                                if (!IsProductPurchaseable(context, product, recipient == null ? user.UserId : recipient.UserId))
                                {
                                    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                                    ReturnCode.StatusMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                    if (recipient != null)
                                    {
                                        if (recipient.UserId != user.UserId) // same user
                                            ReturnCode.StatusMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                                    }
                                    ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        /**************** END OF CHECK FOR WISHLISTING, PRODUCT RESTRICTION ***************/

                        int? cpid = null;
                        if (!String.IsNullOrEmpty(fc["cpid"]))
                            Convert.ToInt32(fc["cpid"]);

                        string ErrorMessage = String.Empty;
                        if (!String.IsNullOrEmpty(pin))
                            if (!Request.IsLocal)
                                pin = MyUtility.GetSHA1(pin);
                        ErrorResponse response = PaymentHelper.PayViaPrepaidCard(context, userId, product.ProductId, subscriptionType, serial, pin, recipient == null ? userId : recipient.UserId, cpid);
                        StatusCode = (Ppc.ErrorCode)response.Code;

                        switch (StatusCode)
                        {
                            case Ppc.ErrorCode.Success:
                                ProcessWishlist(wid, product, recipient, user);

                                //To be used in the Thank You page
                                //TempData["Product"] = product;
                                TempData["PaymentMode"] = "PREPAIDCARD";
                                TempData["StatusMessage"] = String.Format("You have consumed {0}.", serial);

                                ReturnCode.WId = wid;
                                ReturnCode.HtmlUri = "/Payment/Confirmation";
                                if (!String.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
                                    ReturnCode.HtmlUri = String.Format("{0}{1}", ReturnCode.HtmlUri, String.Format("?ReturnUrl={0}", Request.QueryString["ReturnUrl"]));

                                ErrorMessage = String.Format("Well done! You have successfully subscribed to {0}", product.Description); break;
                            default: ErrorMessage = MyUtility.GetPpcErrorMessages(StatusCode); break;
                        }
                        ReturnCode.StatusCode = (int)StatusCode;
                        ReturnCode.StatusMessage = ErrorMessage;
                    }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.NotAuthenticated;
                    ReturnCode.StatusMessage = "Your session has expired. Please login again.";
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                ReturnCode.StatusMessage = e.Message;
            }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _CreditCard(FormCollection fc)
        {
            Response.ContentType = "application/json";
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };

            try
            {
                if (String.IsNullOrEmpty(fc["id"]))
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);

                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Your request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (!GlobalConfig.IsCreditCardPaymentModeEnabled)
                {
                    ReturnCode.StatusCode = (int)PaymentError.CREDIT_CARD_PAYMENT_IS_DISABLED;
                    ReturnCode.StatusMessage = "Credit card payment is currenty disabled.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (User.Identity.IsAuthenticated)
                {
                    int? id = Convert.ToInt32(fc["id"]); //Get ID

                    DateTime registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                    Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
                    if (product == null)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNull;
                        ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNull);
                        return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                    }
                    if (product.StatusId != GlobalConfig.Visible)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                        ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                        return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                    }
                    if (!product.IsForSale)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                        ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                        return Json(ReturnCode, JsonRequestBehavior.AllowGet);
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
                                ReturnCode.StatusCode = (int)ErrorCodes.HasPendingChangeCountryTransaction;
                                ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }

                            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.MaximumTransactionsExceeded;
                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (GlobalConfig.IsRecurringBillingEnabled)
                        {
                            var checkIfEnrolled = CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                            if (checkIfEnrolled.value)
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.IsCurrentlyEnrolledInRecurringBilling;
                                ReturnCode.StatusMessage = "You are currently automatically renewing a similar subscription product through credit card.";
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                        }

                        /**************** CHECK FOR WISHLISTING, PRODUCT RESTRICTION **********************/
                        User recipient = null;
                        //Get Recipient
                        string wid = fc["wid"];
                        if (!String.IsNullOrEmpty(wid))
                        {
                            GSArray gsarray = GigyaMethods.GetWishlistDetails(wid);
                            if (gsarray != null)
                            {
                                JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                                System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                                recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                                //ViewBag.RecipientEmail = recipient.EMail;
                                if (recipient != null)
                                    ReturnCode.Recipient = String.Format("{0} {1}", recipient.FirstName, recipient.LastName);
                            }

                            if (subscriptionType == SubscriptionProductType.Package)
                            {
                                //Check if user has a current subscription
                                if (!IsProductGiftable(context, product, recipient == null ? user.UserId : recipient.UserId))
                                {
                                    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                                    ReturnCode.StatusMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                    if (recipient != null)
                                    {
                                        if (recipient.UserId != user.UserId) // same user
                                            ReturnCode.StatusMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                                    }
                                    ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }

                        if (subscriptionType == SubscriptionProductType.Package)
                        {
                            if (!IsProductRestricted(context, product, recipient == null ? user.UserId : recipient.UserId))
                            {
                                //string productName = ContextHelper.GetProductName(context, product);
                                ReturnCode.StatusMessage = String.Format("Sorry! The ({0}) subscription is not available in your country.", productName);
                                if (recipient != null)
                                {
                                    if (recipient.UserId != user.UserId) // same user
                                        ReturnCode.StatusMessage = String.Format("Sorry! The ({0}) subscription is not available in the country where {1} {2} is. You are not allowed to send this gift.", productName, recipient.FirstName, recipient.LastName);
                                }
                                ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotAllowedInCountry;
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                            if (String.IsNullOrEmpty(wid))
                            {
                                if (!IsProductPurchaseable(context, product, recipient == null ? user.UserId : recipient.UserId))
                                {
                                    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, recipient == null ? user.UserId : recipient.UserId, offering);
                                    ReturnCode.StatusMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                    if (recipient != null)
                                    {
                                        if (recipient.UserId != user.UserId) // same user
                                            ReturnCode.StatusMessage = String.Format("Sorry! {0} {1} is already subscribed to {2}. You can no longer gift {3}.", recipient.FirstName, recipient.LastName, CurrentProductName, productName);
                                    }
                                    ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNotPurchaseable;
                                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        /**************** END OF CHECK FOR WISHLISTING, PRODUCT RESTRICTION ***************/

                        int? cpid = null;
                        if (!String.IsNullOrEmpty(fc["cpid"]))
                            Convert.ToInt32(fc["cpid"]);

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

                        string ErrorMessage = String.Empty;
                        switch (response.Code)
                        {
                            case (int)ErrorCodes.IsProductIdInvalidPpc: ErrorMessage = "Please use proper card for this product."; break;
                            case (int)ErrorCodes.IsReloadPpc: ErrorMessage = "Card is invalid. Type is for reloading wallet."; break;
                            case (int)ErrorCodes.IsInvalidCombinationPpc: ErrorMessage = "Invalid serial/pin combination."; break;
                            case (int)ErrorCodes.IsExpiredPpc: ErrorMessage = "Prepaid card is expired."; break;
                            case (int)ErrorCodes.IsInvalidPpc: ErrorMessage = "Serial does not exist."; break;
                            case (int)ErrorCodes.IsUsedPpc: ErrorMessage = "Ppc is already used."; break;
                            case (int)ErrorCodes.IsNotValidInCountryPpc: ErrorMessage = "Ppc not valid in your country."; break;
                            case (int)ErrorCodes.CreditCardHasExpired: ErrorMessage = "Your credit card has already expired."; break;
                            case (int)ErrorCodes.IsNotValidAmountPpc: ErrorMessage = "Ppc credits not enough to buy this product."; break;
                            case (int)ErrorCodes.Success:
                                {
                                    if (!String.IsNullOrEmpty(wid))
                                    {
                                        GigyaMethods.DeleteWishlist(wid); // Delete from Wishlist
                                        SendGiftShareToSocialNetwork(product, recipient);
                                        ReceiveGiftShareToSocialNetwork(product, recipient, user);
                                    }

                                    ErrorMessage = "Credit card payment successful.";

                                    //To be used in the Thank You page
                                    //TempData["Product"] = product;
                                    TempData["PaymentMode"] = "CREDITCARD";
                                    TempData["StatusMessage"] = ErrorMessage;
                                    //TempData["Transaction"] = user.Transactions.Last();

                                    ReturnCode.WId = wid;
                                    ReturnCode.HtmlUri = "/Payment/Confirmation";
                                    if (!String.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
                                        ReturnCode.HtmlUri = String.Format("{0}{1}", ReturnCode.HtmlUri, String.Format("?ReturnUrl={0}", Request.QueryString["ReturnUrl"]));

                                    if (!String.IsNullOrEmpty(response.CCEnrollmentStatusMessage))
                                    {
                                        ReturnCode.CCStatusMessage = "Auto-renewal was not successful. Please contact Customer Support.";
                                        TempData["StatusMessage"] = String.Format("{0} {1}", ErrorMessage, ReturnCode.CCStatusMessage);
                                    }
                                    break;
                                }
                            default: ErrorMessage = response.Message; break;
                        }
                        ReturnCode.StatusCode = response.Code;
                        ReturnCode.StatusMessage = ErrorMessage;
                    }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.NotAuthenticated;
                    ReturnCode.StatusMessage = "Your session has expired. Please login again.";
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                ReturnCode.StatusMessage = e.Message;
            }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PayPalHandler()
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };

            try
            {
                if (!GlobalConfig.IsPaypalPaymentModeEnabled)
                {
                    ReturnCode.StatusCode = (int)PaymentError.PAYPAL_PAYMENT_IS_DISABLED;
                    ReturnCode.StatusMessage = "Paypal payment is currently disabled.";
                    throw new Exception(ReturnCode.StatusMessage);
                }

                if (User.Identity.IsAuthenticated)
                {
                    string PDTpostData = "";

                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                    Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                    if (user != null)
                    {
                        if (offering != null)
                        {
                            if (user.HasPendingGomsChangeCountryTransaction(offering))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.HasPendingChangeCountryTransaction;
                                ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                throw new Exception(ReturnCode.StatusMessage);
                            }

                            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.MaximumTransactionsExceeded;
                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                throw new Exception(ReturnCode.StatusMessage);
                            }
                        }

                        string tx = Request["tx"];
                        string st = Request["st"];
                        string amt = Request["amt"];
                        string cc = Request["cc"];
                        ErrorCodes StatusCode = ErrorCodes.UnknownError;
                        string ErrorMessage = String.Empty;
                        PDTpostData = ReloadHelper.PDTHandler(tx);
                        if (String.IsNullOrEmpty(PDTpostData))
                            return RedirectToAction("Index", "Home");

                        Custom custom = PDTHolder.Parse(PDTpostData);
                        Product product = null;
                        User recipient = null;
                        if (GlobalConfig.IsPDTEnabled)
                        {
                            int productId = custom.productId;
                            var UserCurrencyCode = user.Country.CurrencyCode;
                            product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                            if (product == null)
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.ProductIsNull;
                                ReturnCode.StatusMessage = "Product does not exist.";
                                throw new Exception(ReturnCode.StatusMessage);
                            }

                            SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                            if (!String.IsNullOrEmpty(custom.WishlistId))
                            {
                                GSArray gsarray = GigyaMethods.GetWishlistDetails(custom.WishlistId);
                                if (gsarray != null)
                                {
                                    JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                                    System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                                    recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                                    if (recipient != null)
                                        ReturnCode.Recipient = String.Format("{0} {1}", recipient.FirstName, recipient.LastName);
                                }
                            }

                            int? cpId = null;
                            if (!String.IsNullOrEmpty(custom.cpId))
                                cpId = Convert.ToInt32(custom.cpId);

                            var productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == UserCurrencyCode);
                            if (productPrice == null)
                                productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);
                            if (!productPrice.Currency.IsPayPalSupported)
                                productPrice = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);
                            StatusCode = PaymentHelper.PayViaPayPal(context, userId, product.ProductId, subscriptionType, custom.TransactionId, recipient == null ? userId : recipient.UserId, cpId);
                        }
                        else
                            StatusCode = ErrorCodes.Success;

                        switch (StatusCode)
                        {
                            case ErrorCodes.IsProcessedPayPalTransaction:
                                ErrorMessage = String.Format("Your Transaction ID is {0}", tx);
                                break;
                            case ErrorCodes.Success:
                                ProcessWishlist(custom.WishlistId, product, recipient, user);
                                ErrorMessage = String.Format("Your Transaction ID is {0}", tx);
                                TempData["PaymentMode"] = "PAYPAL";
                                TempData["StatusMessage"] = ErrorMessage;
                                break;
                            default: ErrorMessage = "An unspecified error occurred. Please try again later."; break;
                        }
                        ReturnCode.StatusCode = (int)StatusCode;
                        ReturnCode.StatusMessage = ErrorMessage;
                        if (StatusCode == ErrorCodes.Success)
                            return View();
                        else
                            throw new Exception(ReturnCode.StatusMessage);
                    }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.NotAuthenticated;
                    ReturnCode.StatusMessage = "Your session has expired. Please login again.";
                    throw new Exception(ReturnCode.StatusMessage);
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                ReturnCode.StatusMessage = e.Message;
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Select(int? id, string ReturnUrl)
        {
            try
            {
                if (id == null)
                    return RedirectToAction("Index", "Home");

                var context = new IPTV2Entities();
                var UserId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                if (user == null)
                    return RedirectToAction("Index", "Home");

                if (GlobalConfig.IsCreditCardPaymentModeEnabled)
                {
                    CreditCardType[] ccTypes = null;
                    if (GlobalConfig.IsMECreditCardEnabled)
                    {
                        if (user.Country.GomsSubsidiaryId != GlobalConfig.MiddleEastGomsSubsidiaryId)
                            ccTypes = user.Country.GetGomsCreditCardTypes();
                        else
                        {
                            var listOfMiddleEastAllowedCountries = GlobalConfig.MECountriesAllowedForCreditCard.Split(',');
                            if (listOfMiddleEastAllowedCountries.Contains(user.Country.Code))
                                ccTypes = user.Country.GetGomsCreditCardTypes();
                        }
                    }
                    else
                        ccTypes = user.Country.GetGomsCreditCardTypes();
                    if (ccTypes != null)
                        return RedirectToAction("CreditCard", new { id = id, ReturnUrl = ReturnUrl });
                }
                if (GlobalConfig.IsPaypalPaymentModeEnabled)
                    if (user.Country.Currency.IsPayPalSupported)
                        return RedirectToAction("PayPal", new { id = id, ReturnUrl = ReturnUrl });
                if (GlobalConfig.IsPpcPaymentModeEnabled)
                    return RedirectToAction("PrepaidCard", new { id = id, ReturnUrl = ReturnUrl });
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("EWallet", new { id = id, ReturnUrl = ReturnUrl });
        }

        public string CancelPaypal(string id)
        {
            PaymentHelper.CancelPaypalRecurring(id);
            return "";
        }
    }
}
