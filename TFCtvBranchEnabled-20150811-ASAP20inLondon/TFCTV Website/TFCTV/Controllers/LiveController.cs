using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;
using StackExchange.Profiling;

namespace TFCTV.Controllers
{
    public class LiveController : Controller
    {
        //
        // GET: /Live/

        public ActionResult Index(string id)
        {
            //if (String.IsNullOrEmpty(id))
            //    return RedirectToAction("Index", "Live", new { id = "TVChannels" });
            //else if (String.Compare(id, "TVChannels", true) == 0 || String.Compare(id, "TVPrograms", true) == 0)
            //{
            //    var context = new IPTV2Entities();
            //    var channels = context.Channels.Where(c => c.ChannelName.Contains(id) && c.Deleted == false);
            //    ViewBag.ChannelTitle = id;
            //    return View(channels.ToList());
            //}
            //else
            //    return RedirectToAction("Index", "Live", new { id = "TVChannels" });

            return RedirectToActionPermanent("Live", "Category");
            //return RedirectToAction("List", "Category", new { id = 2097 });

            //return RedirectToAction("Details", "Channel", new { id = 20 });
            //return View();
        }

        //public ActionResult Details(int? id, string slug)
        //{
        //    if (id == null)
        //        return RedirectToAction("Index", "Home");

        //    var profiler = MiniProfiler.Current;
        //    var context = new IPTV2Entities();

        //    Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
        //    if (episode == null)
        //        return RedirectToAction("Index", "Home");

        //    //Check if episode is a Live Event
        //    if (episode.IsLiveChannelActive != true)
        //        return RedirectToAction("Index", "Home");

        //    DateTime registDt = DateTime.Now;

        //    if (episode.OnlineStartDate > registDt)
        //        return RedirectToAction("Index", "Home");
        //    if (episode.OnlineEndDate < registDt)
        //        return RedirectToAction("Index", "Home");


        //    var dbSlug = MyUtility.GetSlug(episode.EpisodeName);
        //    if (String.Compare(dbSlug, slug, false) != 0)
        //        return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });

        //    bool isUserEntitled = false;

        //    EpisodeCategory category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && e.Show is LiveEvent);
        //    if (category == null)
        //        category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible);

        //    if (category != null)
        //    {
        //        ViewBag.Show = category.Show;
        //        if (MyUtility.isUserLoggedIn())
        //        {
        //            System.Guid userId = new System.Guid(User.Identity.Name);
        //            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //            if (user != null)
        //                ViewBag.EmailAddress = user.EMail;
        //        }

        //        //CHECK USER IF CAN PLAY VIDEO
        //        using (profiler.Step("Check if User is Entitled"))
        //        {
        //            try
        //            {
        //                var offering = context.Offerings.Find(GlobalConfig.offeringId);
        //                var premiumAsset = episode.PremiumAssets.FirstOrDefault();
        //                if (premiumAsset != null)
        //                {
        //                    var assetTemp = premiumAsset.Asset;
        //                    isUserEntitled = ContextHelper.CanPlayVideo(context, offering, episode, assetTemp, User, Request);
        //                }
        //            }
        //            catch (Exception) { }
        //        }

        //        ViewBag.IsUserEntitled = isUserEntitled;
        //        ViewBag.CategoryType = "Show";
        //        if (category.Show is Movie)
        //            ViewBag.CategoryType = "Movie";
        //        else if (category.Show is SpecialShow)
        //            ViewBag.CategoryType = "SpecialShow";
        //        else if (category.Show is WeeklyShow)
        //            ViewBag.CategoryType = "WeeklyShow";
        //        else if (category.Show is DailyShow)
        //            ViewBag.CategoryType = "DailyShow";
        //        else if (category.Show is LiveEvent)
        //            ViewBag.CategoryType = "LiveEvent";

        //        ViewBag.ShowId = category.Show.CategoryId;
        //        ViewBag.EpisodeId = episode.EpisodeId;

        //        Asset asset = episode.PremiumAssets.FirstOrDefault().Asset;
        //        int assetId = asset == null ? 0 : asset.AssetId;

        //        ViewBag.AssetId = assetId;
        //        ViewBag.VideoUrl = Helpers.Akamai.GetVideoUrl(episode.EpisodeId, assetId, Request, User);

