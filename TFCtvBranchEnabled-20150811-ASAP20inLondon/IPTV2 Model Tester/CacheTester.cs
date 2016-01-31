using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Configuration;
using System.Threading;
using TFCTV.Helpers;


namespace IPTV2_Model_Tester
{
    class CacheTester
    {
        public static void CacheTest()
        {
            var offeringId = 2;
            var context = new IPTV2Entities();
            // FillCache(context, offeringId, RightsType.Online);                                   
        }


        public static void ShowCacheExpirations()
        {
            var offeringId = 2;
            var serviceId = 46;
            var context = new IPTV2Entities();
            var rightsType = RightsType.Online;
            var packageList = context.PackageTypes.Where(p => p.OfferingId == offeringId).ToList();
            var countries = context.Countries.ToList();
            var offering = context.Offerings.Find(offeringId);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == serviceId);

            var categoryList = GetIdListFromString("740,746,752,758,755,749,1500,743,764,770,774,776,777,775,773,778,767");

            //fill cache of all packages
            foreach (var p in packageList)
            {
                foreach (var c in countries)
                {
                    var cacheKey = p.GetCacheKey(c.Code, rightsType);
                    var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                    if (cacheItem != null)
                    {

                        var timeOut = cacheItem.Timeout;
                        Console.WriteLine("Key:{0} TimeOut:{1}", cacheKey, timeOut);
                    }

                }
            }

