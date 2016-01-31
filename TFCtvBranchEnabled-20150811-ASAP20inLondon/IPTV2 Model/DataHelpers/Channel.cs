using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class Channel
    {
        public bool IsOnlineAllowed(string countryCode)
        {
            bool isAllowed = true;

            // check if explicitly allowed
            if (OnlineAllowedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0) > 0)
            {
                isAllowed = true;
            }
            else
            {
                // check if explicitly blocked
                if ((OnlineBlockedCountries.Count(c => c.CountryCode == "--") > 0) || (OnlineBlockedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0) > 0))
                {
                    isAllowed = false;
                }
            }

            return (isAllowed);
        }

        public bool IsMobileAllowed(string countryCode)
        {
            bool isAllowed = true;

            // check if explicitly allowed
            if (MobileAllowedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0) > 0)
            {
                isAllowed = true;
            }
            else
            {
                // check if explicitly blocked
                if ((MobileBlockedCountries.Count(c => c.CountryCode == "--") > 0) || (MobileBlockedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0) > 0))
                {
                    isAllowed = false;
                }
            }

            return (isAllowed);
        }

        public bool IsIptvAllowed(string countryCode)
        {
            bool isAllowed = true;

            // check if explicitly allowed
            if (IptvAllowedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0) > 0)
            {
                isAllowed = true;
            }
            else
            {
                // check if explicitly blocked
                if ((IptvBlockedCountries.Count(c => c.CountryCode == "--") > 0) || (IptvBlockedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0) > 0))
                {
                    isAllowed = false;
                }
            }

            return (isAllowed);
        }

        public string GetPackageProductIdsCacheKey(int offeringId, String countryCode, RightsType rightsType)
        {
            return "CPPId:O:" + offeringId.ToString() + "Ch:" + ChannelId.ToString() + ";C:" + countryCode + ";R:" + rightsType.ToString();
        }


        public SortedSet<int> GetPackageProductIds(Offering offering, String countryCode, RightsType rightsType)
        {
            return GetPackageProductIds(offering, countryCode, rightsType, true);
        }

        public SortedSet<int> GetPackageProductIds(Offering offering, String countryCode, RightsType rightsType, bool useCache)
        {
            return GetPackageProductIds(offering, countryCode, rightsType, useCache, DataCache.CacheDuration);
        }

        public SortedSet<int> GetPackageProductIds(Offering offering, String countryCode, RightsType rightsType, bool useCache, TimeSpan cacheDuration)
        {
            SortedSet<int> packageProductList = null;
            var cache = DataCache.Cache;
            var cacheKey = GetPackageProductIdsCacheKey(offering.OfferingId, countryCode, rightsType);
            if (useCache)
            {
                packageProductList = (SortedSet<int>)cache[cacheKey];
            }
            if (packageProductList == null)
            {
                packageProductList = new SortedSet<int>();
                var packageProducts = GetPackageProducts(offering, countryCode, rightsType, useCache, cacheDuration);
                if (packageProducts != null)
                {
                    foreach (var packageProduct in packageProducts)
                    {
                        packageProductList.Add(packageProduct.ProductId);
                    }
                }
                packageProductList = packageProductList.Count > 0 ? packageProductList : null;

                if (useCache && packageProductList != null)
                {
                    cache.Put(cacheKey, packageProductList, DataCache.CacheDuration);
                }

            }

            return packageProductList;
        }


        public SortedSet<PackageSubscriptionProduct> GetPackageProducts(Offering offering, String countryCode, RightsType rightsType, bool useCache, TimeSpan cacheDuration)
        {
            var products = new SortedSet<PackageSubscriptionProduct>();

            foreach (var p in offering.PackageSubscriptionProducts)
            {
                if (p.IsAllowed(countryCode))
                {
                    foreach (var package in p.Packages)
                    {
                        bool isInPackage = false;
                        switch (rightsType)
                        {
                            case RightsType.IPTV:
                                isInPackage = package.Package.GetAllChannelIds(countryCode, RightsType.IPTV, useCache, cacheDuration).Contains(ChannelId);
                                break;
                            case RightsType.Online:
                                isInPackage = package.Package.GetAllChannelIds(countryCode, RightsType.Online, useCache, cacheDuration).Contains(ChannelId);
                                break;
                            case RightsType.Mobile:
                                isInPackage = package.Package.GetAllChannelIds(countryCode, RightsType.Mobile, useCache, cacheDuration).Contains(ChannelId);
                                break;
                            default:
                                throw new Exception("Invalid RightsType.");
                        }
                        if (isInPackage)
                        {
                            products.Add(p);
                        }
                    }
                }

            }

            return (products.Count > 0 ? products : null);

        }


    }
}
