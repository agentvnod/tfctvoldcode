using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;
using Microsoft.WindowsAzure.ServiceRuntime;
using TFCTV.Helpers;
using System.Diagnostics;


namespace TFCtv_Background_Cache__Updater
{
    class MenuGroupCacheRefresher
    {
        int isVisible = 1;

        //public void FillCache(IPTV2Entities context, int offeringId, int serviceId, TimeSpan cacheDuration, string[] listOfMenuNames)
        //{
        //    List<MyMenu> fullMenu = null;
        //    //var MenuMovieBlurbLength = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("MenuMovieBlurbLength"));
        //    //var MenuShowBlurbLength = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("MenuShowBlurbLength"));
        //    //var EpisodeImgPath = RoleEnvironment.GetConfigurationSettingValue("EpisodeImgPath");
        //    //var ShowImgPath = RoleEnvironment.GetConfigurationSettingValue("ShowImgPath");
        //    //var BlankGif = RoleEnvironment.GetConfigurationSettingValue("BlankGif");
        //    //var AssetsBaseUrl = RoleEnvironment.GetConfigurationSettingValue("AssetsBaseUrl");
        //    DateTime registDt = DateTime.Now;
        //    try
        //    {
        //        foreach (var menuName in listOfMenuNames)
        //        {
        //            fullMenu = new List<MyMenu>();
        //            ///////////////////
        //            string menuId = String.Format("{0}MenuIds", menuName);
        //            var menuIdList = StringToIntList(RoleEnvironment.GetConfigurationSettingValue(menuId));
        //            var features = context.Features.Where(f => menuIdList.Contains(f.FeatureId) && f.StatusId == isVisible).ToList();
        //            Dictionary<int, Feature> d = features.ToDictionary(x => x.FeatureId);
        //            List<Feature> ordered = new List<Feature>();
        //            foreach (var i in menuIdList)
        //            {
        //                if (d.ContainsKey(i))
        //                    ordered.Add(d[i]);
        //            }

        //            foreach (var feature in ordered)
        //            {
        //                var temp = feature.Description.Split('|');
        //                int takeAmount = 5;
        //                try { takeAmount = temp.Length > 3 ? Convert.ToInt32(temp[3]) : 5; takeAmount = takeAmount > 0 ? takeAmount : 5; }
        //                catch (Exception) { }
        //                var featureItems = feature.FeatureItems.Where(f => f.StatusId == isVisible).Take(takeAmount).ToList();
        //                List<MyMenuShows> mms = new List<MyMenuShows>();
        //                foreach (var f in featureItems.Where(fI => fI.StatusId == isVisible))
        //                {
        //                    if (f is ShowFeatureItem)
        //                    {
        //                        var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == ((ShowFeatureItem)f).CategoryId);
        //                        if (category != null)
        //                        {
        //                            if (category is Show && category.StartDate < registDt && category.EndDate > registDt && category.StatusId == isVisible)
        //                            {
        //                                Show show = (Show)category;
        //                                MyMenuShows m = new MyMenuShows()
        //                                 {
        //                                     name = show.Description,
        //                                     id = show.CategoryId
        //                                 };
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

        //            ///////////////
        //            var cacheKey = GetMenuGroupCacheKey(menuName);
        //            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(fullMenu);
        //            IPTV2_Model.DataCache.Cache.Put(cacheKey, jsonString, cacheDuration);
        //        }
        //    }
        //    catch (Exception e) { Trace.TraceError("MenuGroupCacheRefresher Cache - Error! " + e.Message); }
        //}


        private static string GetMenuGroupCacheKey(string menuName)
        {
            return "UXBMG:O:" + menuName + ";C:";
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

        private static string Ellipsis(string text, int length)
        {
            if (text.Length <= length) return text;
            int pos = text.IndexOf(" ", length);
            if (pos >= 0)
                return text.Substring(0, pos) + "...";
            return text;
        }

        public void FillCache(IPTV2Entities context, int offeringId, int serviceId, TimeSpan cacheDuration, string[] listOfMenuNames)
        {
            List<MyMenu> fullMenu = null;
            //var MenuMovieBlurbLength = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("MenuMovieBlurbLength"));
            //var MenuShowBlurbLength = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("MenuShowBlurbLength"));
            //var EpisodeImgPath = RoleEnvironment.GetConfigurationSettingValue("EpisodeImgPath");
            //var ShowImgPath = RoleEnvironment.GetConfigurationSettingValue("ShowImgPath");
            //var BlankGif = RoleEnvironment.GetConfigurationSettingValue("BlankGif");
            //var AssetsBaseUrl = RoleEnvironment.GetConfigurationSettingValue("AssetsBaseUrl");
            DateTime registDt = DateTime.Now;
            try
            {
                foreach (var menuName in listOfMenuNames)
                {
                    fullMenu = new List<MyMenu>();
                    ///////////////////
                    string menuId = String.Format("{0}MenuIds", menuName);
                    var menuIdList = StringToIntList(RoleEnvironment.GetConfigurationSettingValue(menuId));
                    var features = context.Features.Where(f => menuIdList.Contains(f.FeatureId) && f.StatusId == isVisible).ToList();
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
                        var temp = feature.Description.Split('|');
                        int takeAmount = 5;
                        try { takeAmount = temp.Length > 3 ? Convert.ToInt32(temp[3]) : 5; takeAmount = takeAmount > 0 ? takeAmount : 5; }
                        catch (Exception) { }
                        MyMenu item = new MyMenu()
                        {
                            name = temp[0],
                            id = Convert.ToInt32(temp[1]),
                            type = temp.Length > 3 ? Convert.ToInt32(temp[3]) : 0
                        };
                        int showCount = takeAmount - 1;
                        try
                        {
                            var categoryId = Convert.ToInt32(temp[1]);
                            var countryCode = "US";
                            var offering = context.Offerings.Find(offeringId);
                            var service = offering.Services.FirstOrDefault(s => s.PackageId == serviceId);
                            if (service != null)
                            {
                                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == categoryId);
                                var showIds = service.GetAllOnlineShowIds(countryCode, (Category)category);
                                showCount = showIds.Count();
                            }
                        }
                        catch (Exception) { }
                        item.showcount = showCount;
                        if (ordered.IndexOf(feature) < 4 || (ordered.IndexOf(feature) - 3) == featureCountCorrection)
                        {
                            var featureItems = feature.FeatureItems.Where(f => f.StatusId == isVisible).ToList();
                            List<MyMenuShows> mms = new List<MyMenuShows>();
                            int ctr = 0;
                            foreach (var f in featureItems.Where(fI => fI.StatusId == isVisible))
                            {
                                if (f is ShowFeatureItem)
                                {
                                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == ((ShowFeatureItem)f).CategoryId);
                                    if (category != null)
                                    {
                                        if (category is Show && category.StartDate < registDt && category.EndDate > registDt && category.StatusId == isVisible)
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

                    ///////////////
                    var cacheKey = GetMenuGroupCacheKey(menuName);
                    var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(fullMenu);
                    IPTV2_Model.DataCache.Cache.Put(cacheKey, jsonString, cacheDuration);
                }
            }
            catch (Exception e) { Trace.TraceError("MenuGroupCacheRefresher Cache - Error! " + e.Message); }
        }
    }
}
