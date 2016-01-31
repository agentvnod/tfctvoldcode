using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;
using System.Diagnostics;

namespace TFCtv_Background_Cache__Updater
{
    class PackageFeatureCacheRefresher
    {
        int Visible = 1;
        public void FillCache(IPTV2Entities context, TimeSpan cacheDuration, int offeringId, int serviceId)
        {
            DateTime registDt = DateTime.Now;
            try
            {
                var countries = context.Countries;
                var listOfProductPackages = context.ProductPackages.Where(p => p.Product.IsForSale && p.Product.StatusId == Visible).Select(p => p.Package).Distinct();
                var offering = context.Offerings.Find(offeringId);
                var service = offering.Services.FirstOrDefault(o => o.PackageId == serviceId);
                foreach (var package in listOfProductPackages)
                {
                    foreach (var c in countries)
                    {
                        string cacheKey = "GPKGFEAT:P:" + package.PackageId + ";C:" + c.Code;
                        List<string> list = new List<string>();
                        SortedSet<int> listOfShowIds = new SortedSet<int>();
                        try
                        {
                            foreach (var category in package.Categories)
                            {
                                listOfShowIds.UnionWith(service.GetAllOnlineShowIds(c.Code, category.Category));
                                if (category.Category is Category)
                                {
                                    var item = (Category)category.Category;
                                    var CategoryShowIds = service.GetAllOnlineShowIds(c.Code, item);
                                    if (CategoryShowIds.Count() > 1000)
                                        list.Add(String.Format("{0}+ in {1}", Floor(CategoryShowIds.Count(), 100), item.Description));
                                    else if (CategoryShowIds.Count() > 100)
                                        list.Add(String.Format("{0}+ in {1}", Floor(CategoryShowIds.Count(), 10), item.Description));
                                    else if (CategoryShowIds.Count() > 10)
                                        list.Add(String.Format("{0}+ in {1}", Floor(CategoryShowIds.Count(), 10), item.Description));
                                    else
                                        list.Add(String.Format("{0} in {1}", CategoryShowIds.Count(), item.Description));
                                }
                            }
                            int showCount = listOfShowIds.Count();
                            if (showCount < 10)
                                list.Add(String.Format("{0} {1}", showCount, showCount == 1 ? "Title" : "Titles"));
                            else
                                list.Add(String.Format("{0}+ Titles", Floor(showCount, 10)));
                            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                            IPTV2_Model.DataCache.Cache.Put(cacheKey, jsonString, cacheDuration);
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception e) { Trace.TraceError("PackageFeatureCacheRefresher Cache - Error! " + e.Message); }
        }

        public int Floor(int i, int divisor)
        {
            return ((int)Math.Floor(i / new decimal(divisor))) * divisor;
        }
    }
}
