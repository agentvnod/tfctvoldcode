using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EngagementsModel;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Helpers
{
    public static class UserPackages
    {
        public static bool CheckUser_Entitlement(Show model)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return false;
            }
            else
            {
                using (IPTV2Entities context = new IPTV2Entities())
                {
                    Guid userID = new Guid(HttpContext.Current.User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userID);
                    var off = context.Offerings.Find(GlobalConfig.offeringId);
                    if (user.IsShowEntitled(off, model, RightsType.Online))
                    {
                        if (user.Entitlements.Where(t => t.OfferingId == GlobalConfig.offeringId && t.EndDate >= DateTime.Now).Count() > 0)
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
        }

        public static bool CheckUser_Entitlement(Episode model)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return false;
            }
            else
            {
                using (IPTV2Entities context = new IPTV2Entities())
                {
                    Guid userID = new Guid(HttpContext.Current.User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userID);
                    var off = context.Offerings.Find(GlobalConfig.offeringId);

                    if (user.IsEpisodeEntitled(off, model, model.PremiumAssets.FirstOrDefault().Asset, RightsType.Online))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckUser_ShowEntitled(int CategoryId)
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                IPTV2Entities context = new IPTV2Entities();
                System.Guid userId = new System.Guid(HttpContext.Current.User.Identity.Name);
                var user = context.Users.Find(userId);
                if (user == null)
                {
                    return false;
                }

                var show = context.CategoryClasses.FirstOrDefault(p => p.CategoryId == CategoryId && p.StatusId == GlobalConfig.Visible);
                var off = context.Offerings.Find(GlobalConfig.offeringId);

                if (show is Show)
                    return user.IsShowEntitled(off, (Show)show, RightsType.Online);

            }

            return false;

        }

        public static List<Entitlement> GetUser_EntitlementsList()
        {
            var context = new IPTV2Entities();
            var currentDate = DateTime.Now;
            List<Entitlement> entitlements = new List<Entitlement>();
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                entitlements.Clear();
                return entitlements;
            }
            else
            {
                try
                {
                    System.Guid userId = new System.Guid(HttpContext.Current.User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    entitlements = user.Entitlements.Where(t => t.OfferingId == GlobalConfig.offeringId && t.EndDate >= currentDate).ToList();
                }
                catch
                {
                    return null;
                }
                return entitlements;
            }
        }

        public static List<ProductGroup> GetUpgradableProductGroup(int productgroupid_toupgrade)
        {
            return GetUpgradableProductGroup(productgroupid_toupgrade, true);
        }

        public static List<ProductGroup> GetUpgradableProductGroup(int productgroupid_toupgrade, bool useCache)
        {

            var context = new IPTV2Entities();
            List<ProductGroup> upgradable_productgrouplist = null;
            var cache = DataCache.Cache;

            string cacheKey = "SUPG:PG:" + productgroupid_toupgrade.ToString();
            if (useCache)
            {
                upgradable_productgrouplist = (List<ProductGroup>)cache[cacheKey];
            }
            if (upgradable_productgrouplist == null)
            {
                upgradable_productgrouplist = context.ProductGroupUpgrades
                                              .Where(p => p.ProductGroupId == productgroupid_toupgrade)
                                              .Select(p => p.UpgradeToProductGroup).ToList();
                if (useCache)
                {
                    cache.Put(cacheKey, upgradable_productgrouplist, DataCache.CacheDuration);
                }
            }
            return upgradable_productgrouplist;
        }

    }

    public class ShowAlacarteProduct : SubscriptionProductC
    {
        public int? ALaCarteSubscriptionTypeId { get; set; }
        public string CurrencyCode { get; set; }
        public int CategoryId { get; set; }
        //for entitlement
        public DateTime? ExpiryDate { get; set; }
    }

    public class Product2
    {
        public int? ProductGroupId { get; set; }
        public int ProductId { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public int DurationInDays { get; set; }
        public string DurationType { get; set; }
        public SubscriptionProductC.ProductPrice ProductPrice { get; set; }
        public IEnumerable<SubscriptionProductC.ProductPrice> ProductPrices { get; set; }

        //for entitlement
        public DateTime ExpiryDate { get; set; }

    }

    public class ShowPackageGroupProduct
    {
        public int ProductGroupId { get; set; }
        public int PackageId { get; set; }
        public ICollection<Product2> Product2 { get; set; }

        //for entitlement
        public DateTime? ExpiryDate { get; set; }
    }

    [Serializable]
    public class ShowPackageProductPrices
    {
        public ShowPackageProductPrices LoadAllPackageAndProduct(int categoryId, string countryCode, bool useCache)
        {
            ShowPackageProductPrices showPackageProductPrices = null;
            var cache = DataCache.Cache;
            string cacheKey = "SGPP:Cat:" + categoryId.ToString() + ";C:" + countryCode;

            if (useCache)
            {
                //var cacheItem = DataCache.Cache.GetCacheItem(cacheKey);
                showPackageProductPrices = (ShowPackageProductPrices)cache[cacheKey];
            }

            if (showPackageProductPrices == null)
            {
                //create helper class for displaying content to the View
                List<ShowAlacarteProduct> showAlarcarteProducts = new List<ShowAlacarteProduct>();

                //create helper class for displaying content to the View
                List<ShowPackageGroupProduct> showPackageGroupProductList = new List<ShowPackageGroupProduct>();

                showPackageProductPrices = new ShowPackageProductPrices();
                var context = new IPTV2Entities();
                var offering = context.Offerings.Find(GlobalConfig.offeringId);
                string currencyCode = MyUtility.GetCurrencyOrDefault(countryCode);
                var show = (Show)context.CategoryClasses.Find(categoryId);

                var packageProductIds = show.GetPackageProductIds(offering, countryCode, RightsType.Online);
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
                                                           ProductPrice = product2.ProductPrices.FirstOrDefault(p => p.CurrencyCode == currencyCode),
                                                           ProductGroupId = product2.ProductGroupId
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

                //Upgradable Products

                showPackageProductPrices.ShowAlacarteProductList = showAlarcarteProducts;
                showPackageProductPrices.ShowPackageGroupProductList = showPackageGroupProductList;

                if (useCache)
                {
                    //cache.Put(cacheKey, showPackageProductPrices, DataCache.CacheDuration);
                    var cacheDuration = new TimeSpan(0, GlobalConfig.PackageAndProductCacheDuration, 0);
                    cache.Put(cacheKey, showPackageProductPrices, cacheDuration);
                }
            }

            return showPackageProductPrices;
        }


        public List<ShowAlacarteProduct> ShowAlacarteProductList { get; set; }
        public List<ShowPackageGroupProduct> ShowPackageGroupProductList { get; set; }
        public List<UpgradeablePackageGroupProduct_Display> UpgradeablePackageGroupProductList { get; set; }
    }



    public class PackageProductPrices
    {
        public PackageProductPrices LoadAllPackageAndProduct(IEnumerable<int> packageIds, string countryCode, bool useCache)
        {

            PackageProductPrices packageProductPrices = null;
            var cache = DataCache.Cache;
            string cacheKey = "PKGPPRD:C:" + countryCode;

            if (useCache)
                packageProductPrices = (PackageProductPrices)cache[cacheKey];


            if (packageProductPrices == null)
            {

                //create helper class for displaying content to the View
                List<ShowPackageGroupProduct> showPackageGroupProductList = new List<ShowPackageGroupProduct>();

                packageProductPrices = new PackageProductPrices();
                var context = new IPTV2Entities();
                var offering = context.Offerings.Find(GlobalConfig.offeringId);
                string currencyCode = MyUtility.GetCurrencyOrDefault(countryCode);

                var packageProductIds = context.ProductPackages.Where(p => packageIds.Contains(p.PackageId)).Select(p => p.ProductId);


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
                                                           ProductPrice = product2.ProductPrices.FirstOrDefault(p => p.CurrencyCode == currencyCode) != null ? product2.ProductPrices.FirstOrDefault(p => p.CurrencyCode == currencyCode) : product2.ProductPrices.FirstOrDefault(p => p.CurrencyCode == GlobalConfig.DefaultCurrency)
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

                packageProductPrices.ShowPackageGroupProductList = showPackageGroupProductList;

                if (useCache)
                    cache.Put(cacheKey, packageProductPrices, DataCache.CacheDuration);
            }

            return packageProductPrices;

        }
        public List<ShowPackageGroupProduct> ShowPackageGroupProductList { get; set; }
    }



    //public class UserPackageEntitlement_Display
    //{
    //    public int PackageId { get; set; }
    //    public int ProductGroupId { get; set; }
    //    public string ProductGroupName { get; set; }
    //    public DateTime ExpiryDate { get; set; }
    //    public string PackageName { get; set; }       
    //    public List<Product2> Product2 { get; set; }

    //}

    //public class UserShowEntitlement_Display
    //{
    //    public int CategoryId { get; set; }
    //    public string ProductName { get; set; }
    //    public int? ALaCarteSubscriptionTypeId { get; set; }
    //    public int ProductId { get; set; }
    //    public int Duration { get; set; }
    //    public string DurationType { get; set; }
    //    public DateTime ExpiryDate { get; set; }
    //    public ICollection<SubscriptionProductC.ProductPrice> ProductPrice { get; set; }
    //}

    public class UpgradeablePackageGroupProduct_Display
    {
        public int ProductGroupId { get; set; }
        public ICollection<Product2> Product2 { get; set; }
    }


}