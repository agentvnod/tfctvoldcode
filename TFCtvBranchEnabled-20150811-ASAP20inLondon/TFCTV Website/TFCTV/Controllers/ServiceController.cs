using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Helpers;
using IPTV2_Model;
using System.Web.Security;
using Gigya.Socialize.SDK;
using System.Collections.Specialized;
using Newtonsoft.Json;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class ServiceController : Controller
    {
        //
        // GET: /Service/

        //[RequireHttps]
        public JsonResult GetProgramSchedule(int id = 0)
        {
            List<ProgramSchedDisplay> obj = null;
            try
            {
                var context = new IPTV2Entities();
                DateTime utc = DateTime.Now.ToUniversalTime();

                DateTime gmt = utc.AddHours(8); //convert to GMT
                var dow = (int)gmt.DayOfWeek;

                var channels = GlobalConfig.ProjectAirProgramScheduleChannelIds.Split(','); //Channel Id (Sunday-Saturday)
                //var psChannelId = Convert.ToInt32(channels[dow]); //ProgramSchedule Channel Id
                //var psChannelIdTomorrow = Convert.ToInt32(channels[dowTomorrow]); //ProgramSchedule Channel Id for the next day
                var psChannelId = MyUtility.StringToIntList(String.Format("{0}", channels[dow]));

                var currentHour = utc.Hour;
                DateTime gmtTomorrow = gmt.AddDays(1);
                var dowTomorrow = (int)gmtTomorrow.DayOfWeek;
                var tomorrowCheck = false;
                if (currentHour >= 14) //Check if it's on or after 10pm
                {
                    tomorrowCheck = true;
                    psChannelId = MyUtility.StringToIntList(String.Format("{0},{1}", channels[dow], channels[dowTomorrow]));
                }

                var sked = context.ProgramSchedules.Where(p => psChannelId.Contains(p.ChannelId));
                if (sked != null)
                {
                    if (sked.Count() > 0)
                    {
                        sked = sked.OrderBy(p => p.ChannelId).ThenBy(p => p.StartTime);
                        List<ProgramSchedDisplay> list = new List<ProgramSchedDisplay>();
                        var slist = sked.ToList();
                        foreach (var item in slist)
                        {
                            try
                            {
                                var startDt = Convert.ToDateTime(item.StartTime.Trim());
                                var endDt = Convert.ToDateTime(item.EndTime.Trim());
                                if (tomorrowCheck)
                                {
                                    if (item.ChannelId == Convert.ToInt32(channels[dowTomorrow]))
                                    {
                                        startDt = startDt.AddDays(1);
                                        endDt = endDt.AddDays(1);
                                    }
                                }
                                var psd = new ProgramSchedDisplay()
                                {
                                    Pid = item.ProgramScheduleId,
                                    CategoryName = item.ShowName.TrimEnd(),
                                    CategoryId = item.CategoryId == null ? 0 : (int)item.CategoryId,
                                    Description = item.Blurb,
                                    StartDate = startDt,
                                    EndDate = endDt,
                                    StartDateStr = startDt.ToString("MM/dd/yyyy HH:mm:ss"),
                                    EndDateStr = endDt.ToString("MM/dd/yyyy HH:mm:ss"),
                                    StartTime = item.StartTime.Trim(),
                                    EndTime = item.EndTime.Trim()
                                };
                                list.Add(psd);
                            }
                            catch (Exception) { throw; }
                        }

                        if (id == 1)
                            obj = list.Where(l => l.EndDate > gmt).ToList();
                        else
                            obj = list.ToList();
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); throw; }
            return this.Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        //[RequireHttps]
        public ActionResult _MobileLogin(FormCollection fc)
        {
            var ReturnCode = new AirLoginReturnObj()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                IsSuccess = false,
                sessionSecret = String.Empty,
                sessionToken = String.Empty,
                firstName = String.Empty,
                lastName = String.Empty
            };
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

                    RegexUtilities util = new RegexUtilities();

                    //if (!MyUtility.isEmail(EmailAddress))
                    if (!util.IsValidEmail(EmailAddress))
                    {
                        ReturnCode.StatusMessage = "Email address is invalid.";
                    }
                    else
                    {
                        using (var context = new IPTV2Entities())
                        {
                            var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
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
                                        try
                                        {

                                            Dictionary<string, object> userInfo = new Dictionary<string, object>();
                                            userInfo.Add("firstName", user.FirstName);
                                            userInfo.Add("lastName", user.LastName);
                                            userInfo.Add("email", user.EMail);
                                            Dictionary<string, object> collection = new Dictionary<string, object>();
                                            collection.Add("siteUID", user.UserId);
                                            collection.Add("cid", "TFCTV - Login (Mobile)");
                                            //collection.Add("sessionExpiration", 2592000);
                                            collection.Add("sessionExpiration", 432000);
                                            collection.Add("targetEnv", "mobile");
                                            collection.Add("userInfo", MyUtility.buildJson(userInfo));
                                            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(collection));
                                            if (res.GetErrorCode() == 0)
                                            {
                                                ReturnCode.sessionToken = res.GetString("sessionToken", String.Empty);
                                                ReturnCode.sessionSecret = res.GetString("sessionSecret", String.Empty);
                                                ReturnCode.StatusMessage = "OK";
                                                ReturnCode.IsSuccess = true;
                                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                ReturnCode.firstName = user.FirstName;
                                                ReturnCode.lastName = user.LastName;
                                            }
                                        }
                                        catch (Exception) { }
                                        ContextHelper.SaveSessionInDatabase(context, user);
                                    }
                                    else
                                        ReturnCode.StatusMessage = "Email/Password do not match.";
                                }
                            }
                        }
                    }
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        //[RequireHttps]
        public ActionResult _MobileRegister(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "Registration",
                TransactionType = "Registration"
            };

            string url = Url.Action("Register", "User").ToString();
            var field_names = new string[] { "uid", "provider", "full_name" };

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

                if (Request.IsLocal)
                    tmpCollection = Request.QueryString.AllKeys.ToDictionary(k => k, v => Request.QueryString[v]);

                if (!isMissingRequiredFields) // process form
                {
                    string FirstName = HttpUtility.UrlDecode(tmpCollection["first_name"]);
                    string LastName = HttpUtility.UrlDecode(tmpCollection["last_name"]);

                    string ip = HttpUtility.UrlDecode(tmpCollection["ip"]);
                    string EMail = HttpUtility.UrlDecode(tmpCollection["login_email"]);
                    string Password = HttpUtility.UrlDecode(tmpCollection["login_pass"]);
                    string ConfirmPassword = HttpUtility.UrlDecode(tmpCollection["login_confirm_pass"]);
                    string CountryCode = String.Empty;
                    string City = String.Empty;
                    string State = String.Empty;

                    try
                    {
                        if (!String.IsNullOrEmpty(ip))
                        {
                            var location = MyUtility.GetLocationBasedOnIpAddress(ip);
                            CountryCode = location.countryCode;
                            City = location.city;
                            State = (String.Compare(CountryCode, GlobalConfig.DefaultCountry, true) == 0) ? location.region : location.regionName;
                        }
                    }
                    catch (Exception) { }

                    string provider = String.Empty;
                    string uid = String.Empty;
                    try
                    {
                        provider = String.IsNullOrEmpty(tmpCollection["provider"]) ? String.Empty : tmpCollection["provider"];
                        uid = String.IsNullOrEmpty(tmpCollection["uid"]) ? String.Empty : tmpCollection["uid"];
                    }
                    catch (Exception) { }

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

                    RegexUtilities util = new RegexUtilities();

                    //if (!MyUtility.isEmail(EMail))
                    if (!util.IsValidEmail(EMail))
                        ReturnCode.StatusMessage = "Email address is invalid.";

                    if (String.Compare(Password, ConfirmPassword, false) != 0)
                        ReturnCode.StatusMessage = "Passwords do not match.";

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
                        RegistrationIp = String.IsNullOrEmpty(ip) ? MyUtility.GetClientIpAddress() : ip,
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
                        Reference = "New Registration (mobile air)",
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
                        string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, FirstName, EMail, verification_email);
                        if (!Request.IsLocal)
                            try { MyUtility.SendEmailViaSendGrid(EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody, MailType.TextOnly, emailBody); }
                            catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }

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
                                    SetAuthenticationCookie(UserId);
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

                        TFCTV.Helpers.UserData privacyData = new UserData() { IsExternalSharingEnabled = "true,false", IsInternalSharingEnabled = "true,false", IsProfilePrivate = "false" };

                        //GigyaUserDataInfo userDataInfo = new GigyaUserDataInfo()
                        //{
                        //    UID = user.UserId.ToString(),
                        //    data = Newtonsoft.Json.JsonConvert.SerializeObject(userData, Newtonsoft.Json.Formatting.None)
                        //};

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

                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                        ReturnCode.info7 = user.EMail;
                        if (user.StatusId == GlobalConfig.Visible)
                        {
                            ReturnCode.StatusHeader = "Your 7-Day Free Trial Starts Now!";
                            ReturnCode.StatusMessage = "Congratulations! You are now registered to TFC.tv.";
                            ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies!";

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
                        return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";
            }
            catch (Exception e) { MyUtility.LogException(e); ReturnCode.StatusMessage = e.Message; }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        //[RequireHttps]
        [HttpPost]
        public ActionResult _MobileSocialize(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
                        {
                            StatusCode = (int)ErrorCodes.UnknownError,
                            StatusMessage = String.Empty,
                            info = "Registration",
                            TransactionType = "Registration"
                        };
            var field_names = new string[] { "uid", "provider", "full_name" };
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

                if (!isMissingRequiredFields) // process form
                {
                    var qs = fc;

                    string gigyaUID = Uri.UnescapeDataString(qs["UID"]);
                    bool isRequestValid = SigUtils.ValidateUserSignature(gigyaUID, Uri.UnescapeDataString(qs["timestamp"]), GlobalConfig.GSsecretkey, Uri.UnescapeDataString(qs["signature"]));
                    isRequestValid = true;
                    if (isRequestValid)
                    {
                        string FirstName = qs["first_name"];
                        string LastName = qs["last_name"];
                        //string ip = qs["ip"];
                        string ip = Request.GetUserHostAddressFromCloudflare();
                        var location = MyUtility.GetLocationBasedOnIpAddress(ip);
                        string City = location.city;
                        string CountryCode = location.countryCode;
                        string State = (String.Compare(CountryCode, GlobalConfig.DefaultCountry, true) == 0) ? location.region : location.regionName;
                        string EMail = qs["email"];
                        string Password = Membership.GeneratePassword(10, 3);
                        string provider = qs["provider"];
                        GSResponse res = null;
                        using (var context = new IPTV2Entities())
                        {
                            User user = null;
                            bool isSiteUID = Convert.ToBoolean(qs["isSiteUID"]);
                            bool IsUserCreateSuccessful = false;
                            if (isSiteUID) //Old user. Signin to site
                            {
                                var UserId = new Guid(gigyaUID);
                                user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                if (user != null)
                                {
                                    if (user.StatusId == GlobalConfig.Visible) //User found login user
                                    {
                                        SetAuthenticationCookie(gigyaUID);
                                        SetSession(gigyaUID);
                                        ContextHelper.SaveSessionInDatabase(context, user);
                                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                        ReturnCode.StatusMessage = "Successful login.";
                                    }
                                    else //user is not active in the database, put code here                                
                                        ReturnCode.StatusMessage = "Social networking account is not verified in our system.";
                                }
                                else //user does not exist on database, put code here
                                {
                                    //Social networking account does not exist in our system;
                                    //Create user                                    
                                    user = new User()
                                    {
                                        UserId = UserId,
                                        FirstName = FirstName,
                                        LastName = LastName,
                                        City = City,
                                        State = State,
                                        CountryCode = CountryCode,
                                        EMail = EMail,
                                        Password = MyUtility.GetSHA1(Password),
                                        GigyaUID = gigyaUID,
                                        RegistrationDate = registDt,
                                        LastUpdated = registDt,
                                        RegistrationIp = MyUtility.GetClientIpAddress(),
                                        StatusId = GlobalConfig.Visible,
                                        ActivationKey = Guid.NewGuid(),
                                        DateVerified = registDt
                                    };
                                    context.Users.Add(user);
                                    SetAuthenticationCookie(UserId.ToString());
                                    SetSession(UserId.ToString());
                                    if (!ContextHelper.SaveSessionInDatabase(context, user))
                                    {
                                        if (context.SaveChanges() > 0)
                                        {
                                            IsUserCreateSuccessful = true;
                                            ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                            ReturnCode.StatusMessage = "Enabled account on database.";
                                        }
                                    }
                                }

                            }
                            else //New user. allow user to register
                            {
                                bool createUser = true;
                                if (!String.IsNullOrEmpty(qs["email"]))
                                {
                                    string email = qs["email"];
                                    user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, email, true) == 0);
                                    if (user != null) // link account
                                    {
                                        Dictionary<string, object> collection = new Dictionary<string, object>();
                                        collection.Add("siteUID", user.UserId);
                                        collection.Add("uid", Uri.UnescapeDataString(qs["UID"]));
                                        collection.Add("cid", String.Format("{0} - New User", qs["provider"]));

                                        GSObject obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(collection));
                                        res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", obj);
                                        if (res.GetErrorCode() == 0) //Successful link
                                        {
                                            var UserId = user.UserId.ToString();
                                            user.StatusId = GlobalConfig.Visible; //activate account
                                            user.DateVerified = DateTime.Now;
                                            SetAuthenticationCookie(UserId);
                                            SetSession(UserId);
                                            if (!ContextHelper.SaveSessionInDatabase(context, user))
                                                context.SaveChanges();
                                        }
                                    }
                                }

                                if (createUser) //user not found on database. create new user
                                {
                                    System.Guid UserId = System.Guid.NewGuid();
                                    user = new User()
                                    {
                                        UserId = UserId,
                                        FirstName = FirstName,
                                        LastName = LastName,
                                        City = City,
                                        State = State,
                                        CountryCode = CountryCode,
                                        EMail = EMail,
                                        Password = MyUtility.GetSHA1(Password),
                                        GigyaUID = UserId.ToString(),
                                        RegistrationDate = registDt,
                                        LastUpdated = registDt,
                                        RegistrationIp = MyUtility.GetClientIpAddress(),
                                        StatusId = GlobalConfig.Visible,
                                        ActivationKey = Guid.NewGuid(),
                                        DateVerified = registDt
                                    };
                                    context.Users.Add(user);
                                    Dictionary<string, object> collection = new Dictionary<string, object>();
                                    collection.Add("siteUID", user.UserId);
                                    collection.Add("uid", Uri.UnescapeDataString(qs["UID"]));
                                    collection.Add("cid", String.Format("{0} - New User", qs["provider"]));

                                    GSObject obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(collection));
                                    res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", obj);
                                    if (res.GetErrorCode() == 0) //Successful link
                                    {
                                        SetAuthenticationCookie(UserId.ToString());
                                        SetSession(UserId.ToString());
                                        if (!ContextHelper.SaveSessionInDatabase(context, user))
                                        {
                                            if (context.SaveChanges() > 0)
                                            {
                                                IsUserCreateSuccessful = true;
                                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                ReturnCode.StatusMessage = "User registration successful.";
                                            }
                                        }
                                    }
                                }

                                if (IsUserCreateSuccessful)
                                {
                                    if (user != null)
                                    {
                                        if (user.StatusId == GlobalConfig.Visible)
                                        {
                                            int freeTrialProductId = 0;
                                            if (GlobalConfig.IsFreeTrialEnabled)
                                            {
                                                freeTrialProductId = MyUtility.GetCorrespondingFreeTrialProductId();
                                                PaymentHelper.PayViaWallet(context, user.UserId, freeTrialProductId, SubscriptionProductType.Package, user.UserId, null);
                                            }
                                        }
                                        PublishGigyaActions(user, res);
                                        string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, FirstName, EMail, String.Format("Your new TFC.tv password is {0}", Password));
                                        if (!Request.IsLocal)
                                            try { MyUtility.SendEmailViaSendGrid(EMail, GlobalConfig.NoReplyEmail, "Your new TFC.tv password", emailBody, MailType.TextOnly, emailBody); }
                                            catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }
                                    }
                                }
                            }
                        }
                    }
                    else
                        ReturnCode.StatusMessage = "Request is not valid.";
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ServerTime()
        {
            var obj = new ServerTimeObj()
            {
                Now = String.Format("{0}", DateTime.Now),
                UniversalTime = String.Format("{0}", DateTime.Now.ToUniversalTime()),
                NowPlus8 = String.Format("{0}", DateTime.Now.AddHours(8)),
                UniversalTimePlus8 = String.Format("{0}", DateTime.Now.ToUniversalTime().AddHours(8))
            };
            return this.Json(obj, JsonRequestBehavior.AllowGet);
        }

        public virtual void SetAuthenticationCookie(string uid)
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

        private void PublishGigyaActions(User user, GSResponse res)
        {
            GigyaHelpers.setCookie(res, this.ControllerContext);
            GigyaUserData userData = new GigyaUserData()
            {
                City = user.City,
                CountryCode = user.CountryCode,
                Email = user.EMail,
                FirstName = user.FirstName,
                LastName = user.LastName,
                State = user.State
            };
            GigyaUserDataInfo userDataInfo = new GigyaUserDataInfo()
            {
                UID = user.UserId.ToString(),
                data = Newtonsoft.Json.JsonConvert.SerializeObject(userData, Newtonsoft.Json.Formatting.None)
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
                actorUID = user.UserId.ToString(),
                userMessage = SNSTemplates.register_usermessage,
                title = SNSTemplates.register_title,
                subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_subtitle),
                linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_linkback),
                description = String.Format(SNSTemplates.register_description, user.FirstName),
                actionLinks = actionlinks,
                mediaItems = mediaItems
            };

            GigyaMethods.PublishUserAction(action, user.UserId, "external");
            action.userMessage = String.Empty;
            action.title = String.Empty;
            action.mediaItems = null;
            GigyaMethods.PublishUserAction(action, user.UserId, "internal");

        }

        //[RequireHttps]
        public JsonResult Feed01()
        {
            var ReturnCode = new ServiceReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            //if (!Request.IsLocal)
            //    if (!GlobalConfig.isUAT)
            //        if (!Request.IsAjaxRequest())
            //        {
            //            ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
            //            ReturnCode.StatusMessage = "Request is not valid.";
            //            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
            //        }

            int id = GlobalConfig.TFCtvMobileAirEpisodeId;
            DateTime registDt = DateTime.Now;
            try
            {
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();

                if (MyUtility.IsWhiteListed(String.Empty))
                    CountryCode = "HK";
                else
                {
                    try
                    {
                        var requestingIp = Request.GetUserHostAddressFromCloudflare();
                        var whiteListedIp = GlobalConfig.IpWhiteList.Split(',');
                        if (whiteListedIp.Contains(requestingIp) && GlobalConfig.isUAT)
                            CountryCode = "HK";
                    }
                    catch (Exception) { }
                }

                using (var context = new IPTV2Entities())
                {
                    Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                    if (episode != null)
                    {
                        if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                        {
                            var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                            var parentCategories = episode.GetParentShows(CacheDuration);
                            Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                            //var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true, offering);
                            var HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                            if (parentCategories != null)
                                HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
                            if (HasActiveSubscriptionBasedOnCategoryId.HasSubscription)
                            {
                                var premiumAsset = episode.PremiumAssets.LastOrDefault();
                                if (premiumAsset != null)
                                {
                                    Asset asset = premiumAsset.Asset;
                                    if (asset != null)
                                    {
                                        int assetId = asset == null ? 0 : asset.AssetId;
                                        if (episode.IsLiveChannelActive == true) //isLiveEvent
                                        {
                                            if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(premiumAsset))
                                            {
                                                if (Request.Browser.IsMobileDevice)
                                                {
                                                    ReturnCode.StatusCode = (int)ErrorCodes.IsNotAvailableOnMobileDevices;
                                                    ReturnCode.StatusMessage = "This stream is not available on this device";
                                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, true, CountryCodeOverride: CountryCode, RemoveIpFromToken: true);
                                            if (clipDetails != null)
                                                if (!String.IsNullOrEmpty(clipDetails.Url))
                                                {
                                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                    ReturnCode.StatusMessage = "OK";
                                                    ReturnCode.info = clipDetails.Url;
                                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                                }
                                        }
                                        ReturnCode.StatusMessage = "Clip not available.";
                                        return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                    }
                                    ReturnCode.StatusMessage = "Asset not available.";
                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                }
                            }
                            ReturnCode.StatusMessage = "Access to this clip is restricted.";
                            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                        }
                    }
                    ReturnCode.StatusMessage = "Episode does not exist.";
                }
            }
            catch (Exception e) { ReturnCode.StatusMessage = e.Message; }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        //[RequireHttps]
        public JsonResult _MobileForgotPassword(FormCollection fc)
        {
            var ReturnCode = new ServiceReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

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

                    RegexUtilities util = new RegexUtilities();
                    //if (!MyUtility.isEmail(EmailAddress))
                    if (!util.IsValidEmail(EmailAddress))
                    {
                        ReturnCode.StatusMessage = "Email address is invalid.";
                    }
                    else
                    {
                        var context = new IPTV2Entities();
                        User user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                        if (user != null)
                        {
                            if (user.StatusId != GlobalConfig.Visible)
                            {
                                ReturnCode.StatusMessage = "Email address is not verified.";
                                return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                            user.LastUpdated = registDt;
                            if (context.SaveChanges() > 0)
                            {
                                string oid = MyUtility.GetSHA1(String.Format("{0}{1}", user.UserId, user.LastUpdated));
                                string reset_pwd_email = String.Format("{0}/User/ResetPassword?key={1}&oid={2}", GlobalConfig.baseUrl, user.ActivationKey, oid.ToLower());
                                string emailBody = String.Format(GlobalConfig.ResetPasswordBodyTextOnly, user.FirstName, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), reset_pwd_email);
                                try
                                {
                                    if (!Request.IsLocal)
                                        MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, "Reset your TFC.tv Password", emailBody, MailType.TextOnly, emailBody);

                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                    ReturnCode.StatusMessage = "Instructions on how to reset your password have been sent to your email address.";
                                }
                                catch (Exception)
                                {
                                    ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                                    ReturnCode.StatusMessage = "The system encountered an unspecified error. Please contact Customer Support.";
                                }
                            }
                            else
                                ReturnCode.StatusMessage = "The system encountered an unidentified error. Please try again.";
                        }
                        else
                        {
                            ReturnCode.StatusCode = (int)ErrorCodes.UserDoesNotExist;
                            ReturnCode.StatusMessage = "Email does not exist.";
                        }
                    }
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";
            }
            catch (Exception e) { ReturnCode.StatusMessage = e.Message; }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EnableFeed()
        {
            var ReturnCode = new ServiceReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            try
            {
                if (GlobalConfig.isUAT)
                {
                    var context = new IPTV2Entities();
                    var restrictions = context.CategoryCountryRestrictions.Where(c => String.Compare(c.CountryCode, "US", true) == 0 && c.CategoryId == 2554);
                    if (restrictions != null)
                    {
                        foreach (var item in restrictions)
                        {
                            item.StatusId = 1;
                        }

                        if (context.SaveChanges() > 0)
                        {
                            ReturnCode.StatusCode = (int)ErrorCodes.Success;
                            ReturnCode.StatusMessage = "FEED ENABLED FOR US";
                        }
                        else
                            ReturnCode.StatusMessage = "UNABLE TO SAVE";
                    }
                    else
                        ReturnCode.StatusMessage = "NO RESTRICTIONS FOUND";
                }
                else
                    ReturnCode.StatusMessage = "FUNCTION NOT ALLOWED";
            }
            catch (Exception) { }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DisableFeed()
        {
            var ReturnCode = new ServiceReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            try
            {
                if (GlobalConfig.isUAT)
                {
                    var context = new IPTV2Entities();
                    var restrictions = context.CategoryCountryRestrictions.Where(c => String.Compare(c.CountryCode, "US", true) == 0 && c.CategoryId == 2554);
                    if (restrictions != null)
                    {
                        foreach (var item in restrictions)
                        {
                            item.StatusId = 5;
                        }

                        if (context.SaveChanges() > 0)
                        {
                            ReturnCode.StatusCode = (int)ErrorCodes.Success;
                            ReturnCode.StatusMessage = "FEED DISABLED FOR US";
                        }
                        else
                            ReturnCode.StatusMessage = "UNABLE TO SAVE";
                    }
                    else
                        ReturnCode.StatusMessage = "NO RESTRICTIONS FOUND";
                }
                else
                    ReturnCode.StatusMessage = "FUNCTION NOT ALLOWED";
            }
            catch (Exception) { }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireHttpsOnProductionOnly]
        public JsonResult VerifyPrepaidCard(FormCollection fc)
        {
            var ReturnCode = new ReturnCodeObj()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

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
                    string serial = fc["serial"];
                    string pin = fc["pin"];
                    serial = serial.Replace(" ", String.Empty);
                    pin = pin.Replace(" ", String.Empty);
                    if (!Request.IsLocal)
                        pin = MyUtility.GetSHA1(pin); // encrypt

                    if (User.Identity.IsAuthenticated)
                    {
                        Ppc.ErrorCode code;
                        var context = new IPTV2Entities();
                        System.Guid userId = new System.Guid(User.Identity.Name);
                        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                            if (user.HasPendingGomsChangeCountryTransaction(offering))
                                ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                            else
                            {
                                var ppc = context.Ppcs.FirstOrDefault(p => String.Compare(p.SerialNumber, serial, true) == 0);
                                if (ppc != null)
                                {
                                    if (!(String.Compare(ppc.SerialNumber, serial, true) == 0 && String.Compare(ppc.Pin, pin, false) == 0))
                                        ReturnCode.StatusMessage = "Invalid serial/pin combination.";
                                    else if (ppc.ExpirationDate < registDt)
                                        ReturnCode.StatusMessage = "Prepaid Card/ePIN is already expired.";
                                    else if (ppc.UserId != null)
                                        ReturnCode.StatusMessage = "Prepaid Card/ePIN is already used.";
                                    else
                                    {
                                        if (ppc is ReloadPpc)
                                        {
                                            if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                            else // process reloadPpc                                            
                                            {
                                                code = ReloadHelper.ReloadViaPrepaidCard(context, userId, serial, pin);
                                                if (code == Ppc.ErrorCode.Success)
                                                {
                                                    var wallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, user.Country.CurrencyCode, true) == 0);
                                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                    ReturnCode.StatusMessage = String.Format("You have successfully topped up your wallet. Your wallet balance is {0} {1}.", wallet.Currency, wallet.Balance);
                                                }
                                                else
                                                    ReturnCode.StatusMessage = MyUtility.GetPpcErrorMessages(code);
                                            }

                                        }
                                        else if (ppc is SubscriptionPpc)
                                        {
                                            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                            else
                                            {
                                                SubscriptionPpc sPpc = (SubscriptionPpc)ppc;
                                                Product product = context.Products.FirstOrDefault(p => p.ProductId == sPpc.ProductId);
                                                if (product == null)
                                                    ReturnCode.StatusMessage = "This product does not exist.";
                                                else
                                                {
                                                    SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                                                    TFCTV.Models.ErrorResponse response = PaymentHelper.PayViaPrepaidCard(context, userId, sPpc.ProductId, subscriptionType, serial, pin, userId, null);
                                                    code = (Ppc.ErrorCode)response.Code;
                                                    if (code == Ppc.ErrorCode.Success)
                                                    {
                                                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                        ReturnCode.StatusMessage = String.Format("You have successfully subscribed to {0}.", product.Description);
                                                    }
                                                    else
                                                        ReturnCode.StatusMessage = MyUtility.GetAirPlusPpcErrorMessages(code);
                                                }
                                            }
                                        }
                                        else
                                            ReturnCode.StatusMessage = "Unable to retrieve this type of Prepaid Card/ePIN.";
                                    }
                                }
                                else
                                    ReturnCode.StatusMessage = "Prepaid Card/ePIN does not exist.";
                            }
                        }
                        else
                            ReturnCode.StatusMessage = "User does not exist.";
                    }
                    else
                        ReturnCode.StatusMessage = "Your session has already expired. Please login again.";
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";
            }
            catch (Exception e) { MyUtility.LogException(e); ReturnCode.StatusMessage = String.Format("Exception: {0} | Inner Exception: {1}", e.Message, e.InnerException.Message); }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireHttpsOnProductionOnly]
        public JsonResult CheckSubscriptionStatus(FormCollection fc)
        {
            var ReturnCode = new AirPlusEntitlementObj()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                IsExpired = false
            };

            try
            {
                var registDt = DateTime.Now;
                if (User.Identity.IsAuthenticated)
                {
                    var context = new IPTV2Entities();
                    var userId = new System.Guid(User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                    {
                        var listofIncludedPackageIds = MyUtility.StringToIntList(GlobalConfig.ListOfAirPlusPackageIds);
                        var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == listofIncludedPackageIds.ElementAt(1));
                        if (entitlement != null)
                        {
                            var daysLeft = Convert.ToInt32(entitlement.EndDate.Subtract(registDt).TotalDays);
                            ReturnCode.NumberOfDaysLeft = daysLeft < 0 ? 0 : daysLeft;
                            ReturnCode.IsExpired = daysLeft > 0 ? false : true;
                            ReturnCode.EndDate = entitlement.EndDate;
                            ReturnCode.EndDateStr = entitlement.EndDate.ToString("yyyy/MM/dd hh:mm:ss");
                            ReturnCode.StatusCode = (int)ErrorCodes.Success;
                        }
                        else
                        {
                            entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == listofIncludedPackageIds.ElementAt(0));
                            if (entitlement != null)
                            {
                                var daysLeft = Convert.ToInt32(entitlement.EndDate.Subtract(registDt).TotalDays);
                                ReturnCode.NumberOfDaysLeft = daysLeft < 0 ? 0 : daysLeft;
                                ReturnCode.IsExpired = daysLeft > 0 ? false : true;
                                ReturnCode.EndDate = entitlement.EndDate;
                                ReturnCode.EndDateStr = entitlement.EndDate.ToString("yyyy/MM/dd hh:mm:ss");
                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                            }
                            else
                            {
                                ReturnCode.StatusMessage = "No entitlement found";
                            }
                        }
                    }
                    else
                        ReturnCode.StatusMessage = "User does not exist";
                }
                else
                    ReturnCode.StatusMessage = "Your session has already expired. Please login again.";
            }
            catch (Exception e) { MyUtility.LogException(e); ReturnCode.StatusMessage = String.Format("Exception: {0} | Inner Exception: {1}", e.Message, e.InnerException.Message); }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [RequireHttpsOnProductionOnly]
        public JsonResult UpdateMyProfile(FormCollection fc)
        {
            var ReturnCode = new ReturnCodeObj()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            try
            {
                DateTime registDt = DateTime.Now;
                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                var field_names = new string[] { "phone" };
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
                    string firstname = tmpCollection["firstname"];
                    string lastname = tmpCollection["lastname"];
                    string phone = string.Empty;
                    if (!String.IsNullOrEmpty(fc["phone"]))
                        phone = fc["phone"];

                    if (firstname.Length > 32)
                        ReturnCode.StatusMessage = "First Name cannot exceed 32 characters.";
                    if (lastname.Length > 32)
                        ReturnCode.StatusMessage = "Last Name cannot exceed 32 characters.";

                    if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                        return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);


                    if (User.Identity.IsAuthenticated)
                    {
                        var context = new IPTV2Entities();
                        var userId = new System.Guid(User.Identity.Name);
                        var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            user.FirstName = firstname;
                            user.LastName = lastname;
                            user.PhoneNumber = phone;

                            if (context.SaveChanges() > 0)
                            {
                                //update userinfo in gigya
                                SetGigyaAccountInfo(user);
                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                ReturnCode.StatusMessage = "You have successfully updated your profile";
                            }
                        }
                        else
                            ReturnCode.StatusMessage = "User does not exist.";
                    }
                    else
                        ReturnCode.StatusMessage = "Your session has already expired. Please login again.";
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";
            }
            catch (Exception e) { MyUtility.LogException(e); ReturnCode.StatusMessage = String.Format("Exception: {0} | Inner Exception: {1}", e.Message, e.InnerException.Message); }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [RequireHttpsOnProductionOnly]
        public JsonResult GetStoreFronts()
        {
            List<StoreFrontC> stores = new List<StoreFrontC>();
            try
            {
                List<StoreFront> result = null;
                using (var context = new IPTV2Entities())
                {
                    var storefronts = context.StoreFronts.Where(s => String.Compare(s.CountryCode, "HK", true) == 0 && s.StatusId == GlobalConfig.Visible && s.OfferingId == 5);
                    if (storefronts != null)
                        result = storefronts.ToList();
                }

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        stores.Add(new StoreFrontC
                        {
                            Address1 = item.Address1,
                            Address2 = item.Address2,
                            BusinessName = item.BusinessName,
                            BusinessPhone = item.BusinessPhone,
                            City = item.City,
                            ContactPerson = item.ContactPerson,
                            CountryCode = item.CountryCode,
                            EMailAddress = item.EMailAddress,
                            Latitude = item.Latitude,
                            Longitude = item.Longitude,
                            MobilePhone = item.MobilePhone,
                            State = item.State,
                            StoreFrontId = item.StoreFrontId,
                            WebSiteUrl = item.WebSiteUrl,
                            ZipCode = item.ZipCode
                        });
                    }
                }
            }
            catch (Exception) { }
            return this.Json(stores, JsonRequestBehavior.AllowGet);
        }

        [RequireHttpsOnProductionOnly]
        public JsonResult GSocialize()
        {
            var ReturnCode = new SocializeReturnCodeObj()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            var registDt = DateTime.Now;
            var skipValidation = false;
            try
            {
                if (!String.IsNullOrEmpty(Request.QueryString["sv"]))
                {
                    var svTemp = Convert.ToInt32(Request.QueryString["sv"]);
                    if (svTemp == 1)
                        skipValidation = true;
                }
            }

            catch (Exception) { }
            try
            {
                NameValueCollection qs = Request.QueryString;
                string gigyaUID = Uri.UnescapeDataString(qs["UID"]);
                bool isRequestValid = SigUtils.ValidateUserSignature(gigyaUID, Uri.UnescapeDataString(qs["timestamp"]), GlobalConfig.GSsecretkey, Uri.UnescapeDataString(qs["signature"]));
                if (isRequestValid || skipValidation)
                {
                    using (var context = new IPTV2Entities())
                    {
                        User user = null;
                        bool isSiteUID = Convert.ToBoolean(qs["isSiteUID"]);
                        if (isSiteUID) //Old user. Signin to site
                        {
                            var UserId = new Guid(gigyaUID);
                            user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                            if (user != null)
                            {
                                if (user.StatusId == GlobalConfig.Visible) //Successful Login
                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                else
                                    ReturnCode.StatusMessage = "Account is not verified in our system.";
                            }
                            else
                            {
                                //ReturnCode.StatusMessage = "Social networking account does not exist in our system.";
                                //Create user                                
                                string FirstName = qs["first_name"];
                                string LastName = qs["last_name"];
                                string EMail = qs["login_email"];
                                string uid = qs["uid"];
                                string provider = qs["provider"];
                                string Password = Membership.GeneratePassword(10, 2);
                                string City = String.Empty;
                                string State = String.Empty;
                                string CountryCode = GlobalConfig.DefaultCountry;
                                var id = UserId;
                                var ip = qs["ip"];

                                try
                                {
                                    var location = MyUtility.GetLocationBasedOnIpAddress(ip);
                                    City = location.city;
                                    CountryCode = location.countryCode;
                                    State = String.Compare(CountryCode, GlobalConfig.DefaultCountry, true) == 0 ? location.region : location.regionName;
                                }
                                catch (Exception) { }

                                user = new User()
                                {
                                    UserId = id,
                                    FirstName = FirstName,
                                    LastName = LastName,
                                    City = City,
                                    State = State,
                                    CountryCode = CountryCode,
                                    EMail = EMail,
                                    Password = MyUtility.GetSHA1(Password),
                                    GigyaUID = id.ToString(),
                                    RegistrationDate = registDt,
                                    LastUpdated = registDt,
                                    RegistrationIp = ip ?? MyUtility.GetClientIpAddress(),
                                    DateVerified = registDt,
                                    StatusId = GlobalConfig.Visible,
                                    ActivationKey = Guid.NewGuid()
                                };

                                var CurrencyCode = GlobalConfig.DefaultCurrency;
                                var country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);
                                if (country != null)
                                    CurrencyCode = country.CurrencyCode;

                                var wallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, CurrencyCode, true) == 0);
                                if (wallet == null)
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
                                    Reference = "New Registration (AIR PLUS)",
                                    Date = registDt,
                                    OfferingId = GlobalConfig.offeringId,
                                    UserId = user.UserId,
                                    StatusId = GlobalConfig.Visible
                                };

                                user.Transactions.Add(transaction);
                                context.Users.Add(user);

                                if (context.SaveChanges() > 0)
                                {
                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                    GSResponse res = null;

                                    GigyaUserData2 userData = new GigyaUserData2()
                                    {
                                        city = user.City,
                                        country = user.CountryCode,
                                        email = user.EMail,
                                        firstName = user.FirstName,
                                        lastName = user.LastName,
                                        state = user.State
                                    };

                                    TFCTV.Helpers.UserData privacyData = new UserData() { IsExternalSharingEnabled = "true,false", IsInternalSharingEnabled = "true,false", IsProfilePrivate = "false" };
                                    GigyaUserDataInfo2 userDataInfo = new GigyaUserDataInfo2()
                                    {
                                        UID = user.UserId.ToString(),
                                        profile = Newtonsoft.Json.JsonConvert.SerializeObject(userData, Formatting.None),
                                        data = Newtonsoft.Json.JsonConvert.SerializeObject(privacyData, Formatting.None)
                                    };

                                    GSObject userDataInfoObj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(userDataInfo));                                    //res = GigyaHelpers.createAndSendRequest("gcs.setUserData", userDataInfoObj);
                                    res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", userDataInfoObj);

                                    //Publish to Activity Feed
                                    List<ActionLink> actionlinks = new List<ActionLink>();
                                    actionlinks.Add(new ActionLink() { text = SNSTemplates.register_actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_actionlink_href) });
                                    //mediaItem
                                    List<MediaItem> mediaItems = new List<MediaItem>();
                                    mediaItems.Add(new MediaItem() { type = SNSTemplates.register_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.register_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_mediaitem_href) });
                                    UserAction action = new UserAction()
                                    {
                                        actorUID = user.UserId.ToString(),
                                        userMessage = SNSTemplates.register_usermessage,
                                        title = SNSTemplates.register_title,
                                        subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_subtitle),
                                        linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_linkback),
                                        description = String.Format(SNSTemplates.register_description, FirstName),
                                        actionLinks = actionlinks,
                                        mediaItems = mediaItems
                                    };

                                    GigyaMethods.PublishUserAction(action, user.UserId, "external");
                                    action.userMessage = String.Empty;
                                    action.title = String.Empty;
                                    action.mediaItems = null;
                                    GigyaMethods.PublishUserAction(action, user.UserId, "internal");
                                }
                            }
                        }
                        else //New user. allow user to register
                        {
                            bool createUser = true;
                            if (!String.IsNullOrEmpty(qs["email"]))
                            {
                                string email = qs["email"];
                                user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, email, true) == 0);
                                if (user != null) // link account
                                {
                                    Dictionary<string, object> collection = new Dictionary<string, object>();
                                    collection.Add("siteUID", user.UserId);
                                    collection.Add("uid", Uri.UnescapeDataString(qs["UID"]));
                                    collection.Add("cid", String.Format("{0} - New User", qs["provider"]));

                                    GSObject obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(collection));
                                    GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", obj);
                                    if (res.GetErrorCode() == 0) //Successful link
                                    {
                                        createUser = false;
                                        var UserId = user.UserId.ToString();
                                        user.StatusId = GlobalConfig.Visible; //activate account
                                        user.DateVerified = DateTime.Now;
                                        if (context.SaveChanges() > 0)
                                            ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                        else
                                            ReturnCode.StatusMessage = "Create user failed";
                                    }
                                    else
                                        ReturnCode.StatusMessage = res.GetErrorMessage();
                                }
                            }

                            if (createUser)
                            {
                                string FirstName = qs["first_name"];
                                string LastName = qs["last_name"];
                                string EMail = qs["login_email"];
                                string uid = qs["uid"];
                                string provider = qs["provider"];
                                string Password = Membership.GeneratePassword(10, 2);
                                string City = String.Empty;
                                string State = String.Empty;
                                string CountryCode = GlobalConfig.DefaultCountry;
                                var id = Guid.NewGuid();
                                var ip = qs["ip"];

                                try
                                {
                                    var location = MyUtility.GetLocationBasedOnIpAddress(ip);
                                    City = location.city;
                                    CountryCode = location.countryCode;
                                    State = String.Compare(CountryCode, GlobalConfig.DefaultCountry, true) == 0 ? location.region : location.regionName;
                                }
                                catch (Exception) { }


                                user = new User()
                                {
                                    UserId = id,
                                    FirstName = FirstName,
                                    LastName = LastName,
                                    City = City,
                                    State = State,
                                    CountryCode = CountryCode,
                                    EMail = EMail,
                                    Password = MyUtility.GetSHA1(Password),
                                    GigyaUID = id.ToString(),
                                    RegistrationDate = registDt,
                                    LastUpdated = registDt,
                                    RegistrationIp = ip ?? MyUtility.GetClientIpAddress(),
                                    ActivationKey = Guid.NewGuid()
                                };

                                var CurrencyCode = GlobalConfig.DefaultCurrency;
                                var country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);
                                if (country != null)
                                    CurrencyCode = country.CurrencyCode;

                                var wallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, CurrencyCode, true) == 0);
                                if (wallet == null)
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
                                    Reference = "New Registration (AIR PLUS)",
                                    Date = registDt,
                                    OfferingId = GlobalConfig.offeringId,
                                    UserId = user.UserId,
                                    StatusId = GlobalConfig.Visible
                                };

                                user.Transactions.Add(transaction);
                                context.Users.Add(user);

                                if (context.SaveChanges() > 0)
                                {
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
                                                if (context.SaveChanges() > 0)
                                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                            }
                                        }
                                    }
                                    else
                                        ReturnCode.StatusMessage = "Missing parameters uid & provider";
                                    //else
                                    //{
                                    //    var info = new GigyaUserInfo()
                                    //    {
                                    //        firstName = FirstName,
                                    //        lastName = LastName,
                                    //        email = EMail
                                    //    };

                                    //    var registrationInfo = new GigyaNotifyLoginInfo()
                                    //    {
                                    //        siteUID = user.UserId.ToString(),
                                    //        cid = "TFCTV - Registration",
                                    //        sessionExpiration = 0,
                                    //        newUser = true,
                                    //        userInfo = Newtonsoft.Json.JsonConvert.SerializeObject(info)
                                    //    };
                                    //    GSObject obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(registrationInfo));
                                    //    res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", obj);
                                    //    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                    //}

                                    if (ReturnCode.StatusCode == (int)ErrorCodes.Success)
                                    {
                                        GigyaUserData2 userData = new GigyaUserData2()
                                        {
                                            city = user.City,
                                            country = user.CountryCode,
                                            email = user.EMail,
                                            firstName = user.FirstName,
                                            lastName = user.LastName,
                                            state = user.State
                                        };

                                        TFCTV.Helpers.UserData privacyData = new UserData() { IsExternalSharingEnabled = "true,false", IsInternalSharingEnabled = "true,false", IsProfilePrivate = "false" };

                                        GigyaUserDataInfo2 userDataInfo = new GigyaUserDataInfo2()
                                        {
                                            UID = user.UserId.ToString(),
                                            profile = Newtonsoft.Json.JsonConvert.SerializeObject(userData, Formatting.None),
                                            data = Newtonsoft.Json.JsonConvert.SerializeObject(privacyData, Formatting.None)
                                        };

                                        GSObject userDataInfoObj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(userDataInfo));                                    //res = GigyaHelpers.createAndSendRequest("gcs.setUserData", userDataInfoObj);
                                        res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", userDataInfoObj);

                                        //Publish to Activity Feed
                                        List<ActionLink> actionlinks = new List<ActionLink>();
                                        actionlinks.Add(new ActionLink() { text = SNSTemplates.register_actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_actionlink_href) });
                                        //mediaItem
                                        List<MediaItem> mediaItems = new List<MediaItem>();
                                        mediaItems.Add(new MediaItem() { type = SNSTemplates.register_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.register_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_mediaitem_href) });
                                        UserAction action = new UserAction()
                                        {
                                            actorUID = user.UserId.ToString(),
                                            userMessage = SNSTemplates.register_usermessage,
                                            title = SNSTemplates.register_title,
                                            subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_subtitle),
                                            linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.register_linkback),
                                            description = String.Format(SNSTemplates.register_description, FirstName),
                                            actionLinks = actionlinks,
                                            mediaItems = mediaItems
                                        };

                                        GigyaMethods.PublishUserAction(action, user.UserId, "external");
                                        action.userMessage = String.Empty;
                                        action.title = String.Empty;
                                        action.mediaItems = null;
                                        GigyaMethods.PublishUserAction(action, user.UserId, "internal");
                                    }
                                }
                            }
                        }

                        if (ReturnCode.StatusCode == (int)ErrorCodes.Success)
                        {
                            ReturnCode.StatusMessage = "Success!";

                            //GenerateToken
                            SynapseUserInfo uInfo = new SynapseUserInfo() { firstName = user.FirstName, lastName = user.LastName, email = user.EMail };
                            Dictionary<string, object> collection = new Dictionary<string, object>();
                            collection.Add("client_id", GlobalConfig.GSapikey);
                            collection.Add("client_secret", GlobalConfig.GSsecretkey);
                            collection.Add("grant_type", "none");
                            collection.Add("x_siteUID", user.UserId);
                            collection.Add("x_sessionExpiration", 0);
                            collection.Add("x_userInfo", JsonConvert.SerializeObject(uInfo));
                            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getToken", GigyaHelpers.buildParameter(collection));
                            SynapseCookie cookie = new SynapseCookie()
                            {
                                cookieName = FormsAuthentication.FormsCookieName,
                                cookiePath = FormsAuthentication.FormsCookiePath,
                                cookieDomain = FormsAuthentication.CookieDomain
                            };

                            if (res.GetErrorCode() == 0)
                            {
                                HttpCookie authCookie = SetCookie(user.UserId.ToString());
                                cookie.cookieValue = authCookie.Value;
                                ContextHelper.SaveSessionInDatabase(context, user, authCookie.Value);
                                SynapseToken token = new SynapseToken()
                                {
                                    uid = user.UserId.ToString(),
                                    token = res.GetString("access_token", String.Empty),
                                    expire = res.GetInt("expires_in", 0),
                                };

                                ReturnCode.tk = token;
                                ReturnCode.gs = cookie;
                            }
                            else
                            {
                                ReturnCode.StatusCode = res.GetErrorCode();
                                ReturnCode.StatusMessage = res.GetErrorMessage();
                            }
                        }
                    }
                }
                else
                    ReturnCode.StatusMessage = "Request is not valid";
            }
            catch (Exception e) { MyUtility.LogException(e); ReturnCode.StatusMessage = String.Format("Exception: {0} | Inner Exception: {1}", e.Message, e.InnerException.Message); }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [RequireHttpsOnProductionOnly]
        public JsonResult Feed02()
        {
            var ReturnCode = new ServiceReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            //if (!Request.IsLocal)
            //    if (!GlobalConfig.isUAT)
            //        if (!Request.IsAjaxRequest())
            //        {
            //            ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
            //            ReturnCode.StatusMessage = "Request is not valid.";
            //            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
            //        }

            int id = GlobalConfig.TFCtvPaidMobileAirEpisodeId;
            DateTime registDt = DateTime.Now;
            try
            {
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();

                if (MyUtility.IsWhiteListed(String.Empty))
                    CountryCode = "HK";
                else
                {
                    try
                    {
                        var requestingIp = Request.GetUserHostAddressFromCloudflare();
                        var whiteListedIp = GlobalConfig.IpWhiteList.Split(',');
                        if (whiteListedIp.Contains(requestingIp) && GlobalConfig.isUAT)
                            CountryCode = "HK";
                    }
                    catch (Exception) { }
                }

                using (var context = new IPTV2Entities())
                {
                    Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                    if (episode != null)
                    {
                        if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                        {
                            var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                            var parentCategories = episode.GetParentShows(CacheDuration);
                            Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                            //var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true, offering);
                            var HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                            if (parentCategories != null)
                            {
                                if (User.Identity.IsAuthenticated)
                                {
                                    var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
                                    if (user != null)
                                        HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt, CountryCode);
                                    else
                                        HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
                                }
                                else
                                    HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
                            }

                            if (HasActiveSubscriptionBasedOnCategoryId.HasSubscription)
                            {
                                var premiumAsset = episode.PremiumAssets.LastOrDefault();
                                if (premiumAsset != null)
                                {
                                    Asset asset = premiumAsset.Asset;
                                    if (asset != null)
                                    {
                                        int assetId = asset == null ? 0 : asset.AssetId;
                                        if (episode.IsLiveChannelActive == true) //isLiveEvent
                                        {
                                            if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(premiumAsset))
                                            {
                                                if (Request.Browser.IsMobileDevice)
                                                {
                                                    ReturnCode.StatusCode = (int)ErrorCodes.IsNotAvailableOnMobileDevices;
                                                    ReturnCode.StatusMessage = "This stream is not available on this device";
                                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, true, CountryCodeOverride: CountryCode, RemoveIpFromToken: true);
                                            if (clipDetails != null)
                                                if (!String.IsNullOrEmpty(clipDetails.Url))
                                                {
                                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                    ReturnCode.StatusMessage = "OK";
                                                    ReturnCode.info = clipDetails.Url;
                                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                                }
                                        }
                                        ReturnCode.StatusMessage = "Clip not available.";
                                        return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                    }
                                    ReturnCode.StatusMessage = "Asset not available.";
                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                }
                            }
                            ReturnCode.StatusMessage = "Access to this clip is restricted.";
                            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                        }
                    }
                    ReturnCode.StatusMessage = "Episode does not exist.";
                }
            }
            catch (Exception e) { ReturnCode.StatusMessage = e.Message; }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [RequireHttpsOnProductionOnly]
        public JsonResult GetSubscriptionList()
        {
            List<EntitlementDisplay> list = null;
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var registDt = DateTime.Now;
                    using (var context = new IPTV2Entities())
                    {
                        var listofPackagesToBeIncluded = MyUtility.StringToIntList(GlobalConfig.ListOfAirPlusPackageIds);
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
                                        ExpiryDateStr = entitlement.EndDate.ToString("yyyy-MM-ddThh:mm:ss")
                                    };

                                    if (entitlement is PackageEntitlement)
                                    {
                                        var pkg = (PackageEntitlement)entitlement;
                                        if (listofPackagesToBeIncluded.Contains(pkg.PackageId))
                                        {
                                            disp.PackageId = pkg.PackageId;
                                            disp.PackageName = pkg.Package.Description;
                                            disp.Content = disp.PackageName;
                                            list.Add(disp);
                                        }
                                    }
                                    //else if (entitlement is ShowEntitlement)
                                    //{
                                    //    var show = (ShowEntitlement)entitlement;
                                    //    disp.CategoryId = show.CategoryId;
                                    //    disp.CategoryName = show.Show.Description;
                                    //    disp.Content = disp.CategoryName;
                                    //}
                                    //else if (entitlement is EpisodeEntitlement)
                                    //{
                                    //    var episode = (EpisodeEntitlement)entitlement;
                                    //    disp.EpisodeId = episode.EpisodeId;
                                    //    disp.EpisodeName = episode.Episode.Description + ", " + episode.Episode.DateAired.Value.ToString("MMMM d, yyyy");
                                    //    disp.Content = disp.EpisodeName;
                                    //}
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return this.Json(list, JsonRequestBehavior.AllowGet);
        }

        [RequireHttpsOnProductionOnly]
        public JsonResult GetPaidProgramSchedule(int id = 0)
        {
            List<ProgramSchedDisplay> obj = null;
            try
            {
                var context = new IPTV2Entities();
                DateTime utc = DateTime.Now.ToUniversalTime();

                DateTime gmt = utc.AddHours(8); //convert to GMT
                var dow = (int)gmt.DayOfWeek;

                var channels = GlobalConfig.ProjectAirProgramScheduleChannelIds.Split(','); //Channel Id (Sunday-Saturday)
                //var psChannelId = Convert.ToInt32(channels[dow]); //ProgramSchedule Channel Id
                //var psChannelIdTomorrow = Convert.ToInt32(channels[dowTomorrow]); //ProgramSchedule Channel Id for the next day
                var psChannelId = MyUtility.StringToIntList(String.Format("{0}", channels[dow]));

                var currentHour = utc.Hour;
                DateTime gmtTomorrow = gmt.AddDays(1);
                var dowTomorrow = (int)gmtTomorrow.DayOfWeek;
                var tomorrowCheck = false;
                if (currentHour >= 14) //Check if it's on or after 10pm
                {
                    tomorrowCheck = true;
                    psChannelId = MyUtility.StringToIntList(String.Format("{0},{1}", channels[dow], channels[dowTomorrow]));
                }

                var sked = context.ProgramSchedules.Where(p => psChannelId.Contains(p.ChannelId));
                if (sked != null)
                {
                    if (sked.Count() > 0)
                    {
                        sked = sked.OrderBy(p => p.ChannelId).ThenBy(p => p.StartTime);
                        List<ProgramSchedDisplay> list = new List<ProgramSchedDisplay>();
                        var slist = sked.ToList();
                        foreach (var item in slist)
                        {
                            try
                            {
                                var startDt = Convert.ToDateTime(item.StartTime.Trim());
                                var endDt = Convert.ToDateTime(item.EndTime.Trim());
                                if (tomorrowCheck)
                                {
                                    if (item.ChannelId == Convert.ToInt32(channels[dowTomorrow]))
                                    {
                                        startDt = startDt.AddDays(1);
                                        endDt = endDt.AddDays(1);
                                    }
                                }
                                var psd = new ProgramSchedDisplay()
                                {
                                    Pid = item.ProgramScheduleId,
                                    CategoryName = item.ShowName.TrimEnd(),
                                    CategoryId = item.CategoryId == null ? 0 : (int)item.CategoryId,
                                    Description = item.Blurb,
                                    StartDate = startDt,
                                    EndDate = endDt,
                                    StartDateStr = startDt.ToString("MM/dd/yyyy HH:mm:ss"),
                                    EndDateStr = endDt.ToString("MM/dd/yyyy HH:mm:ss"),
                                    StartTime = item.StartTime.Trim(),
                                    EndTime = item.EndTime.Trim()
                                };
                                list.Add(psd);
                            }
                            catch (Exception) { throw; }
                        }

                        if (id == 1)
                            obj = list.Where(l => l.EndDate > gmt).ToList();
                        else
                            obj = list.ToList();
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); throw; }
            return this.Json(obj, JsonRequestBehavior.AllowGet);
        }

        [RequireHttpsOnProductionOnly]
        public JsonResult GetAds(int id = 1)
        {
            List<AirPlusAdsObj> list = new List<AirPlusAdsObj>();
            try
            {
                if (id > 0)
                {
                    using (var context = new IPTV2Entities())
                    {
                        var registDt = DateTime.Now;
                        var adTypeIds = MyUtility.StringToIntList(GlobalConfig.AirPlusAdTypeIds);
                        var adTypeId = adTypeIds.ElementAt(id - 1);
                        var adIds = context.EpisodeCategories1.Where(e => e.CategoryId == adTypeId).Select(e => e.EpisodeId);
                        //var ads = context.Episodes.Where(e => adIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).ThenByDescending(e => e.EpisodeId);
                        var ads = context.Episodes.Where(e => adIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderBy(e => Guid.NewGuid()).FirstOrDefault();
                        if (ads != null)
                        {
                            try
                            {
                                string img = String.Empty;
                                if (!String.IsNullOrEmpty(ads.ImageAssets.ImageVideo))
                                    img = String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ads.EpisodeId, ads.ImageAssets.ImageVideo);

                                list.Add(new AirPlusAdsObj()
                                {
                                    copy1 = ads.Description,
                                    copy2 = ads.Synopsis,
                                    cta = ads.Metadata,
                                    img = img,
                                    designtype = ads.EpisodeLength == null ? 1 : (int)ads.EpisodeLength
                                });
                            }
                            catch (Exception ex) { MyUtility.LogException(ex); }

                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public virtual HttpCookie SetCookie(string uid)
        {
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(uid, true, GlobalConfig.FormsAuthenticationTimeout);
            string encTicket = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.Expires = DateTime.Now.AddMonths(1);
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
            return cookie;
        }

        private void SetUserTrackingCookie(string UserId)
        {
            //add uid cookie
            HttpCookie uidCookie = new HttpCookie("uid");
            uidCookie.Value = UserId;
            uidCookie.Expires = DateTime.Now.AddDays(30);
            Response.Cookies.Add(uidCookie);
        }

        private void SetGigyaAccountInfo(User user)
        {
            try
            {
                GigyaUserData3 userData = new GigyaUserData3()
                {
                    city = user.City,
                    country = user.CountryCode,
                    email = user.EMail,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    state = user.State,
                    phone = user.PhoneNumber
                };

                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("uid", user.UserId.ToString());
                collection.Add("profile", JsonConvert.SerializeObject(userData, Formatting.None));
                GSResponse res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", GigyaHelpers.buildParameter(collection));
            }
            catch (Exception) { }
        }

        public class ServerTimeObj
        {
            public string Now { get; set; }
            public string UniversalTime { get; set; }
            public string NowPlus8 { get; set; }
            public string UniversalTimePlus8 { get; set; }
        }
    }
}
