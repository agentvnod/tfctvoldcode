using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;
using System.Web.Security;
using StackExchange.Profiling;
using System.Diagnostics;

namespace TFCTV.Controllers
{
    public class PromoController : Controller
    {
        //
        // GET: /Promo/

        public ActionResult OnlinePremiere()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return RedirectToActionPermanent("List", "Category", new { id = GlobalConfig.OnlinePremiereCategoryId });

            var context = new IPTV2Entities();
            var registDt = DateTime.Now;

            List<CategoryWithPreview> list = new List<CategoryWithPreview>();

            var PreviewCategoryIds = MyUtility.StringToIntList(GlobalConfig.OnlinePremierePreviewCategoryIds);
            var onlinePremiere = context.CategoryClasses.Where(c => PreviewCategoryIds.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible && c.EndDate > registDt && c.StartDate < registDt).OrderByDescending(c => c.StartDate);

            if (onlinePremiere != null)
            {
                var FullCategoryIds = GlobalConfig.OnlinePremiereFullCategoryIds.Split(';');
                foreach (var preview in onlinePremiere)
                {
                    var categoryWithPreview = new CategoryWithPreview() { Preview = preview };
                    foreach (var full in FullCategoryIds)
                    {
                        if (full.Contains(preview.CategoryId.ToString()))
                        {
                            var temp = full.Split('|');
                            var movieCategoryId = Convert.ToInt32(temp[0]);
                            var movie = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == movieCategoryId);
                            if (movie != null)
                                categoryWithPreview.Full = movie;
                        }
                    }
                    list.Add(categoryWithPreview);
                }
                if (list.Count() == 0)
                    return RedirectToAction("Index", "Home");

                return View(list);
            }
            else
                return RedirectToAction("Index", "Home");
            //var AMS = context.CategoryClasses.Count(c => c.CategoryId == 1531 && c.StartDate < registDt && c.StatusId == GlobalConfig.Visible && c.EndDate > registDt);
            //var EBYT = context.CategoryClasses.Count(c => c.CategoryId == 1532 && c.StartDate < registDt && c.StatusId == GlobalConfig.Visible && c.EndDate > registDt);
            //var UY = context.CategoryClasses.Count(c => c.CategoryId == 1533 && c.StartDate < registDt && c.StatusId == GlobalConfig.Visible && c.EndDate > registDt);

            //var ASMK = context.CategoryClasses.Count(c => c.CategoryId == 1546 && c.StartDate < registDt && c.StatusId == GlobalConfig.Visible && c.EndDate > registDt);
            //var SRR13 = context.CategoryClasses.Count(c => c.CategoryId == 1535 && c.StartDate < registDt && c.StatusId == GlobalConfig.Visible && c.EndDate > registDt);

