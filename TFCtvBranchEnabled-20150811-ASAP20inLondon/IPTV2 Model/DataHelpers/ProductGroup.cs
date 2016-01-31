using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class ProductGroup : IComparable<ProductGroup>
    {
        public int CompareTo(ProductGroup other)
        {
            return (ProductGroupId.CompareTo(other.ProductGroupId));
        }

        /// <summary>
        /// Get List of ProductGroups this Product Group is upgradeable TO
        /// </summary>
        /// <returns></returns>
        public List<ProductGroup> UpgradeableToProductGroups()
        {
            List<ProductGroup> productGroups = new List<ProductGroup>();
            foreach (var pu in ProductGroupUpgrades)
            {
                if (!productGroups.Contains(pu.UpgradeToProductGroup))
                    productGroups.Add(pu.UpgradeToProductGroup);
            }
            return (productGroups);
        }

        /// <summary>
        /// Get List of ProductGroups this Product Group is upgradeable FROM
        /// </summary>
        /// <returns></returns>
        public List<ProductGroup> UpgradeableFromProductGroups()
        {
            List<ProductGroup> productGroups = new List<ProductGroup>();
            foreach (var pu in UpgradeToProductGroupUpgrades)
            {
                if (!productGroups.Contains(pu.ProductGroup))
                    productGroups.Add(pu.ProductGroup);
            }
            return (productGroups);
        }


        /// <summary>
        /// Get list of Products this Product Group is upgradeable TO
        /// </summary>
        /// <returns></returns>
        public List<Product> UpgradeableToProducts()
        {
            List<Product> products = new List<Product>();
            var productGroups = UpgradeableToProductGroups();
            foreach (var pg in productGroups)
            {
                foreach (var p in pg.SubscriptionProducts)
                {
                    if (!products.Contains(p))
                        products.Add(p);
                }
            }
            return (products);
        }

        public string GetPackageIdsCacheKey()
        {
            return ("PgPackageIds:Pg:" + ProductGroupId.ToString());
        }
        public string GetShowIdsCacheKey()
        {
            return ("PgShowIds:Pg:" + ProductGroupId.ToString());
        }

        public SortedSet<int> GetPackageIds()
        {
            return GetPackageIds(true, DataCache.CacheDuration);
        }

        public SortedSet<int> GetPackageIds(bool useCache)
        {
            return GetPackageIds(useCache, DataCache.CacheDuration);
        }

        public SortedSet<int> GetPackageIds(bool useCache, TimeSpan cacheDuration)
        {
            string cacheKey = GetPackageIdsCacheKey();
            var cache = DataCache.Cache;
            SortedSet<int> packageIds = null;
            if (useCache)
            {
                packageIds = (SortedSet<int>)cache[cacheKey];
            }

            if (packageIds == null)
            {
                packageIds = new SortedSet<int>();
                var packageSubscriptionProducts = SubscriptionProducts.Where(p => p is PackageSubscriptionProduct);
                foreach (var p in packageSubscriptionProducts)
                {
                    var packageSubscriptionProduct = (PackageSubscriptionProduct)p;
                    foreach (var package in packageSubscriptionProduct.Packages)
                    {
                        packageIds.Add(package.PackageId);
                    }
                }
                cache.Put(cacheKey, packageIds, cacheDuration);
            }
            return packageIds;
        }


        public SortedSet<int> GetShowIds()
        {
            return GetShowIds(true);
        }

        public SortedSet<int> GetShowIds(bool useCache)
        {
            return GetShowIds(useCache, DataCache.CacheDuration);
        }

        public SortedSet<int> GetShowIds(bool useCache, TimeSpan cacheDuration)
        {
            string cacheKey = GetShowIdsCacheKey();
            var cache = DataCache.Cache;

            SortedSet<int> showIds = null;
            if (useCache)
            {
                showIds = (SortedSet<int>)cache[cacheKey];
            }

            if (showIds == null)
            {
                showIds = new SortedSet<int>();
                var showSubscriptionProducts = SubscriptionProducts.Where(p => p is ShowSubscriptionProduct);
                foreach (var s in showSubscriptionProducts)
                {
                    var showProduct = (ShowSubscriptionProduct)s;
                    foreach (var show in showProduct.Categories)
                    {
                        showIds.Add(show.CategoryId);
                    }
                }
                if (useCache)
                {
                    cache.Put(cacheKey, showIds, cacheDuration);
                }
            }
            return showIds;
        }

    }
}
