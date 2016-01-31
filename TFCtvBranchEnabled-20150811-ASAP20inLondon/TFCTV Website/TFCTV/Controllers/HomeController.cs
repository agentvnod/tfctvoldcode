using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using com.Akamai.EdgeAuth;
using DevTrends.MvcDonutCaching;
using Gigya.Socialize.SDK;
using IPTV2_Model;
using MvcSiteMapProvider;
using MvcSiteMapProvider.Web;
using Newtonsoft.Json;
using TFCTV.Helpers;
using TFCTV.Models;
using System.Web.Security;
using StackExchange.Profiling;

namespace TFCTV.Controllers
{
    public class HomeController : Controller
    {
        [RequireHttp]
        public ActionResult Index()
        {
            if (!MyUtility.isUserLoggedIn())
            {
                if (GlobalConfig.IsWelcomePageOn)
                    if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("SplashVisited"))
                        return RedirectToAction("Main");
            }
            else
            {
                if (GlobalConfig.IsWelcomePageOn)
                    if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("SplashVisited"))
                    {
                        this.ControllerContext.HttpContext.Response.Cookies["SplashVisited"].Value = "true";
                        this.ControllerContext.HttpContext.Response.Cookies["SplashVisited"].Expires = DateTime.Now.AddYears(5);
                    }
                    else
                    {
                        HttpCookie cookie = new HttpCookie("SplashVisited");
                        cookie.Value = "true";
                        cookie.Expires = DateTime.Now.AddYears(5);
                        this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                    }

            }

            //ReauthenticateUser();

            string dropdownListId = GlobalConfig.LatestEpisodeDropdownListIds;
            var dropdownListIds = MyUtility.StringToIntList(dropdownListId);

            var context = new IPTV2Entities();
            var features = context.Features.Where(f => dropdownListIds.Contains(f.FeatureId) && f.StatusId == GlobalConfig.Visible).ToList();
            //var fs = features.AsEnumerable().Select(f => new Feature { FeatureId = f.FeatureId, Description = f.Description }).ToList();
            //Dictionary<int, Feature> d = fs.ToDictionary(x => x.FeatureId);
            //var ordered = dropdownListIds.Select(i => d[i]).ToList();
            ViewBag.LatestEpisodeDropdownListIds = features;// ordered;

            ViewBag.IsAlreadyTVEverywhere = false;
            bool HasActiveSubscriptions = false;
            bool IsFreeTrialUser = false;

            var CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
            string EMail = String.Empty;
            bool IsWhiteListed = false;

            //Check if we will show MayWeather Banner
            bool IsPacMayBannerEnabledInHomepage = false;
            if (GlobalConfig.IsStaticPacMayForMECarouselEnabled)
            {
                var MEPacMayCountryCodeList = GlobalConfig.MEPacMayAllowedCountryCodes.Split(',');
                if (MEPacMayCountryCodeList.Contains(CountryCode))
                    IsPacMayBannerEnabledInHomepage = true;
                ViewBag.IsPacMayBannerEnabledInHomepage = IsPacMayBannerEnabledInHomepage;
            }



