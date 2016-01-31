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
    public class TFCkatController : Controller
    {

        public ActionResult Index()
        {
            //ViewBag.CoverItLiveAltCastCode = GlobalConfig.TFCkatAltCastCode;
            //return View();
            return RedirectToAction("Index", "Home");

        }

        public ActionResult Mechanics()
        {
            return View();
        }
        public ActionResult Finalists(int? id)
        {
            try
            {
                List<Celebrity> jfi = null;
                var socialContext = new EngagementsEntities();
                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == GlobalConfig.EventCelebritiesFeatureID && f.StatusId == GlobalConfig.Visible);

                List<FeatureItem> featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).ToList();
                List<int> idlist = new List<int>();
                foreach (var f in featureItems)
                {
                    if (f is CelebrityFeatureItem)
                    {
                        var cft = (CelebrityFeatureItem)f;
                        idlist.Add(cft.CelebrityId);
                    }
                }
                var celeb = context.Celebrities.Where(c => idlist.Contains(c.CelebrityId));
                jfi = new List<Celebrity>();
                foreach (Celebrity person in celeb)
                {
                    bool hasLoved = MyUtility.isUserLoggedIn() && ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, (int)person.CelebrityId, EngagementContentType.Celebrity);
                    string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                    var lovesCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == person.CelebrityId && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                    int love = (int)(lovesCountSummary == null ? 0 : lovesCountSummary.Total);
                    var commentsCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == person.CelebrityId && i.ReactionTypeId == GlobalConfig.SOCIAL_COMMENT);
                    int comment = (int)(commentsCountSummary == null ? 0 : commentsCountSummary.Total);
                    person.ImageUrl = img;
                    person.Birthday = love.ToString() + "-" + comment.ToString();
                    Celebrity c = new Celebrity() { CelebrityId = person.CelebrityId, ImageUrl = person.ImageUrl, FullName = person.FullName, Birthday = person.Birthday, ZodiacSign = person.ZodiacSign, ChineseYear = person.ChineseYear, Birthplace = hasLoved.ToString() };
                    jfi.Add(c);
                }
                return View(jfi);
            }
            catch (Exception) { throw; }
        }

        public ActionResult ContenderDetails(int? id)
        {
            if (id != null)
            {
                var context = new IPTV2Entities();
                var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id);
                var socialContext = new EngagementsEntities();
                if (celebrity != null)
                {
                    if (String.IsNullOrEmpty(celebrity.Description))
                        celebrity.Description = "No description yet.";

                    if (String.IsNullOrEmpty(celebrity.ImageUrl))
                        celebrity.ImageUrl = String.Format("{0}/{1}", GlobalConfig.AssetsBaseUrl, "content/images/celebrity/unknown.jpg");
                    else
                        celebrity.ImageUrl = String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, celebrity.CelebrityId, celebrity.ImageUrl);

                    if (MyUtility.isUserLoggedIn() && ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, (int)id, EngagementContentType.Celebrity))
                        ViewBag.Loved = true;
                    var lovesCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == celebrity.CelebrityId && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                    int love = (int)(lovesCountSummary == null ? 0 : lovesCountSummary.Total);
                    celebrity.Weight = love.ToString();
                    if (!String.IsNullOrEmpty(celebrity.Birthday))
                        ViewBag.BirthDate = celebrity.Birthday;
                    if (!String.IsNullOrEmpty(celebrity.Birthplace))
                    {
                        ViewBag.BirthPlace = celebrity.Birthplace;
                        celebrity.Birthplace = "<p>" + celebrity.FirstName + " VS " + celebrity.Birthplace.Replace(",", "</p><p>" + celebrity.FirstName + " VS ") + "</p>";
                        celebrity.Birthplace = celebrity.Birthplace.Replace(":", "<br />");
                    }
                    return View(celebrity);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetContenderEpisodes(int? id)
        {
            List<JsonFeatureItem> jfi = null;
            if (id != null)
            {
                var registDt = DateTime.Now;
                jfi = new List<JsonFeatureItem>();
                var context = new IPTV2Entities();
                var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id);
                if (celebrity != null)
                {
                    var episodeIds = celebrity.EpisodeCelebrityRoles.Select(s => s.EpisodeId);
                    var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                    foreach (var ep in episodes)
                    {
                        var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                        var episodeCategory = ep.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                        var show = episodeCategory.Show;
                        var name = show.Description;
                        string img = String.IsNullOrEmpty(ep.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ep.EpisodeId.ToString(), ep.ImageAssets.ImageVideo);
                        string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                        JsonFeatureItem j = new JsonFeatureItem() { EpisodeId = ep.EpisodeId, EpisodeDescription = MyUtility.Ellipsis(ep.Description, 20), EpisodeName = ep.EpisodeName, EpisodeAirDate = (ep.DateAired != null) ? ep.DateAired.Value.ToString("MMM d, yyyy") : "", ShowId = show.CategoryId, ShowName = MyUtility.Ellipsis(show.CategoryName, 20), EpisodeImageUrl = img, ShowImageUrl = showImg, Blurb = MyUtility.Ellipsis(ep.Synopsis, 80) };
                        jfi.Add(j);
                    }
                }
            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }
        public ActionResult OnDemand(int? id, string slug)
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

                var dbSlug = MyUtility.GetSlug(episode.Description);
                var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                var parentCategories = episode.GetParentShows(CacheDuration);
                if (parentCategories.Count() > 0)
                {
                    if (!(parentCategories.Contains(GlobalConfig.TFCkatCategoryId) || parentCategories.Contains(GlobalConfig.TFCkatExclusivesCategoryId)))
                        return RedirectToActionPermanent("Details", "Episode", new { id = id, slug = dbSlug });
                }
                var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                EpisodeCategory category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && !excludedCategoryIds.Contains(e.CategoryId));
                ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                ViewBag.Loved = false; ViewBag.Rated = false; // Social
                if (User.Identity.IsAuthenticated)
                {
                    System.Guid userId = new System.Guid(HttpContext.User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;

                    using (profiler.Step("Social Love"))
                    {
                        ViewBag.Loved = ContextHelper.HasSocialEngagement(user.UserId, GlobalConfig.SOCIAL_LOVE, id, EngagementContentType.Episode);
                    }
                    using (profiler.Step("Social Rating"))
                    {
                        ViewBag.Rated = ContextHelper.HasSocialEngagement(user.UserId, GlobalConfig.SOCIAL_RATING, id, EngagementContentType.Episode);
                    }

                    ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, category.CategoryId, DateTime.Now);
                }
                else
                {
                    using (profiler.Step("Check for Active Entitlements (Not Logged In)")) //check for active entitlements based on categoryId
                    {
                        string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, category.CategoryId, registDt, CountryCode);
                    }
                }

                if (category != null)
                {
                    ViewBag.Show = category.Show;
                    var tempShowNameWithDate = String.Format("{0} {1}", category.Show.Description, episode.DateAired.Value.ToString("MMMM d, yyyy"));
                    dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
                    if (String.Compare(dbSlug, slug, false) != 0)
                        return RedirectToActionPermanent("OnDemand", new { id = id, slug = dbSlug });

                    ViewBag.CategoryType = "Show";
                    if (category.Show is Movie)
                        ViewBag.CategoryType = "Movie";
                    else if (category.Show is SpecialShow)
                        ViewBag.CategoryType = "SpecialShow";
                    else if (category.Show is WeeklyShow)
                        ViewBag.CategoryType = "WeeklyShow";
                    else if (category.Show is DailyShow)
                        ViewBag.CategoryType = "DailyShow";

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
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetPaginatedEpisodeList()
        {
            List<JsonFeatureItem> jfi = null;

            jfi = new List<JsonFeatureItem>();
            var context = new IPTV2Entities();
            SortedSet<int> episodeids = new SortedSet<int>();
            var registDt = DateTime.Now;
            var episodeIDList = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.TFCkatCategoryId).Select(e => e.Episode.EpisodeId);
            int epCount = episodeIDList.Count();
            var episodeList = context.Episodes.Where(e => episodeIDList.Contains(e.EpisodeId) && e.StatusId == GlobalConfig.Visible);
            foreach (var ep in episodeList)
            {
                string img = String.IsNullOrEmpty(ep.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ep.EpisodeId.ToString(), ep.ImageAssets.ImageVideo);
                JsonFeatureItem j = new JsonFeatureItem() { EpisodeId = ep.EpisodeId, EpisodeDescription = MyUtility.Ellipsis(ep.Synopsis, 160), EpisodeName = ep.EpisodeName, EpisodeAirDate = (ep.DateAired != null) ? ep.DateAired.Value.ToString("MMM d, yyyy") : "", EpisodeImageUrl = img };
                jfi.Add(j);
            }

            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetPaginatedExclusiveList()
        {
            List<JsonFeatureItem> jfi = null;
            jfi = new List<JsonFeatureItem>();
            var context = new IPTV2Entities();
            SortedSet<int> episodeids = new SortedSet<int>();
            var registDt = DateTime.Now;
            List<Episode> episodes = new List<Episode>();
            var featureItems = context.FeatureItems.Where(f => f.FeatureId == GlobalConfig.TFCkatExclusivesCategoryId && f.StatusId == GlobalConfig.Visible).Take(12);
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

            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }
        const int pageSize = 12;
        public ActionResult ListAllEpisodes(int id = 1)
        {
            int page = id;
            var context = new IPTV2Entities();
            var registDt = DateTime.Now;
            var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
            var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.TFCkatCategoryId && e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderBy(e => e.EpisodeId).Select(e => e.EpisodeId);
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
                return RedirectToAction("ListAllGameEpisodes", "TFCkat", new { id = String.Empty });
            if (episodes != null)
                return View(episodes);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult GrandWinners(int? id)
        {
            try
            {
                List<Celebrity> jfi = null;
                var socialContext = new EngagementsEntities();
                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == GlobalConfig.TFCkatGrandWinners && f.StatusId == GlobalConfig.Visible);

                List<FeatureItem> featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).ToList();
                List<int> idlist = new List<int>();
                foreach (var f in featureItems)
                {
                    if (f is CelebrityFeatureItem)
                    {
                        var cft = (CelebrityFeatureItem)f;
                        idlist.Add(cft.CelebrityId);
                    }
                }
                var celeb = context.Celebrities.Where(c => idlist.Contains(c.CelebrityId));
                jfi = new List<Celebrity>();
                foreach (Celebrity person in celeb)
                {
                    bool hasLoved = MyUtility.isUserLoggedIn() && ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, (int)person.CelebrityId, EngagementContentType.Celebrity);
                    string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                    var lovesCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == person.CelebrityId && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                    int love = (int)(lovesCountSummary == null ? 0 : lovesCountSummary.Total);
                    var commentsCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == person.CelebrityId && i.ReactionTypeId == GlobalConfig.SOCIAL_COMMENT);
                    int comment = (int)(commentsCountSummary == null ? 0 : commentsCountSummary.Total);
                    person.ImageUrl = img;
                    person.Birthday = love.ToString() + "-" + comment.ToString();
                    Celebrity c = new Celebrity() { CelebrityId = person.CelebrityId, ImageUrl = person.ImageUrl, FullName = person.FullName, Birthday = person.Birthday, ZodiacSign = person.ZodiacSign, ChineseYear = person.ChineseYear, Birthplace = hasLoved.ToString() };
                    jfi.Add(c);
                }
                return View(jfi);
            }
            catch (Exception) { throw; }
        }

    }
}
