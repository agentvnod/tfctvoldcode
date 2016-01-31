using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using IPTV2_Model;
using TFCTV.Helpers;
using Microsoft.WindowsAzure.ServiceRuntime;
using TFCTV.Models;

namespace TFCtv_Background_Cache__Updater
{
    class SynapseCacheRefresher
    {
        int Visible = 1;
        string EpisodeImgPath = RoleEnvironment.GetConfigurationSettingValue("EpisodeImgPath");
        string ShowImgPath = RoleEnvironment.GetConfigurationSettingValue("ShowImgPath");
        string BlankGif = RoleEnvironment.GetConfigurationSettingValue("BlankGif");
        string AssetsBaseUrl = RoleEnvironment.GetConfigurationSettingValue("AssetsBaseUrl");
        string CelebrityImgPath = RoleEnvironment.GetConfigurationSettingValue("CelebrityImgPath");
        int LatestShows = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("LatestShowsFeatureId"));
        int FreeTvCategoryId = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("FreeTvCategoryId"));
        int FeaturedCelebrities = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("FeaturedCelebritiesFeatureId"));
        string CarouselImgPath = RoleEnvironment.GetConfigurationSettingValue("CarouselImgPath");
        int CarouselEntertainmentId = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("CarouselEntertainmentId"));
        bool IsSynapseFillCategoriesWithShowsEnabled = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("IsSynapseFillCategoriesWithShowsEnabled"));
        bool IsSynapseFillHomepageEnabled = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("IsSynapseFillHomepageEnabled"));

        public void FillCache(IPTV2Entities context, TimeSpan cacheDuration, IEnumerable<int> categoryIds, int offeringId, int serviceId)
        {
            DateTime registDt = DateTime.Now;
            try
            {
                Trace.TraceInformation("Fill Site Menu Cache - Loading...");
                FillSiteMenu(context, cacheDuration);
                Trace.TraceInformation("Fill Celebrity List - Loading...");
                FillCelebrityList(context, cacheDuration);
                if (IsSynapseFillHomepageEnabled)
                {
                    Trace.TraceInformation("Fill Homepage - Loading...");
                    FillHomePage(context, cacheDuration);
                }
                if (IsSynapseFillCategoriesWithShowsEnabled)
                {
                    Trace.TraceInformation("Fill Categories With Shows - Loading...");
                    FillCategoriesWithShows(context, cacheDuration, categoryIds, offeringId, serviceId);
                }
            }
            catch (Exception e) { Trace.TraceError("SynapseCacheRefresher Cache - Error! " + e.Message); }
        }

        private void FillSiteMenu(IPTV2Entities context, TimeSpan cacheDuration)
        {
            int[] mainMenuIds = { 736, 738, 737 };
            string cacheKey = "SYNGETSITEMENU:0:";
            try
            {
                List<MyMainMenu> siteMenus = new List<MyMainMenu>();
                foreach (int mainMenuId in mainMenuIds)
                {
                    Category mainCategory = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == mainMenuId);
                    List<MyMenu> fullMenu = new List<MyMenu>();
                    string id = String.Format("{0}MenuIds", mainCategory.Description);
                    string menuId = RoleEnvironment.GetConfigurationSettingValue(id);
                    var menuids = StringToIntList(menuId);
                    var features = context.Features.Where(f => menuids.Contains(f.FeatureId) && f.StatusId == Visible).ToList();

                    Dictionary<int, Feature> d = features.ToDictionary(x => x.FeatureId);
                    List<Feature> ordered = new List<Feature>();
                    foreach (var i in menuids)
                    {
                        if (d.ContainsKey(i))
                            ordered.Add(d[i]);
                    }

                    foreach (var feature in ordered)
                    {
                        var temp = feature.Description.Split('|');

                        var featureItems = feature.FeatureItems.Where(f => f.MobileStatusId == Visible);
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
                                        string img = String.IsNullOrEmpty(show.ImagePoster) ? AssetsBaseUrl + BlankGif : String.Format("{0}{1}/{2}", ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
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
                var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(siteMenus);                
                DataCache.Cache.Put(cacheKey, jsonString, cacheDuration);
            }
            catch (Exception) { }
        }

        private void FillCelebrityList(IPTV2Entities context, TimeSpan cacheDuration)
        {
            try
            {
                string cacheKey = "SYNGACELEB;0";
                List<CelebrityDisplay> celebrityList = new List<CelebrityDisplay>();
                var celebrities = context.Celebrities.Where(c => c.StatusId == Visible).OrderBy(c => c.LastName == null ? c.FirstName : c.LastName);
                //.OrderBy(c => c.LastName == null).ThenBy(c => c.LastName == null ? c);

                foreach (var c in celebrities)
                {
                    celebrityList.Add(new CelebrityDisplay()
                    {
                        CelebrityId = c.CelebrityId,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        ImageUrl = String.IsNullOrEmpty(c.ImageUrl) ? AssetsBaseUrl + BlankGif : String.Format("{0}{1}/{2}", CelebrityImgPath, c.CelebrityId.ToString(), c.ImageUrl)

                    });
                }
                var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(celebrityList);                
                DataCache.Cache.Put(cacheKey, jsonString, cacheDuration);
            }
            catch (Exception) { }
        }

        private void FillHomePage(IPTV2Entities context, TimeSpan cacheDuration)
        {
            var countries = context.Countries;
            foreach (var cx in countries)
            {
                try
                {
                    string cacheKey = "SYNAPSEGHOMEPAGE:O:;C:" + cx.Code;
                    SynapseHomepage homepage = new SynapseHomepage();
                    List<FeatureItem> featureItems = null;
                    var featuredShows = context.Features.FirstOrDefault(f => f.FeatureId == LatestShows && f.StatusId == Visible);
                    if (featuredShows != null)
                    {
                        List<SynapseShow> shows = new List<SynapseShow>();
                        var fItems = featuredShows.FeatureItems.Where(f => f.StatusId == Visible);
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
                                    if (show.IsMobileAllowed(cx.Code))
                                    {
                                        foreach (var item in show.ParentCategories.Where(p => p.CategoryId != FreeTvCategoryId))
                                        {
                                            parentId = item.CategoryId;
                                            parent = item.Description;

                                        }
                                        string img = String.IsNullOrEmpty(show.ImagePoster) ? AssetsBaseUrl + BlankGif : String.Format("{0}{1}/{2}", ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                        string banner = String.IsNullOrEmpty(show.ImageBanner) ? AssetsBaseUrl + BlankGif : String.Format("{0}{1}/{2}", ShowImgPath, show.CategoryId.ToString(), show.ImageBanner);
                                        SynapseShow s = new SynapseShow() { id = show.CategoryId, name = show.Description, blurb = show.Blurb, image = img, banner = banner, parent = parent, parentId = parentId };
                                        shows.Add(s);
                                    }
                                }
                            }
                        }

                        homepage.show = shows;
                    }

                    var featuredCelebrities = context.Features.FirstOrDefault(f => f.FeatureId == FeaturedCelebrities && f.StatusId == Visible);
                    if (featuredCelebrities != null)
                    {
                        List<SynapseCelebrity> celebrities = new List<SynapseCelebrity>();
                        var fCelebItems = featuredCelebrities.FeatureItems.Where(f => f.StatusId == Visible);
                        if (fCelebItems != null)
                        {
                            featureItems = fCelebItems.ToList();
                            foreach (var f in featureItems)
                            {
                                if (f is CelebrityFeatureItem)
                                {
                                    CelebrityFeatureItem cft = (CelebrityFeatureItem)f;
                                    Celebrity person = cft.Celebrity;
                                    string img = String.IsNullOrEmpty(person.ImageUrl) ? AssetsBaseUrl + BlankGif : String.Format("{0}{1}/{2}", CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
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

                    var mainCarousel = CarouselEntertainmentId; //hard-coded
                    Carousel carousel = context.Carousels.FirstOrDefault(c => c.CarouselId == mainCarousel && c.StatusId == Visible);
                    if (carousel != null)
                    {
                        var fSlides = carousel.CarouselSlides.Where(c => c.MobileStatusId == Visible).OrderByDescending(c => c.CarouselSlideId);
                        if (fSlides != null)
                        {
                            List<CarouselSlide> slides = fSlides.ToList();
                            var random = slides.OrderBy(x => System.Guid.NewGuid()).FirstOrDefault();
                            JsonCarouselItem item = new JsonCarouselItem() { CarouselSlideId = random.CarouselSlideId, BannerImageUrl = String.Format("{0}{1}/{2}", CarouselImgPath, random.CarouselSlideId.ToString(), random.BannerImageUrl), Blurb = random.Blurb, Name = random.Name, Header = random.Header, TargetUrl = random.TargetUrl, ButtonLabel = random.ButtonLabel };
                            homepage.carousel = item;
                        }
                    }

                    var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(homepage);                    
                    DataCache.Cache.Put(cacheKey, jsonString, cacheDuration);
                }
                catch (Exception e) { Trace.TraceError("FillHomePage Cache - Error! " + e.Message); }
            }
        }

        private void FillCategoriesWithShows(IPTV2Entities context, TimeSpan cacheDuration, IEnumerable<int> categoryIds, int offeringId, int serviceId)
        {
            var countries = context.Countries;
            foreach (int categoryId in categoryIds)
            {
                try
                {
                    foreach (var cx in countries)
                    {
                        string cacheKey = "SYNAPGTSHOWS:0;C:" + categoryId + ";CC:" + cx.Code;
                        SynapseCategory cat = new SynapseCategory();
                        var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == offeringId);
                        var service = offering.Services.FirstOrDefault(s => s.PackageId == serviceId);
                        var categoryClass = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == categoryId && c.StatusId == Visible);
                        if (categoryClass != null)
                        {
                            if (categoryClass is Category)
                            {
                                Category category = (Category)categoryClass;
                                SortedSet<int> showIds = service.GetAllMobileShowIds(cx.Code, category);

                                int[] setofShows = showIds.ToArray();
                                var list = context.CategoryClasses.Where(c => setofShows.Contains(c.CategoryId) && c.StatusId == Visible).OrderBy(c => c.CategoryName).ThenBy(c => c.StartDate).ToList();
                                var random = list.OrderBy(x => System.Guid.NewGuid()).FirstOrDefault();
                                cat.id = category.CategoryId;
                                cat.name = category.Description;
                                string featuredImg = String.IsNullOrEmpty(random.ImagePoster) ? AssetsBaseUrl + BlankGif : String.Format("{0}{1}/{2}", ShowImgPath, random.CategoryId.ToString(), random.ImagePoster);
                                string featuredBanner = String.IsNullOrEmpty(random.ImageBanner) ? AssetsBaseUrl + BlankGif : String.Format("{0}{1}/{2}", ShowImgPath, random.CategoryId.ToString(), random.ImageBanner);
                                SynapseShow feature = new SynapseShow() { id = random.CategoryId, name = random.Description, blurb = random.Blurb, image = featuredImg, banner = featuredBanner, dateairedstr = random.StartDate.Value.ToString("yyyy") };
                                cat.feature = feature;
                                List<SynapseShow> shows = new List<SynapseShow>();
                                foreach (var item in list)
                                {
                                    if (item is Show)
                                    {
                                        string img = String.IsNullOrEmpty(item.ImagePoster) ? AssetsBaseUrl + BlankGif : String.Format("{0}{1}/{2}", ShowImgPath, item.CategoryId.ToString(), item.ImagePoster);
                                        string banner = String.IsNullOrEmpty(item.ImageBanner) ? AssetsBaseUrl + BlankGif : String.Format("{0}{1}/{2}", ShowImgPath, item.CategoryId.ToString(), item.ImageBanner);
                                        SynapseShow show = new SynapseShow() { id = item.CategoryId, name = item.Description, blurb = item.Blurb, image = img, banner = banner, dateairedstr = item.StartDate.Value.ToString("yyyy") };
                                        shows.Add(show);
                                    }
                                }
                                cat.shows = shows;
                            }
                        }
                        DataCache.Cache.Put(cacheKey, cat, cacheDuration);
                    }
                }
                catch (Exception e) { Trace.TraceError("FillCategoryWithShows Cache - Error! " + e.Message); }
            }
        }

        private static IEnumerable<int> StringToIntList(string str)
        {
            if (String.IsNullOrEmpty(str))
                yield break;

            foreach (var s in str.Split(','))
            {
                int num;
                if (int.TryParse(s, out num))
                    yield return num;
            }
        }
    }
}
