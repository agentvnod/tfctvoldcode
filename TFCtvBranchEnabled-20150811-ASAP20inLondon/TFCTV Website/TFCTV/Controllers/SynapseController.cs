using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Linq.SqlClient;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DevTrends.MvcDonutCaching;
using EngagementsModel;
using Gigya.Socialize.SDK;
using GOMS_TFCtv;
using IPTV2_Model;
using Newtonsoft.Json;
using TFCTV.Helpers;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class SynapseController : Controller
    {
        //
        // GET: /Synapse/

        //[OutputCache(VaryByParam = "id", Duration = 60)]
        public JsonResult GetMenu(string id)
        {
            List<MyMenu> fullMenu = null;
            var cache = DataCache.Cache;
            string cacheKey = "SYNAPSEGMenu:O:" + id;
            fullMenu = (List<MyMenu>)cache[cacheKey];

            if (fullMenu == null)
            {
                fullMenu = new List<MyMenu>();
                if (String.IsNullOrEmpty(id))
                    id = "Entertainment";
                id = id + "MenuIds";
                string menuId = ConfigurationManager.AppSettings[id];
                var menuids = MyUtility.StringToIntList(menuId);

                var context = new IPTV2Entities();
                var features = context.Features.Where(f => menuids.Contains(f.FeatureId) && f.StatusId == GlobalConfig.Visible).ToList();

                Dictionary<int, Feature> d = features.ToDictionary(x => x.FeatureId);
                List<Feature> ordered = new List<Feature>();
                foreach (var i in menuids)
                {
                    if (d.ContainsKey(i))
                        ordered.Add(d[i]);
                }
                //var ordered = menuids.Select(i => d[i]).ToList();

                foreach (var feature in ordered)
                {
                    var temp = feature.Description.Split('|');

                    var fItems = feature.FeatureItems.Where(f => f.MobileStatusId == GlobalConfig.Visible);
                    if (fItems != null)
                    {
                        var featureItems = fItems.ToList();
                        List<MyMenuShows> mms = new List<MyMenuShows>();
                        foreach (ShowFeatureItem f in featureItems)
                        {
                            if (f is ShowFeatureItem)
                            {
                                Show show = f.Show;
                                MyMenuShows m = new MyMenuShows() { name = show.Description, id = show.CategoryId };
                                mms.Add(m);
                            }
                        }
                        MyMenu item = new MyMenu()
                        {
                            name = temp[0],
                            id = Convert.ToInt32(temp[1]),
                            shows = mms
                        };

                        if (item.shows.Count > 0)
                            fullMenu.Add(item);
                    }
                }
                var cacheDuration = new TimeSpan(0, GlobalConfig.SynapseGenericCacheDuration, 0);
                cache.Put(cacheKey, fullMenu, cacheDuration);
            }
            return Json(fullMenu, JsonRequestBehavior.AllowGet);
        }

        //[OutputCache(VaryByParam = "*", Duration = 60)]
        [HttpHeader("Content-Encoding", "gzip")]
        public JsonResult GetSiteMenu()
        {
            Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
            int[] mainMenuIds = { 736, 738, 737 };
            List<MyMainMenu> siteMenus = null;
            string jsonString = String.Empty;
            var cache = DataCache.Cache;
            string cacheKey = "SYNGETSITEMENU:0:";
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            if (String.IsNullOrEmpty(jsonString))
            {
                siteMenus = new List<MyMainMenu>();
                foreach (int mainMenuId in mainMenuIds)
                {
                    var context = new IPTV2Entities();
                    Category mainCategory = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == mainMenuId);
                    List<MyMenu> fullMenu = new List<MyMenu>();
                    string id = String.Format("{0}MenuIds", mainCategory.Description);
                    string menuId = ConfigurationManager.AppSettings[id];
                    var menuids = MyUtility.StringToIntList(menuId);

                    var features = context.Features.Where(f => menuids.Contains(f.FeatureId) && f.StatusId == GlobalConfig.Visible).ToList();

                    Dictionary<int, Feature> d = features.ToDictionary(x => x.FeatureId);
                    List<Feature> ordered = new List<Feature>();
                    foreach (var i in menuids)
                    {
                        if (d.ContainsKey(i))
                            ordered.Add(d[i]);
                    }
                    //var ordered = menuids.Select(i => d[i]).ToList();

                    foreach (var feature in ordered)
                    {
                        var temp = feature.Description.Split('|');

                        var featureItems = feature.FeatureItems.Where(f => f.MobileStatusId == GlobalConfig.Visible);
                        List<MyMenuShows> mms = new List<MyMenuShows>();
                        foreach (var f in featureItems)
                        {
                            if (f is ShowFeatureItem)
                            {
                                ShowFeatureItem sft = (ShowFeatureItem)f;
                                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == sft.CategoryId);
                                if (category != null)
                                {
                                    if (category is Show)
                                    {
                                        Show show = sft.Show;
                                        string img = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                        MyMenuShows m = new MyMenuShows() { name = show.Description, id = show.CategoryId, image = img };
                                        mms.Add(m);
                                    }
                                }
                            }
                        }

                        MyMenu item = new MyMenu()
                        {
                            name = temp[0],
                            id = Convert.ToInt32(temp[1]),
                            shows = mms
                        };

                        if (item.shows.Count > 0)
                            fullMenu.Add(item);
                    }
                    MyMainMenu siteMenu = new MyMainMenu()
                    {
                        id = mainCategory.CategoryId,
                        name = mainCategory.Description,
                        menu = fullMenu
                    };
                    siteMenus.Add(siteMenu);
                }

                var cacheDuration = new TimeSpan(0, GlobalConfig.MenuCacheDuration, 0);
                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(siteMenus);
                cache.Put(cacheKey, jsonString, cacheDuration);
            }
            else
                siteMenus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MyMainMenu>>(jsonString);
            return Json(siteMenus, JsonRequestBehavior.AllowGet);
        }

        [HttpHeader("Content-Encoding", "gzip")]
        public JsonResult GetShows(int? id)
        {
            Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
            if (id == null)
                id = 0;
            var cache = DataCache.Cache;
            string jsonString = String.Empty;
            SynapseCategory cat = null;
            var countryCode = MyUtility.GetCurrentCountryCodeOrDefault();
            string cacheKey = "SYNAPGTSHOWS:0;C:" + id + ";CC:" + countryCode;
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            if (String.IsNullOrEmpty(jsonString))
            {
                cat = new SynapseCategory();
                var context = new IPTV2Entities();
                var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                var categoryClass = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == (int)id && c.StatusId == GlobalConfig.Visible);
                if (categoryClass != null)
                {
                    if (categoryClass is Category)
                    {
                        Category category = (Category)categoryClass;
                        SortedSet<int> showIds = service.GetAllMobileShowIds(countryCode, category);
                        ViewBag.Category = category.Description;
                        int[] setofShows = showIds.ToArray();
                        var list = context.CategoryClasses.Where(c => setofShows.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible).OrderBy(c => c.CategoryName).ThenBy(c => c.StartDate).ToList();
                        var random = list.OrderBy(x => System.Guid.NewGuid()).FirstOrDefault();
                        cat.id = category.CategoryId;
                        cat.name = category.Description;
                        string featuredImg = String.IsNullOrEmpty(random.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, random.CategoryId.ToString(), random.ImagePoster);
                        string featuredBanner = String.IsNullOrEmpty(random.ImageBanner) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, random.CategoryId.ToString(), random.ImageBanner);
                        SynapseShow feature = new SynapseShow() { id = random.CategoryId, name = random.Description, blurb = random.Blurb, image = featuredImg, banner = featuredBanner, dateairedstr = random.StartDate.Value.ToString("yyyy") };

                        cat.feature = feature;
                        List<SynapseShow> shows = new List<SynapseShow>();

                        foreach (var item in list)
                        {
                            if (item is Show)
                            {
                                string img = String.IsNullOrEmpty(item.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, item.CategoryId.ToString(), item.ImagePoster);
                                string banner = String.IsNullOrEmpty(item.ImageBanner) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, item.CategoryId.ToString(), item.ImageBanner);
                                SynapseShow show = new SynapseShow() { id = item.CategoryId, name = item.Description, blurb = item.Blurb, image = img, banner = banner, dateairedstr = item.StartDate.Value.ToString("yyyy") };
                                shows.Add(show);
                            }
                        }
                        cat.shows = shows;
                    }
                }
                var cacheDuration = new TimeSpan(0, GlobalConfig.SynapseGenericCacheDuration, 0);
                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(cat);
                cache.Put(cacheKey, jsonString, cacheDuration);
            }
            else
                cat = Newtonsoft.Json.JsonConvert.DeserializeObject<SynapseCategory>(jsonString);
            return Json(cat, JsonRequestBehavior.AllowGet);
        }

        //[OutputCache(VaryByParam = "id", Duration = 60)]
        [HttpHeader("Content-Encoding", "gzip")]
        public JsonResult GetShowDetails(int? id, int? status)
        {
            Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
            if (id == null)
                id = 0;

            var context = new IPTV2Entities();
            DateTime registDt = DateTime.Now;
            var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
            var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
            SynapseShow show = null;
            if (category != null)
            {
                if (category is Show)
                {
                    var countryCode = MyUtility.GetCurrentCountryCodeOrDefault();
                    var showT = (Show)category;
                    if (!showT.IsMobileAllowed(countryCode))
                        return this.Json(String.Empty, JsonRequestBehavior.AllowGet);

                    string type = "show";
                    // Check if movie
                    var temp = (Show)category;
                    int parentId = 0;
                    string parent = String.Empty;
                    foreach (var item in temp.ParentCategories.Where(p => p.CategoryId != GlobalConfig.FreeTvCategoryId))
                    {
                        if (item.CategoryClassParentCategories.Where(p => p.ParentId == GlobalConfig.Movies && p.ParentId != GlobalConfig.FreeTvCategoryId).Count() > 0)
                            type = "movie";

                        parentId = item.CategoryId;
                        parent = item.Description;

                    }

                    //IOrderedEnumerable<EpisodeCategory> eList;
                    var episodeCategories = context.EpisodeCategories1.Where(e => e.CategoryId == category.CategoryId && e.Episode.MobileStatusId == GlobalConfig.Visible).Select(e => e.EpisodeId);
                    var episodeList = context.Episodes.Where(e => episodeCategories.Contains(e.EpisodeId) && e.MobileStatusId == GlobalConfig.Visible && e.MobileStartDate < registDt && e.MobileEndDate > registDt)
                        .OrderByDescending(e => e.DateAired);

                    if (episodeList != null)
                    {
                        //var epList = eList.ToList();
                        List<SynapseEpisode> episodes = new List<SynapseEpisode>();
                        foreach (var e in episodeList)
                        {
                            string epImg = String.IsNullOrEmpty(e.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, e.EpisodeId.ToString(), e.ImageAssets.ImageVideo);
                            string EpLength = "";
                            if (!(e.EpisodeLength == null))
                            {
                                TimeSpan span = new TimeSpan(0, 0, Convert.ToInt32(e.EpisodeLength) * 60);
                                EpLength = String.Format("{0}:{1}:{2}", span.Hours.ToString().PadLeft(2, '0'), span.Minutes.ToString().PadLeft(2, '0'), span.Seconds.ToString().PadLeft(2, '0'));
                            }
                            SynapseEpisode episode = new SynapseEpisode() { id = e.EpisodeId, name = e.Description, dateaired = e.DateAired.Value.ToString("MMM d, yyyy"), synopsis = e.Synopsis, image = epImg, episodelength = EpLength, episodenumber = e.EpisodeNumber, expirydate = e.EndDate.Value.ToString("MM/dd/yyyy hh:mm:ss"), oexpirydate = e.OnlineEndDate.Value.ToString("MM/dd/yyyy hh:mm:ss"), mexpirydate = e.MobileEndDate.Value.ToString("MM/dd/yyyy hh:mm:ss"), statusid = e.MobileStatusId };
                            episodes.Add(episode);

                        }

                        string img = String.IsNullOrEmpty(category.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, category.CategoryId.ToString(), category.ImagePoster);
                        string banner = String.IsNullOrEmpty(category.ImageBanner) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, category.CategoryId.ToString(), category.ImageBanner);
                        show = new SynapseShow() { id = category.CategoryId, name = category.Description, blurb = category.Blurb, image = img, banner = banner, type = type, parent = parent, parentId = parentId };
                        show.episodes = episodes;
                    }
                }

                return this.Json(show, JsonRequestBehavior.AllowGet);
            }
            return this.Json(String.Empty, JsonRequestBehavior.AllowGet);
        }

        //[OutputCache(VaryByParam = "id", Duration = 60)]
        [HttpHeader("Content-Encoding", "gzip")]
        public JsonResult GetEpisodeDetails(int? id)
        {
            Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
            if (id == null)
                id = 0;

            var context = new IPTV2Entities();
            DateTime registDt = DateTime.Now;
            var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
            var e = context.Episodes.FirstOrDefault(ep => ep.EpisodeId == id && ep.MobileStatusId == GlobalConfig.Visible && ep.MobileStartDate < registDt && ep.MobileEndDate > registDt);
            SynapseEpisode episode = null;
            if (e != null)
            {
                EpisodeCategory category = e.EpisodeCategories.FirstOrDefault(ec => ec.Episode.MobileStatusId == GlobalConfig.Visible);
                Show show = null;
                if (category != null)
                    show = category.Show;

                string epImg = String.IsNullOrEmpty(e.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, e.EpisodeId.ToString(), e.ImageAssets.ImageVideo);
                episode = new SynapseEpisode() { id = e.EpisodeId, name = e.Description, dateaired = e.DateAired.Value.ToString("MMM d, yyyy"), synopsis = e.Synopsis, image = epImg, show = show.Description };

                return this.Json(episode, JsonRequestBehavior.AllowGet);
            }

            return this.Json(String.Empty, JsonRequestBehavior.AllowGet);
        }

        //[OutputCache(VaryByParam = "id", Duration = 60)]
        [HttpHeader("Content-Encoding", "gzip")]
        public JsonResult GetCelebrityDetails(int? id)
        {
            Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
            if (id != null)
            {
                var context = new IPTV2Entities();
                var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id && c.StatusId == GlobalConfig.Visible);
                if (celebrity != null)
                {
                    string img = String.IsNullOrEmpty(celebrity.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, celebrity.CelebrityId.ToString(), celebrity.ImageUrl);
                    SynapseCelebrity c = new SynapseCelebrity()
                    {
                        id = celebrity.CelebrityId,
                        name = celebrity.FullName,
                        image = img,
                        birthday = celebrity.Birthday,
                        birthplace = celebrity.Birthplace,
                        description = celebrity.Description,
                        height = celebrity.Height,
                        weight = celebrity.Weight
                    };
                    return this.Json(c, JsonRequestBehavior.AllowGet);
                }
            }
            return this.Json(String.Empty, JsonRequestBehavior.AllowGet);
        }

        //[OutputCache(VaryByParam = "*", Duration = 60)]
        [HttpHeader("Content-Encoding", "gzip")]
        public JsonResult GetHomepage()
        {
            Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
            string jsonString = String.Empty;
            SynapseHomepage homepage = null;
            try
            {
                var countryCode = MyUtility.GetCurrentCountryCodeOrDefault();
                var cache = DataCache.Cache;
                string cacheKey = "SYNAPSEGHOMEPAGE:O:;C:" + countryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    homepage = new SynapseHomepage();
                    var context = new IPTV2Entities();
                    List<FeatureItem> featureItems = null;
                    var featuredShows = context.Features.FirstOrDefault(f => f.FeatureId == GlobalConfig.LatestShows && f.StatusId == GlobalConfig.Visible);
                    if (featuredShows != null)
                    {
                        List<SynapseShow> shows = new List<SynapseShow>();
                        var fItems = featuredShows.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible);
                        if (fItems != null)
                        {
                            featureItems = fItems.ToList();
                            foreach (var f in featureItems)
                            {
                                if (f is ShowFeatureItem)
                                {
                                    ShowFeatureItem sft = (ShowFeatureItem)f;
                                    int parentId = 0;
                                    string parent = String.Empty;
                                    Show show = sft.Show;
                                    if (show.IsMobileAllowed(countryCode))
                                    {
                                        foreach (var item in show.ParentCategories.Where(p => p.CategoryId != GlobalConfig.FreeTvCategoryId))
                                        {
                                            parentId = item.CategoryId;
                                            parent = item.Description;
                                        }
                                        string img = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                        string banner = String.IsNullOrEmpty(show.ImageBanner) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImageBanner);
                                        SynapseShow s = new SynapseShow() { id = show.CategoryId, name = show.Description, blurb = show.Blurb, image = img, banner = banner, parent = parent, parentId = parentId };
                                        shows.Add(s);
                                    }
                                }
                            }
                        }
                        homepage.show = shows;
                    }

                    var featuredCelebrities = context.Features.FirstOrDefault(f => f.FeatureId == GlobalConfig.FeaturedCelebrities && f.StatusId == GlobalConfig.Visible);
                    if (featuredCelebrities != null)
                    {
                        List<SynapseCelebrity> celebrities = new List<SynapseCelebrity>();
                        var fCelebItems = featuredCelebrities.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible);
                        if (fCelebItems != null)
                        {
                            featureItems = fCelebItems.OrderByDescending(f => f.FeatureItemId).ToList();
                            foreach (var f in featureItems)
                            {
                                if (f is CelebrityFeatureItem)
                                {
                                    CelebrityFeatureItem cft = (CelebrityFeatureItem)f;
                                    Celebrity person = cft.Celebrity;
                                    string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                                    SynapseCelebrity c = new SynapseCelebrity()
                                    {
                                        id = person.CelebrityId,
                                        name = person.FullName,
                                        image = img
                                    };
                                    celebrities.Add(c);
                                }
                            }
                        }

                        homepage.celebrity = celebrities;
                    }

                    var mainCarousel = GlobalConfig.CarouselEntertainmentId; //hard-coded
                    Carousel carousel = context.Carousels.FirstOrDefault(c => c.CarouselId == mainCarousel && c.StatusId == GlobalConfig.Visible);
                    if (carousel != null)
                    {
                        var fSlides = carousel.CarouselSlides.Where(c => c.MobileStatusId == GlobalConfig.Visible).OrderByDescending(c => c.CarouselSlideId);
                        if (fSlides != null)
                        {
                            List<CarouselSlide> slides = fSlides.ToList();
                            var random = slides.OrderBy(x => System.Guid.NewGuid()).FirstOrDefault();
                            JsonCarouselItem item = new JsonCarouselItem() { CarouselSlideId = random.CarouselSlideId, BannerImageUrl = String.Format("{0}{1}/{2}", GlobalConfig.CarouselImgPath, random.CarouselSlideId.ToString(), random.BannerImageUrl), Blurb = random.Blurb, Name = random.Name, Header = random.Header, TargetUrl = random.TargetUrl, ButtonLabel = random.ButtonLabel };
                            homepage.carousel = item;
                        }
                    }
                    var cacheDuration = new TimeSpan(0, GlobalConfig.SynapseGenericCacheDuration, 0);
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(homepage);
                    cache.Put(cacheKey, jsonString, cacheDuration);
                }
                else
                    homepage = Newtonsoft.Json.JsonConvert.DeserializeObject<SynapseHomepage>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(homepage, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult Login(FormCollection f)
        //public JsonResult Login(string email, string pw)
        //{
        //    SynapseResponse response = new SynapseResponse();
        //    response.callId = MyUtility.GetSHA1((String.Format("{0}{1}", Guid.NewGuid().ToString(), DateTime.Now.ToString()))).ToLower();
        //    try
        //    {
        //        //string EmailAddress = f["email"];
        //        //string Password = f["pw"];
        //        string EmailAddress = email;
        //        string Password = pw;
        //        var context = new IPTV2Entities();
        //        var user = context.Users.FirstOrDefault(u => u.EMail == EmailAddress);
        //        if (user == null)
        //        {
        //            response.errorCode = (int)ErrorCodes.UserDoesNotExist;
        //            response.errorMessage = "User does not exist";
        //        }
        //        else
        //        {
        //            Password = MyUtility.GetSHA1(Password);
        //            if (user.EMail == EmailAddress && String.Compare(user.Password, Password, false) == 0)
        //            {
        //                SynapseUserInfo uInfo = new SynapseUserInfo() { firstName = user.FirstName, lastName = user.LastName, email = user.EMail };
        //                Dictionary<string, object> collection = new Dictionary<string, object>();
        //                collection.Add("siteUID", user.UserId);
        //                collection.Add("cid", "Synapse-Login");
        //                collection.Add("sessionExpiration", 0);
        //                collection.Add("userInfo", JsonConvert.SerializeObject(uInfo));
        //                GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(collection));
        //                //List<SynapseCookie> cookies = new List<SynapseCookie>();
        //                if (res.GetErrorCode() == 0)
        //                { // Successful login to Gigya
        //                    SynapseCookie cookie = new SynapseCookie()
        //                    {
        //                        cookieName = res.GetString("cookieName", String.Empty),
        //                        cookieDomain = res.GetString("cookieDomain", String.Empty),
        //                        cookiePath = res.GetString("cookiePath", String.Empty),
        //                        cookieValue = res.GetString("cookieValue", String.Empty),
        //                        UID = res.GetString("UID", String.Empty),
        //                        UIDSignature = res.GetString("UIDSignature", String.Empty),
        //                        signatureTimestamp = Convert.ToInt32(res.GetString("signatureTimestamp", String.Empty))
        //                    };
        //                    //cookies.Add(cookie);
        //                    //setAuthCookie
        //                    HttpCookie c = SetAutheticationCookie(user.UserId.ToString());
        //                    SynapseCookie tfcCookie = new SynapseCookie()
        //                    {
        //                        cookieName = c.Name,
        //                        cookieDomain = res.GetString("cookieDomain", String.Empty),
        //                        cookiePath = c.Path,
        //                        cookieValue = c.Value
        //                    };
        //                    // cookies.Add(tfcCookie);
        //                    response.errorCode = (int)ErrorCodes.Success;
        //                    response.errorMessage = "Login success";
        //                    response.data = cookie;
        //                    response.info = tfcCookie;
        //                }
        //                else
        //                {
        //                    response.errorCode = res.GetErrorCode();
        //                    response.errorMessage = "Gigya encountered an error logging you in, please try again";
        //                    response.errorDetails = res.GetErrorMessage();
        //                }
        //            }
        //            else
        //            {
        //                response.errorCode = (int)ErrorCodes.IsWrongPassword;
        //                response.errorMessage = MyUtility.getErrorMessage(ErrorCodes.IsWrongPassword);
        //            }
        //        }
        //    }
        //    catch (Exception e) { response.errorCode = (int)ErrorCodes.UnknownError; response.errorMessage = "System encountered an unspecified error, please try again"; response.errorDetails = e.Message; }
        //    return this.Json(response, JsonRequestBehavior.AllowGet);
        //}

        public ContentResult setStatus(string uid, string status)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("UID", uid);
            collection.Add("data", new { status = status });
            //GSResponse res = GigyaHelpers.createAndSendRequest("gcs.setUserData", GigyaHelpers.buildParameter(collection));
            GSResponse res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", GigyaHelpers.buildParameter(collection));
            return Content(res.GetData().ToJsonString(), "application/json");
        }

        public ContentResult logOut(string uid)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("uid", uid);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.logout", GigyaHelpers.buildParameter(collection));
            return Content(res.GetData().ToJsonString(), "application/json");
        }

        public ContentResult getUserInfo(string uid)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("errorCode", (int)ErrorCodes.UserDoesNotExist);
            string userId = "";
            var context = new IPTV2Entities();

            if (User.Identity.IsAuthenticated)
                userId = User.Identity.Name;
            if (!String.IsNullOrEmpty(uid))
                userId = uid;

            User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(userId));
            Dictionary<string, object> gcollection = new Dictionary<string, object>();
            gcollection.Add("uid", userId);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(gcollection));

            return Content(res.GetData().ToJsonString(), "application/json");
        }

        public JsonResult getUserData(string uid)
        {
            UserDataObj obj = new UserDataObj()
            {
                data = new UserDataData()
                {
                    _firstName = String.Empty,
                    _lastName = String.Empty,
                    _city = String.Empty,
                    State = String.Empty,
                    City = String.Empty,
                    CountryCode = String.Empty,
                    Email = String.Empty,
                    _identities = new List<idIdentity>()
                },
                errorCode = -1,
                statusCode = 0,
                statusReason = "No data found"
            };
            try
            {

                Dictionary<string, object> collection = new Dictionary<string, object>();
                try { uid = uid.ToLower(); }
                catch (Exception) { }
                collection.Add("UID", uid);
                collection.Add("include", "profile,data,identities-active");
                //collection.Add("fields", "*");
                //GSResponse res = GigyaHelpers.createAndSendRequest("gcs.getUserData", GigyaHelpers.buildParameter(collection));
                GSResponse res = GigyaHelpers.createAndSendRequest("ids.getAccountInfo", GigyaHelpers.buildParameter(collection));
                var json = res.GetData().ToJsonString();
                idAccountObj idAccount = new idAccountObj() { errorCode = -1 };
                try
                {
                    idAccount = Newtonsoft.Json.JsonConvert.DeserializeObject<idAccountObj>(json);
                }
                catch (Exception) { }
                if (idAccount.errorCode == -1)
                {
                    var context = new IPTV2Entities();
                    var UserId = new Guid(uid);
                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                    if (user != null)
                    {
                        obj.data._firstName = user.FirstName;
                        obj.data._lastName = user.LastName;
                        obj.data._city = user.City;

                        obj.data.FirstName = user.FirstName;
                        obj.data.LastName = user.LastName;

                        obj.data.Email = user.EMail;
                        obj.data.City = user.City;
                        obj.data.State = user.State;
                        obj.data.CountryCode = user.CountryCode;

                        obj.errorCode = 0;
                        obj.statusCode = 200;
                        obj.statusReason = "OK";

                        var ident = new idIdentity()
                        {
                            city = user.City,
                            email = user.EMail,
                            firstName = user.FirstName,
                            lastName = user.LastName,
                            provider = "site"
                        };

                        obj.data._identities.Add(ident);
                    }
                }
                else
                {
                    obj.data._firstName = idAccount.profile.firstName;
                    obj.data._lastName = idAccount.profile.lastName;
                    obj.data._city = idAccount.profile.city;

                    obj.data.Email = idAccount.profile.email;
                    obj.data.City = idAccount.profile.city;
                    obj.data.State = idAccount.profile.state;
                    obj.data.CountryCode = idAccount.profile.country;

                    obj.data.FirstName = idAccount.profile.firstName;
                    obj.data.LastName = idAccount.profile.lastName;

                    obj.errorCode = idAccount.errorCode;
                    obj.statusCode = idAccount.statusCode;
                    obj.statusReason = idAccount.statusReason;
                    obj.callId = idAccount.callId;

                    obj.data._identities = idAccount.identities;
                }
            }
            catch (Exception) { }
            return this.Json(obj, JsonRequestBehavior.AllowGet);
        }

        public ContentResult getUserFeed(string uid, long timestamp, string group, int limit)
        {
            //group = me|friends|everyone
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("uid", uid);
            if (timestamp != 0)
                collection.Add("startTS", timestamp);
            collection.Add("feedID", "UserAction");
            collection.Add("groups", group);
            collection.Add("limit", limit == 0 ? 50 : limit);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getFeed", GigyaHelpers.buildParameter(collection));
            return Content(res.GetData().ToJsonString(), "application/json");
        }

        //[OutputCache(VaryByParam = "q", Duration = 180)]
        ////[HttpHeader("Content-Encoding", "gzip")]
        //public JsonResult Search(string q)
        //{
        //    //Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
        //    UltimateSearchOutput control = new UltimateSearchOutput();
        //    control.AllowPaging = false;

        //    var context = new IPTV2Entities();

        //    var value = control.Find(q);

        //    List<SynapseSearchResult> results = new List<SynapseSearchResult>();
        //    foreach (DataRowView rowView in value)
        //    {
        //        DataRow row = rowView.Row;
        //        SynapseUrlBreakdown breakdown = null;
        //        string image = "";
        //        if (row["url"] != null)
        //        {
        //            string url = row["url"].ToString().Replace(GlobalConfig.baseUrl, "");
        //            Regex rx = new Regex(@"/([\w\d]+)/([\w\d-]+)(/(\d+))*", RegexOptions.IgnoreCase);
        //            MatchCollection matches = rx.Matches(url);

        //            if (matches.Count > 0)
        //            {
        //                Match controller = matches[0];
        //                GroupCollection groups = controller.Groups;
        //                if (groups.Count > 0)
        //                {
        //                    if (!String.IsNullOrEmpty(groups[4].Value))
        //                        breakdown = new SynapseUrlBreakdown()
        //                        {
        //                            controller = groups[1].Value,
        //                            action = groups[2].Value,
        //                            id = groups[4].Value
        //                        };

        //                    else
        //                        breakdown = new SynapseUrlBreakdown()
        //                        {
        //                            controller = groups[1].Value,
        //                            action = String.Empty,
        //                            id = groups[2].Value
        //                        };

        //                    if (!String.IsNullOrEmpty(groups[1].Value))
        //                    {
        //                        int id = 0;
        //                        switch (groups[1].Value.ToLower())
        //                        {
        //                            case "show":
        //                                id = (Convert.ToInt32(groups[4].Value));
        //                                var show = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
        //                                if (show != null)
        //                                    image = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
        //                                break;
        //                            case "episode":
        //                                id = (Convert.ToInt32(groups[4].Value));
        //                                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.MobileStatusId == GlobalConfig.Visible);
        //                                if (episode != null)
        //                                    image = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
        //                                break;
        //                            case "celebrity":
        //                                id = (Convert.ToInt32(groups[4].Value));
        //                                var person = context.Celebrities.FirstOrDefault(c => c.CelebrityId == id && c.StatusId == GlobalConfig.Visible);
        //                                if (person != null)
        //                                    image = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
        //                                break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        results.Add(new SynapseSearchResult()
        //        {
        //            counter = Convert.ToInt32(row["counter"].ToString()),
        //            url = row["url"].ToString(),
        //            controller = breakdown,
        //            title = row["title"].ToString(),
        //            text = row["text"].ToString(),
        //            image = image,
        //            score = Convert.ToInt32(row["score"].ToString()),
        //            lastModified = Convert.ToDateTime(row["lastModified"].ToString()).ToString("MM/dd/yyyy hh:mm:ss tt")
        //        });
        //    }

        //    return this.Json(results, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult BingSearch(string q, string c)
        {
            var bingSiteSearchClient = new BingSiteSearchClient();
            var model = bingSiteSearchClient.RunSearch(q, String.Empty, c);
            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        public ContentResult GetFriends(string uid)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("uid", uid);
            collection.Add("detailLevel", "extended");
            collection.Add("siteUsersOnly", true);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getFriendsInfo", GigyaHelpers.buildParameter(collection));

            return Content(res.GetData().ToJsonString(), "application/json");
        }

        [HttpHeader("Content-Encoding", "gzip")]
        public JsonResult MyContent()
        {
            Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
            List<MyEntitledContentDisplay> display = new List<MyEntitledContentDisplay>();
            if (!GlobalConfig.IsSynapseEnabled)
                return Json(display, JsonRequestBehavior.AllowGet);
            if (MyUtility.isUserLoggedIn())
            {
                DateTime registDt = DateTime.Now;
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.StatusId == GlobalConfig.Visible);
                var entitlements = context.Entitlements.Where(e => e.UserId == userId && e.OfferingId == service.OfferingId && e.EndDate > registDt);
                foreach (var item in entitlements)
                {
                    if (item is PackageEntitlement)
                    {
                        PackageEntitlement packageEntitlement = (PackageEntitlement)item;
                        var categories = packageEntitlement.Package.Categories;
                        if (categories.Count != 0)
                        {
                            foreach (var cat in categories)
                            {
                                string type = cat.Category.CategoryId == GlobalConfig.moviesPremiumCategoryId || cat.Category.CategoryId == GlobalConfig.Movies ? "movie" : "show";
                                int[] listOfShowIds = service.GetAllMobileShowIds(MyUtility.GetCurrentCountryCodeOrDefault(), cat.Category).ToArray();
                                var shows = context.CategoryClasses.Where(c => listOfShowIds.Contains(c.CategoryId));
                                foreach (var s in shows)
                                {
                                    if (s is Show)
                                    {
                                        Show show = (Show)s;

                                        var showParent = show.CategoryClassParentCategories.Last();
                                        string parent = "";
                                        if (showParent != null)
                                            parent = showParent.ParentCategory.Description;
                                        display.Add(new MyEntitledContentDisplay()
                                        {
                                            id = show.CategoryId,
                                            title = show.Description,
                                            blurb = show.Blurb,
                                            type = type,
                                            parent = parent,
                                            image = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster),
                                            ExpiryDate = item.EndDate.ToString("yyyy-MM-ddThh:mm:ss")
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else if (item is ShowEntitlement)
                    {
                        ShowEntitlement showEntitlement = (ShowEntitlement)item;
                        var show = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == showEntitlement.CategoryId);
                        if (show != null && show is Show)
                        {
                            string type = "show";
                            string parent = "";
                            var showParent = show.CategoryClassParentCategories.Last();
                            if (showParent != null)
                            {
                                if (showParent.CategoryId == GlobalConfig.moviesPremiumCategoryId || showParent.CategoryId == GlobalConfig.Movies) type = "movie";
                                parent = showParent.ParentCategory.Description;
                            }

                            display.Add(new MyEntitledContentDisplay()
                                {
                                    id = show.CategoryId,
                                    title = show.Description,
                                    blurb = show.Blurb,
                                    type = type,
                                    parent = parent,
                                    image = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster),
                                    ExpiryDate = item.EndDate.ToString("yyyy-MM-ddThh:mm:ss")
                                });
                        }
                    }
                    else if (item is EpisodeEntitlement)
                    {
                        EpisodeEntitlement episodeEntitlement = (EpisodeEntitlement)item;
                        var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == episodeEntitlement.EpisodeId);
                        var episodeParent = episode.EpisodeCategories.First();
                        string parent = "";
                        if (episodeParent != null)
                            parent = episodeParent.Show.Description;
                        if (episode != null)
                        {
                            string type = "episode";
                            display.Add(new MyEntitledContentDisplay()
                            {
                                id = episode.EpisodeId,
                                title = episode.Description,
                                blurb = episode.Synopsis,
                                type = type,
                                parent = parent,
                                image = String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId, episode.ImageAssets.ImageVideo),
                                ExpiryDate = item.EndDate.ToString("yyyy-MM-ddThh:mm:ss")
                            });
                        }
                    }
                }
            }

            return this.Json(display, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MyVideos()
        {
            if (!GlobalConfig.IsSynapseEnabled)
                return Json(null, JsonRequestBehavior.AllowGet);

            DateTime registDt = DateTime.Now;
            List<EntitlementDisplay> display = new List<EntitlementDisplay>();
            if (User.Identity.IsAuthenticated)
            {
                System.Guid userId = new System.Guid(User.Identity.Name);
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    var entitlements = context.Entitlements.Where(t => t.UserId == user.UserId && t.OfferingId == GlobalConfig.offeringId && t.EndDate > registDt).OrderByDescending(t => t.EndDate).ToList();
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId && s.StatusId == GlobalConfig.Visible);
                    SortedSet<int> mobileShowIds = new SortedSet<int>();
                    try
                    {
                        mobileShowIds = service.GetAllMobileShowIds(user.CountryCode);
                    }
                    catch (Exception) { }

                    foreach (Entitlement entitlement in entitlements)
                    {
                        EntitlementDisplay disp = new EntitlementDisplay();

                        disp.EntitlementId = entitlement.EntitlementId;
                        disp.ExpiryDate = entitlement.EndDate;
                        disp.ExpiryDateStr = entitlement.EndDate.ToString("yyyy-MM-ddThh:mm:ss");
                        //disp.ExpiryDateStr = entitlement.EndDate.ToString("yyyy-MM-dd");
                        if (entitlement is PackageEntitlement)
                        {
                            var pkg = (PackageEntitlement)entitlement;
                            disp.PackageId = pkg.PackageId;
                            disp.PackageName = pkg.Package.Description;
                            disp.Content = disp.PackageName;
                        }
                        else if (entitlement is ShowEntitlement)
                        {
                            var show = (ShowEntitlement)entitlement;
                            if (mobileShowIds.Contains(show.CategoryId))
                            {
                                if (!(show.Show is LiveEvent))
                                {
                                    disp.CategoryId = show.CategoryId;
                                    disp.CategoryName = show.Show.Description;
                                    disp.Content = disp.CategoryName;
                                }
                            }
                        }
                        else if (entitlement is EpisodeEntitlement)
                        {
                            var episode = (EpisodeEntitlement)entitlement;
                            disp.EpisodeId = episode.EpisodeId;
                            disp.EpisodeName = episode.Episode.Description + ", " + episode.Episode.DateAired.Value.ToString("MMM d, yyyy");
                            disp.Content = disp.EpisodeName;
                        }

                        display.Add(disp);
                    }
                }
            }
            return this.Json(display, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MyTransactions()
        {
            List<TransactionDisplay> display = new List<TransactionDisplay>();
            if (User.Identity.IsAuthenticated)
            {
                System.Guid userId = new System.Guid(User.Identity.Name);
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    var transactions = context.Transactions.Where(t => t.UserId == user.UserId && t.OfferingId == GlobalConfig.offeringId).OrderByDescending(t => t.TransactionId).ToList();

                    foreach (Transaction transaction in transactions)
                    {
                        TransactionDisplay disp = new TransactionDisplay();

                        disp.TransactionId = transaction.TransactionId;
                        disp.Reference = transaction.Reference;
                        disp.Amount = transaction.Amount;
                        disp.Currency = transaction.Currency;
                        disp.TransactionDate = transaction.Date;

                        //TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(transaction.Date);
                        //var result = string.Format("{0:yyyy-dd-MM}T{1}:{2}:00",
                        //                                     transaction.Date,
                        //                                     utcOffset.Hours + 12,
                        //                                     utcOffset.Minutes);
                        disp.TransactionDateStr = transaction.Date.ToString("yyyy-MM-ddThh:mm:ss");
                        //disp.TransactionDateStr = transaction.Date.ToString("yyyy-MM-dd");

                        //if (transaction is PaymentTransaction)
                        //{
                        //    PaymentTransaction ptrans = (PaymentTransaction)transaction;

                        //    string remarks = ptrans.Purchase.Remarks;
                        //    bool purchase = String.IsNullOrEmpty(remarks) ? false : remarks.StartsWith("Gift");
                        //    disp.TransactionType = purchase ? "Gift" : "Subscription";
                        //    PurchaseItem item = ptrans.Purchase.PurchaseItems.FirstOrDefault();
                        //    disp.ProductId = item.ProductId;
                        //    Product product = context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                        //    if (product != null)
                        //        disp.ProductName = product.Name;
                        //    if (ptrans is PpcPaymentTransaction)
                        //    {
                        //        disp.PpcId = ptrans.Reference;
                        //        disp.PaymentType = "Prepaid Card";
                        //    }
                        //    else if (ptrans is PaypalPaymentTransaction)
                        //        disp.PaymentType = "PayPal";
                        //    else if (ptrans is CreditCardPaymentTransaction)
                        //        disp.PaymentType = "Credit Card";
                        //    else if (ptrans is WalletPaymentTransaction)
                        //        disp.PaymentType = "Wallet";
                        //    disp.Method = disp.PaymentType;
                        //}
                        //else if (transaction is UpgradeTransaction)
                        //{
                        //    UpgradeTransaction utrans = (UpgradeTransaction)transaction;

                        //    disp.TransactionType = "Upgrade";
                        //    Product product = context.Products.FirstOrDefault(p => p.ProductId == utrans.OriginalProductId);
                        //    if (product != null)
                        //        disp.ProductName = product.Name;
                        //    Product oldProduct = context.Products.FirstOrDefault(p => p.ProductId == utrans.NewProductId);
                        //    disp.Reference = String.Format("Upgraded to {0}", oldProduct.Name);
                        //    disp.Method = "N/A";
                        //}

                        //else if (transaction is ChangeCountryTransaction)
                        //{
                        //    ChangeCountryTransaction ctrans = (ChangeCountryTransaction)transaction;
                        //    disp.TransactionType = "Change Country";
                        //    disp.ProductName = "N/A";
                        //    disp.Method = "N/A";
                        //    disp.Reference = String.Format("Change Country from {0} to {1}", ctrans.OldCountryCode, ctrans.NewCountryCode);
                        //}
                        //else
                        //{
                        //    ReloadTransaction rtrans = (ReloadTransaction)transaction;
                        //    disp.ProductName = "N/A";
                        //    disp.TransactionType = "Reload";
                        //    if (rtrans is PpcReloadTransaction)
                        //    {
                        //        disp.PpcId = rtrans.Reference;
                        //        disp.ReloadType = "Prepaid Card";
                        //    }
                        //    else if (rtrans is PaypalReloadTransaction)
                        //        disp.ReloadType = "PayPal";
                        //    else if (rtrans is CreditCardReloadTransaction)
                        //        disp.ReloadType = "Credit Card";

                        //    disp.Method = disp.ReloadType;
                        //}
                        if (transaction is PaymentTransaction)
                        {
                            PaymentTransaction ptrans = (PaymentTransaction)transaction;

                            string remarks = ptrans.Purchase.Remarks;
                            bool purchase = String.IsNullOrEmpty(remarks) ? false : remarks.StartsWith("Gift");
                            disp.TransactionType = purchase ? "Gift" : "Subscription";
                            PurchaseItem item = ptrans.Purchase.PurchaseItems.FirstOrDefault();
                            disp.ProductId = item.ProductId;
                            Product product = context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                            if (product != null)
                                disp.ProductName = product.Name;
                            if (ptrans is PpcPaymentTransaction)
                            {
                                disp.PpcId = ptrans.Reference;
                                disp.PaymentType = "Prepaid Card/ePIN";
                            }
                            else if (ptrans is PaypalPaymentTransaction)
                                disp.PaymentType = "Paypal";
                            else if (ptrans is CreditCardPaymentTransaction)
                                disp.PaymentType = "Credit Card";
                            else if (ptrans is WalletPaymentTransaction)
                                disp.PaymentType = "Wallet";
                            disp.Method = disp.PaymentType;
                        }
                        else if (transaction is UpgradeTransaction)
                        {
                            UpgradeTransaction utrans = (UpgradeTransaction)transaction;

                            disp.TransactionType = "Upgrade";
                            Product product = context.Products.FirstOrDefault(p => p.ProductId == utrans.OriginalProductId);
                            if (product != null)
                                disp.ProductName = product.Name;
                            Product oldProduct = context.Products.FirstOrDefault(p => p.ProductId == utrans.NewProductId);
                            disp.Reference = String.Format("Upgraded to {0}", oldProduct.Name);
                            disp.Method = "N/A";
                        }
                        else if (transaction is TfcEverywhereTransaction)
                        {
                            TfcEverywhereTransaction ttrans = (TfcEverywhereTransaction)transaction;
                            disp.TransactionType = "Platinum";
                            disp.ProductName = "TFC.tv Premium";
                            disp.Method = "N/A";
                            disp.Amount = 0;

                        }
                        else if (transaction is ChangeCountryTransaction)
                        {
                            ChangeCountryTransaction ctrans = (ChangeCountryTransaction)transaction;
                            disp.TransactionType = "Change Country";
                            disp.ProductName = "N/A";
                            disp.Method = "N/A";
                            disp.Reference = String.Format("Change Country from {0} to {1}", ctrans.OldCountryCode, ctrans.NewCountryCode);
                        }
                        else if (transaction is MigrationTransaction)
                        {
                            MigrationTransaction mtrans = (MigrationTransaction)transaction;
                            disp.TransactionType = "Migrate Licenses";
                            disp.ProductName = "N/A";
                            if (mtrans.MigratedProductId > 0)
                            {
                                Product product = context.Products.FirstOrDefault(p => p.ProductId == mtrans.MigratedProductId);
                                if (product != null)
                                    disp.ProductName = product.Name;
                            }
                            disp.Method = "N/A";
                            disp.Reference = mtrans.Reference;
                        }
                        else if (transaction is RegistrationTransaction)
                        {
                            RegistrationTransaction rgtrans = (RegistrationTransaction)transaction;
                            disp.TransactionType = "Registration";
                            disp.ProductName = "N/A";
                            disp.Method = "N/A";
                        }
                        else if (transaction is ReloadTransaction)
                        {
                            ReloadTransaction rtrans = (ReloadTransaction)transaction;
                            disp.ProductName = "N/A";
                            disp.TransactionType = "Reload";
                            if (rtrans is PpcReloadTransaction)
                            {
                                disp.PpcId = rtrans.Reference;
                                disp.ReloadType = "Prepaid Card/ePIN";
                            }
                            else if (rtrans is PaypalReloadTransaction)
                                disp.ReloadType = "Paypal";
                            else if (rtrans is CreditCardReloadTransaction)
                                disp.ReloadType = "Credit Card";
                            else if (rtrans is SmartPitReloadTransaction)
                                disp.ReloadType = "Smart Pit";

                            disp.Method = disp.ReloadType;
                        }
                        display.Add(disp);
                    }
                }
            }
            return this.Json(display, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMostLovedShows()
        {
            try
            {
                var socialContext = new EngagementsEntities();
                var context = new IPTV2Entities();

                var mostLovedShowReactions = socialContext.ShowReactions
                    .Where(c => c.ReactionTypeId == 12)
                    .GroupBy(c => c.CategoryId)
                    .Select(c => new
                    {
                        CategoryId = c.Key,
                        TotalLoved = c.Count()
                    }).ToList().OrderByDescending(c => c.TotalLoved).Take(5);

                var mostLovedShows = mostLovedShowReactions.Join(
                        context.CategoryClasses,
                        sr => sr.CategoryId,
                        cc => cc.CategoryId,
                        (sr, cc) => new { ShowReaction = sr, CategoryClasses = cc })
                        .Select(show => new
                        {
                            categoryName = show.CategoryClasses.CategoryName,
                            categoryId = show.CategoryClasses.CategoryId,
                            totalLove = show.ShowReaction.TotalLoved,
                            image = String.IsNullOrEmpty(show.CategoryClasses.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryClasses.CategoryId.ToString(), show.CategoryClasses.ImagePoster),
                            blurb = show.CategoryClasses.Blurb
                        });

                if (mostLovedShows != null)
                    return Json(mostLovedShows, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return null;
        }

        public JsonResult GetMostLovedEpisodes()
        {
            try
            {
                var socialContext = new EngagementsEntities();
                var context = new IPTV2Entities();

                var mostLovedEpisodeReactions = socialContext.EpisodeReactions
                    .Where(rtId => rtId.ReactionTypeId == 12)
                    .GroupBy(e => e.EpisodeId)
                    .Select(e => new
                    {
                        EpisodeId = e.Key,
                        TotalLoved = e.Count()
                    }).ToList().OrderByDescending(e => e.TotalLoved).Take(5);

                var mostLovedEpisodes = mostLovedEpisodeReactions.Join(
                        context.Episodes,
                        er => er.EpisodeId,
                        ee => ee.EpisodeId,
                        (er, ee) => new { EpisodeReactions = er, Episodes = ee })
                        .Select(episode => new
                        {
                            showId = episode.Episodes.EpisodeCategories.FirstOrDefault().Show.CategoryId,
                            showName = episode.Episodes.EpisodeCategories.FirstOrDefault().Show.Description,
                            dateAired = episode.Episodes.DateAired.Value.ToString("MMM d, yyyy"),
                            episodeName = episode.Episodes.Description,
                            episodeId = episode.Episodes.EpisodeId,
                            totalLove = episode.EpisodeReactions.TotalLoved,
                            image = String.IsNullOrEmpty(episode.Episodes.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.Episodes.EpisodeId.ToString(), episode.Episodes.ImageAssets.ImageVideo),
                            blurb = episode.Episodes.Synopsis
                        });

                if (mostLovedEpisodes != null)
                    return Json(mostLovedEpisodes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return null;
        }

        public JsonResult GetMostLovedCelebrities()
        {
            try
            {
                var socialContext = new EngagementsEntities();
                var context = new IPTV2Entities();

                var mostLovedCelebrityReactions = socialContext.CelebrityReactions
                    .Where(rtId => rtId.ReactionTypeId == 12)
                    .GroupBy(celeb => celeb.CelebrityId)
                    .Select(celeb => new
                    {
                        celebrityId = celeb.Key,
                        totalLoved = celeb.Count()
                    }).ToList().OrderByDescending(celeb => celeb.totalLoved).Take(5);

                var mostLovedCelebrities = mostLovedCelebrityReactions.Join(
                    context.Celebrities,
                    cr => cr.celebrityId,
                    cc => cc.CelebrityId,
                    (cr, cc) => new { CelebrityReaction = cr, Celebrity = cc })
                    .Select(celebrity => new
                    {
                        celebrityName = celebrity.Celebrity.FullName,
                        celebrityId = celebrity.Celebrity.CelebrityId,
                        totalLove = celebrity.CelebrityReaction.totalLoved,
                        image = String.IsNullOrEmpty(celebrity.Celebrity.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, celebrity.Celebrity.CelebrityId.ToString(), celebrity.Celebrity.ImageUrl),
                        description = celebrity.Celebrity.Description
                    });

                if (mostLovedCelebrities != null)
                    return Json(mostLovedCelebrities, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return null;
        }

        public ContentResult getComments(string categoryID, string streamID, int? limit, int? depth, string sort)
        {
            if (String.IsNullOrEmpty(sort))
                sort = "dateDesc"; //default

            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("categoryID", categoryID);
            collection.Add("streamID", streamID);
            if (limit != null)
                if (limit > 0)
                    collection.Add("threadLimit", limit);
            if (depth != null)
                if (depth > 0)
                    collection.Add("threadDepth", depth);
            collection.Add("sort", sort);
            collection.Add("includeUID", true);
            GSResponse res = GigyaHelpers.createAndSendRequest("comments.getComments", GigyaHelpers.buildParameter(collection));

            return Content(res.GetData().ToJsonString(), "application/json");
        }

        //[HttpPost]
        public ContentResult PostComment(string uid, string categoryID, string streamID, string comment, string parentID, string title, int? value)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("uid", uid);
            collection.Add("categoryID", categoryID);
            collection.Add("streamID", streamID);
            collection.Add("commentText", Uri.EscapeDataString(comment));
            if (!String.IsNullOrEmpty(parentID))
                collection.Add("parentID", parentID);
            collection.Add("cid", "Comment");
            if (!(String.IsNullOrEmpty(title)) && value != null)
            {
                if (value > 0 && value <= 5)
                {
                    collection.Add("commentTitle", title);
                    Dictionary<string, object> ratings = new Dictionary<string, object>();
                    ratings.Add("_overall", value);
                    collection.Add("ratings", ratings);
                    collection.Remove("parentID");
                    collection["cid"] = "Rating & Reviews";
                }
            }
            GSResponse res = GigyaHelpers.createAndSendRequest("comments.postComment", GigyaHelpers.buildParameter(collection));
            return Content(res.GetData().ToJsonString(), "application/json");
        }

        [HttpPost]
        public ContentResult flagComment(string uid, string commentID, string categoryID, string streamID)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("uid", uid);
            collection.Add("commentID", commentID);
            collection.Add("categoryID", categoryID);
            collection.Add("streamID", streamID);
            GSResponse res = GigyaHelpers.createAndSendRequest("comments.flagComment", GigyaHelpers.buildParameter(collection));
            return Content(res.GetData().ToJsonString(), "application/json");
        }

        [HttpPost]
        public ContentResult voteComment(string uid, string commentID, string categoryID, string streamID, string vote)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("uid", uid);
            collection.Add("commentID", commentID);
            collection.Add("categoryID", categoryID);
            collection.Add("streamID", streamID);
            collection.Add("vote", vote);
            GSResponse res = GigyaHelpers.createAndSendRequest("comments.vote", GigyaHelpers.buildParameter(collection));
            return Content(res.GetData().ToJsonString(), "application/json");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult getVideo(int? id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);

            var context = new IPTV2Entities();
            Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.MobileStatusId == GlobalConfig.Visible);
            if (ep != null)
            {
                Asset asset = ep.PremiumAssets.FirstOrDefault().Asset;
                if (asset != null)
                {
                    //if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
                    //{
                    //    if (User.Identity.IsAuthenticated)
                    //    {
                    //        HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    //        var UserId = new Guid(User.Identity.Name);
                    //        var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                    //        if (user != null)
                    //        {                                
                    //            if (!MyUtility.IsWhiteListed(user.EMail))
                    //            {
                    //                if (!String.IsNullOrEmpty(user.SessionId))
                    //                {
                    //                    if (String.Compare(user.SessionId, authCookie.Value, true) != 0)
                    //                    {
                    //                        collection = MyUtility.setError(ErrorCodes.MultipleLoginDetected, "Multiple login detected");
                    //                        collection.Add("uid", User.Identity.Name);
                    //                        //FormsAuthentication.SignOut();
                    //                        return Content(MyUtility.buildJson(collection), "application/json");
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    int assetId = asset == null ? 0 : asset.AssetId;
                    ViewBag.AssetId = assetId;
                    bool HasHD = ContextHelper.DoesEpisodeHaveAkamaiHDCdnReferenceBasedOnAsset(ep);
                    AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                    if (HasHD)
                        clipDetails = Helpers.Akamai.GetAkamaiClipDetailsHD(ep.EpisodeId, assetId, Request, User);
                    else
                        clipDetails = Helpers.Akamai.GetAkamaiClipDetails(ep.EpisodeId, assetId, Request, User);

                    if (!String.IsNullOrEmpty(clipDetails.Url))
                    {
                        errorCode = ErrorCodes.Success;
                        collection = MyUtility.setError(errorCode, clipDetails.Url);

                        if (!MyUtility.isUserLoggedIn())
                        {
                            clipDetails.Url = String.Empty;
                            errorCode = ErrorCodes.NotAuthenticated;
                            collection = MyUtility.setError(errorCode, "Not Authenticated");
                        }

                        if (clipDetails.PromptToSubscribe == true)
                        {
                            clipDetails.Url = String.Empty;
                            errorCode = ErrorCodes.UserIsNotEntitled;
                            collection = MyUtility.setError(errorCode, "No Subscription");
                        }

                        collection.Add("data", clipDetails);
                    }
                    else
                    {
                        errorCode = ErrorCodes.AkamaiCdnNotFound;
                        collection = MyUtility.setError(errorCode, "Akamai Url not found.");
                        if (Akamai.IsIos(Request))
                        {
                            errorCode = ErrorCodes.UserIsNotEntitled;
                            collection = MyUtility.setError(errorCode, "No Subscription");
                        }
                    }
                }
                else
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "Video not found.");
                }
            }
            else
            {
                errorCode = ErrorCodes.EpisodeNotFound;
                collection = MyUtility.setError(errorCode, "Episode not found.");
            }

            return Content(MyUtility.buildJson(collection), "application/json");
        }

        //public ActionResult Login()
        //{
        //    /**rbr**/
        //    //This will handle request using i.e. browser.
        //    var retUrl = Uri.UnescapeDataString(Request.QueryString["returl"]);

        //    NameValueCollection qs = Request.QueryString;
        //    string gigyaUID = Uri.UnescapeDataString(qs["UID"]);
        //    bool isRequestValid = SigUtils.ValidateUserSignature(gigyaUID, Uri.UnescapeDataString(qs["timestamp"]), GlobalConfig.GSsecretkey, qs["signature"]);
        //    if (isRequestValid)
        //    {
        //        TempData["gid"] = gigyaUID;
        //        var isNewUser = !Convert.ToBoolean(qs["isSiteUID"]); // If isSiteUID=='false' , this means the UID was generated by Gigya, hence the user is new. !(false) == true
        //        if (isNewUser)
        //        {
        //            TempData["qs"] = qs; //bring the parameters to the next view
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            //FormsAuthentication.SetAuthCookie(gigyaUID, true); //Authenticate user to our site.
        //            SetAutheticationCookie(gigyaUID);
        //            var context = new IPTV2Entities();
        //            User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(gigyaUID));
        //            /**rbr**/
        //            var referrerUrl = "";
        //            try
        //            {
        //                referrerUrl = Request.UrlReferrer.ToString();
        //            }
        //            catch (Exception error)
        //            {
        //                //i.e browser may throw an exception
        //                Debug.WriteLine(error.Message);
        //                referrerUrl = retUrl;
        //            }
        //            return Redirect(referrerUrl); //goes back to the same page where user logs in.
        //        }
        //    }
        //    return RedirectToAction("Index", "Home");
        //}

        public ActionResult Socialize()
        {
            NameValueCollection qs = Request.QueryString;
            string value = "";
            int i = 0;
            foreach (var item in qs)
            {
                value += qs[i].ToString();
                i++;
            }
            if (String.IsNullOrEmpty(value))
                value = "Fragment: " + Request.Url.AbsoluteUri;
            return Content(value, "text/plain");
        }

        public ActionResult GenerateSig(string method, string url, string qs)
        {
            GSObject dictionaryParams = new GSObject();
            dictionaryParams.ParseQuerystring(qs);
            string baseString = SigUtils.CalcOAuth1Basestring(method, url, dictionaryParams);
            return Content(SigUtils.CalcSignature(baseString, GlobalConfig.GSsecretkey), "text/plain");
        }

        public virtual HttpCookie SetAutheticationCookie(string uid)
        {
            //FormsAuthentication.SetAuthCookie(uid, true);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(uid, true, GlobalConfig.FormsAuthenticationTimeout);
            //FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, uid, DateTime.Now, DateTime.Now.AddMonths(1), true, String.Empty, FormsAuthentication.FormsCookiePath);
            string encTicket = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.Expires = DateTime.Now.AddMonths(1);
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
            return cookie;
        }

        public JsonResult Login(string email, string pw)
        {
            SynapseResponse response = new SynapseResponse();
            response.callId = MyUtility.GetSHA1((String.Format("{0}{1}", Guid.NewGuid().ToString(), DateTime.Now.ToString()))).ToLower();
            try
            {
                string EmailAddress = email;
                string Password = pw;
                var context = new IPTV2Entities();

                if (String.IsNullOrEmpty(EmailAddress))
                {
                    response.errorCode = (int)ErrorCodes.IsMissingRequiredFields;
                    response.errorMessage = "Email address is required.";
                }

                if (String.IsNullOrEmpty(Password))
                {
                    response.errorCode = (int)ErrorCodes.IsMissingRequiredFields;
                    response.errorMessage = "Password is required.";
                }

                var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                if (user == null)
                {
                    response.errorCode = (int)ErrorCodes.UserDoesNotExist;
                    response.errorMessage = "User does not exist";
                }
                else if (user.StatusId == 0 || user.StatusId == null)
                {
                    response.errorCode = (int)ErrorCodes.IsNotVerified;
                    response.errorMessage = "User is not verified";
                }
                else
                {
                    Password = MyUtility.GetSHA1(Password);
                    if (String.Compare(user.EMail, EmailAddress, true) == 0 && String.Compare(user.Password, Password, false) == 0)
                    {
                        SynapseUserInfo uInfo = new SynapseUserInfo() { firstName = user.FirstName, lastName = user.LastName, email = user.EMail };
                        Dictionary<string, object> collection = new Dictionary<string, object>();
                        collection.Add("client_id", GlobalConfig.GSapikey);
                        collection.Add("client_secret", GlobalConfig.GSsecretkey);
                        collection.Add("grant_type", "none");
                        collection.Add("x_siteUID", user.UserId);
                        collection.Add("x_sessionExpiration", 0);
                        collection.Add("x_userInfo", JsonConvert.SerializeObject(uInfo));
                        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getToken", GigyaHelpers.buildParameter(collection));
                        SynapseCookie cookie = new SynapseCookie()
                        {
                            cookieName = FormsAuthentication.FormsCookieName,
                            cookiePath = FormsAuthentication.FormsCookiePath,
                            cookieDomain = FormsAuthentication.CookieDomain
                        };
                        if (res.GetErrorCode() == 0)
                        { // Successful login to Gigya
                            HttpCookie authCookie = SetAutheticationCookie(user.UserId.ToString());
                            cookie.cookieValue = authCookie.Value;
                            ContextHelper.SaveSessionInDatabase(context, user, authCookie.Value);
                            SynapseToken token = new SynapseToken()
                            {
                                uid = user.UserId.ToString(),
                                token = res.GetString("access_token", String.Empty),
                                expire = res.GetInt("expires_in", 0),
                            };
                            response.data = token;
                            response.info = cookie;
                        }
                        else
                        {
                            response.errorCode = res.GetErrorCode();
                            response.errorMessage = "Gigya encountered an error logging you in, please try again";
                            response.errorDetails = res.GetErrorMessage();
                        }
                    }
                    else
                    {
                        response.errorCode = (int)ErrorCodes.IsWrongPassword;
                        response.errorMessage = MyUtility.getErrorMessage(ErrorCodes.IsWrongPassword);
                    }
                }
            }
            catch (Exception e) { response.errorCode = (int)ErrorCodes.UnknownError; response.errorMessage = "System encountered an unspecified error, please try again"; response.errorDetails = e.Message; }
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult vLogin(string email, string pw)
        {
            SynapseResponse response = new SynapseResponse();
            response.callId = MyUtility.GetSHA1((String.Format("{0}{1}", Guid.NewGuid().ToString(), DateTime.Now.ToString()))).ToLower();
            try
            {
                string EmailAddress = email;
                string Password = pw;
                var context = new IPTV2Entities();

                if (String.IsNullOrEmpty(EmailAddress))
                {
                    response.errorCode = (int)ErrorCodes.IsMissingRequiredFields;
                    response.errorMessage = "Email address is required.";
                }

                if (String.IsNullOrEmpty(Password))
                {
                    response.errorCode = (int)ErrorCodes.IsMissingRequiredFields;
                    response.errorMessage = "Password is required.";
                }

                var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                if (user == null)
                {
                    response.errorCode = (int)ErrorCodes.UserDoesNotExist;
                    response.errorMessage = "User does not exist";
                }
                else
                {
                    Password = MyUtility.GetSHA1(Password);
                    if (String.Compare(user.EMail, EmailAddress, true) == 0 && String.Compare(user.Password, Password, false) == 0)
                    {
                        SynapseUserInfo uInfo = new SynapseUserInfo() { firstName = user.FirstName, lastName = user.LastName, email = user.EMail };
                        Dictionary<string, object> collection = new Dictionary<string, object>();
                        collection.Add("client_id", GlobalConfig.GSapikey);
                        collection.Add("client_secret", GlobalConfig.GSsecretkey);
                        collection.Add("grant_type", "none");
                        collection.Add("x_siteUID", user.UserId);
                        collection.Add("x_sessionExpiration", 0);
                        collection.Add("x_userInfo", JsonConvert.SerializeObject(uInfo));
                        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getToken", GigyaHelpers.buildParameter(collection));
                        SynapseCookie cookie = new SynapseCookie()
                        {
                            cookieName = FormsAuthentication.FormsCookieName,
                            cookiePath = FormsAuthentication.FormsCookiePath,
                            cookieDomain = FormsAuthentication.CookieDomain
                        };
                        if (res.GetErrorCode() == 0)
                        { // Successful login to Gigya
                            HttpCookie authCookie = SetAutheticationCookie(user.UserId.ToString());
                            cookie.cookieValue = authCookie.Value;
                            ContextHelper.SaveSessionInDatabase(context, user, authCookie.Value);
                            SynapseToken token = new SynapseToken()
                            {
                                uid = user.UserId.ToString(),
                                token = res.GetString("access_token", String.Empty),
                                expire = res.GetInt("expires_in", 0),
                            };
                            response.data = token;
                            response.info = cookie;
                        }
                        else
                        {
                            response.errorCode = res.GetErrorCode();
                            response.errorMessage = "Gigya encountered an error logging you in, please try again";
                            response.errorDetails = res.GetErrorMessage();
                        }
                    }
                    else
                    {
                        response.errorCode = (int)ErrorCodes.IsWrongPassword;
                        response.errorMessage = MyUtility.getErrorMessage(ErrorCodes.IsWrongPassword);
                    }
                }
            }
            catch (Exception e) { response.errorCode = (int)ErrorCodes.UnknownError; response.errorMessage = "System encountered an unspecified error, please try again"; response.errorDetails = e.Message; }
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Description: Gets the list of episodes to be used for the FeatureSlides (Latest Full Episodes, Most Viewed, Free TV)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[OutputCache(VaryByParam = "id", Duration = 180)]
        public JsonResult GetCarousel(int id)
        {
            var context = new IPTV2Entities();
            var feature = context.Features.FirstOrDefault(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
            if (feature == null)
                return this.Json(string.Empty, JsonRequestBehavior.AllowGet);

            var fItems = context.FeatureItems.Where(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
            List<JsonFeatureItem> jfi = new List<JsonFeatureItem>();
            if (fItems != null)
            {
                List<FeatureItem> featureItems = fItems.ToList();

                foreach (EpisodeFeatureItem f in featureItems)
                {
                    if (f is EpisodeFeatureItem)
                    {
                        Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == f.EpisodeId && e.MobileStatusId == GlobalConfig.Visible);
                        if (ep != null)
                        {
                            Show show = ep.EpisodeCategories.FirstOrDefault().Show;
                            if (show != null)
                            {
                                int parentId = 0;
                                string parent = String.Empty;
                                foreach (var item in show.ParentCategories.Where(p => p.CategoryId != GlobalConfig.FreeTvCategoryId))
                                {
                                    parent = item.Description;
                                    parentId = item.CategoryId;

                                }
                                string img = String.IsNullOrEmpty(ep.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ep.EpisodeId.ToString(), ep.ImageAssets.ImageVideo);
                                string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                string bannerImg = String.IsNullOrEmpty(show.ImageBanner) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId, show.ImageBanner);
                                JsonFeatureItem j = new JsonFeatureItem() { EpisodeId = ep.EpisodeId, EpisodeDescription = MyUtility.Ellipsis(ep.Description, 20), EpisodeName = ep.EpisodeName, EpisodeAirDate = (ep.DateAired != null) ? ep.DateAired.Value.ToString("MMM d, yyyy") : "", ShowId = show.CategoryId, ShowName = MyUtility.Ellipsis(show.CategoryName, 20), EpisodeImageUrl = img, ShowImageUrl = showImg, Blurb = MyUtility.Ellipsis(ep.Synopsis, 80), ShowBannerUrl = bannerImg, parentId = parentId, parent = parent };
                                jfi.Add(j);
                            }
                        }
                    }
                }
            }

            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Social()
        {
            NameValueCollection qs = Request.Params;
            SynapseResponse response = new SynapseResponse();
            string gigyaUID = Uri.UnescapeDataString(qs["UID"]);
            bool isRequestValid = SigUtils.ValidateUserSignature(gigyaUID, Uri.UnescapeDataString(qs["timestamp"]), GlobalConfig.GSsecretkey, Uri.UnescapeDataString(qs["signature"]));
            if (isRequestValid)
            {
                var isNewUser = !Convert.ToBoolean(qs["isSiteUID"]); // If isSiteUID=='false' , this means the UID was generated by Gigya, hence the user is new. !(false) == true
                if (isNewUser)
                {
                    //TempData["qs"] = qs; //bring the parameters to the next view
                    //return RedirectToAction("Index");

                    response.errorCode = (int)ErrorCodes.IsNewSiteUser;
                    response.errorMessage = "Register the user";
                    response.data = qs;
                }
                else
                {
                    //FormsAuthentication.SetAuthCookie(gigyaUID, true); //Authenticate user to our site.
                    SetAutheticationCookie(gigyaUID);
                    var context = new IPTV2Entities();
                    User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(gigyaUID));
                    if (user != null)
                    {
                        SynapseCookie cookie = new SynapseCookie()
                        {
                            cookieName = FormsAuthentication.FormsCookieName,
                            cookiePath = FormsAuthentication.FormsCookiePath,
                            cookieDomain = FormsAuthentication.CookieDomain
                        };
                        HttpCookie authCookie = SetAutheticationCookie(user.UserId.ToString());
                        cookie.cookieValue = authCookie.Value;
                        response.errorCode = (int)ErrorCodes.Success;
                        response.errorMessage = "Login success";
                        response.info = cookie;
                        GSResponse res = GetToken(user);
                        if (res != null)
                        {
                            SynapseToken token = new SynapseToken()
                            {
                                uid = user.UserId.ToString(),
                                token = res.GetString("access_token", String.Empty),
                                expire = res.GetInt("expires_in", 0),
                            };

                            response.data = token;
                        }
                    }
                    else
                    {
                        response.errorCode = (int)ErrorCodes.UserDoesNotExist;
                        response.errorMessage = "User does not exist";
                    }
                }
            }
            else
            {
                response.errorCode = (int)ErrorCodes.IsInvalidRequest;
                response.errorMessage = "User does not exist";
            }
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Register(FormCollection fc)
        {
            SynapseResponse response = new SynapseResponse();
            Dictionary<string, object> collection = new Dictionary<string, object>();

            response.errorCode = (int)ErrorCodes.IsAlreadyAuthenticated;
            response.errorMessage = @"Please go to http://tfc.tv to register.";
            return this.Json(response, JsonRequestBehavior.AllowGet);

            if (MyUtility.isUserLoggedIn()) //User is logged in.
            {
                response.errorCode = (int)ErrorCodes.IsAlreadyAuthenticated;
                response.errorMessage = "User is already authenticated.";
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }

            if (String.IsNullOrEmpty(fc["Email"]))
            {
                response.errorCode = (int)ErrorCodes.IsEmailEmpty;
                response.errorMessage = MyUtility.getErrorMessage(ErrorCodes.IsEmailEmpty);
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }
            if (String.Compare(fc["Password"], fc["ConfirmPassword"], false) != 0)
            {
                response.errorCode = (int)ErrorCodes.IsMismatchPassword;
                response.errorMessage = MyUtility.getErrorMessage(ErrorCodes.IsMismatchPassword);
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }
            if (String.IsNullOrEmpty(fc["FirstName"]) || String.IsNullOrEmpty(fc["LastName"]) || String.IsNullOrEmpty(fc["CountryCode"]))
            {
                response.errorCode = (int)ErrorCodes.IsMissingRequiredFields;
                response.errorMessage = MyUtility.getErrorMessage(ErrorCodes.IsMissingRequiredFields);
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }

            try
            {
                string FirstName = fc["FirstName"];
                string LastName = fc["LastName"];
                string CountryCode = fc["CountryCode"];
                string EMail = fc["Email"];
                string Password = fc["Password"];
                string City = fc["City"];
                string State = String.IsNullOrEmpty(fc["State"]) ? fc["StateDD"] : fc["State"];
                System.Guid userId = System.Guid.NewGuid();


                if (FirstName.Length > 32)
                {
                    response.errorCode = (int)ErrorCodes.LimitReached;
                    response.errorMessage = "First Name cannot exceed 32 characters.";
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }
                if (LastName.Length > 32)
                {
                    response.errorCode = (int)ErrorCodes.LimitReached;
                    response.errorMessage = "Last Name cannot exceed 32 characters.";
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }
                if (EMail.Length > 64)
                {
                    response.errorCode = (int)ErrorCodes.LimitReached;
                    response.errorMessage = "Email cannot exceed 64 characters.";
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }
                if (!String.IsNullOrEmpty(State))
                    if (State.Length > 30)
                    {
                        response.errorCode = (int)ErrorCodes.LimitReached;
                        response.errorMessage = "State cannot exceed 30 characters.";
                        return this.Json(response, JsonRequestBehavior.AllowGet);
                    }
                if (!String.IsNullOrEmpty(City))
                    if (City.Length > 50)
                    {
                        response.errorCode = (int)ErrorCodes.LimitReached;
                        response.errorMessage = "City cannot exceed 50 characters.";
                        return this.Json(response, JsonRequestBehavior.AllowGet);
                    }

                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EMail, true) == 0);
                if (user != null)
                {
                    response.errorCode = (int)ErrorCodes.IsExistingEmail;
                    response.errorMessage = MyUtility.getErrorMessage(ErrorCodes.IsExistingEmail);
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }

                /***** CHECK FOR COUNTRY CODE ****/
                if (context.Countries.Count(c => String.Compare(c.Code, CountryCode, true) == 0) <= 0)
                {
                    response.errorCode = (int)ErrorCodes.IsMissingRequiredFields;
                    response.errorMessage = "Country Code is invalid.";
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }
                else if (GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',').Contains(CountryCode))
                {
                    response.errorCode = (int)ErrorCodes.IsMissingRequiredFields;
                    response.errorMessage = "Country Code is invalid.";
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }

                DateTime registDt = DateTime.Now;
                user = new User()
                {
                    UserId = userId,
                    FirstName = FirstName,
                    LastName = LastName,
                    City = City,
                    State = State,
                    CountryCode = CountryCode,
                    EMail = EMail,
                    Password = MyUtility.GetSHA1(Password),
                    GigyaUID = userId.ToString(),
                    RegistrationDate = registDt,
                    LastUpdated = registDt,
                    RegistrationIp = Request.GetUserHostAddressFromCloudflare(),
                    StatusId = 1,
                    ActivationKey = Guid.NewGuid(),
                    DateVerified = registDt
                };
                string CurrencyCode = GlobalConfig.DefaultCurrency;
                Country country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);
                if (country != null)
                {
                    Currency currency = context.Currencies.FirstOrDefault(c => String.Compare(c.Code, country.CurrencyCode, true) == 0);
                    if (currency != null) CurrencyCode = currency.Code;
                }
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, CurrencyCode, true) == 0);
                if (wallet == null) // Wallet does not exist. Create new wallet for User.
                {
                    wallet = ContextHelper.CreateWallet(0, CurrencyCode, registDt);
                    user.UserWallets.Add(wallet);
                }

                var transaction = new RegistrationTransaction()
                {
                    RegisteredState = user.State,
                    RegisteredCity = user.City,
                    RegisteredCountryCode = user.CountryCode,
                    Amount = 0,
                    Currency = CurrencyCode,
                    Reference = "New Registration (Mobile)",
                    Date = registDt,
                    OfferingId = GlobalConfig.offeringId,
                    UserId = user.UserId,
                    StatusId = GlobalConfig.Visible
                };

                user.Transactions.Add(transaction);

                context.Users.Add(user);
                if (context.SaveChanges() > 0)
                {
                    if (!String.IsNullOrEmpty(fc["UID"]))
                    {
                        Dictionary<string, object> GigyaCollection = new Dictionary<string, object>();
                        collection.Add("uid", fc["UID"]);
                        collection.Add("siteUID", userId);
                        collection.Add("cid", String.Format("{0} - New User", fc["provider"]));
                        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(collection));
                    }
                    else
                    {
                        Dictionary<string, object> userInfo = new Dictionary<string, object>();
                        userInfo.Add("firstName", user.FirstName);
                        userInfo.Add("lastName", user.LastName);
                        userInfo.Add("email", user.EMail);
                        Dictionary<string, object> gigyaCollection = new Dictionary<string, object>();
                        gigyaCollection.Add("siteUID", user.UserId);
                        gigyaCollection.Add("cid", "TFCTV - Registration");
                        gigyaCollection.Add("sessionExpiration", "0");
                        gigyaCollection.Add("newUser", true);
                        gigyaCollection.Add("userInfo", MyUtility.buildJson(userInfo));
                        GSResponse res = GigyaHelpers.createAndSendRequest("socialize.notifyLogin", GigyaHelpers.buildParameter(gigyaCollection));
                        GigyaHelpers.setCookie(res, this.ControllerContext);
                    }

                    //setUserData
                    User usr = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EMail, true) == 0);
                    setUserData(usr.UserId.ToString(), usr);


                    if (usr.IsTVERegistrant == null || usr.IsTVERegistrant == false)
                    {
                        int freeTrialProductId = 0;
                        if (GlobalConfig.IsFreeTrialEnabled)
                        {
                            freeTrialProductId = MyUtility.GetCorrespondingFreeTrialProductId();
                            context = new IPTV2Entities();
                            if (GlobalConfig.TfcTvFree2StartDate < registDt && GlobalConfig.TfcTvFree2EndDate > registDt)
                            {
                                string UserCountryCode = user.CountryCode;
                                if (!GlobalConfig.isUAT)
                                    try { UserCountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy(); }
                                    catch (Exception) { }

                                var countryList = GlobalConfig.TfcTvFree2CountryWhiteList.Split(',');
                                if (countryList.Contains(UserCountryCode) && String.Compare(user.CountryCode, UserCountryCode, true) == 0)
                                    freeTrialProductId = GlobalConfig.TfcTvFree2ProductId;
                            }
                            //if (user.StatusId == GlobalConfig.Visible)
                            PaymentHelper.PayViaWallet(context, userId, freeTrialProductId, SubscriptionProductType.Package, userId, null);
                        }
                    }

                    //Publish to Activity Feed
                    List<ActionLink> actionlinks = new List<ActionLink>();
                    actionlinks.Add(new ActionLink() { text = SNSTemplates.register_actionlink_text, href = SNSTemplates.register_actionlink_href });
                    UserAction action = new UserAction()
                    {
                        actorUID = userId.ToString(),
                        userMessage = SNSTemplates.register_usermessage,
                        title = SNSTemplates.register_title,
                        subtitle = SNSTemplates.register_subtitle,
                        linkBack = SNSTemplates.register_linkback,
                        description = String.Format(SNSTemplates.register_description, FirstName),
                        actionLinks = actionlinks
                    };

                    GigyaMethods.PublishUserAction(action, userId, "external");
                    action.userMessage = String.Empty;
                    action.title = String.Empty;
                    GigyaMethods.PublishUserAction(action, userId, "internal");
                    //string verification_email = String.Format("{0}/User/Verify?email={1}&key={2}", GlobalConfig.baseUrl, EMail, user.ActivationKey.ToString());
                    //string emailBody = String.Format(GlobalConfig.EmailVerificationBodyTextOnly, FirstName, EMail, verification_email);

                    //if (!Request.IsLocal)
                    //    try { MyUtility.SendEmailViaSendGrid(EMail, GlobalConfig.NoReplyEmail, "Activate your TFC.tv account", emailBody, MailType.TextOnly, emailBody); }
                    //    catch (Exception e) { MyUtility.LogException(e, "Unable to send email via SendGrid"); }

                    response.errorCode = (int)ErrorCodes.Success;
                    response.errorMessage = "Registration successful.";

                    GSResponse gres = GetToken(user);
                    if (gres != null)
                    {
                        SynapseToken token = new SynapseToken()
                        {
                            uid = user.UserId.ToString(),
                            token = gres.GetString("access_token", String.Empty),
                            expire = gres.GetInt("expires_in", 0),
                        };

                        response.data = token;
                    }

                    HttpCookie authCookie = SetAutheticationCookie(user.UserId.ToString());
                    SynapseCookie cookie = new SynapseCookie()
                    {
                        cookieName = authCookie.Name,
                        cookieDomain = authCookie.Domain,
                        cookiePath = authCookie.Path,
                        cookieValue = authCookie.Value
                    };
                    response.info = cookie;
                }
                else
                {
                    response.errorCode = (int)ErrorCodes.EntityUpdateError;
                    response.errorMessage = "Unable to register user";
                }
            }
            catch (Exception e)
            {
                response.errorCode = (int)ErrorCodes.UnknownError;
                response.errorMessage = e.Message;
            }
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Link(FormCollection fc)
        {
            SynapseResponse response = new SynapseResponse();

            if (MyUtility.isUserLoggedIn()) //User is logged in.
            {
                response.errorCode = (int)ErrorCodes.IsAlreadyAuthenticated;
                response.errorMessage = "User is already authenticated.";
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }

            try
            {
                string EmailAddress = fc["EmailAddress"];
                string Password = fc["Password"];
                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EmailAddress, true) == 0);
                if (user == null)
                {
                    response.errorCode = (int)ErrorCodes.UserDoesNotExist;
                    response.errorMessage = "User does not exist.";
                }
                else
                {
                    Password = MyUtility.GetSHA1(Password);
                    if (String.Compare(user.EMail, EmailAddress, true) == 0 && String.Compare(user.Password, Password, false) == 0)
                    {
                        /** notifyRegistration **/
                        Dictionary<string, object> regCollection = new Dictionary<string, object>();

                        regCollection.Add("siteUID", user.UserId.ToString());
                        if (!String.IsNullOrEmpty(fc["UID"]))
                        {
                            regCollection.Add("uid", Uri.UnescapeDataString(fc["UID"]));
                            regCollection.Add("cid", String.Format("{0} - New User", fc["provider"]));
                        }
                        GSResponse notifyRegistration = GigyaHelpers.createAndSendRequest("socialize.notifyRegistration", GigyaHelpers.buildParameter(regCollection));

                        if (notifyRegistration.GetErrorCode() == 0) //Successful link
                        {
                            GSResponse res = GetToken(user);
                            if (res != null)
                            {
                                SynapseToken token = new SynapseToken()
                                {
                                    uid = user.UserId.ToString(),
                                    token = res.GetString("access_token", String.Empty),
                                    expire = res.GetInt("expires_in", 0),
                                };

                                response.data = token;
                            }
                            response.errorCode = (int)ErrorCodes.Success;
                            response.errorMessage = "Linking successful!";
                            HttpCookie authCookie = SetAutheticationCookie(user.UserId.ToString());
                            SynapseCookie cookie = new SynapseCookie()
                            {
                                cookieName = authCookie.Name,
                                cookieDomain = authCookie.Domain,
                                cookiePath = authCookie.Path,
                                cookieValue = authCookie.Value
                            };
                            response.info = cookie;
                        }
                        else
                        {
                            response.errorCode = (int)ErrorCodes.FailedToLinkAccount;
                            response.errorMessage = "Unable to link your account. Please try again." + notifyRegistration.GetErrorMessage() + " " + notifyRegistration.GetErrorCode();
                        }
                    }
                    else
                    {
                        response.errorCode = (int)ErrorCodes.IsWrongPassword;
                        response.errorMessage = "Invalid email/password combination.";
                    }
                }
            }
            catch (Exception) { throw; }
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetCountryList()
        {
            var context = new IPTV2Entities();
            var ExcludedCountriesFromRegistrationDropDown = GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',');
            var countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).Select(c => new { c.Code, c.Description }).ToList();
            return this.Json(countries, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(VaryByParam = "id", Duration = 180)]
        public JsonResult GetCountryState(string id)
        {
            var context = new IPTV2Entities();
            var states = context.States.Where(s => String.Compare(s.CountryCode, id, true) == 0).Select(s => new { s.StateCode, s.Name }).OrderBy(s => s.Name);
            return this.Json(states, JsonRequestBehavior.AllowGet);
        }

        private GSResponse GetToken(User user)
        {
            SynapseUserInfo uInfo = new SynapseUserInfo() { firstName = user.FirstName, lastName = user.LastName, email = user.EMail };
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("client_id", GlobalConfig.GSapikey);
            collection.Add("client_secret", GlobalConfig.GSsecretkey);
            collection.Add("grant_type", "none");
            collection.Add("x_siteUID", user.UserId);
            collection.Add("x_sessionExpiration", 0);
            collection.Add("x_userInfo", JsonConvert.SerializeObject(uInfo));
            return GigyaHelpers.createAndSendRequest("socialize.getToken", GigyaHelpers.buildParameter(collection));
        }

        [HttpPost]
        public ActionResult RemoveConnection(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("provider", fc["provider"]);
            collection.Add("uid", User.Identity.Name);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.removeConnection", GigyaHelpers.buildParameter(collection));
            return Content(res.GetData().ToJsonString(), "application/json");
        }

        private int setUserData(string uid, User user)
        {
            SignUpModel usr = new SignUpModel()
            {
                City = user.City,
                CountryCode = user.CountryCode,
                Email = user.EMail,
                FirstName = user.FirstName,
                LastName = user.LastName,
                State = user.State
            };
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("uid", uid);
            collection.Add("data", JsonConvert.SerializeObject(usr, Formatting.None));
            //gcs.setUserData
            //GSResponse res = GigyaHelpers.createAndSendRequest("gcs.setUserData", GigyaHelpers.buildParameter(collection));
            GSResponse res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", GigyaHelpers.buildParameter(collection));
            return res.GetErrorCode();
        }

        [HttpPost]
        public ActionResult EditProfile(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            DateTime registDt = DateTime.Now;

            if (MyUtility.isUserLoggedIn())
            {
                try
                {
                    var context = new IPTV2Entities();
                    User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(User.Identity.Name));
                    if (user != null)
                    {
                        string CurrencyCode = GlobalConfig.DefaultCurrency;
                        string OldCountryCode = GlobalConfig.DefaultCountry;
                        Country country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, user.CountryCode, true) == 0);
                        if (country != null)
                        {
                            CurrencyCode = country.Currency.Code; country = null;
                            OldCountryCode = user.Country.Code;
                        }

                        UserWallet currentWallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, CurrencyCode, true) == 0);
                        if (currentWallet == null) //If no wallet, get default USD wallet.
                            currentWallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, GlobalConfig.DefaultCurrency, true) == 0);

                        string newCountryCode = fc["CountryCode"];
                        user.FirstName = fc["FirstName"];
                        user.LastName = fc["LastName"];
                        user.City = fc["City"];
                        user.State = String.IsNullOrEmpty(fc["State"]) || fc["State"] == "" ? (String.IsNullOrEmpty(fc["StateDD"]) ? user.State : fc["StateDD"]) : fc["State"];

                        country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, newCountryCode, true) == 0);
                        if (country != null)
                        {
                            Currency currency = country.Currency;
                            CurrencyCode = (currency == null) ? GlobalConfig.DefaultCurrency : currency.Code;
                        }

                        UserWallet wallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, CurrencyCode, true) == 0);
                        decimal balance = 0;
                        decimal oldWalletBalance = currentWallet.Balance;
                        string oldCurrency = currentWallet.Currency;
                        if (wallet == null) // Wallet does not exist. Create new wallet for User.
                        {
                            if (currentWallet != null)
                            {
                                balance = currentWallet.Currency != CurrencyCode ? Forex.Convert(context, currentWallet.Currency, CurrencyCode, currentWallet.Balance) : currentWallet.Balance;
                                //balance = currentWallet.Balance;
                                currentWallet.Balance = 0;
                                currentWallet.IsActive = false;
                                currentWallet.GomsWalletId = null; // Reset Goms WalletId
                                currentWallet.LastUpdated = registDt;
                            }
                            wallet = ContextHelper.CreateWallet(balance, CurrencyCode, registDt);
                            user.UserWallets.Add(wallet);
                        }
                        else // Wallet already exists. Update the balance only.
                        {
                            if (currentWallet.Currency != wallet.Currency)
                            {
                                balance = currentWallet.Currency != wallet.Currency ? Forex.Convert(context, currentWallet.Currency, wallet.Currency, currentWallet.Balance) : currentWallet.Balance;
                                wallet.Balance = balance;
                                //wallet.Balance += (currentWallet.Balance * 1);
                                wallet.IsActive = true;
                                wallet.LastUpdated = registDt;
                                wallet.GomsWalletId = null; // Reset Goms WalletId
                                currentWallet.Balance = 0; // Deactivate old wallet
                                currentWallet.IsActive = false; //Deactivate
                                currentWallet.GomsWalletId = null; // Reset Goms WalletId
                                currentWallet.LastUpdated = registDt;
                            }
                        }

                        user.CountryCode = newCountryCode; // Update user country
                        user.LastUpdated = registDt; // lastUpdate

                        if (OldCountryCode != newCountryCode)
                        {
                            ChangeCountryTransaction transaction = new ChangeCountryTransaction()
                            {
                                OldCountryCode = OldCountryCode,
                                NewCountryCode = newCountryCode,
                                Date = registDt,
                                OfferingId = GlobalConfig.offeringId,
                                Reference = "Change Country",
                                UserId = user.UserId,
                                Amount = 0,
                                NewWalletBalance = balance,
                                OldWalletBalance = oldWalletBalance,
                                Currency = oldCurrency,
                                StatusId = GlobalConfig.Visible
                            };

                            user.Transactions.Add(transaction);
                        }

                        if (context.SaveChanges() > 0)
                        {
                            //setUserData
                            setUserData(user.UserId.ToString(), user);

                            errorMessage = "Your information has been updated successfully.";
                            collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                        }
                        else
                        {
                            errorMessage = "Error in updating your profile.";
                            collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
                        }
                    }
                }
                catch (Exception e)
                {
                    collection = MyUtility.setError(ErrorCodes.UnknownError, e.Message);
                }
            }

            return Content(MyUtility.buildJson(collection), "application/json");
        }

        [HttpPost]
        public JsonResult ChangePassword(FormCollection f)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            DateTime registDt = DateTime.Now;
            string currentPassword = f["Password"];
            string newPassword = f["NewPassword"];
            string confirmPassword = f["ConfirmPassword"];

            if (String.IsNullOrEmpty(currentPassword) || String.IsNullOrEmpty(newPassword) || String.IsNullOrEmpty(confirmPassword))
            {
                errorMessage = "Please fill in the required fields.";
                collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, errorMessage);
                return this.Json(collection, JsonRequestBehavior.AllowGet);
            }

            if (String.Compare(newPassword, confirmPassword, false) != 0)
            {
                errorMessage = "Password mismatch.";
                collection = MyUtility.setError(ErrorCodes.IsMismatchPassword, errorMessage);
                return this.Json(collection, JsonRequestBehavior.AllowGet);
            }

            try
            {
                System.Guid userId = System.Guid.Parse(User.Identity.Name);
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    string hashedPassword = MyUtility.GetSHA1(currentPassword);
                    string hashedNewPassword = MyUtility.GetSHA1(newPassword);
                    if (String.Compare(user.Password, hashedPassword, false) != 0)
                    {
                        errorMessage = "The current password you entered is incorrect. Please try again.";
                        collection = MyUtility.setError(ErrorCodes.IsMismatchPassword, errorMessage);
                        return this.Json(collection, JsonRequestBehavior.AllowGet);
                    }

                    user.Password = hashedNewPassword;
                    user.LastUpdated = registDt;

                    if (context.SaveChanges() > 0)
                    {
                        errorMessage = "Your password has been changed succcessfully.";
                        collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                    }
                    else
                    {
                        errorMessage = "The system encountered an unidentified error. Please try again.";
                        collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
                    }
                }
            }
            catch (Exception e) { collection = MyUtility.setError(ErrorCodes.UnknownError, e.Message); }

            return this.Json(collection, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ForgotPassword(FormCollection f)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(errorCode, errorMessage);
            DateTime registDt = DateTime.Now;
            string email = f["EmailAddress"];

            if (String.IsNullOrEmpty(email))
            {
                errorMessage = "Please fill in the required fields.";
                collection = MyUtility.setError(ErrorCodes.IsMissingRequiredFields, errorMessage);
                return this.Json(collection, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var context = new IPTV2Entities();
                User user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, email, true) == 0);
                if (user != null)
                {
                    Guid randomGuid = System.Guid.NewGuid();
                    string newPassword = randomGuid.ToString().Substring(0, 8);
                    string hashedPassword = MyUtility.GetSHA1(newPassword);
                    //UNCOMMENT WHEN NEEDED
                    //user.Password = hashedPassword;
                    user.LastUpdated = registDt;

                    if (context.SaveChanges() > 0)
                    {
                        //var mailer = new UserMailer();
                        //var msg = mailer.ForgotPassword(to: user.EMail, password: newPassword);
                        //msg.SendAsync();

                        //errorMessage = "Your password has been changed succcessfully.";
                        //collection = MyUtility.setError(ErrorCodes.Success, errorMessage);

                        string oid = MyUtility.GetSHA1(String.Format("{0}{1}", user.UserId, user.LastUpdated));
                        string reset_pwd_email = String.Format("{0}/User/ResetPassword?key={1}&oid={2}", GlobalConfig.baseUrl, user.ActivationKey, oid.ToLower());
                        string emailBody = String.Format(GlobalConfig.ResetPasswordBodyTextOnly, user.FirstName, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), reset_pwd_email);
                        try
                        {
                            MyUtility.SendEmailViaSendGrid(user.EMail, GlobalConfig.NoReplyEmail, "Reset your TFC.tv Password", emailBody, MailType.TextOnly, emailBody);
                            errorMessage = "Instructions on how to reset your password have been sent to your email address.";
                            collection = MyUtility.setError(ErrorCodes.Success, errorMessage);
                        }
                        catch (Exception e)
                        {
                            MyUtility.LogException(e);
                            errorMessage = "The system encountered an unspecified error. Please contact Customer Support.";
                            collection = MyUtility.setError(ErrorCodes.UnknownError, errorMessage);
                        }
                    }
                    else
                    {
                        errorMessage = "The system encountered an unidentified error. Please try again.";
                        collection = MyUtility.setError(ErrorCodes.EntityUpdateError, errorMessage);
                    }
                }

                else
                {
                    errorMessage = "Email does not exist.";
                    collection = MyUtility.setError(ErrorCodes.UserDoesNotExist, errorMessage);
                }
            }
            catch (Exception e) { collection = MyUtility.setError(ErrorCodes.UnknownError, e.Message); }

            return this.Json(collection, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWishlist(string id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = "ERROR";
            collection = MyUtility.setError(errorCode, errorMessage);

            string userId = String.IsNullOrEmpty(id) ? User.Identity.Name : id.ToLower();
            string query = String.Format(@"select _id, ProductId_i, ProductName_s, registDt_d, UID_s from Wishlist where UID_s = ""{0}""", userId);
            Dictionary<string, object> gcollection = new Dictionary<string, object>();
            gcollection.Add("query", query);
            GSResponse res = GigyaHelpers.createAndSendRequest("gcs.search", GigyaHelpers.buildParameter(gcollection));

            if (res.GetData().GetInt("objectsCount") > 0)
                return Content(res.GetArray("data", null).ToString(), "application/json");

            return Json(collection, JsonRequestBehavior.AllowGet);
        }

        //[OutputCache(VaryByParam = "q", Duration = 180)]
        //[HttpHeader("Content-Encoding", "gzip")]
        public JsonResult ContentSearch(string q)
        {
            //Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
            List<MyEntitledContentDisplay> display = new List<MyEntitledContentDisplay>();

            if (String.IsNullOrEmpty(q))
                return this.Json(display, JsonRequestBehavior.AllowGet);

            var context = new IPTV2Entities();
            var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
            SortedSet<int> showIds = service.GetAllMobileShowIds(MyUtility.GetCurrentCountryCodeOrDefault());
            q = q.ToLower();

            var categories = context.CategoryClasses.Where(c => c.Description.IndexOf(q) >= 0);

            //Search shows
            foreach (var item in categories)
            {
                if (item.StatusId == GlobalConfig.Visible)
                {
                    if (item is Show)
                    {
                        if (showIds.Contains(item.CategoryId))
                        {
                            Show show = (Show)item;

                            string parent = "";

                            string type = "show";
                            if (show is Movie)
                                type = "movie";
                            display.Add(new MyEntitledContentDisplay()
                            {
                                id = show.CategoryId,
                                title = show.Description,
                                blurb = show.Blurb,
                                type = type,
                                parent = parent,
                                image = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster)
                            });
                        }
                    }
                }
            }

            //Search episodes
            var searchShows = categories.Select(c => c.CategoryId);
            var episodes = context.EpisodeCategories1.Where(e => searchShows.Contains(e.CategoryId) && e.Episode.MobileStatusId == GlobalConfig.Visible);
            //var episodes = context.EpisodeCategories1.AsQueryable().Where(ep => ep.Show.Description.ToLower().IndexOf(q) >= 0 && ep.Episode.MobileStatusId == GlobalConfig.Visible && ep.Show.StatusId == GlobalConfig.Visible && showIds.Contains(ep.Show.CategoryId));
            foreach (var item in episodes)
            {
                if (showIds.Contains(item.CategoryId))
                {
                    string type = "episode";
                    display.Add(new MyEntitledContentDisplay()
                    {
                        id = item.Episode.EpisodeId,
                        title = String.Format("{0}, {1}", item.Show.Description, item.Episode.DateAired.Value.ToString("MMM d, yyyy")),
                        blurb = item.Episode.Synopsis,
                        type = type,
                        parent = item.Show.Description,
                        image = String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, item.Episode.EpisodeId, item.Episode.ImageAssets.ImageVideo)
                    });
                }
            }
            //Search celebrities
            var celebrities = context.Celebrities.AsQueryable().Where(c => c.FullName.ToLower().IndexOf(q) >= 0 && c.StatusId == GlobalConfig.Visible);
            foreach (var item in celebrities)
            {
                string type = "celebrity";
                display.Add(new MyEntitledContentDisplay()
                {
                    id = item.CelebrityId,
                    title = item.FullName,
                    blurb = item.Description,
                    type = type,
                    parent = string.Empty,
                    image = String.IsNullOrEmpty(item.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, item.CelebrityId.ToString(), item.ImageUrl)
                });
            }

            return Json(display, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        //[ValidateInput(false)]
        public ActionResult SubmitTicket(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();

            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection.Add("errorCode", errorCode);
            collection.Add("errorMessage", errorMessage);

            ErrorResponse response = new ErrorResponse();

            if (String.IsNullOrEmpty(fc["email"]) || String.IsNullOrEmpty(fc["message"]))
            {
                collection["errorCode"] = (int)ErrorCodes.IsMissingRequiredFields;
                collection["errorMessage"] = "Please fill up the required fields.";
                return Content(MyUtility.buildJson(collection), "application/json");
            }

            string email = fc["email"];
            string subject = fc["subject"];
            string message = Uri.EscapeDataString(fc["message"]);

            if (MyUtility.isUserLoggedIn())
                response = CreateGomsTicket(subject, message);

            else
                response = CreateServiceDeskTicket(email, subject, message);

            collection["errorCode"] = response.Code;
            collection["errorMessage"] = response.Message;
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        private ErrorResponse CreateGomsTicket(string subject, string message)
        {
            ErrorResponse response;
            if (!MyUtility.isUserLoggedIn())
                return new ErrorResponse() { Code = (int)ErrorCodes.NotAuthenticated, Message = "User is not logged in." };

            var context = new IPTV2Entities();

            var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(HttpContext.User.Identity.Name));
            if (user != null)
            {
                var gomsService = new GomsTfcTv();
                var agent = (GomsCaseAgent)context.GomsReferences.FirstOrDefault(r => r is GomsCaseAgent);
                var caseIssueType = (GomsCaseIssueType)context.GomsReferences.FirstOrDefault(r => r is GomsCaseIssueType);
                var caseSubIssueType = (GomsCaseSubIssueType)context.GomsReferences.FirstOrDefault(r => r is GomsCaseSubIssueType);
                try
                {
                    var resp = gomsService.CreateSupportCase(context, user.UserId, subject, message, agent, caseIssueType, caseSubIssueType);
                    response = new ErrorResponse() { Code = Convert.ToInt32(resp.StatusCode), Message = resp.StatusMessage };
                    response.Message = response.Code == 0 ? "You have successfully submitted a ticket." : resp.StatusMessage;
                }
                catch (Exception e)
                {
                    response = new ErrorResponse() { Code = (int)ErrorCodes.UnknownError, Message = e.Message };
                }
            }
            else
                response = new ErrorResponse() { Code = (int)ErrorCodes.UserDoesNotExist, Message = "User does not exist." };
            return response;
        }

        private ErrorResponse CreateServiceDeskTicket(string email, string subject, string message)
        {
            string userName = GlobalConfig.ServiceDeskUsername;
            string password = GlobalConfig.ServiceDeskPassword;
            int departmentId = GlobalConfig.ServiceDeskDepartmentId;
            ErrorResponse response;
            var context = new IPTV2Entities();
            try
            {
                var result = ServiceDesk.Ticket.CreateTicket(userName, password, departmentId, email, subject, message, false);
                response = new ErrorResponse() { Code = (int)ErrorCodes.Success, Message = "Ticket has been submitted." };
            }
            catch (Exception e)
            {
                response = new ErrorResponse() { Code = (int)ErrorCodes.UnknownError, Message = e.Message };
            }

            return response;
        }


        [HttpHeader("Content-Encoding", "gzip")]
        public JsonResult GetMovies()
        {
            Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);

            try
            {
                var context = new IPTV2Entities();
                var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.Movies && c.StatusId == GlobalConfig.Visible);

                List<SynapseShow> list = new List<SynapseShow>();

                string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
                var setOfMovies = service.GetAllMobileShowIds(countrycode, (Category)category);

                var movies = context.CategoryClasses.Where(c => setOfMovies.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible);

                if (movies != null)
                {
                    foreach (var movie in movies)
                    {
                        SynapseShow show = null;
                        if (movie is Show)
                        {
                            string type = "show";
                            var temp = (Show)movie;
                            if (temp is Movie)
                            {
                                type = "movie";
                                var eList = temp.Episodes.OrderByDescending(e => e.Episode.DateAired);
                                if (eList != null)
                                {
                                    var epList = eList.ToList();
                                    List<SynapseEpisode> episodes = new List<SynapseEpisode>();
                                    foreach (var e in epList)
                                    {
                                        string epImg = String.IsNullOrEmpty(e.Episode.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, e.Episode.EpisodeId.ToString(), e.Episode.ImageAssets.ImageVideo);
                                        SynapseEpisode episode = new SynapseEpisode() { id = e.Episode.EpisodeId, name = e.Episode.Description, dateaired = e.Episode.DateAired.Value.ToString("MMM d, yyyy"), synopsis = e.Episode.Synopsis, image = epImg };
                                        episodes.Add(episode);
                                    }
                                    string img = String.IsNullOrEmpty(temp.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, temp.CategoryId.ToString(), temp.ImagePoster);
                                    string banner = String.IsNullOrEmpty(temp.ImageBanner) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, temp.CategoryId.ToString(), temp.ImageBanner);
                                    show = new SynapseShow() { id = temp.CategoryId, name = temp.Description, blurb = temp.Blurb, image = img, banner = banner, type = type };
                                    var parentcat = temp.CategoryClassParentCategories.FirstOrDefault(c => c.ParentCategory.CategoryId != GlobalConfig.FreeTvCategoryId);
                                    show.parentId = parentcat.ParentId;
                                    show.parent = parentcat.ParentCategory.Description;
                                    show.episodes = episodes;
                                }
                                list.Add(show);
                            }
                        }
                    }
                    return this.Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return this.Json(null, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public bool CheckIfEntitled(int? id, string uid)
        {
            bool isEntitled = false;

            if (String.IsNullOrEmpty(uid) || id == null)
                return isEntitled;

            try
            {
                var context = new IPTV2Entities();
                Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.MobileStatusId == GlobalConfig.Visible);

                if (ep != null)
                {
                    var pAsset = ep.PremiumAssets.FirstOrDefault();
                    if (pAsset != null)
                    {
                        Asset asset = pAsset.Asset;
                        if (asset != null)
                        {
                            int assetId = asset == null ? 0 : asset.AssetId;
                            var clipDetails = Helpers.Akamai.GetAkamaiClipDetailsByUserId(ep.EpisodeId, assetId, Request, new Guid(uid));

                            if (!String.IsNullOrEmpty(clipDetails.Url))
                                isEntitled = true;
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return isEntitled;
        }

        public bool CheckForLogin()
        {
            bool MultipleLoginDetected = false;

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
                            Dictionary<string, string> collection = new Dictionary<string, string>();
                            collection.Add("uid", User.Identity.Name);
                            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.logout", GigyaHelpers.buildParameter(collection));
                            MultipleLoginDetected = true;
                        }
                    }
                }
            }

            if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
            {
                try
                {
                    if (MyUtility.isUserLoggedIn())
                    {
                        var context = new IPTV2Entities();
                        HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                        var UserId = new Guid(User.Identity.Name);
                        var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            if (!String.IsNullOrEmpty(user.SessionId))
                            {
                                if (String.Compare(user.SessionId, authCookie.Value, true) != 0)
                                {
                                    Dictionary<string, string> collection = new Dictionary<string, string>();
                                    collection.Add("uid", User.Identity.Name);
                                    GSResponse res = GigyaHelpers.createAndSendRequest("socialize.logout", GigyaHelpers.buildParameter(collection));
                                    MultipleLoginDetected = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
            return MultipleLoginDetected;
        }

        public JsonResult GetAllCelebrities()
        {
            string jsonString = String.Empty;
            List<CelebrityDisplay> celebrityList = null;
            try
            {
                var cache = DataCache.Cache;
                string cacheKey = "SYNGACELEB;0";
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    celebrityList = new List<CelebrityDisplay>();
                    var context = new IPTV2Entities();
                    var celebrities = context.Celebrities.Where(c => c.StatusId == GlobalConfig.Visible).OrderBy(c => c.LastName == null ? c.FirstName : c.LastName);
                    foreach (var c in celebrities)
                    {
                        celebrityList.Add(new CelebrityDisplay()
                        {
                            CelebrityId = c.CelebrityId,
                            FirstName = c.FirstName,
                            LastName = c.LastName,
                            ImageUrl = String.IsNullOrEmpty(c.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, c.CelebrityId.ToString(), c.ImageUrl)

                        });
                    }
                    var cacheDuration = new TimeSpan(0, GlobalConfig.SynapseGenericCacheDuration, 0);
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(celebrityList);
                    cache.Put(cacheKey, jsonString, cacheDuration);
                }
                else
                    celebrityList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CelebrityDisplay>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(celebrityList, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetFreeTrialPackageContent()
        //{
        //    var context = new IPTV2Entities();
        //    string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
        //    string currencycode = MyUtility.GetCurrencyOrDefault(countrycode);

        //    int packageid = 0;
        //    int freeTrialPackageId = 0;
        //    var list = MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds);

        //    var registDt = DateTime.Now;

        //    List<ShowLookUpObject> datalist = new List<ShowLookUpObject>();
        //    List<ShowLookUpObject> clone_datalist = null;

        //    try
        //    {

        //        if (!User.Identity.IsAuthenticated)
        //            return Json(null, JsonRequestBehavior.AllowGet);

        //        if (User.Identity.IsAuthenticated)
        //        {
        //            var userId = new Guid(User.Identity.Name);
        //            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //            if (user != null)
        //            {
        //                var package = user.PackageEntitlements.FirstOrDefault(p => list.Contains(p.PackageId) && p.EndDate > registDt);
        //                if (package != null)
        //                    freeTrialPackageId = package.PackageId;
        //            }
        //        }

        //        packageid = freeTrialPackageId;

        //        if (!this.IsAllowed(packageid, countrycode))
        //        {
        //            return Json(null, JsonRequestBehavior.AllowGet);
        //        }

        //        var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.StatusId == GlobalConfig.Visible);
        //        var maincategory = context.PackageCategories.Where(pc => pc.PackageId == packageid).Select(p => p.Category);
        //        var subcategory = maincategory.Select(p => p.SubCategories);
        //        int[] category_ids = context.PackageCategories.Where(pc => pc.PackageId == packageid).Select(p => p.CategoryId).ToArray();

        //        //for movies
        //        foreach (var movie in maincategory)
        //        {
        //            if (movie.SubCategories.Count() == 0)
        //            {
        //                int[] movieIds = service.GetAllOnlineShowIds(MyUtility.GetCurrentCountryCodeOrDefault(), movie).ToArray();
        //                foreach (var item in context.CategoryClasses
        //                            .Where(s => movieIds.Contains(s.CategoryId)
        //                                && s.StatusId == GlobalConfig.Visible))
        //                {
        //                    if (item is Show)
        //                    {
        //                        var show = (Show)item;
        //                        ShowLookUpObject data = new ShowLookUpObject();
        //                        data.Show = show.Description;
        //                        data.ShowId = show.CategoryId;
        //                        data.MainCategory = movie.Description;
        //                        data.MainCategoryId = movie.CategoryId;
        //                        if (show.StartDate < registDt && show.EndDate > registDt)
        //                            datalist.Add(data);
        //                    }
        //                }
        //            }
        //        }

        //        //for shows
        //        foreach (var subitems in subcategory)
        //        {
        //            foreach (Category item in subitems)
        //            {
        //                int[] showIds = service.GetAllOnlineShowIds(MyUtility.GetCurrentCountryCodeOrDefault(), item).ToArray();

        //                foreach (var cat in context.CategoryClasses
        //                            .Where(s => showIds.Contains(s.CategoryId)
        //                                && s.StatusId == GlobalConfig.Visible))
        //                {
        //                    if (cat is Show)
        //                    {
        //                        var show = (Show)cat;
        //                        ShowLookUpObject data = new ShowLookUpObject();
        //                        data.Show = show.Description;
        //                        data.ShowId = show.CategoryId;
        //                        data.MainCategory = item.Description;
        //                        data.MainCategoryId = item.CategoryId;
        //                        if (show.StartDate < registDt && show.EndDate > registDt)
        //                            datalist.Add(data);
        //                    }
        //                }
        //            }
        //        }

        //        clone_datalist = datalist.Where(p => p.ShowId == 310).ToList();
        //        if (clone_datalist.Count() > 0)
        //        {
        //            int mmk_main_id = clone_datalist.FirstOrDefault().MainCategoryId;
        //            clone_datalist.AddRange(datalist.Where(p => p.MainCategoryId == mmk_main_id && p.ShowId != 310).ToList());
        //            clone_datalist.AddRange(datalist.Where(p => p.MainCategoryId != mmk_main_id).OrderBy(p => p.MainCategoryId).ToList());
        //        }
        //        else
        //        {
        //            clone_datalist = datalist;
        //        }

        //        return Json(clone_datalist, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    return Json(clone_datalist, JsonRequestBehavior.AllowGet);
        //}

        private bool IsAllowed(int packageid, string countrycode)
        {
            var context = new IPTV2Entities();
            //check if package is allowed
            if (context.ProductPackages
                .Where(pp => pp.PackageId == packageid && pp.Package.StatusId == GlobalConfig.Visible)
                .AsEnumerable()
                .Where(pp => pp.Product.IsAllowed(countrycode))
                .Count() == 0)
            {
                return false;
            }

            return true;
        }

        public ActionResult GetFreeTrialPackageContent()
        {
            if (!GlobalConfig.IsSynapseEnabled)
                return Json(null, JsonRequestBehavior.AllowGet);

            var context = new IPTV2Entities();
            string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
            string currencycode = MyUtility.GetCurrencyOrDefault(countrycode);

            int packageid = 0;
            //int freeTrialPackageId = 0;
            var list = MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds);

            var registDt = DateTime.Now;

            List<ShowLookUpObject> datalist = new List<ShowLookUpObject>();
            List<ShowLookUpObject> clone_datalist = null;
            List<Int32> packageIds = null;
            IEnumerable<Int32> categoryIds = null;
            try
            {

                if (!User.Identity.IsAuthenticated)
                    return Json(null, JsonRequestBehavior.AllowGet);

                if (User.Identity.IsAuthenticated)
                {
                    var userId = new Guid(User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                    {
                        //var package = user.PackageEntitlements.FirstOrDefault(p => list.Contains(p.PackageId) && p.EndDate > registDt);
                        //if (package != null)
                        //    freeTrialPackageId = package.PackageId;

                        var packages = user.PackageEntitlements.Where(p => p.EndDate > registDt);

                        if (packages != null)
                        {
                            packageIds = new List<int>();
                            foreach (var pkg in packages)
                            {
                                if (this.IsAllowed(pkg.PackageId, countrycode))
                                    packageIds.Add(pkg.PackageId);
                            }
                        }


                        var showPackages = user.ShowEntitlements.Where(s => s.EndDate > registDt);
                        if (showPackages != null)
                        {
                            categoryIds = showPackages.Select(s => s.CategoryId);
                        }
                    }
                }

                if (packageIds == null && categoryIds == null)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }


                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.StatusId == GlobalConfig.Visible);
                var maincategory = context.PackageCategories.Where(pc => packageIds.Contains(pc.PackageId)).Select(p => p.Category);
                var subcategory = maincategory.Select(p => p.SubCategories);
                int[] category_ids = context.PackageCategories.Where(pc => pc.PackageId == packageid).Select(p => p.CategoryId).ToArray();
                category_ids.Union(categoryIds);

                //for movies
                foreach (var movie in maincategory)
                {
                    if (movie.SubCategories.Count() == 0)
                    {
                        int[] movieIds = service.GetAllMobileShowIds(MyUtility.GetCurrentCountryCodeOrDefault(), movie).ToArray();
                        foreach (var item in context.CategoryClasses
                                    .Where(s => movieIds.Contains(s.CategoryId)
                                        && s.StatusId == GlobalConfig.Visible))
                        {
                            if (item is Show)
                            {
                                var show = (Show)item;
                                if (!(show is LiveEvent))
                                {
                                    ShowLookUpObject data = new ShowLookUpObject();
                                    data.Show = show.Description;
                                    data.ShowId = show.CategoryId;
                                    data.MainCategory = movie.Description;
                                    data.MainCategoryId = movie.CategoryId;
                                    if (show.StartDate < registDt && show.EndDate > registDt)
                                        datalist.Add(data);
                                }
                            }
                        }
                    }
                }

                //for shows
                foreach (var subitems in subcategory)
                {
                    foreach (Category item in subitems)
                    {
                        int[] showIds = service.GetAllMobileShowIds(MyUtility.GetCurrentCountryCodeOrDefault(), item).ToArray();

                        foreach (var cat in context.CategoryClasses
                                    .Where(s => showIds.Contains(s.CategoryId)
                                        && s.StatusId == GlobalConfig.Visible))
                        {
                            if (cat is Show)
                            {
                                var show = (Show)cat;
                                if (!(show is LiveEvent))
                                {
                                    ShowLookUpObject data = new ShowLookUpObject();
                                    data.Show = show.Description;
                                    data.ShowId = show.CategoryId;
                                    data.MainCategory = item.Description;
                                    data.MainCategoryId = item.CategoryId;
                                    if (show.StartDate < registDt && show.EndDate > registDt)
                                        datalist.Add(data);
                                }
                            }
                        }
                    }
                }

                clone_datalist = datalist.Where(p => p.ShowId == 310).ToList();
                if (clone_datalist.Count() > 0)
                {
                    int mmk_main_id = clone_datalist.FirstOrDefault().MainCategoryId;
                    clone_datalist.AddRange(datalist.Where(p => p.MainCategoryId == mmk_main_id && p.ShowId != 310).ToList());
                    clone_datalist.AddRange(datalist.Where(p => p.MainCategoryId != mmk_main_id).OrderBy(p => p.MainCategoryId).ToList());
                }
                else
                {
                    clone_datalist = datalist;
                }

                return Json(clone_datalist, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(clone_datalist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllVideosBasedOnEntitlements()
        {
            List<ShowLookUpObject> list = null;

            if (!GlobalConfig.IsSynapseEnabled)
                return Json(list, JsonRequestBehavior.AllowGet);
            var registDt = DateTime.Now;
            if (MyUtility.isUserLoggedIn())
            {
                try
                {
                    //var cache = DataCache.Cache;
                    //string cacheKey = "MOBILEGAV:U:" + User.Identity.Name;
                    //list = (List<ShowLookUpObject>)cache[cacheKey];
                    if (list == null)
                    {
                        list = new List<ShowLookUpObject>();
                        var context = new IPTV2Entities();
                        var UserId = new Guid(User.Identity.Name);
                        var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                        if (user != null)
                        {
                            SortedSet<Int32> ShowIds = new SortedSet<int>();
                            var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.StatusId == GlobalConfig.Visible);
                            foreach (var entitlement in user.Entitlements.Where(e => e.EndDate > registDt))
                            {
                                if (entitlement is PackageEntitlement)
                                {
                                    var packageEntitlement = (PackageEntitlement)entitlement;
                                    var pkgCat = context.PackageCategories.Where(p => p.PackageId == packageEntitlement.PackageId).Select(p => p.Category);
                                    var pkgCatSubCategories = pkgCat.Select(p => p.SubCategories);
                                    foreach (var categories in pkgCatSubCategories)
                                    {
                                        foreach (var category in categories)
                                        {
                                            var listOfIds = service.GetAllMobileShowIds(MyUtility.GetCurrentCountryCodeOrDefault(), category);
                                            var showList = context.CategoryClasses.Where(c => listOfIds.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible);
                                            foreach (var show in showList)
                                            {
                                                if (show != null)
                                                {
                                                    if (!(ShowIds.Contains(show.CategoryId)))
                                                    {
                                                        if (show.StartDate < registDt && show.EndDate > registDt)
                                                        {
                                                            if (show is Show)
                                                            {
                                                                if (!(show is LiveEvent))
                                                                {
                                                                    ShowLookUpObject data = new ShowLookUpObject();
                                                                    data.Show = show.Description;
                                                                    data.ShowId = show.CategoryId;
                                                                    data.MainCategory = category.Description;
                                                                    data.MainCategoryId = category.CategoryId;
                                                                    data.ShowType = (show is Movie) ? "MOVIE" : "SHOW";
                                                                    if (!(show is Movie))
                                                                        list.Add(data);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            ShowIds.UnionWith(listOfIds); //For checking
                                        }
                                    }
                                }
                                else if (entitlement is ShowEntitlement)
                                {
                                    var showEntitlement = (ShowEntitlement)entitlement;
                                    if (!(ShowIds.Contains(showEntitlement.CategoryId)))
                                    {
                                        if (!(showEntitlement.Show is LiveEvent))
                                        {
                                            ShowLookUpObject data = new ShowLookUpObject();
                                            var show = showEntitlement.Show;
                                            if (show != null)
                                            {
                                                if (show.StartDate < registDt && show.EndDate > registDt)
                                                {
                                                    var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentCategoriesCacheDuration, 0);
                                                    var parentCategories = show.GetAllParentCategories(CacheDuration);
                                                    var parent = context.CategoryClasses.Where(c => parentCategories.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible && c is Category);
                                                    data.Show = show.Description;
                                                    data.ShowId = show.CategoryId;
                                                    data.MainCategory = parent.First().Description;
                                                    data.MainCategoryId = parent.First().CategoryId;
                                                    data.ShowType = (show is Movie) ? "MOVIE" : "SHOW";
                                                    if (!(show is Movie))
                                                        ShowIds.Add(show.CategoryId);
                                                    list.Add(data);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (entitlement is EpisodeEntitlement)
                                {
                                    var episodeEntitlement = (EpisodeEntitlement)entitlement;
                                    var eCacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                                    var listOfShows = episodeEntitlement.Episode.GetParentShows(eCacheDuration);
                                    var parentShow = context.CategoryClasses.FirstOrDefault(c => listOfShows.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible && c is Show);
                                    if (parentShow != null)
                                    {
                                        if (!(ShowIds.Contains(parentShow.CategoryId)))
                                        {
                                            if (!(parentShow is LiveEvent))
                                            {
                                                if (parentShow.StartDate < registDt && parentShow.EndDate > registDt)
                                                {
                                                    ShowLookUpObject data = new ShowLookUpObject();
                                                    var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentCategoriesCacheDuration, 0);
                                                    var parentCategories = ((Show)parentShow).GetAllParentCategories(CacheDuration);
                                                    var parent = context.CategoryClasses.Where(c => parentCategories.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible && c is Category);
                                                    data.EpisodeId = episodeEntitlement.Episode.EpisodeId;
                                                    data.EpisodeName = episodeEntitlement.Episode.Description + ", " + episodeEntitlement.Episode.DateAired.Value.ToString("MMMM d, yyyy");
                                                    data.Show = parentShow.Description;
                                                    data.ShowId = parentShow.CategoryId;
                                                    data.MainCategory = parent.First().Description;
                                                    data.MainCategoryId = parent.First().CategoryId;
                                                    data.ShowType = (parentShow is Movie) ? "MOVIE" : "SHOW";

                                                    if (!(parentShow is Movie))
                                                    {
                                                        ShowIds.Add(parentShow.CategoryId);
                                                        list.Add(data);
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            list = list.OrderBy(c => c.Show).ToList();
                            //cache.Put(cacheKey, list, DataCache.CacheDuration);
                        }
                    }
                }
                catch (Exception e) { MyUtility.LogException(e); }
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ContentResult getUserData01(string uid)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            try { uid = uid.ToLower(); }
            catch (Exception) { }
            collection.Add("UID", uid);
            collection.Add("include", "profile,data,identities-active");
            //collection.Add("fields", "*");
            //GSResponse res = GigyaHelpers.createAndSendRequest("gcs.getUserData", GigyaHelpers.buildParameter(collection));
            GSResponse res = GigyaHelpers.createAndSendRequest("ids.getAccountInfo", GigyaHelpers.buildParameter(collection));
            return Content(res.GetData().ToJsonString(), "application/json");
        }
    }
}