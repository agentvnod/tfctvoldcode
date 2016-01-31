using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;
using EngagementsModel;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class CelebrityController : Controller
    {
        //
        // GET: /Celebrity/

        public ActionResult Index()
        {
            //var userName = new IPTV2_Model.User().FirstName;

            return View();
        }

        public ActionResult Profile(int? id, string slug)
        {
            if (id != null)
            {
                ViewBag.IsDolphySkinEnabled = false;
                if (id == GlobalConfig.DolphyCelebrityId)
                    ViewBag.IsDolphySkinEnabled = true;
                var UAAPTeamsCelebrityID = MyUtility.StringToIntList(GlobalConfig.UAAPTeamsCelebrityIDs);
                if (UAAPTeamsCelebrityID.Contains((int)id))
                    return RedirectToAction("TeamDetails", "UAAP", new { id = id });
                var context = new IPTV2Entities();

                var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id);

                if (celebrity != null)
                {

                    var dbSlug = MyUtility.GetSlug(String.IsNullOrEmpty(celebrity.FullName) ? String.Format("{0} {1}", celebrity.FirstName, celebrity.LastName) : celebrity.FullName);
                    if (String.Compare(dbSlug, slug, false) != 0)
                        return RedirectToActionPermanent("Profile", new { id = id, slug = dbSlug });

                    if (celebrity.Description == null)
                        celebrity.Description = "No description yet.";
                    if (celebrity.ImageUrl == null)
                        celebrity.ImageUrl = String.Format("{0}/{1}", GlobalConfig.AssetsBaseUrl, "content/images/celebrity/unknown.jpg");
                    else
                        celebrity.ImageUrl = String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, celebrity.CelebrityId, celebrity.ImageUrl);

                    if (MyUtility.isUserLoggedIn() && HasSocialEngagementRecordOnCelebrity(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, (int)id))
                        ViewBag.Loved = true;

                    if (celebrity.Birthday != null && celebrity.Birthday.Length > 0)
                        ViewBag.BirthDate = "Birthday: " + celebrity.Birthday;
                    if (celebrity.Birthplace != null && celebrity.Birthplace.Length > 0)
                        ViewBag.BirthPlace = "Birthplace: " + celebrity.Birthplace;
                    ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
                    ViewBag.dbSlug = dbSlug;
                    if (!Request.Cookies.AllKeys.Contains("version"))
                        return View("Profile2", celebrity);
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

        //Get shows of a particular celebrity
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetCelebrityShows(int id)
        {
            var context = new IPTV2Entities();
            var celebrity = context.Celebrities.Find(id);
            var registDt = DateTime.Now;
            List<JsonFeatureItem> jfi = null;
            try
            {
                var obj = celebrity.ShowCelebrityRoles.Where(celeb => celeb.CelebrityId == celebrity.CelebrityId).Select(s => s.Show);
                if (obj != null)
                {
                    jfi = new List<JsonFeatureItem>();
                    foreach (Show show in obj.OrderByDescending(s => s.StartDate))
                    {
                        if (show is Movie)
                        {
                            //does nothing
                        }
                        else
                        {
                            if (show.StatusId == GlobalConfig.Visible)
                            {
                                if (show.StartDate < registDt && show.EndDate > registDt)
                                {
                                    JsonFeatureItem j = new JsonFeatureItem()
                                    {
                                        ShowId = show.CategoryId,
                                        ShowName = show.CategoryName,
                                        ShowDescription = show.Description,
                                        ShowImageUrl = GlobalConfig.ShowImgPath + show.CategoryId + "/" + show.ImagePoster
                                    };
                                    jfi.Add(j);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        //Get shows of a particular celebrity
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetCelebrityMovies(int id)
        {
            var context = new IPTV2Entities();
            var celebrity = context.Celebrities.Find(id);
            var registDt = DateTime.Now;
            List<JsonFeatureItem> jfi = null;

            try
            {
                var obj = celebrity.ShowCelebrityRoles.Where(celeb => celeb.CelebrityId == celebrity.CelebrityId).Select(s => s.Show);
                if (obj != null)
                {
                    jfi = new List<JsonFeatureItem>();
                    foreach (Show show in obj.OrderByDescending(s => s.StartDate))
                    {
                        var name = show.CategoryName;
                        if (show is Movie)
                        {
                            if (show.StatusId == GlobalConfig.Visible)
                            {
                                if (show.StartDate < registDt && show.EndDate > registDt)
                                {
                                    JsonFeatureItem j = new JsonFeatureItem()
                                    {
                                        ShowId = show.CategoryId,
                                        ShowName = show.CategoryName,
                                        ShowDescription = show.Description,
                                        ShowImageUrl = GlobalConfig.ShowImgPath + show.CategoryId + "/" + show.ImagePoster
                                    };
                                    jfi.Add(j);
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception) { }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        //Get episodes of a particular celebrity
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetCelebrityEpisodes(int id)
        {
            var context = new IPTV2Entities();
            var celebrity = context.Celebrities.Find(id);
            var registDt = DateTime.Now;
            var obj = celebrity.EpisodeCelebrityRoles.Where(celeb => celeb.CelebrityId == celebrity.CelebrityId).Select(s => s.Episode).ToList();

            List<JsonFeatureItem> jfi = new List<JsonFeatureItem>();
            foreach (var ep in obj)
            {
                if (ep.OnlineStatusId == GlobalConfig.Visible)
                {
                    if (ep.OnlineStartDate < registDt && ep.OnlineEndDate > registDt)
                    {
                        var episodeCategory = ep.EpisodeCategories.FirstOrDefault(e => e.CategoryId != GlobalConfig.FreeTvCategoryId);
                        var show = episodeCategory.Show;
                        var name = show.Description;
                        string img = String.IsNullOrEmpty(ep.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ep.EpisodeId.ToString(), ep.ImageAssets.ImageVideo);
                        string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                        JsonFeatureItem j = new JsonFeatureItem() { EpisodeId = ep.EpisodeId, EpisodeDescription = MyUtility.Ellipsis(ep.Description, 20), EpisodeName = ep.EpisodeName, EpisodeAirDate = (ep.DateAired != null) ? ep.DateAired.Value.ToString("MMMM d, yyyy") : "", ShowId = show.CategoryId, ShowName = MyUtility.Ellipsis(show.CategoryName, 20), EpisodeImageUrl = img, ShowImageUrl = showImg, Blurb = MyUtility.Ellipsis(ep.Synopsis, 80) };
                        jfi.Add(j);
                    }
                }

            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetCelebrities()
        {
            var context = new IPTV2Entities();
            var celebrities = context.Celebrities.ToList().OrderBy(celeb => celeb.FirstName);
            List<JsonFeatureItem> jfi = new List<JsonFeatureItem>();

            foreach (Celebrity celebrity in celebrities)
            {
                string celebrityImg = String.IsNullOrEmpty(celebrity.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, celebrity.CelebrityId, celebrity.ImageUrl);
                JsonFeatureItem j = new JsonFeatureItem()
                {
                    CelebrityFullName = celebrity.FullName,
                    ShowDescription = celebrity.FirstName + "-" + celebrity.LastName,
                    ShowId = celebrity.CelebrityId,
                    ShowImageUrl = celebrityImg
                };
                jfi.Add(j);
            }

            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuildCelebrityContent(int id, string sectionTitle, string containerId, int? pageSize, int page = 0, string featureType = "episode", bool removeShowAll = false, CelebrityContentType contentType = CelebrityContentType.SHOWS)
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
                string cacheKey = "BCSLS:C:" + id.ToString();
                if (contentType == CelebrityContentType.MOVIES)
                    cacheKey = "BCSLS:M:" + id.ToString();
                else if (contentType == CelebrityContentType.EPISODES)
                    cacheKey = "BCSLS:E:" + id.ToString();
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id);
                    if (celebrity != null)
                    {
                        if (contentType == CelebrityContentType.EPISODES)
                        {
                            var episodes = celebrity.EpisodeCelebrityRoles.Select(c => c.Episode);
                            if (episodes != null)
                            {
                                jfi = new List<HomepageFeatureItem>();
                                foreach (var episode in episodes.OrderByDescending(e => e.DateAired))
                                {
                                    if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                                    {
                                        var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                        EpisodeCategory epCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                                        if (epCategory != null)
                                        {
                                            Show show = epCategory.Show;
                                            string img = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                            HomepageFeatureItem j = new HomepageFeatureItem()
                                            {
                                                id = episode.EpisodeId,
                                                name = show.Description,
                                                blurb = HttpUtility.HtmlEncode(show.Blurb),
                                                imgurl = img,
                                                show_id = show.CategoryId,
                                                slug = MyUtility.GetSlug(episode.IsLiveChannelActive == true ? episode.Description : String.Format("{0} {1}", show.Description, episode.DateAired.Value.ToString("MMMM d yyyy")))
                                            };
                                            jfi.Add(j);
                                        }
                                    }
                                }
                                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                                cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                            }
                        }
                        else
                        {
                            var shows = celebrity.ShowCelebrityRoles.Select(c => c.Show);
                            if (shows != null)
                            {
                                jfi = new List<HomepageFeatureItem>();
                                foreach (var show in shows.OrderByDescending(s => s.StartDate))
                                {
                                    if (contentType == CelebrityContentType.SHOWS)
                                    {
                                        if (show.StatusId == GlobalConfig.Visible && !(show is Movie) && show.StartDate < registDt && show.EndDate > registDt)
                                        {
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
                                    }
                                    else if (contentType == CelebrityContentType.MOVIES)
                                    {
                                        if (show.StatusId == GlobalConfig.Visible && show is Movie && show.StartDate < registDt && show.EndDate > registDt)
                                        {
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
                                    }
                                }
                                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                                cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                            }
                        }
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                ViewBag.FeatureType = featureType;
                ViewBag.SectionTitle = sectionTitle;
                ViewBag.id = id;
                ViewBag.containerId = containerId;
                ViewBag.pageSize = size;
                ViewBag.RemoveShowAll = removeShowAll;
                ViewBag.ContentType = contentType;
                if (jfi != null)
                    obj = jfi.ToList();
                    //obj = jfi.Skip(skipSize).Take(size).ToList();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView("BuildSection", obj);
        }

        public ActionResult LoadMoreItems(int id, string sectionTitle, string containerId, int? pageSize, int page = 0, string featureType = "episode", bool removeShowAll = false, CelebrityContentType contentType = CelebrityContentType.SHOWS)
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
                string cacheKey = "BCSLS:C:" + id.ToString();
                if (contentType == CelebrityContentType.MOVIES)
                    cacheKey = "BCSLS:M:" + id.ToString();
                else if (contentType == CelebrityContentType.EPISODES)
                    cacheKey = "BCSLS:E:" + id.ToString();
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id);
                    if (celebrity != null)
                    {
                        if (contentType == CelebrityContentType.EPISODES)
                        {
                            var episodes = celebrity.EpisodeCelebrityRoles.Select(c => c.Episode);
                            if (episodes != null)
                            {
                                jfi = new List<HomepageFeatureItem>();
                                foreach (var episode in episodes)
                                {
                                    if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                                    {
                                        var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                        EpisodeCategory epCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                                        Show show = epCategory.Show;
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
                                }
                                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                                cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                            }
                        }
                        else
                        {
                            var shows = celebrity.ShowCelebrityRoles.Select(c => c.Show);
                            if (shows != null)
                            {
                                jfi = new List<HomepageFeatureItem>();
                                foreach (var show in shows)
                                {
                                    if (contentType == CelebrityContentType.SHOWS)
                                    {
                                        if (show.StatusId == GlobalConfig.Visible && !(show is Movie) && show.StartDate < registDt && show.EndDate > registDt)
                                        {
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
                                    }
                                    else if (contentType == CelebrityContentType.MOVIES)
                                    {
                                        if (show.StatusId == GlobalConfig.Visible && show is Movie && show.StartDate < registDt && show.EndDate > registDt)
                                        {
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
                                    }
                                }
                                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                                cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                            }
                        }
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                ViewBag.FeatureType = featureType;
                ViewBag.SectionTitle = sectionTitle;
                ViewBag.id = id;
                ViewBag.containerId = containerId;
                ViewBag.pageSize = size;
                ViewBag.RemoveShowAll = removeShowAll;
                ViewBag.ContentType = contentType;
                if (jfi != null)
                    obj = jfi.Skip(skipSize).Take(size).ToList();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView("BuildSection", obj);
        }

        [RequireHttp]
        public ActionResult List(int? id, string slug)
        {
            try
            {
                if (id != null)
                {
                    ViewBag.featureType = "celebrity";
                    var context = new IPTV2Entities();
                    var feature = context.Features.FirstOrDefault(f => f.FeatureId == id);
                    if (feature != null)
                        return View("FeatureListCelebrities", feature);
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }
    }
}