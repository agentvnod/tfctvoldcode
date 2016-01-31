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
    public class PinoyBigBrotherController : Controller
    {
        //
        // GET: /PinoyBigBrother/

        public ActionResult Index()
        {
            // for pbb 737
            RedirectToAction("PBB737","PinoyBigBrother");
            //
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            int id = GlobalConfig.PBBLiveEventEpisodeId;

            try
            {
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

                        string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                        EpisodeCategory episodeCategory = null;
                        if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                        {
                            if (User.Identity.IsAuthenticated)
                            {
                                var UserId = new Guid(User.Identity.Name);
                                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                if (user != null)
                                {
                                    //var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                                    //var isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
                                    //if (isEmailAllowed)
                                    if (MyUtility.IsWhiteListed(user.EMail))
                                        CountryCode = user.CountryCode;
                                }
                            }

                            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                            episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                            //var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                            //episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
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
                                        var CoverItLiveCdn = assetTemp.AssetCdns.FirstOrDefault(a => a.CdnId == GlobalConfig.CoverItLiveCdnId);
                                        CoverItLiveAltCastCode = CoverItLiveCdn.CdnReference;
                                    }
                                }
                                catch (Exception) { }
                            }

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
                            if (!Request.Cookies.AllKeys.Contains("version"))
                                return View("Index2", episode);
                            return View(episode);
                        }
                    }
                }

            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }

        //const int pageSize = 12;
        //public ActionResult List(int id = 1)
        //{
        //    int page = id;
        //    int categoryId = GlobalConfig.BCWMHWeddingCategoryId;
        //    try
        //    {
        //        var context = new IPTV2Entities();
        //        var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == categoryId && e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderByDescending(e => e.Episode.AuditTrail.CreatedOn)
        //            .Skip((page - 1) * pageSize)
        //            .Take(pageSize)
        //            .Select(e => e.EpisodeId);

        //        var totalCount = context.EpisodeCategories1.Count(e => e.CategoryId == categoryId && e.Episode.OnlineStatusId == GlobalConfig.Visible);
        //        if (totalCount == 0)
        //            return RedirectToAction("Index");
        //        ViewBag.CurrentPage = page;
        //        ViewBag.PageSize = pageSize;

        //        var totalPage = Math.Ceiling((double)totalCount / pageSize);
        //        ViewBag.TotalPages = totalPage;
        //        ViewBag.TotalCount = totalCount;
        //        var maxCount = page * pageSize > totalCount ? totalCount : page * pageSize;
        //        ViewBag.OutOf = String.Format("{0} - {1}", (page * pageSize) + 1 - pageSize, maxCount);
        //        var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible);

        //        ViewBag.Previous = page == 1 ? String.Empty : (page - 1) == 1 ? String.Empty : (page - 1).ToString();
        //        ViewBag.Next = page == (int)totalPage ? (int)totalPage : page + 1;

        //        if ((page * pageSize) + 1 - pageSize > totalCount)
        //            return RedirectToAction("List", new { id = String.Empty });
        //        if (episodes != null)
        //            return View(episodes);
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }

        //    return RedirectToAction("Index", "Home");
        //}

        //public ActionResult Details(int? id, string slug)
        //{
        //    var profiler = MiniProfiler.Current;
        //    var registDt = DateTime.Now;
        //    try
        //    {
        //        if (id != null)
        //        {
        //            var context = new IPTV2Entities();
        //            var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
        //            if (episode != null)
        //            {
        //                if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
        //                {
        //                    var dbSlug = MyUtility.GetSlug(episode.Description);
        //                    if (episode.IsLiveChannelActive == true)
        //                        return RedirectToActionPermanent("Details", "Live", new { id = id, slug = dbSlug });

        //                    SortedSet<int> parentCategories;
        //                    using (profiler.Step("Check for Parent Shows"))
        //                    {
        //                        var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
        //                        parentCategories = episode.GetParentShows(CacheDuration);
        //                        if (parentCategories.Count() > 0)
        //                        {
        //                            if (parentCategories.Contains(GlobalConfig.HalalanNewsAlertsParentCategoryId))
        //                                return RedirectToActionPermanent("NewsAlerts", "Halalan2013", new { id = id, slug = dbSlug });
        //                            else if (parentCategories.Contains(GlobalConfig.HalalanAdvisoriesParentCategoryId))
        //                                return RedirectToActionPermanent("Advisories", "Halalan2013", new { id = id, slug = dbSlug });
        //                            else if (parentCategories.Contains(GlobalConfig.TFCkatCategoryId))
        //                                return RedirectToActionPermanent("OnDemand", "TFCkat", new { id = id, slug = dbSlug });
        //                            else if (parentCategories.Contains(GlobalConfig.UAAPGreatnessNeverEndsCategoryId))
        //                                return RedirectToActionPermanent("OnDemand", "UAAP", new { id = id, slug = dbSlug });
        //                            else if (parentCategories.Contains(GlobalConfig.DigitalShortsCategoryId) && !parentCategories.Contains(GlobalConfig.BCWMHWeddingCategoryId))
        //                                return Redirect(String.Format("/HaloHalo/{0}", id));
        //                        }
        //                    }
        //                    string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
        //                    EpisodeCategory episodeCategory = null;
        //                    if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
        //                    {
        //                        var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
        //                        episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
        //                    }
        //                    else
        //                    {
        //                        var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
        //                        episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
        //                    }

        //                    if (episodeCategory != null)
        //                    {
        //                        if (episodeCategory.Show.EndDate < registDt || episodeCategory.Show.StatusId != GlobalConfig.Visible)
        //                            return RedirectToAction("Index", "Home");

        //                        ViewBag.Loved = false; ViewBag.Rated = false; // Social
        //                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };

        //                        var tempShowNameWithDate = String.Format("{0} {1}", episodeCategory.Show.Description, episode.DateAired.Value.ToString("MMMM d, yyyy"));
        //                        dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
        //                        if (String.Compare(dbSlug, slug, false) != 0)
        //                            return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });


        //                        if (User.Identity.IsAuthenticated)
        //                        {
        //                            var UserId = new Guid(User.Identity.Name);
        //                            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
        //                            if (user != null)
        //                            {
        //                                CountryCode = user.CountryCode;
        //                                ViewBag.EmailAddress = user.EMail;
        //                                ViewBag.UserId = user.UserId;
        //                                ViewBag.CountryCode = CountryCode;
        //                                using (profiler.Step("Social Love"))
        //                                {
        //                                    ViewBag.Loved = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_LOVE, id, EngagementContentType.Episode);
        //                                }
        //                                using (profiler.Step("Social Rating"))
        //                                {
        //                                    ViewBag.Rated = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_RATING, id, EngagementContentType.Episode);
        //                                }
        //                                using (profiler.Step("Check for Active Entitlements")) //check for active entitlements based on categoryId
        //                                {
        //                                    ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            using (profiler.Step("Check for Active Entitlements (Not Logged In)")) //check for active entitlements based on categoryId
        //                            {
        //                                ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
        //                            }
        //                        }

        //                        if (!ContextHelper.IsCategoryViewableInUserCountry(episodeCategory.Show, CountryCode))
        //                            return RedirectToAction("Index", "Home");

        //                        ViewBag.Show = episodeCategory.Show;
        //                        ViewBag.CategoryType = "Show";
        //                        if (episodeCategory.Show is Movie)
        //                            ViewBag.CategoryType = "Movie";
        //                        else if (episodeCategory.Show is SpecialShow)
        //                            ViewBag.CategoryType = "SpecialShow";
        //                        else if (episodeCategory.Show is WeeklyShow)
        //                            ViewBag.CategoryType = "WeeklyShow";
        //                        else if (episodeCategory.Show is DailyShow)
        //                            ViewBag.CategoryType = "DailyShow";
        //                        return View(episode);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    return RedirectToAction("Index", "Home");
        //}
        public PartialViewResult GetPBBFeaturedContent(int featureId, string id, string partialViewName, int? pageSize, int page = 0, bool is_active = false)
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
                string cacheKey = "JRGL:O:" + featureId + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var feature = context.Features.FirstOrDefault(f => f.FeatureId == featureId && f.StatusId == GlobalConfig.Visible);
                    if (feature != null)
                    {
                        var featureItems = context.FeatureItems.Where(f => f.FeatureId == featureId && f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn);
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
                                    description = person.Height,
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
                if (jfi != null)
                    obj = jfi.Skip(skipSize).Take(size).ToList();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            ViewBag.id = id;
            ViewBag.IsActive = is_active;
            ViewBag.page = page + 1;
            ViewBag.pageSize = pageSize;
            ViewBag.featureId = featureId;
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, obj);
            return PartialView(obj);
        }

        public ActionResult PBB737()
        {
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            int id = GlobalConfig.PBB747Cam1EpisodeId;

            try
            {
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

                        string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                        EpisodeCategory episodeCategory = null;
                        if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                        {
                            if (User.Identity.IsAuthenticated)
                            {
                                var UserId = new Guid(User.Identity.Name);
                                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                if (user != null)
                                {
                                    //var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                                    //var isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
                                    //if (isEmailAllowed)
                                    if (MyUtility.IsWhiteListed(user.EMail))
                                        CountryCode = user.CountryCode;
                                }
                            }

                            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                            episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                            //var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                            //episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
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
                                        var CoverItLiveCdn = assetTemp.AssetCdns.FirstOrDefault(a => a.CdnId == GlobalConfig.CoverItLiveCdnId);
                                        CoverItLiveAltCastCode = CoverItLiveCdn.CdnReference;
                                    }
                                }
                                catch (Exception) { }
                            }

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

                            var cam2 = context.Episodes.FirstOrDefault(e => e.EpisodeId == GlobalConfig.PBB747Cam2EpisodeId && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                            if (cam2 != null)
                            {
                                ViewBag.Camera2 = cam2;
                            }

                            return View(episode);
                        }
                    }
                }

            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }
    }
}
