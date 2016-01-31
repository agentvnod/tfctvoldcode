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
    public class PacMayController : Controller
    {
        //
        // GET: /PacMay/

        [RequireHttp]
        public ActionResult Index(string uat)
        {
            int id = GlobalConfig.PacMayTVCEpisodeId;
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
            if (GlobalConfig.isUAT)
                if (!String.IsNullOrEmpty(uat))
                    if (String.Compare(uat, "true", true) == 0)
                        CountryCode = "AE";
            string coverImg = String.Format("{0}/content/images/banners/OneforPacman_BannerREV.jpg", GlobalConfig.AssetsBaseUrl);
            bool isME = false;
            var MEPacMayCountryCodeList = GlobalConfig.MEPacMayAllowedCountryCodes.Split(',');

            try
            {
                var context = new IPTV2Entities();
                if (User.Identity.IsAuthenticated)
                {
                    var UserId = new Guid(User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                    if (user != null)
                    {
                        var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                        if (allowedEmails.Contains(user.EMail.ToLower()))
                            CountryCode = user.CountryCode;
                    }
                }
                if (MEPacMayCountryCodeList.Contains(CountryCode) || MyUtility.isSearchSpider(Request))
                {
                    coverImg = String.Format("{0}/content/images/banners/Pac_May_TFCtv_MEbanner.jpg", GlobalConfig.AssetsBaseUrl);
                    isME = true;
                    var liveShow = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.PacMayLiveCategoryId);
                    if (liveShow != null)
                    {
                        if (liveShow.StatusId == GlobalConfig.Visible && liveShow.StartDate < registDt && liveShow.EndDate > registDt)
                        {
                            try
                            {
                                if (User.Identity.IsAuthenticated)
                                {
                                    var UserId = new Guid(User.Identity.Name);
                                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                    if (user != null)
                                    {
                                        CountryCode = user.CountryCode;
                                        if (ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, liveShow.CategoryId, registDt, CountryCode).HasSubscription)
                                        {
                                            var episodePC = context.Episodes.FirstOrDefault(e => e.EpisodeId == GlobalConfig.PacMayLiveEpisodeId);
                                            if (episodePC != null)
                                            {
                                                if (episodePC.OnlineStatusId == GlobalConfig.Visible && episodePC.OnlineStartDate < registDt && episodePC.OnlineEndDate > registDt)
                                                    id = GlobalConfig.PacMayLiveEpisodeId;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                    var vodShow = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.PacMayVODShowId);
                    if (vodShow != null)
                    {
                        if (vodShow.StatusId == GlobalConfig.Visible && vodShow.StartDate < registDt && vodShow.EndDate > registDt)
                        {
                            var episodeVOD = context.Episodes.FirstOrDefault(e => e.EpisodeId == GlobalConfig.PacMayVODEpisodeId);
                            if (episodeVOD != null)
                            {
                                if (episodeVOD.OnlineStatusId == GlobalConfig.Visible && episodeVOD.OnlineStartDate < registDt && episodeVOD.OnlineEndDate > registDt)
                                    id = GlobalConfig.PacMayVODEpisodeId;
                            }
                        }
                        //id = GlobalConfig.PacMayVODEpisodeId;
                    }
                }

                ViewBag.isME = isME;
                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                if (episode != null)
                {
                    if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                    {
                        SortedSet<int> parentCategories;
                        using (profiler.Step("Check for Parent Shows"))
                        {
                            var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                            parentCategories = episode.GetParentShows(CacheDuration);
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

                            var tempShowNameWithDate = episode.Description;

                            bool isEmailAllowed = false;
                            string UserCountryCode = CountryCode;

                            if (User.Identity.IsAuthenticated)
                            {
                                var UserId = new Guid(User.Identity.Name);
                                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                if (user != null)
                                {
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
                                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, GlobalConfig.PacMaySubscribeCategoryId, registDt);
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
                            using (profiler.Step("Check for preview asset"))
                            {
                                var cdnIds = MyUtility.StringToIntList("2,6");
                                var previewAsset = episode.PreviewAssets.LastOrDefault();
                                if (previewAsset != null)
                                {
                                    Asset asset = previewAsset.Asset;
                                    if (asset != null)
                                        if (asset.AssetCdns != null)
                                            if (asset.AssetCdns.Count(a => cdnIds.Contains(a.CdnId)) > 0)
                                                ViewBag.HasPreviewAsset = true;
                                }
                            }
                            if (!ContextHelper.IsCategoryViewableInUserCountry(episodeCategory.Show, CountryCode))
                                if (!isEmailAllowed)
                                    return RedirectToAction("Index", "Home");

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
                            ViewBag.coverImg = coverImg;
                            return View(episode);
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }
        public PartialViewResult BuildSectionShow(int id, string sectionTitle, string containerId, int? pageSize = 4, int page = 0, string featureType = "episode", bool removeShowAll = true, bool isFeature = false, bool ShowAllItems = false, string partialViewName = "")
        {
            List<HomepageFeatureItem> obj = new List<HomepageFeatureItem>();
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            int size = GlobalConfig.FeatureItemsPageSize;
            try
            {

                ViewBag.FeatureType = featureType;
                ViewBag.SectionTitle = sectionTitle;
                ViewBag.id = id;
                ViewBag.containerId = containerId;
                ViewBag.pageSize = size;
                ViewBag.RemoveShowAll = removeShowAll;
                ViewBag.IsFeature = isFeature;
                string assetBaseUrl = GlobalConfig.AssetsBaseUrl;
                string episodeImgPath = GlobalConfig.EpisodeImgPath;
                string showImgPath = GlobalConfig.ShowImgPath;
                string celebrityImgPath = GlobalConfig.CelebrityImgPath;
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                var show = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
                if (pageSize != null)
                    size = (int)pageSize;

                if (show != null)
                {
                    if (show is Show)
                    {

                        string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                        var MEPacMayCountryCodeList = GlobalConfig.MEPacMayAllowedCountryCodes.Split(',');
                        bool IncludeTVC = MEPacMayCountryCodeList.Contains(CountryCode);

                        //Get all episode numbers
                        var episode_list = context.EpisodeCategories1.Where(e => e.Show.CategoryId == ((Show)show).CategoryId).Select(e => e.EpisodeId);
                        //var episodes = ((Show)show).Episodes.Where(e => e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderByDescending(e => e.Episode.DateAired).Select((item, index) => new { Item = item, Position = index }).ToList();
                        var episodes = context.Episodes.Where(e => episode_list.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.IsLiveChannelActive).OrderByDescending(e => e.DateAired).ThenByDescending(e => e.EpisodeId).Take(size);
                        ViewBag.sCount = episodes.Count();
                        int epCount = episodes.Count();
                        foreach (var episode in episodes)
                        {

                            string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? assetBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", episodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
                            string showImg = String.IsNullOrEmpty(show.ImagePoster) ? assetBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", showImgPath, show.CategoryId.ToString(), show.ImagePoster);
                            string EpLength = "";
                            if (!(episode.EpisodeLength == null))
                            {
                                TimeSpan span = new TimeSpan(0, 0, Convert.ToInt32(episode.EpisodeLength) * 60);
                                EpLength = String.Format("{0}:{1}:{2}", span.Hours.ToString().PadLeft(2, '0'), span.Minutes.ToString().PadLeft(2, '0'), span.Seconds.ToString().PadLeft(2, '0'));
                            }
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

                            if (episode.EpisodeId == GlobalConfig.PacMayTVCEpisodeId)
                            {
                                if (IncludeTVC)
                                    obj.Add(j);
                            }
                            else
                                obj.Add(j);
                            epCount--;
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, obj);
            return PartialView(obj);
        }
    }
}
