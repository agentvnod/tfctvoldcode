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
    public class SubscribeController : Controller
    {
        //
        // GET: /Subscribe/

        //public ActionResult Index()
        //{
        //    return View();
        //}

        //public ActionResult Create(int? id)
        //{
        //    if (id == null)
        //        return RedirectToAction("Index", "Home");

        //    var context = new IPTV2Entities();
        //    string CountryCode = GlobalConfig.DefaultCountry;
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        System.Guid userId = new System.Guid(User.Identity.Name);
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //        if (user != null)
        //            CountryCode = user.CountryCode;
        //    }

        //    Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
        //    if (product == null)
        //        return RedirectToAction("Index", "Home");

        //    SubscriptionProductType type = ContextHelper.GetProductType(product);

        //    ViewBag.Product = product;

        //    var price = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(CountryCode));

        //    var currency = price.Currency;
        //    if (currency.IsLeft)
        //        ViewBag.ProductAmountFormat = String.Format("{0}{1}", currency.Symbol, price.Amount.ToString("F"));
        //    else
        //        ViewBag.ProductAmountFormat = String.Format("{0}{1}", price.Amount.ToString("F"), currency.Symbol);
        //    return View(product);
        //}

        //public ActionResult Wallet(int? id)
        //{
        //    if (id == null)
        //        return PartialView("_SubscribeErrorPartial");

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var context = new IPTV2Entities();
        //        System.Guid userId = new System.Guid(User.Identity.Name);
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //        if (user != null)
        //        {
        //            Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
        //            if (product == null)
        //                return PartialView("_SubscribeErrorPartial");
        //            ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //            UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //            ViewBag.ProductId = product.ProductId;
        //            //Check if wallet has bigger amount than product then tick isBuyable if condition is true.
        //            ViewBag.isBuyable = (wallet.Balance >= priceOfProduct.Amount) ? true : false;
        //            string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
        //            Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
        //            ViewBag.WalletBalance = wallet.Balance;
        //            ViewBag.User = user;
        //            if (currency.IsLeft)
        //                ViewBag.WalletBalanceFormat = String.Format("Your TFC.tv wallet has a balance of {0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
        //            else
        //                ViewBag.WalletBalanceFormat = String.Format("Your TFC.tv wallet has a balance of {0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
        //            return PartialView("_WalletPartial");
        //        }
        //    }
        //    return PartialView("_SubscribeErrorPartial");
        //}

        //public ActionResult PrepaidCard(int? id)
        //{
        //    if (id == null)
        //        return PartialView("_SubscribeErrorPartial");

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var context = new IPTV2Entities();
        //        System.Guid userId = new System.Guid(User.Identity.Name);
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //        if (user != null)
        //        {
        //            Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
        //            if (product == null)
        //                return PartialView("_SubscribeErrorPartial");
        //            ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //            ViewBag.ProductId = product.ProductId;
        //            ViewBag.User = user;
        //            return PartialView("_PrepaidCardPartial");
        //        }
        //    }
        //    return PartialView("_SubscribeErrorPartial");
        //}

        //public ActionResult CreditCard(int? id)
        //{
        //    if (id == null)
        //        return PartialView("_SubscribeErrorPartial");
        //    var context = new IPTV2Entities();
        //    Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
        //    if (product == null)
        //        return PartialView("_SubscribeErrorPartial");
        //    return PartialView("_CreditCardPartial");
        //}

        //public ActionResult Paypal(int? id)
        //{
        //    if (id == null)
        //        return PartialView("_SubscribeErrorPartial");

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var context = new IPTV2Entities();
        //        System.Guid userId = new System.Guid(User.Identity.Name);
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //        if (user != null)
        //        {
        //            Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
        //            if (product == null)
        //                return PartialView("_SubscribeErrorPartial");
        //            string subscriptionType = ContextHelper.GetProductType(product).ToString();

        //            ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //            ViewBag.ProductId = product.ProductId;
        //            ViewBag.productName = product.Name;
        //            ViewBag.ProductPrice = priceOfProduct.Amount.ToString("F");
        //            ViewBag.Currency = priceOfProduct.CurrencyCode;
        //            ViewBag.subscriptionType = subscriptionType;
        //            ViewBag.User = user;
        //            ViewBag.returnURL = GlobalConfig.PayPalReturnUrl; // String.Format("http://{0}{1}/Subscribe/PayPalHandler", Request.Url.Host, (Request.Url.Port != 80) ? ":" + Request.Url.Port.ToString() : "");
        //            return PartialView("_PaypalPartial");
        //        }
        //    }
        //    return PartialView("_SubscribeErrorPartial");
        //}

        //[HttpPost]
        //public ActionResult PayViaWallet(int? id)
        //{
        //    if (id == null)
        //        return PartialView("_SubscribeErrorPartial");

        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    collection.Add("errorCode", (int)ErrorCodes.UnknownError);
        //    collection.Add("errorMessage", MyUtility.getErrorMessage(ErrorCodes.UnknownError));
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var context = new IPTV2Entities();
        //        System.Guid userId = new System.Guid(User.Identity.Name);
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //        Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
        //        if (product == null)
        //            return PartialView("_SubscribeErrorPartial");
        //        SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);

        //        if (user != null)
        //        {
        //            ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //            UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //            ErrorCodes errorCode = ErrorCodes.UnknownError;
        //            if (wallet.Balance >= priceOfProduct.Amount)
        //            {
        //                errorCode = PaymentHelper.PayViaWallet(context, userId, product.ProductId, subscriptionType);
        //                string CountryCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
        //                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CountryCode);
        //                collection["errorCode"] = (int)errorCode;
        //                if (errorCode == ErrorCodes.Success)
        //                {
        //                    string balance;
        //                    if (currency.IsLeft)
        //                        balance = String.Format("The amount of this product has been deducted from your wallet.<br/>Your new wallet balance is {0}{1}.", currency.Symbol, wallet.Balance.ToString("F"));
        //                    else
        //                        balance = String.Format("The amount of this product has been deducted from your wallet.<br/>Your new wallet balance is {0}{1}.", wallet.Balance.ToString("F"), currency.Symbol);

        //                    collection["errorMessage"] = balance;
        //                }
        //                else
        //                    collection.Add("errorMessage", MyUtility.getErrorMessage(errorCode));
        //            }
        //            else
        //            {
        //                errorCode = ErrorCodes.InsufficientWalletLoad;
        //                collection["errorCode"] = (int)errorCode;
        //                collection["errorMessage"] = MyUtility.getErrorMessage(errorCode);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
        //        collection["errorMessage"] = "Your session has expired. Please login again.";
        //    }

        //    return Content(MyUtility.buildJson(collection), "application/json");
        //}

        //[HttpPost]
        //public void PaypalIPNHandler()
        //{
        //    try
        //    {
        //        string result;
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GlobalConfig.PayPalSubmitUrl);
        //        request.Method = "POST";
        //        request.ContentType = "application/x-www-form-urlencoded";

        //        byte[] param = Request.BinaryRead(HttpContext.Request.ContentLength);
        //        string requestParam = Encoding.ASCII.GetString(param);
        //        string ipnPostData = requestParam;
        //        requestParam += "&cmd=_notify-validate";
        //        request.ContentLength = requestParam.Length;

        //        StreamWriter sw = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
        //        sw.Write(ipnPostData);
        //        sw.Close();

        //        StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream());
        //        result = sr.ReadToEnd();
        //        sr.Close();

        //        switch (result)
        //        {
        //            case "VERIFIED": break;
        //            //check the payment_status is Completed
        //            //check that txn_id has not been previously processed
        //            //check that receiver_email is your Primary PayPal email
        //            //check that payment_amount/payment_currency are correct
        //            //process payment
        //            case "INVALID": break;
        //            default: break;
        //        }
        //    }
        //    catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        //}

        //public ActionResult PayPalHandler()
        //{
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    ErrorCodes errorCode = ErrorCodes.UnknownError;
        //    string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
        //    collection.Add("errorCode", 1);
        //    collection.Add("errorMessage", errorMessage);

        //    string PDTpostData = "";

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var context = new IPTV2Entities();
        //        System.Guid userId = new System.Guid(User.Identity.Name);
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);

        //        string tx = Request["tx"];
        //        string st = Request["st"];
        //        string amt = Request["amt"];
        //        string cc = Request["cc"];

        //        //Call PDT of Paypal

        //        PDTpostData = PaymentHelper.PDTHandler(tx);
        //        Custom custom = PDTHolder.Parse(PDTpostData);

        //        int productId = custom.productId;
        //        //string subscriptionType = custom.subscriptionType;

        //        Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
        //        if (product == null)
        //            return PartialView("_SubscribeErrorPartial");
        //        SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);

        //        if (user != null)
        //        {
        //            ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //            errorCode = PaymentHelper.PayViaPayPal(context, userId, product.ProductId, subscriptionType, custom.TransactionId);
        //            switch (errorCode)
        //            {
        //                case ErrorCodes.IsProcessedPayPalTransaction: errorMessage = "This transaction has already been processed."; break;
        //                case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
        //                case ErrorCodes.Success: errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", tx); break;
        //                default: errorMessage = "Unknown error."; break;
        //            }
        //            collection["errorCode"] = (int)errorCode;
        //            collection["errorMessage"] = errorMessage;
        //        }
        //    }
        //    else
        //    {
        //        collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
        //        collection["errorMessage"] = "Your session has expired. Please login again.";
        //    }

        //    ViewBag.errorCode = collection["errorCode"];
        //    ViewBag.errorMessage = collection["errorMessage"];

        //    return PartialView("_PayPalReturn");
        //}

        //[HttpPost]
        //public ActionResult PayViaPrepaidCard(int? id, FormCollection fc)
        //{
        //    if (id == null)
        //        return PartialView("_SubscribeErrorPartial");

        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    Ppc.ErrorCode errorCode = Ppc.ErrorCode.UnknownError;
        //    string errorMessage = MyUtility.getPpcError(errorCode);
        //    collection.Add("errorCode", (int)errorCode);
        //    collection.Add("errorMessage", errorMessage);

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var context = new IPTV2Entities();
        //        System.Guid userId = new System.Guid(User.Identity.Name);
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);

        //        Product product = context.Products.FirstOrDefault(p => p.ProductId == id);
        //        if (product == null)
        //            return PartialView("_SubscribeErrorPartial");
        //        SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);

        //        if (user != null)
        //        {
        //            string serial = fc["serialnumber"];
        //            string pin = fc["pin"];
        //            if (!String.IsNullOrEmpty(pin))
        //                pin = MyUtility.GetSHA1(pin);
        //            errorCode = PaymentHelper.PayViaPrepaidCard(context, userId, product.ProductId, subscriptionType, serial, pin);
        //            switch (errorCode)
        //            {
        //                case Ppc.ErrorCode.Success: errorMessage = "Congratulations! You have now bought this product."; break;
        //                default: errorMessage = MyUtility.getPpcError(errorCode); break;
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
                    #region   if user is not loggedin
                    if (!User.Identity.IsAuthenticated)
                    {
                        if (!String.IsNullOrEmpty(id))
                        {
                            if (String.Compare(id, "mayweather-vs-pacquiao-may-3", true) == 0)
                            {
                                HttpCookie pacMayCookie = new HttpCookie("redirect3178");
                                pacMayCookie.Expires = DateTime.Now.AddDays(1);
                                Response.Cookies.Add(pacMayCookie);
                                id = GlobalConfig.PacMaySubscribeCategoryId.ToString();
                            }

                            if (String.Compare(id, GlobalConfig.PacMaySubscribeCategoryId.ToString(), true) != 0)
                            {
                                TempData["LoginErrorMessage"] = "Please register or sign in to avail of this promo.";
                                TempData["RedirectUrl"] = Request.Url.PathAndQuery;

                                if (String.Compare(id, "lckbprea", true) == 0)
                                {
                                    HttpCookie preBlackCookie = new HttpCookie("redirectlckbprea");
                                    preBlackCookie.Expires = DateTime.Now.AddDays(1);
                                    Response.Cookies.Add(preBlackCookie);
                                }
                                else if (String.Compare(id, "Promo201410", true) == 0)
                                {
                                    HttpCookie promo2014010 = new HttpCookie("promo2014cok");
                                    promo2014010.Expires = DateTime.Now.AddDays(1);
                                    Response.Cookies.Add(promo2014010);
                                }
                                else if (String.Compare(id, "aintone", true) == 0)
                                {
                                    HttpCookie preBlackCookie = new HttpCookie("redirectaintone");
                                    preBlackCookie.Expires = DateTime.Now.AddDays(1);
                                    Response.Cookies.Add(preBlackCookie);
                                }

                                //check if id is integer
                                try
                                {
                                    int tempId = 0;
                                    if (Int32.TryParse(id, out tempId))
                                    {
                                        TempData["LoginErrorMessage"] = "Please sign in to purchase this product.";
                                        TempData["RedirectUrl"] = Request.Url.PathAndQuery;

                                        if (Request.Browser.IsMobileDevice)
                                            return RedirectToAction("MobileSignin", "User", new { ReturnUrl = Server.UrlEncode(Request.Url.PathAndQuery) });
                                        else
                                            return RedirectToAction("Login", "User", new { ReturnUrl = Server.UrlEncode(Request.Url.PathAndQuery) });
                                    }
                                }
                                catch (Exception) { }

                                if (Request.Browser.IsMobileDevice)
                                    return RedirectToAction("MobileSignin", "User");
                                else
                                    return RedirectToAction("Login", "User");
                            }
                        }
                        else
                        {
                            TempData["LoginErrorMessage"] = "Please register or sign in to subscribe.";
                            TempData["RedirectUrl"] = Request.Url.PathAndQuery;

                            if (Request.Browser.IsMobileDevice)
                                return RedirectToAction("MobileSignin", "User");
                            else
                                return RedirectToAction("Login", "User");
                        }
                    }
                    #endregion
                    #region user authenticated flow
                    else
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
                    
                    var premium          = MyUtility.StringToIntList(GlobalConfig.PremiumProductIds);
                    var lite            = MyUtility.StringToIntList(GlobalConfig.LiteProductIds);
                    var movie           = MyUtility.StringToIntList(GlobalConfig.MovieProductIds);
                    var litepromo       = MyUtility.StringToIntList(GlobalConfig.LitePromoProductIds);
                    var promo2014promo  = MyUtility.StringToIntList(GlobalConfig.Promo2014ProductIds);
                    var q22015promo  = MyUtility.StringToIntList(GlobalConfig.Q22015ProductId);

                    var productIds = premium;//productids
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(o => o.PackageId == GlobalConfig.serviceId);

                    bool IsAlaCarteProduct = true;
                    #region code for showing product based on id
                    if (!String.IsNullOrEmpty(id))
                    {
                        int pid;
                        #region if id is integer
                        if (int.TryParse(id, out pid)) // id is an integer, then it's a show, get ala carte only
                        {
                            var show = (Show)context.CategoryClasses.Find(pid);
                            #region check user is blocked on IP by his email
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
                            #endregion

                            try
                            {
                                #region show parent categories based on show category id
                                var ShowParentCategories = ContextHelper.GetShowParentCategories(show.CategoryId);
                                if (!String.IsNullOrEmpty(ShowParentCategories))
                                {
                                    ViewBag.ShowParentCategories = ShowParentCategories;
                                    if (!MyUtility.IsWhiteListed(String.Empty))
                                    {
                                        #region if not ip whitelisted
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
                                        #endregion
                                    }
                                }
                                #endregion
                            }
                            catch (Exception) { }

                            productIds = show.GetPackageProductIds(offering, CountryCode, RightsType.Online);//*
                            var showProductIds = show.GetShowProductIds(offering, CountryCode, RightsType.Online);//*
                            if (showProductIds != null)
                                productIds = productIds.Union(showProductIds);//*

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
                        #endregion
                        #region if id is not integer
                        else // get packages only
                        {
                            #region if id is Lite
                            if (String.Compare(id, "Lite", true) == 0)
                                productIds = lite;
                                #endregion
                            #region if id is Movie
                            else if (String.Compare(id, "Movie", true) == 0)
                                productIds = movie;
                            #endregion
                            #region if id is LitePromo
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
                            #endregion
                            #region if id is Promo201410
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
                            #endregion
                            #region if id is lckbprea
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
                            #endregion
                            #region if id is atra
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
                            #endregion
                            #region id id is aintone
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
                            #endregion
                        }
                        #endregion
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
                            #region not required according to Albin
                            bool IsXoomUser = false;
                            if (User.Identity.IsAuthenticated)
                            {
                                //XOOM 2 Promo
                                IsXoomUser = ContextHelper.IsXoomEligible(context, user);
                                if (IsXoomUser)
                                {
                                    var freeProductId = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.Xoom2FreeProductId);
                                    if (freeProductId != null)
                                        if (freeProductId is SubscriptionProduct)
                                        {
                                            freeProduct = (SubscriptionProduct)freeProductId;
                                            try
                                            {
                                                freeProductPrice = freeProductId.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, country.CurrencyCode, true) == 0);
                                                ViewBag.IsAPRBoxDisabled = true;
                                                ViewBag.PageDescription = "Choose a package and provide your credit card or PayPal details to finish your registration and get your 1st month FREE!";
                                                TempData["xoom"] = true;
                                            }
                                            catch (Exception) { }

                                        }
                                }
                            }
                            #endregion
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
                                            #region xoom user not required
                                            else if (IsXoomUser)
                                            {
                                                if (freeProductPrice != null && freeProduct != null)
                                                {
                                                    sItem.freeProduct = (SubscriptionProduct)freeProduct;
                                                    sItem.freeProductPrice = freeProductPrice;
                                                }
                                            }
                                            #endregion
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
                    #region
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
                    #endregion
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
            #endregion
            #region else for the above if
            else
            {
                List<SubscriptionProductA> productList = null;
                try
                {
                    var countryCode = MyUtility.GetCurrentCountryCodeOrDefault();
                    var context = new IPTV2Entities();
                    var country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, countryCode, true) == 0);
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(o => o.PackageId == GlobalConfig.serviceId);
                    var listOfPackagesInProductGroup = MyUtility.StringToIntList(GlobalConfig.PackagesInProductGroup);

                    if (!String.IsNullOrEmpty(id))
                    {
                        int pid = 0;
                        try
                        {
                            pid = Int32.Parse(id);
                        }
                        catch (Exception) { }
                        ViewBag.id = pid;
                        using (profiler.Step("Get Subscription (w/ parameter)"))
                        {
                            var show = (Show)context.CategoryClasses.Find(pid);

                            /******* CHECK IF SHOW IS VIEWABLE VIA USER'S COUNTRY CODE & COUNTRY CODE BASED ON IP ADDRESS *****/
                            if (!ContextHelper.IsCategoryViewableInUserCountry(show, countryCode))
                                return RedirectToAction("Index", "Home");
                            string CountryCodeBasedOnIpAddress = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                            if (!ContextHelper.IsCategoryViewableInUserCountry(show, CountryCodeBasedOnIpAddress))
                                return RedirectToAction("Index", "Home");

                            ViewBag.ShowName = show.Description;
                            SortedSet<Int32> productIds = new SortedSet<int>();
                            var packageProductIds = show.GetPackageProductIds(offering, countryCode, RightsType.Online);
                            if (packageProductIds != null)
                                productIds.UnionWith(packageProductIds);
                            var showProductIds = show.GetShowProductIds(offering, countryCode, RightsType.Online);
                            if (showProductIds != null)
                                productIds.UnionWith(showProductIds);
                            ViewBag.PackageProductIds = packageProductIds;
                            ViewBag.ShowProductIds = showProductIds;
                            var products = context.Products.Where(p => productIds.Contains(p.ProductId));
                            List<PackageContentSummary> contentSummary = new List<PackageContentSummary>();
                            if (products != null)
                            {
                                productList = new List<SubscriptionProductA>();
                                foreach (var product in products)
                                {
                                    if (product.IsForSale && product.StatusId == GlobalConfig.Visible)
                                    {
                                        if (product is SubscriptionProduct)
                                        {
                                            if (product is PackageSubscriptionProduct)
                                            {
                                                var pkgSubscriptionProduct = (PackageSubscriptionProduct)product;
                                                var package = pkgSubscriptionProduct.Packages.FirstOrDefault();
                                                var counter = 0;
                                                var item = new SubscriptionProductA() { product = (SubscriptionProduct)product, productGroup = pkgSubscriptionProduct.ProductGroup, isPackage = true };

                                                //SET DEFAULT PRODUCT ID VIA PARAM                                            
                                                if (PackageOption != null)
                                                {
                                                    if (listOfPackagesInProductGroup.Contains((int)PackageOption))
                                                        if (pkgSubscriptionProduct.ProductGroupId == PackageOption)
                                                            if (ProductOption != null)
                                                                item.productGroup.DefaultProductId = ProductOption;
                                                }

                                                if (package != null)
                                                {
                                                    // make sure that this doesnt repeat. save data in list
                                                    if (!contentSummary.Select(s => s.PackageId).Contains(package.PackageId))
                                                    {
                                                        foreach (var cat in package.Package.Categories)
                                                        {
                                                            if (cat.Category is Category)
                                                                counter += service.GetAllOnlineShowIds(countryCode, (Category)cat.Category).Count();
                                                        }
                                                        contentSummary.Add(new PackageContentSummary() { PackageId = package.PackageId, ContentCount = counter });
                                                    }
                                                    else
                                                        counter = contentSummary.FirstOrDefault(s => s.PackageId == package.PackageId).ContentCount;
                                                    item.contentCount = counter;
                                                    //for description of content
                                                    item.contentDescription = ContentDescriptionFlooring(counter > 1 ? counter - 1 : counter, true);
                                                    item.package = (Package)package.Package;
                                                    item.ListOfDescription = ContextHelper.GetPackageFeatures(countryCode, package);
                                                    if (item.productGroup.ProductSubscriptionTypeId != null)
                                                        item.ShowDescription = show.Blurb;
                                                }

                                                try
                                                {
                                                    item.productPrice = pkgSubscriptionProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == country.CurrencyCode);
                                                    productList.Add(item);
                                                }
                                                catch (Exception) { ViewBag.ProductPriceError = true; }

                                            }
                                            else if (product is ShowSubscriptionProduct)
                                            {
                                                var shwSubscriptionProduct = (ShowSubscriptionProduct)product;
                                                var category = shwSubscriptionProduct.Categories.FirstOrDefault();
                                                var item = new SubscriptionProductA() { product = (SubscriptionProduct)product, productGroup = shwSubscriptionProduct.ProductGroup, contentCount = 1 };

                                                if (category != null)
                                                {
                                                    item.show = category.Show;
                                                    item.ShowDescription = category.Show.Blurb;
                                                }

                                                try
                                                {
                                                    item.productPrice = shwSubscriptionProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == country.CurrencyCode);
                                                    productList.Add(item);
                                                }
                                                catch (Exception) { ViewBag.ProductPriceError = true; }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        using (profiler.Step("Get Subscription (no parameter)"))
                        {
                            var groups = context.ProductGroups.Where(p => listOfPackagesInProductGroup.Contains(p.ProductGroupId));
                            if (groups != null)
                            {
                                List<PackageContentSummary> contentSummary = new List<PackageContentSummary>();
                                productList = new List<SubscriptionProductA>();
                                foreach (var grp in groups)
                                {
                                    foreach (var product in grp.SubscriptionProducts)
                                    {
                                        if (product.IsAllowed(countryCode))
                                        {
                                            if (product.IsForSale && product.StatusId == GlobalConfig.Visible)
                                            {
                                                if (product is PackageSubscriptionProduct)
                                                {
                                                    var pkgSubscriptionProduct = (PackageSubscriptionProduct)product;
                                                    var package = pkgSubscriptionProduct.Packages.FirstOrDefault();
                                                    var counter = 0;
                                                    var item = new SubscriptionProductA() { product = product, productGroup = pkgSubscriptionProduct.ProductGroup, isPackage = true };
                                                    //SET DEFAULT PRODUCT ID VIA PARAM                                            
                                                    if (PackageOption != null)
                                                    {
                                                        if (listOfPackagesInProductGroup.Contains((int)PackageOption))
                                                            if (pkgSubscriptionProduct.ProductGroupId == PackageOption)
                                                                if (ProductOption != null)
                                                                    item.productGroup.DefaultProductId = ProductOption;
                                                    }
                                                    if (package != null)
                                                    {
                                                        // make sure that this doesnt repeat. save data in list
                                                        if (!contentSummary.Select(s => s.PackageId).Contains(package.PackageId))
                                                        {
                                                            foreach (var cat in package.Package.Categories)
                                                            {
                                                                if (cat.Category is Category)
                                                                    counter += service.GetAllOnlineShowIds(countryCode, (Category)cat.Category).Count();
                                                            }
                                                            contentSummary.Add(new PackageContentSummary() { PackageId = package.PackageId, ContentCount = counter });
                                                        }
                                                        else
                                                            counter = contentSummary.FirstOrDefault(s => s.PackageId == package.PackageId).ContentCount;
                                                        item.contentCount = counter;
                                                        item.contentDescription = ContentDescriptionFlooring(counter, true);
                                                        item.package = (Package)package.Package;
                                                        item.ListOfDescription = ContextHelper.GetPackageFeatures(countryCode, package);
                                                    }
                                                    try
                                                    {
                                                        item.productPrice = pkgSubscriptionProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == country.CurrencyCode);
                                                        productList.Add(item);
                                                    }
                                                    catch (Exception) { ViewBag.ProductPriceError = true; }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // SET DEFAULT PACKAGE. Position Swap!                            
                    if (productList != null)
                    {
                        if (productList.Count() > 0)
                        {
                            if (PackageOption != null)
                            {
                                int index = 0;
                                if (listOfPackagesInProductGroup.Contains((int)PackageOption))
                                    index = productList.FindIndex(x => x.productGroup.ProductGroupId == PackageOption);
                                else
                                    index = productList.FindIndex(x => !listOfPackagesInProductGroup.Contains(x.productGroup.ProductGroupId));
                                if (index > 0)
                                {
                                    var item = productList[index];
                                    productList[index] = productList[0];
                                    productList[0] = item;
                                }
                            }
                        }
                    }

                    //Only return if productList is not empty and count > 0
                    if (productList != null)
                        if (productList.Count() > 0)
                            return View("Details5", productList);

                }
                catch (Exception e) { MyUtility.LogException(e); }
                return RedirectToAction("Index", "Home");
            }
            #endregion
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

        public JsonResult GetCategories(string list, int type = 0)
        {
            ArrayList slist = new ArrayList();
            if (!String.IsNullOrEmpty(list))
            {
                if (Request.IsAjaxRequest() || Request.IsLocal)
                {
                    try
                    {
                        var ids = MyUtility.StringToIntList(list);
                        var registDt = DateTime.Now;
                        var context = new IPTV2Entities();
                        if (type == 0) // meaning it is a package
                        {
                            var all = new SelectListItem() { Text = "ALL", Value = list };
                            slist.Add(all);
                            var categories = context.PackageCategories.Where(p => ids.Contains(p.PackageId)).Select(p => p.Category);
                            if (categories != null)
                            {
                                foreach (var category in categories)
                                {
                                    if (category.StatusId == GlobalConfig.Visible && category.StartDate < registDt && category.EndDate > registDt)
                                    {
                                        if (categories.Count() > 1)
                                        {
                                            var item = new SelectListItem() { Text = category.Description.ToUpper(), Value = category.CategoryId.ToString() };
                                            slist.Add(item);
                                        }
                                        var subCategories = category.SubCategories.Where(s => s.StatusId == GlobalConfig.Visible && s.StartDate < registDt && s.EndDate > registDt);
                                        if (subCategories.Count() > 0)
                                        {
                                            foreach (var cat in subCategories)
                                            {
                                                var catItem = new SelectListItem() { Text = String.Format("- {0}", cat.Description), Value = cat.CategoryId.ToString() };
                                                slist.Add(catItem);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (type == 1) //meaning its a show/ala carte
                        {
                            var none = new SelectListItem() { Text = "--", Value = list };
                            slist.Add(none);
                        }
                    }
                    catch (Exception e) { MyUtility.LogException(e); }
                }
            }
            return Json(slist, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(VaryByParam = "id;currentCategoryId;type", Duration = 300)]
        public JsonResult GetShows(string id, int? currentCategoryId, int type = 0)
        {
            List<ShowListDisplay> list = null;
            var cache = DataCache.Cache;
            string cacheKey = "SUBSGSWS:O:" + id + ";T:" + type + ";CCID:" + currentCategoryId;
            list = (List<ShowListDisplay>)cache[cacheKey];
            if (list == null)
            {
                list = new List<ShowListDisplay>();
                try
                {
                    var registDt = DateTime.Now;
                    var countryCode = MyUtility.GetCurrentCountryCodeOrDefault();
                    var context = new IPTV2Entities();
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(o => o.PackageId == GlobalConfig.serviceId);

                    if (type == 0) // id is a package id, ALL
                    {
                        if (!String.IsNullOrEmpty(id))
                        {
                            var packageIds = MyUtility.StringToIntList(id);
                            var pkgType = context.PackageTypes.Where(p => packageIds.Contains(p.PackageId));
                            if (pkgType != null)
                            {
                                foreach (var pkg in pkgType)
                                {
                                    if (pkg.Categories != null)
                                    {
                                        foreach (var c in pkg.Categories)
                                        {
                                            var catS = (Category)context.CategoryClasses.Find(c.CategoryId);
                                            if (catS.StatusId == GlobalConfig.Visible && catS.StartDate < registDt && catS.EndDate > registDt)
                                            {
                                                var subcategories = catS.SubCategories.Where(s => s.StatusId == GlobalConfig.Visible && s.StartDate < registDt && s.EndDate > registDt);
                                                if (subcategories.Count() > 0)
                                                {
                                                    foreach (var s in subcategories)
                                                    {
                                                        var showIds = service.GetAllOnlineShowIds(countryCode, s);
                                                        var categories = context.CategoryClasses.Where(cc => showIds.Contains(cc.CategoryId));
                                                        if (categories != null)
                                                        {
                                                            foreach (var cat in categories)
                                                            {
                                                                if (cat.StatusId == GlobalConfig.Visible && cat.StartDate < registDt && cat.EndDate > registDt && cat is Show)
                                                                {
                                                                    string img = String.IsNullOrEmpty(cat.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, cat.CategoryId, cat.ImagePoster);
                                                                    var item = new ShowListDisplay()
                                                                    {
                                                                        CategoryId = cat.CategoryId,
                                                                        Name = MyUtility.Ellipsis(cat.Description, 35),
                                                                        ImgUrl = img,
                                                                        ParentCategoryId = s.CategoryId,
                                                                        ParentCategoryName = s.Description
                                                                    };
                                                                    list.Add(item);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var showIds = service.GetAllOnlineShowIds(countryCode, catS);
                                                    var categories = context.CategoryClasses.Where(cc => showIds.Contains(cc.CategoryId));
                                                    if (categories != null)
                                                    {
                                                        foreach (var cat in categories)
                                                        {
                                                            if (cat.StatusId == GlobalConfig.Visible && cat.StartDate < registDt && cat.EndDate > registDt && cat is Show)
                                                            {
                                                                string img = String.IsNullOrEmpty(cat.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, cat.CategoryId, cat.ImagePoster);
                                                                var item = new ShowListDisplay()
                                                                {
                                                                    CategoryId = cat.CategoryId,
                                                                    Name = MyUtility.Ellipsis(cat.Description, 35),
                                                                    ImgUrl = img,
                                                                    ParentCategoryId = catS.CategoryId,
                                                                    ParentCategoryName = catS.Description
                                                                };
                                                                list.Add(item);
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (type == 1) // id is a category
                    {
                        if (!String.IsNullOrEmpty(id))
                        {
                            int cId = 0;
                            if (Int32.TryParse(id, out cId))
                            {
                                if (cId > 0)
                                {
                                    var category = (Category)context.CategoryClasses.Find(cId);
                                    if (category.StatusId == GlobalConfig.Visible && category.StartDate < registDt && category.EndDate > registDt)
                                    {
                                        var subcategories = category.SubCategories.Where(s => s.StatusId == GlobalConfig.Visible && s.StartDate < registDt && s.EndDate > registDt);
                                        if (subcategories.Count() > 0)
                                        {
                                            foreach (var s in subcategories)
                                            {
                                                var showIds = service.GetAllOnlineShowIds(countryCode, s);
                                                var categories = context.CategoryClasses.Where(cc => showIds.Contains(cc.CategoryId));
                                                if (categories != null)
                                                {
                                                    foreach (var cat in categories)
                                                    {
                                                        if (cat.StatusId == GlobalConfig.Visible && cat.StartDate < registDt && cat.EndDate > registDt && cat is Show)
                                                        {
                                                            string img = String.IsNullOrEmpty(cat.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, cat.CategoryId, cat.ImagePoster);
                                                            var item = new ShowListDisplay()
                                                            {
                                                                CategoryId = cat.CategoryId,
                                                                Name = MyUtility.Ellipsis(cat.Description, 35),
                                                                ImgUrl = img,
                                                                ParentCategoryId = s.CategoryId,
                                                                ParentCategoryName = s.Description
                                                            };
                                                            list.Add(item);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var categoryS = (Category)context.CategoryClasses.Find(cId);
                                            var showIds = service.GetAllOnlineShowIds(countryCode, categoryS);
                                            var categories = context.CategoryClasses.Where(cc => showIds.Contains(cc.CategoryId));
                                            if (categories != null)
                                            {
                                                foreach (var cat in categories)
                                                {
                                                    if (cat.StatusId == GlobalConfig.Visible && cat.StartDate < registDt && cat.EndDate > registDt && cat is Show)
                                                    {
                                                        string img = String.IsNullOrEmpty(cat.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, cat.CategoryId, cat.ImagePoster);
                                                        var item = new ShowListDisplay()
                                                        {
                                                            CategoryId = cat.CategoryId,
                                                            Name = MyUtility.Ellipsis(cat.Description, 35),
                                                            ImgUrl = img,
                                                            ParentCategoryId = categoryS.CategoryId,
                                                            ParentCategoryName = categoryS.Description
                                                        };
                                                        list.Add(item);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else if (type == 2) //ala carte
                    {
                        if (!String.IsNullOrEmpty(id))
                        {
                            int cId = 0;
                            if (Int32.TryParse(id, out cId))
                            {
                                if (cId > 0)
                                {
                                    var sShow = context.CategoryClasses.Find(cId);
                                    if (sShow.StatusId == GlobalConfig.Visible && sShow.StartDate < registDt && sShow.EndDate > registDt && sShow is Show)
                                    {
                                        string img = String.IsNullOrEmpty(sShow.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, sShow.CategoryId, sShow.ImagePoster);
                                        var item = new ShowListDisplay()
                                        {
                                            CategoryId = sShow.CategoryId,
                                            Name = MyUtility.Ellipsis(sShow.Description, 35),
                                            ImgUrl = img,
                                            ParentCategoryId = 0,
                                            ParentCategoryName = ""
                                        };
                                        list.Add(item);
                                    }
                                }
                            }
                        }
                    }

                    //position swap!
                    if (currentCategoryId != null)
                    {
                        var index = list.FindIndex(x => x.CategoryId == currentCategoryId);
                        var item = list[index];
                        list[index] = list[0];
                        list[0] = item;
                    }

                    var cacheDuration = new TimeSpan(0, GlobalConfig.PackageAndProductCacheDuration, 0);
                    //cache.Put(cacheKey, list, cacheDuration);
                }
                catch (Exception e) { MyUtility.LogException(e); }
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FreeTrial(int? id, int? PackageOption, int? ProductOption)
        {
            var profiler = MiniProfiler.Current;
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Details");
                var registDt = DateTime.Now;
                var countryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var context = new IPTV2Entities();
                var UserId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                if (user != null)
                    countryCode = user.CountryCode;
                int freeTrialPackageId = 0; int NumberOfDaysLeft = 0;
                var list = MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds);
                using (profiler.Step("Get free trial package from user's entitlements"))
                {
                    var package = user.PackageEntitlements.FirstOrDefault(p => list.Contains(p.PackageId) && p.EndDate > registDt);
                    if (package != null)
                    {
                        freeTrialPackageId = package.PackageId;
                        NumberOfDaysLeft = Convert.ToInt32(package.EndDate.Subtract((DateTime)registDt).TotalDays);
                        ViewBag.ListOfDescription = ContextHelper.GetPackageFeaturesViaPackage(countryCode, package.Package);
                    }
                }
                ViewBag.NumberOfDaysLeft = NumberOfDaysLeft;
                ViewBag.FreeTrialPackageId = freeTrialPackageId;
                return View();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Details");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _PrepaidCard(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
             {
                 StatusCode = (int)ErrorCodes.UnknownError,
                 StatusMessage = String.Empty,
                 info = "PrepaidCard",
                 TransactionType = "Subscription"
             };

            string url = Url.Action("Details", "Subscribe", new { id = Url.RequestContext.RouteData.Values["id"] }).ToString();
            try
            {
                DateTime registDt = DateTime.Now;
                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;

                foreach (var x in tmpCollection)
                {
                    if (String.IsNullOrEmpty(x.Value))
                    {
                        isMissingRequiredFields = true;
                        break;
                    }
                }

                if (!isMissingRequiredFields) // process form
                {
                    if (GlobalConfig.IsPpcPaymentModeEnabled)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            int pid = Convert.ToInt32(fc["pid"]); //Get ID
                            string serial = fc["serial"];
                            string pin = fc["pin"];
                            //remove whitespaces
                            serial = serial.Replace(" ", String.Empty);
                            pin = pin.Replace(" ", String.Empty);
                            if (!Request.IsLocal)
                                pin = MyUtility.GetSHA1(pin); // encrypt

                            var context = new IPTV2Entities();
                            Product product = context.Products.FirstOrDefault(p => p.ProductId == pid);
                            if (product == null)
                                ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNull);
                            else if (product.StatusId != GlobalConfig.Visible)
                                ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                            //else if (!product.IsForSale)
                            //    ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                            else
                            {
                                System.Guid userId = new System.Guid(User.Identity.Name);
                                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                                if (user != null)
                                {
                                    Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                                        ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                    else if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                                        ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                    else
                                    {
                                        if (GlobalConfig.IsRecurringBillingEnabled)
                                        {
                                            var checkIfEnrolled = ContextHelper.CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                                            if (checkIfEnrolled.value)
                                            {
                                                ReturnCode.StatusMessage = "You are currently automatically renewing a similar subscription product through credit card.";
                                                TempData["ErrorMessage"] = ReturnCode;
                                                return Redirect(url);
                                            }
                                        }

                                        SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                                        string productName = product.Description;
                                        if (subscriptionType == SubscriptionProductType.Package || subscriptionType == SubscriptionProductType.Show)
                                        {
                                            if (!product.IsAllowed(user.CountryCode))
                                                ReturnCode.StatusMessage = String.Format("Sorry! The ({0}) subscription is not available in your country.", productName);
                                            //else if (!ContextHelper.IsProductPurchaseable(context, product, user, offering))
                                            //{
                                            //    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, user.UserId, offering);
                                            //    ReturnCode.StatusMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                            //}
                                            else
                                            {
                                                ErrorResponse response = PaymentHelper.PayViaPrepaidCard(context, userId, product.ProductId, subscriptionType, serial, pin, user.UserId, null);
                                                var StatusCode = (Ppc.ErrorCode)response.Code;
                                                switch (StatusCode)
                                                {
                                                    case Ppc.ErrorCode.Success:
                                                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                        ReturnCode.StatusHeader = "You have successfully purchased a product!";
                                                        ReturnCode.StatusMessage = String.Format("Congratulations! You have successfully subscribed to {0}.", product.Description);
                                                        ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies!";
                                                        ReturnCode.info7 = user.EMail;
                                                        //GA ECommerce
                                                        try
                                                        {
                                                            ReturnCode.info1 = String.Format("{0}", response.transaction.TransactionId); //transaction id ga ecommerce
                                                            ReturnCode.info2 = String.Format("{0}", response.product.ProductId); //sku ga ecommerce
                                                            ReturnCode.info3 = response.product.Description; //name ga ecommerce
                                                            ReturnCode.info4 = String.Format("{0}", response.price.Amount.ToString("F")); //price ga ecommerce
                                                            ReturnCode.info5 = response.price.CurrencyCode; //currency ga ecommerce
                                                            ReturnCode.info6 = response.ProductType; //product type ga ecommerce
                                                        }
                                                        catch (Exception) { }

                                                        try
                                                        {
                                                            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("air"))
                                                            {
                                                                HttpCookie airCookie = new HttpCookie("air");
                                                                airCookie.Expires = DateTime.Now.AddDays(-1);
                                                                Response.Cookies.Add(airCookie);
                                                            }
                                                        }
                                                        catch (Exception) { }

                                                        TempData["ErrorMessage"] = ReturnCode;
                                                        return RedirectToAction("Index", "Home"); // successful payment

                                                    default: ReturnCode.StatusMessage = MyUtility.GetPpcErrorMessages(StatusCode); break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                    ReturnCode.StatusMessage = "User does not exist.";
                            }
                        }
                        else
                            ReturnCode.StatusMessage = "Your session has already expired. Please login again.";
                    }
                    else
                        ReturnCode.StatusMessage = "Prepaid Card/ePIN payment is currenty disabled.";
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                TempData["ErrorMessage"] = ReturnCode;

                url = Request.UrlReferrer.AbsolutePath;
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusMessage = e.Message;
                TempData["ErrorMessage"] = ReturnCode;
            }
            return Redirect(url);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _CreditCard(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
                 {
                     StatusCode = (int)ErrorCodes.UnknownError,
                     StatusMessage = String.Empty,
                     info = "CreditCard",
                     TransactionType = "Subscription"
                 };

            string url = Url.Action("Details", "Subscribe", new { id = Url.RequestContext.RouteData.Values["id"] }).ToString();
            try
            {
                DateTime registDt = DateTime.Now;
                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;

                foreach (var x in tmpCollection)
                {
                    if (String.IsNullOrEmpty(x.Value))
                    {
                        isMissingRequiredFields = true;
                        break;
                    }
                }

                if (!isMissingRequiredFields) // process form
                {
                    if (GlobalConfig.IsCreditCardPaymentModeEnabled)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            int pid = Convert.ToInt32(fc["pid"]); //Get ID                         
                            int cctype = Convert.ToInt32(fc["CreditCard"]);
                            string name = fc["cardholdername"];
                            string cardnumber = fc["cardnumber"];
                            string securitycode = fc["securitycode"];
                            int expirymonth = Convert.ToInt32(fc["ExpiryMonth"]);
                            int expiryyear = Convert.ToInt32(fc["ExpiryYear"]);
                            string address = fc["address"];
                            string city = fc["city"];
                            string zip = fc["zip"];
                            var recur = fc["recur"];
                            var f_apr = fc["f_apr"];
                            if (!String.IsNullOrEmpty(f_apr))
                                if (String.Compare(f_apr, "True", true) == 0)
                                    recur = f_apr;

                            var context = new IPTV2Entities();
                            Product product = context.Products.FirstOrDefault(p => p.ProductId == pid);
                            if (product == null)
                                ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNull);
                            else if (product.StatusId != GlobalConfig.Visible)
                                ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                            else if (!product.IsForSale)
                                ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                            else
                            {
                                System.Guid userId = new System.Guid(User.Identity.Name);
                                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                                if (user != null)
                                {
                                    Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                                    if (offering != null)
                                    {
                                        if (user.HasPendingGomsChangeCountryTransaction(offering))
                                            ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                        else if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                                            ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                        else
                                        {
                                            if (GlobalConfig.IsRecurringBillingEnabled)
                                            {
                                                var checkIfEnrolled = ContextHelper.CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                                                if (checkIfEnrolled.value)
                                                {
                                                    ReturnCode.StatusMessage = "You are currently automatically renewing a similar subscription product through credit card.";
                                                    TempData["ErrorMessage"] = ReturnCode;
                                                    return Redirect(url);
                                                }
                                            }

                                            SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                                            string productName = product.Description;
                                            if (subscriptionType == SubscriptionProductType.Package || subscriptionType == SubscriptionProductType.Show)
                                            {
                                                if (!product.IsAllowed(user.CountryCode))
                                                    ReturnCode.StatusMessage = String.Format("Sorry! The ({0}) product is not available in your country.", productName);
                                                //else if (!ContextHelper.IsProductPurchaseable(context, product, user, offering))
                                                //{
                                                //    //string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, user.UserId, offering);
                                                //    string CurrentProductName = product.Description;
                                                //    ReturnCode.StatusMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                                //}
                                                else
                                                {
                                                    CreditCardInfo info = new CreditCardInfo()
                                                    {
                                                        CardType = (IPTV2_Model.CreditCardType)cctype,
                                                        Name = name,
                                                        Number = cardnumber,
                                                        CardSecurityCode = securitycode,
                                                        ExpiryMonth = expirymonth,
                                                        ExpiryYear = expiryyear,
                                                        PostalCode = zip,
                                                        StreetAddress = String.Format("{0}{1}", address, String.IsNullOrEmpty(city) ? "" : (", " + city))
                                                    };

                                                    ErrorResponse response;
                                                    if (!String.IsNullOrEmpty(recur))
                                                    {
                                                        if (subscriptionType == SubscriptionProductType.Package)
                                                        {
                                                            if (recur.ToLowerInvariant().Contains("on") || recur.ToLowerInvariant().Contains("true"))
                                                            {
                                                                if (ContextHelper.IsXoomEligible(context, user))
                                                                    response = PaymentHelper.PayViaCreditCardWithRecurringBilling_ValidateOnly(context, userId, info, product.ProductId, subscriptionType, user.UserId, null, GlobalConfig.Xoom2FreeProductId);
                                                                else
                                                                    response = PaymentHelper.PayViaCreditCardWithRecurringBilling(context, userId, info, product.ProductId, subscriptionType, user.UserId, null);
                                                            }
                                                            else
                                                                response = PaymentHelper.PayViaCreditCard2(context, userId, info, product.ProductId, subscriptionType, user.UserId, null);
                                                        }
                                                        else
                                                            response = PaymentHelper.PayViaCreditCard2(context, userId, info, product.ProductId, subscriptionType, user.UserId, null);
                                                    }
                                                    else
                                                        response = PaymentHelper.PayViaCreditCard2(context, userId, info, product.ProductId, subscriptionType, user.UserId, null);

                                                    string ErrorMessage = String.Empty;
                                                    switch (response.Code)
                                                    {
                                                        case (int)ErrorCodes.IsProductIdInvalidPpc: ErrorMessage = "Please use proper card/ePIN for this product."; break;
                                                        case (int)ErrorCodes.IsReloadPpc: ErrorMessage = "Card/ePIN is invalid. Type is for reloading wallet."; break;
                                                        case (int)ErrorCodes.IsInvalidCombinationPpc: ErrorMessage = "Invalid serial/pin combination."; break;
                                                        case (int)ErrorCodes.IsExpiredPpc: ErrorMessage = "Prepaid Card/ePIN is expired."; break;
                                                        case (int)ErrorCodes.IsInvalidPpc: ErrorMessage = "Serial does not exist."; break;
                                                        case (int)ErrorCodes.IsUsedPpc: ErrorMessage = "Prepaid Card/ePIN is already used."; break;
                                                        case (int)ErrorCodes.IsNotValidInCountryPpc: ErrorMessage = "Prepaid Card/ePIN not valid in your country."; break;
                                                        case (int)ErrorCodes.CreditCardHasExpired: ErrorMessage = "Your credit card has already expired."; break;
                                                        case (int)ErrorCodes.IsNotValidAmountPpc: ErrorMessage = "Prepaid Card/ePIN credits not enough to buy this product."; break;
                                                        case (int)ErrorCodes.Success:
                                                            {
                                                                string CcStatusMessage = String.Empty;
                                                                if (!String.IsNullOrEmpty(response.CCEnrollmentStatusMessage))
                                                                    CcStatusMessage = "Auto-renewal was not successful. Please contact Customer Support.";

                                                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                                ReturnCode.StatusHeader = "You have successfully purchased a product!";
                                                                ReturnCode.StatusMessage = String.Format("Congratulations! You are now subscribed to {0}. {1}", product.Description, CcStatusMessage);
                                                                ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies!";
                                                                ReturnCode.info7 = user.EMail;
                                                                //Project Black
                                                                PaymentHelper.logProjectBlackUserPromo(context, userId, pid);
                                                                //Xoom2
                                                                if (PaymentHelper.logUserPromo(context, userId, GlobalConfig.Xoom2PromoId))
                                                                {
                                                                    ReturnCode.StatusHeader = "You have successfully registered and subscribed to a package!";
                                                                    ReturnCode.StatusMessage = String.Format("Congratulations! You claimed your FREE 1st month and subscribed to {0}", product.Description);
                                                                }
                                                                //GA ECommerce
                                                                try
                                                                {
                                                                    ReturnCode.info1 = String.Format("{0}", response.transaction.TransactionId); //transaction id ga ecommerce
                                                                    ReturnCode.info2 = String.Format("{0}", response.product.ProductId); //sku ga ecommerce
                                                                    ReturnCode.info3 = response.product.Description; //name ga ecommerce
                                                                    ReturnCode.info4 = String.Format("{0}", response.price.Amount.ToString("F")); //price ga ecommerce
                                                                    ReturnCode.info5 = response.price.CurrencyCode; //currency ga ecommerce
                                                                    ReturnCode.info6 = response.ProductType; //product type ga ecommerce
                                                                }
                                                                catch (Exception) { }

                                                                try
                                                                {
                                                                    if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("air"))
                                                                    {
                                                                        HttpCookie airCookie = new HttpCookie("air");
                                                                        airCookie.Expires = DateTime.Now.AddDays(-1);
                                                                        Response.Cookies.Add(airCookie);
                                                                    }
                                                                }
                                                                catch (Exception) { }

                                                                TempData["ErrorMessage"] = ReturnCode;
                                                                try
                                                                {
                                                                    var pacmayproductIdsForRedirect = MyUtility.StringToIntList(GlobalConfig.PacMayProductIdsForRedirect);
                                                                    if (pacmayproductIdsForRedirect.Contains(pid))
                                                                        TempData["IsPacMayPurchase"] = true;
                                                                }
                                                                catch (Exception) { }

                                                                return RedirectToAction("Index", "Home"); // successful payment
                                                            }
                                                        default: ErrorMessage = response.Message; break;
                                                    }
                                                    ReturnCode.StatusCode = response.Code;
                                                    ReturnCode.StatusMessage = ErrorMessage;
                                                }
                                            }
                                            else
                                                ReturnCode.StatusMessage = "This type of product is not for sale.";
                                        }
                                    }
                                    else
                                        ReturnCode.StatusMessage = "Service not found. Please contact support.";
                                }
                                else
                                    ReturnCode.StatusMessage = "User does not exist.";
                            }
                        }
                        else
                            ReturnCode.StatusMessage = "Your session has already expired. Please login again.";
                    }
                    else
                        ReturnCode.StatusMessage = "Credit card payment is currenty disabled.";
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                TempData["ErrorMessage"] = ReturnCode;

                url = Request.UrlReferrer.AbsolutePath;
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusMessage = e.Message;
                TempData["ErrorMessage"] = ReturnCode;
            }
            return Redirect(url);

        }

        [HttpPost]
        public JsonResult CheckBalance(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "CreditCard"
            };

            try
            {
                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusMessage = "The request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                DateTime registDt = DateTime.Now;
                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;

                foreach (var x in tmpCollection)
                {
                    if (String.IsNullOrEmpty(x.Value))
                    {
                        isMissingRequiredFields = true;
                        break;
                    }
                }

                if (!isMissingRequiredFields) // process form
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        int productId = Convert.ToInt32(fc["pid"]);
                        var UserId = new Guid(User.Identity.Name);

                        var context = new IPTV2Entities();
                        var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            var product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                            if (product != null)
                            {
                                var userWallet = user.UserWallets.FirstOrDefault(u => String.Compare(u.Currency, user.Country.CurrencyCode, true) == 0 && u.IsActive == true);
                                if (userWallet != null)
                                {
                                    var UserCurrencyCode = user.Country.CurrencyCode;
                                    var productPrice = product.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, UserCurrencyCode, true) == 0);
                                    if (productPrice == null)
                                        productPrice = product.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, GlobalConfig.DefaultCurrency, true) == 0);
                                    ViewBag.ProductPrice = productPrice;
                                    if (userWallet.Balance >= productPrice.Amount)
                                    {
                                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                        ReturnCode.HtmlUri = Url.Action("_EWallet", "Subscribe").ToString();
                                    }
                                    else
                                        ReturnCode.HtmlUri = Url.Action("Index", "Load").ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _EWallet(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "EWallet",
                TransactionType = "Subscription"
            };

            string url = Url.Action("Details", "Subscribe", new { id = Url.RequestContext.RouteData.Values["id"] }).ToString();
            try
            {
                DateTime registDt = DateTime.Now;
                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;

                foreach (var x in tmpCollection)
                {
                    if (String.IsNullOrEmpty(x.Value))
                    {
                        isMissingRequiredFields = true;
                        break;
                    }
                }

                if (!isMissingRequiredFields) // process form
                {
                    if (GlobalConfig.IsEWalletPaymentModeEnabled)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            int pid = Convert.ToInt32(fc["pid"]); //Get ID                            
                            var context = new IPTV2Entities();
                            Product product = context.Products.FirstOrDefault(p => p.ProductId == pid);
                            if (product == null)
                                ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNull);
                            else if (product.StatusId != GlobalConfig.Visible)
                                ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                            else if (!product.IsForSale)
                                ReturnCode.StatusMessage = MyUtility.getErrorMessage(ErrorCodes.ProductIsNotPurchaseable);
                            else
                            {
                                System.Guid userId = new System.Guid(User.Identity.Name);
                                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                                if (user != null)
                                {
                                    Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                                        ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                    else if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                                        ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                    else
                                    {
                                        if (GlobalConfig.IsRecurringBillingEnabled)
                                        {
                                            var checkIfEnrolled = ContextHelper.CheckIfUserIsEnrolledToSameRecurringProductGroup(context, offering, user, product);
                                            if (checkIfEnrolled.value)
                                            {
                                                ReturnCode.StatusMessage = "You are currently automatically renewing a similar subscription product through credit card.";
                                                TempData["ErrorMessage"] = ReturnCode;
                                                return Redirect(url);
                                            }
                                        }

                                        SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                                        string productName = product.Description;
                                        if (subscriptionType == SubscriptionProductType.Package || subscriptionType == SubscriptionProductType.Show)
                                        {
                                            if (!product.IsAllowed(user.CountryCode))
                                                ReturnCode.StatusMessage = String.Format("Sorry! The ({0}) subscription is not available in your country.", productName);
                                            //else if (!ContextHelper.IsProductPurchaseable(context, product, user, offering))
                                            //{
                                            //    string CurrentProductName = ContextHelper.GetCurrentSubscribeProduct(context, product, user.UserId, offering);
                                            //    ReturnCode.StatusMessage = String.Format("Sorry! You are already subscribed to {0}. You can no longer purchase {1}.", CurrentProductName, productName);
                                            //}
                                            else
                                            {
                                                ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                                                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                                                ErrorCodes StatusCode = ErrorCodes.UnknownError;
                                                if (wallet.Balance >= priceOfProduct.Amount)
                                                {
                                                    ErrorResponse response = PaymentHelper.PayViaWallet(context, userId, product.ProductId, subscriptionType, userId, null);
                                                    StatusCode = (ErrorCodes)response.Code;
                                                    ReturnCode.StatusCode = (int)StatusCode;
                                                    if (StatusCode == ErrorCodes.Success)
                                                    {
                                                        string balance = String.Format("Your new wallet balance is {0} {1}.", user.Country.CurrencyCode, wallet.Balance.ToString("F"));
                                                        ReturnCode.StatusHeader = "You have successfully purchased a product!";
                                                        ReturnCode.StatusMessage = String.Format("Congratulations! You have successfully subscribed to {0}. {1}", product.Description, balance);
                                                        ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies!";
                                                        ReturnCode.info7 = user.EMail;
                                                        //Project Black
                                                        PaymentHelper.logProjectBlackUserPromo(context, userId, pid);
                                                        //Xoom2
                                                        PaymentHelper.logUserPromo(context, userId, GlobalConfig.Xoom2PromoId);
                                                        //GA ECommerce
                                                        try
                                                        {
                                                            ReturnCode.info1 = String.Format("{0}", response.transaction.TransactionId); //transaction id ga ecommerce
                                                            ReturnCode.info2 = String.Format("{0}", response.product.ProductId); //sku ga ecommerce
                                                            ReturnCode.info3 = response.product.Description; //name ga ecommerce
                                                            ReturnCode.info4 = String.Format("{0}", response.price.Amount.ToString("F")); //price ga ecommerce
                                                            ReturnCode.info5 = response.price.CurrencyCode; //currency ga ecommerce
                                                            ReturnCode.info6 = response.ProductType; //product type ga ecommerce
                                                        }
                                                        catch (Exception) { }

                                                        try
                                                        {
                                                            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("air"))
                                                            {
                                                                HttpCookie airCookie = new HttpCookie("air");
                                                                airCookie.Expires = DateTime.Now.AddDays(-1);
                                                                Response.Cookies.Add(airCookie);
                                                            }
                                                        }
                                                        catch (Exception) { }

                                                        try
                                                        {
                                                            var pacmayproductIdsForRedirect = MyUtility.StringToIntList(GlobalConfig.PacMayProductIdsForRedirect);
                                                            if (pacmayproductIdsForRedirect.Contains(pid))
                                                                TempData["IsPacMayPurchase"] = true;
                                                        }
                                                        catch (Exception) { }

                                                        TempData["ErrorMessage"] = ReturnCode;
                                                        return RedirectToAction("Index", "Home"); // successful payment
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
                                                    ReturnCode.StatusMessage = "You don't have enough credits on your wallet.";
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                    ReturnCode.StatusMessage = "User does not exist.";
                            }
                        }
                        else
                            ReturnCode.StatusMessage = "Your session has already expired. Please login again.";
                    }
                    else
                        ReturnCode.StatusMessage = "Prepaid Card/ePIN payment is currenty disabled.";
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                TempData["ErrorMessage"] = ReturnCode;

                url = Request.UrlReferrer.AbsolutePath;
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusMessage = e.Message;
                TempData["ErrorMessage"] = ReturnCode;
            }
            return Redirect(url);
        }

        public ActionResult ConfirmPaypalPayment()
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "Paypal",
                TransactionType = "Subscription"
            };
            string url = Url.Action("Index", "Home").ToString();
            try
            {
                var context = new IPTV2Entities();
                string UserEmail = String.Empty;
                if (User.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
                    if (user != null)
                        UserEmail = user.EMail;
                }

                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                ReturnCode.StatusHeader = "You have successfully purchased a product";
                ReturnCode.StatusMessage = "Congratulations! You have purchased a product";
                ReturnCode.StatusMessage2 = "Please check your My Transaction page for payment information.";
                ReturnCode.info7 = UserEmail;

                if (TempData["xoom"] != null)
                {
                    var xoomTempData = (bool)TempData["xoom"];
                    if (xoomTempData)
                    {
                        ReturnCode.StatusHeader = "You have successfully registered and subscribed to a package!";
                        ReturnCode.StatusMessage = String.Format("Congratulations! You claimed your FREE 1st month and subscribed to a plan.");
                    }
                }
                TempData["ErrorMessage"] = ReturnCode;
                try
                {
                    if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("air"))
                    {
                        HttpCookie airCookie = new HttpCookie("air");
                        airCookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(airCookie);
                    }
                }
                catch (Exception) { }

                try
                {
                    if (TempData["IsPacPurchase"] != null)
                        TempData["IsPacMayPurchase"] = true;
                }
                catch (Exception) { }

                return View();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Redirect(url);
        }
    }
}