using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;
using IPTV2_Model;
using Newtonsoft.Json;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class MenuController : Controller
    {
        //
        // GET: /Menu/

        public string Build(int id)
        {
            List<CategoryD> cats = new List<CategoryD>();

            try
            {
                var context = new IPTV2Entities();
                var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId && s.StatusId == GlobalConfig.Visible);
                var maincategory = service.Categories.FirstOrDefault(c => c.Category.CategoryId == id);
                var subcategories = maincategory.Category.SubCategories.ToList();

                foreach (Category item in subcategories)
                {
                    var showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode(), item).ToArray();
                    int[] setofShows = showIds.ToArray();
                    var shows = context.CategoryClasses.Where(c => setofShows.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible).OrderByDescending(o => o.StartDate).Take(5);
                    List<ShowD> list = new List<ShowD>();

                    foreach (Show show in shows)
                    {
                        ShowD sh = new ShowD()
                        {
                            id = show.CategoryId,
                            name = MyUtility.Ellipsis(show.Description, 20)
                        };

                        list.Add(sh);
                    }

                    string s = JsonConvert.SerializeObject(list);
                    CategoryD cat = new CategoryD()
                    {
                        id = item.CategoryId,
                        name = item.Description,
                        shows = list
                    };

                    if (cat.shows.Count > 0)
                        cats.Add(cat);
                }
            }
            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
            return JsonConvert.SerializeObject(cats);
        }

        //[OutputCache(VaryByParam = "id", Duration = 180)]
        public List<MyMenu> BuildMenuGroup(string id)
        {
            List<MyMenu> fullMenu = null;
            string jsonString = String.Empty;
            try
            {
                var cache = DataCache.Cache;
                string cacheKey = "JRBMG:O:" + id + ";C:";// +MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                var registDt = DateTime.Now;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    fullMenu = new List<MyMenu>();
                    if (String.IsNullOrEmpty(id))
                        id = "Entertainment";
                    id = id + "MenuIds";
                    string menuId = Settings.GetSettings(id);
                    var menuIdList = MyUtility.StringToIntList(menuId);

                    var context = new IPTV2Entities();
                    var features = context.Features.Where(f => menuIdList.Contains(f.FeatureId) && f.StatusId == GlobalConfig.Visible).ToList();

                    Dictionary<int, Feature> d = features.ToDictionary(x => x.FeatureId);
                    List<Feature> ordered = new List<Feature>();
                    foreach (var i in menuIdList)
                    {
                        if (d.ContainsKey(i))
                            ordered.Add(d[i]);
                    }
                    foreach (var feature in ordered)
                    {
                        try
                        {
                            var temp = feature.Description.Split('|');
                            int takeAmount = 5;
                            try { takeAmount = temp.Length > 3 ? Convert.ToInt32(temp[3]) : 5; takeAmount = takeAmount > 0 ? takeAmount : 5; }
                            catch (Exception) { }
                            var featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).Take(takeAmount).ToList();
                            List<MyMenuShows> mms = new List<MyMenuShows>();
                            foreach (var f in featureItems.Where(fI => fI.StatusId == GlobalConfig.Visible))
                            {
                                if (f is ShowFeatureItem)
                                {
                                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == ((ShowFeatureItem)f).CategoryId);
                                    if (category != null)
                                    {
                                        if (category is Show && category.StartDate < registDt && category.EndDate > registDt && category.StatusId == GlobalConfig.Visible)
                                        {
                                            Show show = (Show)category;
                                            MyMenuShows m = new MyMenuShows()
                                            {
                                                name = show.Description,
                                                id = show.CategoryId
                                            };
                                            mms.Add(m);
                                        }
                                    }
                                }
                            }

                            MyMenu item = new MyMenu()
                            {
                                name = temp[0],
                                id = Convert.ToInt32(temp[1]),
                                //type = temp.Length > 3 ? Convert.ToInt32(temp[3]) : 0,
                                shows = mms
                            };
                            if (item.shows.Count > 0)
                                fullMenu.Add(item);
                        }
                        catch (Exception) { }
                    }
                    var cacheDuration = new TimeSpan(0, GlobalConfig.MenuCacheDuration, 0);
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(fullMenu);
                    cache.Put(cacheKey, jsonString, cacheDuration);
                }
                else
                    fullMenu = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MyMenu>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return fullMenu;
        }

        public PartialViewResult CreateMenu()
        {
            List<MyMainMenu> mainmenu = new List<MyMainMenu>();
            var menuItemList = GlobalConfig.MainMenuItems.Split(',');
            foreach (string item in menuItemList)
            {
                MyMainMenu menuItem = new MyMainMenu();
                menuItem.name = item;
                menuItem.menu = BuildMenuGroup(item);
                mainmenu.Add(menuItem);
            }

            return PartialView("_Menu", mainmenu);
        }

        //added June 21 2013 -Nads
        [OutputCache(VaryByParam = "id", Duration = 300)]
        public JsonResult GetPreviewEpisodes(int? id)
        {
            List<JsonFeatureItem> list = null;
            if (id != null)
            {
                var cache = DataCache.Cache;
                string cacheKey = "CATGPEKS:O:" + id;
                list = (List<JsonFeatureItem>)cache[cacheKey];
                if (list == null)
                {
                    try
                    {
                        DateTime registDt = DateTime.Now;
                        var context = new IPTV2Entities();
                        var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                        if (category != null)
                        {
                            if (category is Show)
                            {
                                Show show = (Show)category;
                                var episodesCats = context.EpisodeCategories1.Where(e => e.CategoryId == id).Select(ec => ec.EpisodeId);
                                var episodes = context.Episodes.Where(e => episodesCats.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt)
                                    .OrderByDescending(ec => ec.DateAired).Take(2);
                                if (episodes != null)
                                {
                                    var epCount = episodes.Count();
                                    var lengthOfBlurb = epCount > 1 ? GlobalConfig.MenuShowBlurbLength : GlobalConfig.MenuMovieBlurbLength;
                                    list = new List<JsonFeatureItem>();
                                    foreach (var ep in episodes)
                                    {
                                        string img = String.IsNullOrEmpty(ep.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ep.EpisodeId, ep.ImageAssets.ImageVideo);
                                        string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId, show.ImagePoster);
                                        var obj = new JsonFeatureItem()
                                        {
                                            EpisodeId = ep.EpisodeId,
                                            Blurb = epCount > 1 || (show is LiveEvent) ? MyUtility.Ellipsis(ep.Synopsis, lengthOfBlurb) : MyUtility.Ellipsis(show.Blurb, lengthOfBlurb),
                                            EpisodeAirDate = (ep.DateAired != null) ? ep.DateAired.Value.ToString("MMMM d, yyyy") : "",
                                            EpisodeImageUrl = img,
                                            ShowImageUrl = showImg
                                        };
                                        list.Add(obj);
                                    }
                                    cache.Put(cacheKey, list, DataCache.CacheDuration);
                                }
                            }
                        }
                    }
                    catch (Exception e) { MyUtility.LogException(e); }
                }
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public PartialViewResult BuildUXMenu(string id = "Entertainment", bool isDesktop = true)
        {
            List<MyMenu> fullMenu = null;
            string jsonString = String.Empty;
            try
            {
                var cache = DataCache.Cache;
                string cacheKey = "UXBMG:O:" + id + ";C:";// +MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                var registDt = DateTime.Now;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    fullMenu = new List<MyMenu>();
                    if (String.IsNullOrEmpty(id))
                        id = "Entertainment";
                    id = id + "MenuIds";
                    string menuId = Settings.GetSettings(id);
                    var menuIdList = MyUtility.StringToIntList(menuId);

                    var context = new IPTV2Entities();
                    var features = context.Features.Where(f => menuIdList.Contains(f.FeatureId) && f.StatusId == GlobalConfig.Visible).ToList();

                    Dictionary<int, Feature> d = features.ToDictionary(x => x.FeatureId);
                    List<Feature> ordered = new List<Feature>();
                    foreach (var i in menuIdList)
                    {
                        if (d.ContainsKey(i))
                            ordered.Add(d[i]);
                    }
                    foreach (var feature in ordered)
                    {
                        try
                        {
                            var temp = feature.Description.Split('|');
                            MyMenu item = new MyMenu()
                            {
                                name = temp[0],
                                id = Convert.ToInt32(temp[1])
                            };
                            if (!String.IsNullOrEmpty(item.name)) item.name = item.name.TrimEnd();
                            fullMenu.Add(item);
                        }
                        catch (Exception) { }
                    }
                    var cacheDuration = new TimeSpan(0, GlobalConfig.MenuCacheDuration, 0);
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(fullMenu);
                    cache.Put(cacheKey, jsonString, cacheDuration);
                }
                else
                    fullMenu = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MyMenu>>(jsonString);
                ViewBag.IsDesktop = isDesktop;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(fullMenu);
        }
        public PartialViewResult BuildUXMenuV2(string id = "Entertainment", bool isDesktop = true)
        {
            List<MyMenu> fullMenu = null;
            string jsonString = String.Empty;
            try
            {
                var cache = DataCache.Cache;
                string cacheKey = "UXBMG:O:" + id + ";C:";// +MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                var registDt = DateTime.Now;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    fullMenu = new List<MyMenu>();
                    if (String.IsNullOrEmpty(id))
                        id = "Entertainment";
                    id = id + "MenuIds";
                    string menuId = Settings.GetSettings(id);
                    var menuIdList = MyUtility.StringToIntList(menuId);

                    var context = new IPTV2Entities();
                    var features = context.Features.Where(f => menuIdList.Contains(f.FeatureId) && f.StatusId == GlobalConfig.Visible).ToList();

                    Dictionary<int, Feature> d = features.ToDictionary(x => x.FeatureId);
                    List<Feature> ordered = new List<Feature>();
                    foreach (var i in menuIdList)
                    {
                        if (d.ContainsKey(i))
                            ordered.Add(d[i]);
                    }

                    var featureCountCorrection = 0;
                    var featurewithShowcount = 0;
                    foreach (var feature in ordered)
                    {
                        try
                        {
                            var temp = feature.Description.Split('|');
                            int takeAmount = 5;
                            try { takeAmount = temp.Length > 3 ? Convert.ToInt32(temp[3]) : 5; takeAmount = takeAmount > 0 ? takeAmount : 5; }
                            catch (Exception) { }
                            // get number of shows inside the category

                            MyMenu item = new MyMenu()
                            {
                                name = temp[0],
                                id = Convert.ToInt32(temp[1])
                            };
                            int showCount = takeAmount - 1;
                            try
                            {
                                var categoryId = Convert.ToInt32(temp[1]);
                                var countryCode = GlobalConfig.DefaultCountry;
                                var offering = context.Offerings.Find(GlobalConfig.offeringId);
                                var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                                if (service != null)
                                {
                                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == categoryId);
                                    var showIds = service.GetAllOnlineShowIds(countryCode, (Category)category);
                                    showCount = showIds.Count();
                                }
                            }
                            catch (Exception) { }
                            item.showcount = showCount;
                            if (!String.IsNullOrEmpty(item.name)) item.name = item.name.TrimEnd();
                            if (ordered.IndexOf(feature) < 4 || (ordered.IndexOf(feature) - 3) == featureCountCorrection)
                            {
                                var featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).ToList();
                                List<MyMenuShows> mms = new List<MyMenuShows>();
                                int ctr = 0;
                                foreach (var f in featureItems.Where(fI => fI.StatusId == GlobalConfig.Visible))
                                {
                                    if (f is ShowFeatureItem)
                                    {
                                        var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == ((ShowFeatureItem)f).CategoryId);
                                        if (category != null)
                                        {
                                            if (category is Show && category.StartDate < registDt && category.EndDate > registDt && category.StatusId == GlobalConfig.Visible)
                                            {
                                                Show show = (Show)category;
                                                MyMenuShows m = new MyMenuShows()
                                                {
                                                    name = show.Description,
                                                    id = show.CategoryId
                                                };
                                                if (takeAmount > ctr)
                                                { mms.Add(m); ctr++; }
                                                else ctr++;
                                            }
                                        }
                                    }
                                }
                                item.shows = mms;
                                if (item.shows == null || item.shows.Count == 0)
                                    featureCountCorrection += 1;
                                else
                                    featurewithShowcount += 1;

                            }
                            if (item.shows != null)
                            {
                                if (item.shows.Count > 0 && featurewithShowcount <= 4)
                                    fullMenu.Add(item);
                            }
                            else if (featurewithShowcount >= 4)
                                fullMenu.Add(item);

                        }
                        catch (Exception) { }
                    }
                    var cacheDuration = new TimeSpan(0, GlobalConfig.MenuCacheDuration, 0);
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(fullMenu);
                    cache.Put(cacheKey, jsonString, cacheDuration);
                }
                else
                    fullMenu = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MyMenu>>(jsonString);
                ViewBag.IsDesktop = isDesktop;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(fullMenu);
        }
    }
}