            //ViewBag.IsAMSAvailable = AMS > 0 ? true : false;
            //ViewBag.IsEBYTAvailable = EBYT > 0 ? true : false;
            //ViewBag.IsUYAvailable = UY > 0 ? true : false;
            //ViewBag.IsASMKAvailable = ASMK > 0 ? true : false;
            //ViewBag.IsSRR13Available = SRR13 > 0 ? true : false;
            //return View();
        }

        public ActionResult ReferAKapamilya()
        {
            //if (MyUtility.isUserLoggedIn())
            //{
            //    var cache = DataCache.Cache;
            //    string cacheKey = "SESSIONID:U:" + User.Identity.Name;
            //    HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            //    string sessionId = (string)cache[cacheKey];
            //    var value = String.Compare(sessionId, authCookie.Value, false);
            //    if (value != 0)
            //    {
            //        FormsAuthentication.SignOut();
            //        return Redirect(String.Format("{0}?multiple_login_detected=1", Request.RawUrl));
            //    }
            //    ViewBag.IsValidSessionID = value == 0 ? "valid" : "invalid";
            //}

            string iosUrl = "http://iptvhls-i.akamaihd.net/hls/live/201825/anc/master.m3u8";
            AkamaiFlowPlayerPluginClipDetails clipDetails = new AkamaiFlowPlayerPluginClipDetails();
            clipDetails.Url = MyUtility.GenerateAkamaiToken(iosUrl, String.Format("{0}{1}", 22, Guid.NewGuid()));
            ViewBag.IosUrl = clipDetails.Url;
            return View();
        }

        public ActionResult HeatMap()
        {
            return View();
        }
        public ActionResult OKGo()
        {

            try
            {
                string ip = Request.GetUserHostAddressFromCloudflare();
                var location = MyUtility.getLocation(ip);
                if (String.Compare(location.countryCode, GlobalConfig.DefaultCountry, true) != 0)
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception) { }

            DateTime registDt = DateTime.Now;
            var context = new IPTV2Entities();
            var channel = context.Channels.FirstOrDefault(c => c.ChannelId == GlobalConfig.OkGoChannelId && c.OnlineStatusId == GlobalConfig.Visible);
            if (channel != null)
            {
                if (channel.OnlineStartDate > registDt)
                    return RedirectToAction("Index", "Home");
                if (channel.OnlineEndDate < registDt)
                    return RedirectToAction("Index", "Home");
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, GlobalConfig.OkGoChannelId, EngagementContentType.Channel);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }
                ViewBag.CategoryType = "Live";
                return View(channel);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult DigitalShorts(int? id)
        {
            var context = new IPTV2Entities();
            EpisodeCategory category;
            if (id == null)
                category = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.DigitalShortsCategoryId).OrderBy(e => Guid.NewGuid()).FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible);
            else
                category = context.EpisodeCategories1.FirstOrDefault(e => e.CategoryId == GlobalConfig.DigitalShortsCategoryId && e.EpisodeId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible);

            if (category != null)
            {
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, category.EpisodeId, EngagementContentType.Episode);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }
                return View(category);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult KwentoNgPasko(int? id)
        {
            var context = new IPTV2Entities();
            EpisodeCategory category;
            if (id == null)
                category = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.KwentoNgPaskoCategoryId).OrderBy(e => Guid.NewGuid()).FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible);
            else
                category = context.EpisodeCategories1.FirstOrDefault(e => e.CategoryId == GlobalConfig.KwentoNgPaskoCategoryId && e.EpisodeId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible);

            if (category != null)
            {
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, category.EpisodeId, EngagementContentType.Episode);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }
                return View(category);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ClickTayoMV()
        {
            var context = new IPTV2Entities();
            var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == GlobalConfig.ClickTayoMVEpisodeId);
            if (episode != null)
            {
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, GlobalConfig.ClickTayoMVEpisodeId, EngagementContentType.Episode);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }

                var show = episode.EpisodeCategories.FirstOrDefault(e => e.CategoryId != GlobalConfig.FreeTvCategoryId).Show;
                ViewBag.Show = show;

                ViewBag.CategoryType = "Show";
                if (show is Movie)
                    ViewBag.CategoryType = "Movie";
                else if (show is SpecialShow)
                    ViewBag.CategoryType = "SpecialShow";
                else if (show is WeeklyShow)
                    ViewBag.CategoryType = "WeeklyShow";
                else if (show is DailyShow)
                    ViewBag.CategoryType = "DailyShow";

                return View(episode);
            }
            return RedirectToAction("Index", "Home");
        }


        public ActionResult GetStream(int? id)
        {
            DateTime registDt = DateTime.Now;
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection = MyUtility.setError(ErrorCodes.UnknownError, String.Empty);
            if (!MyUtility.isUserLoggedIn())
                return Content(MyUtility.buildJson(collection), "application/json");

            var context = new IPTV2Entities();
            if (id == null)
                id = GlobalConfig.OkGoChannelId;

            var userId = new Guid(User.Identity.Name);
            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                // Check for Entitlements
                if (GlobalConfig.IsCheckForEntitlementsForLiveSpecialsEnabled)
                {
                    int categoryIds = 0;
                    int packageIds = 0;
                    //Package Entitlements
                    bool EntitlementCheck = false;
                    if (!String.IsNullOrEmpty(GlobalConfig.LiveSpecialsPackageIdsRestriction))
                    {
                        var LiveSpecialsPackageIdsRestriction = MyUtility.StringToIntList(GlobalConfig.LiveSpecialsPackageIdsRestriction);
                        packageIds = user.PackageEntitlements.Count(p => p.EndDate > registDt && LiveSpecialsPackageIdsRestriction.Contains(p.PackageId));
                        EntitlementCheck = true;
                    }

                    //Show Entitlements
                    if (!String.IsNullOrEmpty(GlobalConfig.LiveSpecialsCategoryIdsRestriction))
                    {
                        var LiveSpecialsCategoryIdsRestriction = MyUtility.StringToIntList(GlobalConfig.LiveSpecialsCategoryIdsRestriction);
                        categoryIds = user.ShowEntitlements.Count(s => s.EndDate > registDt && LiveSpecialsCategoryIdsRestriction.Contains(s.CategoryId));
                        EntitlementCheck = true;
                    }

                    if ((packageIds + categoryIds) <= 0 && !EntitlementCheck)
                    {
                        collection = MyUtility.setError(ErrorCodes.UserIsNotEntitled, String.Empty);
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }
                }
            }

            var channel = context.Channels.FirstOrDefault(c => c.ChannelId == id && c.OnlineStatusId == GlobalConfig.Visible);
            if (channel != null)
            {
                AkamaiFlowPlayerPluginClipDetails clipDetails = new AkamaiFlowPlayerPluginClipDetails();
                clipDetails.Url = MyUtility.GenerateAkamaiToken(channel.VideoStream, String.Format("{0}{1}", channel.ChannelId, Guid.NewGuid()));
                collection = MyUtility.setError(ErrorCodes.Success, clipDetails.Url);
                collection.Add("data", clipDetails);
            }
            return Content(MyUtility.buildJson(collection), "application/json");

        }
        public ActionResult GetAsset()
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection = MyUtility.setError(ErrorCodes.UnknownError, String.Empty);

            var context = new IPTV2Entities();
            Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == GlobalConfig.ClickTayoMVEpisodeId && e.OnlineStatusId == GlobalConfig.Visible);

            if (ep != null)
            {
                Asset asset = ep.PremiumAssets.FirstOrDefault().Asset;

                if (asset != null)
                {
                    int assetId = asset == null ? 0 : asset.AssetId;
                    var clipDetails = Helpers.Akamai.GetAkamaiClipDetails(ep.EpisodeId, assetId, Request, User, true);

                    if (!String.IsNullOrEmpty(clipDetails.Url))
                    {
                        collection = MyUtility.setError(ErrorCodes.Success, clipDetails.Url);
                        collection.Add("data", clipDetails);
                    }
                    else
                    {
                        collection = MyUtility.setError(ErrorCodes.VideoNotFound, "Asset url is not available.");
                    }
                }
                else
                {
                    collection = MyUtility.setError(ErrorCodes.VideoNotFound, "Asset url is not available.");
                }
            }
            else
            {
                collection = MyUtility.setError(ErrorCodes.VideoNotFound, "Episode is not available.");
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }


        public ActionResult HimigHandog(int? id)
        {
            var profiler = MiniProfiler.Current;
            var context = new IPTV2Entities();
            EpisodeCategory category;
            if (id == null)
                category = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.HimigHandogCategoryId).OrderBy(e => Guid.NewGuid()).FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible);
            else
                category = context.EpisodeCategories1.FirstOrDefault(e => e.CategoryId == GlobalConfig.HimigHandogCategoryId && e.EpisodeId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible);

            if (category != null)
            {
                bool isUserEntitled = false;
                ViewBag.Loved = false;
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, category.EpisodeId, EngagementContentType.Episode);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }

                //CHECK USER IF CAN PLAY VIDEO
                using (profiler.Step("Check if User is Entitled"))
                {
                    try
                    {
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var premiumAsset = category.Episode.PremiumAssets.FirstOrDefault();
                        if (premiumAsset != null)
                        {
                            var asset = premiumAsset.Asset;
                            //isUserEntitled = user.IsEpisodeEntitled(offering, episode, asset, RightsType.Online);
                            //isUserEntitled = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            isUserEntitled = ContextHelper.CanPlayVideo(context, offering, category.Episode, asset, User, Request);
                        }
                    }
                    catch (Exception) { }
                }

                ViewBag.IsUserEntitled = isUserEntitled;
                return View(category);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Share(string url, int? id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection = MyUtility.setError(ErrorCodes.UnknownError, String.Empty);
            if (!MyUtility.isUserLoggedIn())
                return Content(MyUtility.buildJson(collection), "application/json");

            var context = new IPTV2Entities();
            if (id == null)
                id = GlobalConfig.ClickTayoMVEpisodeId;

            var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
            if (episode != null)
            {
                System.Guid userId = new Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    string title = String.Empty;
                    var category = episode.EpisodeCategories.FirstOrDefault(e => e.CategoryId != GlobalConfig.FreeTvCategoryId);
                    if (category != null)
                        title = category.Show.Description;
                    List<ActionLink> actionlinks = new List<ActionLink>();
                    actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text, href = url });
                    List<MediaItem> mediaItems = new List<MediaItem>();
                    string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format(SNSTemplates.watch_mediaitem_src, GlobalConfig.EpisodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
                    mediaItems.Add(new MediaItem() { type = SNSTemplates.watch_mediaitem_type, src = img, href = url });

                    UserAction action = new UserAction()
                    {
                        userMessage = "Saan ka man sa mundo Click Tayo! #ClickTayo",
                        title = title,
                        subtitle = GlobalConfig.baseUrl,
                        linkBack = url,
                        description = episode.Synopsis,
                        actionLinks = actionlinks,
                        mediaItems = mediaItems,
                        actorUID = User.Identity.Name
                    };

                    if (id != GlobalConfig.ClickTayoMVEpisodeId)
                    {
                        action.userMessage = "I just had my share of Halo-Halo Clicks!";
                        action.title = episode.EpisodeName;
                    }
                    try
                    {
                        if (episode.EpisodeCategories.Count(c => c.CategoryId == GlobalConfig.KwentoNgPaskoCategoryId) > 0)
                            action.userMessage = "Dumarami ang mga tala ngayong Pasko sa TFC.tv!";
                    }
                    catch (Exception) { }
                    try
                    {
                        if (episode.EpisodeCategories.Count(c => c.CategoryId == GlobalConfig.HimigHandogCategoryId) > 0)
                        {
                            action.userMessage = "I just sang along with this Himig Handog song!";
                            action.title = episode.EpisodeName;
                        }
                    }
                    catch (Exception) { }

                    var userData = MyUtility.GetUserPrivacySetting(user.UserId);

                    var privacy = "public";
                    if (userData.IsProfilePrivate.Contains("true"))
                        privacy = "private";
                    if (userData.IsExternalSharingEnabled.Contains("true"))
                        GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "external", privacy);
                    action.description = String.Empty;
                    if (userData.IsInternalSharingEnabled.Contains("true"))
                    {
                        action.userMessage = String.Format("has watched {0}", episode.EpisodeName);
                        return Content(GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "internal", privacy), "application/json");
                    }

                    collection = MyUtility.setError(ErrorCodes.Success);

                }
            }
            return this.Json(collection, JsonRequestBehavior.AllowGet);
        }


        public ActionResult BeCarefulWithMyHeart()
        {
            //DateTime registDt = DateTime.Now;
            //var context = new IPTV2Entities();
            //var channel = context.Channels.FirstOrDefault(c => c.ChannelId == GlobalConfig.BCWMHChannelId && c.OnlineStatusId == GlobalConfig.Visible);
            //if (channel != null)
            //{
            //    if (channel.OnlineStartDate > registDt)
            //        return RedirectToAction("Index", "Home");
            //    if (channel.OnlineEndDate < registDt)
            //        return RedirectToAction("Index", "Home");
            //    if (MyUtility.isUserLoggedIn())
            //    {
            //        ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, GlobalConfig.BCWMHChannelId, EngagementContentType.Channel);
            //        System.Guid userId = new System.Guid(User.Identity.Name);
            //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
            //        if (user != null)
            //            ViewBag.EmailAddress = user.EMail;
            //    }
            //    ViewBag.CategoryType = "Live";
            //    return View(channel);
            //}
            return RedirectToAction("Index", "Home");
        }


        public ActionResult LiveSpecials()
        {
            if (GlobalConfig.IsLiveStreamRestrictionCheckEnabled)
            {
                try
                {
                    string ip = Request.GetUserHostAddressFromCloudflare();
                    var location = MyUtility.getLocation(ip);
                    var LiveStreamRestrictedCountries = GlobalConfig.LiveStreamRestrictedCountries.Split(',');
                    if (LiveStreamRestrictedCountries.Contains(location.countryCode))
                        return RedirectToAction("Index", "Home");
                }
                catch (Exception) { }
            }


            DateTime registDt = DateTime.Now;
            ViewBag.Loved = false;
            var context = new IPTV2Entities();
            var channel = context.Channels.FirstOrDefault(c => c.ChannelId == GlobalConfig.LiveStreamSpecialChannelId && c.OnlineStatusId == GlobalConfig.Visible);
            if (channel != null)
            {
                if (channel.OnlineStartDate > registDt)
                    return RedirectToAction("Index", "Home");
                if (channel.OnlineEndDate < registDt)
                    return RedirectToAction("Index", "Home");
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, GlobalConfig.LiveStreamSpecialChannelId, EngagementContentType.Channel);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;

                }
                ViewBag.CategoryType = "Live";
                return View(channel);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Concerts()
        {
            try
            {
                var context = new IPTV2Entities();
                var registDt = DateTime.Now;
                var profiler = MiniProfiler.Current;
                var cache = DataCache.Cache;
                Category category = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.ConcertsCategoryId && c.StatusId == GlobalConfig.Visible);

                if (category != null)
                {
                    ViewBag.PageDescription = category.Blurb;
                    ViewBag.MainCategory = category;
                    var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                    SortedSet<int> showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode(), category);
                    if (showIds.Count() == 0)
                        return RedirectToAction("Index", "Home");

                    int[] setofShows = showIds.ToArray();
                    var list = context.CategoryClasses.Where(c => setofShows.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible).OrderBy(c => c.StartDate).ThenBy(c => c.CategoryName).ToList();

                    //string cacheKey = "CONCERTSDISP:O:" + GlobalConfig.ConcertsCategoryId.ToString() + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                    //List<CategoryShowListDisplay> catList = (List<CategoryShowListDisplay>)cache[cacheKey];
                    //if (catList == null)
                    //{                        
                    //    catList = new List<CategoryShowListDisplay>();
                    //    using (profiler.Step("List show for the Concerts Category Id"))
                    //    {
                    //        foreach (var item in list)
                    //        {                             
                    //            catList.Add(new CategoryShowListDisplay
                    //            {
                    //                CategoryId = item.CategoryId,
                    //                Description = item.CategoryName,
                    //                ImagePoster = item.ImagePoster,
                    //                AiredDate = item.StartDate
                    //            });
                    //        }
                    //    }
                    //    cache.Put(cacheKey, catList, DataCache.CacheDuration);
                    //}
                    return View(list);
                }
                else
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception) { return RedirectToAction("Index", "Home"); }

        }


        public ActionResult TFCkat(int? id)
        {
            var profiler = MiniProfiler.Current;
            var context = new IPTV2Entities();
            EpisodeCategory category;

            bool redirectPerma = false;

            if (id == null)
            {
                category = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.TFCkatCategoryId).OrderBy(e => Guid.NewGuid()).FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible);
                redirectPerma = true;
            }
            else
                category = context.EpisodeCategories1.FirstOrDefault(e => (e.CategoryId == GlobalConfig.TFCkatExclusivesCategoryId || e.CategoryId == GlobalConfig.TFCkatCategoryId) && e.EpisodeId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible);

            if (category != null)
            {
                if (redirectPerma)
                    return RedirectToActionPermanent("TFCkat", new { id = category.EpisodeId });

                ViewBag.Loved = false;
                bool isUserEntitled = false;
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, category.EpisodeId, EngagementContentType.Episode);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }

                //CHECK USER IF CAN PLAY VIDEO
                using (profiler.Step("Check if User is Entitled"))
                {
                    try
                    {
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var premiumAsset = category.Episode.PremiumAssets.FirstOrDefault();
                        if (premiumAsset != null)
                        {
                            var asset = premiumAsset.Asset;
                            isUserEntitled = ContextHelper.CanPlayVideo(context, offering, category.Episode, asset, User, Request);
                        }
                    }
                    catch (Exception) { }
                }

                ViewBag.IsUserEntitled = isUserEntitled;

                return View(category);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult KapamilyaChat()
        {
            int id = GlobalConfig.KapamilyaChatLiveEventEpisodeId;

            var profiler = MiniProfiler.Current;
            var context = new IPTV2Entities();

            Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
            if (episode == null)
                return RedirectToAction("Index", "Home");

            //Check if episode is a Live Event
            //if (episode.IsLiveChannelActive != true)
            //    return RedirectToAction("Index", "Home");

            DateTime registDt = DateTime.Now;

            if (episode.OnlineStartDate > registDt)
                return RedirectToAction("Index", "Home");
            if (episode.OnlineEndDate < registDt)
                return RedirectToAction("Index", "Home");

            bool isUserEntitled = false;

            EpisodeCategory category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && e.Show is LiveEvent);
            if (category == null)
                category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible);

            if (category != null)
            {
                ViewBag.Show = category.Show;
                if (MyUtility.isUserLoggedIn())
                {
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }

                //CHECK USER IF CAN PLAY VIDEO
                using (profiler.Step("Check if User is Entitled"))
                {
                    try
                    {
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var premiumAsset = episode.PremiumAssets.FirstOrDefault();
                        if (premiumAsset != null)
                        {
                            var assetTemp = premiumAsset.Asset;
                            isUserEntitled = ContextHelper.CanPlayVideo(context, offering, episode, assetTemp, User, Request);
                        }
                    }
                    catch (Exception) { }
                }

                ViewBag.IsUserEntitled = isUserEntitled;
                ViewBag.CategoryType = "Show";
                if (category.Show is Movie)
                    ViewBag.CategoryType = "Movie";
                else if (category.Show is SpecialShow)
                    ViewBag.CategoryType = "SpecialShow";
                else if (category.Show is WeeklyShow)
                    ViewBag.CategoryType = "WeeklyShow";
                else if (category.Show is DailyShow)
                    ViewBag.CategoryType = "DailyShow";
                else if (category.Show is LiveEvent)
                    ViewBag.CategoryType = "LiveEvent";

                ViewBag.ShowId = category.Show.CategoryId;
                ViewBag.EpisodeId = episode.EpisodeId;

                Asset asset = episode.PremiumAssets.FirstOrDefault().Asset;
                int assetId = asset == null ? 0 : asset.AssetId;

                ViewBag.AssetId = assetId;
                //ViewBag.VideoUrl = Helpers.Akamai.GetVideoUrl(episode.EpisodeId, assetId, Request, User);

                using (profiler.Step("Has Social Love"))
                {
                    if (MyUtility.isUserLoggedIn())
                        ViewBag.Loved = ContextHelper.HasSocialEngagement(new Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, episode.EpisodeId, EngagementContentType.Episode);
                }
                using (profiler.Step("Has Social Rating"))
                {
                    if (MyUtility.isUserLoggedIn())
                        ViewBag.Rated = ContextHelper.HasSocialEngagement(new Guid(User.Identity.Name), GlobalConfig.SOCIAL_RATING, episode.EpisodeId, EngagementContentType.Episode);
                }

                /**** Check for Free Trial ****/
                bool showFreeTrialImage = false;
                using (profiler.Step("Check for Early Bird"))
                {
                    if (GlobalConfig.IsEarlyBirdEnabled)
                    {
                        if (MyUtility.isUserLoggedIn())
                        {

                            var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
                            if (user != null)
                            {
                                var offering = context.Offerings.Find(GlobalConfig.offeringId);
                                if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                                    showFreeTrialImage = true;
                            }
                            //showFreeTrialImage = true;
                        }
                    }
                }
                ViewBag.ShowFreeTrialImage = showFreeTrialImage;
                return View(episode);
            }
            else
                return RedirectToAction("Index", "Home");
        }

        public ActionResult PremiumTrial(string email)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "Promo",
                TransactionType = "Premium Trial"
            };
            if (String.IsNullOrEmpty(email))
                ReturnCode.StatusMessage = "You are missing some required information.";

            var registDt = DateTime.Now;
            try
            {
                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, email, true) == 0);
                if (user != null)
                {
                    var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.PremiumTrialPromoId && p.StartDate < registDt && p.EndDate > registDt && p.StatusId == GlobalConfig.Visible);
                    if (promo != null)
                    {
                        var userpromo = context.UserPromos.FirstOrDefault(s => s.UserId == user.UserId && s.PromoId == GlobalConfig.PremiumTrialPromoId);
                        if (userpromo != null)
                        {
                            if (userpromo.AuditTrail.UpdatedOn == null)
                            {
                                PaymentHelper.PayViaWallet(context, user.UserId, GlobalConfig.PremiumTrialPromoProductId, SubscriptionProductType.Package, user.UserId, null);
                                userpromo.AuditTrail.UpdatedOn = registDt;
                                context.SaveChanges();
                                ReturnCode.StatusHeader = "Your FREE Premium Subscription starts now!";
                                ReturnCode.StatusMessage = "Pwede ka nang manood ng mga piling Kapamilya shows at movies. Visit your My Entitlements Page to see your free entitlement.";
                            }
                            else
                            {
                                ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                                ReturnCode.StatusMessage = "You have already claimed your free entitlement.";
                            }
                        }
                        else
                        {
                            ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                            ReturnCode.StatusMessage = "You are not eligible for this promo.";
                        }
                    }
                    else
                    {
                        ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                        ReturnCode.StatusMessage = "The promo has already ended.";
                    }
                }
                else
                {
                    ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                    ReturnCode.StatusMessage = String.Format("{0} does not exist.", email);
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                ReturnCode.StatusMessage = e.Message;
            }
            if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
            {
                TempData["ErrorMessage"] = ReturnCode;
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
