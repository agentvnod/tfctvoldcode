using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using IPTV2_Model;
using TFCTV.Helpers;
using StackExchange.Profiling;
using System.Collections;
using System.Globalization;
using GOMS_TFCtv;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class VinodController : Controller
    {
        [RequireHttps]
        public ActionResult Details(string id, int? PackageOption, int? ProductOption)
        {
            var profiler = MiniProfiler.Current;
            #region if (!Request.Cookies.AllKeys.Contains("version"))
            if (!Request.Cookies.AllKeys.Contains("version"))
            {
                List<SubscriptionProductA> productList = null;
                try
                {
                    if (!String.IsNullOrEmpty(id))
                        if (String.Compare(id, "mayweather-vs-pacquiao-may-3", true) == 0)
                            id = GlobalConfig.PacMaySubscribeCategoryId.ToString();

                    var context = new IPTV2Entities();
                    string ip = Request.IsLocal ? "78.95.139.99" : Request.GetUserHostAddressFromCloudflare();
                    if (GlobalConfig.isUAT && !String.IsNullOrEmpty(Request.QueryString["ip"]))
                        ip = Request.QueryString["ip"].ToString();
                    var location = MyUtility.GetLocationBasedOnIpAddress(ip);
                    var CountryCode = location.countryCode;
                    //var CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();

                    User user = null;
                    Guid? UserId = null;
                    if (User.Identity.IsAuthenticated)
                    {
                        UserId = new Guid(User.Identity.Name);
                        user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                            CountryCode = user.CountryCode;
                    }

                    var country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);

                    var premium = MyUtility.StringToIntList(GlobalConfig.PremiumProductIds);
                    var lite = MyUtility.StringToIntList(GlobalConfig.LiteProductIds);
                    var movie = MyUtility.StringToIntList(GlobalConfig.MovieProductIds);
                    var litepromo = MyUtility.StringToIntList(GlobalConfig.LitePromoProductIds);
                    var promo2014promo = MyUtility.StringToIntList(GlobalConfig.Promo2014ProductIds);
                    var q22015promo = MyUtility.StringToIntList(GlobalConfig.Q22015ProductId);

                    var productIds = premium;//productids
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(o => o.PackageId == GlobalConfig.serviceId);

                    bool IsAlaCarteProduct = true;
                    #region code for showing product based on id
                    if (!String.IsNullOrEmpty(id))
                    {
                        int pid;
                        if (int.TryParse(id, out pid)) // id is an integer, then it's a show, get ala carte only
                        {
                            var show = (Show)context.CategoryClasses.Find(pid);

                            if (User.Identity.IsAuthenticated)
                            {
                                var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                                var isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
                                if (!isEmailAllowed)
                                {
                                    if (!ContextHelper.IsCategoryViewableInUserCountry(show, CountryCode))
                                        return RedirectToAction("Index", "Home");

                                    string CountryCodeBasedOnIpAddress = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                                    if (!ContextHelper.IsCategoryViewableInUserCountry(show, CountryCodeBasedOnIpAddress))
                                        return RedirectToAction("Index", "Home");
                                }
                            }

                            try
                            {
                                var ShowParentCategories = ContextHelper.GetShowParentCategories(show.CategoryId);
                                if (!String.IsNullOrEmpty(ShowParentCategories))
                                {
                                    ViewBag.ShowParentCategories = ShowParentCategories;
                                    if (!MyUtility.IsWhiteListed(String.Empty))
                                    {
                                        var ids = MyUtility.StringToIntList(ShowParentCategories);
                                        var alaCarteIds = MyUtility.StringToIntList(GlobalConfig.UXAlaCarteParentCategoryIds);
                                        var DoCheckForGeoIPRestriction = ids.Intersect(alaCarteIds).Count() > 0;
                                        if (DoCheckForGeoIPRestriction)
                                        {
                                            var ipx = Request.IsLocal ? "221.121.187.253" : String.Empty;
                                            if (GlobalConfig.isUAT)
                                                ipx = Request["ip"];
                                            if (!MyUtility.CheckIfCategoryIsAllowed(show.CategoryId, context, ipx))
                                            {
                                                var ReturnCode = new TransactionReturnType()
                                                {
                                                    StatusHeader = "Content Advisory",
                                                    StatusMessage = "This content is currently not available in your area.",
                                                    StatusMessage2 = "You may select from among the hundreds of shows and movies that are available on the site."
                                                };
                                                TempData["ErrorMessage"] = ReturnCode;
                                                return RedirectToAction("Index", "Home");
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }

                            productIds = show.GetPackageProductIds(offering, CountryCode, RightsType.Online);
                            var showProductIds = show.GetShowProductIds(offering, CountryCode, RightsType.Online);
                            if (showProductIds != null)
                                productIds = productIds.Union(showProductIds);

                            //Remove premium, lite & movie packages from list
                            productIds = productIds.Except(premium).Except(lite).Except(movie).Except(litepromo);
                            IsAlaCarteProduct = true;

                            if (pid == GlobalConfig.PacMaySubscribeCategoryId)
                            {
                                ViewBag.IsPacMay = true;
                                //add the additional product ids
                                var additional_pacmay_productids = MyUtility.StringToIntList(GlobalConfig.PacMaySubscribeProductIds);
                                productIds = productIds.Union(additional_pacmay_productids);
                                TempData["IsPacPurchase"] = true;
                            }
                        }
                        else // get packages only
                        {
                            if (String.Compare(id, "Lite", true) == 0)
                                productIds = lite;
                            else if (String.Compare(id, "Movie", true) == 0)
                                productIds = movie;
                            else if (String.Compare(id, "LitePromo", true) == 0)
                            {
                                var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.LiteChurnersPromoId);
                                if (promo != null)
                                {
                                    var registDt = DateTime.Now;
                                    if (promo.StatusId == GlobalConfig.Visible && promo.StartDate < registDt && promo.EndDate > registDt)
                                        productIds = litepromo; // discounted lite product Ids
                                }
                            }
                            else if (String.Compare(id, "Promo201410", true) == 0)
                            {
                                var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.Promo201410PromoId);
                                if (promo != null)
                                {
                                    var registDt = DateTime.Now;
                                    if (promo.StatusId == GlobalConfig.Visible && promo.StartDate < registDt && promo.EndDate > registDt)
                                    {
                                        ViewBag.IsPromo2014 = true;
                                        ViewBag.PreventAutoRenewalCheckboxFromUntick = true;
                                        productIds = promo2014promo; // discounted 3month premium
                                        //get amount of 3 month premium
                                        var premium1mo = context.Products.FirstOrDefault(p => p.ProductId == 1);
                                        if (premium1mo != null)
                                            ViewBag.Premium1MonthPrice = premium1mo.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, country.CurrencyCode, true) == 0);

                                        //check purchasetry
                                        try
                                        {
                                            var user_purchases = context.PurchaseItems.Where(p => promo2014promo.Contains(p.ProductId) && p.RecipientUserId == user.UserId);
                                            if (user_purchases != null)
                                            {
                                                var purchaseIds = user_purchases.Select(p => p.PurchaseId);
                                                var purchases = context.Purchases.Count(p => purchaseIds.Contains(p.PurchaseId) && p.Date > promo.StartDate && p.Date < promo.EndDate);
                                                if (purchases > 0)
                                                    ViewBag.UserHasAvailedOfPromo201410 = true;
                                            }
                                        }
                                        catch (Exception) { }
                                    }
                                }
                            }
                            else if (String.Compare(id, "lckbprea", true) == 0)
                            {
                                var registDt = DateTime.Now;
                                var preBlackPromo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.PreBlackPromoId && p.StatusId == GlobalConfig.Visible && p.StartDate < registDt && p.EndDate > registDt);
                                if (preBlackPromo != null)
                                {
                                    //productIds = MyUtility.StringToIntList(GlobalConfig.PreBlackPromoProductIds);
                                    var productIdsList = new List<Int32>();
                                    var preBlackPromoIds = MyUtility.StringToIntList(GlobalConfig.PreBlackPromoProductIds).ToList();
                                    var preBlackPurchaseItems = context.PurchaseItems.Where(p => preBlackPromoIds.Contains(p.ProductId) && p.RecipientUserId == user.UserId).Select(p => p.SubscriptionProduct);
                                    List<Int32> preBlackPurchaseItemsList = preBlackPurchaseItems.Select(p => p.Duration).ToList();
                                    var sumDuration = preBlackPurchaseItemsList.Take(preBlackPurchaseItemsList.Count).Sum();
                                    if (sumDuration > 11)
                                    {
                                        var ReturnCode = new TransactionReturnType()
                                        {
                                            StatusCode = (int)ErrorCodes.UnknownError,
                                            StatusMessage = "You have reached the maximum number of purchases for this product.",
                                            info = "pre-black",
                                            TransactionType = "Purchase"
                                        };

                                        TempData["ErrorMessage"] = ReturnCode;
                                        return RedirectToAction("Index", "Home");
                                    }
                                    if (sumDuration < 12)
                                        productIdsList.Add(preBlackPromoIds[0]);
                                    if (sumDuration < 9)
                                        productIdsList.Add(preBlackPromoIds[1]);
                                    if (sumDuration == 0)
                                        productIdsList.Add(preBlackPromoIds[2]);
                                    productIds = productIdsList;
                                }
                            }
                            else if (String.Compare(id, "atra", true) == 0)
                            {
                                var registDt = DateTime.Now;
                                var productIdsList = new List<Int32>();
                                var projectBlackPromoIdsList = MyUtility.StringToIntList(GlobalConfig.ProjectBlackPromoIds);
                                var projectBlackProductIdsList = MyUtility.StringToIntList(GlobalConfig.ProjectBlackProductIds).ToList();
                                var blackProjetPromo = context.Promos.FirstOrDefault(p => projectBlackPromoIdsList.Contains(p.PromoId) && p.StatusId == GlobalConfig.Visible && p.StartDate < registDt && p.EndDate > registDt);
                                if (blackProjetPromo != null)
                                {
                                    if (User.Identity.IsAuthenticated)
                                    {
                                        var userpromo = context.UserPromos.FirstOrDefault(u => u.UserId == UserId && projectBlackPromoIdsList.Contains(u.PromoId));
                                        if (userpromo == null)
                                        {
                                            if (projectBlackProductIdsList.Count > 0)
                                            {
                                                productIdsList.Add(projectBlackProductIdsList[projectBlackProductIdsList.Count - 1]);
                                                productIds = productIdsList;
                                                ViewBag.isProjectBlack = true;
                                                var premium3mo = context.Products.FirstOrDefault(p => p.ProductId == 1);
                                                if (premium3mo != null)
                                                    ViewBag.Premium1MonthPrice = premium3mo.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, country.CurrencyCode, true) == 0);
                                            }
                                            else
                                            {
                                                return RedirectToAction("Details", new { id = String.Empty });
                                            }
                                        }
                                        else
                                        {
                                            var ReturnCode = new TransactionReturnType()
                                            {
                                                StatusCode = (int)ErrorCodes.UnknownError,
                                                StatusMessage = "You have already claimed this promo.",
                                                info = "black",
                                                TransactionType = "Purchase"
                                            };

                                            TempData["ErrorMessage"] = ReturnCode;
                                            return RedirectToAction("Index", "Home");
                                        }
                                    }
                                }
                                else
                                {
                                    return RedirectToAction("Details", new { id = String.Empty });
                                }
                            }
                            else if (String.Compare(id, "aintone", true) == 0)
                            {
                                var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.Q22015PromoId);
                                if (promo != null)
                                {
                                    var registDt = DateTime.Now;
                                    if (promo.StatusId == GlobalConfig.Visible && promo.StartDate < registDt && promo.EndDate > registDt)
                                    {
                                        ViewBag.IsPromoQ22015 = true;
                                        ViewBag.PreventAutoRenewalCheckboxFromUntick = true;
                                        productIds = q22015promo; // discounted 1month premium
                                        //get amount of 3 month premium
                                        var premium1mo = context.Products.FirstOrDefault(p => p.ProductId == 1);
                                        if (premium1mo != null)
                                            ViewBag.Premium1MonthPrice = premium1mo.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, country.CurrencyCode, true) == 0);

                                        //check purchasetry
                                        try
                                        {
                                            var user_purchases = context.PurchaseItems.Where(p => q22015promo.Contains(p.ProductId) && p.RecipientUserId == user.UserId);
                                            if (user_purchases != null)
                                            {
                                                var purchaseIds = user_purchases.Select(p => p.PurchaseId);
                                                var purchases = context.Purchases.Count(p => purchaseIds.Contains(p.PurchaseId) && p.Date > promo.StartDate && p.Date < promo.EndDate);
                                                if (purchases > 0)
                                                    ViewBag.UserHasAvailedOfQ22015Promo = true;
                                            }
                                        }
                                        catch (Exception) { }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region code for shwoing all the products
                    else
                    {
                        var premiumProductGroupIds = MyUtility.StringToIntList(GlobalConfig.premiumProductGroupIds);
                        var productGroup = context.ProductGroups.Where(p => premiumProductGroupIds.Contains(p.ProductGroupId));
                        if (productGroup != null)
                        {
                            foreach (var gr in productGroup)
                            {
                                var groupProductIds = gr.SubscriptionProducts.Select(p => p.ProductId);
                                productIds = productIds.Union(groupProductIds);
                            }
                        }
                    }
                    #endregion
                    ViewBag.IsAlaCarteProduct = IsAlaCarteProduct;

                    //Check for Recurring Billing
                    List<int> recurring_packages = null;
                    if (User.Identity.IsAuthenticated)
                    {
                        var recurring = context.RecurringBillings.Where(r => r.UserId == user.UserId && r.StatusId == GlobalConfig.Visible);
                        if (recurring != null)
                            recurring_packages = recurring.Select(r => r.PackageId).ToList();
                    }

                    var products = context.Products.Where(p => productIds.Contains(p.ProductId) && p is SubscriptionProduct);
                    #region products not null
                    if (products != null)
                    {
                        if (products.Count() > 0)
                        {
                            Product freeProduct = null;
                            ProductPrice freeProductPrice = null;
                            productList = new List<SubscriptionProductA>();
                            foreach (SubscriptionProduct item in products)
                            {
                                if (item.IsAllowed(CountryCode) && item.IsForSale && item.StatusId == GlobalConfig.Visible)
                                {
                                    #region pakage subscription
                                    if (item is PackageSubscriptionProduct)
                                    {
                                        var psp = (PackageSubscriptionProduct)item;
                                        var package = psp.Packages.FirstOrDefault();
                                        if (package != null)
                                        {
                                            int counter = package.Package.GetAllOnlineShowIds(CountryCode).Count();
                                            var sItem = new SubscriptionProductA()
                                            {
                                                package = (Package)package.Package,
                                                product = item,
                                                contentCount = counter,
                                                contentDescription = ContentDescriptionFlooring(counter > 1 ? counter - 1 : counter, true),
                                                ListOfDescription = ContextHelper.GetPackageFeatures(CountryCode, package),
                                                IsUserEnrolledToSameRecurringPackage = recurring_packages == null ? false : recurring_packages.Contains(package.PackageId)
                                            };

                                            try
                                            {
                                                if (!String.IsNullOrEmpty(psp.ProductGroup.Description))
                                                    sItem.Blurb = psp.ProductGroup.Blurb;
                                            }
                                            catch (Exception) { }

                                            if (item.RegularProductId != null)
                                            {
                                                try
                                                {
                                                    var regularProduct = context.Products.FirstOrDefault(p => p.ProductId == item.RegularProductId);
                                                    if (regularProduct != null)
                                                        if (regularProduct is SubscriptionProduct)
                                                        {
                                                            sItem.regularProduct = (SubscriptionProduct)regularProduct;
                                                            try
                                                            {
                                                                sItem.regularProductPrice = regularProduct.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, country.CurrencyCode, true) == 0);
                                                            }
                                                            catch (Exception) { }

                                                        }
                                                }
                                                catch (Exception) { }
                                            }
                                            else if (IsXoomUser)
                                            {
                                                if (freeProductPrice != null && freeProduct != null)
                                                {
                                                    sItem.freeProduct = (SubscriptionProduct)freeProduct;
                                                    sItem.freeProductPrice = freeProductPrice;
                                                }
                                            }

                                            try
                                            {
                                                sItem.productPrice = item.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, country.CurrencyCode, true) == 0);
                                                if (sItem.productPrice != null)
                                                    productList.Add(sItem);
                                            }
                                            catch (Exception) { ViewBag.ProductPriceError = true; }
                                        }
                                    }
                                    #endregion
                                    #region show subscription
                                    else if (item is ShowSubscriptionProduct)
                                    {
                                        var ssp = (ShowSubscriptionProduct)item;
                                        var category = ssp.Categories.FirstOrDefault();
                                        if (category != null)
                                        {
                                            var sItem = new SubscriptionProductA()
                                            {
                                                product = item,
                                                contentCount = 1,
                                                show = category.Show,
                                                ShowDescription = category.Show.Blurb
                                            };
                                            try
                                            {
                                                if (!String.IsNullOrEmpty(ssp.ProductGroup.Description))
                                                    sItem.Blurb = ssp.ProductGroup.Description;
                                            }
                                            catch (Exception) { }

                                            try
                                            {
                                                sItem.productPrice = item.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, country.CurrencyCode, true) == 0);
                                                if (sItem.productPrice != null)
                                                    productList.Add(sItem);
                                            }
                                            catch (Exception) { ViewBag.ProductPriceError = true; }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion
                    else
                        return RedirectToAction("Index", "Home");
                    //var productPackages = context.ProductPackages.Where(p => productIds.Contains(p.ProductId));
                    //if (productPackages != null)
                    //{
                    //    if (productPackages.Count() > 0)
                    //    {
                    //        productList = new List<SubscriptionProductA>();
                    //        foreach (var item in productPackages)
                    //        {
                    //            if (item.Product.IsAllowed(CountryCode) && item.Product.IsForSale && item.Product.StatusId == GlobalConfig.Visible)
                    //            {
                    //                var package = item.Product.Packages.FirstOrDefault();
                    //                if (package != null)
                    //                {
                    //                    int counter = item.Package.GetAllOnlineShowIds(CountryCode).Count();
                    //                    var sItem = new SubscriptionProductA()
                    //                    {
                    //                        package = (Package)item.Package,
                    //                        product = item.Product,
                    //                        contentCount = counter,
                    //                        contentDescription = ContentDescriptionFlooring(counter > 1 ? counter - 1 : counter, true),
                    //                        ListOfDescription = ContextHelper.GetPackageFeatures(CountryCode, package)
                    //                    };

                    //                    try
                    //                    {
                    //                        sItem.productPrice = item.Product.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, country.CurrencyCode, true) == 0);
                    //                        productList.Add(sItem);
                    //                    }
                    //                    catch (Exception) { ViewBag.ProductPriceError = true; }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    #region product list not null
                    if (productList != null)
                    {
                        if (productList.Count() > 0)
                        {
                            #region productList.Count() > 0
                            //Credit card
                            ViewBag.HasCreditCardEnrolled = false;
                            ViewBag.CreditCardAvailability = true;
                            try
                            {
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

                                if (User.Identity.IsAuthenticated)
                                {
                                    //Get Credit Card List
                                    var ccTypes = user.Country.GetGomsCreditCardTypes();

                                    if (user.Country.GomsSubsidiaryId != GlobalConfig.MiddleEastGomsSubsidiaryId)
                                        ccTypes = user.Country.GetGomsCreditCardTypes();
                                    else
                                    {
                                        var listOfMiddleEastAllowedCountries = GlobalConfig.MECountriesAllowedForCreditCard.Split(',');
                                        if (listOfMiddleEastAllowedCountries.Contains(user.Country.Code))
                                            ccTypes = user.Country.GetGomsCreditCardTypes();
                                        else
                                            ccTypes = null;
                                    }

                                    if (ccTypes == null)
                                        ViewBag.CreditCardAvailability = false;
                                    else
                                    {
                                        List<TFCTV.Helpers.CreditCard> clist = new List<TFCTV.Helpers.CreditCard>();
                                        foreach (var item in ccTypes)
                                            clist.Add(new TFCTV.Helpers.CreditCard() { value = ((int)item).ToString(), text = item.ToString().Replace('_', ' ') });
                                        ViewBag.CreditCardList = clist;
                                    }
                                    if (GlobalConfig.IsRecurringBillingEnabled)
                                    {
                                        var UserCreditCard = user.CreditCards.FirstOrDefault(c => c.StatusId == GlobalConfig.Visible && c.OfferingId == offering.OfferingId);
                                        if (UserCreditCard != null)
                                        {
                                            ViewBag.HasCreditCardEnrolled = true;
                                            ViewBag.UserCreditCard = UserCreditCard;
                                        }
                                    }
                                }
                                else
                                {
                                    var cCountry = context.Countries.FirstOrDefault(c => String.Compare(c.Code, location.countryCode, true) == 0);
                                    if (cCountry != null)
                                    {
                                        var ccTypes = cCountry.GetGomsCreditCardTypes();
                                        if (cCountry.GomsSubsidiaryId != GlobalConfig.MiddleEastGomsSubsidiaryId)
                                            ccTypes = cCountry.GetGomsCreditCardTypes();
                                        else
                                        {
                                            var listOfMiddleEastAllowedCountries = GlobalConfig.MECountriesAllowedForCreditCard.Split(',');
                                            if (listOfMiddleEastAllowedCountries.Contains(cCountry.Code))
                                                ccTypes = cCountry.GetGomsCreditCardTypes();
                                            else
                                                ccTypes = null;
                                        }

                                        if (ccTypes == null)
                                            ViewBag.CreditCardAvailability = false;
                                        else
                                        {
                                            List<TFCTV.Helpers.CreditCard> clist = new List<TFCTV.Helpers.CreditCard>();
                                            foreach (var item in ccTypes)
                                                clist.Add(new TFCTV.Helpers.CreditCard() { value = ((int)item).ToString(), text = item.ToString().Replace('_', ' ') });
                                            ViewBag.CreditCardList = clist;
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }

                            if (User.Identity.IsAuthenticated)
                            {
                                //E-Wallet                         
                                ViewBag.InsufficientWalletBalance = false;
                                try
                                {
                                    var userWallet = user.UserWallets.FirstOrDefault(u => String.Compare(u.Currency, user.Country.CurrencyCode, true) == 0 && u.IsActive == true);
                                    if (userWallet != null)
                                    {
                                        var UserCurrencyCode = user.Country.CurrencyCode;
                                        var productPrice = productList.First().product.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, UserCurrencyCode, true) == 0);
                                        if (productPrice == null)
                                            productPrice = productList.First().product.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, GlobalConfig.DefaultCurrency, true) == 0);
                                        ViewBag.ProductPrice = productPrice;
                                        if (productPrice.Amount > userWallet.Balance)
                                            ViewBag.InsufficientWalletBalance = true;
                                    }
                                }
                                catch (Exception) { }

                                //check if user is first time subscriber
                                ViewBag.IsFirstTimeSubscriber = user.IsFirstTimeSubscriber(offering);
                            }
                            #endregion
                        }

                    }


                    if (productList != null)
                        if (productList.Count() > 0)
                        {
                            if (String.Compare(id, GlobalConfig.PacMaySubscribeCategoryId.ToString(), true) == 0 && !User.Identity.IsAuthenticated)
                                return View("DetailsPacMay2", productList);
                            else
                            {
                                if (ProductOption != null)
                                    ViewBag.ProductOption = (int)ProductOption;
                                return View("Details5", productList);
                            }
                        }

                    return RedirectToAction("Details", new { id = String.Empty });
                    #endregion
            #endregion
                }
                catch (Exception e) { MyUtility.LogException(e); }
                return RedirectToAction("Index", "Home");
            }
        }

        private string ContentDescriptionFlooring(int contentCount, bool floorToTens = false)
        {
            string result = String.Empty;
            if (floorToTens)
            {
                if (contentCount < 10)
                    result = String.Format("{0}", contentCount);
                else
                    result = String.Format("{0}+", contentCount.Floor(10));
                return result;
            }

            if (contentCount > 1000)
                result = String.Format("{0}+", contentCount.Floor(1000));
            else if (contentCount > 100)
                result = String.Format("{0}+", contentCount.Floor(100));
            else if (contentCount > 10)
                result = String.Format("{0}+", contentCount.Floor(10));
            else
                result = String.Format("{0}", contentCount);
            return result;
        }
        public static void coaiho()
        {
            if (!Request.Cookies.AllKeys.Contains("version")) //starts with first line code
            {//ask question how to get the cookie and where it is being created(check the code once)
                if (!User.Identity.IsAuthenticated)
                {
                    if (!String.IsNullOrEmpty(id))
                    {
                        // compare the strings based on ids then create the cookies
                        // Question to ask why we are creating cookies. We are checking unnecessary if conditions even if it is integer.
                        //redirecting the user to sign in page based on mobile or desktop
                    }
                    else
                    {
                        //redirecting the user to sign in page based on mobile or desktop
                    }
                }
                else
                {
                    //comparing id with mayweather-vs-pacquiao-may-3 and assigning that categoryid to the id varible 
                }
                //normal flow
                //getting the user country code based on ip //checking the user authenticity and getting user countrycode // getting country based on country code
                //getting productids of premium, lite, movie, litepromo, promo2014promo, q22015promo so they have to be removed after getting actual productts
                // var productIds = premium;//productids // getting offering and then services
                if (!String.IsNullOrEmpty(id))
                {
                    if (int.TryParse(id, out pid)) // id is an integer, then it's a show, get ala carte only
                    {
                        //var show = (Show)context.CategoryClasses.Find(pid);
                        //check user email is allowed against a set of mails added manually //if user is not allowed by ip and registered country then redirect him to home page// 
                        //var ShowParentCategories = ContextHelper.GetShowParentCategories(show.CategoryId);// ViewBag.ShowParentCategories = ShowParentCategories;
                        //if ip is not whitelisted redirect him to home page by showing some error message
                        //productIds = show.GetPackageProductIds(offering, CountryCode, RightsType.Online);//*
                        //var showProductIds = show.GetShowProductIds(offering, CountryCode, RightsType.Online);//*
                        //if (showProductIds != null)
                        //    productIds = productIds.Union(showProductIds);//*
                        ////Remove premium, lite & movie packages from list
                        //productIds = productIds.Except(premium).Except(lite).Except(movie).Except(litepromo);IsAlaCarteProduct = true;
                        //if pid == PacMaySubscribeCategoryId then get additional pacproducts//productIds = productIds.Union(additional_pacmay_productids);//ViewBag.IsPacMay = true;
                    }
                    else//if id is not integer
                    {
                        //if id is lite then productids=lite
                        //if id is movie productids=movie
                        //if id is litepromo then productids=litepromo
                        //if id is promo2014promo productids=promo2014promo// check whether user has already has this offer//ViewBag.UserHasAvailedOfPromo201410 = true;
                        //if id is lckpreba then productids=preblack products
                        //if id is atra productids=projectblackproducts
                        //if id is aintone // check user has already has this offer//ViewBag.UserHasAvailedOfQ22015Promo = true;
                    }
                }
                else
                {
                    //getting premium product group ids and then products// make an union with productids with the premium produts
                }
                // List<int> recurring_packages = null; getting recuring billings on the user
                //get all the products intersect with productids(driven from the above code)
                if (products != null)//check products is empty
                {
                    if (products.Count() > 0)//products greater than zero
                    {
                        //productList = new List<SubscriptionProductA>(); Intializing the product list which was declared in the top
                        foreach (SubscriptionProduct item in products)
                        {
                            if (item.IsAllowed(CountryCode) && item.IsForSale && item.StatusId == GlobalConfig.Visible)
                            {
                                if (item is PackageSubscriptionProduct)
                                {
                                    //get package associated with the current product//check for pqackage emptiness// int counter = package.Package.GetAllOnlineShowIds(CountryCode).Count();
                                    //Intializing product with different fields package , product, contentCount, contentDescription, ListOfDescription, IsUserEnrolledToSameRecurringPackage
                                    //checking product group description is empty then sitem.blurb=product group blurb
                                    //find out the regular product and its price if the product contains a regular product//add these details to sitem
                                    //find the price of the product and add it to sitem//if (sItem.productPrice != null) productList.Add(sItem);

                                }
                                else if (item is ShowSubscriptionProduct)
                                {
                                    //get category associated with the current product//if category not null intialize sitem with respective fields
                                    //find price of the product assign it to sitem//if (sItem.productPrice != null) productList.Add(sItem);
                                }
                            }
                        }
                    }
                }
                else
                {
                    //return RedirectToAction("Index", "Home");                             
                }
                if (productList != null)
                {
                    if (productList.Count() > 0)
                    {//ViewBag.HasCreditCardEnrolled = false; ViewBag.CreditCardAvailability = true;   ViewBag.Months = list;   ViewBag.Years = list; 
                        if (User.Identity.IsAuthenticated)//Get Credit Card List
                        {
                            //get credit card types based on goms subsidiary//if empty make ViewBag.CreditCardAvailability = false else prepare creditcardtype list// ViewBag.CreditCardList = clist; 
                            //if (GlobalConfig.IsRecurringBillingEnabled) get user credit card//if not null ViewBag.HasCreditCardEnrolled = true;// ViewBag.UserCreditCard = UserCreditCard;
                           
                        }
                        else
                        {
                            //if user is not authenticated still we can show the available payment modes and getting all the credit card types
                        }
                        if (User.Identity.IsAuthenticated)//check use wallet balance
                        {
                            //if user wallet balance is less than a product price then  ViewBag.InsufficientWalletBalance = true;
                        }
                        if (String.Compare(id, GlobalConfig.PacMaySubscribeCategoryId.ToString(), true) == 0 && !User.Identity.IsAuthenticated)
                            //return View("DetailsPacMay2", productList);
                        else
                        {
                            //if (ProductOption != null)
                            //    ViewBag.ProductOption = (int)ProductOption;
                            //return View("Details5", productList);
                        }
                    }
                }
                 //return RedirectToAction("Details", new { id = String.Empty });

            }
            else
            {
                //not required          
            }
        }

    }

}
