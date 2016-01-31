using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DevTrends.MvcDonutCaching;
using Gigya.Socialize.SDK;
using GOMS_TFCtv;
using IPTV2_Model;
using Newtonsoft.Json;
using TFCNowModel;
using TFCTV.Helpers;
using TFCTV.Models;
using System.Text;

namespace TFCTV.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/

        public PartialViewResult Index()
        {
            return PartialView("_SignInPartial");
        }

        //[HttpPost]
        //public ActionResult Index(FormCollection collection)
        //{
        //    var context = new IPTV2Entities();
        //    User p = new IPTV2_Model.User();

        //    Dictionary<string, string> dict = new Dictionary<string, string>();
        //    if (collection["emailaddress"] == "istarbuxs@hotmail.com" && collection["password"] == "1234")
        //    {
        //        dict.Add("Message", "Login successful! Please wait...");
        //        dict.Add("errorCode", "0");

        //        return Content(GigyaHelpers.buildJson(dict), "text/json");
        //    }
        //    else
        //    {
        //        dict.Add("Message", "Invalid username/password. Please try again.");
        //        dict.Add("errorCode", "1");
        //    }
        //    return Content(GigyaHelpers.buildJson(dict), "text/json");
        //}

        [RequireHttps]
        public ActionResult Register()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
            {
                try
                {
                    if (User.Identity.IsAuthenticated)
                        return RedirectToAction("Index", "Home");

                    try
                    {
                        if (!Request.Cookies.AllKeys.Contains("rcdskipcheck")) {
                            var dt = DateTime.Parse(Request.Cookies["rcDate"].Value);
                            if (DateTime.Now.Subtract(dt).Days < 45)
                                return RedirectToAction("Index", "Home");
                        }                        
                    }
                    catch (Exception) { }

                    try
                    {
                        if (GlobalConfig.isUAT || Request.IsLocal)
                            if (!MyUtility.IsWhiteListed(String.Empty))
                                return RedirectToAction("Index", "Home");
                    }
                    catch (Exception) { }
                    using (var context = new IPTV2Entities())
                    {
                        var ExcludedCountriesFromRegistrationDropDown = GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',');
                        List<Country> countries = null;
                        if (GlobalConfig.UseCountryListInMemory)
                        {
                            if (GlobalConfig.CountryList != null)
                                countries = GlobalConfig.CountryList.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                            else
                                countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                        }
                        else
                            countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                        ViewBag.ListOfCountries = countries;
                        var location = MyUtility.GetLocationViaIpAddressWithoutProxy();
                        ViewBag.location = location;
                        string CountryCode = String.IsNullOrEmpty(Request.QueryString["hcountry"]) ? location.countryCode : Request.QueryString["hcountry"];
                        var states = context.States.Where(s => String.Compare(s.CountryCode, CountryCode, true) == 0);
                        if (states != null)
                            if (states.Count() > 0)
                                ViewBag.ListOfStates = states.ToList();
                        if (TempData["qs"] != null)
                        {
                            var qs = (NameValueCollection)TempData["qs"];
                            ViewBag.qs = qs;
                            TempData["qs"] = qs;
                        }

                        //drop a registration cookie
                        try
                        {
                            var regCookieValue = Guid.NewGuid();
                            if (!Request.Cookies.AllKeys.Contains("regcook"))
                            {
                                HttpCookie cookie = new HttpCookie("regcook");
                                cookie.Value = regCookieValue.ToString();
                                cookie.Expires = DateTime.Now.AddYears(5);
                                this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                            }
                        }
                        catch (Exception) { }

                        //check for project air promo                        
                        try
                        {
                            var registDt = DateTime.Now;
                            var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.ProjectAirPromoId && p.StartDate < registDt && p.EndDate > registDt && p.StatusId == GlobalConfig.Visible);
                            if (promo != null)
                            {
                                var CountryCodeIP = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.ProjectAirCategoryId);
                                if (category != null)
                                    if (category is Show)
                                        if (ContextHelper.IsCategoryViewableInUserCountry((Show)category, CountryCodeIP))
                                            return Redirect("/WatchNow"); //return RedirectToAction("Index", "Air");
                            }
                        }
                        catch (Exception) { }
                        //if(xoom)
                        try
                        {
                            if (Request.Cookies.AllKeys.Contains("xoom"))
                            {
                                ViewBag.PageTitle = "Register to claim your FREE 1-month Subscription!";
                            }
                        }
                        catch (Exception) { }

                        return View("Registration");
                    }
                }
                catch (Exception e) { MyUtility.LogException(e); }
                return RedirectToAction("Index", "Home");
            }
            else
                return RedirectToAction("RegisterViaSocial", "User");
            //if (MyUtility.isUserLoggedIn())
            //    return RedirectToAction("Index", "Home");
            //var context = new IPTV2Entities();
            //var countries = context.Countries.Where(c => c.Code != "--").OrderBy(c => c.Description).ToList();
            //ViewBag.ListOfCountries = countries;

            ////Get User Country by Ip
            //var userCountry = MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            //if (String.IsNullOrEmpty(userCountry))
            //    userCountry = GlobalConfig.DefaultCountry;
            //var uCountry = context.Countries.FirstOrDefault(c => c.Code == userCountry);
            //ViewBag.UserCountry = uCountry;

            //if (TempData["qs"] != null)
            //{
            //    NameValueCollection qs = (NameValueCollection)TempData["qs"];
            //    TempData["qs"] = qs;
            //    if (!String.IsNullOrEmpty(qs["UID"]))
            //        ViewBag.isConnectedToGigya = true; //Connected to Gigya. Get userInfo on View. Fill registrationSocial with userInfo from social network.

            //    SignUpModel model = new SignUpModel();
            //    model.Email = (String.IsNullOrEmpty(qs["email"])) ? String.Empty : HttpUtility.UrlDecode(qs["email"]);
            //    model.FirstName = (String.IsNullOrEmpty(qs["firstName"])) ? String.Empty : HttpUtility.UrlDecode(qs["firstName"]);
            //    model.LastName = (String.IsNullOrEmpty(qs["lastName"])) ? String.Empty : HttpUtility.UrlDecode(qs["lastName"]);
            //    model.CountryCode = (String.IsNullOrEmpty(qs["country"])) ? String.Empty : HttpUtility.UrlDecode(qs["country"]);

            //    return View(model);
            //}

            //return View();
        }

        //[HttpPost]
        //public string Register(SignUpModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var context = new IPTV2Entities();
        //        int userExist = context.Users.Where(u => u.EMail == model.Email).Count();
        //        if (userExist == 0) // new user
        //        {
        //            var userId = System.Guid.NewGuid();
        //            User user = new IPTV2_Model.User();
        //            user.UserId = userId;
        //            user.EMail = model.Email;
        //            user.FirstName = model.FirstName;
        //            user.LastName = model.LastName;
        //            user.Password = model.Password;
        //            user.GigyaUID = userId.ToString();
        //            //user.Country = model.Country;
        //            try
        //            {
        //                context.Users.Add(user);
        //                string CurrencyCode = GlobalConfig.DefaultCurrency;
        //                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == CurrencyCode);
        //                if (wallet == null) // Wallet does not exist. Create new wallet for User.
        //                {
        //                    wallet = ContextHelper.CreateWallet(0, CurrencyCode);
        //                    user.UserWallets.Add(wallet);
        //                }

        //                int result = context.SaveChanges();
        //                int notifyRegistration = 0;
        //                if (result == 1)
        //                {
        //                    //check if user comes from Gigya
        //                    if (TempData["qs"] != null)
        //                    {
        //                        NameValueCollection qs = (NameValueCollection)TempData["qs"];
        //                        TempData["qs"] = null;
        //                        //notifyRegistration
        //                        Dictionary<string, string> collection = new Dictionary<string, string>();
        //                        collection.Add("uid", qs["UID"]);
        //                        collection.Add("siteUID", userId.ToString());
        //                        collection.Add("cid", String.Format("{0} - New User", qs["provider"]));
        //                        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(collection));
        //                        notifyRegistration = res.GetErrorCode();
        //                    }
        //                    //return RedirectToAction("Index", "Home", new { register = notifyRegistration });
        //                    return notifyRegistration.ToString();
        //                }
        //            }
        //            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        //        }
        //        else
        //            return "2";
        //        //ModelState.AddModelError("TFCTV_ERROR", "Email is already taken.");
        //    }
        //    //return View(model);
        //    return "1";
        //}

        //[HttpPost]
        //public ActionResult _Register(FormCollection fc)
        //{
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    ErrorCodes errorCode = ErrorCodes.UnknownError;
        //    string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
        //    collection = MyUtility.setError(errorCode, errorMessage);
        //    bool isConnectedToSocialNetworks = false;

        //    if (MyUtility.isUserLoggedIn()) //User is logged in.
        //        return RedirectToAction("Index", "Home");
        //    if (String.IsNullOrEmpty(fc["Email"]))
        //    {
        //        collection = MyUtility.setError(ErrorCodes.IsEmailEmpty, MyUtility.getErrorMessage(ErrorCodes.IsEmailEmpty));
        //        return Content(MyUtility.buildJson(collection), "application/json");
        //    }
        //    if (String.Compare(fc["Password"], fc["ConfirmPassword"], false) != 0)
        //    {
        //        collection = MyUtility.setError(ErrorCodes.IsMismatchPassword, MyUtility.getErrorMessage(ErrorCodes.IsMismatchPassword));
        //        return Content(MyUtility.buildJson(collection), "application/json");
        //    }
        //    if (String.IsNullOrEmpty(fc["FirstName"]) || String.IsNullOrEmpty(fc["LastName"]) || String.IsNullOrEmpty(fc["CountryCode"]))
        //    {
        //        collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, MyUtility.getErrorMessage(ErrorCodes.IsExistingEmail));
        //        return Content(MyUtility.buildJson(collection), "application/json");
        //    }

        //    if (!MyUtility.isEmail(fc["Email"]))
        //    {
        //        collection = MyUtility.setError(ErrorCodes.IsNotValidEmail, MyUtility.getErrorMessage(ErrorCodes.IsNotValidEmail));
        //        return Content(MyUtility.buildJson(collection), "application/json");
        //    }

        //    try
        //    {
        //        string FirstName = fc["FirstName"];
        //        string LastName = fc["LastName"];
        //        string CountryCode = fc["CountryCode"];
        //        string EMail = fc["Email"];
        //        string Password = fc["Password"];
        //        string City = fc["City"];
        //        string State = String.IsNullOrEmpty(fc["State"]) ? fc["StateDD"] : fc["State"];
        //        System.Guid userId = System.Guid.NewGuid();
        //        string provider = "tfctv";

        //        var context = new IPTV2Entities();
        //        User user = context.Users.FirstOrDefault(u => u.EMail == EMail);
        //        if (user != null)
        //        {
        //            collection = MyUtility.setError(ErrorCodes.IsExistingEmail, MyUtility.getErrorMessage(ErrorCodes.IsExistingEmail));
        //            return Content(MyUtility.buildJson(collection), "application/json");
        //        }
        //        DateTime registDt = DateTime.Now;
        //        user = new User()
        //        {
        //            UserId = userId,
        //            FirstName = FirstName,
        //            LastName = LastName,
        //            City = City,
        //            State = State,
        //            CountryCode = CountryCode,
        //            EMail = EMail,
        //            Password = MyUtility.GetSHA1(Password),
        //            GigyaUID = userId.ToString(),
        //            RegistrationDate = registDt,
        //            LastUpdated = registDt,
        //            RegistrationIp = Request.GetUserHostAddressFromCloudflare(),
        //            StatusId = 0,
        //            ActivationKey = Guid.NewGuid()
        //        };
        //        string CurrencyCode = GlobalConfig.DefaultCurrency;
        //        Country country = context.Countries.FirstOrDefault(c => c.Code == CountryCode);
        //        if (country != null)
        //        {
        //            Currency currency = context.Currencies.FirstOrDefault(c => c.Code == country.CurrencyCode);
        //            if (currency != null) CurrencyCode = currency.Code;
        //        }
        //        UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == CurrencyCode);
        //        if (wallet == null) // Wallet does not exist. Create new wallet for User.
        //        {
        //            wallet = ContextHelper.CreateWallet(0, CurrencyCode, registDt);
        //            user.UserWallets.Add(wallet);
        //        }
        //        context.Users.Add(user);
        //        if (context.SaveChanges() > 0)
        //        {
        //            if (TempData["qs"] != null)
        //            {
        //                NameValueCollection qs = (NameValueCollection)TempData["qs"];
        //                Dictionary<string, object> GigyaCollection = new Dictionary<string, object>();
        //                collection.Add("uid", qs["UID"]);
        //                collection.Add("siteUID", userId);
        //                collection.Add("cid", String.Format("{0} - New User", qs["provider"]));
        //                GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(collection));
        //                provider = qs["provider"];
        //                isConnectedToSocialNetworks = true;
        //            }
        //            else
        //            {
        //                Dictionary<string, object> userInfo = new Dictionary<string, object>();
        //                userInfo.Add("firstName", user.FirstName);
        //                userInfo.Add("lastName", user.LastName);
        //                userInfo.Add("email", user.EMail);
        //                Dictionary<string, object> gigyaCollection = new Dictionary<string, object>();
        //                gigyaCollection.Add("siteUID", user.UserId);
        //                gigyaCollection.Add("cid", "TFCTV - Registration");
        //                gigyaCollection.Add("sessionExpiration", "0");
        //                gigyaCollection.Add("newUser", true);
        //                gigyaCollection.Add("userInfo", MyUtility.buildJson(userInfo));
        //                GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(gigyaCollection));
        //                GigyaHelpers.setCookie(res, this.ControllerContext);
        //            }

        //            //setUserData
        //            User usr = context.Users.FirstOrDefault(u => u.EMail == EMail);
        //            setUserData(usr.UserId.ToString(), usr);

        //            if (isConnectedToSocialNetworks)
        //            {
        //                usr.StatusId = 1;
        //                usr.DateVerified = registDt;
        //            }

        //            //If FreeTrial is enabled, insert free trial.
        //            if (GlobalConfig.IsFreeTrialEnabled)
        //            {
        //                context = new IPTV2Entities();
        //                PaymentHelper.PayViaWallet(context, userId, GlobalConfig.FreeTrial14ProductId, SubscriptionProductType.Package, userId, null);
        //                context.SaveChanges();
        //            }

        //            //Publish to Activity Feed
        //            List<ActionLink> actionlinks = new List<ActionLink>();
        //            actionlinks.Add(new ActionLink() { text = SNSTemplates.register_actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_actionlink_href) });
        //            //mediaItem
        //            List<MediaItem> mediaItems = new List<MediaItem>();
        //            mediaItems.Add(new MediaItem() { type = SNSTemplates.register_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_mediaitem_href) });
        //            UserAction action = new UserAction()
        //            {
        //                actorUID = userId.ToString(),
        //                userMessage = SNSTemplates.register_usermessage,
        //                title = SNSTemplates.register_title,
        //                subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_subtitle),
        //                linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_linkback),
        //                description = String.Format(SNSTemplates.register_description, FirstName),
        //                actionLinks = actionlinks,
        //                mediaItems = mediaItems
        //            };

        //            GigyaMethods.PublishUserAction(action, userId, "external");
        //            action.userMessage = String.Empty;
        //            action.title = String.Empty;
        //            action.mediaItems = null;
        //            GigyaMethods.PublishUserAction(action, userId, "internal");

        //            //FormsAuthentication.SetAuthCookie(userId.ToString(), true);
        //            SetAutheticationCookie(userId.ToString());
        //            errorMessage = "Thank you! You are now registered to TFC.tv!";
        //            collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
        //            collection.Add("info", String.Format("{0}|{1}|{2}", user.EMail, Request.GetUserHostAddressFromCloudflare(), provider));

        //        }
        //        else
        //        {
        //            errorMessage = "The system encountered an unidentified error. Please try again.";
        //            collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
        //        }
        //    }
        //    catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        //    return Content(MyUtility.buildJson(collection), "application/json");
        //}

        private int setUserData(string uid, User user)
        {
            try
            {
                //SignUpModel usr = new SignUpModel()
                //{
                //    City = user.City,
                //    CountryCode = user.CountryCode,
                //    Email = user.EMail,
                //    FirstName = user.FirstName,
                //    LastName = user.LastName,
                //    State = user.State
                //};

                GigyaUserData2 userData = new GigyaUserData2()
                {
                    city = user.City,
                    country = user.CountryCode,
                    email = user.EMail,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    state = user.State
                };

                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("uid", uid);
                //collection.Add("data", JsonConvert.SerializeObject(usr, Formatting.None));
                collection.Add("profile", JsonConvert.SerializeObject(userData, Formatting.None));
                //gcs.setUserData
                //GSResponse res = GigyaHelpers.createAndSendRequest("gcs.setUserData", GigyaHelpers.buildParameter(collection));
                GSResponse res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", GigyaHelpers.buildParameter(collection));
                return res.GetErrorCode();
            }
            catch (Exception) { }
            return 0;
        }

        //[HttpPost]
        //public ActionResult LogIn(SignInModel model)
        //{
        //    var context = new IPTV2Entities();
        //    var user = context.Users.FirstOrDefault(u => u.EMail == model.EmailAddress && u.Password == model.Password);
        //    if (user != null)
        //    {
        //        FormsAuthentication.SetAuthCookie(user.UserId.ToString(), false);
        //        Dictionary<string, string> collection = new Dictionary<string, string>();
        //        Dictionary<string, string> uInfo = new Dictionary<string, string>();
        //        uInfo.Add("firstName", user.FirstName);
        //        uInfo.Add("lastName", user.LastName);
        //        uInfo.Add("email", user.EMail);
        //        string userInfo = GigyaHelpers.buildJson(uInfo);
        //        collection.Add("siteUID", user.UserId.ToString());
        //        collection.Add("cid", "TFCTV - Login");
        //        collection.Add("sessionExpiration", "0");
        //        collection.Add("userInfo", userInfo);
        //        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(collection));
        //        return RedirectToAction("Index", "Home", new { err = res.GetErrorCode() });

        //        //return GigyaHelpers.buildJson(collection);
        //    }
        //    return PartialView("_SignInPartial", model);
        //    //return "{}";
        //}

        [HttpPost]
        public ActionResult _LoginX(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            if (MyUtility.isUserLoggedIn()) //User is logged in.
                return RedirectToAction("Index", "Home");

            try
            {
                ViewBag.IsTVEverywhere = false;
                string EmailAddress = fc["EmailAddress"];
                string Password = fc["Password"];

                if (String.IsNullOrEmpty(fc["EmailAddress"]))
                {
                    errorCode = ErrorCodes.IsMissingRequiredFields;
                    collection = MyUtility.setError(errorCode, "Email address is required.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                if (String.IsNullOrEmpty(fc["Password"]))
                {
                    errorCode = ErrorCodes.IsMissingRequiredFields;
                    collection = MyUtility.setError(errorCode, "Password is required.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);

                if (user == null)
                    collection = MyUtility.setError(ErrorCodes.UserDoesNotExist);
                else
                {
                    //if (user.StatusId != 1 && user.DateVerified == null) // Not verified
                    if (user.StatusId != 1) // Not verified
                    {
                        errorCode = ErrorCodes.IsNotVerified;
                        collection = MyUtility.setError(errorCode, "Email is not verified.");
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }

                    Password = MyUtility.GetSHA1(Password);
                    if (String.Compare(user.EMail, EmailAddress, true) == 0 && String.Compare(user.Password, Password, false) == 0)
                    {
                        Dictionary<string, object> userInfo = new Dictionary<string, object>();
                        userInfo.Add("firstName", user.FirstName);
                        userInfo.Add("lastName", user.LastName);
                        userInfo.Add("email", user.EMail);
                        Dictionary<string, object> gigyaCollection = new Dictionary<string, object>();
                        gigyaCollection.Add("siteUID", user.UserId);
                        gigyaCollection.Add("cid", "TFCTV - Login");
                        gigyaCollection.Add("sessionExpiration", "0");
                        gigyaCollection.Add("userInfo", MyUtility.buildJson(userInfo));
                        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(gigyaCollection));
                        GigyaHelpers.setCookie(res, this.ControllerContext);
                        errorMessage = "Login successful!";
                        //Session.SessionID;
                        SetAutheticationCookie(user.UserId.ToString());
                        SetSession(user.UserId.ToString());
                        ContextHelper.SaveSessionInDatabase(context, user);
                        //FormsAuthentication.SetAuthCookie(user.UserId.ToString(), true);
                        collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                        var UserHostAddress = Request.GetUserHostAddressFromCloudflare();
                        collection.Add("info", String.Format("{0}|{1}|{2}|{3}|{4}", user.EMail, UserHostAddress, "Site", MyUtility.getCountry(UserHostAddress).getCode(), DateTime.Now.ToString("yyyyMMdd-HHmmss")));

                        //UPDATE: FEB18,2013 - If TVE cookie is valid, assign user who logged in as TVERegistrant
                        try
                        {
                            if (MyUtility.isTVECookieValid())
                            {
                                user.IsTVERegistrant = true;
                                context.SaveChanges();
                                MyUtility.RemoveTVECookie();
                            }
                            if (user.IsTVEverywhere == true)
                            {
                                collection.Add("href", "/TFCChannel");
                            }
                        }
                        catch (Exception) { }
                    }
                    else
                        collection = MyUtility.setError(ErrorCodes.IsWrongPassword);
                }
            }
            catch (Exception e)
            {
                collection = MyUtility.setError(ErrorCodes.UnknownError, e.Message);
                //Debug.WriteLine(e.InnerException); throw;
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        //[HttpPost]
        //public string LoginAlt(SignInModel model)
        //{
        //    var context = new IPTV2Entities();
        //    var user = context.Users.FirstOrDefault(u => u.EMail == model.EmailAddress && u.Password == model.Password);
        //    if (user != null)
        //    {
        //        FormsAuthentication.SetAuthCookie(user.UserId.ToString(), false);
        //        Dictionary<string, string> collection = new Dictionary<string, string>();
        //        Dictionary<string, string> uInfo = new Dictionary<string, string>();
        //        uInfo.Add("firstName", user.FirstName);
        //        uInfo.Add("lastName", user.LastName);
        //        uInfo.Add("email", user.EMail);
        //        collection.Add("siteUID", user.UserId.ToString());
        //        collection.Add("cid", "TFCTV - Login");
        //        collection.Add("sessionExpiration", "0");
        //        collection.Add("userInfo", GigyaHelpers.buildJson(uInfo));
        //        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(collection));
        //        HttpCookie cookie = new HttpCookie(res.GetString("cookieName", string.Empty));
        //        cookie.Domain = res.GetString("cookieDomain", string.Empty);
        //        cookie.Path = res.GetString("cookiePath", string.Empty);
        //        cookie.Value = res.GetString("cookieValue", string.Empty);
        //        cookie.Expires.AddDays(1);
        //        Response.Cookies.Add(cookie);
        //        HttpCookie fullName = new HttpCookie("fullName", String.Format("{0} {1}", user.FirstName, user.LastName));
        //        Response.Cookies.Add(fullName);
        //        return res.GetErrorCode().ToString();
        //    }
        //    return "1";
        //}

        [HttpPost]
        public ActionResult _Link(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            if (MyUtility.isUserLoggedIn()) //User is logged in.
                return RedirectToAction("Index", "Home");

            try
            {
                string EmailAddress = fc["EmailAddress"];
                string Password = fc["Password"];
                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => u.EMail == EmailAddress);
                if (user == null)
                    collection = MyUtility.setError(ErrorCodes.UserDoesNotExist);
                else
                {
                    Password = MyUtility.GetSHA1(Password);
                    if (String.Compare(user.EMail, EmailAddress, true) == 0 && String.Compare(user.Password, Password, false) == 0)
                    {
                        /** notifyRegistration **/
                        Dictionary<string, object> regCollection = new Dictionary<string, object>();

                        regCollection.Add("siteUID", user.UserId.ToString());
                        NameValueCollection qs;
                        if (TempData["qs"] != null)
                        {
                            qs = (NameValueCollection)TempData["qs"];
                            regCollection.Add("uid", Uri.UnescapeDataString(qs["UID"]));
                            regCollection.Add("cid", String.Format("{0} - New User", qs["provider"]));
                        }
                        GSResponse notifyRegistration = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(regCollection));
                        /** end of notifyRegistration **/

                        if (notifyRegistration.GetErrorCode() == 0) //Successful link
                        {
                            //notifyLogin
                            if (GlobalConfig.EnableNotifyLoginOnLinkAccount)
                            {
                                Dictionary<string, object> userInfo = new Dictionary<string, object>();
                                userInfo.Add("firstName", user.FirstName);
                                userInfo.Add("lastName", user.LastName);
                                userInfo.Add("email", user.EMail);
                                Dictionary<string, object> gigyaCollection = new Dictionary<string, object>();
                                gigyaCollection.Add("siteUID", user.UserId);
                                gigyaCollection.Add("cid", "TFCTV - Login");
                                gigyaCollection.Add("sessionExpiration", "0");
                                gigyaCollection.Add("userInfo", MyUtility.buildJson(userInfo));
                                GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(gigyaCollection));
                                GigyaHelpers.setCookie(res, this.ControllerContext);
                            }


                            //Activate the account
                            user.DateVerified = DateTime.Now;
                            user.StatusId = 1;


                            //UPDATE: FEB 18, 2013 - Cookie is valid. Tag as TVE Registrant.
                            if (GlobalConfig.IsTVERegistrationEnabled)
                            {
                                if (MyUtility.isTVECookieValid())
                                {
                                    user.IsTVERegistrant = true;
                                    MyUtility.RemoveTVECookie();
                                    var href = GlobalConfig.TVERegistrationPage;
                                    collection.Add("href", href);
                                }
                            }

                            context.SaveChanges();

                            errorMessage = "Linking successful!";
                            SetAutheticationCookie(user.UserId.ToString());
                            //FormsAuthentication.SetAuthCookie(user.UserId.ToString(), false);
                            collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                        }
                        else
                        {
                            errorMessage = "Unable to link your account. Please try again.";
                            collection = MyUtility.setError(ErrorCodes.FailedToLinkAccount, errorMessage);
                        }
                    }
                    else
                        collection = MyUtility.setError(ErrorCodes.IsWrongPassword);
                }
            }
            catch (Exception e)
            {
                collection = MyUtility.setError(ErrorCodes.UnknownError, e.Message);
                //Debug.WriteLine(e.InnerException); throw;
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        //[HttpPost]
        //public string Link(SignInModel model)
        //{
        //    var context = new IPTV2Entities();
        //    var user = context.Users.FirstOrDefault(u => u.EMail == model.EmailAddress && u.Password == model.Password);
        //    if (user != null)
        //    {
        //        Dictionary<string, string> collection = new Dictionary<string, string>();
        //        collection.Add("uid", user.UserId.ToString());
        //        collection.Add("siteUID", user.UserId.ToString());
        //        NameValueCollection qs;
        //        if (TempData["qs"] != null)
        //        {
        //            qs = (NameValueCollection)TempData["qs"];
        //            collection.Add("cid", String.Format("{0} - New User", qs["provider"]));
        //        }
        //        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(collection));
        //        return res.GetErrorCode().ToString();
        //    }
        //    return "1";
        //}

        public ActionResult LogOut()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    Dictionary<string, string> collection = new Dictionary<string, string>();
                    collection.Add("uid", User.Identity.Name);
                    GSResponse res = GigyaHelpers.createAndSendRequest("socialize.logout", GigyaHelpers.buildParameter(collection));
                    FormsAuthentication.SignOut();
                    //Response.Cookies.Remove("uid");                    
                    HttpCookie cookie = new HttpCookie("uid");
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(cookie);
                    try
                    {
                        HttpCookie formCookie = new HttpCookie(".TFCTV");
                        formCookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(formCookie);

                        HttpCookie sessionCookie = new HttpCookie("ASP.NET_SessionId");
                        sessionCookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(sessionCookie);
                    }
                    catch (Exception) { }
                    try
                    {
                        TempData["LoginErrorMessage"] = null;
                        TempData["RedirectUrl"] = null;
                    }
                    catch (Exception) { }
                    //string url = Request.UrlReferrer.AbsolutePath;
                    //return Redirect(url);
                    return RedirectToAction("Index", "Home");
                }

            }
            catch (Exception) { }
            return RedirectToAction("Index", "Home");
            ///**rbr**/
            ////This will handle request using i.e. browser.
            //var retUrl = Request.QueryString["returl"];

            //if (MyUtility.isUserLoggedIn())
            //{
            //    Dictionary<string, string> collection = new Dictionary<string, string>();
            //    collection.Add("uid", User.Identity.Name);
            //    GSResponse res = GigyaHelpers.createAndSendRequest("socialize.logout", GigyaHelpers.buildParameter(collection));
            //    //if (res.GetErrorCode() == 0) //When the above line resulted to value greater than zero the user won't be logged out.
            //    //{
            //    FormsAuthentication.SignOut();

            //    //for (int i = 0; i < Request.Cookies.Count - 1; i++)
            //    //{
            //    //    Response.Cookies[i].Expires.AddDays(-1);
            //    //    Response.Cookies.Remove(Request.Cookies[i].Name);
            //    //}
            //    //}
            //}
            ///**rbr**/
            //var referrerUrl = "";
            //try
            //{
            //    referrerUrl = Request.UrlReferrer.ToString();
            //}
            //catch (Exception)
            //{
            //    //i.e browser may throw an exception
            //    referrerUrl = retUrl;
            //}

            //if (!String.IsNullOrEmpty(retUrl))
            //    return Redirect(referrerUrl); //goes back to the same page where user logs out.
            //return RedirectToAction("Index", "Home");

            ////return RedirectToAction("Index", "Home");
        }

        [RequireHttp]
        public ActionResult Transactions()
        {
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("Transactions2");
            return View();
        }

        [RequireHttp]
        public ActionResult Entitlements()
        {
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");
            //if (!Request.Cookies.AllKeys.Contains("version"))
            //    return View("Entitlements2");
            //return View();
            return View("Entitlements2");
        }

        public ActionResult EditProfile()
        {
            if (MyUtility.isUserLoggedIn())
            {
                var context = new IPTV2Entities();
                // Remove EU ME ---
                var ExcludedCountriesFromRegistrationDropDown = GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',');
                List<Country> countries = null;
                if (GlobalConfig.UseCountryListInMemory)
                {
                    if (GlobalConfig.CountryList != null)
                        countries = GlobalConfig.CountryList.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                    else
                        countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                }
                else
                    countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                ViewBag.ListOfCountries = countries;
                User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(User.Identity.Name));
                if (user != null)
                {
                    //Get User Country via profile
                    var userCountry = user.Country != null ? user.Country.Code : GlobalConfig.DefaultCountry;
                    if (String.IsNullOrEmpty(userCountry))
                        userCountry = GlobalConfig.DefaultCountry;
                    var uCountry = context.Countries.FirstOrDefault(c => c.Code == userCountry);
                    ViewBag.UserCountry = uCountry;
                    var userData = MyUtility.GetUserPrivacySetting(user.UserId);
                    ViewBag.UserData = userData;

                    var recurringBilling = context.RecurringBillings.Where(r => r.UserId == user.UserId && r.StatusId != 2);
                    if (recurringBilling != null)
                        ViewBag.RecurringBilling = recurringBilling;
                    if (!Request.Cookies.AllKeys.Contains("version"))
                        return RedirectToActionPermanent("EditYourProfile", "User");
                    return View(user);
                }
                else
                    return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult _EditProfile(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            DateTime registDt = DateTime.Now;
            if (Request.IsAjaxRequest())
            {
                if (MyUtility.isUserLoggedIn())
                {
                    try
                    {
                        string CountryCode = fc["CountryCode"];
                        string State = String.IsNullOrEmpty(fc["State"]) ? fc["StateDD"] : fc["State"];

                        if (String.IsNullOrEmpty(fc["FirstName"]) || String.IsNullOrEmpty(fc["LastName"]) || String.IsNullOrEmpty(fc["CountryCode"]))
                        {
                            collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }

                        string FirstName = fc["FirstName"];
                        string LastName = fc["LastName"];
                        string City = fc["City"];

                        if (FirstName.Length > 32)
                        {
                            collection = MyUtility.setError(ErrorCodes.LimitReached, "First Name cannot exceed 32 characters.");
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                        if (LastName.Length > 32)
                        {
                            collection = MyUtility.setError(ErrorCodes.LimitReached, "Last Name cannot exceed 32 characters.");
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                        if (!String.IsNullOrEmpty(State))
                            if (State.Length > 30)
                            {
                                collection = MyUtility.setError(ErrorCodes.LimitReached, "State cannot exceed 30 characters.");
                                return Content(MyUtility.buildJson(collection), "application/json");
                            }
                        if (!String.IsNullOrEmpty(City))
                            if (City.Length > 50)
                            {
                                collection = MyUtility.setError(ErrorCodes.LimitReached, "City cannot exceed 50 characters.");
                                return Content(MyUtility.buildJson(collection), "application/json");
                            }

                        var context = new IPTV2Entities();

                        /***** CHECK FOR COUNTRY CODE ****/
                        if (context.Countries.Count(c => String.Compare(c.Code, CountryCode, true) == 0) <= 0)
                        {
                            collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "Country Code is invalid.");
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                        else if (GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',').Contains(CountryCode))
                        {
                            collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "Country Code is invalid.");
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }

                        if (String.IsNullOrEmpty(State))
                        {
                            collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "State is required.");
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                        else
                        {
                            if (context.States.Count(c => c.CountryCode == CountryCode.ToUpper()) > 0)
                            {
                                if (context.States.Count(s => s.CountryCode == CountryCode.ToUpper() && (s.StateCode == State || s.Name == State)) == 0)
                                {
                                    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "State is invalid.");
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }
                            }
                        }

                        //if (String.IsNullOrEmpty(State))
                        //{
                        //    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "State is required.");
                        //    return Content(MyUtility.buildJson(collection), "application/json");
                        //}
                        //else
                        //{
                        //    if (context.Countries.Count(c => c.Code == CountryCode.ToUpper()) > 0)
                        //    {
                        //        if (context.States.Count(s => s.CountryCode == CountryCode.ToUpper() && (s.StateCode == State || s.Name == State)) == 0)
                        //        {
                        //            collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "State is invalid.");
                        //            return Content(MyUtility.buildJson(collection), "application/json");
                        //        }
                        //    }
                        //}

                        User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(User.Identity.Name));
                        if (user != null)
                        {
                            string CurrencyCode = GlobalConfig.DefaultCurrency;
                            string OldCountryCode = GlobalConfig.DefaultCountry;
                            Country country = context.Countries.FirstOrDefault(c => c.Code == user.CountryCode);
                            if (country != null)
                            {
                                CurrencyCode = country.Currency.Code; country = null;
                                OldCountryCode = user.Country.Code;
                            }

                            UserWallet currentWallet = user.UserWallets.FirstOrDefault(w => w.Currency == CurrencyCode);
                            if (currentWallet == null) //If no wallet, get default USD wallet.
                                currentWallet = user.UserWallets.FirstOrDefault(w => w.Currency == GlobalConfig.DefaultCurrency);

                            string newCountryCode = fc["CountryCode"];
                            user.FirstName = fc["FirstName"];
                            user.LastName = fc["LastName"];
                            user.City = fc["City"];
                            user.State = String.IsNullOrEmpty(fc["State"]) || fc["State"] == "" ? (String.IsNullOrEmpty(fc["StateDD"]) ? user.State : fc["StateDD"]) : fc["State"];

                            country = context.Countries.FirstOrDefault(c => c.Code == newCountryCode);
                            if (country != null)
                            {
                                Currency currency = country.Currency;
                                CurrencyCode = (currency == null) ? GlobalConfig.DefaultCurrency : currency.Code;
                            }

                            UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == CurrencyCode);
                            decimal balance = 0;
                            decimal oldWalletBalance = currentWallet.Balance;
                            string oldCurrency = currentWallet.Currency;
                            var oldGomsWalletId = currentWallet.GomsWalletId;
                            if (wallet == null) // Wallet does not exist. Create new wallet for User.
                            {
                                if (currentWallet != null)
                                {
                                    balance = currentWallet.Currency != CurrencyCode ? Forex.Convert(context, currentWallet.Currency, CurrencyCode, currentWallet.Balance) : currentWallet.Balance;
                                    //balance = currentWallet.Balance;
                                    currentWallet.Balance = 0;
                                    currentWallet.IsActive = false;
                                    //currentWallet.GomsWalletId = null; // Reset Goms WalletId
                                    currentWallet.LastUpdated = registDt;
                                }
                                wallet = ContextHelper.CreateWallet(balance, CurrencyCode, registDt);
                                user.UserWallets.Add(wallet);
                            }
                            else // Wallet already exists. Update the balance only.
                            {
                                if (currentWallet.Currency != wallet.Currency)
                                {
                                    balance = currentWallet.Currency != wallet.Currency ? Forex.Convert(context, currentWallet.Currency, wallet.Currency, currentWallet.Balance) : currentWallet.Balance;
                                    wallet.Balance = balance;
                                    //wallet.Balance += (currentWallet.Balance * 1);
                                    wallet.IsActive = true;
                                    wallet.LastUpdated = registDt;
                                    wallet.GomsWalletId = null; // Reset Goms WalletId
                                    currentWallet.Balance = 0; // Deactivate old wallet
                                    currentWallet.IsActive = false; //Deactivate
                                    //currentWallet.GomsWalletId = null; // Reset Goms WalletId
                                    currentWallet.LastUpdated = registDt;
                                }
                            }

                            user.CountryCode = newCountryCode; // Update user country
                            user.LastUpdated = registDt; // lastUpdate

                            // update the user's recurring billing
                            if (!String.IsNullOrEmpty(fc["rb_list"]))
                                UpdateRecurringBillingViaEditProfile(context, user, fc["rb_list"]);

                            if (OldCountryCode != newCountryCode)
                            {
                                var offering = context.Offerings.Find(GlobalConfig.offeringId);

                                //Get User Transactions
                                if (user.HasOtherPendingGomsTransaction(offering))
                                {
                                    errorMessage = "We are still processing your transactions. Please try again after a few minutes.";
                                    collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }
                                if (user.HasPendingGomsChangeCountryTransaction(offering))
                                {
                                    errorMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                    collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }

                                if (user.HasTVEverywhereEntitlement(MyUtility.StringToIntList(GlobalConfig.TVEverywherePackageIds), offering))
                                {
                                    errorMessage = "You are not allowed to change country being a TV Everywhere user.";
                                    collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }

                                ChangeCountryTransaction transaction = new ChangeCountryTransaction()
                                {
                                    OldCountryCode = OldCountryCode,
                                    NewCountryCode = newCountryCode,
                                    Date = registDt,
                                    OfferingId = GlobalConfig.offeringId,
                                    Reference = "Change Country",
                                    UserId = user.UserId,
                                    Amount = 0,
                                    NewWalletBalance = balance,
                                    OldWalletBalance = oldWalletBalance,
                                    OldGomsCustomerId = user.GomsCustomerId,
                                    OldGomsWalletId = oldGomsWalletId,
                                    Currency = oldCurrency,
                                    StatusId = GlobalConfig.Visible
                                };

                                user.Transactions.Add(transaction);
                            }

                            if (context.SaveChanges() > 0)
                            {
                                //string ProfilePageShare = fc["ProfilePageShare"];
                                //string EveryoneActivityFeedShare = fc["EveryoneActivityFeedShare"];
                                //string SocialNetworkShare = fc["SocialNetworkShare"];

                                string IsInternalSharingEnabled = fc["IsInternalSharingEnabled"];
                                string IsExternalSharingEnabled = fc["IsExternalSharingEnabled"];
                                string IsProfilePrivate = fc["IsProfilePrivate"];

                                UserData userData = new UserData()
                                {
                                    IsInternalSharingEnabled = IsInternalSharingEnabled,
                                    IsExternalSharingEnabled = IsExternalSharingEnabled,
                                    IsProfilePrivate = IsProfilePrivate
                                    //ProfilePageShare = ProfilePageShare,
                                    //EveryoneActivityFeedShare = EveryoneActivityFeedShare,
                                    //SocialNetworkShare = SocialNetworkShare
                                };
                                var res = GigyaMethods.SetUserData(user.UserId, userData);
                                //setUserData
                                setUserData(user.UserId.ToString(), user);

                                errorMessage = "Your information has been updated successfully.";
                                collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                            }
                            else
                            {
                                errorMessage = "Error in updating your profile.";
                                collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        collection = MyUtility.setError(ErrorCodes.UnknownError, e.Message);
                    }
                }
            }

            return Content(MyUtility.buildJson(collection), "application/json");
        }

        //[GridAction]
        //public ActionResult _Transactions()
        //{
        //    return View(new GridModel<TransactionDisplay> { Data = ShowTransaction() });
        //}

        //[GridAction]
        //[OutputCache(NoStore = true, Duration = 0)]
        //public ActionResult _Entitlements()
        //{
        //    return View(new GridModel<EntitlementDisplay> { Data = ShowEntitlements() });
        //}

        private List<TransactionDisplay> ShowTransaction()
        {
            List<TransactionDisplay> display = new List<TransactionDisplay>();
            if (User.Identity.IsAuthenticated)
            {
                System.Guid userId = new System.Guid(User.Identity.Name);
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    var transactions = context.Transactions.Where(t => t.UserId == user.UserId && t.OfferingId == GlobalConfig.offeringId && t.StatusId == GlobalConfig.Visible).OrderByDescending(t => t.TransactionId).ToList();

                    foreach (Transaction transaction in transactions)
                    {
                        TransactionDisplay disp = new TransactionDisplay();

                        disp.TransactionId = transaction.TransactionId;
                        disp.Reference = transaction.Reference;
                        disp.Amount = transaction.Amount;
                        disp.Currency = transaction.Currency;
                        disp.TransactionDate = transaction.Date;
                        //disp.TransactionDateStr = transaction.Date.ToString("MMM. dd, yyyy hh:mm:ss tt");
                        //disp.TransactionDateStr = transaction.Date.ToString("MM/dd/yyyy hh:mm:ss tt");
                        disp.TransactionDateStr = transaction.Date.ToString("MM/dd/yyyy");

                        if (transaction is PaymentTransaction)
                        {
                            PaymentTransaction ptrans = (PaymentTransaction)transaction;

                            string remarks = ptrans.Purchase.Remarks;
                            bool purchase = String.IsNullOrEmpty(remarks) ? false : remarks.StartsWith("Gift");
                            disp.TransactionType = purchase ? "Gift" : "Subscription";
                            PurchaseItem item = ptrans.Purchase.PurchaseItems.FirstOrDefault();
                            disp.ProductId = item.ProductId;
                            Product product = context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                            if (product != null)
                                disp.ProductName = product.Name;
                            if (ptrans is PpcPaymentTransaction)
                            {
                                disp.PpcId = ptrans.Reference;
                                disp.PaymentType = "Prepaid Card/ePIN";
                            }
                            else if (ptrans is PaypalPaymentTransaction)
                                disp.PaymentType = "Paypal";
                            else if (ptrans is CreditCardPaymentTransaction)
                                disp.PaymentType = "Credit Card";
                            else if (ptrans is WalletPaymentTransaction)
                                disp.PaymentType = "Wallet";
                            disp.Method = disp.PaymentType;
                        }
                        else if (transaction is UpgradeTransaction)
                        {
                            UpgradeTransaction utrans = (UpgradeTransaction)transaction;

                            disp.TransactionType = "Upgrade";
                            Product product = context.Products.FirstOrDefault(p => p.ProductId == utrans.OriginalProductId);
                            if (product != null)
                                disp.ProductName = product.Name;
                            Product oldProduct = context.Products.FirstOrDefault(p => p.ProductId == utrans.NewProductId);
                            disp.Reference = String.Format("Upgraded to {0}", oldProduct.Name);
                            disp.Method = "N/A";
                        }
                        else if (transaction is TfcEverywhereTransaction)
                        {
                            TfcEverywhereTransaction ttrans = (TfcEverywhereTransaction)transaction;
                            disp.TransactionType = "Update";
                            disp.ProductName = "TFC Everywhere";
                            disp.Method = "N/A";
                            disp.Amount = 0;

                        }
                        else if (transaction is CancellationTransaction)
                        {
                            CancellationTransaction ctrans = (CancellationTransaction)transaction;
                            disp.TransactionType = "Cancellation";
                            //disp.ProductName = String.Format("TID: {0}", ctrans.OriginalTransactionId);
                            disp.ProductName = "N/A";
                            disp.Method = "N/A";
                            disp.Amount = 0;

                        }
                        else if (transaction is ChangeCountryTransaction)
                        {
                            ChangeCountryTransaction ctrans = (ChangeCountryTransaction)transaction;
                            disp.TransactionType = "Change Country";
                            disp.ProductName = "N/A";
                            disp.Method = "N/A";
                            disp.Reference = String.Format("Change Country from {0} to {1}", ctrans.OldCountryCode, ctrans.NewCountryCode);
                        }
                        else if (transaction is MigrationTransaction)
                        {
                            MigrationTransaction mtrans = (MigrationTransaction)transaction;
                            disp.TransactionType = "Migrate Licenses";
                            disp.ProductName = "N/A";
                            if (mtrans.MigratedProductId > 0)
                            {
                                Product product = context.Products.FirstOrDefault(p => p.ProductId == mtrans.MigratedProductId);
                                if (product != null)
                                    disp.ProductName = product.Name;
                            }
                            disp.Method = "N/A";
                            disp.Reference = mtrans.Reference;
                        }
                        else if (transaction is RegistrationTransaction)
                        {
                            RegistrationTransaction rgtrans = (RegistrationTransaction)transaction;
                            disp.TransactionType = "Registration";
                            disp.ProductName = "N/A";
                            disp.Method = "N/A";
                        }
                        else if (transaction is ReloadTransaction)
                        {
                            ReloadTransaction rtrans = (ReloadTransaction)transaction;
                            disp.ProductName = "N/A";
                            disp.TransactionType = "Reload";
                            if (rtrans is PpcReloadTransaction)
                            {
                                disp.PpcId = rtrans.Reference;
                                disp.ReloadType = "Prepaid Card/ePIN";
                            }
                            else if (rtrans is PaypalReloadTransaction)
                                disp.ReloadType = "Paypal";
                            else if (rtrans is CreditCardReloadTransaction)
                                disp.ReloadType = "Credit Card";
                            else if (rtrans is SmartPitReloadTransaction)
                                disp.ReloadType = "Smart Pit";

                            disp.Method = disp.ReloadType;
                        }
                        display.Add(disp);
                    }
                }
            }
            return display;
        }

        private List<EntitlementDisplay> ShowEntitlements()
        {
            List<EntitlementDisplay> display = new List<EntitlementDisplay>();
            if (User.Identity.IsAuthenticated)
            {
                System.Guid userId = new System.Guid(User.Identity.Name);
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    var entitlements = context.Entitlements.Where(t => t.UserId == user.UserId && t.OfferingId == GlobalConfig.offeringId).OrderByDescending(t => t.EndDate).ToList();

                    foreach (Entitlement entitlement in entitlements)
                    {
                        EntitlementDisplay disp = new EntitlementDisplay();

                        disp.EntitlementId = entitlement.EntitlementId;
                        disp.ExpiryDate = entitlement.EndDate;
                        //disp.ExpiryDateStr = entitlement.EndDate.ToString("MMM. dd, yyyy hh:mm:ss tt");
                        //disp.ExpiryDateStr = entitlement.EndDate.ToString("MM/dd/yyyy hh:mm:ss tt");
                        disp.ExpiryDateStr = entitlement.EndDate.ToString("MM/dd/yyyy");
                        if (entitlement is PackageEntitlement)
                        {
                            var pkg = (PackageEntitlement)entitlement;
                            disp.PackageId = pkg.PackageId;
                            disp.PackageName = pkg.Package.Description;
                            disp.Content = disp.PackageName;
                        }
                        else if (entitlement is ShowEntitlement)
                        {
                            var show = (ShowEntitlement)entitlement;
                            disp.CategoryId = show.CategoryId;
                            disp.CategoryName = show.Show.Description;
                            disp.Content = disp.CategoryName;
                        }
                        else if (entitlement is EpisodeEntitlement)
                        {
                            var episode = (EpisodeEntitlement)entitlement;
                            disp.EpisodeId = episode.EpisodeId;
                            disp.EpisodeName = episode.Episode.Description + ", " + episode.Episode.DateAired.Value.ToString("MMM. dd, yyyy");
                            disp.Content = disp.EpisodeName;
                        }

                        display.Add(disp);
                    }
                }
            }
            return display;
        }

        public ActionResult GetUserCredits()
        {
            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                    if (wallet != null)
                    {
                        ViewBag.Balance = wallet.Currency + " " + wallet.Balance.ToString("F");
                        //ViewBag.CurrencyCode = wallet.Currency;
                        //ViewBag.Amount = wallet.Balance;
                    }
                    else
                    {
                        var CurrencyCode = context.Countries.FirstOrDefault(c => c.Code == user.CountryCode).Currency.Code;
                        ViewBag.Balance = CurrencyCode + " 0.00";
                        //ViewBag.CurrencyCode = CurrencyCode;
                        //ViewBag.Amount = 0;
                    }

                }
            }
            return PartialView("_UserCreditsPartial");
        }

        public ActionResult GetPhoto(string id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("errorCode", (int)ErrorCodes.UserDoesNotExist);
            string userId = "";
            if (User.Identity.IsAuthenticated)
                userId = User.Identity.Name;
            if (!String.IsNullOrEmpty(id))
                userId = id;
            Dictionary<string, object> gcollection = new Dictionary<string, object>();
            gcollection.Add("uid", userId);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(gcollection));
            collection.Add("thumbnailURL", res.GetString("thumbnailURL", String.Empty));
            collection.Add("photoURL", res.GetString("photoURL", String.Empty));
            collection["errorCode"] = res.GetInt("errorCode", (int)ErrorCodes.UserDoesNotExist);
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        public ActionResult GetBalance()
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("balance", String.Format("{0} {1}", GlobalConfig.DefaultCurrency, "0.00"));
            collection.Add("shortbalance", String.Format("{0}{1}", "$", "0.00"));
            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                if (wallet != null)
                {
                    Currency currency = context.Currencies.FirstOrDefault(c => c.Code == wallet.Currency);
                    collection["balance"] = wallet.Currency + " " + wallet.Balance.ToString("F");
                }

                else
                    collection["balance"] = context.Countries.FirstOrDefault(c => c.Code == user.CountryCode).Currency.Code + " 0.00";
            }
            return this.Json(collection, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserInfo(string id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("errorCode", (int)ErrorCodes.UserDoesNotExist);
            string userId = "";
            var context = new IPTV2Entities();

            if (User.Identity.IsAuthenticated)
                userId = User.Identity.Name;
            if (!String.IsNullOrEmpty(id))
                userId = id;

            Dictionary<string, object> gcollection = new Dictionary<string, object>();
            gcollection.Add("uid", userId);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(gcollection));

            return Content(res.GetData().ToJsonString(), "application/json");
            //User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(userId));

            //Dictionary<string, object> gcollection = new Dictionary<string, object>();
            //gcollection.Add("uid", userId);
            //GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(gcollection));
            //collection.Add("thumbnailURL", res.GetString("thumbnailURL", String.Empty));
            //collection.Add("photoURL", res.GetString("photoURL", String.Empty));
            //collection.Add("fullName", "");
            //collection["errorCode"] = res.GetInt("errorCode", (int)ErrorCodes.UserDoesNotExist);
            //return Content(MyUtility.buildJson(collection), "application/json");
        }

        [OutputCache(VaryByParam = "id", Duration = 180)]
        public JsonResult GetSiteUserInfo(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { ErrorCode = ErrorCodes.UnknownError }, JsonRequestBehavior.AllowGet);
            }
            IPTV2Entities context = new IPTV2Entities();
            System.Guid userId = new Guid(id);

            var UserInfo = context.Users.Find(userId);
            if (UserInfo == null)
            {
                return Json(new { ErrorCode = ErrorCodes.UnknownError }, JsonRequestBehavior.AllowGet);
            }
            return Json(
                    new
                    {
                        ErrorCode = 0,
                        LastName = UserInfo.LastName,
                        FirstName = UserInfo.FirstName,
                        CountryCode = UserInfo.CountryCode,
                        Email = UserInfo.EMail
                    }
                  , JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangePassword()
        {
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            System.Guid userId = System.Guid.Parse(User.Identity.Name);
            var context = new IPTV2Entities();
            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
                ViewBag.User = user;
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("ChangePassword2", user);
            return View();
        }

        [HttpPost]
        public JsonResult _ChangePassword(FormCollection f)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            DateTime registDt = DateTime.Now;
            string currentPassword = f["Password"];
            string newPassword = f["NewPassword"];
            string confirmPassword = f["ConfirmPassword"];

            if (String.IsNullOrEmpty(currentPassword) || String.IsNullOrEmpty(newPassword) || String.IsNullOrEmpty(confirmPassword))
            {
                errorMessage = "Please fill in the required fields.";
                collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, errorMessage);
                return this.Json(collection, JsonRequestBehavior.AllowGet);
            }

            if (String.Compare(newPassword, confirmPassword, false) != 0)
            {
                errorMessage = "Password mismatch.";
                collection = MyUtility.setError(ErrorCodes.IsMismatchPassword, errorMessage);
                return this.Json(collection, JsonRequestBehavior.AllowGet);
            }

            try
            {
                System.Guid userId = System.Guid.Parse(User.Identity.Name);
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    string hashedPassword = MyUtility.GetSHA1(currentPassword);
                    string hashedNewPassword = MyUtility.GetSHA1(newPassword);
                    if (String.Compare(user.Password, hashedPassword, false) != 0)
                    {
                        errorMessage = "The current password you entered is incorrect. Please try again.";
                        collection = MyUtility.setError(ErrorCodes.IsMismatchPassword, errorMessage);
                        return this.Json(collection, JsonRequestBehavior.AllowGet);
                    }

                    user.Password = hashedNewPassword;
                    user.LastUpdated = registDt;

                    if (context.SaveChanges() > 0)
                    {
                        errorMessage = "Your password has been changed succcessfully.";
                        collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                    }
                    else
                    {
                        errorMessage = "The system encountered an unidentified error. Please try again.";
                        collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
                    }
                }
            }
            catch (Exception e) { collection = MyUtility.setError(ErrorCodes.UnknownError, e.Message); }
            return this.Json(collection, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult _ForgotPassword(FormCollection f)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            DateTime registDt = DateTime.Now;
            string email = f["EmailAddress"];

            if (String.IsNullOrEmpty(email))
            {
                errorMessage = "Please fill in the required fields.";
                collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, errorMessage);
                return this.Json(collection, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, email, true) == 0);
                if (user != null)
                {
                    //Guid randomGuid = System.Guid.NewGuid();
                    //string newPassword = randomGuid.ToString().Substring(0, 8);
                    //string hashedPassword = MyUtility.GetSHA1(newPassword);
                    //user.Password = hashedPassword;
                    //user.LastUpdated = registDt;

                    //if (context.SaveChanges() > 0)
                    //{
                    //    var mailer = new UserMailer();
                    //    var msg = mailer.ForgotPassword(to: user.EMail, password: newPassword);
                    //    msg.SendAsync();

                    //    errorMessage = "Your password has been changed succcessfully.";
                    //    collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                    //}

                    //else
                    //{
                    //    errorMessage = "The system encountered an unidentified error. Please try again.";
                    //    collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
                    //}

                    if (user.StatusId != 1)
                    {
                        errorMessage = "Email has not been verified.";
                        collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
                        return this.Json(collection, JsonRequestBehavior.AllowGet);
                    }

                    user.LastUpdated = registDt;
                    if (context.SaveChanges() > 0)
                    {
                        string oid = MyUtility.GetSHA1(String.Format("{0}{1}", user.UserId, user.LastUpdated));
                        string reset_pwd_email = String.Format("{0}/User/ResetPassword?key={1}&oid={2}", GlobalConfig.baseUrl, user.ActivationKey, oid.ToLower());
                        string emailBody = String.Format(GlobalConfig.ResetPasswordBodyTextOnly, user.FirstName, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), reset_pwd_email);
                        try
                        {
                            //MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody);
                            MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, "Reset your TFC.tv Password", emailBody, MailType.TextOnly, emailBody);
                            collection = MyUtility.setError(ErrorCodes.Success, "Instructions on how to reset your password has been sent.");
                        }
                        catch (Exception e)
                        {
                            collection = MyUtility.setError(ErrorCodes.UnknownError, e.InnerException.Message);
                        }
                    }
                }
                else
                {
                    errorMessage = "Email does not exist.";
                    collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, errorMessage);
                }
            }

            catch (Exception e) { collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, e.Message); }

            return this.Json(collection, JsonRequestBehavior.AllowGet);
        }

        public virtual void SetAutheticationCookie(string uid)
        {
            //FormsAuthentication.SetAuthCookie(uid, true);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(uid, true, GlobalConfig.FormsAuthenticationTimeout);
            //FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, uid, DateTime.Now, DateTime.Now.AddMonths(1), true, String.Empty, FormsAuthentication.FormsCookiePath);
            string encTicket = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.Expires = DateTime.Now.AddDays(30);
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
        }

        public virtual void SetSession(string uid)
        {
            if (GlobalConfig.IsPreventionOfMultipleLoginEnabled)
            {
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                var cache = DataCache.Cache;
                string cacheKey = "SESSIONID:U:" + ticket.Name.ToUpper();
                if (GlobalConfig.UseDaysBasedOnCacheDurationForSessionStore)
                    cache.Put(cacheKey, authCookie.Value, new TimeSpan(GlobalConfig.SessionStoreCacheDurationInDays, 0, 0, 0));
                else
                    cache.Put(cacheKey, authCookie.Value, new TimeSpan(ticket.Expiration.Ticks));
            }
        }

        //public ActionResult ResendVerification()
        //{
        //    if (MyUtility.isUserLoggedIn())
        //        return RedirectToAction("Index", "Home");

        //    return View();
        //}

        public ActionResult Verify(string key, string source)
        {

            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "Activation",
                TransactionType = "Activation"
            };


            string url = Url.Action("Index", "Home").ToString();
            try
            {
                if (String.IsNullOrEmpty(key))
                    ReturnCode.StatusMessage = "You are missing some required information.";
                else
                {

                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    var user = context.Users.FirstOrDefault(u => u.ActivationKey == new Guid(key));

                    if (user != null)
                    {
                        if (user.StatusId == 0 || user.StatusId == null)
                        {
                            user.StatusId = GlobalConfig.Visible;
                            user.DateVerified = DateTime.Now;
                            Guid userId = user.UserId;
                            if (context.SaveChanges() > 0)
                            {
                                //if (xoomPromo != null)
                                //{
                                //    var xoomUserPromo = context.UserPromos.FirstOrDefault(u => u.PromoId == GlobalConfig.Xoom2PromoId && u.UserId == user.UserId);
                                //    if (xoomUserPromo != null)
                                //    {
                                //        SetAutheticationCookie(user.UserId.ToString());

                                //        SendToGigya(user);
                                //        SetSession(user.UserId.ToString());
                                //        ContextHelper.SaveSessionInDatabase(context, user);

                                //        //add uid cookie
                                //        HttpCookie uidCookie = new HttpCookie("uid");
                                //        uidCookie.Value = user.UserId.ToString();
                                //        uidCookie.Expires = DateTime.Now.AddDays(30);
                                //        Response.Cookies.Add(uidCookie);
                                //        if (!Request.IsLocal)
                                //            if (user.IsTVERegistrant == null || user.IsTVERegistrant == false) //If user is TVE, do not send email
                                //                if (!String.IsNullOrEmpty("source"))
                                //                    MyUtility.SendConfirmationEmailAir(context, user);
                                //                else
                                //                    MyUtility.SendConfirmationEmail(context, user);

                                //        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                //        ReturnCode.StatusHeader = "Congratulations! You are now registered to TFC.tv.";
                                //        ReturnCode.StatusMessage = "You are one step away from claiming your free 1 Month Premium Subscription.";
                                //        //ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies!";
                                //        TempData["ErrorMessage"] = ReturnCode;
                                //        return RedirectToAction("Details", "Subscribe", new { id = "xoom" });
                                //    }
                                //}

                                bool IsEligibleForXoomPromo = false;
                                try
                                {
                                    var xoomPromo = context.Promos.FirstOrDefault(u => u.PromoId == GlobalConfig.Xoom2PromoId && u.StartDate < registDt && u.EndDate > registDt && u.StatusId == GlobalConfig.Visible);
                                    if (xoomPromo != null)
                                    {
                                        var xoomUserPromo = context.UserPromos.FirstOrDefault(u => u.PromoId == GlobalConfig.Xoom2PromoId && u.UserId == user.UserId);
                                        if (xoomUserPromo != null)
                                            IsEligibleForXoomPromo = true;
                                    }
                                }
                                catch (Exception) { }

                                if (user.IsTVERegistrant == null || user.IsTVERegistrant == false)
                                {
                                    int freeTrialProductId = 0;
                                    if (GlobalConfig.IsFreeTrialEnabled && !IsEligibleForXoomPromo)
                                    {
                                        freeTrialProductId = MyUtility.GetCorrespondingFreeTrialProductId();
                                        if (GlobalConfig.TfcTvFree2StartDate < registDt && GlobalConfig.TfcTvFree2EndDate > registDt)
                                        {
                                            string UserCountryCode = user.CountryCode;
                                            if (!GlobalConfig.isUAT)
                                                try { UserCountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy(); }
                                                catch (Exception) { }

                                            var countryList = GlobalConfig.TfcTvFree2CountryWhiteList.Split(',');
                                            if (countryList.Contains(UserCountryCode) && String.Compare(user.CountryCode, UserCountryCode, true) == 0)
                                                freeTrialProductId = GlobalConfig.TfcTvFree2ProductId;
                                        }
                                        if (Request.Cookies.AllKeys.Contains("vntycok") || GlobalConfig.Country14DayTrials.Contains(user.CountryCode))
                                            freeTrialProductId = GlobalConfig.FreeTrial14ProductId;
                                        PaymentHelper.PayViaWallet(context, userId, freeTrialProductId, SubscriptionProductType.Package, userId, null);
                                    }

                                    if (GlobalConfig.IsABSCBNFreeLiveStreamFreeOnRegistrationEnabled)
                                        if (GlobalConfig.ABSCBNFreeLiveStreamStartDate < registDt && GlobalConfig.ABSCBNFreeLiveStreamEndDate > registDt)
                                            PaymentHelper.PayViaWallet(context, userId, GlobalConfig.ABSCBNFreeLiveStreamProductId, SubscriptionProductType.Package, userId, null);

                                    SetAutheticationCookie(user.UserId.ToString());

                                    SendToGigya(user);
                                    SetSession(user.UserId.ToString());
                                    ContextHelper.SaveSessionInDatabase(context, user);

                                    //add uid cookie
                                    HttpCookie uidCookie = new HttpCookie("uid");
                                    uidCookie.Value = user.UserId.ToString();
                                    uidCookie.Expires = DateTime.Now.AddDays(30);
                                    Response.Cookies.Add(uidCookie);

                                    if (!Request.IsLocal)
                                        if (user.IsTVERegistrant == null || user.IsTVERegistrant == false) //If user is TVE, do not send email
                                            if (!String.IsNullOrEmpty(source))
                                                MyUtility.SendConfirmationEmailAir(context, user);
                                            else
                                                MyUtility.SendWelcomeEmail(context, user, freeTrialProductId);

                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                    ReturnCode.StatusHeader = "Your 7-Day Free Trial Starts Now!";
                                    if (Request.Cookies.AllKeys.Contains("vntycok") || GlobalConfig.Country14DayTrials.Contains(user.CountryCode))
                                    {
                                        ReturnCode.StatusHeader = "Your 14-Day Free Trial Starts Now!";
                                        HttpCookie vanCookie = new HttpCookie("vntycok");
                                        vanCookie.Expires = DateTime.Now.AddDays(-1);
                                        Response.Cookies.Add(vanCookie);
                                    }
                                    ReturnCode.StatusMessage = "Congratulations! You are now registered to TFC.tv.";
                                    ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies!";
                                    TempData["ErrorMessage"] = ReturnCode;
                                    if (!String.IsNullOrEmpty(source))
                                        return Redirect("/WatchNow"); //return RedirectToAction("Index", "Air"); //redirect to air if source is coming from project air

                                    //Eligible for Xoom Promo
                                    if (IsEligibleForXoomPromo)
                                    {
                                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                        ReturnCode.StatusHeader = "Congratulations! You are now registered to TFC.tv.";
                                        ReturnCode.StatusMessage = "You are one step away from claiming your free 1 Month Premium Subscription.";
                                        ReturnCode.StatusMessage2 = String.Empty;
                                        TempData["ErrorMessage"] = ReturnCode;
                                        return RedirectToAction("Details", "Subscribe", new { id = "xoom" });
                                    }

                                    //check preblack cookie
                                    if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("redirectlckbprea"))
                                    {
                                        HttpCookie preBlackCookie = new HttpCookie("redirectlckbprea");
                                        preBlackCookie.Expires = DateTime.Now.AddDays(-1);
                                        Response.Cookies.Add(preBlackCookie);
                                        return RedirectToAction("Details", "Subscribe", new { id = "lckbprea" });
                                    }
                                    if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("redirect3178"))
                                    {
                                        HttpCookie pacMayCookie = new HttpCookie("redirect3178");
                                        pacMayCookie.Expires = DateTime.Now.AddDays(-1);
                                        Response.Cookies.Add(pacMayCookie);
                                        //return Redirect("/Subscribe/mayweather-vs-pacquiao-may-3");  
                                        return RedirectToAction("Details", "Subscribe", new { id = "mayweather-vs-pacquiao-may-3" });
                                    }
                                    if (MyUtility.isTVECookieValid())
                                    {
                                        MyUtility.RemoveTVECookie();
                                        return RedirectToAction("RegisterToTFCEverywhere", "User");
                                    }

                                    return RedirectToAction("Index", "Home"); // successful verification
                                }
                            }
                            else
                                ReturnCode.StatusMessage = "The system encountered an unspecified error. Please contact Customer Support.";
                        }
                        else
                            ReturnCode.StatusMessage = "This email address has already been verified.";
                    }
                    else
                        ReturnCode.StatusMessage = "This email address does not exist.";
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return View("UXGenericError", ReturnCode);

            if (!Request.Cookies.AllKeys.Contains("version"))
            {
            }
            else
            {
                string result = String.Empty;
                if (String.IsNullOrEmpty(key))
                    result = "Missing required fields";

                else
                {
                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    var user = context.Users.FirstOrDefault(u => u.ActivationKey == new Guid(key));
                    if (user != null)
                    {
                        if (user.StatusId == 0 || user.StatusId == null)
                        {
                            user.StatusId = 1;
                            user.DateVerified = DateTime.Now;
                            var userId = user.UserId;

                            //var transaction = new RegistrationTransaction()
                            //{
                            //    RegisteredState = user.State,
                            //    RegisteredCity = user.City,
                            //    RegisteredCountryCode = user.CountryCode,
                            //    Amount = 0,
                            //    Currency = user.Country.CurrencyCode
                            //};
                            //user.Transactions.Add(transaction);

                            //If FreeTrial is enabled, insert free trial.
                            if (context.SaveChanges() > 0)
                            {
                                //if (GlobalConfig.IsFreeTrialEnabled)
                                //    PaymentHelper.PayViaWallet(context, userId, GlobalConfig.FreeTrial7ProductId, SubscriptionProductType.Package, userId, null);

                                /****** DEC 31 2012 *****/

                                //UPDATE: FEB 18, 2013
                                if (user.IsTVERegistrant == null || user.IsTVERegistrant == false)
                                {
                                    int freeTrialProductId = 0;
                                    if (GlobalConfig.IsFreeTrialEnabled)
                                    {
                                        freeTrialProductId = MyUtility.GetCorrespondingFreeTrialProductId();
                                        if (GlobalConfig.TfcTvFree2StartDate < registDt && GlobalConfig.TfcTvFree2EndDate > registDt)
                                        {
                                            string UserCountryCode = user.CountryCode;
                                            if (!GlobalConfig.isUAT)
                                                try { UserCountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy(); }
                                                catch (Exception) { }

                                            var countryList = GlobalConfig.TfcTvFree2CountryWhiteList.Split(',');
                                            if (countryList.Contains(UserCountryCode) && String.Compare(user.CountryCode, UserCountryCode, true) == 0)
                                                freeTrialProductId = GlobalConfig.TfcTvFree2ProductId;
                                        }
                                        PaymentHelper.PayViaWallet(context, userId, freeTrialProductId, SubscriptionProductType.Package, userId, null);
                                    }

                                    if (GlobalConfig.IsABSCBNFreeLiveStreamFreeOnRegistrationEnabled)
                                        if (GlobalConfig.ABSCBNFreeLiveStreamStartDate < registDt && GlobalConfig.ABSCBNFreeLiveStreamEndDate > registDt)
                                            PaymentHelper.PayViaWallet(context, userId, GlobalConfig.ABSCBNFreeLiveStreamProductId, SubscriptionProductType.Package, userId, null);
                                }


                                result = "Successful";
                                SetAutheticationCookie(user.UserId.ToString());
                                TempData["isConnectedToSocialNetworks"] = false;
                                if (!Request.IsLocal)
                                    if (user.IsTVERegistrant == null || user.IsTVERegistrant == false) //If user is TVE, do not send email
                                        MyUtility.SendConfirmationEmail(context, user);
                                return RedirectToAction("RegisterConfirm", "User");
                            }
                            else
                                result = "Not successful";
                        }
                        else
                            result = "This email is already activated.";
                    }
                    else
                        result = "User does not exist";
                }
                ViewBag.ErrorMessage = result;
                return Content(result, "text/plain");
            }
        }

        private void FlagBetaKey(string iid)
        {
            if (!String.IsNullOrEmpty(iid))
            {
                var context = new IPTV2Entities();
                var tester = context.BetaTesters.FirstOrDefault(b => b.InvitationKey == new System.Guid(iid) && b.DateClaimed == null);
                if (tester != null)
                {
                    tester.DateClaimed = DateTime.Now;
                    tester.IpAddress = Request.GetUserHostAddressFromCloudflare();
                    context.SaveChanges();
                }
            }
        }

        public ActionResult RegisterViaSocial()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return RedirectToAction("Register", "User");
            if (MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");
            return View();
        }

        public ActionResult RegisterViaTFC()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return RedirectToAction("Register", "User");
            if (MyUtility.isUserLoggedIn())
            {
                if (TempData["TFCnowCustomer"] != null)
                    return RedirectToAction("ListEntitlements", "Migration");
                return RedirectToAction("Index", "Home");
            }

            var userHostAddress = Request.GetUserHostAddressFromCloudflare();
            var context = new IPTV2Entities();
            // Remove EU ME ---
            var ExcludedCountriesFromRegistrationDropDown = GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',');
            List<Country> countries = null;
            if (GlobalConfig.UseCountryListInMemory)
            {
                if (GlobalConfig.CountryList != null)
                    countries = GlobalConfig.CountryList.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                else
                    countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
            }
            else
                countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
            ViewBag.ListOfCountries = countries;
            //Get User Country by Ip
            var country = MyUtility.getCountry(userHostAddress);
            int stateCount = 0;
            if (country != null)
            {
                var userCountry = country.getCode();
                if (String.IsNullOrEmpty(userCountry) || userCountry == "--")
                    userCountry = GlobalConfig.DefaultCountry;
                var uCountry = context.Countries.FirstOrDefault(c => c.Code == userCountry);
                ViewBag.UserCountry = uCountry;
                ViewBag.IsConnectedToSocialNetwork = false;

                stateCount = context.States.Count(s => s.CountryCode == userCountry);
                ViewBag.StateCount = stateCount;
            }

            if (TempData["qs"] != null)
            {
                ViewBag.IsConnectedToSocialNetwork = true;
                NameValueCollection qs = (NameValueCollection)TempData["qs"];
                TempData["qs"] = qs;
                if (!String.IsNullOrEmpty(qs["UID"]))
                    ViewBag.isConnectedToGigya = true; //Connected to Gigya. Get userInfo on View. Fill registrationSocial with userInfo from social network.

                SignUpModel model = new SignUpModel();
                model.Email = (String.IsNullOrEmpty(qs["email"])) ? String.Empty : HttpUtility.UrlDecode(qs["email"]);
                model.FirstName = (String.IsNullOrEmpty(qs["firstName"])) ? String.Empty : HttpUtility.UrlDecode(qs["firstName"]);
                model.LastName = (String.IsNullOrEmpty(qs["lastName"])) ? String.Empty : HttpUtility.UrlDecode(qs["lastName"]);
                model.CountryCode = (String.IsNullOrEmpty(qs["country"])) ? String.Empty : HttpUtility.UrlDecode(qs["country"]);

                return View(model);
            }
            else
            {
                if (TempData["TFCnowCustomer"] != null)
                {
                    Customer customer = (Customer)TempData["TFCnowCustomer"];
                    if (customer != null)
                    {
                        TempData["TFCnowCustomer"] = customer;
                        SignUpModel model = new SignUpModel();
                        model.Email = customer.EmailAddress;
                        model.FirstName = customer.FirstName;
                        model.LastName = customer.LastName;
                        return View(model);
                    }
                }
            }

            return View();
        }

        public ActionResult RegisterConfirm()
        {
            ViewBag.isConnectedToSocialNetworks = false;
            if (TempData["isConnectedToSocialNetworks"] != null)
                ViewBag.isConnectedToSocialNetworks = (bool)TempData["isConnectedToSocialNetworks"];


            if (MyUtility.isUserLoggedIn())
            {
                var context = new IPTV2Entities();
                var userId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                    if (user.IsTVERegistrant != null)
                        if ((bool)user.IsTVERegistrant)
                            if (user.IsTVEverywhere != null)
                            {
                                if ((bool)user.IsTVEverywhere)
                                    return RedirectToAction("RegistrationCompleteTFCEverywhere", "User");
                                else
                                    return RedirectToAction("RegisterToTFCEverywhere", "User");
                            }
                            else
                                return RedirectToAction("RegisterToTFCEverywhere", "User");
            }

            return View();
        }

        public ActionResult RegisterVerify()
        {
            return View();
        }

        [HttpPost]
        public ActionResult _RegisterUser(FormCollection fc)
        {
            //fc["Email"] = "Albin_Lim@abs-cbn.com";
            //fc["Password"] = "mokmok123";
            //fc["ConfirmPassword"] = "mokmok123";
            //fc["FirstName"] = "Albin";
            //fc["LastName"] = "Lim";
            //fc["CountryCode"] = "US";
            //fc["City"] = "CA";
            //fc["State"] = "CA";

            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            bool isConnectedToSocialNetworks = false;
            string href = "/User/RegisterVerify";

            if (MyUtility.isUserLoggedIn()) //User is logged in.
                return RedirectToAction("Index", "Home");
            if (String.IsNullOrEmpty(fc["Email"]))
            {
                collection = MyUtility.setError(ErrorCodes.IsEmailEmpty);
                return Content(MyUtility.buildJson(collection), "application/json");
            }
            if (String.Compare(fc["Password"], fc["ConfirmPassword"], false) != 0)
            {
                collection = MyUtility.setError(ErrorCodes.IsMismatchPassword);
                return Content(MyUtility.buildJson(collection), "application/json");
            }
            if (String.IsNullOrEmpty(fc["FirstName"]) || String.IsNullOrEmpty(fc["LastName"]) || String.IsNullOrEmpty(fc["CountryCode"]))
            {
                collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields);
                return Content(MyUtility.buildJson(collection), "application/json");
            }

            RegexUtilities util = new RegexUtilities();
            //if (!MyUtility.isEmail(fc["Email"]))
            if (!util.IsValidEmail(fc["Email"]))
            {
                collection = MyUtility.setError(ErrorCodes.IsNotValidEmail);
                return Content(MyUtility.buildJson(collection), "application/json");
            }

            try
            {
                string FirstName = fc["FirstName"];
                string LastName = fc["LastName"];
                string CountryCode = fc["CountryCode"];
                string EMail = fc["Email"];
                string Password = fc["Password"];
                string City = fc["City"];
                string State = String.IsNullOrEmpty(fc["State"]) ? fc["StateDD"] : fc["State"];
                System.Guid userId = System.Guid.NewGuid();
                string provider = "tfctv";

                if (FirstName.Length > 32)
                {
                    collection = MyUtility.setError(ErrorCodes.LimitReached, "First Name cannot exceed 32 characters.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                if (LastName.Length > 32)
                {
                    collection = MyUtility.setError(ErrorCodes.LimitReached, "Last Name cannot exceed 32 characters.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                if (EMail.Length > 64)
                {
                    collection = MyUtility.setError(ErrorCodes.LimitReached, "Email cannot exceed 64 characters.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                if (!String.IsNullOrEmpty(State))
                    if (State.Length > 30)
                    {
                        collection = MyUtility.setError(ErrorCodes.LimitReached, "State cannot exceed 30 characters.");
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }
                if (!String.IsNullOrEmpty(City))
                    if (City.Length > 50)
                    {
                        collection = MyUtility.setError(ErrorCodes.LimitReached, "City cannot exceed 50 characters.");
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }

                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EMail, true) == 0);
                if (user != null)
                {
                    collection = MyUtility.setError(ErrorCodes.IsExistingEmail);
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                /***** CHECK FOR COUNTRY CODE ****/
                if (context.Countries.Count(c => String.Compare(c.Code, CountryCode, true) == 0) <= 0)
                {
                    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "Country Code is invalid.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                else if (GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',').Contains(CountryCode))
                {
                    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "Country Code is invalid.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                if (String.IsNullOrEmpty(State))
                {
                    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "State is required.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                else
                {
                    if (context.States.Count(c => c.CountryCode == CountryCode.ToUpper()) > 0)
                    {
                        if (context.States.Count(s => s.CountryCode == CountryCode.ToUpper() && (s.StateCode == State || s.Name == State)) == 0)
                        {
                            collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "State is invalid.");
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                    }
                }


                DateTime registDt = DateTime.Now;
                user = new User()
                {
                    UserId = userId,
                    FirstName = FirstName,
                    LastName = LastName,
                    City = City,
                    State = State,
                    CountryCode = CountryCode,
                    EMail = EMail,
                    Password = MyUtility.GetSHA1(Password),
                    GigyaUID = userId.ToString(),
                    RegistrationDate = registDt,
                    LastUpdated = registDt,
                    RegistrationIp = Request.GetUserHostAddressFromCloudflare(),
                    StatusId = 0,
                    ActivationKey = Guid.NewGuid()
                };


                //UPDATE: FEB 18, 2013
                if (MyUtility.isTVECookieValid())
                    user.IsTVERegistrant = true;

                string CurrencyCode = GlobalConfig.DefaultCurrency;
                Country country = context.Countries.FirstOrDefault(c => c.Code == CountryCode);
                if (country != null)
                {
                    Currency currency = context.Currencies.FirstOrDefault(c => c.Code == country.CurrencyCode);
                    if (currency != null) CurrencyCode = currency.Code;
                }
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == CurrencyCode);
                if (wallet == null) // Wallet does not exist. Create new wallet for User.
                {
                    wallet = ContextHelper.CreateWallet(0, CurrencyCode, registDt);
                    user.UserWallets.Add(wallet);
                }

                var transaction = new RegistrationTransaction()
                {
                    RegisteredState = user.State,
                    RegisteredCity = user.City,
                    RegisteredCountryCode = user.CountryCode,
                    Amount = 0,
                    Currency = CurrencyCode,
                    Reference = "New Registration",
                    Date = registDt,
                    OfferingId = GlobalConfig.offeringId,
                    UserId = user.UserId,
                    StatusId = GlobalConfig.Visible
                };

                user.Transactions.Add(transaction);

                context.Users.Add(user);
                if (context.SaveChanges() > 0)
                {
                    if (TempData["qs"] != null)
                    {
                        NameValueCollection qs = (NameValueCollection)TempData["qs"];
                        Dictionary<string, object> GigyaCollection = new Dictionary<string, object>();
                        collection.Add("uid", qs["UID"]);
                        collection.Add("siteUID", userId);
                        collection.Add("cid", String.Format("{0} - New User", qs["provider"]));
                        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(collection));
                        provider = qs["provider"];
                        isConnectedToSocialNetworks = true;
                        if (res.GetErrorCode() == 0)
                        {
                            if (GlobalConfig.EnableNotifyLoginOnLinkAccount)
                            {
                                Dictionary<string, object> userInfo = new Dictionary<string, object>();
                                userInfo.Add("firstName", user.FirstName);
                                userInfo.Add("lastName", user.LastName);
                                userInfo.Add("email", user.EMail);
                                Dictionary<string, object> gigyaCollection = new Dictionary<string, object>();
                                gigyaCollection.Add("siteUID", user.UserId);
                                gigyaCollection.Add("cid", String.Format("TFCTV - Login - {0}", qs["provider"]));
                                gigyaCollection.Add("sessionExpiration", "0");
                                gigyaCollection.Add("userInfo", MyUtility.buildJson(userInfo));
                                GSResponse notifyLogin = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(gigyaCollection));
                                GigyaHelpers.setCookie(notifyLogin, this.ControllerContext);
                            }
                        }

                    }
                    else
                    {
                        Dictionary<string, object> userInfo = new Dictionary<string, object>();
                        userInfo.Add("firstName", user.FirstName);
                        userInfo.Add("lastName", user.LastName);
                        userInfo.Add("email", user.EMail);
                        Dictionary<string, object> gigyaCollection = new Dictionary<string, object>();
                        gigyaCollection.Add("siteUID", user.UserId);
                        gigyaCollection.Add("cid", "TFCTV - Registration");
                        gigyaCollection.Add("sessionExpiration", "0");
                        gigyaCollection.Add("newUser", true);
                        gigyaCollection.Add("userInfo", MyUtility.buildJson(userInfo));
                        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(gigyaCollection));
                        GigyaHelpers.setCookie(res, this.ControllerContext);
                    }

                    //setUserData
                    User usr = context.Users.FirstOrDefault(u => u.EMail == EMail);
                    setUserData(usr.UserId.ToString(), usr);
                    var ActivationKey = usr.ActivationKey;

                    bool isTFCnowCustomer = false;

                    if (TempData["TFCnowCustomer"] != null)
                    {
                        Customer customer = (Customer)TempData["TFCnowCustomer"];
                        usr.StatusId = 1;
                        usr.DateVerified = registDt;
                        TempData["TFCnowCustomer"] = customer;
                        href = "/Migration/Migrate";
                        if (context.SaveChanges() > 0)
                        {
                            //SetAutheticationCookie(userId.ToString());
                            isTFCnowCustomer = true;
                        }
                    }

                    if (isConnectedToSocialNetworks)
                    {
                        usr.StatusId = 1;
                        usr.DateVerified = registDt;
                        context.SaveChanges();
                    }

                    //If FreeTrial is enabled, insert free trial.
                    //if (GlobalConfig.IsFreeTrialEnabled)
                    //{
                    //    context = new IPTV2Entities();
                    //    if (isConnectedToSocialNetworks)
                    //        PaymentHelper.PayViaWallet(context, userId, GlobalConfig.FreeTrial14ProductId, SubscriptionProductType.Package, userId, null);
                    //    else
                    //        PaymentHelper.PayViaWallet(context, userId, GlobalConfig.FreeTrial7ProductId, SubscriptionProductType.Package, userId, null);
                    //    context.SaveChanges();
                    //}

                    /***** DEC 31 2012 ****/
                    //UPDATED: FEB 18, 2013 - To include checking for TVE
                    if (usr.IsTVERegistrant == null || usr.IsTVERegistrant == false)
                    {
                        int freeTrialProductId = 0;
                        if (GlobalConfig.IsFreeTrialEnabled)
                        {
                            freeTrialProductId = MyUtility.GetCorrespondingFreeTrialProductId();
                            context = new IPTV2Entities();
                            if (GlobalConfig.TfcTvFree2StartDate < registDt && GlobalConfig.TfcTvFree2EndDate > registDt)
                            {
                                string UserCountryCode = usr.CountryCode;
                                if (!GlobalConfig.isUAT)
                                    try { UserCountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy(); }
                                    catch (Exception) { }

                                var countryList = GlobalConfig.TfcTvFree2CountryWhiteList.Split(',');
                                if (countryList.Contains(UserCountryCode) && String.Compare(user.CountryCode, UserCountryCode, true) == 0)
                                    freeTrialProductId = GlobalConfig.TfcTvFree2ProductId;
                            }
                            if (isConnectedToSocialNetworks)
                                PaymentHelper.PayViaWallet(context, userId, freeTrialProductId, SubscriptionProductType.Package, userId, null);
                        }
                    }


                    //Publish to Activity Feed
                    List<ActionLink> actionlinks = new List<ActionLink>();
                    actionlinks.Add(new ActionLink() { text = SNSTemplates.register_actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_actionlink_href) });
                    //mediaItem
                    List<MediaItem> mediaItems = new List<MediaItem>();
                    mediaItems.Add(new MediaItem() { type = SNSTemplates.register_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.register_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_mediaitem_href) });
                    UserAction action = new UserAction()
                    {
                        actorUID = userId.ToString(),
                        userMessage = SNSTemplates.register_usermessage,
                        title = SNSTemplates.register_title,
                        subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_subtitle),
                        linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_linkback),
                        description = String.Format(SNSTemplates.register_description, FirstName),
                        actionLinks = actionlinks,
                        mediaItems = mediaItems
                    };

                    GigyaMethods.PublishUserAction(action, userId, "external");
                    action.userMessage = String.Empty;
                    action.title = String.Empty;
                    action.mediaItems = null;
                    GigyaMethods.PublishUserAction(action, userId, "internal");
                    var email_err = String.Empty;
                    //FormsAuthentication.SetAuthCookie(userId.ToString(), true);
                    if (isConnectedToSocialNetworks)
                    {
                        //SetAutheticationCookie(userId.ToString());
                        if (!Request.IsLocal)
                        {
                            try { MyUtility.SendConfirmationEmail(context, usr); }
                            catch (Exception) { }
                        }

                        href = GlobalConfig.RegistrationConfirmPage;
                        //UPDATED: FEB 18, 2013
                        if (usr.IsTVERegistrant != null)
                            if ((bool)usr.IsTVERegistrant)
                            {
                                href = GlobalConfig.TVERegistrationPage;
                                MyUtility.RemoveTVECookie();
                            }
                    }
                    else
                    {
                        if (!isTFCnowCustomer)
                        {
                            //string emailBody = String.Format("Copy and paste this url to activate your TFC.tv Account {0}/User/Verify?email={1}&key={2}", GlobalConfig.baseUrl, usr.EMail, ActivationKey.ToString());
                            string verification_email = String.Format("{0}/User/Verify?key={1}", GlobalConfig.baseUrl, usr.ActivationKey.ToString());
                            string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, usr.FirstName, usr.EMail, verification_email);
                            //MyUtility.SendEmailViaSendGrid(usr.EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody);
                            if (!Request.IsLocal)
                                try
                                {
                                    //MyUtility.SendEmailViaSendGrid(usr.EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody);
                                    MyUtility.SendEmailViaSendGrid(usr.EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody, MailType.TextOnly, emailBody);
                                }
                                catch (Exception)
                                {
                                    email_err = " But we are not able to send the verification email.";
                                }
                        }
                    }

                    ////UPDATED: FEB 12, 2012
                    //if (!String.IsNullOrEmpty(fc["TVEverywhere"]))
                    //{
                    //    if (String.Compare(fc["TVEverywhere"], "0", true) == 0)
                    //    {
                    //        TempData["tempUserId"] = userId;
                    //        href = GlobalConfig.TVERegistrationPage;
                    //        TempData["isConnectedToSocialNetworks"] = isConnectedToSocialNetworks;
                    //    }
                    //}

                    if (usr.StatusId == GlobalConfig.Visible) //UPDATED: MARCH 1, 2013. Only set Authentication Cookie when user is verified.
                        SetAutheticationCookie(userId.ToString());
                    errorMessage = "Thank you! You are now registered to TFC.tv!" + email_err;
                    collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                    collection.Add("info", String.Format("{0}|{1}|{2}", user.EMail, Request.GetUserHostAddressFromCloudflare(), provider));
                    collection.Add("href", href);

                    FlagBetaKey(fc["iid"]);
                }
                else
                {
                    errorMessage = "The system encountered an unidentified error. Please try again.";
                    collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
                }
            }
            catch (Exception e)
            {
                collection = MyUtility.setError(ErrorCodes.EntityUpdateError, e.InnerException.InnerException.Message + "<br/>" + e.InnerException.InnerException.StackTrace);
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        public ActionResult EnrollSmartPit()
        {
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            var context = new IPTV2Entities();
            var userId = new Guid(User.Identity.Name);
            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return RedirectToAction("Index", "Home");
            if (user.Country == null)
                return RedirectToAction("Index", "Home");
            if (user.Country.Code != GlobalConfig.JapanCountryCode)
                return RedirectToAction("Index", "Home");
            if (!String.IsNullOrEmpty(user.SmartPitId))
            {
                ViewBag.SmartPitId = user.SmartPitId;
                return View("EnrolledSmartPit");
            }

            return View();
        }

        public ActionResult _CreateSmartPit(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            if (!MyUtility.isUserLoggedIn())
            {
                collection = MyUtility.setError(ErrorCodes.NotAuthenticated);
                return Content(MyUtility.buildJson(collection), "application/json");
            }

            var context = new IPTV2Entities();
            var userId = new Guid(User.Identity.Name);

            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                collection = MyUtility.setError(ErrorCodes.UnknownError, "User does not exist");
                return Content(MyUtility.buildJson(collection), "application/json");
            }

            if (user.Country == null)
            {
                collection = MyUtility.setError(ErrorCodes.UnknownError, "User's country is missing.");
                return Content(MyUtility.buildJson(collection), "application/json");
            }
            if (user.Country.Code != GlobalConfig.JapanCountryCode)
            {
                collection = MyUtility.setError(ErrorCodes.UnknownError, "User is not authorized.");
                return Content(MyUtility.buildJson(collection), "application/json");
            }
            if (!String.IsNullOrEmpty(user.SmartPitId))
            {
                collection = MyUtility.setError(ErrorCodes.UnknownError, "User is already enrolled.");
                return Content(MyUtility.buildJson(collection), "application/json");
            }

            DateTime registDt = DateTime.Now;
            var smartpitcardno = String.IsNullOrEmpty(fc["SmartPitCardNumber"]) ? String.Empty : fc["SmartPitCardNumber"];

            var service = new GomsTfcTv();
            try
            {
                var response = service.EnrollSmartPit(context, userId, smartpitcardno);

                if (response.IsSuccess)
                {
                    user.SmartPitId = response.SmartPitCardNo;
                    user.SmartPitRegistrationDate = registDt;
                    if (context.SaveChanges() > 0)
                    {
                        collection = MyUtility.setError(ErrorCodes.Success, response.StatusMessage);
                        collection.Add("spcno", response.SmartPitCardNo);
                    }
                    else
                        collection = MyUtility.setError(ErrorCodes.EntityUpdateError, "Unable to process your request. Please try again later.");
                }
                else
                    collection = MyUtility.setError(ErrorCodes.EntityUpdateError, response.StatusMessage);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                collection = MyUtility.setError(ErrorCodes.UnknownError, String.Format("{0} {1}", e.Message, e.InnerException == null ? String.Empty : e.InnerException.Message));
            }

            return Content(MyUtility.buildJson(collection), "application/json");
        }

        public ActionResult _ResendVerification(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection = MyUtility.setError(ErrorCodes.UnknownError);

            string result = String.Empty;
            string email = fc["EmailAddress"];
            if (!String.IsNullOrEmpty(email))
            {
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, email, true) == 0);
                if (user != null)
                {
                    if (user.StatusId == 1)
                        collection = MyUtility.setError(ErrorCodes.UnknownError, "This email is already activated.");
                    else
                    {
                        //string emailBody = String.Format("Copy and paste this url to activate your TFC.tv Account {0}/User/Verify?email={1}&key={2}", GlobalConfig.baseUrl, user.EMail, user.ActivationKey.ToString());
                        string verification_email = String.Format("{0}/User/Verify?key={1}", GlobalConfig.baseUrl, user.ActivationKey.ToString());
                        string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, user.FirstName, user.EMail, verification_email);
                        try
                        {
                            //MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody);
                            int productId = MyUtility.StringToIntList(GlobalConfig.FreeTrialProductIdsNEW).First();
                            if (string.Compare(user.CountryCode, "TW") == 0 || Request.Cookies.AllKeys.Contains("vntycok")) //set to config contains country14day
                                productId = GlobalConfig.FreeTrial14ProductId;

                            MyUtility.SendVerificationEmail(context, user, productId);
                            collection = MyUtility.setError(ErrorCodes.Success, "The verification email has been sent.");
                        }
                        catch (Exception e)
                        {
                            collection = MyUtility.setError(ErrorCodes.UnknownError, e.InnerException.Message);
                        }
                    }
                }
                else
                    collection = MyUtility.setError(ErrorCodes.UnknownError, "Email address is not registered to TFC.tv");
            }
            else
                collection = MyUtility.setError(ErrorCodes.UnknownError, "You did not enter an email address.");
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        [RequireHttps]
        public ActionResult ResetPassword(string key, string oid)
        {

            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            string ErrorMessage = String.Empty;
            try
            {
                if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(oid))
                {
                    ErrorMessage = "The page you requested is not valid.";
                    ViewBag.Err = ErrorMessage;
                    return View("ResetPassword2");
                }

                DateTime registDt = DateTime.Now;
                Guid temp_key;
                try
                {
                    temp_key = Guid.Parse(key);
                }
                catch (Exception) { ErrorMessage = "The page you requested has an invalid key (key)."; }

                if (String.IsNullOrEmpty(ErrorMessage))
                {
                    Guid activation_key = new Guid(key);
                    var context = new IPTV2Entities();
                    var user = context.Users.FirstOrDefault(u => u.ActivationKey == activation_key);
                    if (user != null)
                    {
                        if (registDt.Subtract(user.LastUpdated).TotalSeconds > 86400) // has not expired (24 hours)
                        {
                            ErrorMessage = "The page you requested has already expired.";
                            ViewBag.Err = ErrorMessage;
                            return View("ResetPassword2");
                        }

                        string oid_base = MyUtility.GetSHA1(String.Format("{0}{1}", user.UserId, user.LastUpdated));
                        if (String.Compare(oid, oid_base, true) != 0)
                        {
                            ErrorMessage = "The page you requested has an invalid key (oid).";
                            ViewBag.Err = ErrorMessage;
                            return View("ResetPassword2");
                        }

                        ViewBag.oid = oid;
                        ViewBag.key = key;
                        ViewBag.ts = MyUtility.ConvertToTimestamp(registDt);
                        ViewBag.User = user;
                    }
                    else
                        ErrorMessage = "User does not exist.";
                }
            }
            catch (Exception e) { MyUtility.LogException(e); ErrorMessage = "There was a problem retrieving this page."; }
            ViewBag.Err = ErrorMessage;
            return View("ResetPassword2");

            //if (MyUtility.isUserLoggedIn())
            //    return RedirectToAction("Index", "Home");
            //Dictionary<string, object> collection = new Dictionary<string, object>();
            //ErrorCodes errorCode = ErrorCodes.UnknownError;
            //string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            //collection = MyUtility.setError(errorCode, errorMessage);
            //DateTime registDt = DateTime.Now;

            //if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(oid))
            //{
            //    errorMessage = "The page you requested is not valid.";
            //    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, errorMessage);
            //    return this.Json(collection, JsonRequestBehavior.AllowGet);
            //}

            //try
            //{
            //    Guid tempkey;
            //    tempkey = Guid.Parse(key);
            //}
            //catch (Exception)
            //{
            //    errorMessage = "The page you requested is not valid.";
            //    collection = MyUtility.setError(ErrorCodes.UnknownError, errorMessage);
            //    return this.Json(collection, JsonRequestBehavior.AllowGet);
            //}

            //try
            //{
            //    var context = new IPTV2Entities();
            //    var activationKey = new Guid(key);
            //    User user = context.Users.FirstOrDefault(u => u.ActivationKey == activationKey);
            //    if (user != null)
            //    {
            //        ViewBag.User = user;

            //        if (registDt.Subtract(user.LastUpdated).TotalSeconds > 86400)
            //        {
            //            errorMessage = "The page you requested has already expired.";
            //            collection = MyUtility.setError(ErrorCodes.UnknownError, errorMessage);
            //            return this.Json(collection, JsonRequestBehavior.AllowGet);
            //        }

            //        string oid_base = MyUtility.GetSHA1(String.Format("{0}{1}", user.UserId, user.LastUpdated));
            //        if (String.Compare(oid, oid_base, true) != 0)
            //        {
            //            errorMessage = "The page you requested is not valid.";

            //            collection = MyUtility.setError(ErrorCodes.UnknownError, errorMessage);
            //            collection.Add("oid", oid);
            //            collection.Add("oid_base", oid_base);
            //            return this.Json(collection, JsonRequestBehavior.AllowGet);
            //        }

            //        //Guid randomGuid = System.Guid.NewGuid();
            //        //string newPassword = randomGuid.ToString().Substring(0, 8);
            //        //string hashedPassword = MyUtility.GetSHA1(newPassword);
            //        //user.Password = hashedPassword;
            //        //user.LastUpdated = registDt;

            //        //if (context.SaveChanges() > 0)
            //        //{
            //        //    errorMessage = "Your password has been changed succcessfully.";
            //        //    collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
            //        //}
            //        //else
            //        //{
            //        //    errorMessage = "The system encountered an unidentified error. Please try again.";
            //        //    collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
            //        //}
            //        ViewBag.oid = oid;
            //        ViewBag.key = key;
            //        ViewBag.ts = MyUtility.ConvertToTimestamp(DateTime.Now);
            //        return View();
            //    }

            //    else
            //    {
            //        errorMessage = "The page you requested is not valid.";
            //        collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, errorMessage);
            //    }
            //}
            //catch (Exception e) { collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, e.Message); }
            //return View();
        }

        [HttpPost]
        public ActionResult _ResetPassword(FormCollection fc)
        {
            if (MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            string key = fc["key"];
            string oid = fc["oid"];
            string password = fc["NewPassword"];
            string confirm_password = fc["ConfirmPassword"];

            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            DateTime registDt = DateTime.Now;

            if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(oid))
            {
                errorMessage = "The page you requested is not valid.";
                collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, errorMessage);
                return this.Json(collection, JsonRequestBehavior.AllowGet);
            }

            try
            {
                Guid tempkey;
                tempkey = Guid.Parse(key);
            }
            catch (Exception)
            {
                errorMessage = "The page you requested is not valid.";
                collection = MyUtility.setError(ErrorCodes.UnknownError, errorMessage);
                return this.Json(collection, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var context = new IPTV2Entities();
                var activationKey = new Guid(key);
                User user = context.Users.FirstOrDefault(u => u.ActivationKey == activationKey);
                if (user != null)
                {
                    if (registDt.Subtract(user.LastUpdated).TotalSeconds > 86400)
                    {
                        errorMessage = "The page you requested has already expired.";
                        collection = MyUtility.setError(ErrorCodes.UnknownError, errorMessage);
                        return this.Json(collection, JsonRequestBehavior.AllowGet);
                    }

                    string oid_base = MyUtility.GetSHA1(String.Format("{0}{1}", user.UserId, user.LastUpdated));
                    if (String.Compare(oid, oid_base, true) != 0)
                    {
                        errorMessage = "The page you requested is not valid.";
                        collection = MyUtility.setError(ErrorCodes.UnknownError, errorMessage);
                        return this.Json(collection, JsonRequestBehavior.AllowGet);
                    }

                    if (String.Compare(password, confirm_password, false) != 0)
                    {
                        errorMessage = "Password mismatch!";
                        collection = MyUtility.setError(ErrorCodes.IsMismatchPassword, errorMessage);
                        return this.Json(collection, JsonRequestBehavior.AllowGet);
                    }

                    //Guid randomGuid = System.Guid.NewGuid();
                    //string newPassword = randomGuid.ToString().Substring(0, 8);
                    string hashedPassword = MyUtility.GetSHA1(password);
                    user.Password = hashedPassword;
                    user.LastUpdated = registDt;

                    if (context.SaveChanges() > 0)
                    {
                        errorMessage = "Your password has been changed succcessfully.";
                        collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                    }
                    else
                    {
                        errorMessage = "The system encountered an unidentified error. Please try again.";
                        collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
                    }
                }

                else
                {
                    errorMessage = "The page you requested is not valid.";
                    collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, errorMessage);
                }
            }
            catch (Exception e) { collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, e.Message); }
            return this.Json(collection, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult _ClaimTVE(FormCollection fc, string EmailAddress, string MacAddressOrSmartCard, string AccountNumber, string ActivationCode)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            try
            {
                DateTime registDt = DateTime.Now;

                var context = new IPTV2Entities();

                var userId = new Guid("E21D87F3-5940-451A-B7EE-ADA4F3CEC234");

                if (!String.IsNullOrEmpty(EmailAddress))
                {
                    var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                    if (user != null)
                    {
                        userId = user.UserId;
                        string CurrencyCode = GlobalConfig.DefaultCurrency;
                        Country country = context.Countries.FirstOrDefault(c => c.Code == user.CountryCode);
                        if (country != null)
                        {
                            Currency currency = context.Currencies.FirstOrDefault(c => c.Code == country.CurrencyCode);
                            if (currency != null) CurrencyCode = currency.Code;
                        }

                        var transaction = new TfcEverywhereTransaction()
                        {
                            Amount = 0,
                            Date = DateTime.Now,
                            Currency = CurrencyCode,
                            OfferingId = GlobalConfig.offeringId,
                            StatusId = GlobalConfig.Visible,
                            Reference = "TFC Everywhere - Activate"
                        };

                        //string MacAddressOrSmartCard = "00172F0108FC";
                        //string AccountNumber = "US-000179857";
                        //string ActivationCode = "5S3UDP";
                        var gomsService = new GomsTfcTv();
                        var response = gomsService.ClaimTVEverywhere(context, userId, transaction, MacAddressOrSmartCard, AccountNumber, ActivationCode);
                        if (response.IsSuccess)
                        {

                            //ADD Entitlement
                            AddTfcEverywhereEntitlement(context, response.TFCTVSubItemId, response.ExpiryDate, response.TVEServiceId, user);

                            transaction.GomsTFCEverywhereEndDate = Convert.ToDateTime(response.ExpiryDate);
                            transaction.GomsTFCEverywhereStartDate = registDt;
                            user.Transactions.Add(transaction);
                            user.IsTVEverywhere = true;
                            if (context.SaveChanges() > 0)
                                collection = MyUtility.setError(ErrorCodes.Success, "Claimed TVE");
                        }
                        else
                            collection = MyUtility.setError(ErrorCodes.UnknownError, response.StatusMessage);
                    }

                }



            }
            catch (Exception e) { collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, e.Message); }
            return Content(MyUtility.buildJson(collection), "application/json");

        }

        private static void AddTfcEverywhereEntitlement(IPTV2Entities context, int GomsProductId, string expiryDate, int serviceId, User user, int GomsTransactionId = 0)
        {
            DateTime registDt = DateTime.Now;
            var product = context.Products.FirstOrDefault(p => p.GomsProductId == GomsProductId && p.GomsProductQuantity == 1);
            if (product != null)
            {
                ProductPrice productPrice;
                try
                {
                    productPrice = product.ProductPrices.FirstOrDefault(i => i.CurrencyCode == user.Country.CurrencyCode);
                }
                catch (Exception)
                {
                    productPrice = product.ProductPrices.FirstOrDefault(i => i.CurrencyCode == GlobalConfig.DefaultCurrency);
                }

                //Create Purchase
                Purchase purchase = ContextHelper.CreatePurchase(registDt, "TFC Everywhere");
                //Create Purchase Item
                PurchaseItem purchaseItem = ContextHelper.CreatePurchaseItem(user.UserId, product, productPrice);

                var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId);
                //Create Entitlement & EntitlementRequest
                Entitlement entitlement = user.PackageEntitlements.FirstOrDefault(i => i.PackageId == productPackage.PackageId);

                user.Purchases.Add(purchase);
                user.PurchaseItems.Add(purchaseItem);


                if (entitlement != null)
                {
                    entitlement.EndDate = Convert.ToDateTime(expiryDate);
                    EntitlementRequest request = new EntitlementRequest()
                    {
                        DateRequested = registDt,
                        StartDate = registDt,
                        EndDate = entitlement.EndDate,
                        Product = productPackage.Product,
                        Source = "TFC Everywhere",
                        ReferenceId = GomsTransactionId.ToString()
                    };
                    user.EntitlementRequests.Add(request);
                    entitlement.LatestEntitlementRequest = request;
                    purchaseItem.EntitlementRequest = request;
                }
                else
                {
                    EntitlementRequest request = new EntitlementRequest()
                    {
                        DateRequested = registDt,
                        StartDate = registDt,
                        EndDate = Convert.ToDateTime(expiryDate),
                        Product = productPackage.Product,
                        Source = "TFC Everywhere",
                        ReferenceId = GomsTransactionId.ToString()
                    };

                    PackageEntitlement pkg_entitlement = new PackageEntitlement()
                    {
                        EndDate = Convert.ToDateTime(expiryDate),
                        Package = (IPTV2_Model.Package)productPackage.Package,
                        OfferingId = GlobalConfig.offeringId,
                        LatestEntitlementRequest = request
                    };

                    user.PackageEntitlements.Add(pkg_entitlement);
                    user.EntitlementRequests.Add(request);
                    purchaseItem.EntitlementRequest = request;
                }
            }
        }

        [RequireHttps]
        public ActionResult RegisterToTFCEverywhere()
        {
            if (!GlobalConfig.IsTVERegistrationEnabled)
                return RedirectToAction("Index", "Home");

            if (MyUtility.isUserLoggedIn())
            {
                var context = new IPTV2Entities();
                var userId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    if (user.IsTVEverywhere == true)
                        return Redirect("/TFCChannel");
                }
            }
            else
                return RedirectToAction("TVEverywhereMain", "User");

            if (TempData["tempUserId"] != null)
            {
                var userId = TempData["tempUserId"];
                TempData["tempUid"] = userId;
            }
            if (TempData["isConnectedToSocialNetworks"] != null)
            {
                var isConnectedToSocialNetworks = TempData["isConnectedToSocialNetworks"];
                TempData["isConnectedToSocialNetworks"] = isConnectedToSocialNetworks;
                ViewBag.isConnectedToSocialNetworks = isConnectedToSocialNetworks;
            }
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("RegisterToTFCEverywhere2");
            return View();
        }

        [HttpPost]
        public ActionResult _ResendTVEActivationCode(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            try
            {
                if (String.IsNullOrEmpty(fc["MacAddressOrSmartCard"]) && String.IsNullOrEmpty(fc["AccountNumber"]))
                {
                    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "Please fill up the required fields.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                var context = new IPTV2Entities();
                User user;
                if (MyUtility.isUserLoggedIn())
                {
                    var userId = new Guid(User.Identity.Name);
                    user = context.Users.FirstOrDefault(u => u.UserId == userId);
                }
                else
                {
                    var userId = (Guid)TempData["tempUid"];
                    TempData["tempUserId"] = userId; // REASSIGN
                    TempData["tempUid"] = userId; // REASSIGN
                    user = context.Users.FirstOrDefault(u => u.UserId == userId);
                }

                if (user != null)
                {
                    var gomsService = new GomsTfcTv();
                    var response = gomsService.ResendTVEActivationCode(user.EMail, fc["MacAddressOrSmartCard"], fc["AccountNumber"], user.LastName);
                    if (response.IsSuccess)
                    {
                        collection = MyUtility.setError(ErrorCodes.Success, String.Empty);
                        collection.Add("href", GlobalConfig.RegistrationCompleteTVE);
                    }
                    else
                        collection = MyUtility.setError(ErrorCodes.UnknownError, response.StatusMessage);
                }
                else
                    collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, "User does not exist.");
            }
            catch (Exception e) { collection = MyUtility.setError(ErrorCodes.UnknownError, e.Message); }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        [HttpPost]
        public ActionResult _ClaimTVEverywhere(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            DateTime registDt = DateTime.Now;
            try
            {
                if (HasConsumedNumberOfRetriesForTFCEverywhere())
                {
                    collection = MyUtility.setError(ErrorCodes.LimitReached, "Invalid data entered. Please call our Customer Service at 18778846832 or chat with our live support team for assistance.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                if (((String.IsNullOrEmpty(fc["MacAddressOrSmartCard"]) && String.IsNullOrEmpty(fc["AccountNumber"]))) || String.IsNullOrEmpty(fc["ActivationNumber"]))
                {
                    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "Please fill up the required fields.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                var context = new IPTV2Entities();
                User user = null;
                if (MyUtility.isUserLoggedIn())
                {
                    var userId = new Guid(User.Identity.Name);
                    user = context.Users.FirstOrDefault(u => u.UserId == userId);
                }
                else
                {
                    var userId = (Guid)TempData["tempUid"];
                    TempData["tempUserId"] = userId; // REASSIGN
                    TempData["tempUid"] = userId; // REASSIGN
                    user = context.Users.FirstOrDefault(u => u.UserId == userId);
                }

                if (user != null)
                {
                    string CurrencyCode = GlobalConfig.DefaultCurrency;
                    Country country = context.Countries.FirstOrDefault(c => c.Code == user.CountryCode);
                    if (country != null)
                    {
                        Currency currency = context.Currencies.FirstOrDefault(c => c.Code == country.CurrencyCode);
                        if (currency != null) CurrencyCode = currency.Code;
                    }

                    var transaction = new TfcEverywhereTransaction()
                    {
                        Amount = 0,
                        Date = DateTime.Now,
                        Currency = CurrencyCode,
                        OfferingId = GlobalConfig.offeringId,
                        StatusId = GlobalConfig.Visible,
                        Reference = "TFC Everywhere - CLAIM",
                        UserId = user.UserId
                    };

                    var gomsService = new GomsTfcTv();


                    var MacAddressOrSmartCard = fc["MacAddressOrSmartCard"].Replace(" ", "");
                    var AccountNumber = fc["AccountNumber"].Replace(" ", "");
                    var ActivationNumber = fc["ActivationNumber"].Replace(" ", "");

                    var response = gomsService.ClaimTVEverywhere(context, user.UserId, transaction, MacAddressOrSmartCard, AccountNumber, ActivationNumber);
                    if (response.IsSuccess)
                    {
                        AddTfcEverywhereEntitlement(context, response.TFCTVSubItemId, response.ExpiryDate, response.TVEServiceId, user);
                        transaction.GomsTFCEverywhereEndDate = Convert.ToDateTime(response.ExpiryDate);
                        transaction.GomsTFCEverywhereStartDate = registDt;
                        user.Transactions.Add(transaction);
                        user.IsTVEverywhere = true;
                        if (context.SaveChanges() > 0)
                        {
                            collection = MyUtility.setError(ErrorCodes.Success, String.Empty);
                            collection.Add("href", GlobalConfig.RegistrationCompleteTVE);
                        }
                    }
                    else
                    {
                        SetNumberOfTriesForTFCEverywhereCookie();
                        if (String.Compare(response.StatusCode, "8", true) == 0)
                        {
                            var sb = new StringBuilder();
                            sb.Append(response.StatusMessage);
                            sb.Append(" Go to your <a href=\"/EditProfile\" target=\"_blank\">Edit My Profile</a> page to update the last name registered on your TFC.tv account.");
                            collection = MyUtility.setError(ErrorCodes.UnknownError, sb.ToString());
                        }
                        else
                            collection = MyUtility.setError(ErrorCodes.UnknownError, response.StatusMessage);

                    }

                }
                else
                    collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, "User does not exist.");
            }
            catch (Exception e) { MyUtility.LogException(e); collection = MyUtility.setError(ErrorCodes.UnknownError, e.Message); }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        private void SetNumberOfTriesForTFCEverywhereCookie()
        {
            DateTime registDt = DateTime.Now;
            if (Request.Cookies[".ctve"] != null)
            {
                try
                {
                    var trialCount = Convert.ToInt32(Request.Cookies[".ctve"].Value);
                    trialCount += 1;
                    HttpCookie cookie = new HttpCookie(".ctve");
                    cookie.Expires = registDt.AddMinutes(15);
                    cookie.Value = trialCount.ToString();
                    Response.Cookies.Add(cookie);
                }
                catch (Exception)
                {
                    HttpCookie cookie = new HttpCookie(".ctve");
                    cookie.Expires = registDt.AddMinutes(15);
                    cookie.Value = "1";
                    Response.Cookies.Add(cookie);
                }
            }
            else
            {
                HttpCookie cookie = new HttpCookie(".ctve");
                cookie.Expires = registDt.AddMinutes(15);
                cookie.Value = "1";
                Response.Cookies.Add(cookie);
            }
        }

        private bool HasConsumedNumberOfRetriesForTFCEverywhere()
        {
            bool retVal = false;
            try
            {
                if (Request.Cookies[".ctve"] != null)
                {
                    try
                    {
                        var numberOfTries = Convert.ToInt32(Request.Cookies[".ctve"].Value);
                        if (numberOfTries >= 3)
                            retVal = true;
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
            return retVal;

        }

        [RequireHttp]
        public ActionResult TVEverywhereMain()
        {
            if (!GlobalConfig.IsTVERegistrationEnabled)
                return RedirectToAction("Index", "Home");

            if (MyUtility.isUserLoggedIn())
            {
                var userId = new Guid(User.Identity.Name);
                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    if (user.IsTVEverywhere == true)
                        return Redirect("/TFCChannel");
                    else
                        return RedirectToAction("RegisterToTFCEverywhere", "User");
                }
            }
            return View();
        }
        public ActionResult RegistrationCompleteTFCEverywhere()
        {
            if (!GlobalConfig.IsTVERegistrationEnabled)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public ActionResult _SetTVE(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            DateTime registDt = DateTime.Now;
            try
            {
                HttpCookie cookie = new HttpCookie(".tve");
                cookie.Expires = DateTime.Now.AddHours(2);
                cookie.Value = Guid.NewGuid().ToString();
                this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                collection = MyUtility.setError(ErrorCodes.Success, String.Empty);
            }
            catch (Exception) { }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        //[GridAction]
        //public ActionResult _GetRecurringProducts()
        //{
        //    return View(new GridModel<RecurringBillingDisplay> { Data = ShowRecurringBillings() });
        //}

        public ActionResult GetRecurringProducts()
        {
            return View(ShowRecurringBillings());
        }

        private List<RecurringBillingDisplay> ShowRecurringBillings()
        {
            List<RecurringBillingDisplay> display = new List<RecurringBillingDisplay>();
            if (MyUtility.isUserLoggedIn())
            {
                System.Guid userId = new System.Guid(User.Identity.Name);
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    var recurringBillings = context.RecurringBillings.Where(t => t.UserId == user.UserId && t.OfferingId == GlobalConfig.offeringId && t.StatusId == GlobalConfig.Visible).OrderByDescending(t => t.RecurringBillingId);
                    if (recurringBillings != null)
                    {
                        foreach (var item in recurringBillings)
                        {

                            RecurringBillingDisplay disp = new RecurringBillingDisplay()
                            {
                                EndDate = (DateTime)item.EndDate,
                                EndDateStr = item.EndDate.Value.ToShortDateString(),
                                NextRun = (DateTime)item.NextRun,
                                NextRunStr = item.NextRun.Value.ToShortDateString(),
                                PackageId = item.PackageId,
                                PackageName = item.Package.Description,
                                ProductId = item.ProductId,
                                ProductName = item.Product.Description,
                                RecurringBillingId = item.RecurringBillingId,
                                StatusId = item.StatusId,
                                UserId = item.UserId,
                                //isDisabled = ((DateTime)item.NextRun).Date.Subtract(DateTime.Now.Date).Days < 2 ? true : false
                                isDisabled = false,
                                PaymentType = item is CreditCardRecurringBilling ? "Credit Card" : "Paypal"


                            };
                            display.Add(disp);
                        }
                    }
                }
            }
            return display;
        }

        [HttpPost]
        public ActionResult UpdateRecurringProducts(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            collection = MyUtility.setError(errorCode, "Unable to update. Please contact support.");
            try
            {
                DateTime registDt = DateTime.Now;
                var status = fc["value"];
                var name = fc["name"];
                if (!String.IsNullOrEmpty(status) && !String.IsNullOrEmpty(name))
                {
                    if (MyUtility.isUserLoggedIn())
                    {
                        var context = new IPTV2Entities();
                        bool RecurringStatus = false;
                        try { RecurringStatus = Convert.ToBoolean(status); }
                        catch (Exception e) { MyUtility.LogException(e); }

                        int RecurringBillingId = 0;
                        try { RecurringBillingId = Convert.ToInt32(name.Substring(2)); }
                        catch (Exception e) { MyUtility.LogException(e); }

                        var userId = new Guid(User.Identity.Name);
                        var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            var billing = user.RecurringBillings.FirstOrDefault(r => r.RecurringBillingId == RecurringBillingId && r.StatusId != 2);
                            if (billing != null)
                            {

                                //Check first if there is a same package with recurring turned on
                                if (user.RecurringBillings.Count(r => r.PackageId == billing.PackageId && r.StatusId == GlobalConfig.Visible && r.RecurringBillingId != billing.RecurringBillingId) > 0)
                                {
                                    //there is same package with recurring enabled.
                                    collection = MyUtility.setError(ErrorCodes.UnknownError, "There is same recurring package enabled.");
                                }
                                else
                                {
                                    billing.StatusId = RecurringStatus ? 1 : 0;
                                    billing.UpdatedOn = registDt;
                                    if (registDt.Date > billing.NextRun && RecurringStatus)
                                        billing.NextRun = registDt.AddDays(1).Date;

                                    if (context.SaveChanges() > 0)
                                        collection = MyUtility.setError(ErrorCodes.Success, "Success!");
                                    else
                                        collection = MyUtility.setError(ErrorCodes.EntityUpdateError, "Unable to update. Please try again later.");
                                }


                            }
                        }
                        else
                            collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, "User does not exist.");
                    }
                    else
                        collection = MyUtility.setError(ErrorCodes.NotAuthenticated, "Not authenticated.");
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        private void UpdateRecurringBillingViaEditProfile(IPTV2Entities context, User user, string list)
        {
            try
            {
                DateTime registDt = DateTime.Now;
                var rb_list = list.Split(',');

                if (rb_list.Count() > 0)
                {
                    foreach (var item in rb_list)
                    {
                        if (!String.IsNullOrEmpty(item))
                        {
                            var name = item;
                            if (MyUtility.isUserLoggedIn())
                            {
                                bool RecurringStatus = false;

                                int RecurringBillingId = 0;
                                try { RecurringBillingId = Convert.ToInt32(name.Substring(2)); }
                                catch (Exception e) { MyUtility.LogException(e); }

                                if (user != null)
                                {
                                    var billing = user.RecurringBillings.FirstOrDefault(r => r.RecurringBillingId == RecurringBillingId && r.StatusId != 2);
                                    if (billing != null)
                                    {
                                        //Check first if there is a same package with recurring turned on
                                        if (user.RecurringBillings.Count(r => r.PackageId == billing.PackageId && r.StatusId == GlobalConfig.Visible && r.RecurringBillingId != billing.RecurringBillingId) > 0)
                                        {
                                            //there is same package with recurring enabled.                                            
                                        }
                                        else
                                        {
                                            if (billing is PaypalRecurringBilling)
                                            {
                                                try
                                                {
                                                    var paypalrbilling = (PaypalRecurringBilling)billing;
                                                    if (PaymentHelper.CancelPaypalRecurring(paypalrbilling.SubscriberId))
                                                    {
                                                        billing.StatusId = RecurringStatus ? 1 : 0;
                                                        billing.UpdatedOn = registDt;
                                                        if (registDt.Date > billing.NextRun && RecurringStatus)
                                                            billing.NextRun = registDt.AddDays(1).Date;
                                                    }
                                                }
                                                catch (Exception) { }
                                            }
                                            else
                                            {
                                                billing.StatusId = RecurringStatus ? 1 : 0;
                                                billing.UpdatedOn = registDt;
                                                if (registDt.Date > billing.NextRun && RecurringStatus)
                                                    billing.NextRun = registDt.AddDays(1).Date;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
        }

        [HttpPost]
        public string CILLogin(FormCollection fc)
        {
            var collection = new System.Collections.Specialized.NameValueCollection();
            if (String.IsNullOrEmpty(fc["EmailAddress"]))
            {
                collection.Add("CIL_VALID", "0");
                collection.Add("CIL_LOGIN_MSG", "Please fill in required fields");
                return "&" + collection.ToQueryString();
            }

            if (String.IsNullOrEmpty(fc["Password"]))
            {
                collection.Add("CIL_VALID", "0");
                collection.Add("CIL_LOGIN_MSG", "Please fill in required fields");
                return "&" + collection.ToQueryString();
            }

            var EmailAddress = fc["EmailAddress"];
            var Password = fc["Password"];

            try
            {
                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);

                if (user == null)
                {
                    collection.Add("CIL_VALID", "0");
                    collection.Add("CIL_LOGIN_MSG", "User does not exist.");
                }
                else
                {
                    if (user.StatusId != 1) // Not verified
                    {
                        collection.Add("CIL_VALID", "0");
                        collection.Add("CIL_LOGIN_MSG", "Email is not verified.");
                    }

                    Password = MyUtility.GetSHA1(Password);
                    if (String.Compare(user.EMail, EmailAddress, true) == 0 && String.Compare(user.Password, Password, false) == 0)
                    {
                        collection.Add("CIL_VALID", "1");
                        collection.Add("CIL_USERNAME", String.Format("{0} {1}", user.FirstName, user.LastName));

                        try
                        {
                            Dictionary<string, object> gcollection = new Dictionary<string, object>();
                            gcollection.Add("uid", user.UserId);
                            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(gcollection));
                            var avatar = res.GetString("thumbnailURL", String.Empty);
                            if (!String.IsNullOrEmpty(avatar))
                                collection.Add("CIL_AVATAR", avatar);
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        collection.Add("CIL_VALID", "0");
                        collection.Add("CIL_LOGIN_MSG", "Email & password do not match.");
                    }
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                collection.Add("CIL_VALID", "0");
                collection.Add("CIL_LOGIN_MSG", "System encountered an error. Please try again.");
            }
            return "&" + collection.ToQueryString();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _Registration(FormCollection fc)
        {
            //fc["Email"] = "Albin_Lim@abs-cbn.com";
            //fc["Password"] = "mokmok123";
            //fc["ConfirmPassword"] = "mokmok123";
            //fc["FirstName"] = "Albin";
            //fc["LastName"] = "Lim";
            //fc["CountryCode"] = "US";
            //fc["City"] = "CA";
            //fc["State"] = "CA";
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            if (!Request.IsAjaxRequest())
            {
                collection = MyUtility.setError(ErrorCodes.UnknownError, "Your request is invalid.");
                return Content(MyUtility.buildJson(collection), "application/json");
            }


            bool isConnectedToSocialNetworks = false;
            string href = "/User/RegisterVerify";

            if (MyUtility.isUserLoggedIn()) //User is logged in.
                return RedirectToAction("Index", "Home");
            if (String.IsNullOrEmpty(fc["Email"]))
            {
                collection = MyUtility.setError(ErrorCodes.IsEmailEmpty);
                return Content(MyUtility.buildJson(collection), "application/json");
            }
            if (String.Compare(fc["Password"], fc["ConfirmPassword"], false) != 0)
            {
                collection = MyUtility.setError(ErrorCodes.IsMismatchPassword);
                return Content(MyUtility.buildJson(collection), "application/json");
            }
            if (String.IsNullOrEmpty(fc["FirstName"]) || String.IsNullOrEmpty(fc["LastName"]) || String.IsNullOrEmpty(fc["CountryCode"]))
            {
                collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields);
                return Content(MyUtility.buildJson(collection), "application/json");
            }

            RegexUtilities util = new RegexUtilities();
            //if (!MyUtility.isEmail(fc["Email"]))
            if (!util.IsValidEmail(fc["Email"]))
            {
                collection = MyUtility.setError(ErrorCodes.IsNotValidEmail);
                return Content(MyUtility.buildJson(collection), "application/json");
            }

            try
            {
                string FirstName = fc["FirstName"];
                string LastName = fc["LastName"];
                string CountryCode = fc["CountryCode"];
                string EMail = fc["Email"];
                string Password = fc["Password"];
                string City = fc["City"];
                string State = String.IsNullOrEmpty(fc["State"]) ? fc["StateDD"] : fc["State"];
                System.Guid userId = System.Guid.NewGuid();
                string provider = "tfctv";

                if (FirstName.Length > 32)
                {
                    collection = MyUtility.setError(ErrorCodes.LimitReached, "First Name cannot exceed 32 characters.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                if (LastName.Length > 32)
                {
                    collection = MyUtility.setError(ErrorCodes.LimitReached, "Last Name cannot exceed 32 characters.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                if (EMail.Length > 64)
                {
                    collection = MyUtility.setError(ErrorCodes.LimitReached, "Email cannot exceed 64 characters.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                if (!String.IsNullOrEmpty(State))
                    if (State.Length > 30)
                    {
                        collection = MyUtility.setError(ErrorCodes.LimitReached, "State cannot exceed 30 characters.");
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }
                if (!String.IsNullOrEmpty(City))
                    if (City.Length > 50)
                    {
                        collection = MyUtility.setError(ErrorCodes.LimitReached, "City cannot exceed 50 characters.");
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }

                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EMail, true) == 0);
                if (user != null)
                {
                    collection = MyUtility.setError(ErrorCodes.IsExistingEmail);
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                /***** CHECK FOR COUNTRY CODE ****/
                if (context.Countries.Count(c => String.Compare(c.Code, CountryCode, true) == 0) <= 0)
                {
                    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "Country Code is invalid.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                else if (GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',').Contains(CountryCode))
                {
                    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "Country Code is invalid.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                if (String.IsNullOrEmpty(State))
                {
                    collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "State is required.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                else
                {
                    if (context.States.Count(c => c.CountryCode == CountryCode.ToUpper()) > 0)
                    {
                        if (context.States.Count(s => s.CountryCode == CountryCode.ToUpper() && (s.StateCode == State || s.Name == State)) == 0)
                        {
                            collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, "State is invalid.");
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                    }
                }


                DateTime registDt = DateTime.Now;
                user = new User()
                {
                    UserId = userId,
                    FirstName = FirstName,
                    LastName = LastName,
                    City = City,
                    State = State,
                    CountryCode = CountryCode,
                    EMail = EMail,
                    Password = MyUtility.GetSHA1(Password),
                    GigyaUID = userId.ToString(),
                    RegistrationDate = registDt,
                    LastUpdated = registDt,
                    RegistrationIp = Request.GetUserHostAddressFromCloudflare(),
                    StatusId = 0,
                    ActivationKey = Guid.NewGuid()
                };


                //UPDATE: FEB 18, 2013
                if (MyUtility.isTVECookieValid())
                    user.IsTVERegistrant = true;

                string CurrencyCode = GlobalConfig.DefaultCurrency;
                Country country = context.Countries.FirstOrDefault(c => c.Code == CountryCode);
                if (country != null)
                {
                    Currency currency = context.Currencies.FirstOrDefault(c => c.Code == country.CurrencyCode);
                    if (currency != null) CurrencyCode = currency.Code;
                }
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == CurrencyCode);
                if (wallet == null) // Wallet does not exist. Create new wallet for User.
                {
                    wallet = ContextHelper.CreateWallet(0, CurrencyCode, registDt);
                    user.UserWallets.Add(wallet);
                }

                var transaction = new RegistrationTransaction()
                {
                    RegisteredState = user.State,
                    RegisteredCity = user.City,
                    RegisteredCountryCode = user.CountryCode,
                    Amount = 0,
                    Currency = CurrencyCode,
                    Reference = "New Registration",
                    Date = registDt,
                    OfferingId = GlobalConfig.offeringId,
                    UserId = user.UserId,
                    StatusId = GlobalConfig.Visible
                };

                user.Transactions.Add(transaction);

                context.Users.Add(user);
                if (context.SaveChanges() > 0)
                {
                    if (TempData["qs"] != null)
                    {
                        NameValueCollection qs = (NameValueCollection)TempData["qs"];
                        Dictionary<string, object> GigyaCollection = new Dictionary<string, object>();
                        collection.Add("uid", qs["UID"]);
                        collection.Add("siteUID", userId);
                        collection.Add("cid", String.Format("{0} - New User", qs["provider"]));
                        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(collection));
                        provider = qs["provider"];
                        isConnectedToSocialNetworks = true;
                    }
                    else
                    {
                        Dictionary<string, object> userInfo = new Dictionary<string, object>();
                        userInfo.Add("firstName", user.FirstName);
                        userInfo.Add("lastName", user.LastName);
                        userInfo.Add("email", user.EMail);
                        Dictionary<string, object> gigyaCollection = new Dictionary<string, object>();
                        gigyaCollection.Add("siteUID", user.UserId);
                        gigyaCollection.Add("cid", "TFCTV - Registration");
                        gigyaCollection.Add("sessionExpiration", "0");
                        gigyaCollection.Add("newUser", true);
                        gigyaCollection.Add("userInfo", MyUtility.buildJson(userInfo));
                        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(gigyaCollection));
                        GigyaHelpers.setCookie(res, this.ControllerContext);
                    }

                    //setUserData
                    User usr = context.Users.FirstOrDefault(u => u.EMail == EMail);
                    setUserData(usr.UserId.ToString(), usr);
                    var ActivationKey = usr.ActivationKey;

                    bool isTFCnowCustomer = false;

                    if (TempData["TFCnowCustomer"] != null)
                    {
                        Customer customer = (Customer)TempData["TFCnowCustomer"];
                        usr.StatusId = 1;
                        usr.DateVerified = registDt;
                        TempData["TFCnowCustomer"] = customer;
                        href = "/Migration/Migrate";
                        if (context.SaveChanges() > 0)
                        {
                            //SetAutheticationCookie(userId.ToString());
                            isTFCnowCustomer = true;
                        }
                    }

                    if (isConnectedToSocialNetworks)
                    {
                        usr.StatusId = 1;
                        usr.DateVerified = registDt;
                        context.SaveChanges();
                    }

                    //If FreeTrial is enabled, insert free trial.
                    //if (GlobalConfig.IsFreeTrialEnabled)
                    //{
                    //    context = new IPTV2Entities();
                    //    if (isConnectedToSocialNetworks)
                    //        PaymentHelper.PayViaWallet(context, userId, GlobalConfig.FreeTrial14ProductId, SubscriptionProductType.Package, userId, null);
                    //    else
                    //        PaymentHelper.PayViaWallet(context, userId, GlobalConfig.FreeTrial7ProductId, SubscriptionProductType.Package, userId, null);
                    //    context.SaveChanges();
                    //}

                    /***** DEC 31 2012 ****/
                    //UPDATED: FEB 18, 2013 - To include checking for TVE
                    if (usr.IsTVERegistrant == null || usr.IsTVERegistrant == false)
                    {
                        int freeTrialProductId = 0;
                        if (GlobalConfig.IsFreeTrialEnabled)
                        {
                            freeTrialProductId = MyUtility.GetCorrespondingFreeTrialProductId();
                            context = new IPTV2Entities();
                            if (GlobalConfig.TfcTvFree2StartDate < registDt && GlobalConfig.TfcTvFree2EndDate > registDt)
                            {
                                string UserCountryCode = user.CountryCode;
                                if (!GlobalConfig.isUAT)
                                    try { UserCountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy(); }
                                    catch (Exception) { }

                                var countryList = GlobalConfig.TfcTvFree2CountryWhiteList.Split(',');
                                if (countryList.Contains(UserCountryCode) && String.Compare(user.CountryCode, UserCountryCode, true) == 0)
                                    freeTrialProductId = GlobalConfig.TfcTvFree2ProductId;
                            }
                            if (Request.Cookies.AllKeys.Contains("vntycok"))
                            { freeTrialProductId = GlobalConfig.FreeTrial14ProductId; }
                            if (isConnectedToSocialNetworks)
                                PaymentHelper.PayViaWallet(context, userId, freeTrialProductId, SubscriptionProductType.Package, userId, null);
                        }
                    }


                    //Publish to Activity Feed
                    List<ActionLink> actionlinks = new List<ActionLink>();
                    actionlinks.Add(new ActionLink() { text = SNSTemplates.register_actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_actionlink_href) });
                    //mediaItem
                    List<MediaItem> mediaItems = new List<MediaItem>();
                    mediaItems.Add(new MediaItem() { type = SNSTemplates.register_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.register_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_mediaitem_href) });
                    UserAction action = new UserAction()
                    {
                        actorUID = userId.ToString(),
                        userMessage = SNSTemplates.register_usermessage,
                        title = SNSTemplates.register_title,
                        subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_subtitle),
                        linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_linkback),
                        description = String.Format(SNSTemplates.register_description, FirstName),
                        actionLinks = actionlinks,
                        mediaItems = mediaItems
                    };

                    GigyaMethods.PublishUserAction(action, userId, "external");
                    action.userMessage = String.Empty;
                    action.title = String.Empty;
                    action.mediaItems = null;
                    GigyaMethods.PublishUserAction(action, userId, "internal");
                    var email_err = String.Empty;
                    //FormsAuthentication.SetAuthCookie(userId.ToString(), true);
                    if (isConnectedToSocialNetworks)
                    {
                        //SetAutheticationCookie(userId.ToString());
                        if (!Request.IsLocal)
                        {
                            try { MyUtility.SendConfirmationEmail(context, usr); }
                            catch (Exception) { }
                        }

                        href = GlobalConfig.RegistrationConfirmPage;
                        //UPDATED: FEB 18, 2013
                        if (usr.IsTVERegistrant != null)
                            if ((bool)usr.IsTVERegistrant)
                            {
                                href = GlobalConfig.TVERegistrationPage;
                                MyUtility.RemoveTVECookie();
                            }
                    }
                    else
                    {
                        if (!isTFCnowCustomer)
                        {
                            //string emailBody = String.Format("Copy and paste this url to activate your TFC.tv Account {0}/User/Verify?email={1}&key={2}", GlobalConfig.baseUrl, usr.EMail, ActivationKey.ToString());
                            string verification_email = String.Format("{0}/User/Verify?key={1}", GlobalConfig.baseUrl, usr.ActivationKey.ToString());
                            string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, usr.FirstName, usr.EMail, verification_email);
                            //MyUtility.SendEmailViaSendGrid(usr.EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody);
                            if (!Request.IsLocal)
                                try
                                {
                                    //MyUtility.SendEmailViaSendGrid(usr.EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody);
                                    MyUtility.SendEmailViaSendGrid(usr.EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody, MailType.TextOnly, emailBody);
                                }
                                catch (Exception)
                                {
                                    email_err = " But we are not able to send the verification email.";
                                }
                        }
                    }

                    ////UPDATED: FEB 12, 2012
                    //if (!String.IsNullOrEmpty(fc["TVEverywhere"]))
                    //{
                    //    if (String.Compare(fc["TVEverywhere"], "0", true) == 0)
                    //    {
                    //        TempData["tempUserId"] = userId;
                    //        href = GlobalConfig.TVERegistrationPage;
                    //        TempData["isConnectedToSocialNetworks"] = isConnectedToSocialNetworks;
                    //    }
                    //}

                    if (usr.StatusId == GlobalConfig.Visible) //UPDATED: MARCH 1, 2013. Only set Authentication Cookie when user is verified.
                        SetAutheticationCookie(userId.ToString());
                    errorMessage = "Thank you! You are now registered to TFC.tv!" + email_err;
                    collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                    collection.Add("info", String.Format("{0}|{1}|{2}", user.EMail, Request.GetUserHostAddressFromCloudflare(), provider));
                    collection.Add("href", href);

                    FlagBetaKey(fc["iid"]);
                }
                else
                {
                    errorMessage = "The system encountered an unidentified error. Please try again.";
                    collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
                }
            }
            catch (Exception e)
            {
                collection = MyUtility.setError(ErrorCodes.EntityUpdateError, e.InnerException.InnerException.Message + "<br/>" + e.InnerException.InnerException.StackTrace);
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        [RequireHttps]
        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public JsonResult _SignIn(FormCollection fc)
        {
            //Response.ContentType = "application/json";

            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = "The system encountered an unspecified error. Please contact Customer Support.",
                HtmlUri = ""
            };

            try
            {
                ViewBag.IsTVEverywhere = false;
                string EmailAddress = fc["EmailAddress"];
                string Password = fc["Password"];

                if (String.IsNullOrEmpty(fc["EmailAddress"]))
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsMissingRequiredFields;
                    ReturnCode.StatusMessage = "Email address is required.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (String.IsNullOrEmpty(fc["Password"]))
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsMissingRequiredFields;
                    ReturnCode.StatusMessage = "Password is required.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Your request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                var context = new IPTV2Entities();

                if (User.Identity.IsAuthenticated)
                {
                    var UserId = new Guid(User.Identity.Name);
                    var tUser = context.Users.FirstOrDefault(u => u.UserId == UserId);
                    if (tUser != null)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                        ReturnCode.StatusMessage = "Login successful. Please wait while we redirect you...";
                        var UserHostAddress = Request.GetUserHostAddressFromCloudflare();
                        ReturnCode.info = String.Format("{0}|{1}|{2}|{3}|{4}", tUser.EMail, UserHostAddress, "Site", MyUtility.getCountry(UserHostAddress).getCode(), DateTime.Now.ToString("yyyyMMdd-HHmmss"));
                        //UPDATE: FEB18,2013 - If TVE cookie is valid, assign user who logged in as TVERegistrant
                        try
                        {
                            if (tUser.IsTVEverywhere == true)
                                ReturnCode.HtmlUri = "/TFCChannel";
                        }
                        catch (Exception) { }
                    }
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                if (user == null)
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.UserDoesNotExist;
                    ReturnCode.StatusMessage = "User does not exist.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (user.StatusId != GlobalConfig.Visible)
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.UserDoesNotExist;
                    ReturnCode.StatusMessage = "Email address is not verified.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                Password = MyUtility.GetSHA1(Password);
                if (String.Compare(user.EMail, EmailAddress, true) == 0 && String.Compare(user.Password, Password, false) == 0)
                {
                    SendToGigya(user);
                    SetAutheticationCookie(user.UserId.ToString());
                    SetSession(user.UserId.ToString());
                    ContextHelper.SaveSessionInDatabase(context, user);
                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                    ReturnCode.StatusMessage = "Login successful. Please wait while we redirect you...";
                    var UserHostAddress = Request.GetUserHostAddressFromCloudflare();
                    ReturnCode.info = String.Format("{0}|{1}|{2}|{3}|{4}", user.EMail, UserHostAddress, "Site", MyUtility.getCountry(UserHostAddress).getCode(), DateTime.Now.ToString("yyyyMMdd-HHmmss"));
                    //UPDATE: FEB18,2013 - If TVE cookie is valid, assign user who logged in as TVERegistrant
                    try
                    {
                        if (MyUtility.isTVECookieValid())
                        {
                            user.IsTVERegistrant = true;
                            context.SaveChanges();
                            MyUtility.RemoveTVECookie();
                            ReturnCode.HtmlUri = "/RegisterTFCEverywhere";
                        }
                        if (user.IsTVEverywhere == true)
                            ReturnCode.HtmlUri = "/TFCChannel";
                    }
                    catch (Exception) { }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsMismatchPassword;
                    ReturnCode.StatusMessage = "Email/Password do not match.";
                }
                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        private void SendToGigya(User user)
        {
            Dictionary<string, object> userInfo = new Dictionary<string, object>();
            userInfo.Add("firstName", user.FirstName);
            userInfo.Add("lastName", user.LastName);
            userInfo.Add("email", user.EMail);
            Dictionary<string, object> gigyaCollection = new Dictionary<string, object>();
            gigyaCollection.Add("siteUID", user.UserId);
            gigyaCollection.Add("cid", "TFCTV - Login");
            //gigyaCollection.Add("sessionExpiration", 2592000);
            gigyaCollection.Add("sessionExpiration", 432000);
            gigyaCollection.Add("userInfo", MyUtility.buildJson(userInfo));
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(gigyaCollection));
            GigyaHelpers.setCookie(res, this.ControllerContext);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ForgetPassword(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = "The system encountered an unspecified error. Please contact Customer Support."
            };

            try
            {
                var registDt = DateTime.Now;
                string EmailAddress = fc["FEmailAddress"];

                if (String.IsNullOrEmpty(fc["FEmailAddress"]))
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsMissingRequiredFields;
                    ReturnCode.StatusMessage = "Email address is required.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                //RegexUtilities util = new RegexUtilities();
                if (!MyUtility.isEmail(EmailAddress))
                {
                    ReturnCode.StatusMessage = "Email address is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Your request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                if (user == null)
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.UserDoesNotExist;
                    ReturnCode.StatusMessage = "User does not exist.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (user.StatusId != GlobalConfig.Visible)
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.UserDoesNotExist;
                    ReturnCode.StatusMessage = "Email address is not verified.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                user.LastUpdated = registDt;
                if (context.SaveChanges() > 0)
                {
                    string oid = MyUtility.GetSHA1(String.Format("{0}{1}", user.UserId, user.LastUpdated));
                    string reset_pwd_email = String.Format("{0}/User/ResetPassword?key={1}&oid={2}", GlobalConfig.baseUrl, user.ActivationKey, oid.ToLower());
                    string emailBody = String.Format(GlobalConfig.ResetPasswordBodyTextOnly, user.FirstName, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), reset_pwd_email);
                    try
                    {
                        MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, "Reset your TFC.tv Password", emailBody, MailType.TextOnly, emailBody);
                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                        ReturnCode.StatusMessage = "Instructions on how to reset your password have been sent to your email address.";
                    }
                    catch (Exception e)
                    {
                        MyUtility.LogException(e);
                        ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                        ReturnCode.StatusMessage = "The system encountered an unspecified error. Please contact Customer Support.";
                    }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                    ReturnCode.StatusMessage = "The system was unable to process your request. Please try again later.";
                }

                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RedeemLite(string email, string key)
        {
            ViewBag.Success = false;
            string result = String.Empty;
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(key))
                return RedirectToAction("Index", "Home");
            else
            {
                if (!User.Identity.IsAuthenticated)
                    ViewBag.ErrorMessage = "Dear Kapamilya, para ma-enjoy ang inyong free access, maaari po lamang na mag-log-in.";
                else
                {
                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    User user;
                    if (User.Identity.IsAuthenticated)
                        user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
                    else
                        user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, email, true) == 0 && u.ActivationKey == new Guid(key));

                    if (user != null)
                    {
                        if (user.IsTVERegistrant == null || user.IsTVERegistrant == false)
                        {
                            if (user.PurchaseItems.Count(p => p.ProductId == GlobalConfig.TfcTvFree2ProductId) > 0)
                                ViewBag.ErrorMessage = "Dear Kapamilya, your free access has already been activated. You may continue to enjoy your Kapamilya shows by availing any of the TFC.tv packages.";
                            else
                            {
                                int freeTrialProductId = 0;
                                if (GlobalConfig.IsFreeTrialEnabled)
                                {
                                    freeTrialProductId = MyUtility.GetCorrespondingFreeTrialProductId();
                                    if (GlobalConfig.TfcTvFree2StartDate < registDt && GlobalConfig.TfcTvFree2EndDate > registDt)
                                    {
                                        string UserCountryCode = user.CountryCode;
                                        if (!GlobalConfig.isUAT)
                                            try { UserCountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy(); }
                                            catch (Exception) { }

                                        var countryList = GlobalConfig.TfcTvFree2CountryWhiteList.Split(',');
                                        if (countryList.Contains(UserCountryCode) && String.Compare(user.CountryCode, UserCountryCode, true) == 0)
                                            freeTrialProductId = GlobalConfig.TfcTvFree2ProductId;
                                        else
                                            return RedirectToAction("Index", "Home");
                                    }
                                    ErrorResponse response = PaymentHelper.PayViaWallet(context, user.UserId, freeTrialProductId, SubscriptionProductType.Package, user.UserId, null);
                                    var returnCode = (ErrorCodes)response.Code;
                                    if (returnCode == ErrorCodes.Success)
                                    {
                                        ViewBag.Success = true;
                                        ViewBag.ErrorMessage = "Pwede ka nanag manood ng mga piling Kapamilya shows.";
                                    }
                                    else
                                        ViewBag.ErrorMessage = "The system encountered an unspecified error. Please try again later.";
                                }
                            }
                        }
                    }
                    else
                        ViewBag.ErrorMessage = "Email does not exist.";
                }
            }
            return View();
        }

        public ActionResult ClaimFreeABSCBNLiveStream()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    ViewBag.ActivationSuccess = false; ViewBag.UserHasClaimed = false;
                    var registDt = DateTime.Now;
                    var userId = new Guid(User.Identity.Name);
                    if (GlobalConfig.IsABSCBNFreeLiveStreamFreeOnRegistrationEnabled)
                    {
                        if (GlobalConfig.ABSCBNFreeLiveStreamStartDate < registDt && GlobalConfig.ABSCBNFreeLiveStreamEndDate > registDt)
                        {
                            var context = new IPTV2Entities();
                            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                            if (user != null)
                            {
                                if (user.PackageEntitlements.Count(e => e.PackageId == GlobalConfig.ABSCBNFreeLiveStreamPackageId) > 0)
                                    ViewBag.UserHasClaimed = true;
                                else
                                {
                                    PaymentHelper.PayViaWallet(context, userId, GlobalConfig.ABSCBNFreeLiveStreamProductId, SubscriptionProductType.Package, userId, null);
                                    ViewBag.ActivationSuccess = true;
                                }
                            }
                            else
                                return RedirectToAction("Index", "Home");
                        }
                        else
                            return RedirectToAction("Index", "Home");
                    }
                    else
                        return RedirectToAction("Index", "Home");

                }
                catch (Exception e) { MyUtility.LogException(e); return RedirectToAction("Index", "Home"); }
            }
            else
                return RedirectToAction("Index", "Home");
            return View();
        }

        [RequireHttp]
        public ActionResult Registration()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return RedirectToAction("Register", "User");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _Register(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "Registration",
                TransactionType = "Registration"
            };

            bool isSourceAir = false;
            string url = Url.Action("Register", "User").ToString();
            var field_names = new string[] { "uid", "provider", "full_name", "state" };

            if (!String.IsNullOrEmpty(Request.QueryString["source"]))
            {
                url = Url.Action("Index", "Air").ToString();
                isSourceAir = true;
                field_names = new string[] { "uid", "provider", "state" };
            }
            try
            {
                if (TempData["qs"] != null)
                {
                    var qs = (NameValueCollection)TempData["qs"];
                    ViewBag.qs = qs;
                    TempData["qs"] = qs;
                }

                DateTime registDt = DateTime.Now;
                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;

                foreach (var x in tmpCollection)
                {
                    if (!field_names.Contains(x.Key))
                        if (String.IsNullOrEmpty(x.Value))
                        {
                            isMissingRequiredFields = true;
                            break;
                        }
                }

                if (!isMissingRequiredFields) // process form
                {
                    string FirstName = tmpCollection["first_name"];
                    string LastName = tmpCollection["last_name"];
                    string CountryCode = tmpCollection["country"];
                    string EMail = tmpCollection["login_email"];
                    string Password = tmpCollection["login_pass"];
                    string City = tmpCollection["city"];
                    string State = String.Empty;
                    try
                    {
                        //tmpCollection.TryGetValue("state", out State);
                        if (tmpCollection.ContainsKey("state"))
                            State = tmpCollection["state"];
                    }
                    catch (Exception e) { MyUtility.LogException(e, "State undefined in collection"); }
                    string provider = String.IsNullOrEmpty(tmpCollection["provider"]) ? String.Empty : tmpCollection["provider"];
                    string uid = String.IsNullOrEmpty(tmpCollection["uid"]) ? String.Empty : tmpCollection["uid"];
                    System.Guid userId = System.Guid.NewGuid();
                    string browser = Request.UserAgent;
                    bool IsStateEmpty = false;

                    if (FirstName.Length > 32)
                        ReturnCode.StatusMessage = "First Name cannot exceed 32 characters.";
                    if (LastName.Length > 32)
                        ReturnCode.StatusMessage = "Last Name cannot exceed 32 characters.";
                    if (EMail.Length > 64)
                        ReturnCode.StatusMessage = "Email address cannot exceed 64 characters.";
                    if (!String.IsNullOrEmpty(State))
                    {
                        if (State.Length > 30)
                            ReturnCode.StatusMessage = "State cannot exceed 30 characters.";
                    }
                    else
                        IsStateEmpty = true;

                    if (IsStateEmpty)
                    {
                        try
                        {
                            var location = MyUtility.GetLocationViaIpAddressWithoutProxy();
                            State = String.Compare(location.countryCode, GlobalConfig.DefaultCountry, true) == 0 ? location.region : location.regionName;
                            if (!String.IsNullOrEmpty(State))
                                IsStateEmpty = false;
                        }
                        catch (Exception) { }
                    }

                    if (City.Length > 50)
                        ReturnCode.StatusMessage = "City cannot exceed 50 characters.";

                    RegexUtilities util = new RegexUtilities();
                    //if (!MyUtility.isEmail(EMail))
                    if (!util.IsValidEmail(EMail))
                        ReturnCode.StatusMessage = "Email address is invalid.";

                    var context = new IPTV2Entities();
                    User user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EMail, true) == 0);
                    if (user != null)
                        ReturnCode.StatusMessage = "Email address is already taken.";

                    if (IsStateEmpty) //if state is empty, get the first value on the list of states
                    {
                        var firstState = context.States.FirstOrDefault(s => String.Compare(s.CountryCode, CountryCode, true) == 0);
                        if (firstState != null)
                            State = firstState.StateCode;
                    }
                    else
                    {
                        ////check if country is part of the list                       
                        //var tempState = context.States.FirstOrDefault(s => String.Compare(s.Name, State, true) == 0 && String.Compare(s.CountryCode, CountryCode, true) == 0);
                        //if (tempState != null)
                        //    State = tempState.StateCode;
                    }

                    if (String.IsNullOrEmpty(State))
                        ReturnCode.StatusMessage = "State is invalid for this country.";

                    if (GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',').Contains(CountryCode)) // check if country is part of the exclusion list first
                        ReturnCode.StatusMessage = "Country does not exist.";
                    else if (context.Countries.Count(c => String.Compare(c.Code, CountryCode, true) == 0) <= 0) // then check if country is part of the list                    
                        ReturnCode.StatusMessage = "Country does not exist.";
                    if (context.States.Count(s => String.Compare(s.CountryCode, CountryCode, true) == 0) > 0)
                        if (context.States.Count(s => String.Compare(s.CountryCode, CountryCode, true) == 0 && (String.Compare(s.StateCode, State, true) == 0 || String.Compare(s.Name, State, true) == 0)) <= 0)
                            ReturnCode.StatusMessage = "State is invalid for this country.";

                    if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                    {
                        TempData["ErrorMessage"] = ReturnCode;
                        if (isSourceAir)
                            return Redirect("/WatchNow");//return RedirectToAction("Index", "Air");
                        return RedirectToAction("Register", "User");
                    }

                    user = new User()
                    {
                        UserId = userId,
                        FirstName = FirstName,
                        LastName = LastName,
                        City = City,
                        State = State,
                        CountryCode = CountryCode,
                        EMail = EMail,
                        Password = MyUtility.GetSHA1(Password),
                        GigyaUID = userId.ToString(),
                        RegistrationDate = registDt,
                        LastUpdated = registDt,
                        RegistrationIp = MyUtility.GetClientIpAddress(),
                        StatusId = 0,
                        ActivationKey = Guid.NewGuid()
                    };

                    try
                    {
                        if (Request.Cookies.AllKeys.Contains("tuid"))
                            user.RegistrationCookie = Request.Cookies["tuid"].Value;
                        else if (Request.Cookies.AllKeys.Contains("regcook"))
                            user.RegistrationCookie = Request.Cookies["regcook"].Value;
                    }
                    catch (Exception) { }

                    ////check for cookie 
                    try
                    {
                        if (!Request.Cookies.AllKeys.Contains("rcdskipcheck"))
                        {
                            var dt = DateTime.Parse(Request.Cookies["rcDate"].Value);
                            if (registDt.Subtract(dt).Days < 45)
                            {
                                ReturnCode.StatusMessage = "We have detected that you have already registered using this machine.";
                                TempData["ErrorMessage"] = ReturnCode;
                                if (isSourceAir)
                                    return Redirect("/WatchNow"); //return RedirectToAction("Index", "Air");
                                return RedirectToAction("Register", "User");
                            }
                        }                        
                    }
                    catch (Exception) { }

                    string CurrencyCode = GlobalConfig.DefaultCurrency;
                    var country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);
                    if (country != null)
                        CurrencyCode = country.CurrencyCode;
                    var wallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, CurrencyCode, true) == 0);
                    if (wallet == null) // Wallet does not exist. Create new wallet for User.
                    {
                        wallet = ContextHelper.CreateWallet(0, CurrencyCode, registDt);
                        user.UserWallets.Add(wallet);
                    }

                    var transaction = new RegistrationTransaction()
                    {
                        RegisteredState = user.State,
                        RegisteredCity = user.City,
                        RegisteredCountryCode = user.CountryCode,
                        Amount = 0,
                        Currency = CurrencyCode,
                        Reference = isSourceAir ? "New Registration (air)" : "New Registration",
                        Date = registDt,
                        OfferingId = GlobalConfig.offeringId,
                        UserId = user.UserId,
                        StatusId = GlobalConfig.Visible
                    };
                    user.Transactions.Add(transaction);

                    context.Users.Add(user);
                    if (context.SaveChanges() > 0)
                    {
                        string verification_email = String.Format("{0}/User/Verify?key={1}", GlobalConfig.baseUrl, user.ActivationKey.ToString());
                        if (isSourceAir)
                        {
                            try
                            {
                                verification_email = String.Format("{0}&source=air", verification_email);
                                var template = MyUtility.GetUrlContent(GlobalConfig.ProjectAirEmailVerificationBodyTemplateUrl);
                                var htmlBody = String.Format(template, FirstName, EMail, verification_email);
                                if (!Request.IsLocal)
                                    try { MyUtility.SendEmailViaSendGrid(EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", htmlBody, MailType.HtmlOnly, String.Empty); }
                                    catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }
                            }
                            catch (Exception)
                            {
                                string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, FirstName, EMail, verification_email);
                                if (!Request.IsLocal)
                                    try
                                    {
                                        int productId = MyUtility.StringToIntList(GlobalConfig.FreeTrialProductIdsNEW).First();
                                        MyUtility.SendVerificationEmail(context, user, productId);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }
                            }
                        }
                        else
                        {
                            string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, FirstName, EMail, verification_email);
                            if (!Request.IsLocal)
                                try
                                {
                                    int productId = MyUtility.StringToIntList(GlobalConfig.FreeTrialProductIdsNEW).First();
                                    if (string.Compare(user.CountryCode, "TW") == 0 || Request.Cookies.AllKeys.Contains("vntycok")) //set to config contains country14day
                                        productId = GlobalConfig.FreeTrial14ProductId;
                                    MyUtility.SendVerificationEmail(context, user, productId);
                                }
                                catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }
                        }
                        GSResponse res = null;
                        if (!String.IsNullOrEmpty(uid) && !String.IsNullOrEmpty(provider))
                        {
                            Dictionary<string, object> collection = new Dictionary<string, object>();
                            collection.Add("siteUID", user.UserId);
                            collection.Add("uid", Uri.UnescapeDataString(uid));
                            collection.Add("cid", String.Format("{0} - New User", provider));
                            res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(collection));
                            if (res.GetErrorCode() == 0) //Successful link
                            {
                                if (user != null)
                                {
                                    var UserId = user.UserId.ToString();
                                    user.StatusId = GlobalConfig.Visible; //activate account
                                    user.DateVerified = DateTime.Now;
                                    SetAutheticationCookie(UserId);
                                    SetSession(UserId);
                                    if (!ContextHelper.SaveSessionInDatabase(context, user))
                                        context.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            var info = new GigyaUserInfo()
                            {
                                firstName = FirstName,
                                lastName = LastName,
                                email = EMail
                            };
                            var registrationInfo = new GigyaNotifyLoginInfo()
                            {
                                siteUID = user.UserId.ToString(),
                                cid = "TFCTV - Registration",
                                sessionExpiration = 0,
                                newUser = true,
                                userInfo = Newtonsoft.Json.JsonConvert.SerializeObject(info)
                            };
                            GSObject obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(registrationInfo));
                            res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", obj);

                        }

                        if (user != null)
                        {
                            if (user.StatusId == GlobalConfig.Visible)
                            {
                                int freeTrialProductId = 0;
                                if (GlobalConfig.IsFreeTrialEnabled)
                                {
                                    freeTrialProductId = MyUtility.GetCorrespondingFreeTrialProductId();
                                    if (GlobalConfig.TfcTvFree2StartDate < registDt && GlobalConfig.TfcTvFree2EndDate > registDt)
                                    {
                                        string UserCountryCode = user.CountryCode;
                                        if (!GlobalConfig.isUAT)
                                            try { UserCountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy(); }
                                            catch (Exception) { }

                                        var countryList = GlobalConfig.TfcTvFree2CountryWhiteList.Split(',');
                                        if (countryList.Contains(UserCountryCode) && String.Compare(user.CountryCode, UserCountryCode, true) == 0)
                                            freeTrialProductId = GlobalConfig.TfcTvFree2ProductId;
                                    }

                                    if (Request.Cookies.AllKeys.Contains("vntycok"))
                                        freeTrialProductId = GlobalConfig.FreeTrial14ProductId;
                                    PaymentHelper.PayViaWallet(context, userId, freeTrialProductId, SubscriptionProductType.Package, userId, null);
                                }
                            }
                        }

                        GigyaHelpers.setCookie(res, this.ControllerContext);
                        //GigyaUserData userData = new GigyaUserData()
                        //{
                        //    City = user.City,
                        //    CountryCode = user.CountryCode,
                        //    Email = user.EMail,
                        //    FirstName = user.FirstName,
                        //    LastName = user.LastName,
                        //    State = user.State
                        //};

                        GigyaUserData2 userData = new GigyaUserData2()
                        {
                            city = user.City,
                            country = user.CountryCode,
                            email = user.EMail,
                            firstName = user.FirstName,
                            lastName = user.LastName,
                            state = user.State
                        };

                        //GigyaUserDataInfo userDataInfo = new GigyaUserDataInfo()
                        //{
                        //    UID = user.UserId.ToString(),
                        //    data = Newtonsoft.Json.JsonConvert.SerializeObject(userData, Formatting.None)
                        //};

                        TFCTV.Helpers.UserData privacyData = new UserData() { IsExternalSharingEnabled = "true,false", IsInternalSharingEnabled = "true,false", IsProfilePrivate = "false" };

                        GigyaUserDataInfo2 userDataInfo = new GigyaUserDataInfo2()
                        {
                            UID = user.UserId.ToString(),
                            profile = Newtonsoft.Json.JsonConvert.SerializeObject(userData, Formatting.None),
                            data = Newtonsoft.Json.JsonConvert.SerializeObject(privacyData, Formatting.None)
                        };

                        GSObject userDataInfoObj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(userDataInfo));
                        //res = GigyaHelpers.createAndSendRequest("gcs.setUserData", userDataInfoObj);
                        res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", userDataInfoObj);
                        var returnCode = res.GetErrorCode();

                        //Publish to Activity Feed
                        List<ActionLink> actionlinks = new List<ActionLink>();
                        actionlinks.Add(new ActionLink() { text = SNSTemplates.register_actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_actionlink_href) });
                        //mediaItem
                        List<MediaItem> mediaItems = new List<MediaItem>();
                        mediaItems.Add(new MediaItem() { type = SNSTemplates.register_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.register_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_mediaitem_href) });
                        UserAction action = new UserAction()
                        {
                            actorUID = userId.ToString(),
                            userMessage = SNSTemplates.register_usermessage,
                            title = SNSTemplates.register_title,
                            subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_subtitle),
                            linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_linkback),
                            description = String.Format(SNSTemplates.register_description, FirstName),
                            actionLinks = actionlinks,
                            mediaItems = mediaItems
                        };

                        GigyaMethods.PublishUserAction(action, userId, "external");
                        action.userMessage = String.Empty;
                        action.title = String.Empty;
                        action.mediaItems = null;
                        GigyaMethods.PublishUserAction(action, userId, "internal");

                        TempData["qs"] = null; // empty the TempData upon successful registration

                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                        ReturnCode.info7 = user.EMail;
                        bool IsSocialRegistration = false;
                        if (user.StatusId == GlobalConfig.Visible)
                        {
                            ReturnCode.StatusHeader = "Your 7-Day Free Trial Starts Now!";
                            if (Request.Cookies.AllKeys.Contains("vntycok"))
                            {
                                ReturnCode.StatusHeader = "Your 14-Day Free Trial Starts Now!";
                                HttpCookie vanCookie = new HttpCookie("vntycok");
                                vanCookie.Expires = DateTime.Now.AddDays(-1);
                                Response.Cookies.Add(vanCookie);
                            }
                            ReturnCode.StatusMessage = "Congratulations! You are now registered to TFC.tv.";
                            ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies!";

                            //Change to social registration
                            ReturnCode.info = "SocialRegistration";
                            ReturnCode.TransactionType = "SocialRegistration";
                            IsSocialRegistration = true;
                        }
                        else
                        {
                            ReturnCode.StatusHeader = "Email verification sent!";
                            ReturnCode.StatusMessage = "Congratulations! You are one step away from completing your registration.";
                            ReturnCode.StatusMessage2 = "An email has been sent to you.<br> Verify your email address to complete your registration.";
                        }
                        TempData["ErrorMessage"] = ReturnCode;
                        if (isSourceAir)
                            return Redirect("/WatchNow"); //return RedirectToAction("Index", "Air");
                        //if(xoom)
                        if (Request.Cookies.AllKeys.Contains("xoom"))
                        {
                            var userPromo = new UserPromo();
                            userPromo.UserId = user.UserId;
                            userPromo.PromoId = GlobalConfig.Xoom2PromoId;
                            userPromo.AuditTrail.CreatedOn = registDt;
                            context.UserPromos.Add(userPromo);
                            context.SaveChanges();
                        }

                        if (IsSocialRegistration)
                        {
                            if (MyUtility.isTVECookieValid())
                            {
                                ReturnCode.StatusMessage = "Congratulations! You have now successfully registered to TFC.tv. Please complete the details below to activate your TFC Everywhere.";
                                TempData["ErrorMessage"] = ReturnCode;
                                MyUtility.RemoveTVECookie();
                                return RedirectToAction("RegisterToTFCEverywhere", "User");
                            }
                        }
                        return RedirectToAction("Index", "Home"); // successful registration
                    }
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";


                TempData["ErrorMessage"] = ReturnCode;

                tmpCollection.Remove("login_pass"); tmpCollection.Remove("__RequestVerificationToken");
                url = String.Format("{0}?{1}", Request.UrlReferrer.AbsolutePath, MyUtility.DictionaryToQueryString(tmpCollection));
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Redirect(url);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _Login(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                TransactionType = "Login"
            };

            string url = Url.Action("Index", "Home").ToString();
            url = Url.Action("Login", "User").ToString();
            var field_names = new string[] { "rUri" };
            try
            {
                DateTime registDt = DateTime.Now;
                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;
                foreach (var x in tmpCollection)
                {
                    if (!field_names.Contains(x.Key))
                        if (String.IsNullOrEmpty(x.Value))
                        {
                            isMissingRequiredFields = true;
                            break;
                        }
                }

                if (!isMissingRequiredFields)
                {
                    string EmailAddress = fc["login_email"];
                    string Password = fc["login_pass"];
                    if (!String.IsNullOrEmpty(Request.UrlReferrer.AbsolutePath))
                        url = Request.UrlReferrer.AbsolutePath;

                    //RegexUtilities util = new RegexUtilities();
                    //if (!util.IsValidEmail(EmailAddress))
                    if (!MyUtility.isEmail(EmailAddress))
                    {
                        ReturnCode.StatusMessage = "Email address is invalid.";
                        TempData["LoginErrorMessage"] = ReturnCode.StatusMessage;
                        return Redirect(url);
                    }

                    using (var context = new IPTV2Entities())
                    {
                        User user = null;
                        if (User.Identity.IsAuthenticated)
                        {
                            var UserId = new Guid(User.Identity.Name);
                            user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        }
                        else
                        {
                            user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                            if (user == null)
                                ReturnCode.StatusMessage = "Email address does not exist.";
                            else
                            {
                                if (user.StatusId != GlobalConfig.Visible)
                                    ReturnCode.StatusMessage = "Email address is not verified.";
                                else
                                {
                                    Password = MyUtility.GetSHA1(Password);
                                    if (String.Compare(user.EMail, EmailAddress, true) == 0 && String.Compare(user.Password, Password, false) == 0)
                                    {
                                        SendToGigya(user);
                                        SetAutheticationCookie(user.UserId.ToString());
                                        SetSession(user.UserId.ToString());
                                        ContextHelper.SaveSessionInDatabase(context, user);

                                        //add uid cookie
                                        HttpCookie uidCookie = new HttpCookie("uid");
                                        uidCookie.Value = user.UserId.ToString();
                                        uidCookie.Expires = DateTime.Now.AddDays(30);
                                        Response.Cookies.Add(uidCookie);

                                        //check redirectUrl if present
                                        try
                                        {
                                            if (TempData["RedirectUrl"] != null)
                                                url = (string)TempData["RedirectUrl"];
                                        }
                                        catch (Exception) { }

                                        if (user.IsTVEverywhere == true)
                                            return Redirect("/TFCChannel");
                                        else if (MyUtility.isTVECookieValid())
                                        {
                                            MyUtility.RemoveTVECookie();
                                            return RedirectToAction("RegisterToTFCEverywhere", "User");
                                        }
                                        else
                                        {
                                            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("redirect3178"))
                                            {
                                                HttpCookie pacMayCookie = new HttpCookie("redirect3178");
                                                pacMayCookie.Expires = DateTime.Now.AddDays(-1);
                                                Response.Cookies.Add(pacMayCookie);
                                                return RedirectToAction("Details", "Subscribe", new { id = "mayweather-vs-pacquiao-may-3" });
                                            }
                                            else if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("promo2014cok"))
                                            {
                                                HttpCookie tempCookie = new HttpCookie("promo2014cok");
                                                tempCookie.Expires = DateTime.Now.AddDays(-1);
                                                Response.Cookies.Add(tempCookie);
                                                return RedirectToAction("Details", "Subscribe", new { id = "Promo201410" });
                                            }
                                            else if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("redirectaintone"))
                                            {
                                                HttpCookie tempCookie = new HttpCookie("redirectaintone");
                                                tempCookie.Expires = DateTime.Now.AddDays(-1);
                                                Response.Cookies.Add(tempCookie);
                                                return RedirectToAction("Details", "Subscribe", new { id = "aintone" });
                                            }
                                            else if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("vntysicook"))
                                            {
                                                HttpCookie tempCookie = new HttpCookie("vntysicook");
                                                tempCookie.Expires = DateTime.Now.AddDays(-1);
                                                Response.Cookies.Add(tempCookie);
                                                return RedirectToAction("Index", "Events", new { id = tempCookie.Value });
                                            }

                                            try
                                            {
                                                if (!String.IsNullOrEmpty(fc["rUri"]))
                                                {
                                                    string retUri = Server.UrlDecode(fc["rUri"]);
                                                    if (Url.IsLocalUrl(retUri) && retUri.Length > 1 && retUri.StartsWith("/") && !retUri.StartsWith("//") && !retUri.StartsWith("/\\"))
                                                        return Redirect(retUri);
                                                }
                                            }
                                            catch (Exception) { }
                                            Redirect(url);
                                        }
                                    }
                                    else
                                        ReturnCode.StatusMessage = "Email/Password do not match.";
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                            TempData["LoginErrorMessage"] = ReturnCode.StatusMessage;

                        if (user != null)
                        {
                            if (user.IsTVEverywhere == true)
                                return Redirect("/TFCChannel");
                            else
                                return Redirect(url);
                        }
                    }
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                    TempData["LoginErrorMessage"] = ReturnCode.StatusMessage;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Redirect(url);

        }

        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                //SetAutheticationCookie(User.Identity.Name);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public PartialViewResult GetEntitlements()
        {
            List<EntitlementDisplay> list = null;
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    using (var context = new IPTV2Entities())
                    {
                        var userId = new System.Guid(User.Identity.Name);
                        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            var entitlements = context.Entitlements.Where(t => t.UserId == user.UserId && t.OfferingId == GlobalConfig.offeringId).OrderByDescending(t => t.EndDate);
                            if (entitlements != null)
                            {
                                list = new List<EntitlementDisplay>();
                                foreach (Entitlement entitlement in entitlements)
                                {
                                    EntitlementDisplay disp = new EntitlementDisplay()
                                    {
                                        EntitlementId = entitlement.EntitlementId,
                                        ExpiryDate = entitlement.EndDate,
                                        ExpiryDateStr = entitlement.EndDate.ToString("MMMM d, yyyy")
                                    };

                                    if (entitlement is PackageEntitlement)
                                    {
                                        var pkg = (PackageEntitlement)entitlement;
                                        disp.PackageId = pkg.PackageId;
                                        disp.PackageName = pkg.Package.Description;
                                        disp.Content = disp.PackageName;
                                    }
                                    else if (entitlement is ShowEntitlement)
                                    {
                                        var show = (ShowEntitlement)entitlement;
                                        disp.CategoryId = show.CategoryId;
                                        disp.CategoryName = show.Show.Description;
                                        disp.Content = disp.CategoryName;
                                    }
                                    else if (entitlement is EpisodeEntitlement)
                                    {
                                        var episode = (EpisodeEntitlement)entitlement;
                                        disp.EpisodeId = episode.EpisodeId;
                                        disp.EpisodeName = episode.Episode.Description + ", " + episode.Episode.DateAired.Value.ToString("MMMM d, yyyy");
                                        disp.Content = disp.EpisodeName;
                                    }
                                    list.Add(disp);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(list);
        }

        public PartialViewResult GetTransactions()
        {
            List<TransactionDisplay> list = null;
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    using (var context = new IPTV2Entities())
                    {
                        var userId = new System.Guid(User.Identity.Name);
                        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            var transactions = context.Transactions.Where(t => t.UserId == user.UserId && t.OfferingId == GlobalConfig.offeringId && t.StatusId == GlobalConfig.Visible).OrderByDescending(t => t.TransactionId);

                            if (transactions != null)
                            {
                                list = new List<TransactionDisplay>();
                                foreach (Transaction transaction in transactions)
                                {
                                    TransactionDisplay disp = new TransactionDisplay()
                                    {
                                        TransactionId = transaction.TransactionId,
                                        Reference = transaction.Reference,
                                        Amount = transaction.Amount,
                                        Currency = transaction.Currency,
                                        TransactionDate = transaction.Date,
                                        TransactionDateStr = transaction.Date.ToString("MMMM d, yyyy")
                                    };

                                    if (transaction is PaymentTransaction)
                                    {
                                        PaymentTransaction ptrans = (PaymentTransaction)transaction;
                                        string remarks = ptrans.Purchase.Remarks;
                                        bool purchase = String.IsNullOrEmpty(remarks) ? false : remarks.StartsWith("Gift");
                                        disp.TransactionType = purchase ? "Gift" : "Subscription";
                                        PurchaseItem item = ptrans.Purchase.PurchaseItems.FirstOrDefault();
                                        disp.ProductId = item.ProductId;
                                        Product product = context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                                        if (product != null)
                                            disp.ProductName = product.Description;
                                        if (ptrans is PpcPaymentTransaction)
                                        {
                                            disp.PpcId = ptrans.Reference;
                                            disp.PaymentType = "Prepaid Card/ePIN";
                                        }
                                        else if (ptrans is PaypalPaymentTransaction)
                                            disp.PaymentType = "Paypal";
                                        else if (ptrans is CreditCardPaymentTransaction)
                                            disp.PaymentType = "Credit Card";
                                        else if (ptrans is WalletPaymentTransaction)
                                            disp.PaymentType = "Wallet";
                                        disp.Method = disp.PaymentType;
                                    }
                                    else if (transaction is UpgradeTransaction)
                                    {
                                        UpgradeTransaction utrans = (UpgradeTransaction)transaction;

                                        disp.TransactionType = "Upgrade";
                                        Product product = context.Products.FirstOrDefault(p => p.ProductId == utrans.OriginalProductId);
                                        if (product != null)
                                            disp.ProductName = product.Description;
                                        Product oldProduct = context.Products.FirstOrDefault(p => p.ProductId == utrans.NewProductId);
                                        disp.Reference = String.Format("Upgraded to {0}", oldProduct.Name);
                                        disp.Method = String.Empty;
                                    }
                                    else if (transaction is TfcEverywhereTransaction)
                                    {
                                        TfcEverywhereTransaction ttrans = (TfcEverywhereTransaction)transaction;
                                        disp.TransactionType = "Update";
                                        disp.ProductName = "TFC Everywhere";
                                        disp.Method = String.Empty;
                                        disp.Amount = 0;

                                    }
                                    else if (transaction is CancellationTransaction)
                                    {
                                        CancellationTransaction ctrans = (CancellationTransaction)transaction;
                                        disp.TransactionType = "Cancellation";
                                        //disp.ProductName = String.Format("TID: {0}", ctrans.OriginalTransactionId);
                                        disp.ProductName = String.Empty;
                                        disp.Method = String.Empty;
                                        disp.Amount = 0;

                                    }
                                    else if (transaction is ChangeCountryTransaction)
                                    {
                                        ChangeCountryTransaction ctrans = (ChangeCountryTransaction)transaction;
                                        disp.TransactionType = "Change Country";
                                        disp.ProductName = String.Empty;
                                        disp.Method = String.Empty;
                                        disp.Reference = String.Format("{0} to {1}", ctrans.OldCountryCode, ctrans.NewCountryCode);
                                    }
                                    else if (transaction is MigrationTransaction)
                                    {
                                        MigrationTransaction mtrans = (MigrationTransaction)transaction;
                                        disp.TransactionType = "Migrate License";
                                        disp.ProductName = String.Empty;
                                        if (mtrans.MigratedProductId > 0)
                                        {
                                            Product product = context.Products.FirstOrDefault(p => p.ProductId == mtrans.MigratedProductId);
                                            if (product != null)
                                                disp.ProductName = product.Name;
                                        }
                                        disp.Method = String.Empty;
                                        disp.Reference = mtrans.Reference;
                                    }
                                    else if (transaction is RegistrationTransaction)
                                    {
                                        RegistrationTransaction rgtrans = (RegistrationTransaction)transaction;
                                        disp.TransactionType = "Registration";
                                        disp.ProductName = String.Empty;
                                        disp.Method = String.Empty;
                                    }
                                    else if (transaction is ReloadTransaction)
                                    {
                                        ReloadTransaction rtrans = (ReloadTransaction)transaction;
                                        disp.ProductName = String.Empty;
                                        disp.TransactionType = "Reload";
                                        if (rtrans is PpcReloadTransaction)
                                        {
                                            disp.PpcId = rtrans.Reference;
                                            disp.ReloadType = "Prepaid Card/ePIN";
                                        }
                                        else if (rtrans is PaypalReloadTransaction)
                                            disp.ReloadType = "Paypal";
                                        else if (rtrans is CreditCardReloadTransaction)
                                            disp.ReloadType = "Credit Card";
                                        else if (rtrans is SmartPitReloadTransaction)
                                            disp.ReloadType = "Smart Pit";

                                        disp.Method = disp.ReloadType;
                                    }
                                    list.Add(disp);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _UpdatePassword(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            string url = Url.Action("ChangePassword", "User").ToString();
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home");

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
                    string CurrentPassword = tmpCollection["currentPass"];
                    string NewPassword = tmpCollection["newPass"];
                    string CNewPassword = tmpCollection["cnewPass"];

                    if (String.Compare(NewPassword, CNewPassword, false) != 0)
                        ReturnCode.StatusMessage = "The passwords you entered do not match.";
                    else if (String.Compare(CurrentPassword, NewPassword) == 0)
                        ReturnCode.StatusMessage = "Your current & new passwords are the same. Please choose another password.";
                    else
                    {
                        using (var context = new IPTV2Entities())
                        {
                            var userId = new Guid(User.Identity.Name);
                            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                            if (user != null)
                            {
                                string hashedPassword = MyUtility.GetSHA1(CurrentPassword);
                                if (String.Compare(user.Password, hashedPassword, false) != 0)
                                    ReturnCode.StatusMessage = "The current password you entered is incorrect.";
                                else
                                {
                                    string hashedNewPassword = MyUtility.GetSHA1(NewPassword);
                                    user.Password = hashedNewPassword;
                                    user.LastUpdated = registDt;

                                    if (context.SaveChanges() > 0)
                                    {
                                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                        ReturnCode.StatusHeader = "Change Password Complete!";
                                        ReturnCode.StatusMessage = "You've successfully updated your password.";
                                        ReturnCode.StatusMessage2 = String.Empty;
                                        TempData["ErrorMessage"] = ReturnCode;
                                        return RedirectToAction("Index", "Profile");  //Successful change of password, redirect user to his profile.
                                    }
                                    else
                                        ReturnCode.StatusMessage = "We were unable to change your password. Please try again.";
                                }
                            }
                            else
                                return RedirectToAction("ChangePassword", "User");
                        }
                    }

                    TempData["ErrorMessage"] = ReturnCode;
                    return RedirectToAction("ChangePassword", "User");

                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                    TempData["ErrorMessage"] = ReturnCode.StatusMessage;

                url = Request.UrlReferrer.AbsolutePath;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Redirect(url);
        }

        [RequireHttp]
        public ActionResult EditYourProfile()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home");
                using (var context = new IPTV2Entities())
                {
                    var userId = new Guid(User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                    {
                        var ExcludedCountriesFromRegistrationDropDown = GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',');
                        List<Country> countries = null;
                        if (GlobalConfig.UseCountryListInMemory)
                        {
                            if (GlobalConfig.CountryList != null)
                                countries = GlobalConfig.CountryList.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                            else
                                countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                        }
                        else
                            countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                        ViewBag.ListOfCountries = countries;
                        var location = MyUtility.GetLocationViaIpAddressWithoutProxy();
                        ViewBag.location = location;
                        var CountryCode = user.CountryCode;
                        var states = context.States.Where(s => String.Compare(s.CountryCode, CountryCode, true) == 0);
                        if (states != null)
                            if (states.Count() > 0)
                                ViewBag.ListOfStates = states.ToList();

                        var userData = MyUtility.GetUserPrivacySetting(user.UserId);
                        ViewBag.UserData = userData;
                        ViewBag.RecurringBilling = GetRecurringBillingsForUser(user, context);
                        return View(user);
                    }
                }
            }
            catch (Exception) { }
            return RedirectToAction("Index", "Home");
        }

        private List<RecurringBillingDisplay> GetRecurringBillingsForUser(User user, IPTV2Entities context)
        {
            List<RecurringBillingDisplay> list = null;
            try
            {
                var recurringBillings = context.RecurringBillings.Where(r => r.UserId == user.UserId && r.StatusId == GlobalConfig.Visible);
                if (recurringBillings != null)
                {
                    list = new List<RecurringBillingDisplay>();
                    foreach (var item in recurringBillings)
                    {
                        RecurringBillingDisplay disp = new RecurringBillingDisplay()
                        {
                            EndDate = (DateTime)item.EndDate,
                            EndDateStr = item.EndDate.Value.ToShortDateString(),
                            NextRun = (DateTime)item.NextRun,
                            NextRunStr = item.NextRun.Value.ToShortDateString(),
                            PackageId = item.PackageId,
                            PackageName = item.Package.Description,
                            ProductId = item.ProductId,
                            ProductName = item.Product.Description,
                            RecurringBillingId = item.RecurringBillingId,
                            StatusId = item.StatusId,
                            UserId = item.UserId,
                            isDisabled = false,
                            PaymentType = item is CreditCardRecurringBilling ? "Credit Card" : "Paypal"
                        };
                        list.Add(disp);
                    }
                }
            }
            catch (Exception) { }
            return list;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditUserProfile(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            string url = Url.Action("EditYourProfile", "User").ToString();
            var field_names = new string[] { "recurring_status", "disabled_list", "enabled_list", "internal_share", "external_share", "private_profile" };
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("EditYourProfile", "User");

                DateTime registDt = DateTime.Now;
                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;

                foreach (var x in tmpCollection)
                {
                    if (!field_names.Contains(x.Key))
                        if (String.IsNullOrEmpty(x.Value))
                        {
                            isMissingRequiredFields = true;
                            break;
                        }
                }

                if (!isMissingRequiredFields) // process form
                {
                    string FirstName = tmpCollection["first_name"];
                    string LastName = tmpCollection["last_name"];
                    string CountryCode = tmpCollection["country"];
                    string City = tmpCollection["city"];
                    string State = tmpCollection["state"];
                    string browser = Request.UserAgent;

                    if (FirstName.Length > 32)
                        ReturnCode.StatusMessage = "First Name cannot exceed 32 characters.";
                    if (LastName.Length > 32)
                        ReturnCode.StatusMessage = "Last Name cannot exceed 32 characters.";
                    if (State.Length > 30)
                        ReturnCode.StatusMessage = "State cannot exceed 30 characters.";
                    if (City.Length > 50)
                        ReturnCode.StatusMessage = "City cannot exceed 50 characters.";

                    var context = new IPTV2Entities();
                    var userId = new Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user == null)
                        return RedirectToAction("Index", "Home");

                    if (GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',').Contains(CountryCode)) // check if country is part of the exclusion list first
                        ReturnCode.StatusMessage = "Country does not exist.";
                    else if (context.Countries.Count(c => String.Compare(c.Code, CountryCode, true) == 0) <= 0) // then check if country is part of the list                    
                        ReturnCode.StatusMessage = "Country does not exist.";
                    if (context.States.Count(s => String.Compare(s.CountryCode, CountryCode, true) == 0) > 0)
                        if (context.States.Count(s => String.Compare(s.CountryCode, CountryCode, true) == 0 && (String.Compare(s.StateCode, State, true) == 0 || String.Compare(s.Name, State, true) == 0)) <= 0)
                            ReturnCode.StatusMessage = "State is invalid for this country.";

                    if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                    {
                        TempData["ErrorMessage"] = ReturnCode;
                        return RedirectToAction("EditYourProfile", "User");
                    }

                    string currentCountryCode = user.CountryCode;
                    string newCountryCode = CountryCode;
                    string currentCurrencyCode = user.Country.CurrencyCode;
                    string newCurrencyCode = GlobalConfig.DefaultCurrency;


                    //Update information that is not affected by country change                    
                    user.FirstName = FirstName;
                    user.LastName = LastName;
                    user.LastUpdated = registDt;

                    //Update privacy policy
                    string IsInternalSharingEnabled = fc["internal_share"];
                    string IsExternalSharingEnabled = fc["external_share"];
                    string IsProfilePrivate = fc["private_profile"];

                    try
                    {
                        UserData userData = new UserData()
                        {
                            IsInternalSharingEnabled = IsInternalSharingEnabled,
                            IsExternalSharingEnabled = IsExternalSharingEnabled,
                            IsProfilePrivate = IsProfilePrivate
                        };
                        GigyaMethods.SetUserData(user.UserId, userData);
                    }
                    catch (Exception) { }

                    // Update recurring billing
                    if (!String.IsNullOrEmpty(fc["disabled_list"]))
                        UpdateRecurringBillingViaEditProfile2(context, user, fc["disabled_list"], false);
                    if (!String.IsNullOrEmpty(fc["enabled_list"]))
                        UpdateRecurringBillingViaEditProfile2(context, user, fc["enabled_list"], true);

                    //Check if country of user changed
                    if (String.Compare(user.CountryCode, CountryCode, true) != 0)
                    {
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        //Get User Transactions
                        if (user.HasOtherPendingGomsTransaction(offering))
                            ReturnCode.StatusMessage = "We are still processing your transactions. Please try again after a few minutes.";
                        else if (user.HasPendingGomsChangeCountryTransaction(offering))
                            ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                        else if (user.HasTVEverywhereEntitlement(MyUtility.StringToIntList(GlobalConfig.TVEverywherePackageIds), offering))
                            ReturnCode.StatusMessage = "You are not allowed to change country being a TV Everywhere user.";
                        else
                        {
                            var newCountry = context.Countries.FirstOrDefault(c => String.Compare(c.Code, newCountryCode, true) == 0);
                            if (newCountry != null)
                                newCurrencyCode = newCountry.CurrencyCode;

                            var currentWallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, currentCurrencyCode, true) == 0);
                            if (currentWallet == null) //If no wallet, get default USD wallet.
                                currentWallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, GlobalConfig.DefaultCurrency, true) == 0);
                            var newWallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, newCurrencyCode, true) == 0);

                            decimal balance = 0;
                            decimal currentWalletBalance = currentWallet.Balance;
                            var currentGomsWalletId = currentWallet.GomsWalletId;
                            if (newWallet == null) // new wallet currency does not exist. create new wallet for user
                            {
                                if (currentWallet != null)
                                {
                                    balance = Forex.Convert(context, currentWallet.Currency, newCurrencyCode, currentWallet.Balance);
                                    currentWallet.Balance = 0;
                                    currentWallet.IsActive = false;
                                    currentWallet.LastUpdated = registDt;
                                    var wallet = ContextHelper.CreateWallet(balance, newCurrencyCode, registDt);
                                    user.UserWallets.Add(wallet);
                                }
                            }
                            else  // new wallet currency exists, update the balance onl
                            {
                                if (String.Compare(newCurrencyCode, currentCurrencyCode, true) != 0)
                                {
                                    balance = Forex.Convert(context, currentWallet.Currency, newWallet.Currency, currentWallet.Balance);
                                    newWallet.Balance = balance;
                                    newWallet.IsActive = true;
                                    newWallet.LastUpdated = registDt;
                                    newWallet.GomsWalletId = null; // Reset Goms WalletId

                                    currentWallet.Balance = 0; // Deactivate old wallet
                                    currentWallet.IsActive = false; //Deactivate                                
                                    currentWallet.LastUpdated = registDt;
                                }
                                else
                                {
                                    newWallet.GomsWalletId = null;
                                    newWallet.LastUpdated = registDt;
                                }
                            }

                            ChangeCountryTransaction transaction = new ChangeCountryTransaction()
                            {
                                OldCountryCode = currentCountryCode,
                                NewCountryCode = newCountryCode,
                                Date = registDt,
                                OfferingId = GlobalConfig.offeringId,
                                Reference = "Change Country",
                                UserId = user.UserId,
                                Amount = 0,
                                NewWalletBalance = balance,
                                OldWalletBalance = currentWalletBalance,
                                OldGomsCustomerId = user.GomsCustomerId,
                                OldGomsWalletId = currentGomsWalletId,
                                Currency = currentCurrencyCode,
                                StatusId = GlobalConfig.Visible
                            };
                            user.Transactions.Add(transaction);
                            user.CountryCode = CountryCode;
                        }

                        if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                        {
                            TempData["ErrorMessage"] = ReturnCode;
                            return RedirectToAction("EditYourProfile", "User");
                        }

                        user.State = State;
                        user.City = City;


                        if (context.SaveChanges() > 0)
                        {
                            setUserData(user.UserId.ToString(), user);
                            ReturnCode.StatusCode = (int)ErrorCodes.Success;
                            ReturnCode.StatusHeader = "Profile update complete!";
                            ReturnCode.StatusMessage = "You should see the changes on your profile automatically.";
                            ReturnCode.StatusMessage2 = String.Empty;
                            TempData["ErrorMessage"] = ReturnCode;
                            return RedirectToAction("Index", "Profile");
                        }
                        else
                            ReturnCode.StatusMessage = "Ooops! We encountered a problem updating your profile. Please try again later.";
                    }
                    else
                    {
                        user.City = City;
                        user.State = State;
                        if (context.SaveChanges() > 0)
                        {
                            setUserData(user.UserId.ToString(), user);
                            ReturnCode.StatusCode = (int)ErrorCodes.Success;
                            ReturnCode.StatusHeader = "Profile update complete!";
                            ReturnCode.StatusMessage = "You should see the changes on your profile automatically.";
                            ReturnCode.StatusMessage2 = String.Empty;
                            TempData["ErrorMessage"] = ReturnCode;
                            return RedirectToAction("Index", "Profile");
                        }
                        else
                            ReturnCode.StatusMessage = "The system encountered an unspecified error while updating your profile. Please contact support.";
                    }
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                TempData["ErrorMessage"] = ReturnCode;
                url = Request.UrlReferrer.AbsolutePath;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException en)
            {
                MyUtility.LogException((Exception)en, string.Join(",", en.EntityValidationErrors).ToString());
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Redirect(url);
        }


        private void UpdateRecurringBillingViaEditProfile2(IPTV2Entities context, User user, string list, bool? enable)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var rb_list = list.Replace("rs", "");
                    var billingIds = MyUtility.StringToIntList(rb_list);
                    DateTime registDt = DateTime.Now;
                    if (billingIds.Count() > 0)
                    {
                        var gomsService = new GomsTfcTv();
                        var recurring_billings = user.RecurringBillings.Where(r => billingIds.Contains(r.RecurringBillingId) && r.StatusId != 2);
                        if (recurring_billings != null)
                        {
                            if (recurring_billings.Count() > 0)
                            {
                                bool RecurringStatus = false;
                                if (enable != null)
                                    RecurringStatus = (bool)enable;

                                var CurrencyCode = GlobalConfig.DefaultCurrency;
                                try { CurrencyCode = user.Country.CurrencyCode; }
                                catch (Exception) { }

                                foreach (var billing in recurring_billings)
                                {
                                    string cancellation_remarks = String.Empty;
                                    string reference = String.Empty;
                                    bool serviceUpdateSuccess = false;

                                    if (user.RecurringBillings.Count(r => r.PackageId == billing.PackageId && r.StatusId == GlobalConfig.Visible && r.RecurringBillingId != billing.RecurringBillingId) > 0)
                                    {
                                        //there is same package with recurring enabled.                                            
                                    }
                                    else
                                    {
                                        if (billing is PaypalRecurringBilling)
                                        {
                                            try
                                            {
                                                var paypalrbilling = (PaypalRecurringBilling)billing;

                                                billing.StatusId = RecurringStatus ? 1 : 0;
                                                billing.UpdatedOn = registDt;
                                                if (registDt.Date > billing.NextRun && RecurringStatus)
                                                    billing.NextRun = registDt.AddDays(1).Date;
                                                if (!RecurringStatus)
                                                {
                                                    try
                                                    {
                                                        if (PaymentHelper.CancelPaypalRecurring(paypalrbilling.SubscriberId))
                                                        {
                                                            reference = String.Format("PayPal billing id {0} cancelled", billing.RecurringBillingId);
                                                            cancellation_remarks = String.Format("{0} - PayPal Recurring Billing Id cancelled", billing.RecurringBillingId);
                                                            serviceUpdateSuccess = true;
                                                        }
                                                    }
                                                    catch (Exception) { }
                                                }
                                            }
                                            catch (Exception) { }
                                        }
                                        else
                                        {
                                            billing.StatusId = RecurringStatus ? 1 : 0;
                                            billing.UpdatedOn = registDt;
                                            if (registDt.Date > billing.NextRun && RecurringStatus)
                                                billing.NextRun = registDt.AddDays(1).Date;

                                            if (!RecurringStatus)
                                            {
                                                try
                                                {
                                                    var result = gomsService.CancelRecurringPayment(user, billing.Product);
                                                    if (result.IsSuccess)
                                                    {
                                                        reference = String.Format("Credit Card billing id {0} cancelled", billing.RecurringBillingId);
                                                        cancellation_remarks = String.Format("{0} - Credit Card Recurring Billing Id cancelled", billing.RecurringBillingId);
                                                        serviceUpdateSuccess = true;
                                                    }
                                                    else
                                                        throw new Exception(result.StatusMessage);
                                                }
                                                catch (Exception e) { MyUtility.LogException(e); }
                                            }
                                        }

                                        if (!RecurringStatus && serviceUpdateSuccess)
                                        {
                                            var transaction = new CancellationTransaction()
                                            {
                                                Amount = 0,
                                                Currency = CurrencyCode,
                                                OfferingId = GlobalConfig.offeringId,
                                                CancellationRemarks = cancellation_remarks,
                                                OriginalTransactionId = -1,
                                                GomsTransactionId = -1000,
                                                Date = registDt,
                                                Reference = reference,
                                                StatusId = GlobalConfig.Visible
                                            };
                                            user.Transactions.Add(transaction);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
        }


        private void UpdateRecurringBillingViaEditProfile(IPTV2Entities context, User user, string list, bool? enable)
        {
            try
            {
                DateTime registDt = DateTime.Now;
                var rb_list = list.Split(',');

                var gomsService = new GomsTfcTv();

                if (rb_list.Count() > 0)
                {
                    foreach (var item in rb_list)
                    {
                        if (!String.IsNullOrEmpty(item))
                        {
                            var name = item;
                            if (MyUtility.isUserLoggedIn())
                            {
                                bool RecurringStatus = false;
                                if (enable != null)
                                    RecurringStatus = (bool)enable;

                                int RecurringBillingId = 0;
                                try { RecurringBillingId = Convert.ToInt32(name.Substring(2)); }
                                catch (Exception e) { MyUtility.LogException(e); }

                                if (user != null)
                                {
                                    var CurrencyCode = GlobalConfig.DefaultCurrency;
                                    try { CurrencyCode = user.Country.CurrencyCode; }
                                    catch (Exception) { }
                                    string cancellation_remarks = String.Empty;
                                    string reference = String.Empty;
                                    bool serviceUpdateSuccess = false;
                                    var billing = user.RecurringBillings.FirstOrDefault(r => r.RecurringBillingId == RecurringBillingId && r.StatusId != 2);
                                    if (billing != null)
                                    {
                                        //Check first if there is a same package with recurring turned on
                                        if (user.RecurringBillings.Count(r => r.PackageId == billing.PackageId && r.StatusId == GlobalConfig.Visible && r.RecurringBillingId != billing.RecurringBillingId) > 0)
                                        {
                                            //there is same package with recurring enabled.                                            
                                        }
                                        else
                                        {
                                            if (billing is PaypalRecurringBilling)
                                            {
                                                try
                                                {
                                                    var paypalrbilling = (PaypalRecurringBilling)billing;

                                                    billing.StatusId = RecurringStatus ? 1 : 0;
                                                    billing.UpdatedOn = registDt;
                                                    if (registDt.Date > billing.NextRun && RecurringStatus)
                                                        billing.NextRun = registDt.AddDays(1).Date;
                                                    if (!RecurringStatus)
                                                    {
                                                        try
                                                        {
                                                            if (PaymentHelper.CancelPaypalRecurring(paypalrbilling.SubscriberId))
                                                            {
                                                                reference = String.Format("PayPal Payment Renewal {0} cancelled", billing.RecurringBillingId);
                                                                String.Format("{0} - PayPal Recurring Billing Id cancelled", billing.RecurringBillingId);
                                                                serviceUpdateSuccess = true;
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                    }
                                                }
                                                catch (Exception) { }
                                            }
                                            else
                                            {
                                                if (RecurringStatus)
                                                {
                                                    billing.StatusId = RecurringStatus ? 1 : 0;
                                                    billing.UpdatedOn = registDt;
                                                    if (registDt.Date > billing.NextRun && RecurringStatus)
                                                        billing.NextRun = registDt.AddDays(1).Date;
                                                }
                                                else //if (!RecurringStatus)
                                                {
                                                    try
                                                    {
                                                        var result = gomsService.CancelRecurringPayment(user, billing.Product);
                                                        if (result.IsSuccess)
                                                        {
                                                            billing.StatusId = RecurringStatus ? 1 : 0;
                                                            billing.UpdatedOn = registDt;
                                                            reference = String.Format("Credit Card Payment Renewal {0} cancelled", billing.RecurringBillingId);
                                                            cancellation_remarks = String.Format("{0} - Credit Card Recurring Billing Id cancelled", billing.RecurringBillingId);
                                                            serviceUpdateSuccess = true;
                                                        }
                                                        else
                                                            throw new Exception(result.StatusMessage);
                                                    }
                                                    catch (Exception e) { MyUtility.LogException(e); }
                                                }
                                            }

                                            if (!RecurringStatus && serviceUpdateSuccess)
                                            {
                                                var transaction = new CancellationTransaction()
                                                {
                                                    Amount = 0,
                                                    Currency = CurrencyCode,
                                                    OfferingId = GlobalConfig.offeringId,
                                                    CancellationRemarks = cancellation_remarks,
                                                    OriginalTransactionId = -1,
                                                    GomsTransactionId = -1000,
                                                    Date = registDt,
                                                    Reference = reference,
                                                    StatusId = GlobalConfig.Visible
                                                };
                                                user.Transactions.Add(transaction);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
        }

        [HttpPost]
        public JsonResult RemoveConnection(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    using (var context = new IPTV2Entities())
                    {
                        var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
                        if (user != null)
                        {
                            Dictionary<string, string> collection = new Dictionary<string, string>();
                            collection.Add("uid", user.UserId.ToString());
                            if (!String.IsNullOrEmpty(fc["provider"]))
                                collection.Add("provider", fc["provider"]);
                            collection.Add("format", "json");
                            GSObject obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(collection));
                            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.removeConnection", obj);
                            var resp = Newtonsoft.Json.JsonConvert.DeserializeObject<DeleteAccountObj>(res.GetData().ToJsonString());
                            ReturnCode.StatusCode = resp.errorCode;
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
        public ActionResult _RegisterTFCEverywhere(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            string url = Url.Action("RegisterTFCEverywhere", "User").ToString();
            var field_names = new string[] { "smartCardNum", "cusAccount" };
            try
            {
                if (TempData["qs"] != null)
                {
                    var qs = (NameValueCollection)TempData["qs"];
                    ViewBag.qs = qs;
                    TempData["qs"] = qs;
                }

                DateTime registDt = DateTime.Now;
                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;

                foreach (var x in tmpCollection)
                {
                    if (!field_names.Contains(x.Key))
                        if (String.IsNullOrEmpty(x.Value))
                        {
                            isMissingRequiredFields = true;
                            break;
                        }
                }

                if (!isMissingRequiredFields) // process form
                {
                    if (HasConsumedNumberOfRetriesForTFCEverywhere())
                    {
                        ReturnCode.StatusMessage = "Invalid data entered. Please call our Customer Service at 18778846832 or chat with our live support team for assistance.";
                        TempData["ErrorMessage"] = ReturnCode;
                        return RedirectToAction("RegisterTFCEverywhere", "User");
                    }

                    if (String.IsNullOrEmpty(fc["smartCardNum"]) && String.IsNullOrEmpty(fc["cusAccount"]))
                    {
                        ReturnCode.StatusMessage = "Please fill up all the required fields.";
                        TempData["ErrorMessage"] = ReturnCode;
                        return RedirectToAction("RegisterTFCEverywhere", "User");
                    }

                    var context = new IPTV2Entities();
                    User user = null;
                    if (User.Identity.IsAuthenticated)
                    {
                        string CurrencyCode = GlobalConfig.DefaultCurrency;
                        var UserId = new Guid(User.Identity.Name);
                        user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            CurrencyCode = user.Country.CurrencyCode;

                            var transaction = new TfcEverywhereTransaction()
                            {
                                Amount = 0,
                                Date = registDt,
                                Currency = CurrencyCode,
                                OfferingId = GlobalConfig.offeringId,
                                StatusId = GlobalConfig.Visible,
                                Reference = "TFC Everywhere - CLAIM",
                                UserId = user.UserId
                            };

                            var gomsService = new GomsTfcTv();

                            var MacAddressOrSmartCard = fc["smartCardNum"].Replace(" ", "");
                            var AccountNumber = fc["cusAccount"].Replace(" ", "");
                            var ActivationNumber = fc["actCode"].Replace(" ", "");

                            var response = gomsService.ClaimTVEverywhere(context, user.UserId, transaction, MacAddressOrSmartCard, AccountNumber, ActivationNumber);
                            if (response.IsSuccess)
                            {
                                AddTfcEverywhereEntitlement(context, response.TFCTVSubItemId, response.ExpiryDate, response.TVEServiceId, user);
                                transaction.GomsTFCEverywhereEndDate = Convert.ToDateTime(response.ExpiryDate);
                                transaction.GomsTFCEverywhereStartDate = registDt;
                                user.Transactions.Add(transaction);
                                user.IsTVEverywhere = true;
                                if (context.SaveChanges() > 0)
                                {
                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                    ReturnCode.info = "TFC Everywhere Activation";
                                    ReturnCode.CCStatusMessage = "Congratulations! Your TFC Everywhere is now activated.";
                                    ReturnCode.StatusMessage = "Pwede ka nang manood ng piling Kapamilya shows at<br>movies mula sa paborito niyong TFC Channels.";
                                    TempData["ErrorMessage"] = ReturnCode;
                                    return RedirectToAction("Index", "Home"); // successful tve activation                                    
                                }
                            }
                            else
                            {
                                SetNumberOfTriesForTFCEverywhereCookie();
                                if (String.Compare(response.StatusCode, "8", true) == 0)
                                    ReturnCode.StatusMessage = "Go to your Edit My Profile page to update the last name registered on your TFC.tv account.";
                                else
                                    ReturnCode.StatusMessage = response.StatusMessage;
                            }
                        }
                        else
                            ReturnCode.StatusMessage = "User does not exist.";
                    }
                    else
                        ReturnCode.StatusMessage = "You are not logged in.";
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";
                TempData["ErrorMessage"] = ReturnCode;
                url = Request.UrlReferrer.AbsolutePath;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Redirect(url);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult ForgotPassword(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = "The system encountered an unspecified error. Please contact Customer Support.",
                info = "Forget Password"
            };

            string url = Url.Action("Index", "Home").ToString();
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

                if (!isMissingRequiredFields)
                {
                    string EmailAddress = fc["forgotpassword_email"];
                    var context = new IPTV2Entities();
                    var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                    if (user == null)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.UserDoesNotExist;
                        ReturnCode.StatusMessage = "User does not exist.";
                        TempData["ForgotPasswordErrorMessage"] = ReturnCode.StatusMessage;
                        return Redirect(Request.UrlReferrer.AbsoluteUri);
                    }

                    if (user.StatusId != 1)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.UserDoesNotExist;
                        ReturnCode.StatusMessage = "Email address is not verified.";
                        TempData["ForgotPasswordErrorMessage"] = ReturnCode.StatusMessage;
                        return Redirect(Request.UrlReferrer.AbsoluteUri);
                    }

                    user.LastUpdated = registDt;
                    if (context.SaveChanges() > 0)
                    {
                        string oid = MyUtility.GetSHA1(String.Format("{0}{1}", user.UserId, user.LastUpdated));
                        string reset_pwd_email = String.Format("{0}/User/ResetPassword?key={1}&oid={2}", GlobalConfig.baseUrl, user.ActivationKey, oid.ToLower());
                        string emailBody = String.Format(GlobalConfig.ResetPasswordBodyTextOnly, user.FirstName, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), reset_pwd_email);
                        try
                        {
                            MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, "Reset your TFC.tv Password", emailBody, MailType.TextOnly, emailBody);
                            ReturnCode.StatusCode = (int)ErrorCodes.Success;
                            ReturnCode.StatusMessage = "Instructions on how to reset your password have been sent to your email address.";
                        }
                        catch (Exception e)
                        {
                            MyUtility.LogException(e);
                            ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                            ReturnCode.StatusMessage = "The system encountered an unspecified error. Please contact Customer Support.";
                            TempData["ForgotPasswordErrorMessage"] = ReturnCode.StatusMessage;
                            return Redirect(Request.UrlReferrer.AbsoluteUri);
                        }
                    }
                    else
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                        ReturnCode.StatusMessage = "The system was unable to process your request. Please try again later.";
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }

            if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                TempData["ForgotPasswordErrorMessage"] = ReturnCode.StatusMessage;
            url = Request.UrlReferrer.AbsoluteUri;
            return Redirect(url);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult ResendVerification(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = "The system encountered an unspecified error. Please contact Customer Support.",
                info = "Resend Verification"
            };

            string url = Url.Action("Index", "Home").ToString();
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

                if (!isMissingRequiredFields)
                {
                    string EmailAddress = fc["resendverification_email"];

                    //RegexUtilities util = new RegexUtilities();
                    if (!MyUtility.isEmail(EmailAddress))
                    {
                        ReturnCode.StatusMessage = "Email address is invalid.";
                        TempData["ResendVerificationErrorMessage"] = ReturnCode.StatusMessage;
                        return Redirect(Request.UrlReferrer.AbsoluteUri);
                    }

                    var context = new IPTV2Entities();
                    var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                    if (user == null)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.UserDoesNotExist;
                        ReturnCode.StatusMessage = "Email address is not registered to TFC.tv";
                        TempData["ResendVerificationErrorMessage"] = ReturnCode.StatusMessage;
                        return Redirect(Request.UrlReferrer.AbsoluteUri);
                    }

                    if (user.StatusId == GlobalConfig.Visible)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                        ReturnCode.StatusMessage = "This email is already activated.";
                        TempData["ResendVerificationErrorMessage"] = ReturnCode.StatusMessage;
                        return Redirect(Request.UrlReferrer.AbsoluteUri);
                    }

                    string verification_email = String.Format("{0}/User/Verify?key={1}", GlobalConfig.baseUrl, user.ActivationKey.ToString());
                    string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, user.FirstName, user.EMail, verification_email);

                    MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody, MailType.TextOnly, emailBody);
                    ReturnCode.StatusMessage = "The verification email has been sent.";
                    TempData["ResendVerificationErrorMessage"] = ReturnCode.StatusMessage;
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }

            if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                TempData["ResendVerificationErrorMessage"] = ReturnCode.StatusMessage;
            url = Request.UrlReferrer.AbsoluteUri;
            return Redirect(url);
        }

        [RequireHttps]
        public ActionResult MobileSignIn()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[RequireHttps]
        public ActionResult MobileLogin(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            string url = Url.Action("Index", "Home").ToString();
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

                if (!isMissingRequiredFields)
                {
                    string EmailAddress = fc["login_email"];
                    string Password = fc["login_pass"];
                    if (!String.IsNullOrEmpty(Request.UrlReferrer.AbsolutePath))
                        url = Request.UrlReferrer.AbsolutePath;

                    RegexUtilities util = new RegexUtilities();
                    //if (!MyUtility.isEmail(EmailAddress))
                    if (!util.IsValidEmail(EmailAddress))
                    {
                        ReturnCode.StatusMessage = "Email address is invalid.";
                        TempData["LoginErrorMessage"] = ReturnCode.StatusMessage;
                        return Redirect(url);
                    }

                    using (var context = new IPTV2Entities())
                    {
                        User user = null;
                        if (User.Identity.IsAuthenticated)
                        {
                            var UserId = new Guid(User.Identity.Name);
                            user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        }
                        else
                        {
                            user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                            if (user == null)
                                ReturnCode.StatusMessage = "Email address does not exist.";
                            else
                            {
                                if (user.StatusId != GlobalConfig.Visible)
                                    ReturnCode.StatusMessage = "Email address is not verified.";
                                else
                                {
                                    Password = MyUtility.GetSHA1(Password);
                                    if (String.Compare(user.EMail, EmailAddress, true) == 0 && String.Compare(user.Password, Password, false) == 0)
                                    {
                                        SendToGigya(user);
                                        SetAutheticationCookie(user.UserId.ToString());
                                        SetSession(user.UserId.ToString());
                                        ContextHelper.SaveSessionInDatabase(context, user);

                                        //add uid cookie
                                        HttpCookie uidCookie = new HttpCookie("uid");
                                        uidCookie.Value = user.UserId.ToString();
                                        uidCookie.Expires = DateTime.Now.AddDays(30);
                                        Response.Cookies.Add(uidCookie);

                                        if (user.IsTVEverywhere == true)
                                            return Redirect("/TFCChannel");
                                        else if (MyUtility.isTVECookieValid())
                                        {
                                            MyUtility.RemoveTVECookie();
                                            return RedirectToAction("RegisterToTFCEverywhere", "User");
                                        }

                                        if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("redirect3178"))
                                        {
                                            HttpCookie pacMayCookie = new HttpCookie("redirect3178");
                                            pacMayCookie.Expires = DateTime.Now.AddDays(-1);
                                            Response.Cookies.Add(pacMayCookie);
                                            return RedirectToAction("Details", "Subscribe", new { id = "mayweather-vs-pacquiao-may-3" });
                                        }
                                        else if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("promo2014cok"))
                                        {
                                            HttpCookie tempCookie = new HttpCookie("promo2014cok");
                                            tempCookie.Expires = DateTime.Now.AddDays(-1);
                                            Response.Cookies.Add(tempCookie);
                                            return RedirectToAction("Details", "Subscribe", new { id = "Promo201410" });
                                        }
                                        else if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("redirectaintone"))
                                        {
                                            HttpCookie tempCookie = new HttpCookie("redirectaintone");
                                            tempCookie.Expires = DateTime.Now.AddDays(-1);
                                            Response.Cookies.Add(tempCookie);
                                            return RedirectToAction("Details", "Subscribe", new { id = "aintone" });
                                        }
                                        else if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("vntysicook"))
                                        {
                                            HttpCookie tempCookie = new HttpCookie("vntysicook");
                                            tempCookie.Expires = DateTime.Now.AddDays(-1);
                                            Response.Cookies.Add(tempCookie);
                                            return RedirectToAction("Index", "Events", new { id = tempCookie.Value });
                                        }
                                        return RedirectToAction("Index", "Home");
                                    }
                                    else
                                        ReturnCode.StatusMessage = "Email/Password do not match.";
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                            TempData["LoginErrorMessage"] = ReturnCode.StatusMessage;

                        if (user != null)
                        {
                            if (user.IsTVEverywhere == true)
                                return Redirect("/TFCChannel");
                            else
                                return Redirect(url);
                        }
                    }
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                    TempData["LoginErrorMessage"] = ReturnCode.StatusMessage;
                url = Request.UrlReferrer.AbsoluteUri;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Redirect(url);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ResendTVEActivationCode(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            string url = Url.Action("Index", "Home").ToString();
            try
            {
                if (String.IsNullOrEmpty(fc["smartCardNum"]) && String.IsNullOrEmpty(fc["cusAccount"]))
                {
                    ReturnCode.StatusMessage = "Please fill up the required fields.";
                }
                else
                {
                    if (!User.Identity.IsAuthenticated)
                    {
                        ReturnCode.StatusMessage = "Your session has expired. Please login again.";
                    }
                    else
                    {
                        var context = new IPTV2Entities();
                        var UserId = new Guid(User.Identity.Name);
                        User user = context.Users.FirstOrDefault(u => u.UserId == UserId);

                        if (user != null)
                        {
                            var gomsService = new GomsTfcTv();
                            var response = gomsService.ResendTVEActivationCode(user.EMail, fc["smartCardNum"], fc["cusAccount"], user.LastName);
                            if (response.IsSuccess)
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                ReturnCode.StatusMessage = response.StatusMessage;
                            }
                            else
                                ReturnCode.StatusMessage = response.StatusMessage;
                        }
                        else
                            ReturnCode.StatusMessage = "User does not exist.";
                    }
                }
            }
            catch (Exception e) { ReturnCode.StatusMessage = e.Message; }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        private void SetUserTrackingCookie(string UserId)
        {
            //add uid cookie
            HttpCookie uidCookie = new HttpCookie("uid");
            uidCookie.Value = UserId;
            uidCookie.Expires = DateTime.Now.AddDays(30);
            Response.Cookies.Add(uidCookie);
        }


        [HttpPost]
        [RequireHttps]
        [ValidateAntiForgeryToken]
        public ActionResult _ResetYourPassword(FormCollection fc)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            string url = Url.Action("ResetPassword", "User").ToString();
            try
            {
                if (User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home");

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
                    string NewPassword = tmpCollection["newPass"];
                    string CNewPassword = tmpCollection["cnewPass"];

                    string oid = tmpCollection["oid"];
                    string key = tmpCollection["key"];

                    //ViewBag.oid = oid;
                    //ViewBag.key = key;
                    //ViewBag.ts = MyUtility.ConvertToTimestamp(registDt);

                    Guid temp_key;
                    try
                    {
                        temp_key = Guid.Parse(key);
                    }
                    catch (Exception) { ReturnCode.StatusMessage = "The page you requested has an invalid key (key)."; }

                    url = Url.Action("ResetPassword", "User", new { key = key, oid = oid }).ToString();

                    if (String.Compare(NewPassword, CNewPassword, false) != 0)
                        ReturnCode.StatusMessage = "The passwords you entered do not match.";
                    else
                    {
                        using (var context = new IPTV2Entities())
                        {
                            var activation_key = new Guid(key);
                            User user = context.Users.FirstOrDefault(u => u.ActivationKey == activation_key);
                            if (user != null)
                            {
                                string oid_base = MyUtility.GetSHA1(String.Format("{0}{1}", user.UserId, user.LastUpdated));

                                if (registDt.Subtract(user.LastUpdated).TotalSeconds > 86400)
                                    ReturnCode.StatusMessage = "The page you requested has already expired.";
                                else if (String.Compare(oid, oid_base, true) != 0)
                                    ReturnCode.StatusMessage = "The page you requested has an invalid key (oid).";
                                else
                                {
                                    string hashedNewPassword = MyUtility.GetSHA1(NewPassword);
                                    user.Password = hashedNewPassword;
                                    user.LastUpdated = registDt;
                                    if (context.SaveChanges() > 0)
                                    {
                                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                        ReturnCode.StatusHeader = "Reset Password Complete!";
                                        ReturnCode.StatusMessage = "You've successfully updated your password.";
                                        ReturnCode.StatusMessage2 = String.Empty;
                                        TempData["ErrorMessage"] = ReturnCode;
                                        return RedirectToAction("Index", "Home");  //Successful reset of password, redirect user login
                                    }
                                    else
                                        ReturnCode.StatusMessage = "We were unable to reset your password. Please try again.";
                                }
                            }
                            else
                                return RedirectToAction("ResetPassword", "User", new { key = key, oid = oid });
                        }
                    }
                    TempData["ErrorMessage"] = ReturnCode;
                    return RedirectToAction("ResetPassword", "User", new { key = key, oid = oid });
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                    TempData["ErrorMessage"] = ReturnCode.StatusMessage;

            }
            catch (Exception e) { MyUtility.LogException(e); ReturnCode.StatusMessage = "The system encountered an unspecified error."; }
            return Redirect(url);

        }

        [HttpPost]
        [RequireHttps]
        public JsonResult CancelRecurring(int? pid)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            DateTime registDt = DateTime.Now;
            try
            {
                if (pid == null)
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Request is not valid";
                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }
                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Request is not valid";
                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (User.Identity.IsAuthenticated)
                {
                    var context = new IPTV2Entities();
                    var userId = new Guid(User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                    {
                        var product = context.Products.FirstOrDefault(p => p.ProductId == (int)pid);
                        if (product != null)
                        {
                            if (product is SubscriptionProduct)
                            {
                                var subscription_product = (SubscriptionProduct)product;
                                var packageIds = subscription_product.ProductGroup.GetPackageIds(true);
                                var CurrencyCode = GlobalConfig.DefaultCurrency;
                                try { CurrencyCode = user.Country.CurrencyCode; }
                                catch (Exception) { }
                                var rb = context.RecurringBillings.Where(r => r.UserId == user.UserId && r.StatusId == GlobalConfig.Visible && packageIds.Contains(r.PackageId));
                                if (rb != null)
                                {
                                    if (rb.Count() > 0)
                                    {
                                        var gomsService = new GomsTfcTv();
                                        foreach (var billing in rb)
                                        {
                                            string reference = String.Empty;
                                            bool serviceUpdateSuccess = false;
                                            string cancellation_remarks = String.Empty;
                                            if (billing is PaypalRecurringBilling)
                                            {
                                                try
                                                {
                                                    var paypalrbilling = (PaypalRecurringBilling)billing;
                                                    billing.StatusId = 0;
                                                    billing.UpdatedOn = registDt;
                                                    try
                                                    {
                                                        if (PaymentHelper.CancelPaypalRecurring(paypalrbilling.SubscriberId))
                                                        {
                                                            reference = String.Format("PayPal Payment Renewal id {0} cancelled", billing.RecurringBillingId);
                                                            String.Format("{0} - PayPal Recurring Billing Id cancelled", billing.RecurringBillingId);
                                                            serviceUpdateSuccess = true;
                                                        }
                                                    }
                                                    catch (Exception) { }
                                                }
                                                catch (Exception) { }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    var result = gomsService.CancelRecurringPayment(user, billing.Product);
                                                    if (result.IsSuccess)
                                                    {
                                                        billing.StatusId = 0;
                                                        billing.UpdatedOn = registDt;
                                                        reference = String.Format("Credit Card Payment Renewal {0} cancelled", billing.RecurringBillingId);
                                                        cancellation_remarks = String.Format("{0} - Credit Card Recurring Billing Id cancelled", billing.RecurringBillingId);
                                                        serviceUpdateSuccess = true;
                                                    }
                                                    else
                                                    {
                                                        ReturnCode.StatusMessage = result.StatusMessage;
                                                        throw new Exception(result.StatusMessage);
                                                    }

                                                }
                                                catch (Exception) { }
                                            }

                                            //serviceUpdateSuccess = true;
                                            if (serviceUpdateSuccess)
                                            {
                                                var transaction = new CancellationTransaction()
                                                {
                                                    Amount = 0,
                                                    Currency = CurrencyCode,
                                                    OfferingId = GlobalConfig.offeringId,
                                                    CancellationRemarks = cancellation_remarks,
                                                    OriginalTransactionId = -1,
                                                    GomsTransactionId = -1000,
                                                    Date = registDt,
                                                    Reference = reference,
                                                    StatusId = GlobalConfig.Visible
                                                };
                                                user.Transactions.Add(transaction);
                                            }
                                        }

                                        if (context.SaveChanges() > 0)
                                        {
                                            ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                            ReturnCode.StatusMessage = "We have disabled all your automatic payment renewal.";
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.NotAuthenticated;
                    ReturnCode.StatusMessage = "User is not authenticated";
                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }


        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult _RegisterAndSubscribe(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "Registration",
                TransactionType = "Registration"
            };

            if (!Request.IsAjaxRequest())
            {
                ReturnCode.StatusMessage = "Invalid request";
                return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
            }

            bool isSourceAir = false;
            string url = Url.Action("Register", "User").ToString();
            var field_names = new string[] { "uid", "provider", "full_name", "pid", "cmd", "a1", "p1", "t1", "a3", "t3", "p3", "src", "item_name", "amount", "currency", "custom", "ip" };
            try
            {
                if (TempData["qs"] != null)
                {
                    var qs = (NameValueCollection)TempData["qs"];
                    ViewBag.qs = qs;
                    TempData["qs"] = qs;
                }

                DateTime registDt = DateTime.Now;
                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;

                foreach (var x in tmpCollection)
                {
                    if (!field_names.Contains(x.Key))
                        if (String.IsNullOrEmpty(x.Value))
                        {
                            isMissingRequiredFields = true;
                            break;
                        }
                }

                if (!isMissingRequiredFields) // process form
                {
                    var ip = Request.GetUserHostAddressFromCloudflare();
                    if (!String.IsNullOrEmpty(tmpCollection["ip"]))
                        ip = tmpCollection["ip"];

                    var location = MyUtility.GetLocationBasedOnIpAddress(ip);
                    string FirstName = tmpCollection["first_name"];
                    string LastName = tmpCollection["last_name"];

                    string EMail = tmpCollection["p_login_email"];
                    string ConfirmEmail = tmpCollection["p_login_email_c"];
                    string Password = tmpCollection["login_pass"];

                    //autodetect country, city, state
                    string CountryCode = location.countryCode;
                    string City = location.city;
                    string State = location.region;

                    string provider = String.IsNullOrEmpty(tmpCollection["provider"]) ? String.Empty : tmpCollection["provider"];
                    string uid = String.IsNullOrEmpty(tmpCollection["uid"]) ? String.Empty : tmpCollection["uid"];
                    System.Guid userId = System.Guid.NewGuid();
                    string browser = Request.UserAgent;

                    if (FirstName.Length > 32)
                        ReturnCode.StatusMessage = "First Name cannot exceed 32 characters.";
                    if (LastName.Length > 32)
                        ReturnCode.StatusMessage = "Last Name cannot exceed 32 characters.";
                    if (EMail.Length > 64)
                        ReturnCode.StatusMessage = "Email address cannot exceed 64 characters.";
                    if (State.Length > 30)
                        ReturnCode.StatusMessage = "State cannot exceed 30 characters.";
                    if (City.Length > 50)
                        ReturnCode.StatusMessage = "City cannot exceed 50 characters.";
                    if (String.Compare(EMail, ConfirmEmail, true) != 0)
                        ReturnCode.StatusMessage = "Email addresses do not match";

                    RegexUtilities util = new RegexUtilities();
                    //if (!MyUtility.isEmail(EMail))
                    if (!util.IsValidEmail(EMail))
                        ReturnCode.StatusMessage = "Email address is invalid.";

                    var context = new IPTV2Entities();
                    User user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EMail, true) == 0);
                    if (user != null)
                        ReturnCode.StatusMessage = "Email address is already taken.";

                    if (GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',').Contains(CountryCode)) // check if country is part of the exclusion list first
                        ReturnCode.StatusMessage = "Country does not exist.";
                    else if (context.Countries.Count(c => String.Compare(c.Code, CountryCode, true) == 0) <= 0) // then check if country is part of the list                    
                        ReturnCode.StatusMessage = "Country does not exist.";
                    if (context.States.Count(s => String.Compare(s.CountryCode, CountryCode, true) == 0) > 0)
                        if (context.States.Count(s => String.Compare(s.CountryCode, CountryCode, true) == 0 && (String.Compare(s.StateCode, State, true) == 0 || String.Compare(s.Name, State, true) == 0)) <= 0)
                            ReturnCode.StatusMessage = "State is invalid for this country.";

                    if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                        return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);

                    user = new User()
                    {
                        UserId = userId,
                        FirstName = FirstName,
                        LastName = LastName,
                        City = City,
                        State = State,
                        CountryCode = CountryCode,
                        EMail = EMail,
                        Password = MyUtility.GetSHA1(Password),
                        GigyaUID = userId.ToString(),
                        RegistrationDate = registDt,
                        LastUpdated = registDt,
                        RegistrationIp = ip,
                        StatusId = GlobalConfig.Visible,
                        ActivationKey = Guid.NewGuid(),
                        DateVerified = registDt
                    };

                    try
                    {
                        if (Request.Cookies.AllKeys.Contains("tuid"))
                            user.RegistrationCookie = Request.Cookies["tuid"].Value;
                        else if (Request.Cookies.AllKeys.Contains("regcook"))
                            user.RegistrationCookie = Request.Cookies["regcook"].Value;
                    }
                    catch (Exception) { }

                    ////check for cookie 
                    try
                    {
                        var dt = DateTime.Parse(Request.Cookies["rcDate"].Value);
                        if (registDt.Subtract(dt).Days < 45)
                        {
                            ReturnCode.StatusMessage = "We have detected that you have already registered using this machine.";
                            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception) { }

                    string CurrencyCode = GlobalConfig.DefaultCurrency;
                    var country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);
                    if (country != null)
                        CurrencyCode = country.CurrencyCode;
                    var wallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, CurrencyCode, true) == 0);
                    if (wallet == null) // Wallet does not exist. Create new wallet for User.
                    {
                        wallet = ContextHelper.CreateWallet(0, CurrencyCode, registDt);
                        user.UserWallets.Add(wallet);
                    }

                    var transaction = new RegistrationTransaction()
                    {
                        RegisteredState = user.State,
                        RegisteredCity = user.City,
                        RegisteredCountryCode = user.CountryCode,
                        Amount = 0,
                        Currency = CurrencyCode,
                        Reference = isSourceAir ? "New Registration (air)" : "New Registration",
                        Date = registDt,
                        OfferingId = GlobalConfig.offeringId,
                        UserId = user.UserId,
                        StatusId = GlobalConfig.Visible
                    };
                    user.Transactions.Add(transaction);

                    context.Users.Add(user);
                    if (context.SaveChanges() > 0)
                    {
                        string verification_email = String.Format("{0}/User/Verify?key={1}", GlobalConfig.baseUrl, user.ActivationKey.ToString());
                        if (isSourceAir)
                        {
                            try
                            {
                                verification_email = String.Format("{0}&source=air", verification_email);
                                var template = MyUtility.GetUrlContent(GlobalConfig.ProjectAirEmailVerificationBodyTemplateUrl);
                                var htmlBody = String.Format(template, FirstName, EMail, verification_email);
                                if (!Request.IsLocal)
                                    try { MyUtility.SendEmailViaSendGrid(EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", htmlBody, MailType.HtmlOnly, String.Empty); }
                                    catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }
                            }
                            catch (Exception)
                            {
                                string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, FirstName, EMail, verification_email);
                                if (!Request.IsLocal)
                                    try { MyUtility.SendEmailViaSendGrid(EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody, MailType.TextOnly, emailBody); }
                                    catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }
                            }
                        }
                        else
                        {
                            string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, FirstName, EMail, verification_email);
                            if (!Request.IsLocal)
                                try { MyUtility.SendEmailViaSendGrid(EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody, MailType.TextOnly, emailBody); }
                                catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }
                        }
                        GSResponse res = null;
                        if (!String.IsNullOrEmpty(uid) && !String.IsNullOrEmpty(provider))
                        {
                            Dictionary<string, object> collection = new Dictionary<string, object>();
                            collection.Add("siteUID", user.UserId);
                            collection.Add("uid", Uri.UnescapeDataString(uid));
                            collection.Add("cid", String.Format("{0} - New User", provider));
                            res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(collection));
                            if (res.GetErrorCode() == 0) //Successful link
                            {
                                if (user != null)
                                {
                                    var UserId = user.UserId.ToString();
                                    user.StatusId = GlobalConfig.Visible; //activate account
                                    user.DateVerified = DateTime.Now;
                                    SetAutheticationCookie(UserId);
                                    SetSession(UserId);
                                    if (!ContextHelper.SaveSessionInDatabase(context, user))
                                        context.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            var info = new GigyaUserInfo()
                            {
                                firstName = FirstName,
                                lastName = LastName,
                                email = EMail
                            };
                            var registrationInfo = new GigyaNotifyLoginInfo()
                            {
                                siteUID = user.UserId.ToString(),
                                cid = "TFCTV - Registration",
                                sessionExpiration = 0,
                                newUser = true,
                                userInfo = Newtonsoft.Json.JsonConvert.SerializeObject(info)
                            };
                            GSObject obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(registrationInfo));
                            res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", obj);

                        }

                        if (user != null)
                        {
                            if (user.StatusId == GlobalConfig.Visible)
                            {
                                int freeTrialProductId = 0;
                                if (GlobalConfig.IsFreeTrialEnabled)
                                {
                                    freeTrialProductId = MyUtility.GetCorrespondingFreeTrialProductId();
                                    if (GlobalConfig.TfcTvFree2StartDate < registDt && GlobalConfig.TfcTvFree2EndDate > registDt)
                                    {
                                        string UserCountryCode = user.CountryCode;
                                        if (!GlobalConfig.isUAT)
                                            try { UserCountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy(); }
                                            catch (Exception) { }

                                        var countryList = GlobalConfig.TfcTvFree2CountryWhiteList.Split(',');
                                        if (countryList.Contains(UserCountryCode) && String.Compare(user.CountryCode, UserCountryCode, true) == 0)
                                            freeTrialProductId = GlobalConfig.TfcTvFree2ProductId;
                                    }
                                    PaymentHelper.PayViaWallet(context, userId, freeTrialProductId, SubscriptionProductType.Package, userId, null);
                                }

                                //authenticate user
                                SetAutheticationCookie(user.UserId.ToString());

                                SendToGigya(user);
                                SetSession(user.UserId.ToString());
                                ContextHelper.SaveSessionInDatabase(context, user);

                                //add uid cookie
                                HttpCookie uidCookie = new HttpCookie("uid");
                                uidCookie.Value = user.UserId.ToString();
                                uidCookie.Expires = DateTime.Now.AddDays(30);
                                Response.Cookies.Add(uidCookie);
                            }
                        }

                        GigyaHelpers.setCookie(res, this.ControllerContext);
                        GigyaUserData2 userData = new GigyaUserData2()
                        {
                            city = user.City,
                            country = user.CountryCode,
                            email = user.EMail,
                            firstName = user.FirstName,
                            lastName = user.LastName,
                            state = user.State
                        };

                        //GigyaUserDataInfo userDataInfo = new GigyaUserDataInfo()
                        //{
                        //    UID = user.UserId.ToString(),
                        //    data = Newtonsoft.Json.JsonConvert.SerializeObject(userData, Formatting.None)
                        //};

                        TFCTV.Helpers.UserData privacyData = new UserData() { IsExternalSharingEnabled = "true,false", IsInternalSharingEnabled = "true,false", IsProfilePrivate = "false" };

                        GigyaUserDataInfo2 userDataInfo = new GigyaUserDataInfo2()
                        {
                            UID = user.UserId.ToString(),
                            profile = Newtonsoft.Json.JsonConvert.SerializeObject(userData, Formatting.None),
                            data = Newtonsoft.Json.JsonConvert.SerializeObject(privacyData, Formatting.None)
                        };

                        GSObject userDataInfoObj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(userDataInfo));
                        //res = GigyaHelpers.createAndSendRequest("gcs.setUserData", userDataInfoObj);
                        res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", userDataInfoObj);
                        var returnCode = res.GetErrorCode();

                        //Publish to Activity Feed
                        List<ActionLink> actionlinks = new List<ActionLink>();
                        actionlinks.Add(new ActionLink() { text = SNSTemplates.register_actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_actionlink_href) });
                        //mediaItem
                        List<MediaItem> mediaItems = new List<MediaItem>();
                        mediaItems.Add(new MediaItem() { type = SNSTemplates.register_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.register_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_mediaitem_href) });
                        UserAction action = new UserAction()
                        {
                            actorUID = userId.ToString(),
                            userMessage = SNSTemplates.register_usermessage,
                            title = SNSTemplates.register_title,
                            subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_subtitle),
                            linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_linkback),
                            description = String.Format(SNSTemplates.register_description, FirstName),
                            actionLinks = actionlinks,
                            mediaItems = mediaItems
                        };

                        GigyaMethods.PublishUserAction(action, userId, "external");
                        action.userMessage = String.Empty;
                        action.title = String.Empty;
                        action.mediaItems = null;
                        GigyaMethods.PublishUserAction(action, userId, "internal");

                        TempData["qs"] = null; // empty the TempData upon successful registration

                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                        ReturnCode.info7 = user.EMail;
                        if (user.StatusId == GlobalConfig.Visible)
                        {
                            ReturnCode.StatusHeader = "Your 7-Day Free Trial Starts Now!";
                            ReturnCode.StatusMessage = "Congratulations! You are now registered to TFC.tv.";
                            ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies!";
                            ReturnCode.info3 = user.UserId.ToString();

                            //Change to social registration
                            ReturnCode.info = "SocialRegistration";
                            ReturnCode.TransactionType = "SocialRegistration";
                        }
                        else
                        {
                            ReturnCode.StatusHeader = "Email verification sent!";
                            ReturnCode.StatusMessage = "Congratulations! You are one step away from completing your registration.";
                            ReturnCode.StatusMessage2 = "An email has been sent to you.<br> Verify your email address to complete your registration.";
                        }
                        TempData["ErrorMessage"] = ReturnCode;
                        //if(xoom)
                        if (Request.Cookies.AllKeys.Contains("xoom"))
                        {
                            var userPromo = new UserPromo();
                            userPromo.UserId = user.UserId;
                            userPromo.PromoId = GlobalConfig.Xoom2PromoId;
                            userPromo.AuditTrail.CreatedOn = registDt;
                            context.UserPromos.Add(userPromo);
                            context.SaveChanges();
                        }

                        return this.Json(ReturnCode, JsonRequestBehavior.AllowGet); // successful registration
                    }
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                url = String.Format("{0}?{1}", Request.UrlReferrer.AbsolutePath, MyUtility.DictionaryToQueryString(tmpCollection));
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SeshCh()
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var context = new IPTV2Entities();
                    var UserId = new Guid(User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                    if (user != null)
                    {
                        if (MyUtility.IsDuplicateSession(user, Request.Cookies[System.Web.Security.FormsAuthentication.FormsCookieName]))
                        {
                            System.Web.Security.FormsAuthentication.SignOut();
                            ReturnCode.StatusCode = (int)ErrorCodes.MultipleLoginDetected;
                        }
                    }

                }
            }
            catch (Exception) { }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }
    }
}
