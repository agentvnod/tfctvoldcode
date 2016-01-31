using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using IPTV2_Model;
using Maxmind;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using SendGrid;
using Gigya.Socialize.SDK;
using com.Akamai.EdgeAuth;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Specialized;

namespace TFCTV.Helpers
{
    public static class MyUtility
    {
        // static LookupService ls;
        static IpLocation ipLocation;

        static MyUtility()
        {
            // var geoIpFile = HttpContext.Current.Request.MapPath(GlobalConfig.GeoIpPath);
            // ls = new LookupService(geoIpFile, LookupService.GEOIP_MEMORY_CACHE);
            ipLocation = new IpLocation();
        }

        public static Maxmind.Country getCountry(string ip)
        {
            // return ls.getCountry(ip);
            return ipLocation.GetCountry(ip);
        }

        public static Maxmind.Location getLocation(string ip)
        {
            // return ls.getLocation(ip);
            return ipLocation.GetLocation(ip);
        }

        public static DateTime getEntitlementEndDate(int duration, string interval, DateTime registDt)
        {
            DateTime d = DateTime.Now;
            switch (interval)
            {
                case "d": d = registDt.AddDays(duration); break;
                case "m": d = registDt.AddMonths(duration); break;
                case "y": d = registDt.AddYears(duration); break;
                case "h": d = registDt.AddHours(duration); break;
                case "mm": d = registDt.AddMinutes(duration); break;
                case "s": d = registDt.AddSeconds(duration); break;
            }
            return d;
        }

        public static string buildJson(Dictionary<string, object> collection)
        {
            return JsonConvert.SerializeObject(collection, Formatting.None);
        }

        public static string getErrorMessage(ErrorCodes errorCode)
        {
            string error = "";
            switch (errorCode)
            {
                case ErrorCodes.InsufficientWalletLoad: error = "Insufficient Wallet Load"; break;
                case ErrorCodes.Success: error = "Success"; break;
                case ErrorCodes.IsExistingEmail: error = "Email address is already taken."; break;
                case ErrorCodes.NotAuthenticated: error = "User is not authenticated"; break;
                case ErrorCodes.IsMismatchPassword: error = "Passwords do not match."; break;
                case ErrorCodes.IsEmailEmpty: error = "Email address is required."; break;
                case ErrorCodes.IsNotValidEmail: error = "Email format is invalid."; break;
                case ErrorCodes.IsMissingRequiredFields: error = "Please fill in the required fields."; break;
                case ErrorCodes.UserDoesNotExist: error = "User does not exist."; break;
                case ErrorCodes.IsWrongPassword: error = "Email & password do not match."; break;
                case ErrorCodes.ProductIsNotPurchaseable: error = "This product is currently not for sale."; break;
                case ErrorCodes.IsNotAllowedToBuyLite: error = "The recipient is already subscribed to Premium. You can no longer send Lite gift to this user."; break;
                case ErrorCodes.IsNotAllowedToBuyPremium: error = "The recipient is already subscribed to Lite. You can no longer send Premium gift to this user."; break;
                case ErrorCodes.ProductIsNull: error = "You are trying to buy a non-existent product. Please try again."; break;
                case ErrorCodes.UnknownError: error = "The system encounted an unspecified error. Please send feedback to the administrators via Feedback Form."; break;
                case ErrorCodes.ProductIsNotAllowedInCountry: error = "The subscription is not available in the recipient's location. You are not allowed to send this gift."; break;

                default:
                    error = "Something went wrong.";
                    break;
            }

            return error;
        }

        public static string getPpcError(Ppc.ErrorCode errorCode)
        {
            string error = "";
            switch (errorCode)
            {
                case Ppc.ErrorCode.InactivePpc: error = "Your Prepaid Card/ePIN needs to be activated. Contact your TFC.tv dealer or visit our <a href=\"/Help\">Help Center</a>"; break;
                case Ppc.ErrorCode.Success: error = "Success"; break;
                case Ppc.ErrorCode.InvalidCountry: error = "The Prepaid Card/ePIN you have submitted is not valid in this country."; break;
                case Ppc.ErrorCode.InvalidCurrency: error = "The Prepaid Card/ePIN you have submitted is not valid in this country."; break;
                case Ppc.ErrorCode.InvalidPin: error = "The serial number and the PIN you have entered do not match."; break;
                case Ppc.ErrorCode.InvalidSerialNumber: error = "The serial number you have entered does not exist."; break;
                case Ppc.ErrorCode.InvalidUser: error = "User is invalid."; break;
                case Ppc.ErrorCode.NotAReloadPpc: //error = "You have submitted a subscription card. A load card is required.";
                    error = "Your Prepaid Card/ePIN does not match this transaction. Submit your prepaid card <a href=\"/Ppc\">here.</a>";
                    break;
                case Ppc.ErrorCode.NotASubscriptionPpc: //error = "You have submitted a load card. A subscription card is required.";
                    error = "Your Prepaid Card/ePIN does not match this transaction. Submit your prepaid card <a href=\"/Ppc\">here.</a>";
                    break;
                case Ppc.ErrorCode.PpcAlreadyUsed: error = "The Prepaid Card/ePIN you have entered has already been used."; break;
                case Ppc.ErrorCode.PpcDoesNotMatchSubscriptionProduct: error = "Your Prepaid Card/ePIN does not match your purchase requirements."; break;
                case Ppc.ErrorCode.PpcPriceDoesNotMatchProductPrice:
                case Ppc.ErrorCode.PpcHasNoMatchingProductPrice: error = "Your Prepaid Card/ePIN does not match your purchase requirements."; break;
                case Ppc.ErrorCode.PrefixOfStartAndEndingSerialDoNotMatch:
                case Ppc.ErrorCode.StartSerialShouldBeLessThanEndSerial: error = "The serial number you have entered does not exist."; break;
                case Ppc.ErrorCode.UserWalletIsNotActive:
                case Ppc.ErrorCode.UserWalletNotFound: error = "User's wallet does not exist."; break;
                case Ppc.ErrorCode.UnknownError: error = "The system encounted an unidentified error. Please try again."; break;
                case Ppc.ErrorCode.IsExpiredPpc: error = "This Prepaid Card/ePIN is already expired."; break;
                case Ppc.ErrorCode.HasConsumedTrialPpc: error = "You have already consumed a trial card/ePIN."; break;
                default:
                    error = "The system encounted an unspecified error. Please send feedback to the administrators via Feedback Form.";
                    break;
            }

            return error;
        }

        public static string GetPpcErrorMessages(Ppc.ErrorCode code)
        {
            string error = String.Empty;
            switch (code)
            {
                case Ppc.ErrorCode.InvalidSerialNumber: error = "Prepaid Card/ePIN does not exist."; break;
                case Ppc.ErrorCode.InvalidPin: error = "Invalid serial/pin combination."; break;
                case Ppc.ErrorCode.PpcAlreadyUsed: error = "Prepaid Card/ePIN is already used."; break;
                case Ppc.ErrorCode.InactivePpc: error = "Prepaid Card/ePIN is inactive/expired."; break;
                case Ppc.ErrorCode.InvalidCountry:
                case Ppc.ErrorCode.InvalidCurrency: error = "This Prepaid Card/ePIN is not allowed in your country."; break;
                case Ppc.ErrorCode.NotASubscriptionPpc: error = "This Prepaid Card/ePIN can not be used for subscription."; break;
                case Ppc.ErrorCode.PpcDoesNotMatchSubscriptionProduct: error = "This Prepaid Card/ePIN is not valid for this product."; break;
                case Ppc.ErrorCode.PpcHasNoMatchingProductPrice: error = "This Prepaid Card/ePIN is not valid for this product."; break;
                case Ppc.ErrorCode.PpcPriceDoesNotMatchProductPrice: error = "This Prepaid Card/ePIN does not have enough credits to buy this product."; break;
                case Ppc.ErrorCode.NotAReloadPpc: error = "This Prepaid Card/ePIN can not be used for E-Wallet reloading."; break;
                case Ppc.ErrorCode.HasConsumedTrialPpc: error = "You have already consumed a trial card/ePIN."; break;
                case Ppc.ErrorCode.IsExpiredPpc: error = "This Prepaid Card/ePIN is already expired."; break;
                case Ppc.ErrorCode.UserWalletIsNotActive:
                case Ppc.ErrorCode.UserWalletNotFound: error = "User's wallet does not exist."; break;
                case Ppc.ErrorCode.PrefixOfStartAndEndingSerialDoNotMatch:
                case Ppc.ErrorCode.StartSerialShouldBeLessThanEndSerial: error = "The serial number you have entered does not exist."; break;
                default: error = "The system encounted an unspecified error."; break;
            }
            return error;
        }

        public static bool isUserLoggedIn()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
                return true;
            return false;
        }