        //        using (profiler.Step("Has Social Love"))
        //        {
        //            if (MyUtility.isUserLoggedIn())
        //                ViewBag.Loved = ContextHelper.HasSocialEngagement(new Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, episode.EpisodeId, EngagementContentType.Episode);
        //        }
        //        using (profiler.Step("Has Social Rating"))
        //        {
        //            if (MyUtility.isUserLoggedIn())
        //                ViewBag.Rated = ContextHelper.HasSocialEngagement(new Guid(User.Identity.Name), GlobalConfig.SOCIAL_RATING, episode.EpisodeId, EngagementContentType.Episode);
        //        }

        //        /**** Check for Free Trial ****/
        //        bool showFreeTrialImage = false;
        //        using (profiler.Step("Check for Early Bird"))
        //        {
        //            if (GlobalConfig.IsEarlyBirdEnabled)
        //            {
        //                if (MyUtility.isUserLoggedIn())
        //                {

        //                    var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
        //                    if (user != null)
        //                    {
        //                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
        //                        if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
        //                            showFreeTrialImage = true;
        //                    }
        //                    showFreeTrialImage = true;
        //                }
        //            }
        //        }
        //        ViewBag.ShowFreeTrialImage = showFreeTrialImage;
        //        return View(episode);
        //    }
        //    else
        //        return RedirectToAction("Index", "Home");
        //}


        //public ActionResult DetailsBAK(int? id, string slug)
        //{
        //    //int id = GlobalConfig.KapamilyaChatLiveEventEpisodeId;
        //    if (id == null)
        //        return RedirectToAction("Index", "Home");
        //    if (id == GlobalConfig.UAAPLiveStreamEpisodeId)
        //        return RedirectToAction("Live", "UAAP");
        //    var profiler = MiniProfiler.Current;
        //    var context = new IPTV2Entities();

        //    Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.OnlineStatusId == GlobalConfig.Visible);
        //    if (episode == null)
        //        return RedirectToAction("Index", "Home");

        //    //Check if episode is a Live Event
        //    if (episode.IsLiveChannelActive != true)
        //        return RedirectToAction("Index", "Home");

        //    DateTime registDt = DateTime.Now;

        //    if (episode.OnlineStartDate > registDt)
        //        return RedirectToAction("Index", "Home");
        //    if (episode.OnlineEndDate < registDt)
        //        return RedirectToAction("Index", "Home");

        //    var dbSlug = MyUtility.GetSlug(episode.EpisodeName);
        //    if (String.Compare(dbSlug, slug, false) != 0)
        //        return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });

        //    bool isUserEntitled = false;
        //    string CoverItLiveAltCastCode = String.Empty;

        //    var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
        //    EpisodeCategory category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && e.Show is LiveEvent && !excludedCategoryIds.Contains(e.CategoryId));
        //    if (category == null)
        //        category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && !excludedCategoryIds.Contains(e.CategoryId));

        //    if (category != null)
        //    {
        //        ViewBag.Show = category.Show;

        //        if (!ContextHelper.IsCategoryViewableInUserCountry(category.Show))
        //            return RedirectToAction("Index", "Home");

        //        if (MyUtility.isUserLoggedIn())
        //        {
        //            System.Guid userId = new System.Guid(User.Identity.Name);
        //            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //            if (user != null)
        //                ViewBag.EmailAddress = user.EMail;
        //        }

        //        //CHECK USER IF CAN PLAY VIDEO
        //        using (profiler.Step("Check if User is Entitled"))
        //        {
        //            try
        //            {
        //                var offering = context.Offerings.Find(GlobalConfig.offeringId);
        //                //Make sure that first asset is always the livestreaming asset.
        //                var premiumAsset = episode.PremiumAssets.OrderBy(p => p.Priority).FirstOrDefault();
        //                if (premiumAsset != null)
        //                {
        //                    var assetTemp = premiumAsset.Asset;
        //                    isUserEntitled = ContextHelper.CanPlayVideo(context, offering, episode, assetTemp, User, Request);
        //                    var CoverItLiveCdn = assetTemp.AssetCdns.FirstOrDefault(a => a.CdnId == GlobalConfig.CoverItLiveCdnId);
        //                    CoverItLiveAltCastCode = CoverItLiveCdn.CdnReference;
        //                }
        //            }
        //            catch (Exception) { }
        //        }

        //        ViewBag.CoverItLiveAltCastCode = CoverItLiveAltCastCode;
        //        ViewBag.IsUserEntitled = isUserEntitled;
        //        ViewBag.CategoryType = "Show";
        //        if (category.Show is Movie)
        //            ViewBag.CategoryType = "Movie";
        //        else if (category.Show is SpecialShow)
        //            ViewBag.CategoryType = "SpecialShow";
        //        else if (category.Show is WeeklyShow)
        //            ViewBag.CategoryType = "WeeklyShow";
        //        else if (category.Show is DailyShow)
        //            ViewBag.CategoryType = "DailyShow";
        //        else if (category.Show is LiveEvent)
        //            ViewBag.CategoryType = "LiveEvent";

