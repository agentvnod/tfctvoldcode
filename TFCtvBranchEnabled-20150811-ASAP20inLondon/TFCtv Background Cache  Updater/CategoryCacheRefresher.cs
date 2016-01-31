using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace TFCtv_Background_Cache__Updater
{
    class CategoryCacheRefresher
    {
        public void FillCache(IPTV2Entities context, int offeringId, int serviceId, IEnumerable<int> categoryIds, RightsType rightsType, TimeSpan cacheDuration)
        {
            var countries = context.Countries;
            var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == offeringId);
            var service = offering.Services.FirstOrDefault(s => s.PackageId == serviceId);
            // fill cache of all categories
            foreach (int categoryId in categoryIds)
            {
                try
                {
                    var cat = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == categoryId && c is Category);
                    if (cat != null)
                    {
                        var category = (Category)cat;
                        foreach (var c in countries)
                        {
                            try
                            {
                                // loop and fill cache for 1 hour
                                var shows = service.GetAllShowIds(c.Code, category, rightsType, false);
                                var cacheKey = service.GetCacheKey(c.Code, category, rightsType);
                                DataCache.Cache.Put(cacheKey, shows, cacheDuration);
                            }
                            catch (Exception) { }
                        }
                    }
                }
                catch (Exception) { }
            }

        }
    }
}
