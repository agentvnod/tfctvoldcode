using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IPTV2_Model;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using TFCTV.Helpers;
using System.Diagnostics;

namespace TFCtv_Background_Cache__Updater
{
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

        public void FillCacheOfAllPackageAndProduct(IPTV2Entities context, int offeringId, string DefaultCurrency, RightsType rightsType, TimeSpan cacheDuration)
        {
            var categories = context.CategoryClasses.Where(c => c.StatusId == 1 && c is Show).ToList();
            var countries = context.Countries.ToList();
            var offering = context.Offerings.Find(offeringId);

            foreach (var category in categories)
            {
                var categoryId = category.CategoryId;
                foreach (var country in countries)
                {
                    try
                    {
                        var countryCode = country.Code;
                        string cacheKey = "SGPP:Cat:" + categoryId.ToString() + ";C:" + countryCode;
                        //create helper class for displaying content to the View
                        List<ShowAlacarteProduct> showAlarcarteProducts = new List<ShowAlacarteProduct>();

                        //create helper class for displaying content to the View
                        List<ShowPackageGroupProduct> showPackageGroupProductList = new List<ShowPackageGroupProduct>();

                        ShowPackageProductPrices showPackageProductPrices = new ShowPackageProductPrices();

                        string currencyCode = PackageCacheRefresher.GetCurrencyOrDefault(countryCode, DefaultCurrency);
                        var show = (Show)category;

                        var packageProductIds = show.GetPackageProductIds(offering, countryCode, RightsType.Online);
                        var showProductIds = show.GetShowProductIds(offering, countryCode, RightsType.Online);

                        var subscriptionProductC = SubscriptionProductC.LoadAll(context, offering.OfferingId)
                                                       .Where(p => p.IsAllowed(countryCode));


                        string AlaCarteCacheKey = "SALACARTEPRD:Cat:" + categoryId.ToString() + ";C:" + countryCode;
                        var cacheItem = DataCache.Cache.GetCacheItem(AlaCarteCacheKey);
                        if (cacheItem != null)
                        {
                            showAlarcarteProducts = (List<ShowAlacarteProduct>)DataCache.Cache[AlaCarteCacheKey];
                        }
                        else
                        {
                            if (showProductIds != null)
                            {
                                var showProducts = from product in subscriptionProductC
                                                   join id in showProductIds
                                                   on product.ProductId equals id
                                                   where product.IsForSale
                                                   select product;


                                //show product with alacarte subscription
                                var aLaCarteProducts = from showproducts in showProducts
                                                       join subcription in show.Products
                                                       on showproducts.ProductId equals subcription.ProductId
                                                       select new
                                                       {
                                                           ALaCarteSubscriptionTypeId = subcription.Product.ALaCarteSubscriptionTypeId,
                                                           ProductPrices = showproducts.ProductPrices,
                                                           Duration = showproducts.Duration,
                                                           DurationInDays = showproducts.DurationInDays,
                                                           DurationType = showproducts.DurationType,
                                                           ProductId = showproducts.ProductId,
                                                           CategoryId = subcription.CategoryId
                                                       };


                                foreach (var item in aLaCarteProducts)
                                {
                                    showAlarcarteProducts.Add(new ShowAlacarteProduct
                                    {
                                        ALaCarteSubscriptionTypeId = item.ALaCarteSubscriptionTypeId,
                                        Duration = item.Duration,
                                        DurationInDays = item.DurationInDays,
                                        DurationType = item.DurationType,
                                        CurrencyCode = currencyCode,
                                        ProductId = item.ProductId,
                                        CategoryId = item.CategoryId,
                                        ProductPrices = item.ProductPrices.Where(p => p.CurrencyCode == currencyCode).ToList()
                                    });
                                }
                            }
                        }

                        string PackageCacheKey = "SPKGPRD:Cat:" + categoryId.ToString() + ";C:" + countryCode;
                        var PackageCacheItem = DataCache.Cache.GetCacheItem(PackageCacheKey);
                        if (PackageCacheItem != null)
                        {
                            showPackageGroupProductList = (List<ShowPackageGroupProduct>)DataCache.Cache[PackageCacheKey];
                        }
                        else
                        {
                            if (packageProductIds != null)
                            {
                                //join the packageproduct to get the packageID 
                                var packageProducts = from product in subscriptionProductC
                                                      join id in packageProductIds
                                                      on product.ProductId equals id
                                                      where product.IsForSale
                                                      select product;

                                var packageGroupProducts = from product in packageProducts
                                                           group product by product.ProductGroupId into ProductGroup
                                                           orderby ProductGroup.Key
                                                           select new
                                                           {
                                                               GroupId = ProductGroup.Key,
                                                               Products =
                                                               (
                                                                   from product2 in ProductGroup
                                                                   orderby product2.DurationInDays descending
                                                                   select new Product2
                                                                   {
                                                                       ProductId = product2.ProductId,
                                                                       Description = product2.Description,
                                                                       Duration = product2.Duration,
                                                                       DurationInDays = product2.DurationInDays,
                                                                       DurationType = product2.DurationType,
                                                                       ProductPrice = product2.ProductPrices.FirstOrDefault(p => p.CurrencyCode == currencyCode)
                                                                   }
                                                               )
                                                           };

                                foreach (var item in packageGroupProducts)
                                {
                                    showPackageGroupProductList.Add(new ShowPackageGroupProduct
                                    {
                                        ProductGroupId = item.GroupId,
                                        Product2 = item.Products.ToList()
                                    });
                                }
                            }
                        }
                        //Upgradable Products
                        showPackageProductPrices.ShowAlacarteProductList = showAlarcarteProducts;
                        showPackageProductPrices.ShowPackageGroupProductList = showPackageGroupProductList;
                        DataCache.Cache.Put(cacheKey, showPackageProductPrices, cacheDuration);
                    }
                    catch (Exception e) { Trace.TraceInformation(e.Message); }
                }
            }
        }