        public static Dictionary<string, object> setError(ErrorCodes errorCode, string errorMessage)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary["errorCode"] = (int)errorCode;
            dictionary["errorMessage"] = errorMessage;
            return dictionary;
        }

        public static Dictionary<string, object> setError(ErrorCodes errorCode)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary["errorCode"] = (int)errorCode;
            dictionary["errorMessage"] = MyUtility.getErrorMessage(errorCode);
            return dictionary;
        }

        public static string Ellipsis(string text, int length)
        {
            if (text.Length <= length) return text;
            int pos = text.IndexOf(" ", length);
            if (pos >= 0)
                return text.Substring(0, pos) + "...";
            return text;
        }

        public static string GetCurrencyOrDefault(string CountryCode)
        {
            var context = new IPTV2Entities();
            string CurrencyCode = GlobalConfig.DefaultCurrency;
            IPTV2_Model.Country country = context.Countries.FirstOrDefault(c => c.Code == CountryCode);
            if (country != null)
                CurrencyCode = country.CurrencyCode;
            return CurrencyCode;
        }

        public static string GetClientIpAddress()
        {
            return HttpContext.Current.Request.GetUserHostAddressFromCloudflare();
            string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            try
            {
                if (!string.IsNullOrEmpty(ip))
                {
                    string[] ipRange = ip.Split(',');
                    ip = ipRange[0];
                }
                else
                    ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            catch (Exception) { ip = HttpContext.Current.Request.UserHostAddress; }
            return ip;
        }

        public static string GetCurrentCountryCodeOrDefault()
        {
            try
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var context = new IPTV2Entities();
                    System.Guid UserId = new System.Guid(HttpContext.Current.User.Identity.Name);
                    User User = context.Users.Find(UserId);
                    return User == null ? MyUtility.getCountry(MyUtility.GetClientIpAddress()).getCode() : User.CountryCode;
                }
                //Get Default CountryCode or CountryCode base on current User IP Address
                return MyUtility.getCountry(MyUtility.GetClientIpAddress()).getCode() == "--" ? GlobalConfig.DefaultCountry : MyUtility.getCountry(MyUtility.GetClientIpAddress()).getCode();
            }
            catch (Exception) { return "US"; }
            //GetUserCountry Code            
        }

        public static IEnumerable<int> StringToIntList(string str)
        {
            if (String.IsNullOrEmpty(str))
                yield break;

            foreach (var s in str.Split(','))
            {
                int num;
                if (int.TryParse(s, out num))
                    yield return num;
            }
        }

        public static string GetFullName()
        {
            string fullName = "";
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(HttpContext.Current.User.Identity.Name));
                if (user != null)
                {
                    fullName = String.Format("{0} {1}", user.FirstName, user.LastName);
                }
            }
            return fullName;
        }

        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static string ReplaceHighlightingCharacters(string text, string beginStr, string endStr)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            var regexBegin = new Regex("\uE000", RegexOptions.Multiline);
            var regexEnd = new Regex("\uE001", RegexOptions.Multiline);
            return regexEnd.Replace(regexBegin.Replace(text, beginStr), endStr);
        }

        public static string GetQuestionId(HttpRequestBase req)
        {
            string userAgent = req.UserAgent;
            if (System.Text.RegularExpressions.Regex.IsMatch(userAgent, "msie", RegexOptions.IgnoreCase))
                return Settings.GetSettings("MSIEqid");
            else if (System.Text.RegularExpressions.Regex.IsMatch(userAgent, "chrome", RegexOptions.IgnoreCase))
                return Settings.GetSettings("Chromeqid");
            else if (System.Text.RegularExpressions.Regex.IsMatch(userAgent, "firefox", RegexOptions.IgnoreCase))
                return Settings.GetSettings("Firefoxqid");
            else if (System.Text.RegularExpressions.Regex.IsMatch(userAgent, "opera|presto", RegexOptions.IgnoreCase))
                return Settings.GetSettings("Operaqid");
            else if (System.Text.RegularExpressions.Regex.IsMatch(userAgent, "safari", RegexOptions.IgnoreCase))
                return Settings.GetSettings("Safariqid");
            else
                return Settings.GetSettings("MSIEqid");
        }

        public static bool isEmail(string inputEmail)
        {
            if (String.IsNullOrEmpty(inputEmail) || inputEmail.Length == 0)
                return false;
            const string expression = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                      @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                      @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex regex = new Regex(expression);
            return regex.IsMatch(inputEmail);
        }

        public static bool isSearchSpider(HttpRequestBase req)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(req.UserAgent, "bingbot|googlebot|msnbot|Slurp", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public static void SendEmailViaSendGrid(string to, string from, string subject, string body)
        {
            try
            {
                //var message = SendGrid.GenerateInstance();
                var message = new SendGridMessage();
                message.AddTo(to);
                message.From = new System.Net.Mail.MailAddress(from, "TFC.tv Team");
                message.Subject = subject;
                //message.Html = body;
                message.Text = body;
                message.EnableOpenTracking();
                message.EnableClickTracking();
                message.DisableUnsubscribe();
                message.DisableFooter();
                message.EnableBypassListManagement();
                //var transportInstance = SMTP.GenerateInstance(new System.Net.NetworkCredential(GlobalConfig.SendGridUsername, GlobalConfig.SendGridPassword), GlobalConfig.SendGridSmtpHost, GlobalConfig.SendGridSmtpPort);
                var transportInstance = new Web(new System.Net.NetworkCredential(GlobalConfig.SendGridUsername, GlobalConfig.SendGridPassword));
                transportInstance.Deliver(message);
            }
            catch (Exception) { throw; }
        }

        public static void SendEmailViaSendGrid(string to, string from, string subject, string htmlBody, MailType type, string textBody)
        {
            try
            {
                //var message = SendGrid.GenerateInstance();
                var message = new SendGridMessage();
                message.AddTo(to);
                message.From = new System.Net.Mail.MailAddress(from, "TFC.tv Team");
                message.Subject = subject;
                if (type == MailType.TextOnly)
                    message.Text = textBody.Replace(@"\r\n", Environment.NewLine);
                else if (type == MailType.HtmlOnly)
                    message.Html = htmlBody;
                else
                {
                    message.Html = htmlBody;
                    message.Text = textBody;
                }

                message.EnableOpenTracking();
                message.EnableClickTracking();
                message.DisableUnsubscribe();
                message.DisableFooter();
                message.EnableBypassListManagement();
                //var transportInstance = SMTP.GenerateInstance(new System.Net.NetworkCredential(GlobalConfig.SendGridUsername, GlobalConfig.SendGridPassword), GlobalConfig.SendGridSmtpHost, GlobalConfig.SendGridSmtpPort);
                var transportInstance = new Web(new System.Net.NetworkCredential(GlobalConfig.SendGridUsername, GlobalConfig.SendGridPassword));
                transportInstance.Deliver(message);
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// method for converting a System.DateTime value to a UNIX Timestamp
        /// </summary>
        /// <param name="value">date to convert/// <returns></returns>
        public static double ConvertToTimestamp(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return (double)span.TotalSeconds;
        }

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public static string GetABSCBNSubsidiary()
        {
            string subsidiary = "International";
            try
            {
                var context = new IPTV2Entities();
                string userCountry = String.Empty;
                if (MyUtility.isUserLoggedIn())
                {
                    var userId = new Guid(HttpContext.Current.User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        userCountry = user.CountryCode;
                }
                else
                    userCountry = MyUtility.getCountry(MyUtility.GetClientIpAddress()).getCode();

                if (String.IsNullOrEmpty(userCountry))
                    userCountry = GlobalConfig.DefaultCountry;
                var uCountry = context.Countries.FirstOrDefault(c => c.Code == userCountry);

                if (uCountry != null)
                {
                    if (uCountry.GomsSubsidiary != null)
                    {
                        switch (uCountry.GomsSubsidiary.Code)
                        {
                            case "GLB : AP":
                            case "GLB : AU": subsidiary = "Australia Pty. Ltd."; break;
                            case "GLB : IT":
                            case "GLB : IT : UK":
                            case "GLB : IT : ES": subsidiary = "Europe Ltd."; break;
                            case "GLB : CA": subsidiary = "Canada ULC"; break;
                            case "GLB : ME": subsidiary = "Middle East FZ-LLC"; break;
                            case "GLB : JP": subsidiary = "Japan, Inc."; break;
                            case "GLB : US":
                            default: subsidiary = "International"; break;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return String.Format("{0} {1}", "ABS-CBN", subsidiary);
        }

        public static bool IsIos(HttpRequestBase req)
        {
            return IsIos(req.UserAgent);
        }

        public static bool IsIos(string userAgent)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(userAgent, "iPad|iPhone|iPod");
        }

        public static UserData GetUserPrivacySetting(Guid? userId)
        {
            UserData uData = new UserData() { IsExternalSharingEnabled = "true,false", IsInternalSharingEnabled = "true,false", IsProfilePrivate = "false" };
            if (userId == null)
                return uData;
            try
            {
                var userData = GigyaMethods.GetUserData((Guid)userId, "IsExternalSharingEnabled,IsInternalSharingEnabled,IsProfilePrivate");
                if (userData.GetKeys().Count() >= 3)
                    if (userData != null)
                        uData = JsonConvert.DeserializeObject<UserData>(userData.ToJsonString());
            }
            catch (Exception)
            {
            }
            return uData;
        }

        public static string ToUpperFirstLetter(this string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;
            // convert to char array of the string
            char[] letters = source.ToCharArray();
            // upper case the first char
            letters[0] = char.ToUpper(letters[0]);
            // return the array made of the new char array
            return new string(letters);
        }

        public static string GetActivityFeedScope(UserData userData)
        {
            string scope = String.Empty;
            if (userData.IsExternalSharingEnabled.Contains("true") && userData.IsInternalSharingEnabled.Contains("true"))
                scope = "both";
            else if (userData.IsInternalSharingEnabled.Contains("true"))
                scope = "internal";
            else if (userData.IsExternalSharingEnabled.Contains("true"))
                scope = "external";
            return scope;
        }

        public static string GetActivityFeedPrivacy(UserData userData)
        {
            string privacy = "private";
            if (userData.IsInternalSharingEnabled.Contains("true"))
                privacy = "public";
            return privacy;
        }

        public static string GenerateAkamaiToken(string videoUrl, string Payload)
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
            double unixTime = ts.TotalSeconds;

            var tokenConfig = new AkamaiTokenConfig();
            tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
            tokenConfig.StartTime = Convert.ToUInt32(unixTime);
            tokenConfig.Window = 300;
            tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
            tokenConfig.Acl = "/*";
            tokenConfig.IP = HttpContext.Current.Request.GetUserHostAddressFromCloudflare();
            tokenConfig.PreEscapeAcl = false;
            tokenConfig.IsUrl = false;
            tokenConfig.SessionID = string.Empty;
            tokenConfig.Payload = Payload;
            tokenConfig.Salt = string.Empty;
            tokenConfig.FieldDelimiter = '~';
            var token = AkamaiTokenGenerator.GenerateToken(tokenConfig);
            if (!String.IsNullOrEmpty(videoUrl))
                videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "hdnea=" + token;
            return videoUrl;
        }

        public static string FormatUrls(string input)
        {
            string output = input;
            Regex regx = new Regex("http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*([a-zA-Z0-9\\?\\#\\=\\/]){1})?", RegexOptions.IgnoreCase);

            MatchCollection mactches = regx.Matches(output);

            foreach (Match match in mactches)
            {
                output = output.Replace(match.Value, "<a href=\"" + match.Value + "\" class=\"default_link\" style=\"font-size: 12px;\">" + match.Value + "</a>");
            }
            return output;
        }


        public static byte[] StrToByteArray(string str)
        {
            if (str.Length == 0)
                throw new Exception("Invalid string value in StrToByteArray");

            return HttpServerUtility.UrlTokenDecode(str);
        }

        public static string ByteArrToString(byte[] byteArr)
        {
            return HttpServerUtility.UrlTokenEncode(byteArr);
        }

        public static TFCTV.Models.Youtube.DBMGetUserResponse DBMGetUser(string videoId)
        {

            var url = String.Format(GlobalConfig.DBMGetUserUrl, videoId);
            var result = string.Empty;
            var webRequest = WebRequest.Create(url);
            webRequest.Timeout = 4000;
            try
            {
                using (var response = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var receiveStream = response.GetResponseStream();
                        if (receiveStream != null)
                        {
                            var stream = new StreamReader(receiveStream);
                            result = stream.ReadToEnd();
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        return new Models.Youtube.DBMGetUserResponse();
                    }
                }
                if (string.IsNullOrEmpty(result))
                    return null;
                var javaScriptSerializer = new JavaScriptSerializer();
                var apiResponse = javaScriptSerializer.Deserialize<Models.Youtube.DBMGetUserResponse>(result);
                return apiResponse;
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    WebException webEx = (WebException)e;
                    if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Forbidden)
                        return new Models.Youtube.DBMGetUserResponse();
                }
                return null;
            }
        }

        public static string GetUrlContent(string url)
        {
            var template = String.Empty;
            var webRequest = WebRequest.Create(url);
            webRequest.Timeout = 100000;
            while (String.IsNullOrEmpty(template))
            {
                try
                {
                    using (var response = webRequest.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var receiveStream = response.GetResponseStream();
                            if (receiveStream != null)
                            {
                                var stream = new StreamReader(receiveStream);
                                template = stream.ReadToEnd();
                            }
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            //return template;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return template;
        }

        public static void SendConfirmationEmail(IPTV2Entities context, User user)
        {
            try
            {

                var template = GetUrlContent(GlobalConfig.RegistrationCompleteTemplateUrl);
                var htmlBody = template.Replace("[firstname]", user.FirstName);
                var dealers = GetDealerNearUser(context, user, GlobalConfig.offeringId);
                htmlBody = htmlBody.Replace("[dealers]", dealers);
                SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, GlobalConfig.RegistrationCompleteSubject, htmlBody, MailType.HtmlOnly, String.Empty);
            }
            catch (Exception) { }
        }

        public static string GetDealerNearUser(IPTV2Entities context, User user, int offeringId)
        {
            string dealers = String.Empty;
            try
            {
                string userIp = user.RegistrationIp;
                if (!String.IsNullOrEmpty(userIp))
                {
                    var ipLocation = MyUtility.getLocation(userIp);
                    GeoLocation location = new GeoLocation() { Latitude = ipLocation.latitude, Longitude = ipLocation.longitude };
                    SortedSet<StoreFrontDistance> result;
                    if (Convert.ToInt32(Settings.GetSettings("maximumDistance")) != 0)
                        result = StoreFront.GetNearestStores(context, offeringId, location, true, 1000);
                    else
                        result = StoreFront.GetNearestStores(context, offeringId, location, true);
                    StringBuilder sb = new StringBuilder();
                    var ctr = 0;
                    foreach (var item in result)
                    {
                        var email = item.Store.EMailAddress;
                        var fullAddress = String.Format("{0}, {1}, {2} {3}", item.Store.Address1, item.Store.City, item.Store.State, item.Store.ZipCode);
                        string li = String.Format("<li>{0}<br />Address: {1}<br />Phone: {2}</li>", item.Store.BusinessName, fullAddress, item.Store.BusinessPhone);
                        sb.AppendLine(li);
                        ctr++;
                        if (ctr + 1 > 3)
                            break;
                    }

                    dealers = sb.ToString();
                }
            }
            catch (Exception) { }
            return dealers;
        }


        public static int GetCorrespondingFreeTrialPackageId()
        {
            int freeTrialPackageId = 0;
            var list = MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds);
            try
            {
                //int weekNumber = DateTime.Now.GetWeekOfMonth();
                int weekNumber = GetWeekNumberBasedOnStartDate();
                //if (weekNumber == 5) weekNumber = 1;
                try { freeTrialPackageId = list.ElementAt(weekNumber - 1); }
                catch (ArgumentOutOfRangeException) { freeTrialPackageId = list.ElementAt(0); }
            }
            catch (Exception)
            {
                freeTrialPackageId = list.ElementAt(0);
            }
            return freeTrialPackageId;
        }


        public static int GetCorrespondingFreeTrialProductId()
        {
            int freeTrialProductId = 0;
            var list = MyUtility.StringToIntList(GlobalConfig.FreeTrialProductIdsNEW);
            try
            {
                //int weekNumber = DateTime.Now.GetWeekOfMonth();
                int weekNumber = GetWeekNumberBasedOnStartDate();
                //if (weekNumber == 5) weekNumber = 1;
                try { freeTrialProductId = list.ElementAt(weekNumber - 1); }
                catch (ArgumentOutOfRangeException) { freeTrialProductId = list.ElementAt(0); }
            }
            catch (Exception)
            {
                freeTrialProductId = list.ElementAt(0);
            }
            return freeTrialProductId;
        }

        public static int GetWeekNumberBasedOnStartDate()
        {
            int weekNumber = 1;
            try
            {
                DateTime registDt = DateTime.Now;
                var startDt = Convert.ToDateTime(GlobalConfig.FreeTrialStartDt);
                var dateDiff = (int)Math.Floor((registDt - startDt).TotalDays);
                weekNumber = ((dateDiff / 7) % 4 + 1);
            }
            catch (Exception e) { MyUtility.LogException(e, "Unable to get week number based on start date."); }
            return weekNumber;

        }

        public static string Encrypt(string strToEncrypt, string strKey)
        {
            try
            {
                TripleDESCryptoServiceProvider objDESCrypto =
                    new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider objHashMD5 = new MD5CryptoServiceProvider();
                byte[] byteHash, byteBuff;
                string strTempKey = strKey;
                byteHash = objHashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(strTempKey));
                objHashMD5 = null;
                objDESCrypto.Key = byteHash;
                objDESCrypto.Mode = CipherMode.ECB; //CBC, CFB
                byteBuff = ASCIIEncoding.ASCII.GetBytes(strToEncrypt);
                return Convert.ToBase64String(objDESCrypto.CreateEncryptor().
                    TransformFinalBlock(byteBuff, 0, byteBuff.Length));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string Decrypt(string strEncrypted, string strKey)
        {
            try
            {
                TripleDESCryptoServiceProvider objDESCrypto =
                    new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider objHashMD5 = new MD5CryptoServiceProvider();
                byte[] byteHash, byteBuff;
                string strTempKey = strKey;
                byteHash = objHashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(strTempKey));
                objHashMD5 = null;
                objDESCrypto.Key = byteHash;
                objDESCrypto.Mode = CipherMode.ECB; //CBC, CFB
                byteBuff = Convert.FromBase64String(strEncrypted);
                string strDecrypted = ASCIIEncoding.ASCII.GetString
                (objDESCrypto.CreateDecryptor().TransformFinalBlock
                (byteBuff, 0, byteBuff.Length));
                objDESCrypto = null;
                return strDecrypted;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsAdRestricted(int categoryId)
        {
            var isRestricted = false;
            try
            {
                var RestrictedAdsCategoryIds = MyUtility.StringToIntList(GlobalConfig.RestrictedAdsCategoryIds);
                if (RestrictedAdsCategoryIds.Contains(categoryId))
                    isRestricted = true;
            }
            catch (Exception) { }
            return isRestricted;
        }

        public static bool isTVECookieValid()
        {
            var isValid = false;
            try
            {
                if (HttpContext.Current.Request.Cookies[".tve"] != null)
                {
                    HttpCookie cookie = HttpContext.Current.Request.Cookies[".tve"];
                    if (!String.IsNullOrEmpty(cookie.Value))
                    {
                        try
                        {
                            var cookieGuid = Guid.Parse(cookie.Value);
                            isValid = true;
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }
            return isValid;
        }

        public static void RemoveTVECookie()
        {
            try
            {
                if (HttpContext.Current.Request.Cookies[".tve"] != null)
                {
                    HttpCookie cookie = new HttpCookie(".tve");
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }
            catch (Exception) { }
        }

        public static string GetSlug(string slug)
        {
            try
            {
                slug = slug.Trim();
                Regex rx = new Regex(@"\/+");
                slug = rx.Replace(slug, " ");
                rx = new Regex(@"[^a-zA-Z0-9\-\s]");
                slug = rx.Replace(slug, "");
                rx = new Regex(@"\s+");
                slug = rx.Replace(slug, " ");
                rx = new Regex(@"\s");
                slug = rx.Replace(slug, "-");
                return slug.ToLower();
            }
            catch (Exception) { }
            return String.Empty;

        }

        public static void LogException(Exception exception, string message = "")
        {
            try
            {
                if (exception != null)
                    if (String.IsNullOrEmpty(message))
                        Elmah.ErrorSignal.FromCurrentContext().Raise(exception);
                    else
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Elmah.ApplicationException(message, exception));
            }
            catch (Exception) { }
        }

        public static bool IsTVEAllowedInCurrentCountry()
        {
            bool isAllowed = false;
            try
            {
                if (String.IsNullOrEmpty(GlobalConfig.TVECountryWhiteList))
                    isAllowed = false;
                else if (GlobalConfig.TVECountryWhiteList == "*")
                    isAllowed = true;
                else
                {
                    var countryBlackList = GlobalConfig.TVECountryWhiteList.Split(',');
                    var currentCountry = GetCurrentCountryCodeOrDefault();
                    isAllowed = countryBlackList.Contains(currentCountry);
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }
            return isAllowed;
        }

        public static string GetCountryCodeViaIp()
        {
            try
            {
                var ip = HttpContext.Current.Request.GetUserHostAddressFromCloudflare();
                var countryCode = MyUtility.getCountry(ip).getCode();
                return countryCode == "--" ? GlobalConfig.DefaultCountry : countryCode;
            }
            catch (Exception e) { MyUtility.LogException(e, "Get Country Code Via IP Address Error"); return "US"; }
        }

        public static string GetCountryCodeViaIpAddressWithoutProxy()
        {
            try
            {
                var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!String.IsNullOrEmpty(ip))
                {
                    string[] ipRange = ip.Split(',');
                    ip = ipRange[0];
                }
                else
                    ip = HttpContext.Current.Request.GetUserHostAddressFromCloudflare();
                var CountryCode = MyUtility.getCountry(ip).getCode();
                return CountryCode == "--" ? GlobalConfig.DefaultCountry : CountryCode;
            }
            catch (Exception e) { MyUtility.LogException(e, "Get Country Code Via IP Address Error"); return "US"; }
        }

        public static string GenerateUrlToken(string controllerName, string actionName, NameValueCollection collection, string password)
        {
            string token = "";
            try
            {
                //The salt can be defined global
                string salt = GlobalConfig.UrlTokenSalt;
                //generating the partial url
                string collectionStr = "";
                if (collection != null)
                    collectionStr = collection.ToQueryString();
                string stringToToken = controllerName + "/" + actionName + "/" + collectionStr;

                //Converting the salt in to a byte array
                byte[] saltValueBytes = System.Text.Encoding.ASCII.GetBytes(salt);
                //Encrypt the salt bytes with the password
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, saltValueBytes);
                //get the key bytes from the above process
                byte[] secretKey = key.GetBytes(16);
                //generate the hash
                HMACSHA1 tokenHash = new HMACSHA1(secretKey);
                tokenHash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(stringToToken));
                //convert the hash to a base64string
                token = Convert.ToBase64String(tokenHash.Hash);
            }
            catch (Exception) { }
            return token;
        }

        public static bool ValidateUrlToken(NameValueCollection collection, string urltoken, string controllerName, string actionName)
        {
            string token = "";
            try
            {
                //This method employs a common business logic for all urls, but only the parameters are different
                //It helps the url password protected
                token = MyUtility.GenerateUrlToken(controllerName, actionName, collection, GlobalConfig.UrlTokenPassword);
            }
            catch (Exception) { }
            //The url token is cross checked here to ensure that url parameters are not forged
            return String.Compare(token, urltoken, true) == 0;
        }

        public static string FormatDuration(string durationType, int duration, bool removeOne = false)
        {
            string result = String.Empty;
            switch (durationType)
            {
                case "d":
                    if (duration < 3)
                        result = String.Format("{0} hours", duration * 24);
                    else
                        result = duration > 1 ? String.Format("{0} days", duration) : (removeOne ? "day" : "1 day");
                    break;
                case "m": result = duration > 1 ? String.Format("{0} months", duration) : (removeOne ? "month" : "1 month"); break;
                case "y": result = duration > 1 ? String.Format("{0} years", duration) : (removeOne ? "year" : "1 year"); break;
                default: break;
            }
            return result;
        }

        public static bool IsAlertBoxEnabledInThisUrl()
        {
            if (GlobalConfig.IsAlertBoxEnabled)
            {
                try
                {
                    if (!HttpContext.Current.Request.Cookies.AllKeys.Contains("alertbox"))
                    {
                        var listOfUrls = GlobalConfig.AlertBoxEnabledUrlList.Split(',');
                        if (HttpContext.Current.Request.Url != null)
                            return listOfUrls.Contains(HttpContext.Current.Request.Url.PathAndQuery, StringComparer.OrdinalIgnoreCase);
                    }
                }
                catch (Exception) { }
            }
            return false;
        }

        public static string HashMD5(string toHash, string secret)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(secret + toHash);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GetAlertBoxUrl(string CountryCode)
        {
            string uri = GlobalConfig.AlertBoxURL;
            var registDt = DateTime.Now;

            try
            {
                if (String.IsNullOrEmpty(CountryCode))
                    CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                switch (CountryCode)
                {
                    case "DE":
                        if (GlobalConfig.TfcTvFree1StartDate < registDt && GlobalConfig.TfcTvFree1EndDate > registDt)
                            uri = GlobalConfig.AlertBoxForGermanyUrl;
                        break;
                    default:
                        if (GlobalConfig.TfcTvFree2StartDate < registDt && GlobalConfig.TfcTvFree2EndDate > registDt)
                        {
                            var countryList = GlobalConfig.TfcTvFree2CountryWhiteList.Split(',');
                            if (countryList.Contains(CountryCode))
                                uri = GlobalConfig.AlertBoxForItalyTaiwanUrl;
                        }
                        break;
                }
            }
            catch (Exception) { }
            return uri;
        }

        public static string RemoveNonAlphaNumericCharacters(string str)
        {
            try
            {
                string pattern = @"[^\w\s\-,]*";
                return Regex.Replace(str, pattern, "");
            }
            catch (Exception) { }
            return str;
        }

        public static Maxmind.Location GetLocationViaIpAddressWithoutProxy()
        {
            try
            {
                var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!String.IsNullOrEmpty(ip))
                {
                    string[] ipRange = ip.Split(',');
                    ip = ipRange[0];
                }
                else
                    ip = HttpContext.Current.Request.UserHostAddress;
                ip = HttpContext.Current.Request.GetUserHostAddressFromCloudflare();
                return MyUtility.getLocation(ip);
            }
            catch (Exception)
            {
                return new Location()
                {
                    city = "Los Angeles",
                    regionName = "CA",
                    countryCode = "US"
                };
            }
        }

        public static string DictionaryToQueryString(Dictionary<string, string> collection)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var x in collection)
                {
                    sb.Append(x.Key);
                    sb.Append("=");
                    sb.Append(x.Value);
                    sb.Append("&");
                }
                return sb.ToString(0, sb.Length - 1);
            }
            catch (Exception) { }
            return String.Empty;
        }


        public static List<Int32> getIncludedShowIDsForMenu(int categoryId)
        {
            List<Int32> ids = new List<Int32>();
            try
            {
                var categories = GlobalConfig.MenuExCategory.Split('|');
                foreach (string cats in categories)
                {
                    var select = cats.Split(':');
                    if (select[0] != null)
                    {
                        if (Convert.ToInt16(select[0]) == categoryId)
                        {
                            var showids = select[1].Split(',');
                            foreach (string sids in showids) { ids.Add(Convert.ToInt16(sids)); }
                        }
                    }
                }
            }

            catch (Exception) { ids.Clear(); }
            return ids;
        }

        public static readonly Regex trimmer = new Regex(@"\s\s+", RegexOptions.Compiled);


        public static Html5CapableObj IsDeviceHtml5Capable(HttpRequestBase request, int EpisodeId)
        {
            var obj = new Html5CapableObj() { playbackUri = String.Format("/Ajax/GetMedia/{0}", EpisodeId), IsMobileDeviceHtml5Capable = false };
            try
            {
                if (GlobalConfig.IsFlowHTML5PlayerEnabled)
                {
                    if (request.Browser.IsMobileDevice)
                    {
                        //var wap_profile_header = request.ServerVariables["HTTP_X_WAP_PROFILE"];
                        //var wap_exist = request.ServerVariables["HTTP_ACCEPT"].ToLower().Contains("wap");
                        var agent = request.ServerVariables["HTTP_USER_AGENT"];
                        var rPattern = @GlobalConfig.Html5CapableDevicesRegex;
                        var rx = new System.Text.RegularExpressions.Regex(rPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        var match = rx.Match(agent);
                        if (match.Value.IndexOf("android", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var user_android_os = match.Value.Replace("Android", "").Replace(";", "");
                            var check_version = new Version("4.0.0");
                            var agent_version = new Version(user_android_os);
                            var isAllowedMp4Playback = check_version.CompareTo(agent_version);
                            if (!(isAllowedMp4Playback > 0))
                            {
                                obj.playbackUri = String.Format("/Ajax/GetMedia/{0}?p=1", EpisodeId);
                                obj.IsMobileDeviceHtml5Capable = true;
                            }
                        }
                        else if (match.Value.IndexOf("windows phone", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var user_wp_os = match.Value.Replace("Windows Phone OS", "").Replace("Windows Phone", "").Replace(";", "");
                            var check_version = new Version("7.0.0");
                            var agent_version = new Version(user_wp_os);
                            var isAllowedMp4Playback = check_version.CompareTo(agent_version);
                            if (!(isAllowedMp4Playback > 0))
                            {
                                obj.playbackUri = String.Format("/Ajax/GetMedia/{0}?p=1", EpisodeId);
                                obj.IsMobileDeviceHtml5Capable = true;
                            }
                        }
                        else if (match.Value.IndexOf("MSIE", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var matches = rx.Matches(agent);
                            if (matches.Count >= 3)
                            {
                                obj.playbackUri = String.Format("/Ajax/GetMedia/{0}?p=1", EpisodeId);
                                obj.IsMobileDeviceHtml5Capable = true;
                            }
                        }
                    }
                    else if (MyUtility.IsSamsungTV(request))
                    {
                        obj.playbackUri = String.Format("/Ajax/GetMedia/{0}?p=1", EpisodeId);
                        obj.IsMobileDeviceHtml5Capable = true;
                    }
                }
            }
            catch (Exception) { }
            return obj;
        }

        public static bool IsAndroid(HttpRequestBase request)
        {
            try
            {
                if (GlobalConfig.IsFlowHTML5PlayerEnabled)
                {
                    if (request.Browser.IsMobileDevice)
                    {
                        var agent = request.ServerVariables["HTTP_USER_AGENT"];
                        var rPattern = @GlobalConfig.Html5CapableDevicesRegex;
                        var rx = new System.Text.RegularExpressions.Regex(rPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        var match = rx.Match(agent);
                        return (match.Value.IndexOf("android", StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                }
            }
            catch (Exception) { }
            return false;
        }

        public static string EncodeJsString(string s)
        {
            return s.Replace("'", @"\'").Replace(@"""", @"\""");
        }
        public static void SetOptimizelyCookie(IPTV2Entities context)
        {
            try
            {
                var registDt = DateTime.Now;
                HttpCookie cookie = new HttpCookie("optmzl");
                if (HttpContext.Current.User.Identity.IsAuthenticated) //logged in
                {
                    var UserId = new Guid(HttpContext.Current.User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                    if (user != null)
                    {
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        if (user.IsFirstTimeSubscriber(offering)) //free trial
                        {
                            if (user.PackageEntitlements.Count(e => e.EndDate > registDt) > 0) //still on free trial
                                cookie.Value = "3";
                            else // no more free trial
                                cookie.Value = "2";
                        }
                        else //not on free trial
                        {
                            if (user.Entitlements.Count(e => e.EndDate > registDt) > 0) //has subscription
                                cookie.Value = "4";
                            else //churner
                                cookie.Value = "5";
                        }
                    }
                }
                else //not logged in
                    cookie.Value = "1";

                cookie.Expires.AddDays(5);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            catch (Exception) { }
        }

        public static string GetCurrentIpWithoutProxy()
        {
            return HttpContext.Current.Request.GetUserHostAddressFromCloudflare();
            try
            {
                var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!String.IsNullOrEmpty(ip))
                {
                    string[] ipRange = ip.Split(',');
                    ip = ipRange[0];
                }
                else
                    ip = HttpContext.Current.Request.UserHostAddress;
                return ip;
            }
            catch (Exception e) { MyUtility.LogException(e, "Get Country Code Via IP Address Error"); return "US"; }
        }

        public static bool IsWhiteListed(string email)
        {
            try
            {
                bool isIpWhiteListed = false;
                bool isEmailWhiteListed = false;
                try
                {
                    var currentIp = MyUtility.GetCurrentIpWithoutProxy();
                    string ip = GlobalConfig.IpWhiteList;
                    string[] IpAddresses = ip.Split(';');
                    isIpWhiteListed = IpAddresses.Contains(currentIp);
                }
                catch (Exception) { }
                try
                {
                    if (!String.IsNullOrEmpty(email))
                    {
                        var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                        isEmailWhiteListed = allowedEmails.Contains(email.ToLower());
                    }
                }
                catch (Exception) { }
                return isIpWhiteListed || isEmailWhiteListed;
            }
            catch (Exception) { }
            return false;

        }
        public static void SendConfirmationEmailAir(IPTV2Entities context, User user)
        {
            try
            {
                var template = GetUrlContent(GlobalConfig.AirRegistrationCompleteTemplateUrl);
                var htmlBody = template;
                SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, String.Format("Hinihintay ka na ng mga Kapamilya stars, {0}", user.FirstName), htmlBody, MailType.HtmlOnly, String.Empty);
            }
            catch (Exception) { }
        }

        public static string FormatNumberCurrency(decimal value, Currency currency)
        {
            try
            {
                if (currency.IsLeft)
                    return String.Format("{0}{1}", currency.Symbol, value.ToString("F"));
                else
                    return String.Format("{0}{1}", value.ToString("F"), currency.Symbol);
            }
            catch (Exception) { }
            return String.Empty;
        }

        public static Mp4CapableObj CheckIfMp4Compatible(HttpRequestBase request, int EpisodeId, string maximumAndroidVersionForMp4Playback = "4.0.0")
        {
            var obj = new Mp4CapableObj() { playbackUri = String.Format("/Ajax/GetMedia/{0}", EpisodeId), UseMp4ForPlayback = false };
            try
            {
                //var wap_profile_header = request.ServerVariables["HTTP_X_WAP_PROFILE"];
                //var wap_exist = request.ServerVariables["HTTP_ACCEPT"].ToLower().Contains("wap");
                var agent = request.ServerVariables["HTTP_USER_AGENT"];
                var rPattern = @GlobalConfig.Html5CapableDevicesRegex;
                var rx = new System.Text.RegularExpressions.Regex(rPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var match = rx.Match(agent);

                if (request.Browser.IsMobileDevice)
                {
                    if (match.Value.IndexOf("android", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var user_android_os = match.Value.Replace("Android", "").Replace(";", "");
                        var check_version = new Version(maximumAndroidVersionForMp4Playback);
                        var agent_version = new Version(user_android_os);
                        var isAllowedMp4Playback = check_version.CompareTo(agent_version);
                        if (isAllowedMp4Playback > 0)
                        {
                            obj.playbackUri = String.Format("/Ajax/GetMedia/{0}?p=1", EpisodeId);
                            obj.UseMp4ForPlayback = true;
                        }
                    }
                    else if (match.Value.IndexOf("windows phone", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var user_wp_os = match.Value.Replace("Windows Phone OS", "").Replace("Windows Phone", "").Replace(";", "");
                        var check_version = new Version("7.0.0");
                        var agent_version = new Version(user_wp_os);
                        var isAllowedMp4Playback = check_version.CompareTo(agent_version);
                        if (isAllowedMp4Playback > 0)
                        {
                            obj.playbackUri = String.Format("/Ajax/GetMedia/{0}?p=1", EpisodeId);
                            obj.UseMp4ForPlayback = true;
                        }
                    }
                    else if (match.Value.IndexOf("MSIE", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var matches = rx.Matches(agent);
                        if (matches.Count >= 3)
                        {
                            obj.playbackUri = String.Format("/Ajax/GetMedia/{0}?p=1", EpisodeId);
                            obj.UseMp4ForPlayback = true;
                        }
                    }
                }
                else if (!request.Browser.IsMobileDevice)
                {
                    if (match.Value.IndexOf("safari", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        obj.playbackUri = String.Format("/Ajax/GetMedia/{0}?p=1", EpisodeId);
                        obj.UseMp4ForPlayback = true;
                    }
                }
            }
            catch (Exception) { }
            return obj;
        }

        public static string GetSHA1(string value)
        {
            try
            {
                SHA1 algorithm = SHA1.Create();
                byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                string sh1 = "";
                for (int i = 0; i < data.Length; i++)
                {
                    sh1 += data[i].ToString("x2").ToUpperInvariant();
                }
                return sh1;
            }
            catch (Exception) { }
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(value, GlobalConfig.SHA1Encryption);
        }

        public static Maxmind.Location GetLocationBasedOnIpAddress(string ip)
        {
            try
            {
                if (String.IsNullOrEmpty(ip))
                    return MyUtility.getLocation(MyUtility.GetClientIpAddress());
                return MyUtility.getLocation(ip);
            }
            catch (Exception)
            {
                return new Location()
                {
                    city = "Los Angeles",
                    regionName = "CA",
                    region = "CA",
                    countryCode = "US"
                };
            }
        }

        public static string getITEError(ITEResponseError errorCode)
        {
            string error = "";
            switch (errorCode)
            {
                case ITEResponseError.SUCCESS: error = "0,Success,{0},{1}"; break;
                case ITEResponseError.IDTYPE_NOT_DEFINED: //error = "IDType is not defined."; break;
                case ITEResponseError.IDTYPE_SHOULD_BE_NUMERIC: //error = "IDType should be numeric."; break;
                case ITEResponseError.IDTYPE_LENGTH_SHOULD_BE_1: //error = "IDType length should be 1."; break;
                case ITEResponseError.ACCOUNTANI_SHOULD_BE_DEFINED: //error = "Account number is not defined."; break;
                case ITEResponseError.ACCOUNTANI_SHOULD_BE_NUMERIC: //error = "Account number should be numeric."; break;
                case ITEResponseError.ACCOUNTANI_LENGTH_SHOULD_BE_10: //error = "Account number length should be 10."; break;IPTV2Entities context = null
                case ITEResponseError.ACCOUNTANI_NOT_FOUND: error = "Your phone number is invalid. 4G Customers, please enter your assigned phone number as 671nnnxxxx or 670nnnxxxx. DSL customer, please enter your DSL number as 194nnnxxxx. You may also contact the IT&E 24-Hour Care Center Team @ 671/922-4483 or 670/682-4483 for further assistance. Thank you."; break;
                case ITEResponseError.VALIDATIONCODE_NOT_DEFINED: //error = "Validation code is not defined."; break;
                case ITEResponseError.VALIDAITONCODE_LENGTH_SHOULD_BE_10: //error = "Validation code length should be 10."; break;
                case ITEResponseError.ACTIVATION_CODE_MISMTCH: error = "Your activation code is invalid. To receive a copy your activation code: 4G Customers, please text TFC CODE to 3282. DSL Customers, please connect to the IT&E Online Billing Site. Or call the IT&E 24-Hour Care Center Team @ 671/922-4483 or 670/682-4483 for further assistance. Thank you."; break;
                case ITEResponseError.TFCTVUSERID_NOT_DEFINED: //error = "TFC.tv UserId is not defined."; break;
                case ITEResponseError.TFCTVUSERID_IS_BLANK: error = "Sorry, we are unable to activate your phone number and TFC.tv code at this time. Please call the IT&E 24-Hour Care Center Team @ 671/922-4483 or 670/682-4483 for further assistance. Thank you."; break;
                case ITEResponseError.REGISTRATION_REQUIRED: error = "Please register or log in."; break;
                case ITEResponseError.ACCOUNTANI_ALREADY_USED: error = "Your activation code and Phone number is already in use. To receive a copy your activation code: 4G Customers, please text TFC CODE to 3282. DSL Customers, please connect to the IT&E Online Billing Site. Or call the IT&E 24-Hour Care Center Team @ 671/922-4483 or 670/682-4483 for further assistance. Thank you."; break;
                case ITEResponseError.TFCTV_ACCOUNT_ALREADY_ACTIVATED: error = "Your TFC.tv account is already activated and linked to your IT&E account. If you need further assistance, please call the IT&E 24-Hour Care Center Team @ 671/922-4483 or 670/682-4483. Thank you."; break;
                case ITEResponseError.VALIDATION_SUCCESS: error = "Your TFC.tv subscription is successfully activated.  Thank you for subscribing to TFC.tv and IT&E."; break;
                case ITEResponseError.IP_NOT_ALLOWED: error = "Ip address is not allowed."; break;
                case ITEResponseError.MISSING_PARAMETERS: error = "Missing parameters"; break;
                default:
                    error = "The system encounted an unspecified error. Please contact Customer Support.";
                    break;
            }

            return error;
        }

        public static bool CheckIfCategoryIsAllowed(int CategoryId, IPTV2Entities context = null, string ip = "")
        {
            bool IsAllowed = true;
            try
            {
                if (context == null)
                    context = new IPTV2_Model.IPTV2Entities();
                if (String.IsNullOrEmpty(ip))
                    ip = MyUtility.GetClientIpAddress();
                var location = MyUtility.GetLocationBasedOnIpAddress(ip);
                if (location != null)
                {
                    //bool? t = null;
                    //if (String.Compare(location.countryCode, GlobalConfig.DefaultCountry, true) == 0)
                    //    t = context.CheckCategoryIfGeoAllowed(CategoryId, location.countryCode, location.regionName, location.city, location.postalCode).SingleOrDefault();
                    //else
                    var t = context.CheckCategoryIfGeoAllowed(CategoryId, location.countryCode, String.IsNullOrEmpty(location.regionName) ? "OTHERS" : location.regionName, location.city, location.postalCode).SingleOrDefault();
                    if (t != null)
                        IsAllowed = (bool)t;
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return IsAllowed;
        }

        public static bool IsSmartTV(HttpRequestBase req)
        {
            //try { return System.Text.RegularExpressions.Regex.IsMatch(req.UserAgent, "smart-tv|smarttv"); }
            //catch (Exception) { }
            try { return String.Compare(MyUtility.GetDeviceType(req), "tv", true) == 0; }
            catch (Exception) { }
            return false;
        }

        public static bool IsSamsungTV(HttpRequestBase req)
        {
            //try { return System.Text.RegularExpressions.Regex.IsMatch(req.UserAgent, "smart-tv|smarttv"); }
            //catch (Exception) { }
            try { return String.Compare(MyUtility.GetDeviceType(req), "samsungtv", true) == 0; }
            catch (Exception) { }
            return false;
        }

        private static string GetDeviceType(HttpRequestBase req)
        {
            string ret = "";
            try
            {
                string ua = req.UserAgent;
                // Check if user agent is a smart TV - http://goo.gl/FocDk
                if (Regex.IsMatch(ua, @"Tizen|Samsung", RegexOptions.IgnoreCase))
                {
                    ret = "samsungtv";
                }
                else if (Regex.IsMatch(ua, @"GoogleTV|Smart-TV|SmartTV|Internet.TV|NetCast|NETTV|AppleTV|boxee|Kylo|Roku|DLNADOC|CE\-HTML", RegexOptions.IgnoreCase))
                {
                    ret = "tv";
                }
                // Check if user agent is a TV Based Gaming Console
                else if (Regex.IsMatch(ua, "Xbox|PLAYSTATION.3|PLAYSTATION.4|Nintendo|Wii", RegexOptions.IgnoreCase))
                {
                    ret = "tv";
                }
                // Check if user agent is a Tablet
                else if ((Regex.IsMatch(ua, "iP(a|ro)d", RegexOptions.IgnoreCase) || (Regex.IsMatch(ua, "tablet", RegexOptions.IgnoreCase)) && (!Regex.IsMatch(ua, "RX-34", RegexOptions.IgnoreCase)) || (Regex.IsMatch(ua, "FOLIO", RegexOptions.IgnoreCase))))
                {
                    ret = "tablet";
                }
                // Check if user agent is an Android Tablet
                else if ((Regex.IsMatch(ua, "Linux", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, "Android", RegexOptions.IgnoreCase)) && (!Regex.IsMatch(ua, "Fennec|mobi|HTC.Magic|HTCX06HT|Nexus.One|SC-02B|fone.945", RegexOptions.IgnoreCase)))
                {
                    ret = "tablet";
                }
                // Check if user agent is a Kindle or Kindle Fire
                else if ((Regex.IsMatch(ua, "Kindle", RegexOptions.IgnoreCase)) || (Regex.IsMatch(ua, "Mac.OS", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, "Silk", RegexOptions.IgnoreCase)))
                {
                    ret = "tablet";
                }
                // Check if user agent is a pre Android 3.0 Tablet
                else if ((Regex.IsMatch(ua, @"GT-P10|SC-01C|SHW-M180S|SGH-T849|SCH-I800|SHW-M180L|SPH-P100|SGH-I987|zt180|HTC(.Flyer|\\_Flyer)|Sprint.ATP51|ViewPad7|pandigital(sprnova|nova)|Ideos.S7|Dell.Streak.7|Advent.Vega|A101IT|A70BHT|MID7015|Next2|nook", RegexOptions.IgnoreCase)) || (Regex.IsMatch(ua, "MB511", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, "RUTEM", RegexOptions.IgnoreCase)))
                {
                    ret = "tablet";
                }
                // Check if user agent is unique Mobile User Agent
                else if ((Regex.IsMatch(ua, "BOLT|Fennec|Iris|Maemo|Minimo|Mobi|mowser|NetFront|Novarra|Prism|RX-34|Skyfire|Tear|XV6875|XV6975|Google.Wireless.Transcoder", RegexOptions.IgnoreCase)))
                {
                    ret = "mobile";
                }
                // Check if user agent is an odd Opera User Agent - http://goo.gl/nK90K
                else if ((Regex.IsMatch(ua, "Opera", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, "Windows.NT.5", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, @"HTC|Xda|Mini|Vario|SAMSUNG\-GT\-i8000|SAMSUNG\-SGH\-i9", RegexOptions.IgnoreCase)))
                {
                    ret = "mobile";
                }
                // Check if user agent is Windows Desktop
                else if ((Regex.IsMatch(ua, "Windows.(NT|XP|ME|9)")) && (!Regex.IsMatch(ua, "Phone", RegexOptions.IgnoreCase)) || (Regex.IsMatch(ua, "Win(9|.9|NT)", RegexOptions.IgnoreCase)))
                {
                    ret = "desktop";
                }
                // Check if agent is Mac Desktop
                else if ((Regex.IsMatch(ua, "Macintosh|PowerPC", RegexOptions.IgnoreCase)) && (!Regex.IsMatch(ua, "Silk", RegexOptions.IgnoreCase)))
                {
                    ret = "desktop";
                }
                // Check if user agent is a Linux Desktop
                else if ((Regex.IsMatch(ua, "Linux", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, "X11", RegexOptions.IgnoreCase)))
                {
                    ret = "desktop";
                }
                // Check if user agent is a Solaris, SunOS, BSD Desktop
                else if ((Regex.IsMatch(ua, "Solaris|SunOS|BSD", RegexOptions.IgnoreCase)))
                {
                    ret = "desktop";
                }
                // Check if user agent is a Desktop BOT/Crawler/Spider
                else if ((Regex.IsMatch(ua, "Bot|Crawler|Spider|Yahoo|ia_archiver|Covario-IDS|findlinks|DataparkSearch|larbin|Mediapartners-Google|NG-Search|Snappy|Teoma|Jeeves|TinEye", RegexOptions.IgnoreCase)) && (!Regex.IsMatch(ua, "Mobile", RegexOptions.IgnoreCase)))
                {
                    ret = "desktop";
                }
                // Otherwise assume it is a Mobile Device
                else
                {
                    ret = "mobile";
                }
            }
            catch (Exception) { }
            return ret;
        }

        public static bool IsDuplicateSession(User user, HttpCookie authCookie)
        {
            try
            {
                if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
                {
                    if (!MyUtility.IsWhiteListed(user.EMail))
                    {
                        if (!String.IsNullOrEmpty(user.SessionId))
                            return String.Compare(user.SessionId, authCookie.Value, true) != 0;
                    }
                }
            }
            catch (Exception) { }
            return false;
        }

        public static void SendVerificationEmail(IPTV2Entities context, User user, int productId)
        {
            try
            {
                var registDt = System.DateTime.Now;
                string productDesc = string.Empty;
                string premium1Price = string.Empty;
                string premium3Price = string.Empty;
                string prem3Save = string.Empty;
                string prem3SavePercent = string.Empty;
                string premium12Price = string.Empty;
                string prem12Save = string.Empty;
                string prem12SavePercent = string.Empty;
                string offerFontSize = "18px";
                string offerFontSizeMid = "19.8px";
                var product = context.Products.FirstOrDefault(p => p.ProductId == productId && p.StatusId == GlobalConfig.Visible);
                productDesc = product.Description;
                var productprice1 = context.ProductPrices.FirstOrDefault(p => p.CurrencyCode == user.Country.CurrencyCode && p.ProductId == 1);
                if (productprice1 == null)
                {
                    productprice1 = context.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, "USD") == 0 && p.ProductId == 1);
                }
                var productprice3 = context.ProductPrices.FirstOrDefault(p => p.CurrencyCode == user.Country.CurrencyCode && p.ProductId == 3);
                if (productprice3 == null)
                {
                    productprice3 = context.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, "USD") == 0 && p.ProductId == 3);
                }
                var productprice12 = context.ProductPrices.FirstOrDefault(p => p.CurrencyCode == user.Country.CurrencyCode && p.ProductId == 4);
                if (productprice12 == null)
                {
                    productprice12 = context.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, "USD") == 0 && p.ProductId == 12);
                }

                premium1Price = String.Format("{0} {1}", productprice1.CurrencyCode, productprice1.Amount.ToString("#,##0.00"));
                premium3Price = String.Format("{0} {1}", productprice3.CurrencyCode, productprice3.Amount.ToString("#,##0.00"));
                premium12Price = String.Format("{0} {1}", productprice12.CurrencyCode, productprice12.Amount.ToString("#,##0.00"));
                prem3Save = String.Format("{0} {1}", productprice3.CurrencyCode, productprice3.DiscountAmount.ToString("#,##0.00"));
                prem12Save = String.Format("{0} {1}", productprice12.CurrencyCode, productprice12.DiscountAmount.ToString("#,##0.00"));
                if (premium1Price.Length > 10 || premium3Price.Length > 10 || premium12Price.Length > 10 || prem3Save.Length > 10 || prem12Save.Length > 10)
                {
                    offerFontSize = "14px";
                    offerFontSize = "15.8px";
                }
                prem3SavePercent = (productprice3.DiscountPercentage * 100).ToString("0");
                prem12SavePercent = (productprice12.DiscountPercentage * 100).ToString("0");
                string year = System.DateTime.Now.Year.ToString();
                string verification_link = String.Format("{0}/User/Verify?key={1}", GlobalConfig.baseUrl, user.ActivationKey.ToString());
                var template = GetUrlContent(GlobalConfig.VerificationTemplateUrl);
                var htmlBody = template.Replace("[firstname]", user.FirstName).Replace("[email]", user.EMail).Replace("[verification_link]", verification_link).Replace("[year]", registDt.Year.ToString()).Replace("[productDesc]", productDesc).Replace("[baseURL]", GlobalConfig.baseUrl).Replace("[premium1Price]", premium1Price).Replace("[premium3Price]", premium3Price).Replace("[prem3Save]", prem3Save).Replace("[prem3SavePercent]", prem3SavePercent).Replace("[premium12Price]", premium12Price).Replace("[prem12Save]", prem12Save).Replace("[prem12SavePercent]", prem12SavePercent).Replace("[year]", year).Replace("[offerFontSize]", offerFontSize).Replace("[offerFontSizeMid]", offerFontSizeMid);
                SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, "Verify your email", htmlBody, MailType.HtmlOnly, String.Empty);
            }
            catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }
        }

        public static void SendWelcomeEmail(IPTV2Entities context, User user, int productId)
        {
            try
            {

                var registDt = System.DateTime.Now;
                string productDesc = string.Empty;
                string premium1Price = string.Empty;
                string premium3Price = string.Empty;
                string prem3Save = string.Empty;
                string prem3SavePercent = string.Empty;
                string premium12Price = string.Empty;
                string prem12Save = string.Empty;
                string prem12SavePercent = string.Empty;
                string offerFontSize = "18px";
                string offerFontSizeMid = "19.8px";
                var product = context.Products.FirstOrDefault(p => p.ProductId == productId && p.StatusId == GlobalConfig.Visible);
                productDesc = product.Description;
                var productprice1 = context.ProductPrices.FirstOrDefault(p => p.CurrencyCode == user.Country.CurrencyCode && p.ProductId == 1);
                if (productprice1 == null)
                {
                    productprice1 = context.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, GlobalConfig.DefaultCurrency, true) == 0 && p.ProductId == 1);
                }
                var productprice3 = context.ProductPrices.FirstOrDefault(p => p.CurrencyCode == user.Country.CurrencyCode && p.ProductId == 3);
                if (productprice3 == null)
                {
                    productprice3 = context.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, GlobalConfig.DefaultCurrency, true) == 0 && p.ProductId == 3);
                }
                var productprice12 = context.ProductPrices.FirstOrDefault(p => p.CurrencyCode == user.Country.CurrencyCode && p.ProductId == 4);
                if (productprice12 == null)
                {
                    productprice12 = context.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, GlobalConfig.DefaultCurrency, true) == 0 && p.ProductId == 4);
                }

                premium1Price = String.Format("{0} {1}", productprice1.CurrencyCode, productprice1.Amount.ToString("#,##0.00"));
                premium3Price = String.Format("{0} {1}", productprice3.CurrencyCode, productprice3.Amount.ToString("#,##0.00"));
                premium12Price = String.Format("{0} {1}", productprice12.CurrencyCode, productprice12.Amount.ToString("#,##0.00"));
                prem3Save = String.Format("{0} {1}", productprice3.CurrencyCode, productprice3.DiscountAmount.ToString("#,##0.00"));
                prem12Save = String.Format("{0} {1}", productprice12.CurrencyCode, productprice12.DiscountAmount.ToString("#,##0.00"));
                if (premium1Price.Length > 10 || premium3Price.Length > 10 || premium12Price.Length > 10 || prem3Save.Length > 10 || prem12Save.Length > 10)
                {
                    offerFontSize = "14px";
                    offerFontSizeMid = "15.8px";
                }

                prem3SavePercent = (productprice3.DiscountPercentage * 100).ToString("0");
                prem12SavePercent = (productprice12.DiscountPercentage * 100).ToString("0");
                string year = System.DateTime.Now.Year.ToString();
                var enddate = user.EntitlementRequests.FirstOrDefault(e => e.ProductId == productId).EndDate.ToString("MM-dd-yyyy");

                var template = GetUrlContent(GlobalConfig.WelcomeTemplateUrl);
                var htmlBody = template.Replace("[firstname]", user.FirstName).Replace("[email]", user.EMail).Replace("[enddate]", enddate).Replace("[premium1Price]", premium1Price).Replace("[premium3Price]", premium3Price).Replace("[prem3Save]", prem3Save).Replace("[prem3SavePercent]", prem3SavePercent).Replace("[premium12Price]", premium12Price).Replace("[prem12Save]", prem12Save).Replace("[prem12SavePercent]", prem12SavePercent).Replace("[year]", year).Replace("[offerFontSize]", offerFontSize).Replace("[offerFontSizeMid]", offerFontSizeMid);
                SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, "Welcome to TFC.tv", htmlBody, MailType.HtmlOnly, String.Empty);
            }
            catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }
        }

        public static void SendReceiptEmail(string productDesc, User user, string enddate, string subject, string tranid, string trandate, string amount, string currency, string type, string mode, string reference, bool isExtension)
        {
            try
            {
                var registDt = System.DateTime.Now;
                var template = GetUrlContent(GlobalConfig.ReceiptTemplateUrl);
                string year = System.DateTime.Now.Year.ToString();
                string phrase = "Naka-subscribe ka na sa ";
                if (isExtension)
                    phrase = "Na-extend na ang iyong ";
                var htmlBody = template.Replace("[firstname]", user.FirstName).Replace("[email]", user.EMail).Replace("[enddate]", enddate).Replace("[productDesc]", productDesc).Replace("[year]", year).Replace("[tranid]", tranid).Replace("[trandate]", trandate).Replace("[amount]", amount).Replace("[currency]", currency).Replace("[type]", type).Replace("[mode]", mode).Replace("[reference]", reference).Replace("[phrase]", phrase);
                SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, subject, htmlBody, MailType.HtmlOnly, String.Empty);
            }
            catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }
        }
        public static string ReplaceWhiteSpaces(string text, string newChar)
        {
            try
            {
                text = text.Trim();
                Regex rx = new Regex(@"\s+");
                text = rx.Replace(text, " ");
                rx = new Regex(@"\s");
                text = rx.Replace(text, newChar);
                return text.ToLower();
            }
            catch (Exception) { }
            return String.Empty;

        }

        public static bool IsDeviceHtml5Capable(HttpRequestBase request)
        {
            bool IsDeviceHtml5Capable = false;
            try
            {
                if (request.Browser.IsMobileDevice || true)
                {
                    //var wap_profile_header = request.ServerVariables["HTTP_X_WAP_PROFILE"];
                    //var wap_exist = request.ServerVariables["HTTP_ACCEPT"].ToLower().Contains("wap");
                    var agent = request.ServerVariables["HTTP_USER_AGENT"];
                    var rPattern = @GlobalConfig.Html5CapableDevicesRegex;
                    var rx = new System.Text.RegularExpressions.Regex(rPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    var match = rx.Match(agent);
                    if (match.Value.IndexOf("android", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var user_android_os = Regex.Replace(match.Value, "Android", "", RegexOptions.IgnoreCase).Replace(";", ""); //match.Value.Replace("Android", "").Replace("android", "").Replace(";", "");
                        var check_version = new Version("4.0.0");
                        var agent_version = new Version(user_android_os);
                        var isAllowedMp4Playback = check_version.CompareTo(agent_version);
                        if (!(isAllowedMp4Playback > 0))
                        {
                            IsDeviceHtml5Capable = true;
                        }
                    }
                    else if (match.Value.IndexOf("windows phone", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var user_wp_os = match.Value.Replace("Windows Phone OS", "").Replace("Windows Phone", "").Replace(";", "");
                        var check_version = new Version("7.0.0");
                        var agent_version = new Version(user_wp_os);
                        var isAllowedMp4Playback = check_version.CompareTo(agent_version);
                        if (!(isAllowedMp4Playback > 0))
                        {
                            IsDeviceHtml5Capable = true;
                        }
                    }
                    else if (match.Value.IndexOf("MSIE", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var matches = rx.Matches(agent);
                        if (matches.Count >= 3)
                        {
                            IsDeviceHtml5Capable = true;
                        }
                    }
                    else if (MyUtility.IsSamsungTV(request))
                    {
                        IsDeviceHtml5Capable = true;
                    }
                }
                else if (MyUtility.IsSamsungTV(request))
                {
                    IsDeviceHtml5Capable = true;
                }
            }
            catch (Exception) { }
            return IsDeviceHtml5Capable;
        }

        public static VideoApiPlaybackObj MakePlaybackApiRequest(int id)
        {
            VideoApiPlaybackObj obj = null;
            try
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var uri = String.Format("{0}/{1}/user/{2}", GlobalConfig.TFCtvApiVideoPlaybackUri, id, HttpContext.Current.User.Identity.Name);
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                    req.ContentType = "application/json";
                    WebResponse response = req.GetResponse();
                    Stream dataStream = response.GetResponseStream();
                    var statusCode = ((HttpWebResponse)response).StatusCode;
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                    if (!String.IsNullOrEmpty(responseFromServer) && statusCode == HttpStatusCode.OK)
                        obj = Newtonsoft.Json.JsonConvert.DeserializeObject<VideoApiPlaybackObj>(responseFromServer);
                }
            }
            catch (Exception) { }
            return obj;
        }

        public static string GetDaxSlug(string slug)
        {
            try
            {
                slug = slug.Trim();
                Regex rx = new Regex(@"\/+");
                slug = rx.Replace(slug, " ");
                rx = new Regex(@"[^a-zA-Z0-9\-\s|]");
                slug = rx.Replace(slug, "");
                rx = new Regex(@"\s+");
                slug = rx.Replace(slug, " ");
                rx = new Regex(@"\s");
                slug = rx.Replace(slug, "-");
                slug = slug.Replace("|", ":");
                return slug.ToLower();
            }
            catch (Exception) { }
            return String.Empty;

        }

        public static RecommendedItemObj GetRecommendedItems(string uid)
        {
            RecommendedItemObj obj = null;
            try
            {
                var uri = String.Format("{0}/user-{1}", GlobalConfig.TfctvRecommendedItemsApi, uid.ToLower());
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                req.ContentType = "application/json";
                WebResponse response = req.GetResponse();
                Stream dataStream = response.GetResponseStream();
                var statusCode = ((HttpWebResponse)response).StatusCode;
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                if (!String.IsNullOrEmpty(responseFromServer) && statusCode == HttpStatusCode.OK)
                {
                    responseFromServer = Regex.Replace(responseFromServer, "(@|#)([a-zA-Z0-9]+)", "$2");
                    obj = Newtonsoft.Json.JsonConvert.DeserializeObject<RecommendedItemObj>(responseFromServer, new JsonSerializerSettings { Error = HandleDeserializationError });
                }
            }
            catch (Exception ex) { obj.errorMessage = ex.Message; }
            return obj;
        }

        public static RecommendedShowObj GetRecommendedShows(string uid, string tag)
        {
            RecommendedShowObj obj = null;
            try
            {
                var uri = String.Format("{0}/user-{1}/recommended?tags={2}", GlobalConfig.TfctvRecommendedItemsApi, uid.ToLower(), tag);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                req.ContentType = "application/json";
                WebResponse response = req.GetResponse();
                Stream dataStream = response.GetResponseStream();
                var statusCode = ((HttpWebResponse)response).StatusCode;
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                if (!String.IsNullOrEmpty(responseFromServer) && statusCode == HttpStatusCode.OK)
                {
                    responseFromServer = Regex.Replace(responseFromServer, "(@|#)([a-zA-Z0-9]+)", "$2");
                    obj = Newtonsoft.Json.JsonConvert.DeserializeObject<RecommendedShowObj>(responseFromServer, new JsonSerializerSettings { Error = HandleDeserializationError });
                }
            }
            catch (Exception ex) { obj.errorMessage = ex.Message; }
            return obj;
        }

        public static void PostCelebrityReaction(int ReactionTypeId, int CelebrityId, Guid UserId)
        {
            var registDt = DateTime.Now;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(GlobalConfig.TFCtvApiVideoPlaybackUri);
                req.Method = "POST";
                req.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    string jsonString = String.Empty;
                    var obj = new TfcTvApiCelebrityReactionPostObj()
                    {
                        ReactionTypeId = ReactionTypeId,
                        CelebrityId = CelebrityId,
                        UserId = UserId,
                        DateTime = registDt.ToString("o"),
                        Rating = 0
                    };
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                    streamWriter.Write(jsonString);
                }

                WebResponse response = req.GetResponse();
                var statusDescription = ((HttpWebResponse)response).StatusDescription;
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (Exception) { }
        }

        public static void PostCategoryReaction(int ReactionTypeId, int CategoryId, Guid UserId)
        {
            var registDt = DateTime.Now;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(GlobalConfig.TFCtvApiVideoPlaybackUri);
                req.Method = "POST";
                req.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    string jsonString = String.Empty;
                    var obj = new TfcTvApiCategoryReactionPostObj()
                    {
                        ReactionTypeId = ReactionTypeId,
                        CategoryId = CategoryId,
                        UserId = UserId,
                        DateTime = registDt.ToString("o"),
                        Rating = 0
                    };
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                    streamWriter.Write(jsonString);
                }

                WebResponse response = req.GetResponse();
                var statusDescription = ((HttpWebResponse)response).StatusDescription;
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (Exception) { }
        }

        public static void PostEpisodeReaction(int ReactionTypeId, int CategoryId, int EpisodeId, Guid UserId)
        {
            var registDt = DateTime.Now;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(GlobalConfig.TFCtvApiVideoPlaybackUri);
                req.Method = "POST";
                req.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    string jsonString = String.Empty;
                    var obj = new TfcTvApiEpisodeReactionPostObj()
                    {
                        ReactionTypeId = ReactionTypeId,
                        EpisodeId = EpisodeId,
                        CategoryId = CategoryId,
                        UserId = UserId,
                        DateTime = registDt.ToString("o"),
                        Rating = 0
                    };
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                    streamWriter.Write(jsonString);
                }

                WebResponse response = req.GetResponse();
                var statusDescription = ((HttpWebResponse)response).StatusDescription;
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (Exception) { }
        }

        private static void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }

        public static void GetRecommendedEpisodes()
        {

            var obj = GetRecommendedItems(HttpContext.Current.User.Identity.Name);
        }

        public static string GetAirPlusPpcErrorMessages(Ppc.ErrorCode code)
        {
            string error = String.Empty;
            switch (code)
            {
                case Ppc.ErrorCode.InvalidSerialNumber: error = "The prepaid card/e-PIN does not exist."; break;
                case Ppc.ErrorCode.InvalidPin: error = "The serial and PIN do not match."; break;
                case Ppc.ErrorCode.PpcAlreadyUsed: error = "The prepaid card/e-PIN is already used."; break;
                case Ppc.ErrorCode.InactivePpc: error = "The prepaid card/e-PIN is inactive or expired."; break;
                case Ppc.ErrorCode.InvalidCountry:
                case Ppc.ErrorCode.InvalidCurrency: error = "The prepaid card/e-PIN is not allowed in your country."; break;
                case Ppc.ErrorCode.NotASubscriptionPpc: error = "The prepaid card/e-PIN is not allowed for subscription."; break;
                case Ppc.ErrorCode.PpcHasNoMatchingProductPrice:
                case Ppc.ErrorCode.PpcDoesNotMatchSubscriptionProduct: error = "The prepaid card/e-PIN is not valid for this product."; break;
                case Ppc.ErrorCode.PpcPriceDoesNotMatchProductPrice: error = "The prepaid card/e-PIN does not have enough credits to buy this product."; break;
                case Ppc.ErrorCode.NotAReloadPpc: error = "The prepaid card/e-PIN can not be used for e-wallet reloading."; break;
                case Ppc.ErrorCode.HasConsumedTrialPpc: error = "You have already consumed a trial card/e-PIN."; break;
                case Ppc.ErrorCode.IsExpiredPpc: error = "The prepaid card/e-PIN is already expired."; break;
                case Ppc.ErrorCode.UserWalletIsNotActive:
                case Ppc.ErrorCode.UserWalletNotFound: error = "User's wallet does not exist."; break;
                case Ppc.ErrorCode.PrefixOfStartAndEndingSerialDoNotMatch:
                case Ppc.ErrorCode.StartSerialShouldBeLessThanEndSerial: error = "The serial number does not exist."; break;
                default: error = "The system encounted an error."; break;
            }
            return error;
        }
    }

}
