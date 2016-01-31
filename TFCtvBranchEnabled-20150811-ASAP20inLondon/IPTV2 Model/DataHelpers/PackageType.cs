using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// using Soss.Client;
// using Microsoft.ApplicationServer.Caching;


namespace IPTV2_Model
{
    public partial class PackageType : IComparable<PackageType>
    {
        public int DataCacheDuration { get; set; }

        public int CompareTo(PackageType other)
        {
            return (PackageId.CompareTo(other.PackageId));
        }

        public string GetChannelsCacheKey(string countryCode, RightsType rightsType)
        {
            return "GAChI:O:" + OfferingId.ToString() + ";P:" + PackageId.ToString() + ";C:" + countryCode + ";R:" + rightsType.ToString();
        }

        public string GetShowsCacheKey(string countryCode, RightsType rightsType)
        {
            return GetCacheKey(countryCode, rightsType);
        }

        public string GetCacheKey(string countryCode, RightsType rightsType)
        {
            return "GASI:O:" + OfferingId.ToString() + ";P:" + PackageId.ToString() + ";C:" + countryCode + ";R:" + rightsType.ToString();
        }

        public string GetShowsCacheKey(string countryCode, Category category, RightsType rightsType)
        {
            return GetCacheKey(countryCode, category, rightsType);
        }

        public string GetCacheKey(string countryCode, Category category, RightsType rightsType)
        {
            return "GASI:O:" + OfferingId.ToString() + ";P:" + PackageId.ToString() + ";C:" + countryCode + ";Cat:" + category.CategoryId.ToString() + ";R:" + rightsType.ToString();
        }

        public SortedSet<int> GetAllShowIds(string countryCode, RightsType rightsType)
        {
            return GetAllShowIds(countryCode, rightsType, true);
        }

        public SortedSet<int> GetAllShowIds(string countryCode, RightsType rightsType, bool useCache)
        {
            return GetAllShowIds(countryCode, rightsType, useCache, DataCache.CacheDuration);
        }

        public SortedSet<int> GetAllShowIds(string countryCode, RightsType rightsType, bool useCache, TimeSpan cacheDuration)
        {
            SortedSet<int> showList = null;

            var cache = DataCache.Cache;

            // string cacheKey = "GASI:O:" + OfferingId.ToString() + ";P:" + PackageId.ToString() + ";C:" + countryCode + ";R:" + rightsType.ToString();
            string cacheKey = GetCacheKey(countryCode, rightsType);
            if (useCache)
            {
                showList = (SortedSet<int>)cache[cacheKey];
            }

            if (showList == null)
            {
                showList = new SortedSet<int>();
                foreach (var c in Categories)
                {
                    var thisShowList = GetAllShowIds(countryCode, c.Category, rightsType, useCache, cacheDuration);
                    if (thisShowList != null)
                    {
                        showList.UnionWith(thisShowList);
                    }
                }
                if (useCache)
                {
                    cache.Put(cacheKey, showList, cacheDuration);
                }

            }
            return (showList);
        }

        public SortedSet<int> GetAllShowIds(string countryCode, Category category, RightsType rightsType)
        {
            return GetAllShowIds(countryCode, category, rightsType, true);
        }

        public SortedSet<int> GetAllShowIds(string countryCode, Category category, RightsType rightsType, bool useCache)
        {
            return GetAllShowIds(countryCode, category, rightsType, useCache, DataCache.CacheDuration);
        }

        public SortedSet<int> GetAllShowIds(string countryCode, Category category, RightsType rightsType, bool useCache, TimeSpan cacheDuration)
        {
            SortedSet<int> showList = null;
            var registDt = DateTime.Now;
            var cache = DataCache.Cache;
            // string cacheKey = "GASI:O:" + OfferingId.ToString() + ";P:" + PackageId.ToString() + ";C:" + countryCode + ";Cat:" + category.CategoryId.ToString() + ";R:" + rightsType.ToString();
            string cacheKey = GetCacheKey(countryCode, category, rightsType);

            if (useCache)
            {
                showList = (SortedSet<int>)cache[cacheKey];
            }

            if (showList == null)
            {
                showList = new SortedSet<int>();
                foreach (var c in category.SubCategories)
                {
                    var thisShowList = GetAllShowIds(countryCode, c, rightsType, useCache, cacheDuration);
                    if (thisShowList != null)
                    {
                        showList.UnionWith(thisShowList);
                    }
                }

                foreach (var s in category.Shows)
                {
                    if (s.StatusId == 1 && s.StartDate < registDt && s.EndDate > registDt) // && s.IsOnlineAllowed(countryCode))
                    {
                        bool isAllowed = false;
                        switch (rightsType)
                        {
                            case RightsType.IPTV:
                                isAllowed = s.IsIptvAllowed(countryCode);
                                break;

                            case RightsType.Online:
                                isAllowed = s.IsOnlineAllowed(countryCode);
                                break;

                            case RightsType.Mobile:
                                isAllowed = s.IsMobileAllowed(countryCode);
                                break;
                            default:
                                throw new Exception("Invalid RightsType");

                        }
                        if (isAllowed)
                        {
                            showList.Add(s.CategoryId);
                        }
                    }
                }

                if (useCache)
                {
                    cache.Put(cacheKey, showList, cacheDuration);
                }

            }

            return (showList);

        }