            if (MyUtility.isUserLoggedIn())
            {
                var userId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    if (user.IsTVEverywhere == true)
                        ViewBag.IsAlreadyTVEverywhere = true;
                    ViewBag.FirstName = user.FirstName;
                    HasActiveSubscriptions = user.HasActiveSubscriptions();
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    IsFreeTrialUser = user.IsFirstTimeSubscriber(offering);
                    CountryCode = user.CountryCode;
                    IsWhiteListed = MyUtility.IsWhiteListed(user.EMail);
                }
            }

            if (!IsWhiteListed)
            {
                //check for project air promo
                var registDt = DateTime.Now;
                var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.ProjectAirPromoId && p.StartDate < registDt && p.EndDate > registDt && p.StatusId == GlobalConfig.Visible);
                if (promo != null)
                {
                    bool isFirstTimeVisit = true;
                    try
                    {
                        if (Request.UrlReferrer != null)
                            isFirstTimeVisit = false;
                    }
                    catch (Exception) { }
                    var CountryCodeIP = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                    bool isViewable = false;
                    ViewBag.ShowAirLink = false;
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.ProjectAirCategoryId);
                    if (category != null)
                        if (category is Show)
                            isViewable = ContextHelper.IsCategoryViewableInUserCountry((Show)category, CountryCodeIP);

                    if (IsWhiteListed)
                        isViewable = true;

                    if (isViewable)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            if (isFirstTimeVisit)
                            {
                                if (isViewable)
                                {
                                    if (IsFreeTrialUser && !HasActiveSubscriptions)
                                        return Redirect("/WatchNow");
                                    else if (!IsFreeTrialUser && !HasActiveSubscriptions)
                                        return Redirect("/WatchNow");
                                }
                                //if ((IsFreeTrialUser && isViewable) || (!HasActiveSubscriptions && isViewable))
                                //    return Redirect("/WatchNow");
                                //return RedirectToAction("Index", "Air");
                            }
                            else
                            {
                                if (isViewable)
                                {
                                    if (IsFreeTrialUser && !HasActiveSubscriptions)
                                        ViewBag.ShowAirLink = true;
                                    else if (!IsFreeTrialUser && !HasActiveSubscriptions)
                                        ViewBag.ShowAirLink = true;
                                }
                                //if ((IsFreeTrialUser && isViewable) || (!HasActiveSubscriptions && isViewable))
                                //    ViewBag.ShowAirLink = true;
                            }
                        }
                        else
                            if (isViewable && isFirstTimeVisit)
                                return Redirect("/WatchNow");//return RedirectToAction("Index", "Air");
                            else if (isViewable)
                                ViewBag.ShowAirLink = true;
                        try
                        {

                            if (ViewBag.ShowAirLink || IsWhiteListed)
                            {
                                if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("air"))
                                {
                                    HttpCookie airCookie = new HttpCookie("air");
                                    airCookie.Value = "true";
                                    airCookie.Expires = DateTime.Now.AddYears(5);
                                    Response.Cookies.Add(airCookie);
                                }
                            }
                            else
                            {
                                if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("air"))
                                {
                                    HttpCookie airCookie = new HttpCookie("air");
                                    airCookie.Expires = DateTime.Now.AddDays(-1);
                                    Response.Cookies.Add(airCookie);
                                }
                            }

                        }
                        catch (Exception) { }
                    }

                }
            }

            //Get premium 1 month price for user
            try
            {
                var product = context.Products.FirstOrDefault(c => c.ProductId == GlobalConfig.DefaultProductIdOffering); //1 month premium
                if (product != null)
                {
                    var country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);
                    if (country == null)
                        country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, GlobalConfig.DefaultCountry, true) == 0);
                    var product_price = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == country.CurrencyCode);
                    if (product_price == null)
                        product_price = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency);
                    //ViewBag.DefaultProductOfferingPrice = String.Format("{0} {1}", product_price.CurrencyCode, product_price.Amount.ToString("F"));
                    ViewBag.DefaultProductOfferingPrice = MyUtility.FormatNumberCurrency(product_price.Amount, product_price.Currency);
                }
            }
            catch (Exception) { }

            ViewBag.HasActiveSubscriptions = HasActiveSubscriptions;
            ViewBag.IsFreeTrialUser = IsFreeTrialUser;
            MyUtility.SetOptimizelyCookie(context);
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("IndexRichMedia");
            return View();
        }

        public string GetDate()
        {
            return DateTime.Now.ToString();
        }

        public string GetDate1(int d, int h, int m, int s)
        {
            return DateTime.Now.Ceil(new TimeSpan(d, h, m, s)).AddSeconds(-1).ToString();
        }
        //public ActionResult About(string id)
        //{
        //    Dictionary<string, string> collection = new Dictionary<string, string>();

        //    collection.Add("uid", "helloworld");
        //    collection.Add("siteUID", "hiii");

        //    string d = GigyaHelpers.buildJson(collection);

        //    string APIKey = "";
        //    string secretKey = "";
        //    string json = @"{  ""url"": ""http://www.google.com""  }";
        //    string data = Json(json).Data.ToString();
        //    ViewBag.Data = data;

        //    GSObject obj = new GSObject(data);

        //    GSRequest gr = new GSRequest(APIKey, secretKey, "socialize.shortenURL", obj);
        //    GSResponse resp = gr.Send();

        //    string err = resp.GetErrorCode().ToString();
        //    GSObject res = resp.GetData();
        //    err = res.ToJsonString();
        //    ViewBag.Data = err;

        //    if (!String.IsNullOrEmpty(id))
        //        ViewBag.UserId = id;
        //    return View();
        //}

        public ActionResult topsecretfunctionthatyoudidntknowthatexisted(string uid)
        {
            Dictionary<string, string> collection = new Dictionary<string, string>();
            collection.Add("uid", uid);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.deleteAccount", GigyaHelpers.buildParameter(collection));
            return Content(res.GetData().ToJsonString(), "application/json");
        }

        public string getUserInfo(string uid)
        {
            Dictionary<string, string> collection = new Dictionary<string, string>();
            collection.Add("uid", uid);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(collection));

            return res.GetData().ToJsonString();
        }

        /*      public string country(string ip)
              {
                  return MyUtility.getCountry(ip).getCode();
              }

              public string location(string ip)
              {
                  return MyUtility.getLocation(ip).countryName;
              }
          */

        //public string GetOnlineShows(int id)
        //{
        //    var context = new IPTV2Entities();
        //    var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
        //    var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);

        //    //Category category = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
        //    Category category = (Category)service.Categories.FirstOrDefault(c => c.CategoryId == GlobalConfig.Entertainment).Category.SubCategories.FirstOrDefault(s => s.CategoryId == id);

        //    SortedSet<int> ss = service.GetAllIptvShowIds("--", category);
        //    StringBuilder sb = new StringBuilder();
        //    foreach (int i in ss)
        //    {
        //        sb.Append(i.ToString());
        //        sb.AppendLine(",");
        //    }

        //    return sb.ToString();
        //}

        //[HttpPost]
        //public ActionResult Search(FormCollection formValues)
        //{
        //    string searchTerms = formValues["search"];

        //    // Check search terms
        //    if (searchTerms != null)
        //    {
        //        // Set search output page
        //        string outUrl = "/Home/Search";

        //        // Build query string
        //        outUrl += "?usterms=" + searchTerms;

        //        // Set page size

        //        // Redirect to search output page
        //        return Redirect(outUrl);
        //    }

        //    return View();
        //}

        //public ActionResult PublishUserAction()
        //{
        //    List<ActionLink> actionlinks = new List<ActionLink>();
        //    actionlinks.Add(new ActionLink() { text = "Click here!", href = "http://www.tfc.tv/User/Register" });
        //    List<MediaItem> mediaItems = new List<MediaItem>();

        //    mediaItems.Add(new MediaItem() { type = "image", src = "http://upload.wikimedia.org/wikipedia/commons/7/72/Crystal_Clear_action_player_play.png", href = "http://www.tfc.tv" });
        //    string gender = GigyaMethods.GetUserInfoByKey(new System.Guid(User.Identity.Name), "gender");

        //    UserAction action = new UserAction()
        //    {
        //        userMessage = String.Format("has added {0} to {1} wishlist", "Premium 10 Days", gender == "m" ? "his" : "her"),
        //        title = "My Wishlist on TFC.tv",
        //        subtitle = String.Format("http://www.tfc.tv/Profile/{0}", new System.Guid(User.Identity.Name)),
        //        linkBack = String.Format("http://www.tfc.tv/Profile/{0}", new System.Guid(User.Identity.Name)),
        //        description = "Build your own wishlist and share with other TFC.tv users the products that you want.",
        //        actionLinks = actionlinks,
        //        mediaItems = mediaItems
        //    };

        //    string ret = GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "external");
        //    return Content(ret, "application/json");
        //}

        //public ActionResult removeConnection(string provider)
        //{
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    collection.Add("provider", provider);
        //    collection.Add("uid", User.Identity.Name);
        //    GSResponse res = GigyaHelpers.createAndSendRequest("socialize.removeConnection", GigyaHelpers.buildParameter(collection));
        //    return Content(res.GetData().ToJsonString(), "application/json");
        //}

        //public ActionResult Search(string query)
        //{
        //    var googleCustomSiteSearchClient = new GoogleCustomSiteSearchClient();
        //    var model = googleCustomSiteSearchClient.RunSearch(query, null);
        //    if (model == null)
        //        return RedirectToAction("Results", "Search", new { query = query });
        //    return View("TestSearch", model);
        //}

        //[OutputCache(NoStore = true, Duration = 0)]
        //public ActionResult TestVideo(int? id, string hls)
        //{
        //    //if (String.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))
        //    //    return RedirectToAction("Index", "Home");
        //    if (id == null)
        //        id = 21471;
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    ErrorCodes errorCode = ErrorCodes.UnknownError;
        //    string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
        //    collection = MyUtility.setError(errorCode, errorMessage);

        //    var context = new IPTV2Entities();
        //    Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.OnlineStatusId == GlobalConfig.Visible);
        //    if (ep != null)
        //    {
        //        Asset asset = ep.PremiumAssets.FirstOrDefault().Asset;
        //        if (asset != null)
        //        {
        //            int assetId = asset == null ? 0 : asset.AssetId;
        //            ViewBag.AssetId = assetId;
        //            var clipDetails = Gakamai(ep.EpisodeId, assetId, Request, User, Uri.UnescapeDataString(hls));

        //            if (!String.IsNullOrEmpty(clipDetails.Url))
        //            {
        //                errorCode = ErrorCodes.Success;
        //                collection = MyUtility.setError(errorCode, clipDetails.Url);
        //                collection.Add("data", clipDetails);
        //            }
        //            else
        //            {
        //                errorCode = ErrorCodes.AkamaiCdnNotFound;
        //                collection = MyUtility.setError(errorCode, "Akamai Url not found.");
        //            }
        //        }
        //        else
        //        {
        //            errorCode = ErrorCodes.VideoNotFound;
        //            collection = MyUtility.setError(errorCode, "Video not found.");
        //        }
        //    }
        //    else
        //    {
        //        errorCode = ErrorCodes.EpisodeNotFound;
        //        collection = MyUtility.setError(errorCode, "Episode not found.");
        //    }

        //    return Content(MyUtility.buildJson(collection), "application/json");
        //}

        //private AkamaiFlowPlayerPluginClipDetails Gakamai(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, string hls)
        //{
        //    AkamaiFlowPlayerPluginClipDetails clipDetails = null;

        //    var offeringId = Helpers.GlobalConfig.offeringId;
        //    var videoUrl = string.Empty;
        //    var canPlay = false;
        //    var countryCode = Helpers.MyUtility.getCountry(req.UserHostAddress).getCode();

        //    var context = new IPTV2Entities();
        //    var offering = context.Offerings.Find(offeringId);

        //    var episode = context.Episodes.Find(episodeId);
        //    var asset = context.Assets.Find(assetId);

        //    var cdnId = 2; // akamai's cdn id

        //    if ((episode != null) & (asset != null))
        //    {
        //        clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

        //        canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
        //        // check with anonymous default package

        //        var packageId = GlobalConfig.AnonymousDefaultPackageId;
        //        if (!canPlay)
        //        {
        //            canPlay = IPTV2_Model.User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
        //        }

        //        // check user's access rights
        //        if (!canPlay && thisUser.Identity.IsAuthenticated)
        //        {
        //            var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
        //            if (user != null)
        //            {
        //                // check access from default logged in user package
        //                packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
        //                canPlay = IPTV2_Model.User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

        //                if (!canPlay)
        //                {
        //                    // check if user has entitlements that can play the video
        //                    canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
        //                }
        //            }
        //        }

        //        // get asset URL
        //        var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
        //        if (assetCdn != null)
        //        {
        //            var hlsPrefixPattern = "hls://o1-i.akamaihd.net/i/";
        //            var hlsSuffixPattern = "/master.m3u8";
        //            var zeriPrefixPattern = "http://o1-f.akamaihd.net/z/";
        //            var zeriSuffixPattern = "/manifest.f4m";
        //            var testUrl = "hls://o1-i.akamaihd.net/i/testfile/tvppampanga/tvppampanga-,300000,500000,800000,1000000,1300000,1500000,.mp4.csmil/manifest.f4m";
        //            testUrl = "hls://o1-i.akamaihd.net/i/testfile/20120227/20120227-tvppampanga-,300000,500000,800000,1000000,1300000,1500000,.mp4.csmil/manifest.f4m";
        //            testUrl = "hls://o1-i.akamaihd.net/i/testfile/tvpbatangas/20120228/tvpbatangas-,300000,500000,1500000,.mp4.csmil/master.m3u8";
        //            testUrl = "hls://o1-i.akamaihd.net/i/testfile/tvppanay/20120301/tvppanay-,300000,500000,800000,1000000,1300000,1500000,.mp4.csmil/master.m3u8";
        //            testUrl = hls;
        //            //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
        //            videoUrl = "hls://o1-i.akamaihd.net/i/testfile/darryltest/MAMS_TFCTV_MP4_,300000,500000,800000,1000000,1300000,1500000,.mp4.csmil/manifest.f4m".Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
        //            videoUrl = "hls://o1-i.akamaihd.net/i/iwantvtest/20120224-iwantvtest-,300000,500000,700000,1000000,.mp4.csmil/master.m3u8".Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
        //            videoUrl = testUrl.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);

        //            string ipAddress = string.Empty;
        //            if (!req.IsLocal)
        //            {
        //                ipAddress = req.GetUserHostAddressFromCloudflare();
        //            }

        //            int snippetStart = 0;
        //            int snippetEnd = 0;

        //            if (!canPlay)
        //            {
        //                if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
        //                {
        //                    snippetStart = asset.SnippetStart.Value.Seconds;
        //                    snippetEnd = asset.SnippetEnd.Value.Seconds;
        //                }
        //                else
        //                {
        //                    snippetStart = 0;
        //                    snippetEnd = 30;
        //                }
        //            }

        //            clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

        //            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
        //            double unixTime = ts.TotalSeconds;

        //            var tokenConfig = new AkamaiTokenConfig();
        //            tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
        //            tokenConfig.StartTime = Convert.ToUInt32(unixTime);
        //            tokenConfig.Window = 300;
        //            tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
        //            tokenConfig.Acl = "/*";
        //            tokenConfig.IP = ipAddress;
        //            tokenConfig.PreEscapeAcl = false;
        //            tokenConfig.IsUrl = false;
        //            tokenConfig.SessionID = string.Empty;
        //            tokenConfig.Payload = asset.AssetId.ToString() + ((clipDetails.SubClip == null) ? string.Empty : ":" + snippetStart.ToString() + ":" + snippetEnd.ToString());
        //            tokenConfig.Salt = string.Empty;
        //            tokenConfig.FieldDelimiter = '~';

        //            var token = AkamaiTokenGenerator.GenerateToken(tokenConfig);

        //            videoUrl += "?hdnea=" + token;
        //            clipDetails.Url = videoUrl;
        //            clipDetails.PromptToSubscribe = (clipDetails != null);
        //        }
        //    }
        //    return (clipDetails);
        //}

        //public ActionResult setStatus(string uid, string status)
        //{
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    collection.Add("uid", uid);
        //    collection.Add("status", status);
        //    GSResponse res = GigyaHelpers.createAndSendRequest("socialize.setStatus", GigyaHelpers.buildParameter(collection));
        //    return Content(res.GetData().ToJsonString(), "application/json");
        //}

        //public ActionResult logout(string uid)
        //{
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    collection.Add("uid", uid);
        //    GSResponse res = GigyaHelpers.createAndSendRequest("socialize.logout", GigyaHelpers.buildParameter(collection));
        //    return Content(res.GetData().ToJsonString(), "application/json");
        //}

        public ActionResult SiteMapXml()
        {
            var result = new XmlSiteMapResult();

            return result;
        }

        //public string WebClient()
        //{
        //    WebClient client = new WebClient();

        //    // Add a user agent header in case the
        //    // requested URI contains a query.

        //    Stream data = client.OpenRead("http://localhost:50696/Episode/_GetClip/21690");
        //    StreamReader reader = new StreamReader(data);
        //    string s = reader.ReadToEnd();
        //    // JsonConvert.DeserializeObject(s);
        //    data.Close();
        //    reader.Close();
        //    return s;
        //}

        public ActionResult CheckJS()
        {
            return PartialView("_CheckJS");
        }

        public ActionResult JavascriptError()
        {
            return View();
        }

        public ActionResult GoogleAnalytics()
        {
            return PartialView("_GoogleAnalyticsPartial");
        }

        [RequireHttp]
        public ActionResult TermsAndConditions()
        {
            try
            {
                var context = new IPTV2Entities();
                string countryCode = MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                if (MyUtility.isUserLoggedIn())
                {
                    var userId = new Guid(User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        countryCode = user.CountryCode;
                }

                //var userCountry = MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                if (String.IsNullOrEmpty(countryCode))
                    countryCode = GlobalConfig.DefaultCountry;
                var uCountry = context.Countries.FirstOrDefault(c => String.Compare(c.Code, countryCode, true) == 0);
                string subsidiary = "International";
                string termsCode = String.Empty;
                if (uCountry != null)
                {
                    if (uCountry.GomsSubsidiary != null)
                    {
                        switch (uCountry.GomsSubsidiary.Code)
                        {
                            case "GLB : AP": if (String.Compare(uCountry.Code, "TW", true) == 0) termsCode = "TW"; break;
                            case "GLB : AU": subsidiary = "Australia Pty. Ltd."; termsCode = "AU"; break;
                            case "GLB : IT":
                            case "GLB : IT : UK":
                            case "GLB : IT : ES": subsidiary = "Europe Ltd."; termsCode = "EU"; break;
                            case "GLB : CA": subsidiary = "Canada ULC"; termsCode = "CA"; break;
                            case "GLB : ME": subsidiary = "Middle East FZ-LLC"; termsCode = "ME"; break;
                            case "GLB : JP": subsidiary = "Japan, Inc."; termsCode = "JP"; break;
                            case "GLB : US":
                            default: subsidiary = "International"; termsCode = String.Empty; break;
                        }
                    }
                }
                ViewBag.Subsidiary = String.Format("ABS-CBN {0}", subsidiary);
                ViewBag.ShowAirTerms = false;
                try
                {
                    //check for project air promo
                    var registDt = DateTime.Now;
                    var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.ProjectAirPromoId && p.StartDate < registDt && p.EndDate > registDt && p.StatusId == GlobalConfig.Visible);
                    if (promo != null)
                    {
                        var CountryCodeIP = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                        bool isViewable = false;
                        var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.ProjectAirCategoryId);
                        if (category != null)
                            if (category is Show)
                                isViewable = ContextHelper.IsCategoryViewableInUserCountry((Show)category, CountryCodeIP);
                        ViewBag.ShowAirTerms = isViewable;
                        try
                        {
                            if (ViewBag.ShowAirTerms)
                            {
                                if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("air"))
                                {
                                    HttpCookie airCookie = new HttpCookie("air");
                                    airCookie.Value = "true";
                                    airCookie.Expires = DateTime.Now.AddYears(5);
                                    Response.Cookies.Add(airCookie);
                                }
                            }
                            else
                            {
                                if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("air"))
                                {
                                    HttpCookie airCookie = new HttpCookie("air");
                                    airCookie.Expires = DateTime.Now.AddDays(-1);
                                    Response.Cookies.Add(airCookie);
                                }
                            }

                        }
                        catch (Exception) { }
                    }
                }
                catch (Exception) { }

                if (!Request.Cookies.AllKeys.Contains("version"))
                    return View(String.Format("TermsAndConditions2{0}", termsCode));
                return View(String.Format("TermsAndConditions{0}", termsCode));
            }
            catch (Exception)
            {
                if (!Request.Cookies.AllKeys.Contains("version"))
                    return View("TermsAndConditions2");
                return View();
            }
        }

        [RequireHttp]
        public ActionResult ContactUs()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
            {
                try
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        var context = new IPTV2Entities();
                        var UserId = new Guid(User.Identity.Name);
                        var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            ViewBag.EmailAddress = user.EMail;
                            try
                            {
                                HttpCookie cookie = new HttpCookie("uspcook");
                                cookie.Domain = ".tfc.tv";
                                cookie.Expires = DateTime.Now.AddHours(2);
                                string fullname = String.Format("{0} {1}", user.FirstName, user.LastName);
                                cookie.Value = String.Format("{0}|{1}|{2}|{3}|{4}", user.EMail, fullname, user.Country.GomsSubsidiary.Description, user.GomsCustomerId, user.GomsServiceId);
                                Response.Cookies.Add(cookie);
                            }
                            catch (Exception) { }
                        }

                    }
                }
                catch (Exception) { }
                return View("ContactUs2");
            }

            return View();
        }

        [RequireHttp]
        public ActionResult CookiePolicy()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("CookiePolicy2");
            return View();
        }

        public ActionResult SendFeedback()
        {
            if (MyUtility.isUserLoggedIn())
            {
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(User.Identity.Name));
                ViewBag.Email = user.EMail;
            }
            return PartialView("_SendFeedback");
        }

        //[HttpPost]
        //public ActionResult _SendFeedback(FormCollection fc)
        //{
        //    Dictionary<string, object> collection = new Dictionary<string, object>();

        //    ErrorCodes errorCode = ErrorCodes.UnknownError;
        //    string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
        //    collection.Add("errorCode", errorCode);
        //    collection.Add("errorMessage", errorMessage);

        //    ErrorResponse response = new ErrorResponse();

        //    if (String.IsNullOrEmpty(fc["email"]) || String.IsNullOrEmpty(fc["message"]))
        //    {
        //        collection["errorCode"] = (int)ErrorCodes.IsMissingRequiredFields;
        //        collection["errorMessage"] = "Please fill up the required fields.";
        //        return Content(MyUtility.buildJson(collection), "application/json");
        //    }

        //    string email = fc["email"];
        //    string subject = fc["subject"];
        //    //string message = Uri.EscapeDataString(fc["message"]);
        //    string message = fc["message"];
        //    try
        //    {
        //        subject = String.Format("[{0}] {1}", fc["email"], fc["subject"]);
        //        var mailer = new UserMailer();
        //        var msg = mailer.SendFeedback(from: fc["email"], subject: subject, message: fc["message"]);
        //        msg.SendAsync();
        //        collection["errorCode"] = (int)ErrorCodes.Success;
        //        collection["errorMessage"] = "Feedback sent successfully.";
        //    }
        //    catch (Exception e)
        //    {
        //        collection["errorCode"] = (int)ErrorCodes.UnknownError;
        //        collection["errorMessage"] = e.Message;
        //    }
        //    return Content(MyUtility.buildJson(collection), "application/json");
        //}

        //[DonutOutputCache(NoStore = true, Duration = 0)]
        public ActionResult StatusBar()
        {
            if (User.Identity.IsAuthenticated)
                ViewBag.Name = MyUtility.GetFullName();
            return PartialView("_StatusBarPartial");
        }

        public ActionResult ConcurrentLogin()
        {
            if (MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            ViewBag.CallGigyaLogout = false;
            if (TempData["ToBeLoggedOutUid"] != null)
                ViewBag.CallGigyaLogout = true;
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("ConcurrentLogin2");
            return View();
        }

        public void PreventMultipleLogin()
        {
            var profiler = MiniProfiler.Current;
            if (GlobalConfig.IsPreventionOfMultipleLoginEnabled)
            {
                if (MyUtility.isUserLoggedIn())
                {
                    var cache = DataCache.Cache;
                    string cacheKey = "SESSIONID:U:" + User.Identity.Name.ToUpper();
                    HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    string sessionId = (string)cache[cacheKey];
                    if (!String.IsNullOrEmpty(sessionId))
                    {
                        var value = String.Compare(sessionId, authCookie.Value, true);
                        if (value != 0)
                        {
                            //Logout from Gigya
                            Dictionary<string, string> collection = new Dictionary<string, string>();
                            collection.Add("uid", User.Identity.Name);

                            TempData["ToBeLoggedOutUid"] = User.Identity.Name;
                            //using (profiler.Step("Gigya Logout"))
                            //{
                            //    GSResponse res = GigyaHelpers.createAndSendRequest("socialize.logout", GigyaHelpers.buildParameter(collection));
                            //}

                            FormsAuthentication.SignOut();

                            if (GlobalConfig.UseResponseBufferOutput)
                                ControllerContext.HttpContext.Response.BufferOutput = true;
                            //return Redirect(String.Format("{0}?multiple_login_detected=1", Request.RawUrl));
                            ControllerContext.HttpContext.Response.Redirect(GlobalConfig.MultipleLoginRedirectedUrl);
                        }
                    }
                }
            }

        }


        public PartialViewResult PreventMultipleLoginUsingJavaScript()
        {
            bool retVal = false;
            var profiler = MiniProfiler.Current;
            if (GlobalConfig.IsPreventionOfMultipleLoginEnabled)
            {
                if (MyUtility.isUserLoggedIn())
                {
                    var cache = DataCache.Cache;
                    string cacheKey = "SESSIONID:U:" + User.Identity.Name.ToUpper();
                    HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    string sessionId = (string)cache[cacheKey];
                    if (!String.IsNullOrEmpty(sessionId))
                    {
                        var value = String.Compare(sessionId, authCookie.Value, true);
                        if (value != 0)
                        {
                            //Logout from Gigya
                            Dictionary<string, string> collection = new Dictionary<string, string>();
                            collection.Add("uid", User.Identity.Name);
                            TempData["ToBeLoggedOutUid"] = User.Identity.Name;
                            FormsAuthentication.SignOut();
                            retVal = true; // Multiple Login Detected
                        }
                    }
                }
            }
            ViewBag.retVal = retVal;
            return PartialView();
        }

        //public ActionResult GenerateToken()
        //{
        //    return Content(MyUtility.ConvertToTimestamp(DateTime.Now).ToString().Replace('.', '-').PadRight(14, '0') + Guid.NewGuid().ToString().ToLower(), "text/plain");
        //}

        //public DateTime ActualTime(string ts)
        //{

        //    var t = Convert.ToDouble(ts);
        //    return MyUtility.ConvertFromUnixTimestamp(t);

        //}     

        [RequireHttp]
        public ActionResult JapanECommerceLawCompliance(string id)
        {
            if (String.IsNullOrEmpty(id))
                if (!Request.Cookies.AllKeys.Contains("version"))
                    return View("JapanECommerceLawCompliance2");
                else
                    return View();
            else
                if (String.Compare(id, "JP", true) == 0)
                    if (!Request.Cookies.AllKeys.Contains("version"))
                        return View("JapanECommerceLawCompliance2JP");
                    else
                        return View("JapanECommerceLawComplianceJP");
                else
                    if (!Request.Cookies.AllKeys.Contains("version"))
                        return View("JapanECommerceLawCompliance2");
                    else
                        return View();
        }


        public ActionResult CacheTester(int? id)
        {
            var offeringId = GlobalConfig.offeringId;
            var serviceId = GlobalConfig.serviceId;
            var context = new IPTV2Entities();
            var rightsType = RightsType.Online;
            var packageList = context.PackageTypes.Where(p => p.OfferingId == offeringId).ToList();
            var countries = context.Countries.Select(c => new { Code = c.Code });
            var offering = context.Offerings.Find(offeringId);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == serviceId);

            var categoryList = MyUtility.StringToIntList(GlobalConfig.CategoryIdsInCache);

            StringBuilder sb = new StringBuilder();

            if (id == null)
            {
                //fill cache of all packages
                foreach (var p in packageList)
                {
                    foreach (var c in countries)
                    {
                        var cacheKey = p.GetCacheKey(c.Code, rightsType);
                        var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                        if (cacheItem != null)
                        {
                            var timeOut = cacheItem.Timeout;
                            sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", cacheKey, timeOut, cacheItem.Size, cacheItem.RegionName));
                        }
                    }
                }

                foreach (var categoryId in categoryList)
                {
                    var cat = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == categoryId);
                    if (cat != null)
                    {
                        if (cat is Category)
                        {
                            var category = (Category)cat;
                            foreach (var c in countries)
                            {
                                var cacheKey = service.GetCacheKey(c.Code, category, rightsType);
                                var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                                if (cacheItem != null)
                                {
                                    var timeOut = cacheItem.Timeout;
                                    sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", cacheKey, timeOut, cacheItem.Size, cacheItem.RegionName));
                                }
                            }
                        }
                    }
                }
            }

            if (id == 1)
            {
                sb.AppendLine("PACKAGE FEATURES");
                var listOfProductPackages = context.ProductPackages.Where(p => p.Product.IsForSale && p.Product.StatusId == GlobalConfig.Visible).Select(p => p.PackageId).Distinct();
                foreach (var packageId in listOfProductPackages)
                {
                    foreach (var c in countries)
                    {
                        string cacheKey = "GPKGFEAT:P:" + packageId + ";C:" + c.Code;
                        var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                        if (cacheItem != null)
                        {
                            var timeOut = cacheItem.Timeout;
                            sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", cacheKey, timeOut, cacheItem.Size, cacheItem.RegionName));
                        }
                    }
                }
            }

            if (id == 2)
            {
                sb.AppendLine("---------SYNAPSE SITE MENU---------");
                var SynapseSiteMenuCacheKey = "SYNGETSITEMENU:0:";
                var SynapseSiteMenuCacheItem = DataCache.Cache.GetCacheItem(SynapseSiteMenuCacheKey);
                if (SynapseSiteMenuCacheItem != null)
                {
                    var timeOut = SynapseSiteMenuCacheItem.Timeout;
                    sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", SynapseSiteMenuCacheKey, timeOut, SynapseSiteMenuCacheItem.Size, SynapseSiteMenuCacheItem.RegionName));
                }
                sb.AppendLine("---------SYNAPSE CELEBRITY LIST---------");
                var SynapseCelebCacheKey = "SYNGACELEB;0";
                var SynapseCelebCacheItem = DataCache.Cache.GetCacheItem(SynapseCelebCacheKey);
                if (SynapseCelebCacheItem != null)
                {
                    var timeOut = SynapseCelebCacheItem.Timeout;
                    sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", SynapseCelebCacheKey, timeOut, SynapseCelebCacheItem.Size, SynapseCelebCacheItem.RegionName));
                }
                sb.AppendLine("---------SYNAPSE HOMEPAGE---------");
                foreach (var c in countries)
                {
                    var cacheKey = "SYNAPSEGHOMEPAGE:O:;C:" + c.Code;
                    var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                    if (cacheItem != null)
                    {
                        var timeOut = cacheItem.Timeout;
                        sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", cacheKey, timeOut, cacheItem.Size, cacheItem.RegionName));
                    }
                }
                sb.AppendLine("---------SYNAPSE CATEGORY LIST---------");
                foreach (var categoryId in categoryList)
                {
                    foreach (var c in countries)
                    {
                        var cacheKey = "SYNAPGTSHOWS:0;C:" + categoryId + ";CC:" + c.Code;
                        var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                        if (cacheItem != null)
                        {
                            var timeOut = cacheItem.Timeout;
                            sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", cacheKey, timeOut, cacheItem.Size, cacheItem.RegionName));
                        }
                    }
                }
            }

            if (id == 3)
            {
                sb.AppendLine("---------TOP REVIEWERS---------");
                if (true)
                {
                    var cacheKey = "SEGTR1:O;C:";
                    var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                    if (cacheItem != null)
                    {
                        var timeOut = cacheItem.Timeout;
                        sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", cacheKey, timeOut, cacheItem.Size, cacheItem.RegionName));
                    }
                }


                sb.AppendLine("---------MOST LOVED CELEB---------");
                if (true)
                {
                    var cacheKey = "SEGMLC1:O;C:";
                    var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                    if (cacheItem != null)
                    {
                        var timeOut = cacheItem.Timeout;
                        sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", cacheKey, timeOut, cacheItem.Size, cacheItem.RegionName));
                    }
                }

                sb.AppendLine("---------MOST LOVED EP---------");
                foreach (var c in countries)
                {
                    var cacheKey = "SEGMLE1:O;C:" + c.Code;
                    var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                    if (cacheItem != null)
                    {
                        var timeOut = cacheItem.Timeout;
                        sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", cacheKey, timeOut, cacheItem.Size, cacheItem.RegionName));
                    }
                }

                sb.AppendLine("---------MOST LOVED SHOW---------");
                foreach (var c in countries)
                {
                    var cacheKey = "SEGMLS1:O;C:" + c.Code;
                    var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                    if (cacheItem != null)
                    {
                        var timeOut = cacheItem.Timeout;
                        sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", cacheKey, timeOut, cacheItem.Size, cacheItem.RegionName));
                    }
                }
            }

            if (id == 4)
            {
                sb.AppendLine("SITEMAP CELEBRITY");
                var siteMapCelebrityCacheKey = "SMAPCELEB:O:00";
                var siteMapCelebrityCacheItem = DataCache.Cache.GetCacheItem(siteMapCelebrityCacheKey);
                if (siteMapCelebrityCacheItem != null)
                {
                    var timeOut = siteMapCelebrityCacheItem.Timeout;
                    sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", siteMapCelebrityCacheKey, timeOut, siteMapCelebrityCacheItem.Size, siteMapCelebrityCacheItem.RegionName));
                }

                sb.AppendLine("SITEMAP PROFILE");
                var siteMapProfileCacheKey = "SMAPPROFILE:O:00";
                var siteMapProfileCacheItem = DataCache.Cache.GetCacheItem(siteMapProfileCacheKey);
                if (siteMapProfileCacheItem != null)
                {
                    var timeOut = siteMapProfileCacheItem.Timeout;
                    sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", siteMapProfileCacheKey, timeOut, siteMapProfileCacheItem.Size, siteMapProfileCacheItem.RegionName));
                }

                sb.AppendLine("SITEMAP SHOW");
                var siteMapShowCacheKey = "SMAPSHOW:O:00";
                var siteMapShowCacheItem = DataCache.Cache.GetCacheItem(siteMapShowCacheKey);
                if (siteMapShowCacheItem != null)
                {
                    var timeOut = siteMapShowCacheItem.Timeout;
                    sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", siteMapShowCacheKey, timeOut, siteMapShowCacheItem.Size, siteMapShowCacheItem.RegionName));
                }

                sb.AppendLine("MENU");
                var menuNames = "Entertainment,News,Movies,Live".Split(',');
                foreach (var menuName in menuNames)
                {
                    var menuCacheKey = "JRBMG:O:" + menuName + ";C:";
                    var menuNameCacheItem = DataCache.Cache.GetCacheItem(menuCacheKey);
                    if (menuNameCacheItem != null)
                    {
                        var timeOut = menuNameCacheItem.Timeout;
                        sb.AppendLine(String.Format("Key:{0} TimeOut:{1} Size:{2} Region:{3}", menuCacheKey, timeOut, menuNameCacheItem.Size, menuNameCacheItem.RegionName));
                    }
                }
            }
            return Content(sb.ToString(), "text/plain");
        }

        //public ActionResult CacheTester2()
        //{
        //    var offeringId = GlobalConfig.offeringId;
        //    var serviceId = GlobalConfig.serviceId;
        //    var context = new IPTV2Entities();
        //    var rightsType = RightsType.Online;
        //    var packageList = context.PackageTypes.Where(p => p.OfferingId == offeringId).ToList();
        //    var countries = context.Countries.ToList();
        //    var offering = context.Offerings.Find(offeringId);
        //    var service = offering.Services.FirstOrDefault(s => s.PackageId == serviceId);
        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendLine("SUBSCRIPTION PACKAGE PRODUCTS");
        //    var shows = context.CategoryClasses.Where(c => c.StatusId == GlobalConfig.Visible && c is Show).ToList();
        //    foreach (var show in shows)
        //    {
        //        var categoryId = show.CategoryId;
        //        foreach (var c in countries)
        //        {
        //            var countryCode = c.Code;
        //            string cacheKey = "SGPP:Cat:" + categoryId.ToString() + ";C:" + countryCode;
        //            var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
        //            if (cacheItem != null)
        //            {
        //                var timeOut = cacheItem.Timeout;
        //                sb.AppendLine(String.Format("Key:{0} TimeOut:{1}", cacheKey, timeOut));
        //            }
        //        }
        //    }
        //    sb.AppendLine("LOAD ALL PRODUCTS");
        //    string scacheKey = "SPC:O:" + offeringId.ToString();
        //    var scacheItem = DataCache.Cache.GetCacheItem(scacheKey);
        //    if (scacheItem != null)
        //    {
        //        var timeOut = scacheItem.Timeout;
        //        sb.AppendLine(String.Format("Key:{0} TimeOut:{1}", scacheKey, timeOut));
        //    }

        //    return Content(sb.ToString(), "text/plain");
        //}

        public ActionResult TermsAndConditionsTFCEverywhere()
        {
            if (!GlobalConfig.IsTVERegistrationEnabled)
                return RedirectToAction("Index", "Home");
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("TermsAndConditionsTVE2");
            return View("TermsAndConditionsTVE");
        }

        public string GSamplePkg(int? id, string CountryCode)
        {
            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            var pkg = offering.Packages.FirstOrDefault(p => p.PackageId == id);
            var listOfShows = pkg.GetAllOnlineShowIds(CountryCode);
            return string.Join(",", listOfShows);
        }

        public string GetCountryCode()
        { return MyUtility.GetCurrentCountryCodeOrDefault(); }

        public string GetAllOnlineShowIds(int? id)
        {
            try
            {
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                if (category != null)
                {
                    var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                    var listOfShows = service.GetAllOnlineShowIds(GlobalConfig.DefaultCountry, (Category)category);
                    return string.Join(",", listOfShows);
                }
            }
            catch (Exception e) { return e.Message; }
            return String.Empty;
        }

        public string GetEpisodeParentShows(int? id)
        {
            try
            {
                var context = new IPTV2Entities();
                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                if (episode != null)
                {
                    var parentShows = episode.GetParentShows();
                    return string.Join(",", parentShows);
                }
            }
            catch (Exception) { }
            return String.Empty;
        }

        public string GetCategoryParentCategories(int? id)
        {
            try
            {
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                if (category != null)
                {
                    if (category is Show)
                    {
                        var show = (Show)category;
                        var listOfCategories = show.GetAllParentCategories();
                        return string.Join(",", listOfCategories);
                    }
                }
            }
            catch (Exception) { }
            return String.Empty;
        }

        public ActionResult Main()
        {
            try
            {
                if (!MyUtility.isUserLoggedIn())
                {
                    if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("SplashVisited"))
                    {
                        HttpCookie cookie = new HttpCookie("SplashVisited");
                        cookie.Value = "true";
                        cookie.Expires = DateTime.Now.AddDays(1);
                        this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                    }
                }
                else return RedirectToAction("Index");

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
                    var uCountry = context.Countries.FirstOrDefault(c => String.Compare(c.Code, userCountry, true) == 0);
                    ViewBag.UserCountry = uCountry;
                    stateCount = context.States.Count(s => String.Compare(s.CountryCode, userCountry, true) == 0);
                    ViewBag.StateCount = stateCount;
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return View();
        }

        public string GetAllShowsBasedOnOffering(string CountryCode, bool addExclusion)
        {
            var context = new IPTV2Entities();
            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, addExclusion);
            return string.Join(",", ShowListBasedOnCountryCode);
        }

        public ActionResult GetCacheItemTimeOut(string key)
        {
            StringBuilder sb = new StringBuilder();
            var cacheItem = DataCache.Cache.GetCacheItem(key);
            if (cacheItem != null)
            {
                var timeOut = cacheItem.Timeout;
                sb.AppendLine(String.Format("Key:{0} TimeOut:{1}", key, timeOut));
            }
            return Content(sb.ToString(), "text/plain");
        }

        public JsonResult IsEpisodeEntitled(int id)
        {
            var result = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
            var context = new IPTV2Entities();
            var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
            var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
            var parentCategories = episode.GetParentShows(CacheDuration);
            var registDt = DateTime.Now;
            if (User.Identity.IsAuthenticated)
            {
                var UserId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                result = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt);
            }
            else
                result = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public string GetProductIdsForShow(int id, string countryCode)
        {
            var context = new IPTV2Entities();
            var country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, countryCode, true) == 0);
            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            var service = offering.Services.FirstOrDefault(o => o.PackageId == GlobalConfig.serviceId);
            var show = (Show)context.CategoryClasses.Find(id);
            SortedSet<Int32> productIds = new SortedSet<int>();
            var packageProductIds = show.GetPackageProductIds(offering, countryCode, RightsType.Online);
            if (packageProductIds != null)
                productIds.UnionWith(packageProductIds);
            var showProductIds = show.GetShowProductIds(offering, countryCode, RightsType.Online);
            if (showProductIds != null)
                productIds.UnionWith(showProductIds);
            return string.Join(",", productIds);
        }

        public string GetShowsFromCategory(int id, string CountryCode)
        {
            var context = new IPTV2Entities();
            var country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);
            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            var service = offering.Services.FirstOrDefault(o => o.PackageId == GlobalConfig.serviceId);
            var category = (Category)context.CategoryClasses.Find(id);
            int[] showIds = service.GetAllOnlineShowIds(CountryCode, category).ToArray();
            return string.Join(",", showIds);
        }

        public string GetCountryC(string id)
        {
            return MyUtility.getCountry(id).getCode();
        }

        public ActionResult GetDataCache()
        {
            var sb = new StringBuilder();
            var totalSize = 0;
            try
            {
                var regions = DataCache.Cache.GetSystemRegions();
                foreach (var region in regions)
                {
                    foreach (var kvp in DataCache.Cache.GetObjectsInRegion(region).OrderByDescending(c => c.Key))
                    {
                        var cacheItem = DataCache.Cache.GetCacheItem(kvp.Key);
                        sb.AppendLine(String.Format("Key:{0}\tTimeOut:{1}\tSize: {2}\tRegion {3}\tCache {4}\tObject: {5}", cacheItem.Key, cacheItem.Timeout, cacheItem.Size, region, "default", cacheItem.Value.ToString()));
                        totalSize += cacheItem.Size;
                    }
                }
            }
            catch (Exception e) { sb.AppendLine(e.Message); }
            sb.AppendLine(String.Format("Total Cache Size: {0}", totalSize));
            return Content(sb.ToString(), "text/plain");
        }

        public PartialViewResult BuildFeature(int id, int itemPerSlide, string containerId, string navigationContainerId, string featureType = "episode")
        {
            List<HomepageFeatureItem> jfi = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            try
            {
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "JRGL:O:" + id.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var feature = context.Features.FirstOrDefault(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
                    if (feature != null)
                    {
                        var featureItems = context.FeatureItems.Where(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn);
                        jfi = new List<HomepageFeatureItem>();
                        foreach (var f in featureItems)
                        {
                            if (f is EpisodeFeatureItem)
                            {
                                if (((EpisodeFeatureItem)f).Episode != null)
                                {
                                    var episode = ((EpisodeFeatureItem)f).Episode;
                                    if (episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt && episode.OnlineStatusId == GlobalConfig.Visible)
                                    {
                                        EpisodeCategory epCategory = null;
                                        if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                                        {
                                            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                            epCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                                        }
                                        else
                                        {
                                            var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                            epCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                                        }
                                        if (epCategory != null)
                                        {
                                            Show show = epCategory.Show;
                                            if (show != null)
                                            {
                                                string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
                                                string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                                HomepageFeatureItem j = new HomepageFeatureItem()
                                                {
                                                    id = episode.EpisodeId,
                                                    description = episode.Description,
                                                    name = episode.EpisodeName,
                                                    airdate = (episode.DateAired != null) ? episode.DateAired.Value.ToString("MMMM d, yyyy") : "",
                                                    show_id = show.CategoryId,
                                                    show_name = String.Compare(episode.EpisodeName, episode.EpisodeCode) == 0 ? show.CategoryName : episode.EpisodeName,
                                                    imgurl = img,
                                                    show_imgurl = showImg,
                                                    blurb = HttpUtility.HtmlEncode(episode.Synopsis),
                                                    slug = MyUtility.GetSlug(episode.IsLiveChannelActive == true ? episode.Description : String.Format("{0} {1}", show.Description, episode.DateAired.Value.ToString("MMMM d yyyy")))
                                                };
                                                jfi.Add(j);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (f is ShowFeatureItem)
                            {
                                Show show = ((ShowFeatureItem)f).Show;
                                string img = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    id = show.CategoryId,
                                    name = show.Description,
                                    blurb = HttpUtility.HtmlEncode(show.Blurb),
                                    imgurl = img,
                                    slug = MyUtility.GetSlug(show.Description)
                                };
                                jfi.Add(j);
                            }
                            else if (f is CelebrityFeatureItem)
                            {
                                Celebrity person = ((CelebrityFeatureItem)f).Celebrity;
                                string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    name = person.FullName,
                                    id = person.CelebrityId,
                                    imgurl = img,
                                    blurb = HttpUtility.HtmlEncode(person.Description),
                                    slug = MyUtility.GetSlug(person.FullName)
                                };
                                jfi.Add(j);
                            }
                        }
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                        cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                ViewBag.itemPerSlide = itemPerSlide;
                ViewBag.containerId = containerId;
                ViewBag.navigationContainerId = navigationContainerId;
                ViewBag.featureType = featureType;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(jfi);
        }

        public PartialViewResult BuildCarousel(int id, int itemPerSlide, string containerId)
        {
            List<JsonCarouselItem> jci = null;
            string jsonString = String.Empty;
            try
            {
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "JRGC:U:" + id.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    jci = new List<JsonCarouselItem>();
                    Carousel carousel = context.Carousels.FirstOrDefault(c => c.CarouselId == id && c.StatusId == GlobalConfig.Visible);
                    if (carousel != null)
                    {
                        List<CarouselSlide> slides = carousel.CarouselSlides.Where(c => c.StatusId == GlobalConfig.Visible).OrderByDescending(c => c.AuditTrail.UpdatedOn).ThenByDescending(c => c.CarouselSlideId).ToList();
                        foreach (var slide in slides)
                        {
                            JsonCarouselItem item = new JsonCarouselItem() { CarouselSlideId = slide.CarouselSlideId, BannerImageUrl = String.Format("{0}{1}/{2}", GlobalConfig.CarouselImgPath, slide.CarouselSlideId.ToString(), slide.BannerImageUrl), Blurb = HttpUtility.HtmlEncode(slide.Blurb), Name = slide.Name, Header = slide.Header, TargetUrl = slide.TargetUrl, ButtonLabel = slide.ButtonLabel };
                            if (MyUtility.StringToIntList(GlobalConfig.PBBCarouselSlideIds).Contains(slide.CarouselSlideId))
                            {
                                var PBBCountryCodeList = GlobalConfig.PBBBlockedCountryCodes.Split(',');
                                if (!PBBCountryCodeList.Contains(CountryCode))
                                    jci.Add(item);
                            }
                            else if (MyUtility.StringToIntList(GlobalConfig.USAllowedCarouselIds).Contains(slide.CarouselSlideId))
                            {
                                if (String.Compare(CountryCode, "US", true) == 0)
                                    jci.Add(item);
                            }
                            else if (MyUtility.StringToIntList(GlobalConfig.CAAllowedCarouselIds).Contains(slide.CarouselSlideId))
                            {
                                if (String.Compare(CountryCode, "CA", true) == 0)
                                    jci.Add(item);
                            }
                            else
                                jci.Add(item);
                        }
                    }
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jci);
                    cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                }
                else
                    jci = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonCarouselItem>>(jsonString);
                ViewBag.itemPerSlide = itemPerSlide;
                ViewBag.containerId = containerId;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(jci);
        }

        public string GetAllOnlineShowsFromService(string CountryCode)
        {
            try
            {
                var context = new IPTV2Entities();
                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                var listOfShows = service.GetAllOnlineShowIds(CountryCode);
                return string.Join(",", listOfShows);
            }
            catch (Exception e) { return e.Message; }
        }

        public PartialViewResult BuildFeatureCustomLink(int id, int itemPerSlide, string containerId, int maxCount, string customLink, string featureType = "episode", bool isContainerActive = true, bool targetSameWindow = true, bool useDescription = false)
        {
            List<HomepageFeatureItem> jfi = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            try
            {
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "JRGL:O:" + id.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var feature = context.Features.FirstOrDefault(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
                    if (feature != null)
                    {
                        var featureItems = context.FeatureItems.Where(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).Take(maxCount);
                        jfi = new List<HomepageFeatureItem>();
                        foreach (var f in featureItems)
                        {
                            if (f is EpisodeFeatureItem)
                            {
                                if (((EpisodeFeatureItem)f).Episode != null)
                                {
                                    var episode = ((EpisodeFeatureItem)f).Episode;
                                    if (episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt && episode.OnlineStatusId == GlobalConfig.Visible)
                                    {

                                        EpisodeCategory epCategory = null;
                                        if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                                        {
                                            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                            epCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                                        }
                                        else
                                        {
                                            var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                            epCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                                        }
                                        if (epCategory != null)
                                        {
                                            Show show = epCategory.Show;
                                            if (show != null)
                                            {
                                                string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
                                                string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                                HomepageFeatureItem j = new HomepageFeatureItem()
                                                {
                                                    id = episode.EpisodeId,
                                                    description = episode.Description,
                                                    name = episode.EpisodeName,
                                                    airdate = (episode.DateAired != null) ? episode.DateAired.Value.ToString("MMMM d, yyyy") : "",
                                                    show_id = show.CategoryId,
                                                    show_name = String.Compare(episode.EpisodeName, episode.EpisodeCode) == 0 ? show.CategoryName : episode.EpisodeName,
                                                    imgurl = img,
                                                    show_imgurl = showImg,
                                                    blurb = HttpUtility.HtmlEncode(episode.Synopsis),
                                                    slug = MyUtility.GetSlug(episode.IsLiveChannelActive == true ? episode.Description : String.Format("{0} {1}", show.Description, episode.DateAired.Value.ToString("MMMM d yyyy")))
                                                };
                                                jfi.Add(j);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (f is ShowFeatureItem)
                            {
                                Show show = ((ShowFeatureItem)f).Show;
                                string img = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    id = show.CategoryId,
                                    name = show.Description,
                                    blurb = HttpUtility.HtmlEncode(show.Blurb),
                                    imgurl = img,
                                    slug = MyUtility.GetSlug(show.Description)
                                };
                                jfi.Add(j);
                            }
                            else if (f is CelebrityFeatureItem)
                            {
                                Celebrity person = ((CelebrityFeatureItem)f).Celebrity;
                                string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    name = person.FullName,
                                    id = person.CelebrityId,
                                    imgurl = img,
                                    blurb = HttpUtility.HtmlEncode(person.Description),
                                    slug = MyUtility.GetSlug(person.FullName)
                                };
                                jfi.Add(j);
                            }
                        }
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                        cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                ViewBag.itemPerSlide = itemPerSlide;
                ViewBag.containerId = containerId;
                ViewBag.featureType = featureType;
                ViewBag.customLink = customLink;
                ViewBag.isContainerActive = isContainerActive;
                ViewBag.targetSameWindow = targetSameWindow;
                ViewBag.useDescription = useDescription;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(jfi);
        }

        public ActionResult Index2()
        {
            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                var userId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    ViewBag.FirstName = user.FirstName;
                }
            }
            return View();
        }

        public PartialViewResult BuildSection(int id, string sectionTitle, string containerId, int? pageSize, int page = 0, string featureType = "episode", bool removeShowAll = true, bool isFeature = false, bool ShowAllItems = false, string partialViewName = "")
        {
            List<HomepageFeatureItem> jfi = null;
            List<HomepageFeatureItem> obj = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            int size = GlobalConfig.FeatureItemsPageSize;
            int skipSize = 0;
            try
            {
                if (pageSize != null)
                    size = (int)pageSize;
                skipSize = size * page;
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "JRGL:O:" + id.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Microsoft.ApplicationServer.Caching.DataCacheException) { DataCache.Refresh(); }
                catch (Exception) { }

                ViewBag.FeatureType = featureType;
                ViewBag.SectionTitle = sectionTitle;
                ViewBag.id = id;
                ViewBag.containerId = containerId;
                ViewBag.pageSize = size;
                ViewBag.RemoveShowAll = removeShowAll;
                ViewBag.IsFeature = isFeature;

                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
                if (feature != null)
                {
                    if (String.IsNullOrEmpty(jsonString))
                    {
                        string assetBaseUrl = GlobalConfig.AssetsBaseUrl;
                        string episodeImgPath = GlobalConfig.EpisodeImgPath;
                        string showImgPath = GlobalConfig.ShowImgPath;
                        string celebrityImgPath = GlobalConfig.CelebrityImgPath;
                        try
                        {
                            assetBaseUrl = assetBaseUrl.Replace("http:", "");
                            episodeImgPath = episodeImgPath.Replace("http:", "");
                            showImgPath = showImgPath.Replace("http:", "");
                            celebrityImgPath = celebrityImgPath.Replace("http:", "");
                        }
                        catch (Exception) { }

                        var featureItems = context.FeatureItems.Where(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.FeatureItemId);
                        jfi = new List<HomepageFeatureItem>();
                        foreach (var f in featureItems)
                        {
                            if (f is EpisodeFeatureItem)
                            {
                                if (((EpisodeFeatureItem)f).Episode != null)
                                {
                                    var episode = ((EpisodeFeatureItem)f).Episode;
                                    if (episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt && episode.OnlineStatusId == GlobalConfig.Visible)
                                    {
                                        EpisodeCategory epCategory = null;
                                        if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                                        {
                                            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                            epCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                                        }
                                        else
                                        {
                                            var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                            epCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                                        }
                                        if (epCategory != null)
                                        {
                                            Show show = epCategory.Show;
                                            if (show != null)
                                            {
                                                string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? assetBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", episodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
                                                string showImg = String.IsNullOrEmpty(show.ImagePoster) ? assetBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", showImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                                HomepageFeatureItem j = new HomepageFeatureItem()
                                                {
                                                    id = episode.EpisodeId,
                                                    description = episode.Description,
                                                    name = episode.EpisodeName,
                                                    airdate = (episode.DateAired != null) ? episode.DateAired.Value.ToString("MMMM d, yyyy") : "",
                                                    show_id = show.CategoryId,
                                                    show_name = String.Compare(episode.EpisodeName, episode.EpisodeCode) == 0 ? show.CategoryName : episode.EpisodeName,
                                                    imgurl = img,
                                                    show_imgurl = showImg,
                                                    blurb = HttpUtility.HtmlEncode(episode.Synopsis),
                                                    slug = MyUtility.GetSlug(episode.IsLiveChannelActive == true ? episode.Description : String.Format("{0} {1}", show.Description, episode.DateAired.Value.ToString("MMMM d yyyy")))
                                                };
                                                jfi.Add(j);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (f is ShowFeatureItem)
                            {
                                Show show = ((ShowFeatureItem)f).Show;
                                string img = String.IsNullOrEmpty(show.ImagePoster) ? assetBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", showImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    id = show.CategoryId,
                                    name = show.Description,
                                    blurb = HttpUtility.HtmlEncode(show.Blurb),
                                    imgurl = img,
                                    slug = MyUtility.GetSlug(show.Description)
                                };
                                jfi.Add(j);
                            }
                            else if (f is CelebrityFeatureItem)
                            {
                                Celebrity person = ((CelebrityFeatureItem)f).Celebrity;
                                string img = String.IsNullOrEmpty(person.ImageUrl) ? assetBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", celebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    name = person.FullName,
                                    id = person.CelebrityId,
                                    imgurl = img,
                                    blurb = HttpUtility.HtmlEncode(person.Description),
                                    slug = MyUtility.GetSlug(person.FullName)
                                };
                                jfi.Add(j);
                            }
                        }
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                        cache.Put(cacheKey, jsonString, DataCache.CacheDuration);

                    }
                    else
                        jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                    ViewBag.LinkSlug = MyUtility.GetSlug(feature.Description);
                }
                try
                {
                    if (jfi != null)
                        obj = pageSize == null ? jfi : jfi.Take(size).ToList();
                }
                catch (Exception)
                {
                    obj = jfi;
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, obj);
            return PartialView(obj);
        }

        public PartialViewResult LoadMoreItems(int id, int page, int? pageSize, string featureType = "episode")
        {
            List<HomepageFeatureItem> jfi = null;
            List<HomepageFeatureItem> obj = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            int size = GlobalConfig.FeatureItemsPageSize;
            int skipSize = 0;
            try
            {
                if (pageSize != null)
                    size = (int)pageSize;
                skipSize = size * page;
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "JRGL:O:" + id.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var feature = context.Features.FirstOrDefault(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
                    if (feature != null)
                    {
                        var featureItems = context.FeatureItems.Where(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).Skip(skipSize).Take(size);
                        jfi = new List<HomepageFeatureItem>();
                        foreach (var f in featureItems)
                        {
                            if (f is EpisodeFeatureItem)
                            {
                                if (((EpisodeFeatureItem)f).Episode != null)
                                {
                                    var episode = ((EpisodeFeatureItem)f).Episode;
                                    if (episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt && episode.OnlineStatusId == GlobalConfig.Visible)
                                    {
                                        EpisodeCategory epCategory = null;
                                        if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                                        {
                                            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                            epCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                                        }
                                        else
                                        {
                                            var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                            epCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                                        }
                                        if (epCategory != null)
                                        {
                                            Show show = epCategory.Show;
                                            if (show != null)
                                            {
                                                string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
                                                string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                                HomepageFeatureItem j = new HomepageFeatureItem()
                                                {
                                                    id = episode.EpisodeId,
                                                    description = episode.Description,
                                                    name = episode.EpisodeName,
                                                    airdate = (episode.DateAired != null) ? episode.DateAired.Value.ToString("MMMM d, yyyy") : "",
                                                    show_id = show.CategoryId,
                                                    show_name = String.Compare(episode.EpisodeName, episode.EpisodeCode) == 0 ? show.CategoryName : episode.EpisodeName,
                                                    imgurl = img,
                                                    show_imgurl = showImg,
                                                    blurb = HttpUtility.HtmlEncode(episode.Synopsis),
                                                    slug = MyUtility.GetSlug(episode.IsLiveChannelActive == true ? episode.Description : String.Format("{0} {1}", show.Description, episode.DateAired.Value.ToString("MMMM d yyyy")))
                                                };
                                                jfi.Add(j);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (f is ShowFeatureItem)
                            {
                                Show show = ((ShowFeatureItem)f).Show;
                                string img = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    id = show.CategoryId,
                                    name = show.Description,
                                    blurb = HttpUtility.HtmlEncode(show.Blurb),
                                    imgurl = img,
                                    slug = MyUtility.GetSlug(show.Description)
                                };
                                jfi.Add(j);
                            }
                            else if (f is CelebrityFeatureItem)
                            {
                                Celebrity person = ((CelebrityFeatureItem)f).Celebrity;
                                string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    name = person.FullName,
                                    id = person.CelebrityId,
                                    imgurl = img,
                                    blurb = HttpUtility.HtmlEncode(person.Description),
                                    slug = MyUtility.GetSlug(person.FullName)
                                };
                                jfi.Add(j);
                            }
                        }
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                        cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                ViewBag.page = page + 1;
                ViewBag.FeatureType = featureType;
                ViewBag.id = id;
                ViewBag.pageSize = size;
                if (jfi != null)
                    obj = jfi.Skip(skipSize).Take(size).ToList();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(obj);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFeedback(FormCollection fc)
        {

            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            string url = Url.Action("ContactUs", "Home").ToString();
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
                    string email = fc["email"];
                    string subject = fc["subject"];
                    string message = fc["message"];
                    subject = String.Format("[{0}] {1}", fc["email"], fc["subject"]);
                    //var mailer = new UserMailer();                    
                    //var msg = mailer.SendFeedback(from: email, subject: subject, message: message);
                    //msg.SendAsync();

                    MyUtility.SendEmailViaSendGrid(GlobalConfig.SupportEmail, GlobalConfig.NoReplyEmail, subject, message, MailType.TextOnly, message);

                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                    ReturnCode.StatusHeader = "Your feedback has been submitted!";
                    ReturnCode.StatusMessage = "Thanks for letting us know what you think!";
                    ReturnCode.StatusMessage2 = "We will see what we can do about it and get back to you.";
                    TempData["ErrorMessage"] = ReturnCode;
                    return RedirectToAction("Index", "Home"); // successful submission
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusMessage = e.Message;
                TempData["ErrorMessage"] = ReturnCode;
            }
            return Redirect(url);
        }

        public PartialViewResult GetFeaturedImageFromCarousel(int id, string partialViewName = "")
        {
            List<JsonCarouselItem> jci = null;
            string jsonString = String.Empty;
            try
            {
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "JRGC:U:" + id + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    jci = new List<JsonCarouselItem>();
                    Carousel carousel = context.Carousels.FirstOrDefault(c => c.CarouselId == id && c.StatusId == GlobalConfig.Visible);
                    if (carousel != null)
                    {
                        List<CarouselSlide> slides = carousel.CarouselSlides.Where(c => c.StatusId == GlobalConfig.Visible).OrderByDescending(c => c.AuditTrail.UpdatedOn).ThenByDescending(c => c.CarouselSlideId).ToList();

                        var MEPacMayCountryCodeList = GlobalConfig.MEPacMayAllowedCountryCodes.Split(',');

                        foreach (var slide in slides)
                        {
                            JsonCarouselItem item = new JsonCarouselItem() { CarouselSlideId = slide.CarouselSlideId, BannerImageUrl = String.Format("{0}{1}/{2}", GlobalConfig.CarouselImgPath, slide.CarouselSlideId.ToString(), slide.BannerImageUrl), Blurb = HttpUtility.HtmlEncode(slide.Blurb), Name = slide.Name, Header = slide.Header, TargetUrl = slide.TargetUrl, ButtonLabel = slide.ButtonLabel };
                            if (MyUtility.StringToIntList(GlobalConfig.PBBCarouselSlideIds).Contains(slide.CarouselSlideId))
                            {
                                var PBBCountryCodeList = GlobalConfig.PBBBlockedCountryCodes.Split(',');
                                if (!PBBCountryCodeList.Contains(CountryCode))
                                    jci.Add(item);
                            }
                            else if (MyUtility.StringToIntList(GlobalConfig.BCWMHThanksgivingConcertSlideIds).Contains(slide.CarouselSlideId))
                            {
                                var BCWMHCountryCodeList = GlobalConfig.BCWMHThanksgivingConcertBlockedCountryCodes.Split(',');
                                if (!BCWMHCountryCodeList.Contains(CountryCode))
                                    jci.Add(item);
                            }
                            else if (MyUtility.StringToIntList(GlobalConfig.USAllowedCarouselIds).Contains(slide.CarouselSlideId))
                            {
                                if (String.Compare(CountryCode, "US", true) == 0)
                                    jci.Add(item);
                            }
                            else if (MyUtility.StringToIntList(GlobalConfig.CAAllowedCarouselIds).Contains(slide.CarouselSlideId))
                            {
                                if (String.Compare(CountryCode, "CA", true) == 0)
                                    jci.Add(item);
                            }
                            else if (MyUtility.StringToIntList(GlobalConfig.MEPacMayCarouselIds).Contains(slide.CarouselSlideId))
                            {
                                if (MEPacMayCountryCodeList.Contains(CountryCode))
                                    jci.Add(item);
                            }
                            else if (MyUtility.StringToIntList(GlobalConfig.NonMEPacMayCarouselIds).Contains(slide.CarouselSlideId))
                            {
                                if (!MEPacMayCountryCodeList.Contains(CountryCode))
                                    jci.Add(item);
                            }
                            else
                                jci.Add(item);
                        }
                    }
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jci);
                    cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                }
                else
                    jci = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonCarouselItem>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, jci);
            return PartialView(jci);
        }

        public PartialViewResult BuildReviews(int id)
        {
            List<HomepageFeatureItem> jfi = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            try
            {
                var cache = DataCache.Cache;
                string cacheKey = "UXBREV:O:" + id.ToString();
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                    if (category != null)
                    {
                        var subcategories = category.CategoryClassSubCategories.Select(c => c.SubCategory);
                        if (subcategories != null)
                        {
                            jfi = new List<HomepageFeatureItem>();
                            int ctr = 0;
                            foreach (var item in subcategories)
                            {
                                var obj = new HomepageFeatureItem()
                                {
                                    blurb = item.Blurb
                                };
                                jfi.Add(obj);
                                ctr = ctr + 1;
                                if (ctr > 2)
                                    break;
                            }
                        }
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(jfi);
        }

        public string GetAllShowsBasedOnCountryCode(string CountryCode)
        {
            var context = new IPTV2Entities();
            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
            return string.Join(",", ShowListBasedOnCountryCode);
        }

        public string NoProxy()
        {
            return MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
        }

        public ActionResult Pixel()
        {
            return View();
        }

        public ActionResult Survey()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["LoginErrorMessage"] = "Please sign in to answer the survey.";
                TempData["RedirectUrl"] = Request.Url.PathAndQuery;
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        private void ReauthenticateUser()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var uid = User.Identity.Name;
                    HttpCookie authCookie = Request.Cookies[System.Web.Security.FormsAuthentication.FormsCookieName];
                    if (authCookie.Expires.Subtract(DateTime.Now).Days < 50)
                    {
                        authCookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(authCookie);
                        System.Web.Security.FormsAuthenticationTicket ticket = new System.Web.Security.FormsAuthenticationTicket(uid, true, GlobalConfig.FormsAuthenticationTimeout);
                        string encTicket = System.Web.Security.FormsAuthentication.Encrypt(ticket);
                        HttpCookie cookie = new HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName, encTicket);
                        cookie.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Add(cookie);
                    }
                }
            }
            catch (Exception ex) { MyUtility.LogException(ex, "Renew Session Cookie Exception"); }
        }

        public string GCountry(string ip)
        {
            string CountryCode = String.Empty;
            try
            {
                var location = MyUtility.GetLocationBasedOnIpAddress(ip);
                if (location != null)
                    CountryCode = location.countryCode;
            }
            catch (Exception) { }
            return CountryCode;
        }

        public ActionResult TestP(string ip)
        {
            if (String.IsNullOrEmpty(ip))
                ip = Request.GetUserHostAddressFromCloudflare();
            var location = MyUtility.GetLocationBasedOnIpAddress(ip);
            ViewBag.location = location;
            return View();
        }

        public string sha1(string p)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(p, "SHA1");
        }

        public string IsOnlineAllowed(int id, string CountryCode)
        {
            var context = new IPTV2Entities();
            var category = context.CategoryClasses.Find(id);
            if (category is Show)
            {
                var show = (Show)category;
                return String.Format("{0}", show.IsOnlineAllowed(CountryCode));
            }
            return String.Empty;
        }

        public string GetParent(int? id)
        {
            try
            {
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                if (category != null)
                {
                    return string.Join(",", category.CategoryClassParentCategories.Select(c => c.ParentCategory.Description));
                }
            }
            catch (Exception) { }
            return String.Empty;
        }

        public string CFCode()
        {
            return Request.ServerVariables["HTTP_CF_IPCOUNTRY"];
        }

        public ActionResult Rinternal()
        {
            try
            {
                HttpCookie cookie = new HttpCookie("rcdskipcheck");
                cookie.Value = "skip";
                cookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(cookie);
            }
            catch (Exception) { }
            return RedirectToAction("Index", "Home");
        }
    }
}