            foreach (var categoryId in categoryList)
            {
                var cat = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == categoryId && c is Category);
                if (cat != null)
                {
                    var category = (Category)cat;
                    foreach (var c in countries)
                    {
                        var cacheKey = service.GetCacheKey(c.Code, category, rightsType);
                        var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                        if (cacheItem != null)
                        {
                            var timeOut = cacheItem.Timeout;
                            Console.WriteLine("Key:{0} TimeOut:{1}", cacheKey, timeOut);
                        }

                    }
                }

            }

        }

        public static void FillCache()
        {

            var context = new IPTV2Entities();
            int offeringId = 2;
            var rightsType = RightsType.Online;
            var cacheDuration = new TimeSpan(0, 60, 0);
            SubscriptionProductC.LoadAll(context, offeringId, true, cacheDuration);
            return;
            var packageList = context.PackageTypes.Where(p => p.OfferingId == offeringId).ToList();
            var countries = context.Countries.ToList();
            // var cacheDuration = new TimeSpan(0, 60, 0);
            // fill cache of all packages
            long totalSize = 0;
            foreach (var p in packageList)
            {
                foreach (var c in countries)
                {
                    // loop and fill cache for 1 hour
                    // fill show IDs
                    var shows = p.GetAllShowIds(c.Code, rightsType, false);
                    var cacheKey = p.GetCacheKey(c.Code, rightsType);
                    DataCache.Cache.Put(cacheKey, shows, cacheDuration);
                    totalSize += GetObjectSize(shows);

                    // fill channel IDs
                    cacheKey = p.GetChannelsCacheKey(c.Code, rightsType);
                    var channels = p.GetAllChannelIds(c.Code, rightsType, false);
                    DataCache.Cache.Put(cacheKey, channels, cacheDuration);
                }
            }

            var offering = context.Offerings.Find(offeringId);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == 46);
            int[] categoryIds = new int[] { 740, 746, 752, 758, 755, 749, 1500, 743, 764, 770, 774, 776, 777, 775, 773, 778, 767 };
            // fill cache of all categories
            foreach (int categoryId in categoryIds)
            {
                var cat = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == categoryId && c is Category);
                if (cat != null)
                {
                    var category = (Category)cat;
                    foreach (var c in countries)
                    {
                        // loop and fill cache for 1 hour
                        var shows = service.GetAllShowIds(c.Code, category, rightsType, false);
                        var cacheKey = service.GetCacheKey(c.Code, category, rightsType);
                        DataCache.Cache.Put(cacheKey, shows, cacheDuration);
                    }
                }
            }


        }

        private static long GetObjectSize(object obj)
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, obj);
            var size = ms.Length;
            ms.Dispose();
            return size;
        }

        public static IList<int> GetIdListFromString(string idList)
        {
            string[] values = idList.Split(',');

            List<int> ids = new List<int>(values.Length);

            foreach (string s in values)
            {
                int i;

                if (int.TryParse(s, out i))
                {
                    ids.Add(i);
                }
            }

            return ids;
        }

        public static void ShowMenuCacheExpiration()
        {

            string[] listOfMenuNames = new string[] { "Entertainment", "News", "Movies", "Live" };
            foreach (var menuName in listOfMenuNames)
            {
                var cacheKey = GetMenuGroupCacheKey(menuName);
                var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                if (cacheItem != null)
                {
                    var fullMenu = (List<MyMenu>)DataCache.Cache[cacheKey];
                    var timeOut = cacheItem.Timeout;
                    Console.WriteLine("Key:{0} TimeOut:{1}", cacheKey, timeOut);
                }
            }

        }
        public static void FillMenuCache()
        {
            var context = new IPTV2Entities();
            var cacheDuration = new TimeSpan(0, 60, 0);
            string[] listOfMenuNames = new string[] { "Entertainment", "News", "Movies", "Live" };
            List<MyMenu> fullMenu = null;

            foreach (var menuName in listOfMenuNames)
            {
                fullMenu = new List<MyMenu>();

                string menuId = String.Format("{0}MenuIds", menuName);
                var listOfIdsUnderMenuId = StringToIntList(ConfigurationManager.AppSettings[menuId]);
                var features = context.Features.Where(f => listOfIdsUnderMenuId.Contains(f.FeatureId) && f.StatusId == 1).ToList();
                Dictionary<int, Feature> d = features.ToDictionary(x => x.FeatureId);
                List<Feature> ordered = new List<Feature>();
                foreach (var i in listOfIdsUnderMenuId)
                {
                    if (d.ContainsKey(i))
                        ordered.Add(d[i]);
                }
                foreach (var feature in ordered)
                {
                    var temp = feature.Description.Split('|');

                    var featureItems = feature.FeatureItems.Where(f => f.StatusId == 1).ToList();
                    List<MyMenuShows> mms = new List<MyMenuShows>();
                    foreach (var f in featureItems)
                    {
                        if (f is ShowFeatureItem)
                        {
                            var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == ((ShowFeatureItem)f).CategoryId);
                            if (category != null)
                            {
                                if (category is Show)
                                {
                                    Show show = (Show)category;
                                    MyMenuShows m = new MyMenuShows() { name = show.Description, id = show.CategoryId };
                                    mms.Add(m);
                                }
                            }
                        }
                        else if (f is ChannelFeatureItem)
                        {
                            var channel = context.Channels.FirstOrDefault(c => c.ChannelId == ((ChannelFeatureItem)f).ChannelId);
                            if (channel != null)
                            {
                                if (channel is Channel)
                                {
                                    Channel ch = (Channel)channel;
                                    MyMenuShows m = new MyMenuShows() { id = ch.ChannelId, name = ch.Description, type = (int)MenuShow.Channel };
                                    mms.Add(m);
                                }
                            }
                        }
                    }

                    MyMenu item = new MyMenu()
                    {
                        name = temp[0],
                        id = Convert.ToInt32(temp[1]),
                        type = temp.Length > 3 ? Convert.ToInt32(temp[3]) : 0,
                        shows = mms
                    };

                    if (item.shows.Count > 0)
                        fullMenu.Add(item);
                }

                var cacheKey = GetMenuGroupCacheKey(menuName);
                DataCache.Cache.Put(cacheKey, fullMenu, cacheDuration);
            }
        }

        private static string GetMenuGroupCacheKey(string menuName)
        {
            return "JRBMG:O:" + menuName + ";C:";
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


    public class BackgroundCacheWorker
    {
        public static void FillCache()
        {

            int fillCacheDurationInMinutes = 60;

            string categoryIdsToCache = "740,746,752,758,755,749,1500,743,764,770,774,776,777,775,773,778,767";
            var categoryList = GetIdListFromString(categoryIdsToCache);
            var cacheDuration = new TimeSpan(0, fillCacheDurationInMinutes, 0);
            int offeringId = 2;
            int serviceId = 46;

            var context = new IPTV2Entities();

            //Console.WriteLine("Package Cache - Loading...");
            //var packageCacheRefresher = new PackageCacheRefresher();
            //packageCacheRefresher.FillCache(context, offeringId, RightsType.Online, cacheDuration);
            //Console.WriteLine("Package Cache - Loading Completed...");

            //Console.WriteLine("Category Cache - Loading...");
            //var categoryCacheRefresher = new CategoryCacheRefresher();
            //categoryCacheRefresher.FillCache(context, offeringId, serviceId, categoryList, RightsType.Online, cacheDuration);
            //Console.WriteLine("Category Cache - Loading Completed...");

            Console.WriteLine("SubscriptionProductC Cache - Loading...");
            var subscriptionProductCCacheRefresher = new SubscriptionProductCCacheRefresher();
            subscriptionProductCCacheRefresher.FillCache(context, offeringId, RightsType.Online, cacheDuration);
            Console.WriteLine("SubscriptionProductC Cache - Loading Completed...");


        }

        public static IList<int> GetIdListFromString(string idList)
        {
            string[] values = idList.Split(',');

            List<int> ids = new List<int>(values.Length);

            foreach (string s in values)
            {
                int i;

                if (int.TryParse(s, out i))
                {
                    ids.Add(i);
                }
            }

            return ids;
        }
    }
    public class CategoryCacheRefresher
    {
        public void FillCache(IPTV2Entities context, int offeringId, int serviceId, IEnumerable<int> categoryIds, RightsType rightsType, TimeSpan cacheDuration)
        {
            var countries = context.Countries.ToList();
            var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == offeringId);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == serviceId);
            // fill cache of all categories
            foreach (int categoryId in categoryIds)
            {
                var cat = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == categoryId && c is Category);
                if (cat != null)
                {
                    var category = (Category)cat;
                    foreach (var c in countries)
                    {
                        // loop and fill cache for 1 hour
                        var shows = service.GetAllShowIds(c.Code, category, rightsType, false);
                        var cacheKey = service.GetCacheKey(c.Code, category, rightsType);
                        DataCache.Cache.Put(cacheKey, shows, cacheDuration);
                    }
                }
            }

        }
    }

    //public class MenuGroupCacheRefresher
    //{
    //    int isVisible = 1;

    //    public void FillCache(IPTV2Entities context, int offeringId, int serviceId, TimeSpan cacheDuration, string[] listOfMenuNames)
    //    {

    //        List<MyMenu> fullMenu = null;

    //        foreach (var menuName in listOfMenuNames)
    //        {
    //            fullMenu = new List<MyMenu>();

    //            string menuId = String.Format("{0}MenuIds", menuName);
    //            // var listOfIdsUnderMenuId = StringToIntList(RoleEnvironment.GetConfigurationSettingValue(menuId));
    //            var features = context.Features.Where(f => listOfIdsUnderMenuId.Contains(f.FeatureId) && f.StatusId == isVisible).ToList();
    //            Dictionary<int, Feature> d = features.ToDictionary(x => x.FeatureId);
    //            List<Feature> ordered = new List<Feature>();
    //            foreach (var i in listOfIdsUnderMenuId)
    //            {
    //                if (d.ContainsKey(i))
    //                    ordered.Add(d[i]);
    //            }
    //            foreach (var feature in ordered)
    //            {
    //                var temp = feature.Description.Split('|');

    //                var featureItems = feature.FeatureItems.Where(f => f.StatusId == isVisible).ToList();
    //                List<MyMenuShows> mms = new List<MyMenuShows>();
    //                foreach (var f in featureItems)
    //                {
    //                    if (f is ShowFeatureItem)
    //                    {
    //                        var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == ((ShowFeatureItem)f).CategoryId);
    //                        if (category != null)
    //                        {
    //                            if (category is Show)
    //                            {
    //                                Show show = (Show)category;
    //                                MyMenuShows m = new MyMenuShows() { name = show.Description, id = show.CategoryId };
    //                                mms.Add(m);
    //                            }
    //                        }
    //                    }
    //                    else if (f is ChannelFeatureItem)
    //                    {
    //                        var channel = context.Channels.FirstOrDefault(c => c.ChannelId == ((ChannelFeatureItem)f).ChannelId);
    //                        if (channel != null)
    //                        {
    //                            if (channel is Channel)
    //                            {
    //                                Channel ch = (Channel)channel;
    //                                MyMenuShows m = new MyMenuShows() { id = ch.ChannelId, name = ch.Description, type = (int)MenuShow.Channel };
    //                                mms.Add(m);
    //                            }
    //                        }
    //                    }
    //                }

    //                MyMenu item = new MyMenu()
    //                {
    //                    name = temp[0],
    //                    id = Convert.ToInt32(temp[1]),
    //                    type = temp.Length > 3 ? Convert.ToInt32(temp[3]) : 0,
    //                    shows = mms
    //                };

    //                if (item.shows.Count > 0)
    //                    fullMenu.Add(item);
    //            }

    //            var cacheKey = GetMenuGroupCacheKey(menuName);
    //            DataCache.Cache.Put(cacheKey, fullMenu, cacheDuration);
    //        }
    //    }


    //    private static string GetMenuGroupCacheKey(string menuName)
    //    {
    //        return "JRBMG:O:" + menuName + ";C:";
    //    }
    //    private static IEnumerable<int> StringToIntList(string str)
    //    {
    //        if (String.IsNullOrEmpty(str))
    //            yield break;

    //        foreach (var s in str.Split(','))
    //        {
    //            int num;
    //            if (int.TryParse(s, out num))
    //                yield return num;
    //        }
    //    }
    //}

    public class PackageCacheRefresher
    {
        public void FillCache(IPTV2Entities context, int offeringId, RightsType rightsType, TimeSpan cacheDuration)
        {
            var packageList = context.PackageTypes.Where(p => p.OfferingId == offeringId).ToList();
            var countries = context.Countries.ToList();
            // fill cache of all packages
            long totalSize = 0;
            foreach (var p in packageList)
            {
                foreach (var c in countries)
                {
                    // loop and fill cache for 1 hour
                    // fill show IDs
                    var shows = p.GetAllShowIds(c.Code, rightsType, false);
                    var cacheKey = p.GetCacheKey(c.Code, rightsType);
                    DataCache.Cache.Put(cacheKey, shows, cacheDuration);
                    totalSize += GetObjectSize(shows);

                    // fill channel IDs
                    cacheKey = p.GetChannelsCacheKey(c.Code, rightsType);
                    var channels = p.GetAllChannelIds(c.Code, rightsType, false);
                    DataCache.Cache.Put(cacheKey, channels, cacheDuration);
                }
            }


        }

        private static long GetObjectSize(object obj)
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, obj);
            var size = ms.Length;
            ms.Dispose();
            return size;
        }

    }

    public class SubscriptionProductCCacheRefresher
    {
        public void FillCache(IPTV2Entities context, int offeringId, RightsType rightsType, TimeSpan cacheDuration)
        {
            SubscriptionProductC.LoadAll(context, offeringId, true, cacheDuration);
        }
    }

}
