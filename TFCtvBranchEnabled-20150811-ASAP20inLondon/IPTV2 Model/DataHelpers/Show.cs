using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class Show
    {
        public bool IsOnlineAllowed(string countryCode)
        {
            bool isAllowed = true;

            // check if explicitly allowed
            if ((OnlineAllowedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0 && c.StatusId == 1) > 0) || (OnlineAllowedCountries.Count(c => String.Compare(c.CountryCode, "--", true) == 0 && c.StatusId == 1) > 0))
            {
                isAllowed = true;
            }            
            else
            {
                // check if explicitly blocked
                if ((OnlineBlockedCountries.Count(c => c.CountryCode == "--" && c.StatusId == 1) > 0) || (OnlineBlockedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0 && c.StatusId == 1) > 0))
                {
                    isAllowed = false;
                }
                else if (OnlineAllowedCountries.Count(c => c.StatusId == 1) > 0 && OnlineBlockedCountries.Count(c => c.StatusId == 1) <= 0)
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
            if ((MobileAllowedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0 && c.StatusId == 1) > 0) || (MobileAllowedCountries.Count(c => String.Compare(c.CountryCode, "--", true) == 0 && c.StatusId == 1) > 0))
            {
                isAllowed = true;
            }
            else
            {
                // check if explicitly blocked
                if ((MobileBlockedCountries.Count(c => c.CountryCode == "--" && c.StatusId == 1) > 0) || (MobileBlockedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0 && c.StatusId == 1) > 0))
                {
                    isAllowed = false;
                }
                else if (MobileAllowedCountries.Count(c => c.StatusId == 1) > 0 && MobileBlockedCountries.Count(c => c.StatusId == 1) <= 0)
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
            if ((IptvAllowedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0 && c.StatusId == 1) > 0) || (IptvAllowedCountries.Count(c => String.Compare(c.CountryCode, "--", true) == 0 && c.StatusId == 1) > 0))
            {
                isAllowed = true;
            }
            else
            {
                // check if explicitly blocked
                if ((IptvBlockedCountries.Count(c => c.CountryCode == "--" && c.StatusId == 1) > 0) || (IptvBlockedCountries.Count(c => String.Compare(c.CountryCode, countryCode, true) == 0 && c.StatusId == 1) > 0))
                {
                    isAllowed = false;
                }
                else if (IptvAllowedCountries.Count(c => c.StatusId == 1) > 0 && IptvBlockedCountries.Count(c => c.StatusId == 1) <= 0)
                {
                    isAllowed = false;
                }
            }

            return (isAllowed);
        }

        public SortedSet<Package> GetPackages(Offering offering, String countryCode, RightsType rightsType)
        {
            var packages = new SortedSet<Package>();

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
                                isInPackage = package.Package.GetAllIptvShowIds(countryCode).Contains(CategoryId);
                                break;
                            case RightsType.Online:
                                isInPackage = package.Package.GetAllOnlineShowIds(countryCode).Contains(CategoryId);
                                break;
                            case RightsType.Mobile:
                                isInPackage = package.Package.GetAllMobileShowIds(countryCode).Contains(CategoryId);
                                break;
                            default:
                                throw new Exception("Invalid RightsType.");
                        }
                        if (isInPackage && package.Package is Package)
                        {
                            packages.Add((Package)package.Package);
                        }
                    }
                }

            }
            return (packages.Count > 0 ? packages : null);
        }

        public SortedSet<PackageSubscriptionProduct> GetPackageProducts(SortedSet<Package> packages)
        {
            var products = new SortedSet<PackageSubscriptionProduct>();

            foreach (var package in packages)
            {
                foreach (var packageProduct in package.Products)
                {
                    if (packageProduct.Product is PackageSubscriptionProduct)
                    {
                        products.Add((PackageSubscriptionProduct)packageProduct.Product);
                    }

                }
            }
            return (products.Count > 0 ? products : null);
        }


        public string GetPackageProductIdsCacheKey(int offeringId, String countryCode, RightsType rightsType)
        {
            return "SPPId:O:" + offeringId.ToString() + "Cat:" + CategoryId.ToString() + ";C:" + countryCode + ";R:" + rightsType.ToString();
        }

        public SortedSet<int> GetPackageProductIds(Offering offering, String countryCode, RightsType rightsType)
        {
            return GetPackageProductIds(offering, countryCode, rightsType, true);
        }

        public SortedSet<int> GetPackageProductIds(Offering offering, String countryCode, RightsType rightsType, bool useCache)
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
                var packageProducts = GetPackageProducts(offering, countryCode, rightsType);
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


        public string GetShowProductIdsCacheKey(int offeringId, String countryCode, RightsType rightsType)
        {
            return "SSPId:O:" + offeringId.ToString() + "Cat:" + CategoryId.ToString() + ";C:" + countryCode + ";R:" + rightsType.ToString();
        }

        public SortedSet<int> GetShowProductIds(Offering offering, String countryCode, RightsType rightsType)
        {
            return GetShowProductIds(offering, countryCode, rightsType, true);
        }

        public SortedSet<int> GetShowProductIds(Offering offering, String countryCode, RightsType rightsType, bool useCache)
        {
            SortedSet<int> showProductList = null;
            var cache = DataCache.Cache;
            var cacheKey = GetShowProductIdsCacheKey(offering.OfferingId, countryCode, rightsType);
            if (useCache)
            {
                showProductList = (SortedSet<int>)cache[cacheKey];
            }
            if (showProductList == null)
            {
                showProductList = new SortedSet<int>();
                var showProducts = GetShowProducts(offering, countryCode, rightsType);
                if (showProducts != null)
                    foreach (var showProduct in showProducts)
                    {
                        showProductList.Add(showProduct.ProductId);
                    }
                showProductList = showProductList.Count > 0 ? showProductList : null;

                if (useCache && showProductList != null)
                {
                    cache.Put(cacheKey, showProductList, DataCache.CacheDuration);
                }

            }
            return showProductList;
        }

        public SortedSet<PackageSubscriptionProduct> GetPackageProducts(Offering offering, String countryCode, RightsType rightsType)
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
                                isInPackage = package.Package.GetAllIptvShowIds(countryCode).Contains(CategoryId);
                                break;
                            case RightsType.Online:
                                isInPackage = package.Package.GetAllOnlineShowIds(countryCode).Contains(CategoryId);
                                break;
                            case RightsType.Mobile:
                                isInPackage = package.Package.GetAllMobileShowIds(countryCode).Contains(CategoryId);
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

        public SortedSet<PackageSubscriptionProduct> GetOnlinePackageProducts(Offering offering, String countryCode)
        {
            return GetPackageProducts(offering, countryCode, RightsType.Online);
        }

        public SortedSet<PackageSubscriptionProduct> GetMobilePackageProducts(Offering offering, String countryCode)
        {
            return GetPackageProducts(offering, countryCode, RightsType.Mobile);
        }

        public SortedSet<PackageSubscriptionProduct> GetIptvPackageProducts(Offering offering, String countryCode)
        {
            return GetPackageProducts(offering, countryCode, RightsType.IPTV);
        }


        public SortedSet<ShowSubscriptionProduct> GetShowProducts(Offering offering, String countryCode, RightsType rightsType)
        {
            var products = new SortedSet<ShowSubscriptionProduct>();

            if (StatusId == 1)
            {
                bool isAllowed = false;
                switch (rightsType)
                {
                    case RightsType.IPTV:
                        isAllowed = IsIptvAllowed(countryCode);
                        break;
                    case RightsType.Mobile:
                        isAllowed = IsMobileAllowed(countryCode);
                        break;
                    case RightsType.Online:
                        isAllowed = IsOnlineAllowed(countryCode);
                        break;
                    default:
                        throw new Exception("Invalid rightsType.");
                }
                if (isAllowed)
                {
                    var productShows = Products.Where(p => (p.Product.OfferingId == offering.OfferingId) && (p.Product.StatusId == 1));
                    foreach (var p in productShows)
                    {
                        products.Add(p.Product);
                    }
                }
            }

            return (products.Count > 0 ? products : null);

        }

        public SortedSet<ShowSubscriptionProduct> GetOnlineShowProducts(Offering offering, String countryCode)
        {
            return GetShowProducts(offering, countryCode, RightsType.Online);
        }

        public SortedSet<ShowSubscriptionProduct> GetMobileShowProducts(Offering offering, String countryCode)
        {
            return GetShowProducts(offering, countryCode, RightsType.Mobile);
        }

        public SortedSet<ShowSubscriptionProduct> GetIptvShowProducts(Offering offering, String countryCode)
        {
            return GetShowProducts(offering, countryCode, RightsType.IPTV);
        }

        public SortedSet<int> GetAllParentCategories(CategoryClass category, TimeSpan? CacheDuration = null)
        {
            SortedSet<int> parentCategoryList = null;

            var cache = DataCache.Cache;
            string cacheKey = "GAParentCat:Cat:" + category.CategoryId;
            parentCategoryList = (SortedSet<int>)cache[cacheKey];
            if (parentCategoryList == null)
            {
                parentCategoryList = new SortedSet<int>();
                var parentCategories = category.CategoryClassParentCategories;
                var categoryParentIds = parentCategories.Select(c => c.ParentId);
                parentCategoryList.UnionWith(categoryParentIds);
                foreach (var item in parentCategories)
                {
                    var categoryIds = GetAllParentCategories(item.ParentCategory, CacheDuration);
                    if (categoryIds != null)
                        parentCategoryList.UnionWith(categoryIds);
                }
                cache.Put(cacheKey, parentCategoryList, CacheDuration != null ? (TimeSpan)CacheDuration : DataCache.CacheDuration);
            }
            return (parentCategoryList);
        }

        public SortedSet<int> GetAllParentCategories(TimeSpan? CacheDuration = null)
        {
            SortedSet<int> parentCategoryList = null;
            var cache = DataCache.Cache;
            string cacheKey = "GAPRNTCAT:Cat:" + CategoryId;
            parentCategoryList = (SortedSet<int>)cache[cacheKey];
            if (parentCategoryList == null)
            {
                parentCategoryList = new SortedSet<int>();
                var parentCategories = CategoryClassParentCategories;
                var categoryParentIds = parentCategories.Select(c => c.ParentId);
                parentCategoryList.UnionWith(categoryParentIds);
                foreach (var item in parentCategories)
                {
                    var categoryIds = GetAllParentCategories(item.ParentCategory, CacheDuration);
                    if (categoryIds != null)
                        parentCategoryList.UnionWith(categoryIds);
                }
                cache.Put(cacheKey, parentCategoryList, CacheDuration != null ? (TimeSpan)CacheDuration : DataCache.CacheDuration);
            }
            return (parentCategoryList);
        }
    }
}