        public void FillCacheOfAllAlaCarteProducts(IPTV2Entities context, int offeringId, string DefaultCurrency, RightsType rightsType, TimeSpan cacheDuration)
        {
            var categories = context.CategoryClasses.Where(c => c.StatusId == 1 && c is Show).ToList();
            var countries = context.Countries.ToList();
            var offering = context.Offerings.Find(offeringId);

            foreach (var category in categories)
            {
                var categoryId = category.CategoryId;
                foreach (var country in countries)
                {
                    try
                    {
                        var countryCode = country.Code;
                        string cacheKey = "SALACARTEPRD:Cat:" + categoryId.ToString() + ";C:" + countryCode;

                        List<ShowAlacarteProduct> showAlarcarteProducts = new List<ShowAlacarteProduct>();

                        string currencyCode = PackageCacheRefresher.GetCurrencyOrDefault(countryCode, DefaultCurrency);
                        var show = (Show)category;

                        var showProductIds = show.GetShowProductIds(offering, countryCode, RightsType.Online);

                        var subscriptionProductC = SubscriptionProductC.LoadAll(context, offering.OfferingId)
                                                       .Where(p => p.IsAllowed(countryCode));

                        if (showProductIds != null)
                        {
                            var showProducts = from product in subscriptionProductC
                                               join id in showProductIds
                                               on product.ProductId equals id
                                               where product.IsForSale
                                               select product;


                            //show product with alacarte subscription
                            var aLaCarteProducts = from showproducts in showProducts
                                                   join subcription in show.Products
                                                   on showproducts.ProductId equals subcription.ProductId
                                                   select new
                                                   {
                                                       ALaCarteSubscriptionTypeId = subcription.Product.ALaCarteSubscriptionTypeId,
                                                       ProductPrices = showproducts.ProductPrices,
                                                       Duration = showproducts.Duration,
                                                       DurationInDays = showproducts.DurationInDays,
                                                       DurationType = showproducts.DurationType,
                                                       ProductId = showproducts.ProductId,
                                                       CategoryId = subcription.CategoryId
                                                   };


                            foreach (var item in aLaCarteProducts)
                            {
                                showAlarcarteProducts.Add(new ShowAlacarteProduct
                                {
                                    ALaCarteSubscriptionTypeId = item.ALaCarteSubscriptionTypeId,
                                    Duration = item.Duration,
                                    DurationInDays = item.DurationInDays,
                                    DurationType = item.DurationType,
                                    CurrencyCode = currencyCode,
                                    ProductId = item.ProductId,
                                    CategoryId = item.CategoryId,
                                    ProductPrices = item.ProductPrices.Where(p => p.CurrencyCode == currencyCode).ToList()
                                });
                            }
                        }

                        DataCache.Cache.Put(cacheKey, showAlarcarteProducts, cacheDuration);
                    }
                    catch (Exception e) { Trace.TraceInformation(e.Message); }
                }
            }

        }