        //        ViewBag.ShowId = category.Show.CategoryId;
        //        ViewBag.EpisodeId = episode.EpisodeId;

        //        Asset asset = episode.PremiumAssets.FirstOrDefault().Asset;
        //        int assetId = asset == null ? 0 : asset.AssetId;

        //        ViewBag.AssetId = assetId;
        //        //ViewBag.VideoUrl = Helpers.Akamai.GetVideoUrl(episode.EpisodeId, assetId, Request, User);

        //        using (profiler.Step("Has Social Love"))
        //        {
        //            if (MyUtility.isUserLoggedIn())
        //                ViewBag.Loved = ContextHelper.HasSocialEngagement(new Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, episode.EpisodeId, EngagementContentType.Episode);
        //        }
        //        using (profiler.Step("Has Social Rating"))
        //        {
        //            if (MyUtility.isUserLoggedIn())
        //                ViewBag.Rated = ContextHelper.HasSocialEngagement(new Guid(User.Identity.Name), GlobalConfig.SOCIAL_RATING, episode.EpisodeId, EngagementContentType.Episode);
        //        }

        //        /**** Check for Free Trial ****/
        //        bool showFreeTrialImage = false;
        //        using (profiler.Step("Check for Early Bird"))
        //        {
        //            if (GlobalConfig.IsEarlyBirdEnabled)
        //            {
        //                if (MyUtility.isUserLoggedIn())
        //                {
        //                    var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
        //                    if (user != null)
        //                    {
        //                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
        //                        if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
        //                            showFreeTrialImage = true;
        //                    }
        //                    //showFreeTrialImage = true;
        //                }
        //            }
        //        }
        //        ViewBag.ShowFreeTrialImage = showFreeTrialImage;

        //        ViewBag.ShowPackageProductPrices = null;
        //        string countryCode = MyUtility.GetCurrentCountryCodeOrDefault();
        //        ViewBag.CountryCode = countryCode;
        //        if (!isUserEntitled)
        //        {
        //            using (profiler.Step("Get Show Package & Product Prices"))
        //            {
        //                try
        //                {
        //                    ViewBag.ShowPackageProductPrices = ContextHelper.GetShowPackageProductPrices(category.Show.CategoryId, countryCode);
        //                }
        //                catch (Exception e) { MyUtility.LogException(e); }
        //            }
        //        }
        //        return View(episode);
        //    }
        //    else
        //        return RedirectToAction("Index", "Home");
        //}

