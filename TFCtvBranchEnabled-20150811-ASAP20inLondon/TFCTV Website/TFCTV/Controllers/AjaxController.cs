using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

//using DevTrends.MvcDonutCaching;
using IPTV2_Model;
using Newtonsoft.Json;
using TFCTV.Helpers;
using Gigya.Socialize.SDK;
using VideoEngagementsModel;
using System.Diagnostics;
using System.Web.Security;
using System.Web;
using EngagementsModel;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.IO;

namespace TFCTV.Controllers
{
    public class AjaxController : Controller
    {
        /// <summary>
        /// Description: Gets the list of episodes to be used for the FeatureSlides (Latest Full Episodes, Most Viewed, Free TV)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(VaryByParam = "id", Duration = 300)]
        public JsonResult GetListing(int id)
        {
            List<JsonFeatureItem> jfi = null;

            var cache = DataCache.Cache;
            string cacheKey = "JRGL:O:" + id.ToString() + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            jfi = (List<JsonFeatureItem>)cache[cacheKey];

            if (jfi == null)
            {
                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
                if (feature == null)
                    return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                List<FeatureItem> featureItems = context.FeatureItems.Where(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                jfi = new List<JsonFeatureItem>();
                foreach (EpisodeFeatureItem f in featureItems)
                {
                    if (f is EpisodeFeatureItem)
                    {
                        Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == f.EpisodeId && e.OnlineStatusId == GlobalConfig.Visible);
                        if (ep != null)
                        {
                            //var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                            //var epCategory = ep.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));                            

                            string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                            EpisodeCategory epCategory = null;
                            if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                            {
                                var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                epCategory = ep.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                            }
                            else
                            {
                                var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                epCategory = ep.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                            }
                            if (epCategory != null)
                            {
                                Show show = epCategory.Show;
                                if (show != null)
                                {
                                    string img = String.IsNullOrEmpty(ep.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ep.EpisodeId.ToString(), ep.ImageAssets.ImageVideo);
                                    string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                    JsonFeatureItem j = new JsonFeatureItem()
                                    {
                                        EpisodeId = ep.EpisodeId,
                                        EpisodeDescription = MyUtility.Ellipsis(ep.Description, 20),
                                        EpisodeName = ep.EpisodeName,
                                        EpisodeAirDate = (ep.DateAired != null) ? ep.DateAired.Value.ToString("MMM d, yyyy") : "",
                                        ShowId = show.CategoryId,
                                        ShowName = String.Compare(ep.EpisodeName, ep.EpisodeCode) == 0 ? MyUtility.Ellipsis(show.CategoryName, 20) : MyUtility.Ellipsis(ep.EpisodeName, 20),
                                        EpisodeImageUrl = img,
                                        ShowImageUrl = showImg,
                                        Blurb = HttpUtility.HtmlEncode(MyUtility.Ellipsis(ep.Synopsis, 80))
                                    };
                                    jfi.Add(j);
                                }
                            }
                        }
                    }
                }
                cache.Put(cacheKey, jfi, DataCache.CacheDuration);
            }

            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Description: Gets the list of shows to be used for the FeatureSlides (Latest Shows)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 300)]
        public JsonResult GetLatestShows()
        {
            List<JsonFeatureItem> jfi = null;

            var cache = DataCache.Cache;
            string cacheKey = "JRGLS:C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            jfi = (List<JsonFeatureItem>)cache[cacheKey];

            if (jfi == null)
            {
                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == GlobalConfig.LatestShows && f.StatusId == GlobalConfig.Visible);
                if (feature == null)
                    return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                List<FeatureItem> featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                jfi = new List<JsonFeatureItem>();
                foreach (ShowFeatureItem f in featureItems)
                {
                    if (f is ShowFeatureItem)
                    {
                        Show show = f.Show;
                        string img = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                        JsonFeatureItem j = new JsonFeatureItem() { ShowId = show.CategoryId, ShowName = MyUtility.Ellipsis(show.Description, 22), ShowDescription = MyUtility.Ellipsis(show.Description, 20), ShowImageUrl = img };
                        jfi.Add(j);
                    }
                }
                cache.Put(cacheKey, jfi, DataCache.CacheDuration);
            }

            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Description: Gets the list of shows to be used for the FeatureSlides (Latest Shows)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 300)]
        public JsonResult GetLatestMovies()
        {
            List<JsonFeatureItem> jfi = null;

            var cache = DataCache.Cache;
            string cacheKey = "JRGLM:C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            jfi = (List<JsonFeatureItem>)cache[cacheKey];

            if (jfi == null)
            {
                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == Convert.ToInt32(GlobalConfig.LatestMovies) && f.StatusId == GlobalConfig.Visible);
                if (feature == null)
                    return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                List<FeatureItem> featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                jfi = new List<JsonFeatureItem>();
                foreach (ShowFeatureItem f in featureItems)
                {
                    if (f is ShowFeatureItem)
                    {
                        Show show = f.Show;
                        string img = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                        JsonFeatureItem j = new JsonFeatureItem() { ShowId = show.CategoryId, ShowName = show.Description, ShowDescription = show.Description, ShowImageUrl = img };
                        jfi.Add(j);
                    }
                }
                cache.Put(cacheKey, jfi, DataCache.CacheDuration);
            }

            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Description: Gets the list of shows to be used for the FeatureSlides (Latest Shows)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 300)]
        public JsonResult GetListingShow(int id)
        {
            List<JsonFeatureItem> jfi = null;

            var cache = DataCache.Cache;
            string cacheKey = "JRGLS:U:" + id.ToString() + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            jfi = (List<JsonFeatureItem>)cache[cacheKey];

            if (jfi == null)
            {
                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
                if (feature == null)
                    return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                List<FeatureItem> featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                jfi = new List<JsonFeatureItem>();
                foreach (ShowFeatureItem f in featureItems)
                {
                    if (f is ShowFeatureItem)
                    {
                        Show show = f.Show;
                        string img = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                        JsonFeatureItem j = new JsonFeatureItem() { ShowId = show.CategoryId, ShowName = show.Description, ShowDescription = show.Description, ShowImageUrl = img };
                        jfi.Add(j);
                    }
                }
                cache.Put(cacheKey, jfi, DataCache.CacheDuration);
            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Description: Get featured celebrities
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 300)]
        public JsonResult GetFeaturedCelebrities()
        {
            List<JsonFeatureItem> jfi = null;

            var cache = DataCache.Cache;
            string cacheKey = "JRGFC:C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            jfi = (List<JsonFeatureItem>)cache[cacheKey];

            if (jfi == null)
            {
                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == GlobalConfig.FeaturedCelebrities && f.StatusId == GlobalConfig.Visible);
                if (feature == null)
                    return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                List<FeatureItem> featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                jfi = new List<JsonFeatureItem>();
                foreach (CelebrityFeatureItem f in featureItems)
                {
                    if (f is CelebrityFeatureItem)
                    {
                        Celebrity person = f.Celebrity;
                        string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                        JsonFeatureItem j = new JsonFeatureItem()
                        {
                            CelebrityFullName = person.FullName,
                            ShowId = person.CelebrityId,
                            ShowImageUrl = img
                        };
                        jfi.Add(j);
                    }
                }
                cache.Put(cacheKey, jfi, DataCache.CacheDuration);
            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Description: Get featured celebrities
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 300)]
        public JsonResult GetFeaturedPerson(int id)
        {
            List<JsonFeatureItem> jfi = null;

            var cache = DataCache.Cache;
            string cacheKey = "JRGFP:U:" + id.ToString() + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            jfi = (List<JsonFeatureItem>)cache[cacheKey];

            if (jfi == null)
            {
                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
                List<FeatureItem> featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                if (feature == null)
                    return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                jfi = new List<JsonFeatureItem>();
                foreach (CelebrityFeatureItem f in featureItems)
                {
                    if (f is CelebrityFeatureItem)
                    {
                        Celebrity person = f.Celebrity;
                        JsonFeatureItem j = new JsonFeatureItem()
                        {
                            CelebrityFullName = person.FullName,
                            ShowId = person.CelebrityId,
                            ShowImageUrl = String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl)
                        };
                        jfi.Add(j);
                    }
                }
                cache.Put(cacheKey, jfi, DataCache.CacheDuration);
            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Carousel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        //[OutputCache(VaryByParam = "id", Duration = 300)]
        public JsonResult GetCarousel(int id)
        {
            List<JsonCarouselItem> jci = null;
            string jsonString = String.Empty;
            try
            {
                var cache = DataCache.Cache;
                string cacheKey = "JRGC:U:" + id.ToString() + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
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

                        foreach (CarouselSlide slide in slides)
                        {
                            JsonCarouselItem item = new JsonCarouselItem() { CarouselSlideId = slide.CarouselSlideId, BannerImageUrl = String.Format("{0}{1}/{2}", GlobalConfig.CarouselImgPath, slide.CarouselSlideId.ToString(), slide.BannerImageUrl), Blurb = HttpUtility.HtmlEncode(slide.Blurb), Name = slide.Name, Header = slide.Header, TargetUrl = slide.TargetUrl, ButtonLabel = slide.ButtonLabel };
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
            return Json(jci, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCountries(string text)
        {
            using (var context = new IPTV2Entities())
            {
                var countries = context.Countries.Where(c => c.Code != "--").AsQueryable();
                if (!String.IsNullOrEmpty(text))
                {
                    countries = countries.Where(c => c.Code.StartsWith(text));
                }

                return new JsonResult { Data = new SelectList(countries.ToList(), "Code", "Description") };
            }
        }

        [HttpPost]
        public ActionResult PublishAction(int? id, string type)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            if (id == null || String.IsNullOrEmpty(type))
                return Content(MyUtility.buildJson(collection), "application/json");

            if (!MyUtility.isUserLoggedIn())
                return Content(MyUtility.buildJson(collection), "application/json");

            var context = new IPTV2Entities();
            Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
            if (episode == null)
                return Content(MyUtility.buildJson(collection), "application/json");

            EpisodeCategory category = episode.EpisodeCategories.FirstOrDefault(e => e.EpisodeId == id);
            if (category == null)
                return Content(MyUtility.buildJson(collection), "application/json");
            string CategoryName = category.Show.Description;

            List<ActionLink> actionlinks = new List<ActionLink>();
            actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text, href = SNSTemplates.watch_actionlink_href });
            List<MediaItem> mediaItems = new List<MediaItem>();
            string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format(SNSTemplates.watch_mediaitem_src, GlobalConfig.EpisodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
            mediaItems.Add(new MediaItem() { type = SNSTemplates.watch_mediaitem_type, src = img, href = String.Format(SNSTemplates.watch_mediaitem_href, episode.EpisodeId) });
            UserAction action = new UserAction()
            {
                userMessage = String.Format(SNSTemplates.watch_usermessage, CategoryName),
                title = SNSTemplates.watch_title,
                subtitle = String.Format(SNSTemplates.watch_subtitle, episode.EpisodeId.ToString()),
                linkBack = String.Format(SNSTemplates.watch_linkback, episode.EpisodeId.ToString()),
                description = SNSTemplates.watch_description,
                actionLinks = actionlinks,
                mediaItems = mediaItems,
                actorUID = User.Identity.Name
            };
            GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "external");
            action.description = String.Empty;
            return Content(GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "internal"), "application/json");
        }

        [HttpPost]
        public ActionResult ShareEpisode(int? id, string type, string categoryType, bool? isClip, int? sid)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            if (id == null || String.IsNullOrEmpty(type))
                return Content(MyUtility.buildJson(collection), "application/json");

            if (!MyUtility.isUserLoggedIn())
                return Content(MyUtility.buildJson(collection), "application/json");

            var context = new IPTV2Entities();
            Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
            if (episode == null)
                return Content(MyUtility.buildJson(collection), "application/json");

            System.Guid userId = new Guid(User.Identity.Name);
            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return Content(MyUtility.buildJson(collection), "application/json");

            EpisodeCategory category = episode.EpisodeCategories.LastOrDefault(e => e.EpisodeId == id);
            if (category == null)
                return Content(MyUtility.buildJson(collection), "application/json");
            string CategoryName = category.Show.Description;

            if (sid != null)
            {
                var show = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == sid);
                if (show != null)
                    if (show is Show)
                        CategoryName = ((Show)show).Description;
            }
            if (isClip == null)
                isClip = false;

            string userMessage = (bool)isClip ? String.Format(SNSTemplates.watch_preview_usermessage, CategoryName) : String.Format(SNSTemplates.watch_usermessage, CategoryName);
            string actionTitle = SNSTemplates.watch_title;
            string actionDescription = SNSTemplates.watch_description;
            if (category.Show is Movie)
            {
                actionTitle = SNSTemplates.watch_movie_title;
                actionDescription = SNSTemplates.watch_movie_description;
                userMessage = (bool)isClip ? String.Format(SNSTemplates.watch_movie_preview_usermessage, CategoryName) : String.Format(SNSTemplates.watch_movie_usermessage, CategoryName);
            }

            List<ActionLink> actionlinks = new List<ActionLink>();
            actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.watch_actionlink_href) });
            List<MediaItem> mediaItems = new List<MediaItem>();
            string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format(SNSTemplates.watch_mediaitem_src, GlobalConfig.EpisodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
            mediaItems.Add(new MediaItem() { type = SNSTemplates.watch_mediaitem_type, src = img, href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.watch_mediaitem_href, episode.EpisodeId)) });
            UserAction action = new UserAction()
            {
                userMessage = userMessage,
                title = actionTitle,
                subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.watch_subtitle, episode.EpisodeId.ToString())),
                linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.watch_linkback, episode.EpisodeId.ToString())),
                description = actionDescription,
                actionLinks = actionlinks,
                mediaItems = mediaItems,
                actorUID = User.Identity.Name
            };

            //var userData = GigyaMethods.GetUserData(user.UserId, "ProfileSetting");
            var userData = MyUtility.GetUserPrivacySetting(user.UserId);
            if (userData.IsExternalSharingEnabled.Contains("true"))
                GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "external");
            action.description = String.Empty;
            if (userData.IsInternalSharingEnabled.Contains("true"))
                return Content(GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "internal"), "application/json");
            collection = MyUtility.setError(ErrorCodes.Success);
            return this.Json(collection, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [OutputCache(VaryByParam = "id", Duration = 300)]
        public JsonResult GetCountryState(string id)
        {
            var context = new IPTV2Entities();
            var states = context.States.Where(s => s.CountryCode == id).OrderBy(s => s.Name);
            return this.Json(new SelectList(states, "StateCode", "Name"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult hmGetCountryCount()
        {
            List<hmCountryCount> list = null;
            var cache = DataCache.Cache;
            if (isWhitelistedForHeatMap())
            {
                try
                {
                    string cacheKey = "HMGCCNT:U:;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                    list = (List<hmCountryCount>)cache[cacheKey];
                    if (list == null)
                    {
                        var context = new IPTV2Entities();
                        var item = context.Users.Where(u => u.StatusId == GlobalConfig.Visible)
                              .GroupBy(u => u.CountryCode)
                              .Select(
                               u => new hmCountryCount()
                               {
                                   CountryCode = u.Key,
                                   count = u.Count()
                               }
                              ).ToList();
                        list = item;
                        cache.Put(cacheKey, list, DataCache.CacheDuration);
                    }
                }
                catch (Exception) { }
            }
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            return Content(serializer.Serialize(list), "application/json");
        }

        //Heat Map
        //[OutputCache(VaryByParam = "*", Duration = 300)]
        public ActionResult hmGetAllData(string countryCode, string cityName)
        {
            List<hmUserData> list = null;
            var cache = DataCache.Cache;
            if (isWhitelistedForHeatMap())
            {
                var context = new IPTV2Entities();
                if (!String.IsNullOrEmpty(countryCode) && !String.IsNullOrEmpty(cityName)) // getAllDataByCity
                {
                    string cacheKey = "HMGAD:U:" + String.Format("{0}{1}", countryCode, cityName) + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                    list = (List<hmUserData>)cache[cacheKey];
                    if (list == null)
                    {
                        var users = context.Users.Where(u => u.CountryCode.ToUpper() == countryCode.ToUpper() && u.City.ToUpper().Contains(cityName.ToUpper()) && u.StatusId == GlobalConfig.Visible);
                        if (users != null)
                        {
                            list = new List<hmUserData>();
                            foreach (var user in users)
                            {
                                var item =
                                new hmUserData()
                                {
                                    FirstName = MyUtility.ToUpperFirstLetter(user.FirstName.ToLower()),
                                    CountryCode = user.CountryCode,
                                    City = MyUtility.ToUpperFirstLetter(user.City)
                                };
                                list.Add(item);

                            }
                            cache.Put(cacheKey, list, DataCache.CacheDuration);
                        }
                    }
                }
                else if (!String.IsNullOrEmpty(countryCode)) // getAllDataByCountry
                {
                    string cacheKey = "HMGAD:U:" + String.Format("{0}", countryCode) + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                    list = (List<hmUserData>)cache[cacheKey];
                    if (list == null)
                    {
                        var users = context.Users.Where(u => u.CountryCode.ToUpper() == countryCode.ToUpper() && u.StatusId == GlobalConfig.Visible);
                        if (users != null)
                        {
                            list = new List<hmUserData>();
                            foreach (var user in users)
                            {
                                var item =
                                new hmUserData()
                                {
                                    FirstName = MyUtility.ToUpperFirstLetter(user.FirstName.ToLower()),
                                    CountryCode = user.CountryCode,
                                    City = MyUtility.ToUpperFirstLetter(user.City)

                                };
                                list.Add(item);
                            }
                            cache.Put(cacheKey, list, DataCache.CacheDuration);
                        }
                    }
                }
                else //getAllData
                {
                    string cacheKey = "HMGAD:U:;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                    //list = (List<hmUserData>)cache[cacheKey];
                    if (list == null)
                    {
                        var users = context.Users.Where(u => u.StatusId == GlobalConfig.Visible).ToList();
                        if (users != null)
                        {
                            list = new List<hmUserData>();
                            foreach (var user in users)
                            {
                                var item =
                                new hmUserData()
                                {
                                    FirstName = MyUtility.ToUpperFirstLetter(user.FirstName.ToLower()),
                                    CountryCode = user.CountryCode,
                                    City = MyUtility.ToUpperFirstLetter(user.City)
                                };
                                list.Add(item);

                            }
                            // cache.Put(cacheKey, list, DataCache.CacheDuration);
                        }
                    }
                }
            }
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            return Content(serializer.Serialize(list), "application/json");
            //return this.Json(list, JsonRequestBehavior.AllowGet);
        }

        //[OutputCache(VaryByParam = "*", Duration = 300)]
        public JsonResult hmGetAllRecentlyAddedData(string timestamp, string countryCode, string cityName)
        {
            List<hmUserData> list = new List<hmUserData>();
            DateTime timeStamp = DateTime.Now;
            if (timestamp == null)
                return this.Json(list, JsonRequestBehavior.AllowGet);
            try
            {
                timeStamp = DateTime.Parse(timestamp);
            }
            catch (Exception)
            {
                return this.Json(list, JsonRequestBehavior.AllowGet);
            }

            var cache = DataCache.Cache;
            if (isWhitelistedForHeatMap())
            {
                var context = new IPTV2Entities();
                if (!String.IsNullOrEmpty(countryCode) && !String.IsNullOrEmpty(cityName)) // getAllDataByCity
                {
                    string cacheKey = "HMGARAD:U:" + String.Format("{0}{1}{2}", countryCode, cityName, timeStamp) + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                    list = (List<hmUserData>)cache[cacheKey];
                    if (list == null)
                    {
                        var users = context.Users.Where(u => u.CountryCode.ToUpper() == countryCode.ToUpper() && u.City.ToUpper().Contains(cityName.ToUpper()) && u.RegistrationDate >= timeStamp && u.StatusId == GlobalConfig.Visible);
                        if (users != null)
                        {
                            list = new List<hmUserData>();
                            foreach (var user in users)
                            {
                                var item =
                                new hmUserData()
                                {
                                    FirstName = MyUtility.ToUpperFirstLetter(user.FirstName.ToLower()),
                                    CountryCode = user.CountryCode,
                                    City = MyUtility.ToUpperFirstLetter(user.City)
                                };
                                list.Add(item);
                            }
                            cache.Put(cacheKey, list, DataCache.CacheDuration);
                        }
                    }
                }
                else if (!String.IsNullOrEmpty(countryCode)) // getAllDataByCountry
                {
                    string cacheKey = "HMGARAD:U:" + String.Format("{0}{1}", countryCode, timeStamp) + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                    list = (List<hmUserData>)cache[cacheKey];
                    if (list == null)
                    {
                        var users = context.Users.Where(u => u.CountryCode.ToUpper() == countryCode.ToUpper() && u.RegistrationDate >= timeStamp && u.StatusId == GlobalConfig.Visible);
                        if (users != null)
                        {
                            list = new List<hmUserData>();
                            foreach (var user in users)
                            {
                                var item =
                                new hmUserData()
                                {
                                    FirstName = MyUtility.ToUpperFirstLetter(user.FirstName.ToLower()),
                                    CountryCode = user.CountryCode,
                                    City = MyUtility.ToUpperFirstLetter(user.City)
                                };
                                list.Add(item);
                            }
                            cache.Put(cacheKey, list, DataCache.CacheDuration);
                        }
                    }
                }
                else //getAllData
                {
                    string cacheKey = "HMGARAD:U:" + String.Format("{0}{1}", countryCode, timeStamp) + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                    list = (List<hmUserData>)cache[cacheKey];
                    if (list == null)
                    {
                        var users = context.Users.Where(u => u.RegistrationDate >= timeStamp && u.StatusId == GlobalConfig.Visible);
                        if (users != null)
                        {
                            list = new List<hmUserData>();
                            foreach (var user in users)
                            {
                                var item =
                                new hmUserData()
                                {
                                    FirstName = MyUtility.ToUpperFirstLetter(user.FirstName.ToLower()),
                                    CountryCode = user.CountryCode,
                                    City = MyUtility.ToUpperFirstLetter(user.City)
                                };
                                list.Add(item);
                            }
                            cache.Put(cacheKey, list, DataCache.CacheDuration);
                        }
                    }
                }
            }
            return this.Json(list, JsonRequestBehavior.AllowGet);
        }

        private bool isWhitelistedForHeatMap()
        {
            string ip = ConfigurationManager.AppSettings["HeatMapIpWhiteList"];
            string[] IpAddresses = ip.Split(';');
            return IpAddresses.Contains(Request.GetUserHostAddressFromCloudflare());
        }

        private bool IsProfilePrivate(Guid userId)
        {
            UserData uData = new UserData() { IsExternalSharingEnabled = "true", IsInternalSharingEnabled = "true", IsProfilePrivate = "false" };
            try
            {
                var userData = GigyaMethods.GetUserData(userId, "IsExternalSharingEnabled,IsInternalSharingEnabled,IsProfilePrivate");
                if (userData.GetKeys().Count() >= 3)
                    if (userData != null)
                    {
                        uData = JsonConvert.DeserializeObject<UserData>(userData.ToJsonString());
                        if (uData.IsProfilePrivate.Contains("true"))
                            return true;
                        else
                            return false;
                    }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ActionResult IsUserLoggedIn()
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("IsAuthenticated", MyUtility.isUserLoggedIn());
            if (MyUtility.isUserLoggedIn())
            {
                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
                var userInfo = new TFCTV.Models.SynapseUserInfo()
                {
                    uid = user.UserId,
                    email = user.EMail,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    City = user.City,
                    State = user.State,
                    CountryCode = user.CountryCode
                };
                collection.Add("info", userInfo);
            }
            return Content(MyUtility.buildJson(collection), "application/json");

            //return this.Json(collection, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult PublishUserAction(FormCollection f) {

        //    UserAction action = new UserAction()
        //    {
        //        userMessage = f["UserMessage"],
        //        title = f["Title"],
        //        subtitle = f["Subtitle"],
        //        linkBack = f["LinkBack"],
        //        description = f["Description"],
        //        actionLinks = actionlinks,
        //        mediaItems = mediaItems,
        //        actorUID = User.Identity.Name
        //    };
        //}


        public ActionResult GetComments(string categoryID, string streamID, int? limit, int? depth, string sort, string start)
        {
            if (String.IsNullOrEmpty(sort))
                sort = "dateDesc"; //default
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("categoryID", categoryID);
            collection.Add("streamID", streamID);
            if (limit != null)
                collection.Add("threadLimit", limit);
            if (depth != null)
                collection.Add("threadDepth", depth);
            if (!String.IsNullOrEmpty(start) && limit != null)
                collection.Add("start", start);
            collection.Add("sort", sort);
            collection.Add("includeUID", true);
            GSResponse res = GigyaHelpers.createAndSendRequest("comments.getComments", GigyaHelpers.buildParameter(collection));
            return Content(res.GetData().ToJsonString(), "application/json");
        }


        //public ActionResult PostComment(string categoryID, string streamID, string commentText)
        //{

        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    if (String.IsNullOrEmpty(commentText)) {
        //        collection.Add("errorCode", -1);
        //        return Content(MyUtility.buildJson(collection), "application/json");
        //    }

        //    collection.Add("categoryID", categoryID);
        //    collection.Add("streamID", streamID);                     
        //    collection.Add("commentText", commentText);
        //    collection.Add("UID", User.Identity.Name);
        //    GSResponse res = GigyaHelpers.createAndSendRequest("comments.getComments", GigyaHelpers.buildParameter(collection));
        //    return Content(res.GetData().ToJsonString(), "application/json");
        //}

        private bool IsDefaultCountry()
        {
            try
            {
                string ip = Request.GetUserHostAddressFromCloudflare();
                var location = MyUtility.getLocation(ip);
                if (String.Compare(location.countryCode, GlobalConfig.DefaultCountry, true) == 0)
                    return true;
            }
            catch (Exception) { }
            return false;
        }

        public ActionResult GetLiveStream(int? id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection = MyUtility.setError(ErrorCodes.UnknownError, String.Empty);
            if (!MyUtility.isUserLoggedIn())
                return Content(MyUtility.buildJson(collection), "application/json");

            var context = new IPTV2Entities();
            if (id == null)
                return Content(MyUtility.buildJson(collection), "application/json");

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


        public ActionResult LogPlaybackOld(int type, int id, int playTypeId, int? fullDuration, bool isPreview, string positionDuration, int? streamType)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection = MyUtility.setError(ErrorCodes.UnknownError, String.Empty);
            DateTime registDt = DateTime.Now;
            int positionDuration_Integer = 0;
            try
            {
                positionDuration_Integer = Int32.Parse(positionDuration);
            }
            catch (Exception) { }

            if (fullDuration == null)
                fullDuration = -1000;

            try
            {
                if (MyUtility.isUserLoggedIn())
                {
                    var context = new IPTV2Entities();
                    var userId = new Guid(User.Identity.Name);
                    int categoryId = 0, assetId = 0;
                    var videoContext = new VideoEngagementsEntities();

                    int[] fillDuration = { 2, 3, 4 };

                    switch (type)
                    {
                        case 4:
                        case 1: //EpisodePlay
                            {
                                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                                if (episode != null)
                                {
                                    var episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => e.CategoryId != GlobalConfig.FreeTvCategoryId);
                                    categoryId = episodeCategory.CategoryId;
                                    var asset = episode.PremiumAssets.FirstOrDefault();
                                    if (asset != null)
                                        assetId = asset.AssetId;
                                }

                                EpisodePlay episodePlay = null;

                                if (episode.IsLiveChannelActive == true)
                                {
                                    episodePlay = new EpisodePlay()
                                    {
                                        EpisodeId = id,
                                        CategoryId = categoryId,
                                        UserId = userId,
                                        DateTime = registDt,
                                        AssetId = assetId,
                                        PlayTypeId = playTypeId,
                                        Duration = fillDuration.Contains(playTypeId) ? positionDuration_Integer : 0,
                                        IsPreview = false,
                                        StartPosition = -1000,
                                        Length = -1000,
                                        StreamType = 0
                                    };
                                }
                                else
                                {
                                    episodePlay = new EpisodePlay()
                                    {
                                        EpisodeId = id,
                                        CategoryId = categoryId,
                                        UserId = userId,
                                        DateTime = registDt,
                                        AssetId = assetId,
                                        PlayTypeId = playTypeId,
                                        Duration = fillDuration.Contains(playTypeId) ? positionDuration_Integer : 0,
                                        IsPreview = isPreview,
                                        StartPosition = fillDuration.Contains(playTypeId) ? 0 : positionDuration_Integer,
                                        Length = (int)fullDuration,
                                        StreamType = streamType == null ? 0 : streamType
                                    };
                                }

                                if (episodePlay != null)
                                    videoContext.EpisodePlays.Add(episodePlay);

                                break;
                            }
                        case 2: //ChannelPlay
                            {
                                var channelPlay = new ChannelPlay()
                                {
                                    ChannelId = id,
                                    //ChannelPlayId = 0,
                                    DateTime = registDt,
                                    Duration = fillDuration.Contains(playTypeId) ? positionDuration_Integer : 0,
                                    PlayTypeId = playTypeId,
                                    UserId = userId
                                };
                                videoContext.ChannelPlays.Add(channelPlay);
                                break;
                            }
                        case 3: //YoutubePlay
                            {
                                break;
                            }
                    }
                    videoContext.SaveChanges();
                    collection = MyUtility.setError(ErrorCodes.Success, String.Empty);
                }
                else
                    collection = MyUtility.setError(ErrorCodes.NotAuthenticated, String.Empty);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Content(MyUtility.buildJson(collection), "application/json");
        }



        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetMediaOld(int? id, int p = 0)
        // id = episodeid, p = 1 for progressive-low, 2 for progressive-high, 0 for adaptive
        // c = if 1 it is a channel/live event, else 0, which is normal episode
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;

            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            try
            {
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
                                collection = MyUtility.setError(ErrorCodes.MultipleLoginDetected, "Not allowed to use account.");
                                collection.Add("uid", User.Identity.Name);
                                FormsAuthentication.SignOut();
                                return Content(MyUtility.buildJson(collection), "application/json");
                            }
                        }
                    }
                }

                var context = new IPTV2Entities();

                //Usage of new prevention of multiple login via database
                if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
                {
                    if (MyUtility.isUserLoggedIn())
                    {
                        HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                        var UserId = new Guid(User.Identity.Name);
                        var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            if (!String.IsNullOrEmpty(user.SessionId))
                            {
                                if (String.Compare(user.SessionId, authCookie.Value, true) != 0)
                                {
                                    collection = MyUtility.setError(ErrorCodes.MultipleLoginDetected, "Not allowed to use account.");
                                    collection.Add("uid", User.Identity.Name);
                                    FormsAuthentication.SignOut();
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }
                            }
                        }
                    }
                }

                Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.OnlineStatusId == GlobalConfig.Visible);

                if (ep != null)
                {
                    DateTime registDt = DateTime.Now;
                    if (ep.OnlineStartDate > registDt)
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "Media not found.");
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }

                    if (ep.OnlineEndDate < registDt)
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "Media not found.");
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }

                    //var pAsset = ep.PremiumAssets.FirstOrDefault();
                    var pAsset = ep.PremiumAssets.LastOrDefault();
                    if (pAsset == null)
                    {
                        errorCode = ErrorCodes.AkamaiCdnNotFound;
                        collection = MyUtility.setError(errorCode, "MediaUrl not found.");
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }
                    Asset asset = pAsset.Asset;
                    if (asset != null)
                    {
                        int assetId = asset == null ? 0 : asset.AssetId;

                        AkamaiFlowPlayerPluginClipDetails clipDetails = null;

                        if (ep.IsLiveChannelActive == true) //isLiveEvent
                        {
                            if (!GlobalConfig.IsIosHLSCdnEnabled)
                            {
                                if (Request.Browser.IsMobileDevice)
                                {
                                    errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                    collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }
                            }
                            else
                            {
                                //var excludedEpisodeIds = MyUtility.StringToIntList(GlobalConfig.WhitelistedLiveStreamEpisodeIdFromMobileCheck);
                                //if (!excludedEpisodeIds.Contains((int)id))
                                //{
                                //    if (Request.Browser.IsMobileDevice)
                                //    {
                                //        errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                //        collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                //        return Content(MyUtility.buildJson(collection), "application/json");
                                //    }
                                //}
                                //if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOnAsset(ep))
                                if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOPremiumnAsset(pAsset))
                                {
                                    if (Request.Browser.IsMobileDevice)
                                    {
                                        errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                        collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                        return Content(MyUtility.buildJson(collection), "application/json");
                                    }
                                }
                            }

                            if (!Request.IsAjaxRequest())
                                if (!User.Identity.IsAuthenticated)
                                {
                                    errorCode = ErrorCodes.NotAuthenticated;
                                    collection = MyUtility.setError(errorCode, "User is not logged in");
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }

                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails(ep.EpisodeId, assetId, Request, User, true);
                            if (GlobalConfig.IsLiveEventEntitlementCheckEnabled)
                            {
                                Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                if (!ContextHelper.CanPlayVideo(context, offering, ep, asset, User, Request))
                                {
                                    clipDetails.Url = String.Empty;
                                    errorCode = ErrorCodes.UserIsNotEntitled;
                                    collection = MyUtility.setError(errorCode, "Live event can't be played.");
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }
                            }
                            else
                            {
                                //Get a list of episodes that are tagged as free
                                if (!String.IsNullOrEmpty(GlobalConfig.FreeLiveEventEpisodeIds))
                                {
                                    var freeLiveEventIds = MyUtility.StringToIntList(GlobalConfig.FreeLiveEventEpisodeIds);
                                    if (!(freeLiveEventIds.Contains(ep.EpisodeId))) //means it is not part of the free episodes
                                    {
                                        Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                        if (!ContextHelper.CanPlayVideo(context, offering, ep, asset, User, Request))
                                        {
                                            clipDetails.Url = String.Empty;
                                            errorCode = ErrorCodes.UserIsNotEntitled;
                                            collection = MyUtility.setError(errorCode, "Live event can't be played.");
                                            return Content(MyUtility.buildJson(collection), "application/json");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (p > 0)
                            {
                                var progressive = p == 1 ? Progressive.Low : Progressive.High;
                                if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(ep.EpisodeId, assetId, Request, User, progressive);
                                else
                                {
                                    if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                                    {
                                        if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                                        {
                                            var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                                            if (listOfEpisodeIds.Contains(ep.EpisodeId))
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(ep.EpisodeId, assetId, Request, User, progressive);
                                            else
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(ep.EpisodeId, assetId, Request, User, progressive);
                                        }
                                        else
                                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(ep.EpisodeId, assetId, Request, User, progressive);
                                    }
                                    else
                                        clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(ep.EpisodeId, assetId, Request, User, progressive);
                                }


                                //if (!MyUtility.isUserLoggedIn())
                                Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                if (!ContextHelper.CanPlayVideo(context, offering, ep, asset, User, Request))
                                {
                                    clipDetails.Url = String.Empty;
                                    errorCode = ErrorCodes.UserIsNotEntitled;
                                    collection = MyUtility.setError(errorCode, "Progressive media can't be played.");
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }

                            }
                            else
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetails(ep.EpisodeId, assetId, Request, User);
                        }

                        if (!String.IsNullOrEmpty(clipDetails.Url))
                        {
                            errorCode = ErrorCodes.Success;
                            collection = MyUtility.setError(errorCode, clipDetails.Url);
                            collection.Add("data", clipDetails);
                        }
                        else
                        {
                            errorCode = ErrorCodes.AkamaiCdnNotFound;
                            collection = MyUtility.setError(errorCode, "MediaUrl not found.");
                            if (Akamai.IsIos(Request))
                            {
                                errorCode = ErrorCodes.IPodPreviewNotAvailable;
                                collection = MyUtility.setError(errorCode, "Media preview not available.");
                            }
                        }
                    }
                    else
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "Media not found.");
                    }
                }
                else
                {
                    errorCode = ErrorCodes.EpisodeNotFound;
                    collection = MyUtility.setError(errorCode, "MediaId not found.");
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Content(MyUtility.buildJson(collection), "application/json");
        }


        public ActionResult ShareMedia(string message, string title, string description, string img, string href)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            if (!MyUtility.isUserLoggedIn())
                return Content(MyUtility.buildJson(collection), "application/json");

            try
            {
                var context = new IPTV2Entities();
                System.Guid userId = new Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                    return Content(MyUtility.buildJson(collection), "application/json");

                List<ActionLink> actionlinks = new List<ActionLink>();
                actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text_generic, href = href });
                List<MediaItem> mediaItems = new List<MediaItem>();
                mediaItems.Add(new MediaItem() { type = SNSTemplates.watch_mediaitem_type, src = img, href = href });
                UserAction action = new UserAction()
                {
                    userMessage = message,
                    title = title,
                    subtitle = SNSTemplates.subtitle_generic,
                    linkBack = href,
                    description = description,
                    actionLinks = actionlinks,
                    mediaItems = mediaItems,
                    actorUID = User.Identity.Name
                };

                var userData = MyUtility.GetUserPrivacySetting(user.UserId);
                if (userData.IsExternalSharingEnabled.Contains("true"))
                    GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "external");
                action.description = String.Empty;
                if (userData.IsInternalSharingEnabled.Contains("true"))
                    return Content(GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "internal"), "application/json");
                collection = MyUtility.setError(ErrorCodes.Success);
            }
            catch (Exception) { }
            return this.Json(collection, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLatestEpisodesForElectionCoverage()
        {
            List<JsonFeatureItem> jfi = null;
            DateTime registDt = DateTime.Now;

            var cache = DataCache.Cache;
            string cacheKey = "JRGL:O:ECOVERAGE;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            jfi = (List<JsonFeatureItem>)cache[cacheKey];
            try
            {
                if (jfi == null)
                {
                    var context = new IPTV2Entities();
                    var feature = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.SpecialElectionCoverageCategoryId && c.StatusId == GlobalConfig.Visible);

                    if (feature == null)
                        return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                    if (!(feature is Show))
                        return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                    if (feature.StartDate > registDt)
                        return this.Json(string.Empty, JsonRequestBehavior.AllowGet);
                    if (feature.EndDate < registDt)
                        return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                    //List<FeatureItem> featureItems = context.FeatureItems.Where(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                    var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == feature.CategoryId).Select(e => e.EpisodeId);
                    var featureItems = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible).OrderByDescending(e => e.EpisodeId).ThenByDescending(e => e.AuditTrail.UpdatedBy);
                    jfi = new List<JsonFeatureItem>();
                    Show show = (Show)feature;
                    foreach (var f in featureItems)
                    {
                        if (f is Episode)
                        {
                            if (f.OnlineStartDate < registDt && f.OnlineEndDate > registDt)
                            {
                                string img = String.IsNullOrEmpty(f.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, f.EpisodeId.ToString(), f.ImageAssets.ImageVideo);
                                string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                JsonFeatureItem j = new JsonFeatureItem()
                                {
                                    EpisodeId = f.EpisodeId,
                                    EpisodeDescription = MyUtility.Ellipsis(f.Description, 20),
                                    EpisodeName = f.EpisodeName,
                                    EpisodeAirDate = (f.DateAired != null) ? f.DateAired.Value.ToString("MMM d, yyyy") : "",
                                    ShowId = show.CategoryId,
                                    ShowName = String.Compare(f.EpisodeName, f.EpisodeCode) == 0 ? MyUtility.Ellipsis(show.CategoryName, 20) : MyUtility.Ellipsis(f.EpisodeName, 20),
                                    EpisodeImageUrl = img,
                                    ShowImageUrl = showImg,
                                    Blurb = MyUtility.Ellipsis(f.Synopsis, 80)
                                };
                                if (jfi.Count() < GlobalConfig.SpecialElectionCoverageEpisodeDisplayCount)
                                    jfi.Add(j);
                                else
                                    break;
                            }
                        }
                    }
                    cache.Put(cacheKey, jfi, DataCache.CacheDuration);
                }
                return this.Json(jfi, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) { MyUtility.LogException(e); return this.Json(string.Empty, JsonRequestBehavior.AllowGet); }
        }
        /// <summary>
        /// Description: Get featured Teams
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 300)]
        public JsonResult GetFeaturedTeams()
        {
            List<JsonFeatureItem> jfi = null;

            var cache = DataCache.Cache;
            string cacheKey = "UAAPTEAMSMENUCACHEKEY:0;";
            jfi = (List<JsonFeatureItem>)cache[cacheKey];

            if (jfi == null)
            {
                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == 82 && f.StatusId == GlobalConfig.Visible);
                if (feature == null)
                    return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                List<FeatureItem> featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                jfi = new List<JsonFeatureItem>();
                foreach (CelebrityFeatureItem f in featureItems)
                {
                    if (f is CelebrityFeatureItem)
                    {
                        Celebrity person = f.Celebrity;
                        string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                        JsonFeatureItem j = new JsonFeatureItem()
                        {
                            CelebrityFullName = person.FullName,
                            ShowId = person.CelebrityId,
                            ShowImageUrl = img,
                            EpisodeName = person.Height
                        };
                        jfi.Add(j);
                    }
                }
                var cacheDuration = new TimeSpan(0, GlobalConfig.MenuCacheDuration, 0);
                cache.Put(cacheKey, jfi, DataCache.CacheDuration);
            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLatestUAAPGameEpisodes()
        {
            List<JsonFeatureItem> jfi = null;
            DateTime registDt = DateTime.Now;

            var cache = DataCache.Cache;
            string cacheKey = "NadsUaapLatestGameCacKey";
            jfi = (List<JsonFeatureItem>)cache[cacheKey];
            try
            {
                if (jfi == null)
                {
                    var context = new IPTV2Entities();
                    var feature = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.SpecialElectionCoverageCategoryId && c.StatusId == GlobalConfig.Visible);

                    if (feature == null)
                        return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                    if (!(feature is Show))
                        return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                    if (feature.StartDate > registDt)
                        return this.Json(string.Empty, JsonRequestBehavior.AllowGet);
                    if (feature.EndDate < registDt)
                        return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                    //List<FeatureItem> featureItems = context.FeatureItems.Where(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                    var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == feature.CategoryId).Select(e => e.EpisodeId);
                    var featureItems = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible).OrderByDescending(e => e.EpisodeId).ThenByDescending(e => e.AuditTrail.UpdatedBy);
                    jfi = new List<JsonFeatureItem>();
                    Show show = (Show)feature;
                    foreach (var f in featureItems)
                    {
                        if (f is Episode)
                        {
                            if (f.OnlineStartDate < registDt && f.OnlineEndDate > registDt)
                            {
                                string img = String.IsNullOrEmpty(f.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, f.EpisodeId.ToString(), f.ImageAssets.ImageVideo);
                                string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                JsonFeatureItem j = new JsonFeatureItem()
                                {
                                    EpisodeId = f.EpisodeId,
                                    EpisodeDescription = MyUtility.Ellipsis(f.Description, 20),
                                    EpisodeName = f.EpisodeName,
                                    EpisodeAirDate = (f.DateAired != null) ? f.DateAired.Value.ToString("MMM d, yyyy") : "",
                                    ShowId = show.CategoryId,
                                    ShowName = String.Compare(f.EpisodeName, f.EpisodeCode) == 0 ? MyUtility.Ellipsis(show.CategoryName, 20) : MyUtility.Ellipsis(f.EpisodeName, 20),
                                    EpisodeImageUrl = img,
                                    ShowImageUrl = showImg,
                                    Blurb = MyUtility.Ellipsis(f.Synopsis, 80)
                                };
                                if (jfi.Count() < GlobalConfig.SpecialElectionCoverageEpisodeDisplayCount)
                                    jfi.Add(j);
                                else
                                    break;
                            }
                        }
                    }
                    cache.Put(cacheKey, jfi, DataCache.CacheDuration);
                }
                return this.Json(jfi, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) { MyUtility.LogException(e); return this.Json(string.Empty, JsonRequestBehavior.AllowGet); }
        }

        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 300)]
        public JsonResult GetEventCelebritiesFeature(int id)
        {
            var socialContext = new EngagementsEntities();
            List<JsonFeatureItem> jfi = null;

            var cache = DataCache.Cache;
            string cacheKey = "EVENTFEATUREDCELEBCACHEKEY:0;";
            jfi = (List<JsonFeatureItem>)cache[cacheKey];
            if (jfi == null)
            {
                var context = new IPTV2Entities();
                var feature = context.Features.FirstOrDefault(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
                if (feature == null)
                    return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

                List<FeatureItem> featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn).ToList();
                jfi = new List<JsonFeatureItem>();
                foreach (var f in featureItems)
                {
                    if (f is CelebrityFeatureItem)
                    {
                        var cft = (CelebrityFeatureItem)f;
                        Celebrity person = cft.Celebrity;
                        string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                        var lovesCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == person.CelebrityId && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                        int love = (int)(lovesCountSummary == null ? 0 : lovesCountSummary.Total);
                        JsonFeatureItem j = new JsonFeatureItem()
                        {
                            CelebrityFullName = person.FullName,
                            ShowId = person.CelebrityId,
                            ShowImageUrl = img,
                            EpisodeName = person.ZodiacSign,
                            EpisodeDescription = love.ToString(),
                            ShowDescription = person.ChineseYear,
                            parentId = love
                        };
                        jfi.Add(j);
                    }
                }
                var CacheDuration = new TimeSpan(0, 30, 0);
                jfi = jfi.OrderByDescending(j => j.parentId).ToList();
                cache.Put(cacheKey, jfi, CacheDuration);
            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }
        /************* ANNIVERSARY PROMO *********************/
        /// <summary>
        /// This function returns the status of the user in the challenge.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetChallengeStatus()
        {
            GigyaResponseData response = new GigyaResponseData() { errorCode = -1, errorMessage = "Unidentified error" };
            if (!Request.IsLocal)
                if (!Request.IsAjaxRequest())
                {
                    response.errorMessage = "Invalid request";
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }

            if (!User.Identity.IsAuthenticated)
                response.errorMessage = "Unauthorized user";
            else
            {
                try
                {
                    response = GigyaMethods.GetChallengeStatus(new Guid(User.Identity.Name));
                }
                catch (Exception) { response.errorMessage = "Invalid action"; }
            }
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This function gets all the actions log of a user
        /// </summary>
        /// <returns></returns>
        public JsonResult GetActionsLog(string id)
        {
            GigyaResponseData response = new GigyaResponseData() { errorCode = -1, errorMessage = "Unidentified error" };
            if (!Request.IsLocal && !GlobalConfig.isUAT)
                if (!Request.IsAjaxRequest())
                {
                    response.errorMessage = "Invalid request";
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }

            if (!User.Identity.IsAuthenticated)
                response.errorMessage = "Unauthorized user";
            else
            {
                try
                {
                    response = GigyaMethods.GetActionsLog(new Guid(String.IsNullOrEmpty(id) ? User.Identity.Name : id));
                    response.actions = response.actions.Where(x => String.Compare(x.challengeID, GlobalConfig.GigyaPromoChallengeID, true) == 0);
                }
                catch (Exception) { response.errorMessage = "Invalid action"; }
            }
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This function gets the top users, with the PromoChallengeID hardcoded in the Globalconfig. This can be used for future challenges, provided that the challenge id will be part of the parameters
        /// </summary>
        /// <returns></returns>
        public JsonResult GetTopUsers()
        {
            GigyaResponseData response = new GigyaResponseData() { errorCode = -1, errorMessage = "Unidentified error" };

            if (!Request.IsLocal)
                if (!Request.IsAjaxRequest())
                    response.errorMessage = "Invalid request";
            try
            {
                if (User.Identity.IsAuthenticated)
                    response = GigyaMethods.GetTopUsers(new Guid(User.Identity.Name), GlobalConfig.GigyaPromoChallengeID, 10, true);
                else
                    response = GigyaMethods.GetTopUsers(null, GlobalConfig.GigyaPromoChallengeID, 10, true);
            }
            catch (Exception) { response.errorMessage = "Invalid action"; }
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// This function gets all the actions log of a user
        /// </summary>
        /// <returns></returns>
        public JsonResult GetVariants(int id)
        {
            GigyaResponseData sourceResponse = new GigyaResponseData() { errorCode = -1, errorMessage = "Unidentified error" };
            GigyaResponseData response = new GigyaResponseData() { errorCode = -1, errorMessage = "Unidentified error" };
            if (!Request.IsLocal)
                if (!Request.IsAjaxRequest())
                {
                    response.errorMessage = "Invalid request";
                    return this.Json(null, JsonRequestBehavior.AllowGet);
                }
            if (!User.Identity.IsAuthenticated)
            {
                response.errorMessage = "Unauthorized user";
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            if (!Enum.IsDefined(typeof(AnniversaryPromo), id))
            {
                response.errorMessage = "Value not defined in enumerator.";
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            try
            {
                sourceResponse = GigyaMethods.GetVariantIDsbyUser(new Guid(User.Identity.Name), (AnniversaryPromo)id);
                response = GigyaMethods.GetVariantsbyVariantIDs(sourceResponse.actions.Select(s => s.VariantId));
                if (response != null)
                {
                    var finalResponse = from s in sourceResponse.actions
                                        join rs in response.variants on s.VariantId equals rs.variantID
                                        select new { actions = new GigyaCombinedActionsLog() { time = Convert.ToDateTime(s.time.Replace("Z", "")), timeStr = s.time.Replace("Z", ""), points = s.points, description = rs.actionAttributes.description.FirstOrDefault() } };
                    if (finalResponse != null)
                        if (finalResponse.Count() > 0)
                        {
                            var fr = finalResponse.OrderByDescending(f => f.actions.time);
                            return this.Json(fr, JsonRequestBehavior.AllowGet);
                        }
                }
            }
            catch (Exception e) { response.errorMessage = e.Message + "Invalid action"; }
            return this.Json(null, JsonRequestBehavior.AllowGet);
            //return this.Json(response == null ? null : response.variants.Select(r => r.actionAttributes.description), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        /// <summary>
        /// This function will add a point depending on the action done.
        /// </summary>
        /// <param name="id">Please base the ids on the Enum Anniversary Promo</param>
        /// <param name="contentId">Id of the episode, show, celebrity</param>
        /// <param name="description">Can be the show name, celebrity name, episode name (show - date)</param>
        /// <returns></returns>
        public JsonResult NotifyAction(FormCollection fc)
        {
            int naId = 0;
            GigyaResponseData response = new GigyaResponseData() { errorCode = -1, errorMessage = "Unidentified error" };
            if (String.IsNullOrEmpty(fc["naId"]) || String.IsNullOrEmpty(fc["naDescription"]))
            {
                response.errorMessage = "Missing parameter";
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            naId = Convert.ToInt32(fc["naId"]);
            string naDescription = fc["naDescription"];

            if (!Request.IsLocal)
                if (!Request.IsAjaxRequest())
                {
                    response.errorMessage = "Invalid request";
                    return this.Json(null, JsonRequestBehavior.AllowGet);
                }

            if (!User.Identity.IsAuthenticated)
                response.errorMessage = "Unauthorized user";
            else
            {
                if (!ContextHelper.IsUserPartOfPromo(GlobalConfig.TFCtvFirstYearAnniversaryPromoId, User.Identity.IsAuthenticated ? new Guid(User.Identity.Name) : Guid.Empty))
                    response.errorMessage = "User not in Promo";
                else
                {
                    try
                    {
                        string action = String.Empty;
                        action = ((AnniversaryPromo)naId).ToString();
                        if (!String.IsNullOrEmpty(action))
                        {
                            GigyaActionSingleAttribute actionAttribute = new GigyaActionSingleAttribute();
                            {
                                actionAttribute.description = new List<string> { naDescription };
                            }
                            response = GigyaMethods.NotifyAction(new Guid(User.Identity.Name), action, actionAttribute);
                        }
                    }
                    catch (Exception) { response.errorMessage = "Invalid action"; }
                }
            }
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// This function gets the users points on a particular action
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPointsByAction(int id)
        {
            GigyaResponseData response = new GigyaResponseData() { errorCode = -1, errorMessage = "Unidentified error", statusReason = "0 entries" };
            int points = 0;
            if (!Request.IsLocal)
                if (!Request.IsAjaxRequest())
                {
                    response.errorMessage = "Invalid request";
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }

            if (!Enum.IsDefined(typeof(AnniversaryPromo), id))
            {
                response.errorMessage = "Value not defined in enumerator.";
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }

            if (!User.Identity.IsAuthenticated)
                response.errorMessage = "Unauthorized user";
            else
            {
                try
                {
                    points = GigyaMethods.GetPointsLogByAction(new Guid(User.Identity.Name), (AnniversaryPromo)id);
                    response.errorCode = 0;
                    response.statusReason = points > 1 || points == 0 ? String.Format("{0} entries", points) : String.Format("{0} entry", points);
                }
                catch (Exception) { response.errorMessage = "Invalid action"; }
            }
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// This function gets the top users, with the PromoChallengeID hardcoded in the Globalconfig. This can be used for future challenges, provided that the challenge id will be part of the parameters
        /// </summary>
        /// <returns></returns>
        public String GetUserData()
        {
            if (User.Identity.IsAuthenticated)
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("UID", User.Identity.Name);
                collection.Add("include", "data");
                GSResponse res = GigyaHelpers.createAndSendRequest("accounts.getAccountInfo", GigyaHelpers.buildParameter(collection));
                return res.GetData().ToJsonString();
            }
            return String.Empty;
        }

        [HttpPost]
        public JsonResult ConnectSNS(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };

            if (GlobalConfig.IsForceConnectToSNSEnabled)
            {
                if (String.IsNullOrEmpty(fc["UID"]) || String.IsNullOrEmpty(fc["provider"]))
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsMissingRequiredFields;
                    ReturnCode.StatusMessage = "Required fields missing.";
                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (!User.Identity.IsAuthenticated)
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.NotAuthenticated;
                    ReturnCode.StatusMessage = "User is not authenticated.";
                }
                else
                {
                    try
                    {
                        var context = new IPTV2Entities();
                        var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
                        if (user != null)
                        {
                            Dictionary<string, object> collection = new Dictionary<string, object>();

                            string UID = fc["UID"];
                            string provider = fc["provider"];
                            collection.Add("siteUID", user.UserId.ToString());
                            collection.Add("uid", Uri.UnescapeDataString(UID));
                            collection.Add("cid", String.Format("{0} - New User", provider));
                            GSResponse notifyRegistration = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(collection));
                            if (notifyRegistration.GetErrorCode() == 0)
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                ReturnCode.StatusMessage = "Successfully linked social network account.";
                            }
                            else
                                ReturnCode.StatusMessage = notifyRegistration.GetErrorMessage();
                        }
                    }
                    catch (Exception e) { ReturnCode.StatusMessage = e.Message; MyUtility.LogException(e); }
                }
            }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LogPlayback(int type, int id, int playTypeId, int? fullDuration, bool isPreview, string positionDuration, string streamType, int? bufferCount, int? minBandwidth, int? maxBandwidth, int? avgBandwidth)
        {
            //Response.ContentType = "application/json";
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };

            if (!Request.IsLocal)
                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Your request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

            if (!User.Identity.IsAuthenticated)
            {
                ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                ReturnCode.StatusMessage = "Your request is invalid.";
                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
            }
            try
            {
                DateTime registDt = DateTime.Now;
                int positionDuration_Integer = 0; int sType = 0;
                try { positionDuration_Integer = Int32.Parse(positionDuration); }
                catch (Exception) { }
                try { sType = Int32.Parse(streamType); }
                catch (Exception) { }
                if (fullDuration == null)
                    fullDuration = -1000;

                var context = new IPTV2Entities();
                var userId = new Guid(User.Identity.Name);
                int categoryId = 0, assetId = 0;


                int[] fillDuration = { 2, 3, 4 };

                var returnCode = new SqlParameter()
                {
                    ParameterName = "returnCode",
                    DbType = System.Data.DbType.Int32,
                    Direction = System.Data.ParameterDirection.Output
                };

                string tableName = String.Format("EpisodePlay{0}", registDt.ToString("yyyyMM"));
                using (var videoContext = new VideoEngagementsEntities())
                {
                    switch (type)
                    {
                        case 4:
                        case 1: //EpisodePlay
                            {
                                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                                if (episode != null)
                                {
                                    var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                    var episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                                    categoryId = episodeCategory.CategoryId;
                                    var asset = episode.PremiumAssets.FirstOrDefault();
                                    if (asset != null)
                                        assetId = asset.AssetId;
                                }

                                if (episode.IsLiveChannelActive == true)
                                {
                                    var sTypeParam = new SqlParameter()
                                    {
                                        ParameterName = "StreamType",
                                        DbType = System.Data.DbType.Int32,
                                        Value = 0
                                    };
                                    var result = videoContext.Database.ExecuteSqlCommand("EXEC LogMediaPlayback @table, @PlayTypeId, @EpisodeId, @AssetId, @UserId, @registDt, @duration, @length, @CategoryId, @StartPosition, @isPreview, @StreamType, @returnCode OUTPUT",
                                    new object[] {  
                                 new SqlParameter("table", tableName),
                                 new SqlParameter("PlayTypeId", playTypeId),
                                 new SqlParameter("EpisodeId", id),
                                 new SqlParameter("AssetId", assetId),
                                 new SqlParameter("UserId", userId),
                                 new SqlParameter("registDt", registDt),
                                 new SqlParameter("duration", fillDuration.Contains(playTypeId) ? positionDuration_Integer : 0),
                                 new SqlParameter("length", -1000),
                                 new SqlParameter("CategoryId", categoryId),
                                 new SqlParameter("StartPosition", -1000),
                                 new SqlParameter("isPreview", false),
                                 sTypeParam,
                                 returnCode
                                });
                                }
                                else
                                {
                                    var result = videoContext.Database.ExecuteSqlCommand("EXEC LogMediaPlayback @table, @PlayTypeId, @EpisodeId, @AssetId, @UserId, @registDt, @duration, @length, @CategoryId, @StartPosition, @isPreview, @StreamType, @returnCode OUTPUT",
                                    new object[] {  
                                 new SqlParameter("table", tableName),
                                 new SqlParameter("PlayTypeId", playTypeId),
                                 new SqlParameter("EpisodeId", id),
                                 new SqlParameter("AssetId", assetId),
                                 new SqlParameter("UserId", userId),
                                 new SqlParameter("registDt", registDt),
                                 new SqlParameter("duration", fillDuration.Contains(playTypeId) ? positionDuration_Integer : 0),
                                 new SqlParameter("length",(int)fullDuration),
                                 new SqlParameter("CategoryId", categoryId),
                                 new SqlParameter("StartPosition", fillDuration.Contains(playTypeId) ? 0 : positionDuration_Integer),
                                 new SqlParameter("isPreview", isPreview),
                                 new SqlParameter("StreamType", sType),
                                 returnCode
                                });
                                }


                                //Log onto TFCtvWebApi (Json)
                                try
                                {
                                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(GlobalConfig.TFCtvApiVideoPlaybackUri);
                                    req.Method = "POST";
                                    req.ContentType = "application/json";
                                    using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                                    {
                                        string jsonString = String.Empty;
                                        var obj = new TfcTvApiPlaybackObj()
                                        {
                                            PlayTypeId = playTypeId,
                                            UserId = userId.ToString(),
                                            DateTime = registDt.ToString("o"),
                                            EpisodeId = id,
                                            CategoryId = categoryId,
                                            Duration = fillDuration.Contains(playTypeId) ? positionDuration_Integer : 0,
                                            Length = episode.IsLiveChannelActive == true ? -1000 : (int)fullDuration,
                                            AssetId = assetId,
                                            StartPosition = episode.IsLiveChannelActive == true ? -1000 : fillDuration.Contains(playTypeId) ? 0 : positionDuration_Integer,
                                            LastPosition = fillDuration.Contains(playTypeId) ? positionDuration_Integer : 0,
                                            ClientIp = MyUtility.GetClientIpAddress(),
                                            IsPreview = isPreview,
                                            DeviceHeader = Request.UserAgent,
                                            BufferCount = bufferCount != null ? (int)bufferCount : -1,
                                            MaxBitrate = maxBandwidth != null ? (int)maxBandwidth : -1,
                                            MinBitrate = minBandwidth != null ? (int)minBandwidth : -1,
                                            AvgBitrate = avgBandwidth != null ? (int)avgBandwidth : -1

                                        };
                                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                                        streamWriter.Write(jsonString);
                                    }

                                    WebResponse response = req.GetResponse();
                                    var statusDescription = ((HttpWebResponse)response).StatusDescription;
                                    Stream dataStream = response.GetResponseStream();
                                    StreamReader reader = new StreamReader(dataStream);
                                    string responseFromServer = reader.ReadToEnd();
                                    reader.Close();
                                    dataStream.Close();
                                    response.Close();
                                }
                                catch (Exception) { }

                                ////Log onto TFCtvWebApi
                                //try
                                //{
                                //    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(GlobalConfig.TFCtvApiVideoPlaybackUri);
                                //    req.Method = "POST";
                                //    req.ContentType = "application/x-www-form-urlencoded";
                                //    StringBuilder postData = new StringBuilder();
                                //    postData.Append(String.Format("PlayTypeId={0}&", playTypeId));
                                //    postData.Append(String.Format("UserId={0}&", userId));
                                //    postData.Append(String.Format("DateTime={0}&", registDt.ToString("o")));
                                //    postData.Append(String.Format("EpisodeId={0}&", id));
                                //    postData.Append(String.Format("CategoryId={0}&", categoryId));
                                //    postData.Append(String.Format("Duration={0}&", fillDuration.Contains(playTypeId) ? positionDuration_Integer : 0));
                                //    postData.Append(String.Format("Length={0}&", episode.IsLiveChannelActive == true ? -1000 : (int)fullDuration));
                                //    postData.Append(String.Format("AssetId={0}&", assetId));
                                //    postData.Append(String.Format("StartPosition={0}&", episode.IsLiveChannelActive == true ? -1000 : fillDuration.Contains(playTypeId) ? 0 : positionDuration_Integer));
                                //    postData.Append(String.Format("ClientIp={0}&", MyUtility.GetClientIpAddress()));
                                //    postData.Append(String.Format("IsPreview={0}&", isPreview));
                                //    postData.Append(String.Format("DeviceHeader={0}&", Request.UserAgent));

                                //    byte[] param = Encoding.UTF8.GetBytes(postData.ToString());
                                //    req.ContentLength = param.Length;

                                //    Stream dataStream = req.GetRequestStream();
                                //    dataStream.Write(param, 0, param.Length);
                                //    dataStream.Close();
                                //    WebResponse response = req.GetResponse();
                                //    var statusDescription = ((HttpWebResponse)response).StatusDescription;
                                //    dataStream = response.GetResponseStream();
                                //    StreamReader reader = new StreamReader(dataStream);
                                //    string responseFromServer = reader.ReadToEnd();
                                //    reader.Close();
                                //    dataStream.Close();
                                //    response.Close();
                                //}
                                //catch (Exception) { }
                                break;
                            }
                        case 2: //ChannelPlay
                            {
                                break;
                            }
                        case 3: //YoutubePlay
                            {
                                break;
                            }
                    }
                }
                if ((int)returnCode.Value > 0)
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                    ReturnCode.StatusMessage = "Playback logged.";
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusMessage = e.Message;
            }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetMediaV2(int? id, int p = 0, int q = 0)
        // id = episodeid, p = 1 for progressive-low, 2 for progressive-high, 0 for adaptive
        // c = if 1 it is a channel/live event, else 0, which is normal episode
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            if (!Request.IsLocal)
                if (!Request.IsAjaxRequest())
                {
                    errorCode = ErrorCodes.IsInvalidRequest;
                    collection = MyUtility.setError(errorCode, "Request is not valid.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

            try
            {
                var context = new IPTV2Entities();
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                User user = null;
                if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                        var UserId = new Guid(User.Identity.Name);
                        user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            CountryCode = user.CountryCode;
                            if (!String.IsNullOrEmpty(user.SessionId))
                            {
                                if (String.Compare(user.SessionId, authCookie.Value, true) != 0)
                                {
                                    collection = MyUtility.setError(ErrorCodes.MultipleLoginDetected, "Not allowed to use account.");
                                    collection.Add("uid", User.Identity.Name);
                                    FormsAuthentication.SignOut();
                                    return Content(MyUtility.buildJson(collection), "application/json");
                                }
                            }
                        }
                    }
                }

                Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                DateTime registDt = DateTime.Now;
                if (episode != null)
                {
                    if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                    {
                        Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                        AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                        var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true, offering);
                        var episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                        var HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };

                        if (HasActiveSubscriptionBasedOnCategoryId.HasSubscription) // Get premium asset
                        {
                            collection = ProcessPremiumAsset(episode, p, q);
                        }
                        else //get preview asset
                        {
                            var previewAsset = episode.PreviewAssets.LastOrDefault();
                            if (previewAsset != null)
                            {
                                Asset asset = previewAsset.Asset;
                                if (asset != null)
                                {
                                    int assetId = asset == null ? 0 : asset.AssetId;
                                    if (episode.IsLiveChannelActive == true) //isLiveEvent
                                    {
                                        if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(previewAsset))
                                        {
                                            if (Request.Browser.IsMobileDevice)
                                            {
                                                errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                                collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                                return Content(MyUtility.buildJson(collection), "application/json");
                                            }
                                        }
                                        clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails(episode.EpisodeId, assetId, Request, User, true);
                                    }
                                    else
                                    {
                                        if (p > 0)
                                        {
                                            var progressive = p == 1 ? Progressive.Low : Progressive.High;
                                            if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive);
                                            else
                                            {
                                                if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                                                {
                                                    if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                                                    {
                                                        var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                                                        if (listOfEpisodeIds.Contains(episode.EpisodeId))
                                                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive);
                                                        else
                                                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive);
                                                    }
                                                    else
                                                        clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive);
                                                }
                                                else
                                                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive);
                                            }
                                        }
                                        else
                                            clipDetails = Helpers.Akamai.GetAkamaiClipDetails(episode.EpisodeId, assetId, Request, User, null);
                                    }
                                }
                            }
                            else // no preview asset, use 60 seconds
                            {

                                collection = ProcessPremiumAsset(episode, p, q);
                            }
                        }
                        if (!String.IsNullOrEmpty(clipDetails.Url))
                        {
                            errorCode = ErrorCodes.Success;
                            collection = MyUtility.setError(errorCode, clipDetails.Url);
                            collection.Add("data", clipDetails);
                        }
                        else
                        {
                            errorCode = ErrorCodes.VideoNotFound;
                            collection = MyUtility.setError(errorCode, "Media not found.");
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                    }
                    else
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "Media not found.");
                    }
                }
                else
                {
                    errorCode = ErrorCodes.EpisodeNotFound;
                    collection = MyUtility.setError(errorCode, "MediaId not found.");
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        private Dictionary<string, object> ProcessPremiumAsset(Episode episode, int p, int q)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = String.Empty;
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
            try
            {
                var premiumAsset = episode.PremiumAssets.LastOrDefault();
                if (premiumAsset != null)
                {
                    Asset asset = premiumAsset.Asset;
                    if (asset != null)
                    {
                        int assetId = asset == null ? 0 : asset.AssetId;
                        if (episode.IsLiveChannelActive == true) //isLiveEvent
                        {
                            if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(premiumAsset))
                            {
                                if (Request.Browser.IsMobileDevice)
                                {
                                    errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                    collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                    return collection;
                                }
                            }
                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails(episode.EpisodeId, assetId, Request, User, true);
                        }
                        else
                        {
                            VideoQualityCdnReference quality = q == 0 ? VideoQualityCdnReference.StandardDefinition : VideoQualityCdnReference.HighDefinition;
                            if (p > 0 && p != 3)
                            {
                                var progressive = p == 1 ? Progressive.Low : Progressive.High;
                                if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                else
                                {
                                    if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                                    {
                                        if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                                        {
                                            var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                                            if (listOfEpisodeIds.Contains(episode.EpisodeId))
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                            else
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                        }
                                        else
                                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                    }
                                    else
                                        clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                }
                            }
                            else if (p == 3)
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetailsHD(episode.EpisodeId, assetId, Request, User);
                            else
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetails(episode.EpisodeId, assetId, Request, User, quality);

                        }
                    }
                }
                if (!String.IsNullOrEmpty(clipDetails.Url))
                {
                    errorCode = ErrorCodes.Success;
                    collection = MyUtility.setError(errorCode, clipDetails.Url);
                    collection.Add("data", clipDetails);
                }
                else
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "Media not found.");
                }
            }
            catch (Exception) { }
            return collection;
        }

        private Dictionary<string, object> GetMediaVersion1(int? id, int p = 0)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;

            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            try
            {
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
                                collection = MyUtility.setError(ErrorCodes.MultipleLoginDetected, "Not allowed to use account.");
                                collection.Add("uid", User.Identity.Name);
                                FormsAuthentication.SignOut();
                                return collection;
                            }
                        }
                    }
                }

                var context = new IPTV2Entities();

                //Usage of new prevention of multiple login via database
                if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
                {
                    if (MyUtility.isUserLoggedIn())
                    {
                        HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                        var UserId = new Guid(User.Identity.Name);
                        var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            if (!String.IsNullOrEmpty(user.SessionId))
                            {
                                if (String.Compare(user.SessionId, authCookie.Value, true) != 0)
                                {
                                    collection = MyUtility.setError(ErrorCodes.MultipleLoginDetected, "Not allowed to use account.");
                                    collection.Add("uid", User.Identity.Name);
                                    FormsAuthentication.SignOut();
                                    return collection;
                                }
                            }
                        }
                    }
                }

                Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.OnlineStatusId == GlobalConfig.Visible);

                if (ep != null)
                {
                    DateTime registDt = DateTime.Now;
                    if (ep.OnlineStartDate > registDt)
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "Media not found.");
                        return collection;
                    }

                    if (ep.OnlineEndDate < registDt)
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "Media not found.");
                        return collection;
                    }

                    //var pAsset = ep.PremiumAssets.FirstOrDefault();
                    var pAsset = ep.PremiumAssets.LastOrDefault();
                    if (pAsset == null)
                    {
                        errorCode = ErrorCodes.AkamaiCdnNotFound;
                        collection = MyUtility.setError(errorCode, "MediaUrl not found.");
                        return collection;
                    }
                    Asset asset = pAsset.Asset;
                    if (asset != null)
                    {
                        int assetId = asset == null ? 0 : asset.AssetId;

                        AkamaiFlowPlayerPluginClipDetails clipDetails = null;

                        if (ep.IsLiveChannelActive == true) //isLiveEvent
                        {
                            if (!GlobalConfig.IsIosHLSCdnEnabled)
                            {
                                if (Request.Browser.IsMobileDevice)
                                {
                                    errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                    collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                    return collection;
                                }
                            }
                            else
                            {
                                //var excludedEpisodeIds = MyUtility.StringToIntList(GlobalConfig.WhitelistedLiveStreamEpisodeIdFromMobileCheck);
                                //if (!excludedEpisodeIds.Contains((int)id))
                                //{
                                //    if (Request.Browser.IsMobileDevice)
                                //    {
                                //        errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                //        collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                //        return Content(MyUtility.buildJson(collection), "application/json");
                                //    }
                                //}
                                //if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOnAsset(ep))
                                if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOPremiumnAsset(pAsset))
                                {
                                    if (Request.Browser.IsMobileDevice)
                                    {
                                        errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                        collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                        return collection;
                                    }
                                }
                            }

                            if (!Request.IsAjaxRequest())
                                if (!User.Identity.IsAuthenticated)
                                {
                                    errorCode = ErrorCodes.NotAuthenticated;
                                    collection = MyUtility.setError(errorCode, "User is not logged in");
                                    return collection;
                                }

                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails(ep.EpisodeId, assetId, Request, User, true);
                            if (GlobalConfig.IsLiveEventEntitlementCheckEnabled)
                            {
                                Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                if (!ContextHelper.CanPlayVideo(context, offering, ep, asset, User, Request))
                                {
                                    clipDetails.Url = String.Empty;
                                    errorCode = ErrorCodes.UserIsNotEntitled;
                                    collection = MyUtility.setError(errorCode, "Live event can't be played.");
                                    return collection;
                                }
                            }
                            else
                            {
                                //Get a list of episodes that are tagged as free
                                if (!String.IsNullOrEmpty(GlobalConfig.FreeLiveEventEpisodeIds))
                                {
                                    var freeLiveEventIds = MyUtility.StringToIntList(GlobalConfig.FreeLiveEventEpisodeIds);
                                    if (!(freeLiveEventIds.Contains(ep.EpisodeId))) //means it is not part of the free episodes
                                    {
                                        Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                        if (!ContextHelper.CanPlayVideo(context, offering, ep, asset, User, Request))
                                        {
                                            clipDetails.Url = String.Empty;
                                            errorCode = ErrorCodes.UserIsNotEntitled;
                                            collection = MyUtility.setError(errorCode, "Live event can't be played.");
                                            return collection;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (p > 0)
                            {
                                var progressive = p == 1 ? Progressive.Low : Progressive.High;
                                if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(ep.EpisodeId, assetId, Request, User, progressive);
                                else
                                {
                                    if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                                    {
                                        if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                                        {
                                            var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                                            if (listOfEpisodeIds.Contains(ep.EpisodeId))
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(ep.EpisodeId, assetId, Request, User, progressive);
                                            else
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(ep.EpisodeId, assetId, Request, User, progressive);
                                        }
                                        else
                                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(ep.EpisodeId, assetId, Request, User, progressive);
                                    }
                                    else
                                        clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(ep.EpisodeId, assetId, Request, User, progressive);
                                }


                                //if (!MyUtility.isUserLoggedIn())
                                Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                if (!ContextHelper.CanPlayVideo(context, offering, ep, asset, User, Request))
                                {
                                    clipDetails.Url = String.Empty;
                                    errorCode = ErrorCodes.UserIsNotEntitled;
                                    collection = MyUtility.setError(errorCode, "Progressive media can't be played.");
                                    return collection;
                                }

                            }
                            else
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetails(ep.EpisodeId, assetId, Request, User);
                        }

                        if (!String.IsNullOrEmpty(clipDetails.Url))
                        {
                            errorCode = ErrorCodes.Success;
                            collection = MyUtility.setError(errorCode, clipDetails.Url);
                            collection.Add("data", clipDetails);
                        }
                        else
                        {
                            errorCode = ErrorCodes.AkamaiCdnNotFound;
                            collection = MyUtility.setError(errorCode, "MediaUrl not found.");
                            if (Akamai.IsIos(Request))
                            {
                                errorCode = ErrorCodes.IPodPreviewNotAvailable;
                                collection = MyUtility.setError(errorCode, "Media preview not available.");
                            }
                        }
                    }
                    else
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "Media not found.");
                    }
                }
                else
                {
                    errorCode = ErrorCodes.EpisodeNotFound;
                    collection = MyUtility.setError(errorCode, "MediaId not found.");
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return collection;
        }

        private Dictionary<string, object> GetMediaVersion2(int? id, int p = 0, int q = 0)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            if (!(Request.IsLocal || GlobalConfig.isUAT))
                if (!Request.IsAjaxRequest())
                {
                    errorCode = ErrorCodes.IsInvalidRequest;
                    collection = MyUtility.setError(errorCode, "Request is not valid.");
                    return collection;
                }

            try
            {
                var context = new IPTV2Entities();
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                User user = null;
                if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                        var UserId = new Guid(User.Identity.Name);
                        user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            CountryCode = user.CountryCode;
                            if (!MyUtility.IsWhiteListed(user.EMail))
                            {
                                if (!String.IsNullOrEmpty(user.SessionId))
                                {
                                    if (String.Compare(user.SessionId, authCookie.Value, true) != 0)
                                    {
                                        collection = MyUtility.setError(ErrorCodes.MultipleLoginDetected, "Not allowed to use account.");
                                        collection.Add("uid", User.Identity.Name);
                                        FormsAuthentication.SignOut();
                                        return collection;
                                    }
                                }
                            }
                        }
                    }
                }

                Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                DateTime registDt = DateTime.Now;
                if (episode != null)
                {
                    if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                    {
                        Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                        AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                        var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true, offering);
                        //var episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                        var categoryIds = episode.EpisodeCategories.Where(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                        var HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                        if (categoryIds != null)
                            HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, categoryIds.Select(c => c.CategoryId), registDt);
                        //HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, episodeCategory.CategoryId, registDt);                       

                        if (HasActiveSubscriptionBasedOnCategoryId.HasSubscription) // Get premium asset
                        {
                            collection = ProcessPremiumAsset(episode, p, q);
                        }
                        else //get preview asset
                        {
                            var previewAsset = episode.PreviewAssets.LastOrDefault();
                            if (previewAsset != null)
                            {
                                collection = ProcessPreviewAsset(episode, p, q);
                                //Asset asset = previewAsset.Asset;
                                //if (asset != null)
                                //{
                                //    int assetId = asset == null ? 0 : asset.AssetId;
                                //    if (episode.IsLiveChannelActive == true) //isLiveEvent
                                //    {
                                //        if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(previewAsset))
                                //        {
                                //            if (Request.Browser.IsMobileDevice)
                                //            {
                                //                errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                //                collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                //                return collection;
                                //            }
                                //        }
                                //        clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails(episode.EpisodeId, assetId, Request, User, true);
                                //    }
                                //    else
                                //    {
                                //        if (p > 0 && p != 3)
                                //        {
                                //            var progressive = p == 1 ? Progressive.Low : Progressive.High;
                                //            if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                                //                clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive);
                                //            else
                                //            {
                                //                if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                                //                {
                                //                    if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                                //                    {
                                //                        var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                                //                        if (listOfEpisodeIds.Contains(episode.EpisodeId))
                                //                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive);
                                //                        else
                                //                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive);
                                //                    }
                                //                    else
                                //                        clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive);
                                //                }
                                //                else
                                //                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive);
                                //            }
                                //        }
                                //        else if (p == 3)
                                //        {

                                //        }
                                //        else
                                //            clipDetails = Helpers.Akamai.GetAkamaiClipDetails(episode.EpisodeId, assetId, Request, User, VideoQualityCdnReference.StandardDefinition);
                                //    }
                                //}
                            }
                            else // no preview asset, use 60 seconds
                            {
                                if (!(episode.IsLiveChannelActive == true))
                                {
                                    if (Akamai.IsIos(Request))
                                    {
                                        errorCode = ErrorCodes.IPodPreviewNotAvailable;
                                        collection = MyUtility.setError(errorCode, "Media preview not available.");
                                    }
                                    else
                                        collection = ProcessPremiumAsset(episode, p, q);
                                }
                                else
                                {
                                    if (User.Identity.IsAuthenticated)
                                    {
                                        if (!HasActiveSubscriptionBasedOnCategoryId.HasSubscription)
                                        {
                                            errorCode = ErrorCodes.UserIsNotEntitled;
                                            collection = MyUtility.setError(errorCode, "Live event can't be played.");
                                        }
                                    }
                                    else
                                    {
                                        errorCode = ErrorCodes.NotAuthenticated;
                                        collection = MyUtility.setError(errorCode, "You are not logged in.");
                                    }
                                }
                            }
                        }
                        if (clipDetails != null)
                        {
                            if (!String.IsNullOrEmpty(clipDetails.Url))
                            {
                                errorCode = ErrorCodes.Success;
                                collection = MyUtility.setError(errorCode, clipDetails.Url);
                                collection.Add("data", clipDetails);
                            }
                            else
                            {
                                errorCode = ErrorCodes.VideoNotFound;
                                collection = MyUtility.setError(errorCode, "Media not found.");
                                return collection;
                            }
                        }
                    }
                    else
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "Media not found.");
                    }
                }
                else
                {
                    errorCode = ErrorCodes.EpisodeNotFound;
                    collection = MyUtility.setError(errorCode, "MediaId not found.");
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return collection;
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetMedia(int? id, int p = 0, int q = 0, int j = 0, int qxt = 0)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            if (GlobalConfig.IsMobilePreviewEnabled)
            {
                if (id == GlobalConfig.ProjectAirEpisodeId || j == 1)
                {
                    if (qxt == 1)
                        collection = TEST_GetMediaVersion_M3U8(id, p, q);
                    else
                        collection = GetMediaVersion_M3U8(id, p, q);
                    collection.Add("eid", id);
                }
                else
                {
                    //collection = GetMediaVersion2(id, p, q); - UNCOMMENT, STABLE FUNCTION
                    if (GlobalConfig.UseJWPlayer)
                        collection = GetMediaVersionFIX(id, p, q);
                    else
                        collection = GetMediaVersion2(id, p, q);
                }
            }
            else
                collection = GetMediaVersion1(id, p);
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        [HttpPost]
        public JsonResult CreateInteraction(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };
            try
            {
                if (User.Identity.IsAuthenticated)
                {
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

                    if (!isMissingRequiredFields) // process form
                    {
                        int reactionId = Convert.ToInt32(fc["reactionId"]);
                        string type = fc["type"];
                        int id = Convert.ToInt32(fc["id"]);
                        var UserId = new Guid(User.Identity.Name);
                        var registDt = DateTime.Now;
                        using (var context = new EngagementsEntities())
                        {
                            switch (type)
                            {
                                case "show":
                                    if (reactionId == GlobalConfig.SOCIAL_LOVE) //12 Is Social_Love
                                    {
                                        int click = Convert.ToInt32(fc["click"]);
                                        var reaction = context.ShowReactions.FirstOrDefault(i => i.CategoryId == id && i.ReactionTypeId == reactionId && i.UserId == UserId);
                                        if (reaction != null || click == 0)
                                            context.ShowReactions.Remove(reaction);
                                        else
                                        {
                                            reaction = new ShowReaction()
                                            {
                                                UserId = UserId,
                                                CategoryId = id,
                                                ReactionTypeId = reactionId,
                                                DateTime = registDt
                                            };
                                            context.ShowReactions.Add(reaction);
                                        }
                                    }
                                    else
                                    {
                                        var reaction = new ShowReaction()
                                        {
                                            UserId = UserId,
                                            CategoryId = id,
                                            ReactionTypeId = reactionId,
                                            DateTime = registDt
                                        };
                                        context.ShowReactions.Add(reaction);
                                    }
                                    break;
                                case "episode":
                                    if (reactionId == GlobalConfig.SOCIAL_LOVE) //12 Is Social_Love
                                    {
                                        int click = Convert.ToInt32(fc["click"]);
                                        var reaction = context.EpisodeReactions.FirstOrDefault(i => i.EpisodeId == id && i.ReactionTypeId == reactionId && i.UserId == UserId);
                                        if (reaction != null || click == 0)
                                            context.EpisodeReactions.Remove(reaction);
                                        else
                                        {
                                            reaction = new EpisodeReaction()
                                            {
                                                UserId = UserId,
                                                EpisodeId = id,
                                                ReactionTypeId = reactionId,
                                                DateTime = registDt
                                            };
                                            context.EpisodeReactions.Add(reaction);
                                        }
                                    }
                                    else
                                    {
                                        var reaction = new EpisodeReaction()
                                        {
                                            UserId = UserId,
                                            EpisodeId = id,
                                            ReactionTypeId = reactionId,
                                            DateTime = registDt
                                        };
                                        context.EpisodeReactions.Add(reaction);
                                    }
                                    break;
                                case "celebrity":
                                    if (reactionId == GlobalConfig.SOCIAL_LOVE) //12 Is Social_Love
                                    {
                                        int click = Convert.ToInt32(fc["click"]);
                                        var reaction = context.CelebrityReactions.FirstOrDefault(i => i.CelebrityId == id && i.ReactionTypeId == reactionId && i.UserId == UserId);
                                        if (reaction != null || click == 0)
                                            context.CelebrityReactions.Remove(reaction);
                                        else
                                        {
                                            reaction = new CelebrityReaction()
                                            {
                                                UserId = UserId,
                                                CelebrityId = id,
                                                ReactionTypeId = reactionId,
                                                DateTime = registDt
                                            };
                                            context.CelebrityReactions.Add(reaction);
                                        }
                                    }
                                    else
                                    {
                                        var reaction = new CelebrityReaction()
                                        {
                                            UserId = UserId,
                                            CelebrityId = id,
                                            ReactionTypeId = reactionId,
                                            DateTime = registDt
                                        };
                                        context.CelebrityReactions.Add(reaction);
                                    }
                                    break;
                            }

                            context.SaveChanges();
                        }
                    }

                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetReactionCount(int reactionId, string type, int id)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
            };

            try
            {
                int reactionCount = 0;
                using (var context = new EngagementsEntities())
                {
                    switch (type)
                    {
                        case "show":
                            reactionCount = context.ShowReactions.Count(i => i.CategoryId == id && i.ReactionTypeId == reactionId);
                            break;
                        case "episode":
                            reactionCount = context.EpisodeReactions.Count(i => i.EpisodeId == id && i.ReactionTypeId == reactionId);
                            break;
                        case "celebrity":
                            reactionCount = context.CelebrityReactions.Count(i => i.CelebrityId == id && i.ReactionTypeId == reactionId);
                            break;
                    }
                }
                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                ReturnCode.StatusMessage = String.Format("{0}", reactionCount);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ViewPlayback(int id)
        {
            //Log onto TFCtvWebApi
            try
            {
                string url = String.Format("http://tfctvwebapi.cloudapp.net/api/videoPlaybacks/{0}/user/{1}", id, User.Identity.Name);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                WebResponse response = req.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                return this.Json(responseFromServer, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { }
            return this.Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LogUserToPromo()
        {
            var ReturnCode = new TransactionReturnType()
                       {
                           StatusCode = (int)ErrorCodes.UnknownError,
                           StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
                       };

            if (!Request.IsLocal)
                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Your request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

            if (!User.Identity.IsAuthenticated)
            {
                ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                ReturnCode.StatusMessage = "Your request is invalid.";
                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var UserId = new Guid(User.Identity.Name);
                var registDt = DateTime.Now;
                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                if (user != null)
                {
                    var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.ProjectAirPromoId && p.StartDate < registDt && p.EndDate > registDt && p.StatusId == GlobalConfig.Visible);
                    if (promo != null)
                    {
                        if (context.UserPromos.Count(p => p.PromoId == GlobalConfig.ProjectAirPromoId && p.UserId == user.UserId) <= 0)
                        {
                            var uObj = new UserPromo()
                            {
                                UserId = user.UserId,
                                PromoId = promo.PromoId,
                                AuditTrail = new AuditTrail() { CreatedOn = DateTime.Now }
                            };
                            context.UserPromos.Add(uObj);
                            if (context.SaveChanges() > 0)
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                ReturnCode.StatusMessage = "Success";
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusMessage = e.Message;
            }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCurrentShow()
        {
            var ReturnCode = new ShowListDisplay()
            {
                Name = String.Empty
            };

            try
            {
                var context = new IPTV2Entities();
                DateTime utc = DateTime.Now.ToUniversalTime();
                DateTime gmt = utc.AddHours(8); //convert to GMT
                var dow = (int)gmt.DayOfWeek;
                var channels = GlobalConfig.ProjectAirProgramScheduleChannelIds.Split(','); //Channel Id (Sunday-Saturday)
                var psChannelId = Convert.ToInt32(channels[dow]); //ProgramSchedule Channel Id                               

                var sked = context.ProgramSchedules.Where(p => p.ChannelId == psChannelId);
                if (sked != null)
                    if (sked.Count() > 0)
                    {
                        var military_time = gmt.ToString("HH:mm");
                        var current_show = sked.FirstOrDefault(s => military_time.CompareTo(s.StartTime) >= 0 && military_time.CompareTo(s.EndTime) <= 0);
                        if (current_show != null)
                            ReturnCode.Name = current_show.ShowName;
                    }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        private Dictionary<string, object> ProcessPreviewAsset(Episode episode, int p, int q)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = String.Empty;
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
            try
            {
                var previewAsset = episode.PreviewAssets.LastOrDefault();
                if (previewAsset != null)
                {
                    Asset asset = previewAsset.Asset;
                    if (asset != null)
                    {
                        int assetId = asset == null ? 0 : asset.AssetId;
                        if (episode.IsLiveChannelActive == true) //isLiveEvent
                        {
                            if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(previewAsset))
                            {
                                if (Request.Browser.IsMobileDevice)
                                {
                                    errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                    collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                    return collection;
                                }
                            }
                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails(episode.EpisodeId, assetId, Request, User, true);

                            ///**ADDED PREVIEW VIDEO (VOD) for Livestream**/
                            //VideoQualityCdnReference quality = q == 0 ? VideoQualityCdnReference.StandardDefinition : VideoQualityCdnReference.HighDefinition;
                            //if (p > 0 && p != 3)
                            //{
                            //    var progressive = p == 1 ? Progressive.Low : Progressive.High;
                            //    if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                            //        clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                            //    else
                            //    {
                            //        if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                            //        {
                            //            if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                            //            {
                            //                var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                            //                if (listOfEpisodeIds.Contains(episode.EpisodeId))
                            //                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                            //                else
                            //                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                            //            }
                            //            else
                            //                clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                            //        }
                            //        else
                            //            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                            //    }
                            //}
                            //else if (p == 3)
                            //{
                            //    clipDetails = Helpers.Akamai.GetAkamaiClipDetailsHD(episode.EpisodeId, assetId, Request, User);
                            //    if (clipDetails != null)
                            //        if (String.IsNullOrEmpty(clipDetails.Url))
                            //            clipDetails = Helpers.Akamai.GetAkamaiClipDetails(episode.EpisodeId, assetId, Request, User, quality);
                            //}
                            //else
                            //    clipDetails = Helpers.Akamai.GetAkamaiClipDetails(episode.EpisodeId, assetId, Request, User, quality);
                            ///** END **/

                        }
                        else
                        {
                            VideoQualityCdnReference quality = q == 0 ? VideoQualityCdnReference.StandardDefinition : VideoQualityCdnReference.HighDefinition;
                            if (p > 0 && p != 3)
                            {
                                var progressive = p == 1 ? Progressive.Low : Progressive.High;
                                if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                else
                                {
                                    if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                                    {
                                        if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                                        {
                                            var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                                            if (listOfEpisodeIds.Contains(episode.EpisodeId))
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                            else
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                        }
                                        else
                                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                    }
                                    else
                                        clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                }
                            }
                            else if (p == 3)
                            {
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetailsHD(episode.EpisodeId, assetId, Request, User);
                                if (clipDetails != null)
                                    if (String.IsNullOrEmpty(clipDetails.Url))
                                        clipDetails = Helpers.Akamai.GetAkamaiClipDetails(episode.EpisodeId, assetId, Request, User, quality);
                            }
                            else
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetails(episode.EpisodeId, assetId, Request, User, quality);

                        }
                    }
                }
                if (!String.IsNullOrEmpty(clipDetails.Url))
                {
                    errorCode = ErrorCodes.Success;
                    collection = MyUtility.setError(errorCode, clipDetails.Url);
                    collection.Add("data", clipDetails);
                }
                else
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "Media not found.");
                }
            }
            catch (Exception) { }
            return collection;
        }

        private Dictionary<string, object> GetMediaVersion_M3U8(int? id, int p = 0, int q = 0)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            if (!Request.IsLocal)
                if (!GlobalConfig.isUAT)
                    if (!Request.IsAjaxRequest())
                    {
                        errorCode = ErrorCodes.IsInvalidRequest;
                        collection = MyUtility.setError(errorCode, "Request is not valid.");
                        return collection;
                    }

            try
            {
                var context = new IPTV2Entities();
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                User user = null;
                if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                        var UserId = new Guid(User.Identity.Name);
                        user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            CountryCode = user.CountryCode;
                            if (!MyUtility.IsWhiteListed(user.EMail))
                            {
                                if (!String.IsNullOrEmpty(user.SessionId))
                                {
                                    if (String.Compare(user.SessionId, authCookie.Value, true) != 0)
                                    {
                                        collection = MyUtility.setError(ErrorCodes.MultipleLoginDetected, "Not allowed to use account.");
                                        collection.Add("uid", User.Identity.Name);
                                        FormsAuthentication.SignOut();
                                        return collection;
                                    }
                                }
                            }
                        }
                    }
                }

                Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                DateTime registDt = DateTime.Now;
                if (episode != null)
                {
                    if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                    {
                        var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                        var parentCategories = episode.GetParentShows(CacheDuration);

                        Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                        AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                        var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true, offering);
                        //var episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                        //var categoryIds = episode.EpisodeCategories.Where(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                        var HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                        if (parentCategories != null)
                            HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt, !User.Identity.IsAuthenticated ? CountryCode : null);
                        //HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, episodeCategory.CategoryId, registDt);                       

                        if (HasActiveSubscriptionBasedOnCategoryId.HasSubscription) // Get premium asset
                        {
                            collection = ProcessPremiumAsset_M3U8(episode, p, q);
                        }
                        else //get preview asset
                        {
                            var previewAsset = episode.PreviewAssets.LastOrDefault();
                            if (previewAsset != null)
                            {
                                collection = ProcessPreviewAsset_M3U8(episode, p, q);
                            }
                            else // no preview asset, use 60 seconds
                            {
                                if (!(episode.IsLiveChannelActive == true))
                                {
                                    if (Request.Browser.IsMobileDevice)
                                    {
                                        errorCode = ErrorCodes.IPodPreviewNotAvailable;
                                        collection = MyUtility.setError(errorCode, "This stream is not available on this device");
                                    }
                                    else
                                        collection = ProcessPremiumAsset_M3U8(episode, p, q);
                                }
                                else
                                {
                                    if (User.Identity.IsAuthenticated)
                                    {
                                        if (!HasActiveSubscriptionBasedOnCategoryId.HasSubscription)
                                        {
                                            errorCode = ErrorCodes.UserIsNotEntitled;
                                            collection = MyUtility.setError(errorCode, "You must be subscribed to watch this stream");
                                        }
                                    }
                                    else
                                    {
                                        if (!HasActiveSubscriptionBasedOnCategoryId.HasSubscription)
                                        {
                                            errorCode = ErrorCodes.NotAuthenticated;
                                            collection = MyUtility.setError(errorCode, "You must be logged in to watch this stream");
                                        }
                                    }
                                }
                            }
                        }
                        if (clipDetails != null)
                        {
                            if (!String.IsNullOrEmpty(clipDetails.Url))
                            {
                                errorCode = ErrorCodes.Success;
                                collection = MyUtility.setError(errorCode, clipDetails.Url);
                                collection.Add("data", clipDetails);
                            }
                            else
                            {
                                errorCode = ErrorCodes.VideoNotFound;
                                collection = MyUtility.setError(errorCode, "This video does not exist on this site");
                                return collection;
                            }
                        }
                    }
                    else
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "This video does not exist on this site");
                    }
                }
                else
                {
                    errorCode = ErrorCodes.EpisodeNotFound;
                    collection = MyUtility.setError(errorCode, "This video does not exist on this site");
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return collection;
        }

        private Dictionary<string, object> ProcessPremiumAsset_M3U8(Episode episode, int p, int q)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = String.Empty;
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
            try
            {
                var premiumAsset = episode.PremiumAssets.LastOrDefault();
                if (premiumAsset != null)
                {
                    Asset asset = premiumAsset.Asset;
                    if (asset != null)
                    {
                        int assetId = asset == null ? 0 : asset.AssetId;
                        if (episode.IsLiveChannelActive == true) //isLiveEvent
                        {
                            if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(premiumAsset))
                            {
                                if (Request.Browser.IsMobileDevice)
                                {
                                    errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                    collection = MyUtility.setError(errorCode, "This stream is not available on this device");
                                    return collection;
                                }
                            }
                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, true);
                        }
                        else
                        {
                            VideoQualityCdnReference quality = q == 0 ? VideoQualityCdnReference.StandardDefinition : VideoQualityCdnReference.HighDefinition;
                            if (p > 0 && p != 3)
                            {
                                var progressive = p == 1 ? Progressive.Low : Progressive.High;
                                if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                else
                                {
                                    if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                                    {
                                        if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                                        {
                                            var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                                            if (listOfEpisodeIds.Contains(episode.EpisodeId))
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                            else
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                        }
                                        else
                                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                    }
                                    else
                                        clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                }
                            }
                            else if (p == 3)
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetailsHD_M3U8(episode.EpisodeId, assetId, Request, User);
                            else
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, quality);

                        }
                    }
                }
                if (!String.IsNullOrEmpty(clipDetails.Url))
                {
                    errorCode = ErrorCodes.Success;
                    collection = MyUtility.setError(errorCode, clipDetails.Url);
                    collection.Add("data", clipDetails);
                }
                else
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "This video does not exist on this site");
                }
            }
            catch (Exception) { }
            return collection;
        }

        private Dictionary<string, object> ProcessPreviewAsset_M3U8(Episode episode, int p, int q)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = String.Empty;
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
            try
            {
                var previewAsset = episode.PreviewAssets.LastOrDefault();
                if (previewAsset != null)
                {
                    Asset asset = previewAsset.Asset;
                    if (asset != null)
                    {
                        int assetId = asset == null ? 0 : asset.AssetId;
                        if (episode.IsLiveChannelActive == true) //isLiveEvent
                        {
                            if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(previewAsset))
                            {
                                if (Request.Browser.IsMobileDevice)
                                {
                                    errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                    collection = MyUtility.setError(errorCode, "This stream is not available on this device");
                                    return collection;
                                }
                            }
                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, true);
                        }
                        else
                        {
                            VideoQualityCdnReference quality = q == 0 ? VideoQualityCdnReference.StandardDefinition : VideoQualityCdnReference.HighDefinition;
                            if (p > 0 && p != 3)
                            {
                                var progressive = p == 1 ? Progressive.Low : Progressive.High;
                                if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                else
                                {
                                    if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                                    {
                                        if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                                        {
                                            var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                                            if (listOfEpisodeIds.Contains(episode.EpisodeId))
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                            else
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                        }
                                        else
                                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                    }
                                    else
                                        clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                }
                            }
                            else if (p == 3)
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetailsHD_M3U8(episode.EpisodeId, assetId, Request, User);
                            else
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, quality);

                        }
                    }
                }
                if (!String.IsNullOrEmpty(clipDetails.Url))
                {
                    errorCode = ErrorCodes.Success;
                    collection = MyUtility.setError(errorCode, clipDetails.Url);
                    collection.Add("data", clipDetails);
                }
                else
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "This video does not exist on this site");
                }
            }
            catch (Exception) { }
            return collection;
        }


        private Dictionary<string, object> TEST_GetMediaVersion_M3U8(int? id, int p = 0, int q = 0)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            if (!Request.IsLocal)
                if (!GlobalConfig.isUAT)
                    if (!Request.IsAjaxRequest())
                    {
                        errorCode = ErrorCodes.IsInvalidRequest;
                        collection = MyUtility.setError(errorCode, "Request is not valid.");
                        return collection;
                    }

            try
            {
                var context = new IPTV2Entities();

                User user = null;
                if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                        var UserId = new Guid(User.Identity.Name);
                        user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            if (!MyUtility.IsWhiteListed(user.EMail))
                            {
                                if (!String.IsNullOrEmpty(user.SessionId))
                                {
                                    if (String.Compare(user.SessionId, authCookie.Value, true) != 0)
                                    {
                                        collection = MyUtility.setError(ErrorCodes.MultipleLoginDetected, "Not allowed to use account.");
                                        collection.Add("uid", User.Identity.Name);
                                        FormsAuthentication.SignOut();
                                        return collection;
                                    }
                                }
                            }
                        }
                    }
                }

                Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                DateTime registDt = DateTime.Now;
                if (episode != null)
                {
                    if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                    {
                        var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                        var parentCategories = episode.GetParentShows(CacheDuration);

                        Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                        AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                        //var episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                        //var categoryIds = episode.EpisodeCategories.Where(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                        var HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                        if (parentCategories != null)
                            HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt, null);
                        //HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, episodeCategory.CategoryId, registDt);                       

                        if (HasActiveSubscriptionBasedOnCategoryId.HasSubscription) // Get premium asset
                        {
                            collection = ProcessPremiumAsset_M3U8(episode, p, q);
                        }
                        else //get preview asset
                        {
                            var previewAsset = episode.PreviewAssets.LastOrDefault();
                            if (previewAsset != null)
                            {
                                collection = ProcessPreviewAsset_M3U8(episode, p, q);
                            }
                            else // no preview asset, use 60 seconds
                            {
                                if (!(episode.IsLiveChannelActive == true))
                                {
                                    if (Request.Browser.IsMobileDevice)
                                    {
                                        errorCode = ErrorCodes.IPodPreviewNotAvailable;
                                        collection = MyUtility.setError(errorCode, "This stream is not available on this device");
                                    }
                                    else
                                        collection = ProcessPremiumAsset_M3U8(episode, p, q);
                                }
                                else
                                {
                                    if (User.Identity.IsAuthenticated)
                                    {
                                        if (!HasActiveSubscriptionBasedOnCategoryId.HasSubscription)
                                        {
                                            errorCode = ErrorCodes.UserIsNotEntitled;
                                            collection = MyUtility.setError(errorCode, "You must be subscribed to watch this stream");
                                        }
                                    }
                                    else
                                    {
                                        if (!HasActiveSubscriptionBasedOnCategoryId.HasSubscription)
                                        {
                                            errorCode = ErrorCodes.NotAuthenticated;
                                            collection = MyUtility.setError(errorCode, "You must be logged in to watch this stream");
                                        }
                                    }
                                }
                            }
                        }
                        if (clipDetails != null)
                        {
                            if (!String.IsNullOrEmpty(clipDetails.Url))
                            {
                                errorCode = ErrorCodes.Success;
                                collection = MyUtility.setError(errorCode, clipDetails.Url);
                                collection.Add("data", clipDetails);
                            }
                            else
                            {
                                errorCode = ErrorCodes.VideoNotFound;
                                collection = MyUtility.setError(errorCode, "This video does not exist on this site");
                                return collection;
                            }
                        }
                    }
                    else
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "This video does not exist on this site");
                    }
                }
                else
                {
                    errorCode = ErrorCodes.EpisodeNotFound;
                    collection = MyUtility.setError(errorCode, "This video does not exist on this site");
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return collection;
        }

        [HttpPost]
        public JsonResult ResolveMediaUrl(string url)
        {
            ReturnObj obj = new ReturnObj();
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                WebResponse response = req.GetResponse();
                if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
                    obj.IsSuccess = true;
                //var statusDescription = ((HttpWebResponse)response).StatusDescription;
                //Stream dataStream = response.GetResponseStream();
                //StreamReader reader = new StreamReader(dataStream);
                //string responseFromServer = reader.ReadToEnd();
                //reader.Close();
                //dataStream.Close();
                response.Close();
            }
            catch (Exception) { }
            return this.Json(obj, JsonRequestBehavior.AllowGet);
        }


        public JsonResult MakePlaybackApiRequest(int id)
        {
            VideoApiPlaybackObj obj = null;
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var uri = String.Format("{0}/{1}/user/{2}", GlobalConfig.TFCtvApiVideoPlaybackUri, id, User.Identity.Name);
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                    req.ContentType = "application/json";
                    WebResponse response = req.GetResponse();
                    Stream dataStream = response.GetResponseStream();
                    var statusCode = ((HttpWebResponse)response).StatusCode;
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                    if (!String.IsNullOrEmpty(responseFromServer) && statusCode == HttpStatusCode.OK)
                        obj = Newtonsoft.Json.JsonConvert.DeserializeObject<VideoApiPlaybackObj>(responseFromServer);
                }
            }
            catch (Exception) { }
            return this.Json(obj, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPacMayVideos()
        {
            List<PacMayVideoObj> list = null;
            DateTime registDt = DateTime.Now;
            try
            {
                var cache = DataCache.Cache;
                string cacheKey = "MPPPVLIVE:O:";
                list = (List<PacMayVideoObj>)cache[cacheKey];

                if (list == null)
                {
                    var context = new IPTV2Entities();
                    var show = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.PacMayTrendingVideosCategoryId);
                    if (show != null)
                    {
                        if (show.StartDate < registDt && show.EndDate > registDt && show.StatusId == GlobalConfig.Visible)
                        {
                            var episode_category = context.EpisodeCategories1.Where(e => e.CategoryId == show.CategoryId);
                            if (episode_category != null)
                            {
                                var episode_list = episode_category.Select(e => e.EpisodeId);
                                var episodes = context.Episodes.Where(e => episode_list.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).ThenByDescending(e => e.EpisodeId);
                                if (episodes != null)
                                {
                                    if (episodes.Count() > 0)
                                    {
                                        list = new List<PacMayVideoObj>();
                                        foreach (var episode in episodes)
                                        {
                                            try
                                            {
                                                string img = GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif;
                                                if (!String.IsNullOrEmpty(episode.ImageAssets.ImageVideo))
                                                    img = String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId, episode.ImageAssets.ImageVideo);

                                                list.Add(new PacMayVideoObj()
                                                {
                                                    DateAired = episode.AuditTrail.UpdatedOn.HasValue ? episode.AuditTrail.UpdatedOn.Value.ToString("MM/dd/yyyy hh:mm tt") : episode.AuditTrail.CreatedOn.ToString("MM/dd/yyyy hh:mm tt"),
                                                    Title = episode.EpisodeName,
                                                    img = img,
                                                    url = String.Format("{0}/Episode/Details/{1}/{2}", GlobalConfig.baseUrl, episode.EpisodeId, MyUtility.GetSlug(episode.EpisodeName)),
                                                    EpisodeId = episode.EpisodeId
                                                });
                                            }
                                            catch (Exception) { }
                                        }

                                        //move to top of the list
                                        try
                                        {
                                            var idx = list.FindIndex(l => l.EpisodeId == GlobalConfig.PacMayPlugOnTrendingVideosEpisodeId);
                                            var item = list[idx];
                                            list.RemoveAt(idx);
                                            list.Insert(0, item);
                                        }
                                        catch (Exception) { }
                                        cache.Put(cacheKey, list, DataCache.CacheDuration);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return this.Json(list, JsonRequestBehavior.AllowGet);
        }

        private Dictionary<string, object> GetMediaVersionFIX(int? id, int p = 0, int q = 0)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            if (!(Request.IsLocal || GlobalConfig.isUAT))
                if (!Request.IsAjaxRequest())
                {
                    errorCode = ErrorCodes.IsInvalidRequest;
                    collection = MyUtility.setError(errorCode, "Request is not valid.");
                    return collection;
                }

            try
            {
                var context = new IPTV2Entities();
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                User user = null;
                if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                        var UserId = new Guid(User.Identity.Name);
                        user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            CountryCode = user.CountryCode;
                            if (!MyUtility.IsWhiteListed(user.EMail))
                            {
                                if (!String.IsNullOrEmpty(user.SessionId))
                                {
                                    if (String.Compare(user.SessionId, authCookie.Value, true) != 0)
                                    {
                                        collection = MyUtility.setError(ErrorCodes.MultipleLoginDetected, "Not allowed to use account.");
                                        collection.Add("uid", User.Identity.Name);
                                        FormsAuthentication.SignOut();
                                        return collection;
                                    }
                                }
                            }
                        }
                    }
                }

                Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                DateTime registDt = DateTime.Now;
                if (episode != null)
                {
                    if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                    {
                        Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                        AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                        var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true, offering);
                        //var episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                        var categoryIds = episode.EpisodeCategories.Where(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                        var HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                        if (categoryIds != null)
                            HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, categoryIds.Select(c => c.CategoryId), registDt);
                        //HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, episodeCategory.CategoryId, registDt);                       

                        if (HasActiveSubscriptionBasedOnCategoryId.HasSubscription) // Get premium asset
                        {
                            collection = ProcessPremiumAssetFIX(episode, p, q);
                        }
                        else //get preview asset
                        {
                            var previewAsset = episode.PreviewAssets.LastOrDefault();
                            if (previewAsset != null)
                            {
                                collection = ProcessPreviewAssetFIX(episode, p, q);
                            }
                            else // no preview asset, use 60 seconds
                            {
                                if (!(episode.IsLiveChannelActive == true))
                                {
                                    if (Akamai.IsIos(Request))
                                    {
                                        errorCode = ErrorCodes.IPodPreviewNotAvailable;
                                        collection = MyUtility.setError(errorCode, "Media preview not available.");
                                    }
                                    else
                                        collection = ProcessPremiumAssetFIX(episode, p, q);
                                }
                                else
                                {
                                    if (User.Identity.IsAuthenticated)
                                    {
                                        if (!HasActiveSubscriptionBasedOnCategoryId.HasSubscription)
                                        {
                                            errorCode = ErrorCodes.UserIsNotEntitled;
                                            collection = MyUtility.setError(errorCode, "Live event can't be played.");
                                        }
                                    }
                                    else
                                    {
                                        errorCode = ErrorCodes.NotAuthenticated;
                                        collection = MyUtility.setError(errorCode, "You are not logged in.");
                                    }
                                }
                            }
                        }
                        if (clipDetails != null)
                        {
                            if (!String.IsNullOrEmpty(clipDetails.Url))
                            {
                                errorCode = ErrorCodes.Success;
                                collection = MyUtility.setError(errorCode, clipDetails.Url);
                                collection.Add("data", clipDetails);
                            }
                            else
                            {
                                errorCode = ErrorCodes.VideoNotFound;
                                collection = MyUtility.setError(errorCode, "Media not found.");
                                return collection;
                            }
                        }
                    }
                    else
                    {
                        errorCode = ErrorCodes.VideoNotFound;
                        collection = MyUtility.setError(errorCode, "Media not found.");
                    }
                }
                else
                {
                    errorCode = ErrorCodes.EpisodeNotFound;
                    collection = MyUtility.setError(errorCode, "MediaId not found.");
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return collection;
        }
        private Dictionary<string, object> ProcessPremiumAssetFIX(Episode episode, int p, int q)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = String.Empty;
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
            try
            {
                var premiumAsset = episode.PremiumAssets.LastOrDefault();
                if (premiumAsset != null)
                {
                    Asset asset = premiumAsset.Asset;
                    if (asset != null)
                    {
                        int assetId = asset == null ? 0 : asset.AssetId;
                        if (episode.IsLiveChannelActive == true) //isLiveEvent
                        {
                            if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(premiumAsset))
                            {
                                if (Request.Browser.IsMobileDevice)
                                {
                                    errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                    collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                    return collection;
                                }
                            }
                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails(episode.EpisodeId, assetId, Request, User, true);
                        }
                        else
                        {
                            VideoQualityCdnReference quality = q == 0 ? VideoQualityCdnReference.StandardDefinition : VideoQualityCdnReference.HighDefinition;
                            if (p > 0 && p != 3)
                            {
                                var progressive = p == 1 ? Progressive.Low : Progressive.High;
                                if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                else
                                {
                                    if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                                    {
                                        if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                                        {
                                            var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                                            if (listOfEpisodeIds.Contains(episode.EpisodeId))
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                            else
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetailsFIX(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                        }
                                        else
                                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetailsFIX(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                    }
                                    else
                                        clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetailsFIX(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                }
                            }
                            else if (p == 3)
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetailsHDFIX(episode.EpisodeId, assetId, Request, User);
                            else
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetailsFIX(episode.EpisodeId, assetId, Request, User, quality);

                        }
                    }
                }
                if (!String.IsNullOrEmpty(clipDetails.Url))
                {
                    errorCode = ErrorCodes.Success;
                    collection = MyUtility.setError(errorCode, clipDetails.Url);
                    collection.Add("data", clipDetails);
                }
                else
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "Media not found.");
                }
            }
            catch (Exception) { }
            return collection;
        }
        private Dictionary<string, object> ProcessPreviewAssetFIX(Episode episode, int p, int q)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = String.Empty;
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
            try
            {
                var previewAsset = episode.PreviewAssets.LastOrDefault();
                if (previewAsset != null)
                {
                    Asset asset = previewAsset.Asset;
                    if (asset != null)
                    {
                        int assetId = asset == null ? 0 : asset.AssetId;
                        if (episode.IsLiveChannelActive == true) //isLiveEvent
                        {
                            if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(previewAsset))
                            {
                                if (Request.Browser.IsMobileDevice)
                                {
                                    errorCode = ErrorCodes.IsNotAvailableOnMobileDevices;
                                    collection = MyUtility.setError(errorCode, "Live event not available on mobile.");
                                    return collection;
                                }
                            }
                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails(episode.EpisodeId, assetId, Request, User, true);

                            ///**ADDED PREVIEW VIDEO (VOD) for Livestream**/
                            //VideoQualityCdnReference quality = q == 0 ? VideoQualityCdnReference.StandardDefinition : VideoQualityCdnReference.HighDefinition;
                            //if (p > 0 && p != 3)
                            //{
                            //    var progressive = p == 1 ? Progressive.Low : Progressive.High;
                            //    if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                            //        clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                            //    else
                            //    {
                            //        if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                            //        {
                            //            if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                            //            {
                            //                var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                            //                if (listOfEpisodeIds.Contains(episode.EpisodeId))
                            //                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                            //                else
                            //                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                            //            }
                            //            else
                            //                clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                            //        }
                            //        else
                            //            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                            //    }
                            //}
                            //else if (p == 3)
                            //{
                            //    clipDetails = Helpers.Akamai.GetAkamaiClipDetailsHD(episode.EpisodeId, assetId, Request, User);
                            //    if (clipDetails != null)
                            //        if (String.IsNullOrEmpty(clipDetails.Url))
                            //            clipDetails = Helpers.Akamai.GetAkamaiClipDetails(episode.EpisodeId, assetId, Request, User, quality);
                            //}
                            //else
                            //    clipDetails = Helpers.Akamai.GetAkamaiClipDetails(episode.EpisodeId, assetId, Request, User, quality);
                            ///** END **/

                        }
                        else
                        {
                            VideoQualityCdnReference quality = q == 0 ? VideoQualityCdnReference.StandardDefinition : VideoQualityCdnReference.HighDefinition;
                            if (p > 0 && p != 3)
                            {
                                var progressive = p == 1 ? Progressive.Low : Progressive.High;
                                if (GlobalConfig.UseProgressiveViaAdaptiveTechnology)
                                    clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                else
                                {
                                    if (GlobalConfig.CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology)
                                    {
                                        if (!String.IsNullOrEmpty(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology))
                                        {
                                            var listOfEpisodeIds = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology);
                                            if (listOfEpisodeIds.Contains(episode.EpisodeId))
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveViaAdaptiveClipDetails(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                            else
                                                clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetailsFIX(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                        }
                                        else
                                            clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetailsFIX(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                    }
                                    else
                                        clipDetails = Helpers.Akamai.GetAkamaiProgressiveClipDetailsFIX(episode.EpisodeId, assetId, Request, User, progressive, quality);
                                }
                            }
                            else if (p == 3)
                            {
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetailsHDFIX(episode.EpisodeId, assetId, Request, User);
                                if (clipDetails != null)
                                    if (String.IsNullOrEmpty(clipDetails.Url))
                                        clipDetails = Helpers.Akamai.GetAkamaiClipDetailsFIX(episode.EpisodeId, assetId, Request, User, quality);
                            }
                            else
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetailsFIX(episode.EpisodeId, assetId, Request, User, quality);

                        }
                    }
                }
                if (!String.IsNullOrEmpty(clipDetails.Url))
                {
                    errorCode = ErrorCodes.Success;
                    collection = MyUtility.setError(errorCode, clipDetails.Url);
                    collection.Add("data", clipDetails);
                }
                else
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "Media not found.");
                }
            }
            catch (Exception) { }
            return collection;
        }

        public JsonResult GetSection(int id, int? pageSize, int page = 0)
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
            return this.Json(obj, JsonRequestBehavior.AllowGet);
        }
    }
}
