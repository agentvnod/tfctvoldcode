using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Helpers;
using IPTV2_Model;
using StackExchange.Profiling;

namespace TFCTV.Controllers
{
    public class TFC20Controller : Controller
    {
        //
        // GET: /TFC20/

        [RequireHttp]
        public ActionResult Details(int? id, string slug)
        {
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            try
            {
                if (id == null)
                    id = GlobalConfig.TFC20DefaultEpisodeId;
                if (id != null)
                {
                    var context = new IPTV2Entities();
                    var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                    if (episode != null)
                    {
                        if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                        {
                            var dbSlug = MyUtility.GetSlug(episode.Description);
                            if (episode.IsLiveChannelActive == true)
                                return RedirectToActionPermanent("Details", "Live", new { id = id, slug = dbSlug });

                            SortedSet<int> parentCategories;
                            using (profiler.Step("Check for Parent Shows"))
                            {
                                var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                                parentCategories = episode.GetParentShows(CacheDuration);
                                if (parentCategories.Count() > 0)
                                    if (!parentCategories.Contains(GlobalConfig.TFC20BayaningFilipinoCategoryId))
                                        return RedirectToActionPermanent("Details", "Episode", new { id = id, slug = dbSlug });
                            }

                            string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                            EpisodeCategory episodeCategory = null;
                            if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                            {
                                var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                            }
                            else
                            {
                                var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                            }

                            if (episodeCategory != null)
                            {
                                if (episodeCategory.Show.EndDate < registDt || episodeCategory.Show.StatusId != GlobalConfig.Visible)
                                    return RedirectToAction("Index", "Home");

                                ViewBag.Loved = false; ViewBag.Rated = false; // Social
                                ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };

                                //var tempShowNameWithDate = String.Format("{0} {1}", episodeCategory.Show.Description, episode.DateAired.Value.ToString("MMM dd, yyyy"));
                                var tempShowNameWithDate = episode.Description;
                                dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
                                if (String.Compare(dbSlug, slug, false) != 0)
                                    return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });
                                bool isEmailAllowed = false;

                                if (User.Identity.IsAuthenticated)
                                {
                                    var UserId = new Guid(User.Identity.Name);
                                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                    if (user != null)
                                    {
                                        CountryCode = user.CountryCode;
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
                                    return RedirectToAction("Index", "Home");

                                string CoverItLiveAltCastCode = GlobalConfig.TFC20CoverItLiveAltCastCode;

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
                                ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
                                if (!Request.Cookies.AllKeys.Contains("version"))
                                    return View("Details2", episode);
                                return View(episode);
                            }

                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }

        public PartialViewResult GetTFC20Content(int? pageSize, int page = 0)
        {
            List<HomepageFeatureItem> jfi = null;
            List<HomepageFeatureItem> obj = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            int size = 12;
            int skipSize = 0;
            try
            {
                if (pageSize != null)
                    size = (int)pageSize;
                skipSize = size * page;
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "T20BFC:O:" + GlobalConfig.TFC20BayaningFilipinoCategoryId;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }

                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.TFC20BayaningFilipinoCategoryId).Select(e => e.EpisodeId);
                    var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId));
                    if (episodes != null)
                    {
                        jfi = new List<HomepageFeatureItem>();
                        foreach (var item in episodes)
                        {
                            if (item.OnlineStatusId == GlobalConfig.Visible && item.OnlineStartDate < registDt && item.OnlineEndDate > registDt)
                            {
                                string img = String.IsNullOrEmpty(item.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, item.EpisodeId, item.ImageAssets.ImageVideo);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    id = item.EpisodeId,
                                    description = item.Description,
                                    name = item.EpisodeName,
                                    airdate = (item.DateAired != null) ? item.DateAired.Value.ToString("MMMM d, yyyy") : "",
                                    imgurl = img,
                                    blurb = HttpUtility.HtmlEncode(item.Synopsis),
                                    slug = MyUtility.GetSlug(item.EpisodeName)
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
                if (jfi != null)
                    obj = jfi.Skip(skipSize).Take(size).ToList();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            ViewBag.pageSize = size;
            return PartialView(obj);
        }
        public PartialViewResult GetMoreTFC20Content(int? pageSize, int page = 0)
        {
            List<HomepageFeatureItem> jfi = null;
            List<HomepageFeatureItem> obj = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            int size = 12;
            int skipSize = 0;
            try
            {
                if (pageSize != null)
                    size = (int)pageSize;
                skipSize = size * page;
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "T20BFC:O:" + GlobalConfig.TFC20BayaningFilipinoCategoryId;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }

                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.TFC20BayaningFilipinoCategoryId).Select(e => e.EpisodeId);
                    var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId));
                    if (episodes != null)
                    {
                        jfi = new List<HomepageFeatureItem>();
                        foreach (var item in episodes)
                        {
                            if (item.OnlineStatusId == GlobalConfig.Visible && item.OnlineStartDate < registDt && item.OnlineEndDate > registDt)
                            {
                                string img = String.IsNullOrEmpty(item.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, item.EpisodeId, item.ImageAssets.ImageVideo);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    id = item.EpisodeId,
                                    description = item.Description,
                                    name = item.EpisodeName,
                                    airdate = (item.DateAired != null) ? item.DateAired.Value.ToString("MMMM d, yyyy") : "",
                                    imgurl = img,
                                    blurb = HttpUtility.HtmlEncode(item.Synopsis),
                                    slug = MyUtility.GetSlug(item.EpisodeName)
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

                if (jfi != null)
                    obj = jfi.Skip(skipSize).Take(size).ToList();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            ViewBag.pageSize = size;
            ViewBag.page = page + 1;
            return PartialView(obj);
        }

    }
}
