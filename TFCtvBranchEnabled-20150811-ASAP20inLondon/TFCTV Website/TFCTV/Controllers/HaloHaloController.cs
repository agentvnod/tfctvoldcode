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
    public class HaloHaloController : Controller
    {
        //
        // GET: /HaloHalo/

        public ActionResult Video(int? id, string slug)
        {
            var profiler = MiniProfiler.Current;
            var context = new IPTV2Entities();
            EpisodeCategory category;
            bool redirectPerma = false;
            if (id == null)
            {
                category = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.DigitalShortsCategoryId).OrderBy(e => Guid.NewGuid()).FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible);
                redirectPerma = true;
            }
            else
                category = context.EpisodeCategories1.FirstOrDefault(e => e.CategoryId == GlobalConfig.DigitalShortsCategoryId && e.EpisodeId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible);

            if (category != null)
            {
                if (redirectPerma)
                    return RedirectToActionPermanent("Video", new { id = category.EpisodeId });

                var dbSlug = MyUtility.GetSlug(category.Episode.EpisodeName);
                if (String.Compare(dbSlug, slug, false) != 0)
                    return RedirectToActionPermanent("Video", new { id = id, slug = dbSlug });

                ViewBag.Loved = false;
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, category.EpisodeId, EngagementContentType.Episode);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }
                ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                var registDt = DateTime.Now;
                if (User.Identity.IsAuthenticated)
                {
                    System.Guid userId = new System.Guid(HttpContext.User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, category.CategoryId, DateTime.Now);
                }
                else
                {
                    var CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                    using (profiler.Step("Check for Active Entitlements (Not Logged In)")) //check for active entitlements based on categoryId
                    {
                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, category.CategoryId, registDt, CountryCode);
                    }
                }
                ////CHECK USER IF CAN PLAY VIDEO
                //using (profiler.Step("Check if User is Entitled"))
                //{
                //    try
                //    {
                //        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                //        var premiumAsset = category.Episode.PremiumAssets.FirstOrDefault();
                //        if (premiumAsset != null)
                //        {
                //            var asset = premiumAsset.Asset;
                //            //isUserEntitled = user.IsEpisodeEntitled(offering, episode, asset, RightsType.Online);
                //            //isUserEntitled = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                //            isUserEntitled = ContextHelper.CanPlayVideo(context, offering, category.Episode, asset, User, Request);
                //        }
                //    }
                //    catch (Exception) { }
                //}

                //ViewBag.IsUserEntitled = isUserEntitled;
                ViewBag.dbSlug = dbSlug;
                if (!Request.Cookies.AllKeys.Contains("version"))
                {
                    if (GlobalConfig.UseJWPlayer)
                    {
                        if (Request.Cookies.AllKeys.Contains("hplayer"))
                        {
                            if (Request.Cookies["hplayer"].Value == "2")
                                return View("VideoFlowHLS", category);
                            else if (Request.Cookies["hplayer"].Value == "7")
                                return View("VideoJwplayer7Akamai", category);
                            else
                                return View("Video2", category);
                        }
                        else
                        {
                            return View("VideoJwplayerAkamai", category);
                            //return View("Video3", category);
                        }
                    }
                    else
                        return View("Video2", category);
                }
                return View(category);
            }
            return RedirectToAction("Index", "Home");
        }

        const int pageSize = 12;
        public ActionResult List(int id = 1)
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("List2");

            int page = id;

            var context = new IPTV2Entities();
            var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.DigitalShortsCategoryId && e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderByDescending(e => e.Episode.AuditTrail.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => e.EpisodeId);

            var totalCount = context.EpisodeCategories1.Count(e => e.CategoryId == GlobalConfig.DigitalShortsCategoryId && e.Episode.OnlineStatusId == GlobalConfig.Visible);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            var totalPage = Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalPages = totalPage;
            ViewBag.TotalCount = totalCount;
            var maxCount = page * pageSize > totalCount ? totalCount : page * pageSize;
            ViewBag.OutOf = String.Format("{0} - {1}", (page * pageSize) + 1 - pageSize, maxCount);
            var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible).OrderByDescending(e => e.AuditTrail.UpdatedOn).ThenByDescending(e => e.EpisodeId);

            ViewBag.Previous = page == 1 ? String.Empty : (page - 1) == 1 ? String.Empty : (page - 1).ToString();
            ViewBag.Next = page == (int)totalPage ? (int)totalPage : page + 1;

            if ((page * pageSize) + 1 - pageSize > totalCount)
                return RedirectToAction("List", "HaloHalo", new { id = String.Empty });
            if (episodes != null)
                return View(episodes);
            return RedirectToAction("Index", "Home");
        }

        public PartialViewResult GetFeatures(int id, string containerId, bool IsActive = false)
        {
            List<HomepageFeatureItem> jfi = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            try
            {
                ViewBag.containerId = containerId;
                ViewBag.IsActive = IsActive;
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
                                                    slug = MyUtility.GetSlug(episode.EpisodeName)
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
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(jfi);
        }

        public PartialViewResult BuildContentList(int? pageSize, int page = 0)
        {
            List<HomepageFeatureItem> jfi = null;
            List<HomepageFeatureItem> obj = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            int size = GlobalConfig.GenericListContentSize;
            int skipSize = 0;
            try
            {
                if (pageSize != null)
                    size = (int)pageSize;
                skipSize = size * page;
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "DSHH:O:" + GlobalConfig.DigitalShortsCategoryId;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }

                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.DigitalShortsCategoryId).Select(e => e.EpisodeId);
                    var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId)).OrderByDescending(e => e.EpisodeId).ThenByDescending(e => e.AuditTrail.UpdatedOn);
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

        public PartialViewResult LoadMoreContent(int? pageSize, int page = 0)
        {
            List<HomepageFeatureItem> jfi = null;
            List<HomepageFeatureItem> obj = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            int size = GlobalConfig.GenericListContentSize;
            int skipSize = 0;
            try
            {
                if (pageSize != null)
                    size = (int)pageSize;
                skipSize = size * page;
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "DSHH:O:" + GlobalConfig.DigitalShortsCategoryId;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }

                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.DigitalShortsCategoryId).Select(e => e.EpisodeId);
                    var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId)).OrderByDescending(e => e.EpisodeId).ThenByDescending(e => e.AuditTrail.UpdatedOn);
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