        public SortedSet<int> GetAllOnlineShowIds(string countryCode)
        {
            return GetAllShowIds(countryCode, RightsType.Online);
        }

        public SortedSet<int> GetAllOnlineShowIds(string countryCode, Category category)
        {
            return GetAllShowIds(countryCode, category, RightsType.Online);
        }

        public SortedSet<int> GetAllMobileShowIds(string countryCode)
        {
            return GetAllShowIds(countryCode, RightsType.Mobile);
        }

        public SortedSet<int> GetAllMobileShowIds(string countryCode, Category category)
        {
            return GetAllShowIds(countryCode, category, RightsType.Mobile);
        }

        public SortedSet<int> GetAllIptvShowIds(string countryCode)
        {
            return GetAllShowIds(countryCode, RightsType.IPTV);
        }

        public SortedSet<int> GetAllIptvShowIds(string countryCode, Category category)
        {
            return GetAllShowIds(countryCode, category, RightsType.IPTV);
        }

        public SortedSet<int> GetAllSubCategoryIds(TimeSpan? CacheDuration = null)
        {

            SortedSet<int> categoryList = null;

            var cache = DataCache.Cache;
            string cacheKey = "GASCI:O:" + OfferingId.ToString() + ";P:" + PackageId.ToString();
            categoryList = (SortedSet<int>)cache[cacheKey];

            if (categoryList == null)
            {
                categoryList = new SortedSet<int>();

                foreach (var c in Categories)
                {
                    var categoryIds = GetAllSubCategoryIds(c.Category);
                    if (categoryIds != null)
                    {
                        categoryList.UnionWith(categoryIds);
                    }
                }
                cache.Put(cacheKey, categoryList, CacheDuration != null ? (TimeSpan)CacheDuration : DataCache.CacheDuration);
                // cache[cacheKey] = categoryList;
            }
            return (categoryList);
        }

        public SortedSet<int> GetAllSubCategoryIds(Category category)
        {
            SortedSet<int> categoryList = null;

            var cache = DataCache.Cache;
            string cacheKey = "GASCI:O:" + OfferingId.ToString() + ";P:" + PackageId.ToString() + ";Cat:" + category.CategoryId.ToString();
            categoryList = (SortedSet<int>)cache[cacheKey];

            if (categoryList == null)
            {
                categoryList = new SortedSet<int>();
                categoryList.Add(category.CategoryId);
                foreach (var c in category.SubCategories)
                {
                    var categoryIds = GetAllSubCategoryIds(c);
                    if (categoryIds != null)
                    {
                        categoryList.UnionWith(categoryIds);
                    }
                }
                cache.Put(cacheKey, categoryList, DataCache.CacheDuration);
                // cache[cacheKey] = categoryList;
            }

            return (categoryList);
        }

        public SortedSet<int> GetAllChannelIds(string countryCode, RightsType rightsType, bool useCache, TimeSpan cacheDuration)
        {
            SortedSet<int> channelList = null;

            var cache = DataCache.Cache;


            string cacheKey = GetChannelsCacheKey(countryCode, rightsType);
            if (useCache)
            {
                channelList = (SortedSet<int>)cache[cacheKey];
            }

            if (channelList == null)
            {
                channelList = new SortedSet<int>();
                foreach (var c in Channels)
                {
                    var channel = c.Channel;
                    var currentDate = DateTime.Now;

                    bool isAllowed = false;
                    switch (rightsType)
                    {
                        case RightsType.Online:
                            isAllowed = (channel.OnlineEndDate >= currentDate) && channel.IsOnlineAllowed(countryCode);
                            break;
                        case RightsType.Mobile:
                            isAllowed = (channel.MobileEndDate >= currentDate) && channel.IsMobileAllowed(countryCode);
                            break;
                        case RightsType.IPTV:
                            isAllowed = (channel.EndDate >= currentDate) && channel.IsIptvAllowed(countryCode);
                            break;
                        default:
                            throw new Exception("Invalid rights type.");
                    }
                    if (isAllowed)
                    {
                        channelList.Add(c.ChannelId);
                    }
                }
                if (useCache)
                {
                    cache.Put(cacheKey, channelList, cacheDuration);
                }

            }
            return (channelList);
        }

        public SortedSet<int> GetAllChannelIds(string countryCode, RightsType rightsType, bool useCache)
        {
            return GetAllChannelIds(countryCode, rightsType, useCache, DataCache.CacheDuration);
        }

        public SortedSet<int> GetAllChannelIds(string countryCode, RightsType rightsType)
        {
            return GetAllChannelIds(countryCode, rightsType, true, DataCache.CacheDuration);
        }

        public SortedSet<int> GetAllOnlineChannelIds(string countryCode)
        {
            return GetAllChannelIds(countryCode, RightsType.Online);
        }

        public SortedSet<int> GetAllMobileChannelIds(string countryCode)
        {
            return GetAllChannelIds(countryCode, RightsType.Mobile);
        }

        public SortedSet<int> GetAllIptvChannelIds(string countryCode)
        {
            return GetAllChannelIds(countryCode, RightsType.IPTV);
        }


    }
}
