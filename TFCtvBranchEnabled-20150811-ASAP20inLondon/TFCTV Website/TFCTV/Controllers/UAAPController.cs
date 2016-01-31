using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EngagementsModel;
using IPTV2_Model;
using StackExchange.Profiling;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class UAAPController : Controller
    {
        //
        // GET: /UAAP/

        public ActionResult Index()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("Index3");

            var profiler = MiniProfiler.Current;
            var context = new IPTV2Entities();
            ViewBag.CoverItLiveAltCastCode = GlobalConfig.UAAPCoverItLiveAltCastCode;
            var registDt = DateTime.Now;
            ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
            if (User.Identity.IsAuthenticated)
            {
                System.Guid userId = new System.Guid(HttpContext.User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, GlobalConfig.UAAPMainCategoryId, registDt);
            }
            List<Episode> latestEpisodes = null;
            var cache = DataCache.Cache;
            string cacheKey = "UAAPLatestGame";
            latestEpisodes = (List<Episode>)cache[cacheKey];

            if (latestEpisodes == null)
            {
                latestEpisodes = new List<Episode>();
                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                var cats = (Category)context.CategoryClasses.Find(GlobalConfig.UAAPGamesParentId);
                var listOfShows = service.GetAllOnlineShowIds(GlobalConfig.DefaultCountry, cats);
                var episodeids = context.EpisodeCategories1.Where(e => listOfShows.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                var episodes = context.Episodes.Where(e => episodeids.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).ThenBy(e => e.AuditTrail.CreatedOn).Take(4);
                foreach (var e in episodes)
                {
                    Episode ep = new Episode() { EpisodeCode = e.EpisodeCode, EpisodeId = e.EpisodeId, EpisodeName = e.EpisodeName, Description = e.Description, DateAired = e.DateAired, ImageAssets = e.ImageAssets };
                    latestEpisodes.Add(ep);
                }
                cache.Put(cacheKey, latestEpisodes, DataCache.CacheDuration);

            }
            string countryCode = MyUtility.GetCurrentCountryCodeOrDefault();
            ViewBag.CountryCode = countryCode;
            ViewBag.LatestGameImage1 = GlobalConfig.EpisodeImgPath + latestEpisodes[0].EpisodeId.ToString() + "/" + latestEpisodes[0].ImageAssets.ImageVideo.ToString();
            ViewBag.LatestGameID1 = latestEpisodes[0].EpisodeId.ToString();
            ViewBag.LatestGameDescription1 = latestEpisodes[0].EpisodeName.ToString();
            ViewBag.LatestGameAirDate1 = ((DateTime)latestEpisodes[0].DateAired).ToString("MMM d, yyyy");
            ViewBag.LatestGameImage2 = GlobalConfig.EpisodeImgPath + latestEpisodes[1].EpisodeId.ToString() + "/" + latestEpisodes[1].ImageAssets.ImageVideo.ToString();
            ViewBag.LatestGameImage3 = GlobalConfig.EpisodeImgPath + latestEpisodes[2].EpisodeId.ToString() + "/" + latestEpisodes[2].ImageAssets.ImageVideo.ToString();
            ViewBag.LatestGameImage4 = GlobalConfig.EpisodeImgPath + latestEpisodes[3].EpisodeId.ToString() + "/" + latestEpisodes[3].ImageAssets.ImageVideo.ToString();
            ViewBag.LatestGameID2 = latestEpisodes[1].EpisodeId.ToString();
            ViewBag.LatestGameID3 = latestEpisodes[2].EpisodeId.ToString();
            ViewBag.LatestGameID4 = latestEpisodes[3].EpisodeId.ToString();
            return View();
        }


        public bool hasUAAPEntitlements(IPTV2Entities context)
        {
            bool entitled = false;
            if (MyUtility.isUserLoggedIn())
            {
                var registDt = DateTime.Now;
                var entitlementPackageIds = MyUtility.StringToIntList(GlobalConfig.UAAPEntitlementPackageID);
                System.Guid userId = new System.Guid(HttpContext.User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                entitled = user.PackageEntitlements.Count(p => entitlementPackageIds.Contains(p.PackageId) && p.EndDate > registDt) > 0;
            }
            return entitled;
        }

        //[ChildActionOnly]
        //[OutputCache(VaryByParam = "*", Duration = 300)]
        //public PartialViewResult GetPackages(int categoryId, string countryCode)
        //{
        //    ShowPackageProductPrices showPackageProductPrices = new ShowPackageProductPrices();
        //    try
        //    {
        //        showPackageProductPrices = showPackageProductPrices.LoadAllPackageAndProduct(categoryId, countryCode, true);
        //        //if (showPackageProductPrices == null)
        //        //    return null;
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    return PartialView("_GetShowProductPackagesVertical", showPackageProductPrices);
        //}

        //[ChildActionOnly]
        //[OutputCache(VaryByParam = "*", Duration = 300)]
        //public PartialViewResult GetPackages2(ShowPackageProductPrices showPackageProductPrices)
        //{
        //    return PartialView("_GetShowProductPackagesVertical", showPackageProductPrices);
        //}

        //[GridAction]
        //public ActionResult _ShowEpisodes()
        //{
        //    return View(new GridModel<EpisodeDisplayMiniImage> { Data = GetShowEpisodes() });
        //}

        private List<EpisodeDisplayMiniImage> GetShowEpisodes()
        {
            List<EpisodeDisplayMiniImage> display = null;
            var cache = DataCache.Cache;
            string cacheKey = "UAAPPagesCacheKeyShowEpisodes";
            display = (List<EpisodeDisplayMiniImage>)cache[cacheKey];
            if (display == null)
            {
                var id = GlobalConfig.UAAPGamesParentId;
                display = new List<EpisodeDisplayMiniImage>();
                DateTime registDt = DateTime.Now;
                var context = new IPTV2Entities();
                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                var cats = (Category)context.CategoryClasses.Find(GlobalConfig.UAAPGamesParentId);
                var listOfShows = service.GetAllOnlineShowIds(GlobalConfig.DefaultCountry, cats);
                var episodeIds = context.EpisodeCategories1.Where(e => listOfShows.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                var episodeList = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).ThenBy(e => e.AuditTrail.CreatedOn);

                int epCount = episodeList.Count();
                ViewBag.sCount = epCount;
                foreach (var episode in episodeList)
                {
                    string EpLength = String.Empty;
                    if (!(episode.EpisodeLength == null))
                    {
                        TimeSpan span = new TimeSpan(0, 0, Convert.ToInt32(episode.EpisodeLength) * 60);
                        EpLength = String.Format("{0}:{1}:{2}", span.Hours.ToString().PadLeft(2, '0'), span.Minutes.ToString().PadLeft(2, '0'), span.Seconds.ToString().PadLeft(2, '0'));
                    }

                    EpisodeDisplayMiniImage disp = new EpisodeDisplayMiniImage()
                    {
                        EpisodeId = episode.EpisodeId,
                        EpisodeName = episode.Synopsis,
                        Description = episode.Description,
                        DateAired = episode.DateAired,
                        DateAiredStr = episode.DateAired.Value.ToString("MMM d, yyyy"),
                        EpisodeLength = episode.EpisodeLength,
                        EpisodeNumber = epCount,
                        EpLength = EpLength,
                        ImageSmall = GlobalConfig.EpisodeImgPath + episode.EpisodeId.ToString() + "/" + episode.ImageAssets.ImageVideo
                    };
                    display.Add(disp);
                    epCount--;
                }
                cache.Put(cacheKey, display, DataCache.CacheDuration);
            }
            return display;
        }

        //[GridAction]
        //public ActionResult _ExclusiveEpisodes()
        //{
        //    return View(new GridModel<EpisodeDisplayMiniImage> { Data = GetFeatureEpisodes() });
        //}

        private List<EpisodeDisplayMiniImage> GetFeatureEpisodes()
        {
            List<EpisodeDisplayMiniImage> display = null;
            var cache = DataCache.Cache;
            string cacheKey = "UAAPPagesExclusiveFeatureCacheKey";
            display = (List<EpisodeDisplayMiniImage>)cache[cacheKey];
            if (display == null)
            {
                var id = GlobalConfig.UAAPExclusiveFeaturesId;
                DateTime registDt = DateTime.Now;
                display = new List<EpisodeDisplayMiniImage>();
                var context = new IPTV2Entities();

                var featureItems = context.FeatureItems.Where(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
                int epCount = featureItems.Count();
                foreach (var item in featureItems)
                {
                    if (item is EpisodeFeatureItem)
                    {
                        var episodeFeatureItem = (EpisodeFeatureItem)item;
                        var episode = episodeFeatureItem.Episode;
                        string EpLength = String.Empty;
                        if (!(episode.EpisodeLength == null))
                        {
                            TimeSpan span = new TimeSpan(0, 0, Convert.ToInt32(episode.EpisodeLength) * 60);
                            EpLength = String.Format("{0}:{1}:{2}", span.Hours.ToString().PadLeft(2, '0'), span.Minutes.ToString().PadLeft(2, '0'), span.Seconds.ToString().PadLeft(2, '0'));
                        }

                        EpisodeDisplayMiniImage disp = new EpisodeDisplayMiniImage()
                        {
                            EpisodeId = episode.EpisodeId,
                            EpisodeName = episode.Synopsis,
                            Description = episode.Description,
                            DateAired = episode.DateAired,
                            DateAiredStr = episode.DateAired.Value.ToString("MMM d, yyyy"),
                            EpisodeLength = episode.EpisodeLength,
                            EpisodeNumber = epCount,
                            EpLength = EpLength,
                            ImageSmall = GlobalConfig.EpisodeImgPath + episode.EpisodeId.ToString() + "/" + episode.ImageAssets.ImageVideo
                        };
                        display.Add(disp);
                        epCount--;
                    }
                }
                cache.Put(cacheKey, display, DataCache.CacheDuration);
            }
            return display;
        }

        //[GridAction]
        //public ActionResult _GameEpisodes(int? id)
        //{
        //    return View(new GridModel<EpisodeDisplayMiniImage> { Data = GetGameEpisodes(id) });
        //}

        private List<EpisodeDisplayMiniImage> GetGameEpisodes(int? id)
        {
            List<EpisodeDisplayMiniImage> display = null;
            if (id != null)
            {
                var cache = DataCache.Cache;
                string cacheKey = "UAAPGGESS;I:" + id;
                display = (List<EpisodeDisplayMiniImage>)cache[cacheKey];
                if (display == null)
                {
                    DateTime registDt = DateTime.Now;
                    display = new List<EpisodeDisplayMiniImage>();
                    var context = new IPTV2Entities();

                    var ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                    var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                    var parentShows = ep.GetParentShows(CacheDuration);
                    parentShows.Remove(GlobalConfig.UAAPMainCategoryId);
                    var episodeIds = context.EpisodeCategories1.Where(e => parentShows.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                    var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                    int epCount = episodes.Count();
                    ViewBag.sCount = epCount;
                    foreach (var episode in episodes)
                    {
                        if (episode.EpisodeId != id)
                        {
                            string EpLength = String.Empty;
                            if (episode.EpisodeLength != null)
                            {
                                TimeSpan span = new TimeSpan(0, 0, Convert.ToInt32(episode.EpisodeLength) * 60);
                                EpLength = String.Format("{0}:{1}:{2}", span.Hours.ToString().PadLeft(2, '0'), span.Minutes.ToString().PadLeft(2, '0'), span.Seconds.ToString().PadLeft(2, '0'));
                            }

                            EpisodeDisplayMiniImage disp = new EpisodeDisplayMiniImage()
                            {
                                EpisodeId = episode.EpisodeId,
                                EpisodeName = episode.Synopsis,
                                Description = episode.Description,
                                DateAired = episode.DateAired,
                                DateAiredStr = episode.DateAired.Value.ToString("MMM d, yyyy"),
                                EpisodeLength = episode.EpisodeLength,
                                EpisodeNumber = epCount,
                                EpLength = EpLength,
                                ImageSmall = GlobalConfig.EpisodeImgPath + episode.EpisodeId.ToString() + "/" + episode.ImageAssets.ImageVideo
                            };
                            display.Add(disp);
                            epCount--;
                        }
                    }
                    cache.Put(cacheKey, display, DataCache.CacheDuration);
                }
            }
            return display;
        }

        [OutputCache(VaryByParam = "id", Duration = 300)]
        public ActionResult GetEpisodes(int? id)
        {
            if (id != GlobalConfig.UAAPGamesParentId)
                return PartialView("_GetExclusiveEpisodes");
            return PartialView("_GetEpisodesMinimizedWithImage");

        }

        [OutputCache(VaryByParam = "id", Duration = 300)]
        public ActionResult GetPlaylist(int? id)
        {
            if (id == GlobalConfig.UAAPGamesParentId)
                return PartialView("_GetAllEpisodes");
            ViewBag.EpisodeId = id;
            return PartialView("_GetGameEpisodes");

        }

        [OutputCache(VaryByParam = "id", Duration = 300)]
        public ActionResult GetPlaylistExclusive(int? id)
        {
            return PartialView("_GetOnDemandExclusiveEpisodes");
        }

        public ActionResult GetFeatureTeamsPR()
        {
            List<JsonFeatureItem> jfi = null;
            var cache = DataCache.Cache;
            string cacheKey = "UAAPGFTPR:0;";
            jfi = (List<JsonFeatureItem>)cache[cacheKey];
            if (jfi == null)
            {
                var context = new IPTV2Entities();
                var socialContext = new EngagementsEntities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == GlobalConfig.UAAPTeamsFeatureId && f.StatusId == GlobalConfig.Visible);
                if (feature != null)
                {
                    var featureItems1 = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible);
                    if (featureItems1 != null)
                    {
                        List<FeatureItem> featureItems = featureItems1.OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                        jfi = new List<JsonFeatureItem>();
                        foreach (var f in featureItems)
                        {
                            if (f is CelebrityFeatureItem)
                            {
                                var celeb = (CelebrityFeatureItem)f;
                                Celebrity person = celeb.Celebrity;
                                string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                                var lovesCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == celeb.CelebrityId && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                                int love = (int)(lovesCountSummary == null ? 0 : lovesCountSummary.Total);
                                var commentsCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == celeb.CelebrityId && i.ReactionTypeId == GlobalConfig.SOCIAL_COMMENT);
                                int comment = (int)(commentsCountSummary == null ? 0 : commentsCountSummary.Total);
                                JsonFeatureItem j = new JsonFeatureItem()
                                {
                                    CelebrityFullName = person.FullName,
                                    ShowId = person.CelebrityId,
                                    ShowImageUrl = img,
                                    Blurb = person.Height.ToString(),
                                    EpisodeDescription = love.ToString() + "-" + comment.ToString()

                                };
                                jfi.Add(j);
                            }
                        }
                        cache.Put(cacheKey, jfi, DataCache.CacheDuration);
                    }
                }
            }
            return PartialView(jfi);
        }

        public ActionResult TeamDetails(int? id, string slug)
        {
            if (id != null)
            {
                var context = new IPTV2Entities();
                var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id);
                if (celebrity != null)
                {
                    var dbSlug = MyUtility.GetSlug(celebrity.FullName);
                    if (String.Compare(dbSlug, slug, false) != 0)
                        return RedirectToActionPermanent("TeamDetails", "UAAP", new { id = id, slug = dbSlug });

                    if (String.IsNullOrEmpty(celebrity.Description))
                        celebrity.Description = "No description yet.";

                    if (String.IsNullOrEmpty(celebrity.ImageUrl))
                        celebrity.ImageUrl = String.Format("{0}/{1}", GlobalConfig.AssetsBaseUrl, "content/images/celebrity/unknown.jpg");
                    else
                        celebrity.ImageUrl = String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, celebrity.CelebrityId, celebrity.ImageUrl);

                    if (MyUtility.isUserLoggedIn() && HasSocialEngagementRecordOnCelebrity(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, (int)id))
                        ViewBag.Loved = true;

                    if (!String.IsNullOrEmpty(celebrity.Birthday))
                        ViewBag.BirthDate = celebrity.Birthday;
                    if (!String.IsNullOrEmpty(celebrity.Birthplace))
                    {
                        ViewBag.BirthPlace = celebrity.Birthplace;
                        celebrity.Birthplace = "<p>" + celebrity.FirstName + " VS " + celebrity.Birthplace.Replace(",", "</p><p>" + celebrity.FirstName + " VS ") + "</p>";
                        celebrity.Birthplace = celebrity.Birthplace.Replace(":", "<br />");
                    }
                    if (!String.IsNullOrEmpty(celebrity.ZodiacSign))
                        celebrity.ZodiacSign = String.Format("{0}/content/images/uaap/{1}", GlobalConfig.AssetsBaseUrl, celebrity.ZodiacSign);
                    if (!String.IsNullOrEmpty(celebrity.ChineseYear))
                        celebrity.ChineseYear = String.Format("{0}/content/images/uaap/{1}", GlobalConfig.AssetsBaseUrl, celebrity.ChineseYear);
                    if (!Request.Cookies.AllKeys.Contains("version"))
                        return View("TeamDetails2", celebrity);
                    return View(celebrity);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public bool HasSocialEngagementRecordOnCelebrity(Guid userId, int reactionTypeId, int celebrityId)
        {
            var context = new EngagementsEntities();
            var celebrity = context.CelebrityReactions.FirstOrDefault(c => c.CelebrityId == celebrityId && c.ReactionTypeId == reactionTypeId && c.UserId == userId);
            if (celebrity == null)
                return false;
            return true;
        }

        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetTeamGameEpisodes(int? id)
        {
            List<JsonFeatureItem> jfi = null;
            if (id != null)
            {
                var registDt = DateTime.Now;
                var cache = DataCache.Cache;
                string cacheKey = "UAAPGTGES:0;" + id;
                jfi = (List<JsonFeatureItem>)cache[cacheKey];
                if (jfi == null)
                {
                    jfi = new List<JsonFeatureItem>();
                    var context = new IPTV2Entities();
                    var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id);
                    if (celebrity != null)
                    {
                        var gamesCategoryIds = celebrity.ShowCelebrityRoles.Where(celeb => celeb.CelebrityId == celebrity.CelebrityId).Select(s => s.CategoryId);
                        var episodeIds = context.EpisodeCategories1.Where(e => gamesCategoryIds.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                        var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                        foreach (var ep in episodes)
                        {
                            var episodeCategory = ep.EpisodeCategories.FirstOrDefault(e => e.CategoryId != GlobalConfig.FreeTvCategoryId);
                            var show = episodeCategory.Show;
                            var name = show.Description;
                            string img = String.IsNullOrEmpty(ep.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ep.EpisodeId.ToString(), ep.ImageAssets.ImageVideo);
                            string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                            JsonFeatureItem j = new JsonFeatureItem() { EpisodeId = ep.EpisodeId, EpisodeDescription = MyUtility.Ellipsis(ep.Description, 20), EpisodeName = ep.EpisodeName, EpisodeAirDate = (ep.DateAired != null) ? ep.DateAired.Value.ToString("MMM d, yyyy") : "", ShowId = show.CategoryId, ShowName = MyUtility.Ellipsis(show.CategoryName, 20), EpisodeImageUrl = img, ShowImageUrl = showImg, Blurb = MyUtility.Ellipsis(ep.Synopsis, 80) };
                            jfi.Add(j);
                        }
                        cache.Put(cacheKey, jfi, DataCache.CacheDuration);
                    }
                }
            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Live(int? id, string slug)
        {
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            try
            {
                id = GlobalConfig.UAAPLiveStreamEpisodeId;
                if (id != null)
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

                            using (profiler.Step("Check for Parent Shows"))
                            {
                                var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                                var parentCategories = episode.GetParentShows(CacheDuration);
                                if (parentCategories.Count() > 0)
                                {
                                    if (parentCategories.Contains(GlobalConfig.HalalanNewsAlertsParentCategoryId))
                                        return RedirectToActionPermanent("NewsAlerts", "Halalan2013", new { id = id, slug = dbSlug });
                                    else if (parentCategories.Contains(GlobalConfig.HalalanAdvisoriesParentCategoryId))
                                        return RedirectToActionPermanent("Advisories", "Halalan2013", new { id = id, slug = dbSlug });
                                    else if (parentCategories.Contains(GlobalConfig.TFCkatCategoryId))
                                        return RedirectToActionPermanent("OnDemand", "TFCkat", new { id = id, slug = dbSlug });
                                    else if (parentCategories.Contains(GlobalConfig.UAAPMainCategoryId) && episode.IsLiveChannelActive != true)
                                        return RedirectToActionPermanent("OnDemand", "UAAP", new { id = id, slug = dbSlug });
                                }
                            }

                            var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                            var episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                            if (episodeCategory != null)
                            {
                                ViewBag.Loved = false; ViewBag.Rated = false; // Social
                                ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };

                                var tempShowNameWithDate = String.Format("{0} {1}", episodeCategory.Show.Description, episode.DateAired.Value.ToString("MMMM d, yyyy"));
                                dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
                                if (String.Compare(dbSlug, slug, false) != 0)
                                    return RedirectToActionPermanent("Live", new { id = id, slug = dbSlug });
                                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
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
                                            ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, episodeCategory.CategoryId, registDt);
                                        }
                                    }
                                }
                                else
                                {
                                    using (profiler.Step("Check for Active Entitlements (Not Logged In)")) //check for active entitlements based on categoryId
                                    {
                                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, episodeCategory.CategoryId, registDt, CountryCode);
                                    }
                                }
                                if (!ContextHelper.IsCategoryViewableInUserCountry(episodeCategory.Show, CountryCode))
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
                                ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
                                if (!Request.Cookies.AllKeys.Contains("version"))
                                {
                                    if (GlobalConfig.UseJWPlayer)
                                    {
                                        if (Request.Cookies.AllKeys.Contains("hplayer"))
                                        {
                                            if (Request.Cookies["hplayer"].Value == "2")
                                                return View("LiveUXFlowHLS", episode);
                                            else
                                                return View("LiveUX", episode);
                                        }
                                        else
                                            return View("LiveUX2", episode);
                                    }
                                    else
                                        return View("LiveUX", episode);
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

        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetPaginatedEpisodeList()
        {
            List<JsonFeatureItem> jfi = null;
            var cache = DataCache.Cache;
            string cacheKey = "UAAPPaginatedEP";
            jfi = (List<JsonFeatureItem>)cache[cacheKey];
            if (jfi == null)
            {
                jfi = new List<JsonFeatureItem>();
                var context = new IPTV2Entities();
                SortedSet<int> episodeids = new SortedSet<int>();
                var registDt = DateTime.Now;
                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                var cats = (Category)context.CategoryClasses.Find(GlobalConfig.UAAPGamesParentId);
                var listOfShows = service.GetAllOnlineShowIds(GlobalConfig.DefaultCountry, cats);
                var episodeIds = context.EpisodeCategories1.Where(e => listOfShows.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                var episodeList = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).Take(12).OrderByDescending(e => e.DateAired).ThenBy(e => e.AuditTrail.CreatedOn);
                foreach (var ep in episodeList)
                {
                    string img = String.IsNullOrEmpty(ep.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ep.EpisodeId.ToString(), ep.ImageAssets.ImageVideo);
                    JsonFeatureItem j = new JsonFeatureItem() { EpisodeId = ep.EpisodeId, EpisodeDescription = ep.Description, EpisodeName = ep.EpisodeName, EpisodeAirDate = (ep.DateAired != null) ? ep.DateAired.Value.ToString("MMM d, yyyy") : "", EpisodeImageUrl = img };
                    jfi.Add(j);
                }
                cache.Put(cacheKey, jfi, DataCache.CacheDuration);
            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetPaginatedExclusiveList()
        {
            List<JsonFeatureItem> jfi = null;
            var cache = DataCache.Cache;
            string cacheKey = "UAAPPaginatedEx";
            jfi = (List<JsonFeatureItem>)cache[cacheKey];
            if (jfi == null)
            {
                jfi = new List<JsonFeatureItem>();
                var context = new IPTV2Entities();
                SortedSet<int> episodeids = new SortedSet<int>();
                var registDt = DateTime.Now;
                List<Episode> episodes = new List<Episode>();
                var featureItems = context.FeatureItems.Where(f => f.FeatureId == GlobalConfig.UAAPExclusiveFeaturesId && f.StatusId == GlobalConfig.Visible).Take(12);
                foreach (var item in featureItems)
                {
                    if (item is EpisodeFeatureItem)
                    {
                        var episodeFeatureItem = (EpisodeFeatureItem)item;
                        var ep = episodeFeatureItem.Episode;
                        string img = String.IsNullOrEmpty(ep.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ep.EpisodeId.ToString(), ep.ImageAssets.ImageVideo);
                        JsonFeatureItem j = new JsonFeatureItem() { EpisodeId = ep.EpisodeId, EpisodeDescription = ep.Description, EpisodeName = ep.EpisodeName, EpisodeAirDate = (ep.DateAired != null) ? ep.DateAired.Value.ToString("MMM d, yyyy") : "", EpisodeImageUrl = img };
                        jfi.Add(j);
                    }

                }
                cache.Put(cacheKey, jfi, DataCache.CacheDuration);
            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OnDemandOld(int? id, string slug)
        {
            if (id != null)
            {
                var profiler = MiniProfiler.Current;
                var context = new IPTV2Entities();

                Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                if (episode == null)
                    return RedirectToAction("Index", "Home");

                DateTime registDt = DateTime.Now;
                if (episode.OnlineStartDate > registDt)
                    return RedirectToAction("Index", "Home");
                if (episode.OnlineEndDate < registDt)
                    return RedirectToAction("Index", "Home");
                ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                if (User.Identity.IsAuthenticated)
                {
                    System.Guid userId = new System.Guid(HttpContext.User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, GlobalConfig.UAAPMainCategoryId, registDt);
                }
                //else
                //{
                //    using (profiler.Step("Check for Active Entitlements (Not Logged In)")) //check for active entitlements based on categoryId
                //    {
                //        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, GlobalConfig.UAAPGreatnessNeverEndsCategoryId, registDt, CountryCode);
                //    }
                //}
                bool isUserEntitled = false;
                var dbSlug = MyUtility.GetSlug(episode.Description);
                if (episode.IsLiveChannelActive == true)
                    return RedirectToActionPermanent("Details", "Live", new { id = id, slug = dbSlug });

                var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                var parentCategories = episode.GetParentShows(CacheDuration);
                if (parentCategories.Count() > 0)
                {
                    if (!(parentCategories.Contains(GlobalConfig.UAAPMainCategoryId)))
                        return RedirectToActionPermanent("Details", "Episode", new { id = id, slug = dbSlug });
                }

                var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                EpisodeCategory category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && !excludedCategoryIds.Contains(e.CategoryId));

                if (category != null)
                {
                    ViewBag.Show = category.Show;
                    var tempShowNameWithDate = String.Format("{0} {1}", category.Show.Description, episode.DateAired.Value.ToString("MMMM d, yyyy"));
                    dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
                    if (String.Compare(dbSlug, slug, false) != 0)
                        return RedirectToActionPermanent("OnDemand", new { id = id, slug = dbSlug });

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
                                //isUserEntitled = user.IsEpisodeEntitled(offering, episode, asset, RightsType.Online);
                                //isUserEntitled = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
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


                    using (profiler.Step("Has Social Love"))
                    {
                        if (MyUtility.isUserLoggedIn() && HasSocialEngagementRecordOnEpisode(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, (int)id))

                            ViewBag.Loved = true;
                    }
                    using (profiler.Step("Has Social Rating"))
                    {
                        if (MyUtility.isUserLoggedIn() && HasSocialEngagementRecordOnEpisode(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_RATING, (int)id))

                            ViewBag.Rated = true;
                    }

                    ///**** Check for Free Trial ****/
                    //bool showFreeTrialImage = false;
                    //using (profiler.Step("Check for Early Bird"))
                    //{
                    //    if (GlobalConfig.IsEarlyBirdEnabled)
                    //    {
                    //        if (MyUtility.isUserLoggedIn())
                    //        {

                    //            var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
                    //            if (user != null)
                    //            {
                    //                var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    //                if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                    //                    showFreeTrialImage = true;
                    //            }
                    //            //showFreeTrialImage = true;
                    //        }
                    //    }
                    //}
                    ////REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
                    ////showFreeTrialImage = false;
                    //ViewBag.ShowFreeTrialImage = showFreeTrialImage;
                    bool isExclusive = IsEpisodePartOfTheExclusiveFeature(context, episode);
                    ViewBag.isExclusiveEpisode = isExclusive;

                    ViewBag.ShowPackageProductPrices = null;
                    string countryCode = MyUtility.GetCurrentCountryCodeOrDefault();
                    ViewBag.CountryCode = countryCode;
                    //if (!isUserEntitled)
                    //{
                    //    using (profiler.Step("Get Show Package & Product Prices"))
                    //    {
                    //        try
                    //        {
                    //            ViewBag.ShowPackageProductPrices = ContextHelper.GetShowPackageProductPrices(category.Show.CategoryId, countryCode);
                    //        }
                    //        catch (Exception e) { MyUtility.LogException(e); }
                    //    }
                    //}
                    ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
                    if (!Request.Cookies.AllKeys.Contains("version"))
                        return View("OnDemand2", episode);
                    return View(episode);
                }
                else
                    return RedirectToAction("Index", "Home");
            }

            else return RedirectToAction("Index", "Home");
        }

        private bool IsEpisodePartOfTheExclusiveFeature(IPTV2Entities context, Episode episode)
        {
            var feature = context.Features.Find(GlobalConfig.UAAPExclusiveFeaturesId);
            var listOfFeatureIds = episode.EpisodeFeatureItems.Select(e => e.FeatureId);
            var featureItemsOfFeature = feature.FeatureItems.Select(f => f.FeatureId);
            return listOfFeatureIds.Intersect(featureItemsOfFeature).Any();
        }

        private bool HasSocialEngagementRecordOnEpisode(Guid userId, int reactionTypeId, int episodeId)
        {
            var context = new EngagementsEntities();
            var episode = context.EpisodeReactions.FirstOrDefault(s => s.EpisodeId == episodeId && s.ReactionTypeId == reactionTypeId && s.UserId == userId);
            if (episode == null)
                return false;
            return true;
        }

        public ActionResult Teams(int? id)
        {
            try
            {
                if (!Request.Cookies.AllKeys.Contains("version"))
                    return View("Teams2");

                List<Celebrity> jfi = null;
                var socialContext = new EngagementsEntities();
                var cache = DataCache.Cache;
                string cacheKey = "UaapCelebCacKey1:0;" + id;
                jfi = (List<Celebrity>)cache[cacheKey];

                if (jfi == null)
                {
                    var context = new IPTV2Entities();
                    var feature = context.Features.FirstOrDefault(f => f.FeatureId == 82 && f.StatusId == GlobalConfig.Visible);

                    List<FeatureItem> featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).ToList();
                    List<int> idlist = new List<int>();
                    foreach (CelebrityFeatureItem f in featureItems)
                    {
                        idlist.Add(f.CelebrityId);
                    }

                    var celeb = context.Celebrities.Where(c => idlist.Contains(c.CelebrityId));
                    jfi = new List<Celebrity>();
                    foreach (Celebrity person in celeb)
                    {
                        bool hasLoved = MyUtility.isUserLoggedIn() && HasSocialEngagementRecordOnCelebrity(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, (int)person.CelebrityId);
                        string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                        var lovesCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == person.CelebrityId && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                        int love = (int)(lovesCountSummary == null ? 0 : lovesCountSummary.Total);
                        var commentsCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == person.CelebrityId && i.ReactionTypeId == GlobalConfig.SOCIAL_COMMENT);
                        int comment = (int)(commentsCountSummary == null ? 0 : commentsCountSummary.Total);
                        person.ImageUrl = img;
                        person.Birthday = love.ToString() + "-" + comment.ToString();
                        Celebrity c = new Celebrity() { CelebrityId = person.CelebrityId, ImageUrl = person.ImageUrl, Height = person.Height, FullName = person.FullName, Birthday = person.Birthday, Birthplace = hasLoved.ToString() };
                        jfi.Add(c);
                    }
                    cache.Put(cacheKey, jfi, DataCache.CacheDuration);
                }
                return View(jfi);
            }
            catch (Exception) { throw; }

        }

        const int pageSize = 12;
        public ActionResult ListAllGameEpisodes(int id = 1)
        {
            int page = id;
            var context = new IPTV2Entities();
            var registDt = DateTime.Now;
            var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
            //var cats = (Category)context.CategoryClasses.Find(GlobalConfig.UAAPGamesParentId);
            //var listOfShows = service.GetAllOnlineShowIds(GlobalConfig.DefaultCountry, cats);
            //var episodeIds = context.EpisodeCategories1.Where(e => listOfShows.Contains(e.CategoryId) && e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderBy(e => e.EpisodeId).Select(e => e.EpisodeId);

            var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.UAAPMainCategoryId && e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderBy(e => e.EpisodeId).Select(e => e.EpisodeId);
            var episodeDelimited = episodeIds.Skip((page - 1) * pageSize).Take(pageSize);

            var totalCount = episodeIds.Count();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            var totalPage = Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalPages = totalPage;
            ViewBag.TotalCount = totalCount;
            var maxCount = page * pageSize > totalCount ? totalCount : page * pageSize;
            ViewBag.OutOf = String.Format("{0} - {1}", (page * pageSize) + 1 - pageSize, maxCount);
            var episodes = context.Episodes.Where(e => episodeDelimited.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);

            ViewBag.Previous = page == 1 ? String.Empty : (page - 1) == 1 ? String.Empty : (page - 1).ToString();
            ViewBag.Next = page == (int)totalPage ? (int)totalPage : page + 1;

            if ((page * pageSize) + 1 - pageSize > totalCount)
                return RedirectToAction("ListAllGameEpisodes", "UAAP", new { id = String.Empty });
            if (episodes != null)
                return View(episodes);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ListAllExclusives(int id = 1)
        {
            int page = id;
            var context = new IPTV2Entities();
            var registDt = DateTime.Now;

            var featureItems = context.FeatureItems.Where(f => f.FeatureId == GlobalConfig.UAAPExclusiveFeaturesId && f.StatusId == GlobalConfig.Visible);

            var episodeList = new List<Episode>();
            foreach (var item in featureItems)
            {
                if (item is EpisodeFeatureItem)
                {
                    var episodeFeatureItem = (EpisodeFeatureItem)item;
                    episodeList.Add(episodeFeatureItem.Episode);
                }
            }

            var episodesDelimited = episodeList.OrderBy(e => e.EpisodeId).Select(e => e.EpisodeId).Skip((page - 1) * pageSize).Take(pageSize);

            var totalCount = featureItems.Count();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            var totalPage = Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalPages = totalPage;
            ViewBag.TotalCount = totalCount;
            var maxCount = page * pageSize > totalCount ? totalCount : page * pageSize;
            ViewBag.OutOf = String.Format("{0} - {1}", (page * pageSize) + 1 - pageSize, maxCount);
            var episodes = context.Episodes.Where(e => episodesDelimited.Contains(e.EpisodeId) && e.OnlineStatusId == 1 && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);


            ViewBag.Previous = page == 1 ? String.Empty : (page - 1) == 1 ? String.Empty : (page - 1).ToString();
            ViewBag.Next = page == (int)totalPage ? (int)totalPage : page + 1;

            if ((page * pageSize) + 1 - pageSize > totalCount)
                return RedirectToAction("ListAllExclusives", "UAAP", new { id = String.Empty });
            if (episodes != null)
                return View(episodes);
            return RedirectToAction("Index", "Home");
        }

        public PartialViewResult GetFeaturedTeams(string partialViewName)
        {
            List<HomepageFeatureItem> jfi = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            var id = GlobalConfig.UAAPTeamsFeatureId;
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
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, jfi);
            return PartialView(jfi);
        }

        public PartialViewResult GetUAAPContent(string id, string partialViewName, int? pageSize, int page = 0, bool is_active = false)
        {
            var registDt = DateTime.Now;
            List<Episode> obj = null;
            try
            {
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.UAAPGamesParentId);
                if (category != null)
                {
                    if (category is Category)
                    {
                        string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                        var showIds = service.GetAllOnlineShowIds(CountryCode, (Category)category);
                        var episodeIds = context.EpisodeCategories1.Where(e => showIds.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                        var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                        if (episodes != null)
                        {
                            episodes = episodes.OrderByDescending(e => e.DateAired).ThenByDescending(e => e.AuditTrail.CreatedOn);
                            obj = pageSize == null ? episodes.ToList() : episodes.Skip(page).Take((int)pageSize).ToList();
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
            {
                ViewBag.id = id;
                ViewBag.IsActive = is_active;
                return PartialView(partialViewName, obj);
            }
            return PartialView(obj);
        }

        public PartialViewResult GetMoreUAAPContent(int? pageSize, int page = 0)
        {
            var registDt = DateTime.Now;
            List<Episode> obj = null;
            int skipSize = 0;
            int size = GlobalConfig.FeatureItemsPageSize;
            try
            {
                if (pageSize != null)
                    size = (int)pageSize;
                skipSize = size * page;
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.UAAPGamesParentId);
                if (category != null)
                {
                    if (category is Category)
                    {
                        string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                        var showIds = service.GetAllOnlineShowIds(CountryCode, (Category)category);
                        var episodeIds = context.EpisodeCategories1.Where(e => showIds.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                        var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                        if (episodes != null)
                        {
                            episodes = episodes.OrderByDescending(e => e.DateAired).ThenByDescending(e => e.AuditTrail.CreatedOn);
                            obj = pageSize == null ? episodes.ToList() : episodes.Skip(skipSize).Take((int)pageSize).ToList();
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            ViewBag.page = page + 1;
            ViewBag.pageSize = pageSize;
            return PartialView(obj);
        }

        public PartialViewResult GetUAAPFeaturedContent(int featureId, string id, string partialViewName, int? pageSize, int page = 0, bool is_active = false)
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

        public PartialViewResult GetMoreUAAPFeaturedContent(int featureId, string id, string partialViewName, int? pageSize, int page = 0, bool is_active = false)
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
            return PartialView(obj);
        }

        public PartialViewResult GetTeamGames(int id, int? pageSize, int page = 0)
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
                string cacheKey = "UAAPGTGES:O:" + id.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }

                ViewBag.id = id;
                ViewBag.pageSize = pageSize;

                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id);
                    if (celebrity != null)
                    {
                        var categoryIds = celebrity.ShowCelebrityRoles.Select(s => s.CategoryId);
                        var episodeIds = context.EpisodeCategories1.Where(e => categoryIds.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                        var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                        if (episodes != null)
                        {
                            jfi = new List<HomepageFeatureItem>();
                            foreach (var episode in episodes)
                            {
                                string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    id = episode.EpisodeId,
                                    description = episode.Description,
                                    name = episode.EpisodeName,
                                    airdate = (episode.DateAired != null) ? episode.DateAired.Value.ToString("MMMM d, yyyy") : "",
                                    imgurl = img,
                                    blurb = HttpUtility.HtmlEncode(episode.Synopsis),
                                    slug = MyUtility.GetSlug(episode.EpisodeName)
                                };
                                jfi.Add(j);
                            }
                            jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                            cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                        }
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                if (jfi != null)
                    obj = jfi.Skip(skipSize).Take(size).ToList();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(obj);
        }

        public PartialViewResult LoadMoreGames(int id, int? pageSize, int page = 0)
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
                string cacheKey = "UAAPGTGES:O:" + id.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }

                ViewBag.id = id;
                ViewBag.page = page + 1;
                ViewBag.pageSize = pageSize;

                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id);
                    if (celebrity != null)
                    {
                        var categoryIds = celebrity.ShowCelebrityRoles.Select(s => s.CategoryId);
                        var episodeIds = context.EpisodeCategories1.Where(e => categoryIds.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                        var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                        if (episodes != null)
                        {
                            jfi = new List<HomepageFeatureItem>();
                            foreach (var episode in episodes)
                            {
                                string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    id = episode.EpisodeId,
                                    description = episode.Description,
                                    name = episode.EpisodeName,
                                    airdate = (episode.DateAired != null) ? episode.DateAired.Value.ToString("MMMM d, yyyy") : "",
                                    imgurl = img,
                                    blurb = HttpUtility.HtmlEncode(episode.Synopsis),
                                    slug = MyUtility.GetSlug(episode.EpisodeName)
                                };
                                jfi.Add(j);
                            }
                            jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                            cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                        }
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                if (jfi != null)
                    obj = jfi.Skip(skipSize).Take(size).ToList();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(obj);

        }

        public PartialViewResult GetUAAPGameEpisodes(string id, string partialViewName, int? pageSize, int episodeId, int page = 0, bool is_active = false)
        {
            var registDt = DateTime.Now;
            List<Episode> obj = null;
            try
            {
                var context = new IPTV2Entities();
                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == episodeId);
                if (episode != null)
                {
                    var duration = new TimeSpan(0, GlobalConfig.GetParentCategoriesCacheDuration, 0);
                    var parentShows = episode.GetParentShows(duration);
                    parentShows.Remove(GlobalConfig.UAAPMainCategoryId);
                    var episodeIds = context.EpisodeCategories1.Where(e => parentShows.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                    var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                    if (episodes != null)
                    {
                        episodes = episodes.OrderByDescending(e => e.DateAired).ThenByDescending(e => e.AuditTrail.CreatedOn);
                        obj = pageSize == null ? episodes.ToList() : episodes.Skip(page).Take((int)pageSize).ToList();
                    }

                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
            {
                ViewBag.id = id;
                ViewBag.IsActive = is_active;
                return PartialView(partialViewName, obj);
            }
            return PartialView(obj);
        }

        public ActionResult OnDemand(int? id, string slug)
        {
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            try
            {
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
                                    if (!parentCategories.Contains(GlobalConfig.UAAPMainCategoryId))
                                        return RedirectToActionPermanent("Details", "Episode", new { id = id, slug = dbSlug });
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
                                        var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                                        var isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
                                        if (isEmailAllowed)
                                            CountryCode = user.CountryCode;
                                    }
                                }
                                var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                            }
                            else
                            {
                                var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                            }

                            if (episodeCategory == null)
                            {
                                var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && !excludedCategoryIds.Contains(e.CategoryId));
                            }

                            if (episodeCategory != null)
                            {
                                if (episodeCategory.Show.EndDate < registDt || episodeCategory.Show.StatusId != GlobalConfig.Visible)
                                    return RedirectToAction("Index", "Home");

                                ViewBag.Loved = false; ViewBag.Rated = false; // Social
                                ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };

                                var tempShowNameWithDate = String.Format("{0} {1}", episodeCategory.Show.Description, episode.DateAired.Value.ToString("MMMM d yyyy"));
                                dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
                                if (String.Compare(dbSlug, slug, false) != 0)
                                    return RedirectToActionPermanent("OnDemand", new { id = id, slug = dbSlug });
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

                                using (profiler.Step("Check for preview asset"))
                                {
                                    var previewAsset = episode.PreviewAssets.LastOrDefault();
                                    if (previewAsset != null)
                                    {
                                        Asset asset = previewAsset.Asset;
                                        if (asset != null)
                                            if (asset.AssetCdns != null)
                                                if (asset.AssetCdns.Count(a => a.CdnId == 2) > 0)
                                                    ViewBag.HasPreviewAsset = true;
                                    }
                                }

                                if (!ContextHelper.IsCategoryViewableInUserCountry(episodeCategory.Show, CountryCode))
                                    if (!isEmailAllowed)
                                        return RedirectToAction("Index", "Home");

                                bool isExclusive = IsEpisodePartOfTheExclusiveFeature(context, episode);
                                ViewBag.isExclusiveEpisode = isExclusive;

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

                                ViewBag.EpisodeNumber = 0;
                                if (!(episodeCategory.Show is Movie || episodeCategory.Show is SpecialShow))
                                {
                                    using (profiler.Step("Get Episode Number"))
                                    {
                                        var listOfEpisodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == episodeCategory.CategoryId).Select(e => e.EpisodeId);
                                        var episodeIdsOrderedByDate = context.Episodes.Where(e => listOfEpisodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).Select(e => e.EpisodeId);
                                        if (episodeIdsOrderedByDate != null)
                                        {
                                            var EpIds = episodeIdsOrderedByDate.ToList();
                                            var EpisodeCount = EpIds.Count();
                                            var indexOfCurrentEpisode = EpIds.IndexOf(episode.EpisodeId);
                                            ViewBag.EpisodeNumber = EpisodeCount - indexOfCurrentEpisode;
                                            ViewBag.NextEpisodeId = EpIds.ElementAt(indexOfCurrentEpisode <= 0 ? indexOfCurrentEpisode : indexOfCurrentEpisode - 1); //Next episode id
                                            ViewBag.PreviousEpisodeId = EpIds.ElementAt(indexOfCurrentEpisode >= EpisodeCount - 1 ? indexOfCurrentEpisode : indexOfCurrentEpisode + 1); //Previous episode id
                                            ViewBag.EpisodeCount = EpisodeCount; //Total episode count
                                        }
                                    }
                                }
                                ViewBag.dbSlug = dbSlug;
                                ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable

                                if (!Request.Cookies.AllKeys.Contains("version"))
                                {
                                    if (GlobalConfig.UseJWPlayer)
                                    {
                                        if (Request.Cookies.AllKeys.Contains("hplayer"))
                                        {
                                            if (Request.Cookies["hplayer"].Value == "2")
                                                return View("OnDemandFlowHLS", episode);
                                            else
                                                return View("OnDemand2", episode);
                                        }
                                        else
                                            return View("OnDemand3", episode);
                                    }
                                    else
                                        return View("OnDemand2", episode);
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