        [RequireHttp]
        public ActionResult Details(int? id, string slug)
        {
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            try
            {
                if (id != null)
                {
                    if (id == GlobalConfig.UAAPLiveStreamEpisodeId)
                        return RedirectToAction("Live", "UAAP");
                    else if (id == GlobalConfig.BCWMHWeddingLiveEventEpisodeId)
                        return RedirectToAction("Index", "Wedding");
                    else if (id == GlobalConfig.PBBLiveEventEpisodeId)
                        return RedirectPermanent("/pinoy-big-brother");
                    else if (id == GlobalConfig.ProjectAirEpisodeId)
                        return RedirectPermanent("/WatchNow");

                    var context = new IPTV2Entities();
                    var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                    if (episode != null)
                    {
                        if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                        {
                            var dbSlug = MyUtility.GetSlug(episode.Description);
                            if (episode.IsLiveChannelActive != true)
                                return RedirectToActionPermanent("Details", "Episode", new { id = id, slug = dbSlug });

                            SortedSet<int> parentCategories;
                            using (profiler.Step("Check for Parent Shows"))
                            {
                                var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                                parentCategories = episode.GetParentShows(CacheDuration);
                                if (parentCategories.Count() > 0)
                                {
                                    if (parentCategories.Contains(GlobalConfig.HalalanNewsAlertsParentCategoryId))
                                        return RedirectToActionPermanent("NewsAlerts", "Halalan2013", new { id = id, slug = dbSlug });
                                    else if (parentCategories.Contains(GlobalConfig.HalalanAdvisoriesParentCategoryId))
                                        return RedirectToActionPermanent("Advisories", "Halalan2013", new { id = id, slug = dbSlug });
                                    else if (parentCategories.Contains(GlobalConfig.TFCkatCategoryId))
                                        return RedirectToActionPermanent("OnDemand", "TFCkat", new { id = id, slug = dbSlug });
                                    else if (parentCategories.Contains(GlobalConfig.UAAPGreatnessNeverEndsCategoryId))
                                        return RedirectToActionPermanent("OnDemand", "UAAP", new { id = id, slug = dbSlug });
                                }
                            }

                            var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                            var episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                            if (episodeCategory != null)
                            {
                                if (episodeCategory.Show.EndDate < registDt || episodeCategory.Show.StatusId != GlobalConfig.Visible)
                                    return RedirectToAction("Index", "Home");

                                ViewBag.Loved = false; ViewBag.Rated = false; // Social
                                ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                                ViewBag.IsAllowedToViewABSCBNFreeLiveStream = false; // ABS-CBN Free Live Stream

                                //var tempShowNameWithDate = String.Format("{0} {1}", episodeCategory.Show.Description, episode.DateAired.Value.ToString("MMM dd, yyyy"));
                                var tempShowNameWithDate = episode.Description;
                                dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
                                if (String.Compare(dbSlug, slug, false) != 0)
                                    return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });
                                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                                bool isEmailAllowed = false;
                                string UserCountryCode = CountryCode;

                                if (User.Identity.IsAuthenticated)
                                {
                                    var UserId = new Guid(User.Identity.Name);
                                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                    if (user != null)
                                    {
                                        if (MyUtility.IsDuplicateSession(user, Request.Cookies[System.Web.Security.FormsAuthentication.FormsCookieName]))
                                        {
                                            System.Web.Security.FormsAuthentication.SignOut();
                                            return RedirectToAction("ConcurrentLogin", "Home");
                                        }

                                        //CountryCode = user.CountryCode; -- REMOVED. WE WILL BASE COUNTRYCODE ON IP ADDRESS
                                        UserCountryCode = user.CountryCode;
                                        ViewBag.EmailAddress = user.EMail;
                                        ViewBag.UserId = user.UserId;
                                        ViewBag.CountryCode = CountryCode;
                                        using (profiler.Step("Social Love"))
                                        {
                                            ViewBag.Loved = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_LOVE, id, EngagementContentType.Episode);
                                        }
                                        using (profiler.Step("Social Rating"))
                                        {
                                            ViewBag.Rated = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_RATING, id, EngagementContentType.Episode);
                                        }
                                        using (profiler.Step("Check for Active Entitlements")) //check for active entitlements based on categoryId
                                        {
                                            ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt);
                                        }

                                        // Get maximum date of user's entitlement
                                        if (id == GlobalConfig.ABSCBNFreeLiveStreamEpisodeId)
                                        {
                                            try
                                            {
                                                var userEntitlements = user.Entitlements.Where(e => e.OfferingId == GlobalConfig.offeringId);
                                                if (userEntitlements != null)
                                                    if (userEntitlements.Count() > 0)
                                                        if (userEntitlements.Max(e => e.EndDate) > registDt)
                                                        {
                                                            ViewBag.MaxEntitlementEndDate = userEntitlements.Max(e => e.EndDate);
                                                            ViewBag.IsAllowedToViewABSCBNFreeLiveStream = true;
                                                        }

                                            }
                                            catch (Exception) { }
                                        }

                                        var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                                        isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
                                    }
                                }
                                else
                                {
                                    using (profiler.Step("Check for Active Entitlements (Not Logged In)")) //check for active entitlements based on categoryId
                                    {
                                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
                                    }
                                }

                                if (!ContextHelper.IsCategoryViewableInUserCountry(episodeCategory.Show, CountryCode))
                                    if (!isEmailAllowed)
                                        return RedirectToAction("Index", "Home");

                                string CoverItLiveAltCastCode = String.Empty;
                                string TwitterUrl = String.Empty;
                                string TwitterWidgetId = String.Empty;
                                using (profiler.Step("Get CoverItLive Alt Cast Code"))
                                {
                                    try
                                    {
                                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                                        //Make sure that first asset is always the livestreaming asset.
                                        var premiumAsset = episode.PremiumAssets.OrderBy(p => p.Priority).FirstOrDefault();
                                        if (premiumAsset != null)
                                        {
                                            var assetTemp = premiumAsset.Asset;
                                            try
                                            {
                                                var CoverItLiveCdn = assetTemp.AssetCdns.FirstOrDefault(a => a.CdnId == GlobalConfig.CoverItLiveCdnId);
                                                if (CoverItLiveCdn != null)
                                                    CoverItLiveAltCastCode = CoverItLiveCdn.CdnReference;
                                            }
                                            catch (Exception) { }
                                            try
                                            {
                                                //Twitter Widget Details
                                                var TwitterUriCdn = assetTemp.AssetCdns.FirstOrDefault(a => a.CdnId == GlobalConfig.TwitterUriCdnId);
                                                if (TwitterUriCdn != null)
                                                    TwitterUrl = TwitterUriCdn.CdnReference;

                                                var TwitterWidgetCdn = assetTemp.AssetCdns.FirstOrDefault(a => a.CdnId == GlobalConfig.TwitterWidgetCdnId);
                                                if (TwitterWidgetCdn != null)
                                                    TwitterWidgetId = TwitterWidgetCdn.CdnReference;
                                            }
                                            catch (Exception) { }
                                        }
                                    }
                                    catch (Exception) { }
                                }

                                ViewBag.TwitterUrl = TwitterUrl;
                                ViewBag.TwitterWidgetId = TwitterWidgetId;

                                ViewBag.CoverItLiveAltCastCode = CoverItLiveAltCastCode;

                                ViewBag.Show = episodeCategory.Show;
                                ViewBag.CategoryType = "Show";
                                if (episodeCategory.Show is Movie)
                                    ViewBag.CategoryType = "Movie";
                                else if (episodeCategory.Show is SpecialShow)
                                    ViewBag.CategoryType = "SpecialShow";
                                else if (episodeCategory.Show is WeeklyShow)
                                    ViewBag.CategoryType = "WeeklyShow";
                                else if (episodeCategory.Show is DailyShow)
                                    ViewBag.CategoryType = "DailyShow";

                                //Check if video has Ios Asset
                                ViewBag.DoesEpisodeHaveIosCdnReferenceBasedOnAsset = ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOnAsset(episode);
                                ViewBag.dbSlug = dbSlug;
                                if (id == GlobalConfig.ABSCBNFreeLiveStreamEpisodeId)
                                {
                                    ViewBag.PlayHitCounterLimitReached = false;
                                    if (Request.Cookies.AllKeys.Contains("playhitcounter"))
                                    {
                                        try
                                        {
                                            int PlayHitCounter = Convert.ToInt32(Request.Cookies["playhitcounter"].Value);
                                            if (PlayHitCounter > 3)
                                                ViewBag.PlayHitCounterLimitReached = true;
                                        }
                                        catch (Exception) { }
                                    }
                                    return View("ABSCBNFreeLiveStreamView", episode);
                                }
                                ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
                                MyUtility.SetOptimizelyCookie(context);

                                try
                                {
                                    var pkg = context.PackageTypes.FirstOrDefault(p => p.PackageId == GlobalConfig.premiumId);
                                    var listOfShows = pkg.GetAllOnlineShowIds(CountryCode);
                                    bool IsPartOfPremium = listOfShows.Contains(episodeCategory.CategoryId);
                                    ViewBag.IsPartOfPremium = IsPartOfPremium;
                                }
                                catch (Exception) { }

                                //get product if it's pinoy pride
                                if (episode.EpisodeId == GlobalConfig.PinoyPride30EpisodeId)
                                {
                                    try
                                    {
                                        var product = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.PinoyPride30ProductId);
                                        if (product != null)
                                        {
                                            if (product is SubscriptionProduct)
                                            {
                                                ViewBag.PinoyPride30Product = product;
                                                var country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, UserCountryCode, true) == 0);
                                                ViewBag.PinoyPride30ProductPrice = product.ProductPrices.FirstOrDefault(p => String.Compare(country.CurrencyCode, p.CurrencyCode, true) == 0);
                                            }
                                        }
                                    }
                                    catch (Exception) { }
                                }


                                if (!Request.Cookies.AllKeys.Contains("version"))
                                {
                                    if (GlobalConfig.UseJWPlayer)
                                    {
                                        if (Request.Cookies.AllKeys.Contains("hplayer"))
                                        {
                                            if (Request.Cookies["hplayer"].Value == "2")
                                                return View("DetailsFlowHLS", episode);
                                            if (Request.Cookies["hplayer"].Value == "7")
                                                return View("DetailsJwplayer7Akamai", episode);
                                            else
                                                return View("Details2", episode);
                                        }
                                        else
                                        {
                                            return View("DetailsJwplayerAkamai", episode);
                                            //return View("Details3", episode);
                                        }
                                    }
                                    else
                                        return View("Details2", episode);
                                }
                                return View(episode);
                            }

                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }
    }
}