        public void FillCacheOfAllPackageProducts(IPTV2Entities context, int offeringId, string DefaultCurrency, RightsType rightsType, TimeSpan cacheDuration)
        {

            var categories = context.CategoryClasses.Where(c => c.StatusId == 1 && c is Show).ToList();
            var countries = context.Countries.ToList();
            var offering = context.Offerings.Find(offeringId);

            foreach (var category in categories)
            {
                var categoryId = category.CategoryId;
                foreach (var country in countries)
                {
                    try
                    {
                        var countryCode = country.Code;
                        string cacheKey = "SPKGPRD:Cat:" + categoryId.ToString() + ";C:" + countryCode;

                        List<ShowPackageGroupProduct> showPackageGroupProductList = new List<ShowPackageGroupProduct>();
                        string currencyCode = PackageCacheRefresher.GetCurrencyOrDefault(countryCode, DefaultCurrency);
                        var show = (Show)category;

                        var packageProductIds = show.GetPackageProductIds(offering, countryCode, RightsType.Online);

                        var subscriptionProductC = SubscriptionProductC.LoadAll(context, offering.OfferingId)
                                                       .Where(p => p.IsAllowed(countryCode));

                        if (packageProductIds != null)
                        {
                            //join the packageproduct to get the packageID 
                            var packageProducts = from product in subscriptionProductC
                                                  join id in packageProductIds
                                                  on product.ProductId equals id
                                                  where product.IsForSale
                                                  select product;

                            var packageGroupProducts = from product in packageProducts
                                                       group product by product.ProductGroupId into ProductGroup
                                                       orderby ProductGroup.Key
                                                       select new
                                                       {
                                                           GroupId = ProductGroup.Key,
                                                           Products =
                                                           (
                                                               from product2 in ProductGroup
                                                               orderby product2.DurationInDays descending
                                                               select new Product2
                                                               {
                                                                   ProductId = product2.ProductId,
                                                                   Description = product2.Description,
                                                                   Duration = product2.Duration,
                                                                   DurationInDays = product2.DurationInDays,
                                                                   DurationType = product2.DurationType,
                                                                   ProductPrice = product2.ProductPrices.FirstOrDefault(p => p.CurrencyCode == currencyCode)
                                                               }
                                                           )
                                                       };

                            foreach (var item in packageGroupProducts)
                            {
                                showPackageGroupProductList.Add(new ShowPackageGroupProduct
                                {
                                    ProductGroupId = item.GroupId,
                                    Product2 = item.Products.ToList()
                                });
                            }
                        }
                        DataCache.Cache.Put(cacheKey, showPackageGroupProductList, cacheDuration);
                    }
                    catch (Exception e) { Trace.TraceInformation(e.Message); }
                }
            }
        }

        public void FillCacheOfAllSubscriptionProducts(IPTV2Entities context, int offeringId, RightsType rightsType, TimeSpan cacheDuration)
        {
            SortedSet<SubscriptionProductC> productCs = null;
            string cacheKey = "SPC:O:" + offeringId.ToString();

            var products = context.Products.Where(p => p.OfferingId == offeringId && p is SubscriptionProduct && p.StatusId == 1);
            if (products.Count() > 0)
            {
                productCs = new SortedSet<SubscriptionProductC>();
                foreach (var product in products)
                {
                    productCs.Add(SubscriptionProductC.Load(context, offeringId, product.ProductId, false));
                }

                DataCache.Cache.Put(cacheKey, productCs, cacheDuration);
            }
        }

        private static string GetCurrencyOrDefault(string CountryCode, string DefaultCurrency)
        {
            var context = new IPTV2Entities();
            string CurrencyCode = DefaultCurrency;
            IPTV2_Model.Country country = context.Countries.FirstOrDefault(c => c.Code == CountryCode);
            if (country != null)
                CurrencyCode = country.CurrencyCode;
            return CurrencyCode;
        }

    }
}