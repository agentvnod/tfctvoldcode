using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EngagementsModel;
using IPTV2_Model;
using StackExchange.Profiling;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class ShowController : Controller
    {
        //
        // GET: /Show/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Show/Details/5

        public int GetMovieEpisodeId(int id)
        {
            IPTV2Entities context = new IPTV2Entities();
            int EpisodeId = 0;
            try
            {
                var registDt = DateTime.Now;
                try
                {
                    var t = context.EpisodeCategories1.Where(ep => ep.CategoryId == id && ep.Episode.OnlineStatusId == GlobalConfig.Visible && ep.Episode.OnlineStartDate < registDt && ep.Episode.OnlineEndDate > registDt).OrderByDescending(ep => ep.AuditTrail.CreatedOn).FirstOrDefault();
                    EpisodeId = t.EpisodeId;
                }
                catch (Exception) { }
                if (EpisodeId == 0)
                    EpisodeId = context.EpisodeCategories1.FirstOrDefault(ep => ep.CategoryId == id && ep.Episode.OnlineStatusId == GlobalConfig.Visible && ep.Episode.OnlineStartDate < registDt && ep.Episode.OnlineEndDate > registDt).EpisodeId;
            }
            catch (Exception)
            {
                return EpisodeId;
            }
            return EpisodeId;
        }

        public int GetLiveEventEpisodeId(int id)
        {
            int EpisodeId = 0;
            try
            {
                IPTV2Entities context = new IPTV2Entities();
                EpisodeId = context.EpisodeCategories1.FirstOrDefault(ep => ep.CategoryId == id && ep.Episode.OnlineStatusId == GlobalConfig.Visible).EpisodeId;
                if (EpisodeId > 0)
                {
                    var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == EpisodeId && e.OnlineStatusId == GlobalConfig.Visible);
                    if (episode != null)
                        if (episode.IsLiveChannelActive == true)
                            EpisodeId = episode.EpisodeId;
                }
            }
            catch (Exception) { }
            return EpisodeId;
        }

        ////[OutputCache(VaryByParam = "id", Duration = 300)]
        //public ActionResult DetailsBAK(int? id, string slug)
        //{
        //    var profiler = MiniProfiler.Current;

        //    if (id != null)
        //    {
        //        CategoryClass category = null;
        //        //var cache = DataCache.Cache;
        //        //string cacheKey = "SCCD:O:" + id.ToString() + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
        //        //category = (CategoryClass)cache[cacheKey];

        //        if (id == GlobalConfig.TFCkatCategoryId)
        //            return Redirect("/TFCkat");

        //        if (id == GlobalConfig.UAAPGreatnessNeverEndsCategoryId)
        //            return RedirectToAction("Index", "UAAP");
        //        var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
        //        if (excludedCategoryIds.Contains((int)id))
        //            return RedirectToAction("Index", "Home");

        //        var context = new IPTV2Entities();
        //        //if (category == null)
        //        //{
        //        category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
        //        //  cache.Put(cacheKey, category, DataCache.CacheDuration);
        //        //}

        //        //var  offering = context.Offerings.Find(GlobalConfig.offeringId);                
        //        //var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
        //        //var showIds = service.GetAllOnlineShowIds(MyUtility.GetCurrentCountryCodeOrDefault());
        //        //if(!showIds.Contains((int)id))

        //        if (category != null && category is Show)
        //        {
        //            var dbSlug = MyUtility.GetSlug(category.Description);
        //            if (String.Compare(dbSlug, slug, false) != 0)
        //                return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });

        //            var show = (Show)category;

        //            if (!ContextHelper.IsCategoryViewableInUserCountry(show))
        //                return RedirectToAction("Index", "Home");

        //            ViewBag.Loved = false;

        //            DateTime registDt = DateTime.Now;
        //            if (show.StartDate > registDt)
        //                return RedirectToAction("Index", "Home");
        //            if (show.EndDate < registDt)
        //                return RedirectToAction("Index", "Home");

        //            //var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
        //            //var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
        //            //SortedSet<int> showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode());

        //            //if (!showIds.Contains(show.CategoryId))
        //            //    return RedirectToAction("Index", "Home");
        //            using (profiler.Step("Episode Count"))
        //            {
        //                ViewBag.ShowId = show.CategoryId;
        //                //ViewBag.EpisodeCount = show.Episodes.Where(e => e.Episode.OnlineStatusId == GlobalConfig.Visible).Count();
        //                ViewBag.EpisodeCount = context.EpisodeCategories1.Count(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && e.Show.CategoryId == show.CategoryId);
        //            }



        //            //if (MyUtility.isUserLoggedIn())
        //            //{
        //            //    System.Guid userId = new System.Guid(User.Identity.Name);
        //            //    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //            //    if (user != null)
        //            //        ViewBag.EmailAddress = user.EMail;
        //            //}


        //            /**** Check for Free Trial ****/
        //            bool showFreeTrialImage = false;
        //            using (profiler.Step("Check for Early Bird"))
        //            {
        //                if (GlobalConfig.IsEarlyBirdEnabled)
        //                {
        //                    if (MyUtility.isUserLoggedIn())
        //                    {

        //                        var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
        //                        if (user != null)
        //                        {
        //                            ViewBag.EmailAddress = user.EMail;
        //                            var offering = context.Offerings.Find(GlobalConfig.offeringId);
        //                            if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
        //                                showFreeTrialImage = true;
        //                        }
        //                        //showFreeTrialImage = true;
        //                    }
        //                }

        //            }

        //            //REMOVE THIS LINE ON RELEASE OF EARLY BIRD.
        //            //showFreeTrialImage = false;
        //            ViewBag.ShowFreeTrialImage = showFreeTrialImage;


        //            bool isUserEntitled = false;
        //            if (MyUtility.isUserLoggedIn())
        //            {
        //                System.Guid userId = new System.Guid(User.Identity.Name);
        //                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //                if (user != null)
        //                    ViewBag.EmailAddress = user.EMail;
        //            }

        //            using (profiler.Step("Social Love"))
        //            {
        //                if (MyUtility.isUserLoggedIn() && HasSocialEngagementRecordOnShow(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, (int)id))
        //                    ViewBag.Loved = true;
        //            }

        //            using (profiler.Step("Social Rating"))
        //            {
        //                if (MyUtility.isUserLoggedIn() && HasSocialEngagementRecordOnShow(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_RATING, (int)id))
        //                    ViewBag.Rated = true;
        //            }


        //            //CHECK USER IF CAN PLAY VIDEO
        //            using (profiler.Step("Check If User Is Entitled"))
        //            {
        //                try
        //                {
        //                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
        //                    var ec = context.EpisodeCategories1.FirstOrDefault(c => c.CategoryId == id);
        //                    if (ec != null)
        //                    {
        //                        var episode = ec.Episode;
        //                        var premiumAsset = episode.PremiumAssets.FirstOrDefault();
        //                        if (premiumAsset != null)
        //                        {
        //                            var asset = premiumAsset.Asset;
        //                            //isUserEntitled = user.IsEpisodeEntitled(offering, episode, asset, RightsType.Online);
        //                            //isUserEntitled = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
        //                            isUserEntitled = ContextHelper.CanPlayVideo(context, offering, episode, asset, User, Request);
        //                        }
        //                    }
        //                }
        //                catch (Exception) { }
        //            }

        //            ViewBag.IsUserEntitled = isUserEntitled;


        //            ViewBag.ShowPackageProductPrices = null;
        //            string countryCode = MyUtility.GetCurrentCountryCodeOrDefault();
        //            ViewBag.CountryCode = countryCode;
        //            if (!isUserEntitled)
        //            {
        //                using (profiler.Step("Get Show Package & Product Prices"))
        //                {
        //                    try
        //                    {
        //                        ViewBag.ShowPackageProductPrices = ContextHelper.GetShowPackageProductPrices(show.CategoryId, countryCode);
        //                    }
        //                    catch (Exception e) { MyUtility.LogException(e); }
        //                }
        //            }

        //            using (profiler.Step("Checking if its a movie/special"))
        //            {
        //                if (show is Movie)
        //                {
        //                    ViewBag.HideEpisodeList = true;
        //                    ViewBag.CategoryType = "Movie";
        //                    //redirect to MovieVideo View
        //                    return View("MovieVideo", show);
        //                }
        //                else if (show is SpecialShow)
        //                {
        //                    ViewBag.HideEpisodeList = true;
        //                    ViewBag.CategoryType = "Special";

        //                    //redirect to MovieVideo View
        //                    return View("MovieVideo", show);
        //                }
        //                else if (show is LiveEvent)
        //                {
        //                    // transfer user to LiveEvent page format
        //                    ViewBag.HideEpisodeList = true;
        //                    ViewBag.CategoryType = "LiveEvent";

        //                    var episode = show.Episodes.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible);
        //                    if (episode != null)
        //                        return RedirectToAction("Details", "Live", new { id = episode.EpisodeId, slug = dbSlug });

        //                    //return View("LiveEvent", show);
        //                }
        //                //foreach (var item in show.ParentCategories.ToList())
        //                //{
        //                //    ViewBag.HideEpisodeList = true;
        //                //    string categories = item.Description + ", ";
        //                //    ViewBag.Category = categories.Trim().TrimEnd(',');

        //                //    if (item.CategoryClassParentCategories.Where(p => p.ParentId == GlobalConfig.Movies).Count() > 0)
        //                //    {
        //                //        ViewBag.CategoryType = "Movie";
        //                //        if (MyUtility.isUserLoggedIn())
        //                //        {
        //                //            System.Guid userId = new System.Guid(User.Identity.Name);
        //                //            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //                //            if (user != null)
        //                //                ViewBag.EmailAddress = user.EMail;
        //                //        }
        //                //        //redirect to MovieVideo View
        //                //        return View("MovieVideo", show);
        //                //    }
        //                //}
        //            }

        //            return View(show);
        //        }
        //    }
        //    return RedirectToAction("Index", "Home");
        //}

        private bool HasSocialEngagementRecordOnShow(Guid userId, int reactionTypeId, int categoryId)
        {
            var context = new EngagementsEntities();

            var show = context.ShowReactions.FirstOrDefault(s => s.CategoryId == categoryId && s.ReactionTypeId == reactionTypeId && s.UserId == userId);

            if (show == null)
                return false;

            return true;
        }

        [ChildActionOnly]
        [OutputCache(VaryByParam = "id", Duration = 300)]
        public PartialViewResult GetCasts(int? id)
        {
            var context = new IPTV2Entities();
            var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
            if (category is Show)
            {
                var celebrities = ((Show)category).CelebrityRoles.Where(cr => cr.Celebrity.StatusId == GlobalConfig.Visible).Select(c => c.Celebrity);
                return PartialView("_GetCasts", celebrities);
            }
            return null;
        }

        //[ChildActionOnly]
        //[OutputCache(VaryByParam = "*", Duration = 300)]
        //public PartialViewResult GetPackages(int categoryId, string countryCode, string ReturnUrl)
        //{
        //    ViewBag.ReturnUrl = ReturnUrl;
        //    ShowPackageProductPrices showPackageProductPrices = new ShowPackageProductPrices();
        //    try
        //    {
        //        showPackageProductPrices = showPackageProductPrices.LoadAllPackageAndProduct(categoryId, countryCode, true);
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    return PartialView("_GetShowProductPackages", showPackageProductPrices);
        //}


        //[ChildActionOnly]
        //public PartialViewResult GetPackages2(ShowPackageProductPrices showPackageProductPrices)
        //{
        //    return PartialView("_GetShowProductPackages", showPackageProductPrices);
        //}

        //public ActionResult GetUserEntitlements(int categoryId, string countryCode)
        //{
        //    try
        //    {
        //        string currencyCode = MyUtility.GetCurrencyOrDefault(countryCode);
        //        //create helper class for displaying content to the View
        //        List<ShowAlacarteProduct> showAlarcarteProducts = new List<ShowAlacarteProduct>();
        //        List<ShowPackageGroupProduct> showPackageGroupProductList = new List<ShowPackageGroupProduct>();

        //        ShowPackageProductPrices showPackageProductPrices = new ShowPackageProductPrices();

        //        var context = new IPTV2Entities();
        //        System.Guid userId = new System.Guid(HttpContext.User.Identity.Name);
        //        var user = context.Users.FirstOrDefault(u => u.UserId == userId);

        //        showPackageProductPrices = showPackageProductPrices.LoadAllPackageAndProduct(categoryId, countryCode, true);
        //        if (showPackageProductPrices == null)
        //            return null;
        //        else
        //        {
        //            showPackageGroupProductList = (from productGroups in showPackageProductPrices.ShowPackageGroupProductList
        //                                           select new ShowPackageGroupProduct
        //                                           {
        //                                               ExpiryDate = context.ProductGroups.Find(productGroups.ProductGroupId) == null ? null : (DateTime?)user.GetProductGroupExpiration(context.ProductGroups.Find(productGroups.ProductGroupId)),
        //                                               ProductGroupId = productGroups.ProductGroupId,
        //                                               PackageId = productGroups.PackageId,
        //                                               Product2 = productGroups.Product2
        //                                           }).ToList();
        //            if (showPackageGroupProductList == null)
        //            {
        //                showAlarcarteProducts = (from productGroups in showPackageProductPrices.ShowAlacarteProductList
        //                                         select new ShowAlacarteProduct
        //                                         {
        //                                             ExpiryDate = context.ProductGroups.Find(productGroups.ProductGroupId) == null ? null : (DateTime?)user.GetProductGroupExpiration(context.ProductGroups.Find(productGroups.ProductGroupId)),
        //                                             ProductGroupId = productGroups.ProductGroupId,
        //                                             ALaCarteSubscriptionTypeId = productGroups.ALaCarteSubscriptionTypeId,
        //                                             ProductPrices = productGroups.ProductPrices,
        //                                             Duration = productGroups.Duration,
        //                                             DurationInDays = productGroups.DurationInDays,
        //                                             DurationType = productGroups.DurationType,
        //                                             ProductId = productGroups.ProductId,
        //                                             CategoryId = productGroups.CategoryId
        //                                         }).ToList();
        //            }

        //            var subscriptionProductC = SubscriptionProductC.LoadAll(context, GlobalConfig.offeringId)
        //                                  .Where(p => p.IsAllowed(countryCode));
        //        }

        //        if (showPackageGroupProductList == null && showAlarcarteProducts == null)
        //            return null;
        //        ViewBag.UserPackageEntitlementList = showPackageGroupProductList;
        //        ViewBag.userShowEntitlement = showAlarcarteProducts;
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    //return Json(new { aLaCarteProducts = showAlarcarteProducts, showPackageGroupProductList = showPackageGroupProductList, upgarablePackageGroupProducts = upgarablePackageGroupProducts }, JsonRequestBehavior.AllowGet);
        //    return PartialView("_GetUserEntitlements");
        //}

        //[Authorize]
        //public ActionResult GetUserEntitlements_Scrap(int categoryId, string countryCode)
        //{
        //    //create helper class for displaying content to the View
        //    List<ShowAlacarteProduct> showAlarcarteProducts = new List<ShowAlacarteProduct>();

        //    //create helper class for displaying content to the View
        //    List<ShowPackageGroupProduct> showPackageGroupProductList = new List<ShowPackageGroupProduct>();

        //    var context = new IPTV2Entities();
        //    var offering = context.Offerings.Find(GlobalConfig.offeringId);
        //    string currencyCode = MyUtility.GetCurrencyOrDefault(countryCode);

        //    System.Guid userId = new System.Guid(HttpContext.User.Identity.Name);
        //    var user = context.Users.FirstOrDefault(u => u.UserId == userId);


        //    var userEntitlements = user.Entitlements.Where(t => t.OfferingId == GlobalConfig.offeringId && t.EndDate >= DateTime.Now);
        //    //get package user entitlement
        //    var userPackageEntitlementList = from userPackageEntitlemts in userEntitlements
        //                                     where userPackageEntitlemts is PackageEntitlement
        //                                     select (PackageEntitlement)userPackageEntitlemts;
        //    //get show user entitlement
        //    var userShowEntitlementList = from userShowEntitlements in userEntitlements
        //                                  where userShowEntitlements is ShowEntitlement
        //                                  select (ShowEntitlement)userShowEntitlements;



        //    var show = (Show)context.CategoryClasses.Find(categoryId);
        //    var packageProductIds = show.GetPackageProductIds(offering, countryCode, RightsType.Online);



        //    var showProductIds = show.GetShowProductIds(offering, countryCode, RightsType.Online);
        //    var subscriptionProductC = SubscriptionProductC.LoadAll(context, offering.OfferingId)
        //                               .Where(p => p.IsAllowed(countryCode));

        //    #region User Show Product
        //    if (showProductIds != null)
        //    {
        //        var showProducts = from product in subscriptionProductC
        //                           join id in showProductIds
        //                           on product.ProductId equals id
        //                           where product.IsForSale
        //                           select product;


        //        //show product with alacarte subscription
        //        var aLaCarteProducts = from showproducts in showProducts
        //                               join subcription in show.Products
        //                               on showproducts.ProductId equals subcription.ProductId
        //                               join usershowentitlement in userShowEntitlementList
        //                               on subcription.CategoryId equals usershowentitlement.CategoryId
        //                               select new
        //                               {
        //                                   ExpiryDate = user.GetProductGroupExpiration(context.ProductGroups.Find(showproducts.ProductGroupId)),
        //                                   ProductGroupId = showproducts.ProductGroupId,
        //                                   ALaCarteSubscriptionTypeId = subcription.Product.ALaCarteSubscriptionTypeId,
        //                                   ProductPrices = showproducts.ProductPrices,
        //                                   Duration = showproducts.Duration,
        //                                   DurationInDays = showproducts.DurationInDays,
        //                                   DurationType = showproducts.DurationType,
        //                                   ProductId = showproducts.ProductId,
        //                                   CategoryId = subcription.CategoryId
        //                               };


        //        foreach (var item in aLaCarteProducts)
        //        {
        //            showAlarcarteProducts.Add(new ShowAlacarteProduct
        //            {
        //                ExpiryDate = item.ExpiryDate,
        //                ALaCarteSubscriptionTypeId = item.ALaCarteSubscriptionTypeId,
        //                Duration = item.Duration,
        //                DurationInDays = item.DurationInDays,
        //                DurationType = item.DurationType,
        //                CurrencyCode = currencyCode,
        //                ProductId = item.ProductId,
        //                CategoryId = item.CategoryId,
        //                ProductPrices = item.ProductPrices.Where(p => p.CurrencyCode == currencyCode).ToList()
        //            });
        //        }
        //    }
        //    #endregion

        //    #region User Package Entitlement
        //    if (packageProductIds != null)
        //    {
        //        //join the packageproduct to get the packageID 
        //        var packageProducts = from product in subscriptionProductC
        //                              join id in packageProductIds
        //                              on product.ProductId equals id
        //                              where product.IsForSale
        //                              select product;


        //        var packageGroupProducts = from product in packageProducts
        //                                   group product by product.ProductGroupId into ProductGroup
        //                                   orderby ProductGroup.Key
        //                                   select new
        //                                   {
        //                                       GroupId = ProductGroup.Key,
        //                                       Products =
        //                                       (
        //                                           from product2 in ProductGroup
        //                                           orderby product2.DurationInDays descending
        //                                           select new Product2
        //                                           {
        //                                               ExpiryDate = user.GetProductGroupExpiration(context.ProductGroups.Find(product2.ProductGroupId)),
        //                                               ProductId = product2.ProductId,
        //                                               Description = product2.Description,
        //                                               Duration = product2.Duration,
        //                                               DurationInDays = product2.DurationInDays,
        //                                               DurationType = product2.DurationType,
        //                                               ProductPrice = product2.ProductPrices.FirstOrDefault(p => p.CurrencyCode == currencyCode)
        //                                           }
        //                                       )
        //                                   };

        //        foreach (var item in packageGroupProducts)
        //        {
        //            showPackageGroupProductList.Add(new ShowPackageGroupProduct
        //            {
        //                ProductGroupId = item.GroupId,
        //                Product2 = item.Products.ToList()
        //            });

        //        }

        //    }
        //    #endregion

        //    #region GetUserUpgradableProducts
        //    var upgradableGroupId = from userpackageEntitlements in userPackageEntitlementList
        //                            join packages in context.ProductPackages
        //                            on userpackageEntitlements.PackageId equals packages.PackageId
        //                            join upgradableproducts in context.ProductGroupUpgrades
        //                            on packages.Product.ProductGroupId equals upgradableproducts.ProductGroupId
        //                            select upgradableproducts.UpgradeToProductGroupId;


        //    var upgradablePackageProducts = from product in subscriptionProductC
        //                                    join id in upgradableGroupId.Distinct()
        //                                    on product.ProductGroupId equals id
        //                                    where product.IsForSale
        //                                    select product;


        //    var upgarablePackageGroupProducts = from product in upgradablePackageProducts
        //                                        group product by product.ProductGroupId into ProductGroup
        //                                        orderby ProductGroup.Key
        //                                        select new
        //                                        {
        //                                            ProductGroupId = ProductGroup.Key,
        //                                            Product2 =
        //                                            (
        //                                                from product2 in ProductGroup
        //                                                orderby product2.DurationInDays descending
        //                                                select new Product2
        //                                                {
        //                                                    ProductId = product2.ProductId,
        //                                                    Description = product2.Description,
        //                                                    Duration = product2.Duration,
        //                                                    DurationInDays = product2.DurationInDays,
        //                                                    DurationType = product2.DurationType,
        //                                                    ProductPrices = product2.ProductPrices.Where(p => p.CurrencyCode == currencyCode)
        //                                                }
        //                                            )
        //                                        };

        //    List<UpgradeablePackageGroupProduct_Display> upgarablePackageGroupProductList = new List<UpgradeablePackageGroupProduct_Display>();
        //    foreach (var item in upgarablePackageGroupProducts)
        //    {
        //        upgarablePackageGroupProductList.Add(new UpgradeablePackageGroupProduct_Display
        //                {
        //                    ProductGroupId = item.ProductGroupId,
        //                    Product2 = item.Product2.ToList()
        //                }
        //        );
        //    }
        //    #endregion


        //    ViewBag.UserPackageEntitlementList = showPackageGroupProductList;
        //    ViewBag.userShowEntitlement = showAlarcarteProducts;
        //    ViewBag.upgarablePackageGroupProductList = upgarablePackageGroupProductList;

        //    //return Json(new { aLaCarteProducts = showAlarcarteProducts, showPackageGroupProductList = showPackageGroupProductList, upgarablePackageGroupProducts = upgarablePackageGroupProducts }, JsonRequestBehavior.AllowGet);
        //    return PartialView("_GetUserEntitlements");
        //}

        #region GetUserEntitlements_Old
        ////[ChildActionOnly]
        //[Authorize]
        //public PartialViewResult GetUserEntitlements_Old(int categoryId, string countryCode)
        //{
        //    List<UserShowEntitlement_Display> UserShowEntitlementList = new List<UserShowEntitlement_Display>();
        //    List<UserPackageEntitlement_Display> UserPackageEntitlementList = new List<UserPackageEntitlement_Display>();

        //    string currencyCode = MyUtility.GetCurrencyOrDefault(countryCode);

        //    var context = new IPTV2Entities();
        //    //get user entitlements
        //    //System.Guid userId = new System.Guid("896ca57e-67bc-4a28-86ea-a52e5426b693");
        //    System.Guid userId = new System.Guid( HttpContext.User.Identity.Name );

        //    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //    var userEntitlements = user.Entitlements.Where(t => t.OfferingId == GlobalConfig.offeringId && t.EndDate >= DateTime.Now);

        //    //get package user entitlement
        //    var userPackageEntitlementList = from userPackageEntitlemts in userEntitlements
        //                                     where userPackageEntitlemts is PackageEntitlement
        //                                     select (PackageEntitlement)userPackageEntitlemts;
        //    //get show user entitlement
        //    var userShowEntitlementList = from userShowEntitlements in userEntitlements
        //                                   where userShowEntitlements is ShowEntitlement
        //                                   select (ShowEntitlement)userShowEntitlements;

        //    var offering = context.Offerings.Find(GlobalConfig.offeringId);
        //    var subscriptionProductC = SubscriptionProductC.LoadAll(context, offering.OfferingId,true)
        //                                  .Where(p => p.IsAllowed(countryCode));

        //    // GetUserEntitlementPackageProducts 
        //    // return UserPackageEntitlementList
        //    #region GetUserPackageEntitlement
        //    if (userPackageEntitlementList != null)
        //    {
        //        var userPackageEntitlement = from userpackageEntitlements in userPackageEntitlementList
        //                                     select new
        //                                     {
        //                                         PackageId = userpackageEntitlements.PackageId,
        //                                         PackageName = userpackageEntitlements.Package.PackageName,
        //                                         ExpiryDate = userpackageEntitlements.EndDate,
        //                                         Product2 = (
        //                                           from productpackages in context.ProductPackages
        //                                           where userpackageEntitlements.PackageId == productpackages.PackageId
        //                                           && productpackages.Product.IsForSale
        //                                           orderby productpackages.Product.DurationType descending
        //                                           orderby productpackages.Product.Duration descending
        //                                           select new Product2
        //                                           {
        //                                               ProductGroupId = productpackages.Product.ProductGroupId,
        //                                               ProductId = productpackages.ProductId,
        //                                               Duration = productpackages.Product.Duration,
        //                                               DurationType = productpackages.Product.DurationType,
        //                                               ProductPrices = (
        //                                                  from productprices in productpackages.Product.ProductPrices
        //                                                  where productprices.CurrencyCode == currencyCode && productprices.Product.IsForSale
        //                                                  orderby productprices.Amount descending
        //                                                  select new SubscriptionProductC.ProductPrice
        //                                                  {
        //                                                      Amount = productprices.Amount,
        //                                                      CurrencyCode = productprices.CurrencyCode,
        //                                                      IsLeft = productprices.Currency.IsLeft,
        //                                                      Symbol = productprices.Currency.Symbol
        //                                                  }
        //                                               )
        //                                           }
        //                                         )
        //                                     };


        //        foreach (var item in userPackageEntitlement)
        //        {
        //            UserPackageEntitlementList.Add(new UserPackageEntitlement_Display
        //            {
        //                PackageId = item.PackageId,
        //                ExpiryDate = item.ExpiryDate,
        //                Product2 = item.Product2.ToList()
        //            });
        //        }
        //    }
        //    #endregion

        //    // GetUserShowProducts    
        //    // return UserShowEntitlementList     
        //    #region GetUserShowEntitlement
        //    if (userShowEntitlementList != null)
        //    {
        //        var userShowEntitlement = from usershowentilements in userShowEntitlementList
        //                                  join productshows in context.ProductShows.Where(p => p.Product.IsForSale)// showproductpackages.ShowAlacarteProductList
        //                                  on usershowentilements.CategoryId equals productshows.CategoryId
        //                                  select new
        //                                  {
        //                                      CategoryId = usershowentilements.CategoryId,
        //                                      ProductName = productshows.Product.Name,
        //                                      ALaCarteSubscriptionTypeId = productshows.Product.ALaCarteSubscriptionTypeId,
        //                                      ProductId = productshows.ProductId,
        //                                      Duration = productshows.Product.Duration,
        //                                      DurationType = productshows.Product.DurationType,
        //                                      ExpiryDate = usershowentilements.EndDate,
        //                                      ProductPrice = (
        //                                                   from productprices in productshows.Product.ProductPrices
        //                                                   where productprices.CurrencyCode == currencyCode && productprices.Product.IsForSale
        //                                                   orderby productprices.Amount descending
        //                                                   select new SubscriptionProductC.ProductPrice
        //                                                   {
        //                                                       Amount = productprices.Amount,
        //                                                       CurrencyCode = productprices.CurrencyCode,
        //                                                       IsLeft = productprices.Currency.IsLeft,
        //                                                       Symbol = productprices.Currency.Symbol
        //                                                   }
        //                                      )

        //                                  };


        //        foreach (var item in userShowEntitlement)
        //        {
        //            UserShowEntitlementList.Add(new UserShowEntitlement_Display
        //            {
        //                CategoryId = item.CategoryId,
        //                ProductName = item.ProductName,
        //                ALaCarteSubscriptionTypeId = item.ALaCarteSubscriptionTypeId,
        //                ProductId = item.ProductId,
        //                Duration = item.Duration,
        //                DurationType = item.DurationType,
        //                ExpiryDate = item.ExpiryDate,
        //                ProductPrice = item.ProductPrice.ToList()
        //            });
        //        }
        //    }
        //    #endregion

        //    // GetUserUpgradableProducts
        //    // return upgarablePackageGroupProductList
        //    #region GetUserUpgradableProducts
        //    var upgradableGroupId =  from userpackageEntitlements in userPackageEntitlementList
        //                             join packages in context.ProductPackages
        //                             on userpackageEntitlements.PackageId equals packages.PackageId
        //                             join upgradableproducts in context.ProductGroupUpgrades
        //                             on packages.Product.ProductGroupId equals upgradableproducts.ProductGroupId
        //                             select upgradableproducts.UpgradeToProductGroupId;


        //    var upgradablePackageProducts = from product in subscriptionProductC
        //                                    join id in upgradableGroupId.Distinct()
        //                                    on product.ProductGroupId equals id
        //                                    where product.IsForSale
        //                                    select product;


        //    var upgarablePackageGroupProducts = from product in upgradablePackageProducts
        //                                       group product by product.ProductGroupId into ProductGroup
        //                                       orderby ProductGroup.Key
        //                                       select new
        //                                       {
        //                                           ProductGroupId = ProductGroup.Key,
        //                                           Product2 = 
        //                                           (
        //                                               from product2 in ProductGroup                                                       
        //                                               orderby product2.DurationInDays descending
        //                                               select new Product2
        //                                               {
        //                                                   ProductId = product2.ProductId,
        //                                                   Description = product2.Description,
        //                                                   Duration = product2.Duration,
        //                                                   DurationInDays = product2.DurationInDays,
        //                                                   DurationType = product2.DurationType,
        //                                                   ProductPrices = product2.ProductPrices.Where(p => p.CurrencyCode == currencyCode )
        //                                               }
        //                                           )
        //                                       };

        //    List<UpgradeablePackageGroupProduct_Display> upgarablePackageGroupProductList = new List<UpgradeablePackageGroupProduct_Display>();
        //    foreach (var item in upgarablePackageGroupProducts)
        //    {
        //        upgarablePackageGroupProductList.Add(new UpgradeablePackageGroupProduct_Display
        //                {
        //                    ProductGroupId = item.ProductGroupId,
        //                    Product2 = item.Product2.ToList()
        //                }
        //        );
        //    }
        //    #endregion

        //    ViewBag.UserPackageEntitlementList = UserPackageEntitlementList;
        //    ViewBag.userShowEntitlement = UserShowEntitlementList;
        //    ViewBag.upgarablePackageGroupProductList = upgarablePackageGroupProductList;

        //    /*return Json(new {
        //        userPackageEntitlement = UserPackageEntitlementList,
        //        userShowEntitlement = userShowEntitlement,
        //        upgarablePackageGroupProducts = upgarablePackageGroupProductList 
        //                }
        //                , JsonRequestBehavior.AllowGet);
        //    */

        //   return PartialView("_GetUserEntitlements");
        //}
        #endregion


        #region old getpackage

        //public PartialViewResult OldGetPackages(int id)
        //{
        //    var context = new IPTV2Entities();
        //    List<Package> packagelist = new List<Package>();
        //    List<ProductPackage> productlist = new List<ProductPackage>();
        //    List<ProductShow> productshowlist = new List<ProductShow>();

        //    List<int> productGrpIds = new List<int>();
        //    int? dummyPackageId = null;
        //    List<PackageProductUpgradeDisplay> upgradable_packagelist = new List<PackageProductUpgradeDisplay>();
        //    List<ProductPackage> upgradable_productlist = new List<ProductPackage>();
        //    List<ProductGroup> upgradable_productgrouplist = new List<ProductGroup>();

        //    string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
        //    string currencycode = MyUtility.GetCurrencyOrDefault(countrycode);

        //    //created a stopwatch
        //    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //    sw.Start();

        //    #region Subscription Packages

        //    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
        //    Offering off = context.Offerings.Find(GlobalConfig.offeringId);

        //    if (category != null && category is Show)
        //    {
        //        Show show = (Show)category;
        //        var productGroup = show.GetOnlinePackageProducts(off, MyUtility.GetCurrentCountryCodeOrDefault());

        //        if (productGroup != null && productGroup.Count() > 0)
        //        {
        //            //get the distinct productgroups and only allowed products to a specific country
        //            int[] ProductGroups = productGroup
        //                                  .Where(pp => pp.IsForSale)
        //                                  .Select(pp => pp.ProductGroupId)
        //                                  .Distinct().ToArray();

        //            //user current currency
        //            productlist = context.ProductPackages
        //                            .Where(pd => ProductGroups.Contains(pd.Product.ProductGroupId))
        //                            .AsEnumerable()
        //                            .OrderBy(pp => pp.PackageId)
        //                            .ThenByDescending(pp => pp.Product.DurationType)
        //                            .ThenByDescending(ps => ps.Product.Duration)
        //                            .ToList();

        //            var data = productlist.GroupBy(pck => new { pck.PackageId, pck.Package.PackageName })
        //                                          .Select(group => new
        //                                          {
        //                                              group.Key.PackageId,
        //                                              group.Key.PackageName
        //                                          });
        //            foreach (var item in data)
        //            {
        //                packagelist.Add(new Package
        //                {
        //                    PackageId = item.PackageId
        //                    ,
        //                    PackageName = item.PackageName
        //                });
        //            }
        //            //}

        //            ViewBag.PackageList = packagelist;
        //            ViewBag.productlist = productlist;
        //        }

        //    #endregion Subscription Packages

        //        #region Alacarte

        //        productshowlist = context.ProductShows
        //                    .Where(ps => ps.CategoryId == id
        //                        && ps.Show.StatusId == GlobalConfig.Visible
        //                        && ps.Product.OfferingId == GlobalConfig.offeringId
        //                        && ps.Product.IsForSale
        //                        && ps.Product.ProductPrices.Where(p => p.CurrencyCode == currencycode && p.ProductId == ps.ProductId).Count() > 0
        //                        )
        //                    .AsEnumerable()
        //                    .Where(p => p.Product.IsAllowed(countrycode))
        //                    .ToList();
        //        if (productshowlist.Count() == 0)
        //        {
        //            productshowlist = context.ProductShows
        //                    .Where(ps => ps.CategoryId == id
        //                        && ps.Show.StatusId == GlobalConfig.Visible
        //                        && ps.Product.OfferingId == GlobalConfig.offeringId
        //                        && ps.Product.IsForSale
        //                        && ps.Product.ProductPrices.Where(p => p.CurrencyCode == GlobalConfig.DefaultCurrency && p.ProductId == ps.ProductId).Count() > 0
        //                        )
        //                    .AsEnumerable()
        //                    .Where(p => p.Product.IsAllowed(countrycode))
        //                    .ToList();
        //        }
        //        ViewBag.productshow = productshowlist;

        //        #endregion Alacarte

        //        //If user is entitled don't show the packages anymore

        //        #region User is entitled

        //        if (UserPackages.CheckUser_ShowEntitled(id))
        //        {
        //            List<UserEntitlementDisplay> userPackageList = new List<UserEntitlementDisplay>();
        //            List<ProductShow> userProductShow = new List<ProductShow>();

        //            foreach (var item in UserPackages.GetUser_EntitlementsList())
        //            {
        //                #region for package entitlements

        //                if (item is PackageEntitlement)
        //                {
        //                    var pck = (PackageEntitlement)item;
        //                    foreach (Package package in packagelist)
        //                    {
        //                        if (package.PackageId == pck.PackageId)
        //                        {
        //                            userPackageList.Add(new UserEntitlementDisplay
        //                            {
        //                                PackageId = pck.PackageId,
        //                                PackageName = pck.Package.PackageName,
        //                                ExpiryDate = item.EndDate
        //                            });

        //                            dummyPackageId = pck.PackageId;

        //                            int productGrpID = context.ProductPackages
        //                                            .FirstOrDefault(p => p.PackageId == pck.PackageId)
        //                                            .Product
        //                                            .ProductGroupId;

        //                            upgradable_productgrouplist.AddRange(
        //                                  UserPackages.GetUpgradableProductGroup(productGrpID)
        //                                );

        //                            productGrpIds.AddRange(upgradable_productgrouplist.Select(p => p.ProductGroupId).ToArray());
        //                        }
        //                    }
        //                }

        //                #endregion for package entitlements

        //                #region for alacarte entitlements

        //                else if (item is ShowEntitlement)
        //                {
        //                    var shw = (ShowEntitlement)item;
        //                    foreach (ProductShow pdshow in productshowlist)
        //                    {
        //                        if (pdshow.CategoryId == shw.CategoryId)
        //                        {
        //                            userPackageList.Add(new UserEntitlementDisplay
        //                            {
        //                                ProductId = pdshow.ProductId,
        //                                ProductName = pdshow.Product.Name,
        //                                ALaCarteSubscriptionTypeId = pdshow.Product.ALaCarteSubscriptionTypeId ?? 1,
        //                                DurationType = pdshow.Product.DurationType,
        //                                Duration = pdshow.Product.Duration,
        //                                CategoryId = shw.CategoryId,
        //                                ExpiryDate = item.EndDate
        //                            });
        //                        }
        //                    }
        //                }

        //                #endregion for alacarte entitlements

        //                #region Get the Upgradable Packages

        //                if (upgradable_productgrouplist.Count() > 0)
        //                {
        //                    //user curreny currency
        //                    upgradable_productlist = context.ProductPackages
        //                                   .Where(pd => productGrpIds.Contains(pd.Product.ProductGroupId)
        //                                       && pd.Product.IsForSale
        //                                    )
        //                                    .AsEnumerable()
        //                                    .Where(p => p.Product.IsAllowed(countrycode))
        //                                    .OrderBy(pp => pp.PackageId)
        //                                    .ThenByDescending(pp => pp.Product.DurationType)
        //                                    .ThenByDescending(ps => ps.Product.Duration)
        //                                    .ToList();

        //                    var datalist = upgradable_productlist.GroupBy(pc => new { pc.PackageId, pc.Package.PackageName })
        //                                                  .Select(group => new
        //                                                  {
        //                                                      group.Key.PackageId,
        //                                                      group.Key.PackageName
        //                                                  });
        //                    foreach (var data in datalist)
        //                    {
        //                        upgradable_packagelist.Add(new PackageProductUpgradeDisplay
        //                        {
        //                            PackageId = data.PackageId,
        //                            PackageName = data.PackageName,
        //                            CurrentProductId = (item.LatestEntitlementRequest != null ?
        //                                                 (int?)item.LatestEntitlementRequest.ProductId
        //                                                 : null)
        //                        });
        //                    }
        //                }

        //                #endregion Get the Upgradable Packages

        //                ViewBag.Upgrade_PackageList = upgradable_packagelist;
        //                ViewBag.Upgrade_productlist = upgradable_productlist;
        //                ViewBag.UpGradableProductGroupList = upgradable_productgrouplist;
        //            }

        //            ViewBag.UserPackageList = userPackageList;

        //            //stop the watch
        //            sw.Stop();
        //            ViewBag.ElapsedTime = sw.Elapsed.TotalSeconds;
        //            return PartialView("_UserIsEntitled");
        //        }

        //        #endregion User is entitled

        //        if (productlist != null)
        //        {
        //            if (productlist.Count() > 0 || productshowlist.Count() > 0)
        //            {
        //                //stop the watch
        //                sw.Stop();
        //                ViewBag.ElapsedTime = sw.Elapsed.TotalSeconds;
        //                return PartialView("_GetShowPackages");
        //            }
        //        }
        //    }

        //    return null;
        //}

        public string GetAmount(int productid)
        {
            var context = new IPTV2Entities();
            string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
            string currencycode = MyUtility.GetCurrencyOrDefault(countrycode);

            var data = context.ProductPrices
                .Where(p => p.ProductId == productid
                && p.CurrencyCode == currencycode
                && p.Product.OfferingId == GlobalConfig.offeringId
                && p.Product.StatusId == GlobalConfig.Visible
                && p.Product.IsForSale).AsEnumerable()
                .AsEnumerable()
                .FirstOrDefault(pp => pp.Product.IsAllowed(countrycode));

            if (data != null)
            {
                if (data.CurrencyCode == "AED" || data.CurrencyCode == "SAR")
                {
                    return String.Format("{0} {1}", data.CurrencyCode, data.Amount.ToString("F"));
                }
                else
                {
                    if (data.Currency.IsLeft)
                    {
                        return String.Format("{0}{1}", data.Currency.Symbol, data.Amount.ToString("F"));
                    }
                    return String.Format("{0}{1}", data.Amount.ToString("F"), data.Currency.Symbol);
                }
            }
            else
            {
                data = context.ProductPrices
                    .FirstOrDefault(p => p.ProductId == productid
                    && p.CurrencyCode == GlobalConfig.DefaultCurrency);
                if (data == null)
                {
                    return null;
                }

                return String.Format("{0}{1}", data.Currency.Symbol, data.Amount.ToString("F"));
            }
        }

        #endregion

        //public ActionResult GetEpisodes(int? id)
        //{
        //    return PartialView("_GetEpisodes");
        //}

        [OutputCache(VaryByParam = "id", Duration = 300)]
        public ActionResult GetEpisodes(int? id)
        {
            if (MyUtility.isSearchSpider(Request))
                return PartialView("_GetEpisodesForDetails", GetShowEpisodes(id));

            return PartialView("_GetEpisodes");
        }

        public ActionResult ShowEpisodes(int? id)
        {
            return View();
        }

        //[GridAction]
        //public ActionResult _ShowEpisodes(int? id)
        //{
        //    return View(new GridModel<EpisodeDisplay> { Data = GetShowEpisodes(id) });
        //}

        private List<EpisodeDisplay> GetShowEpisodes(int? id)
        {
            List<EpisodeDisplay> display = null;

            var cache = DataCache.Cache;
            string cacheKey = "LEGSE:O:" + id.ToString() + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            display = (List<EpisodeDisplay>)cache[cacheKey];
            DateTime registDt = DateTime.Now;
            if (display == null)
            {
                display = new List<EpisodeDisplay>();
                if (id != null)
                {
                    var context = new IPTV2Entities();
                    var show = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
                    if (show != null)
                    {
                        if (show is Show)
                        {
                            //Get all episode numbers
                            var episode_list = context.EpisodeCategories1.Where(e => e.Show.CategoryId == ((Show)show).CategoryId).Select(e => e.EpisodeId);
                            //var episodes = ((Show)show).Episodes.Where(e => e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderByDescending(e => e.Episode.DateAired).Select((item, index) => new { Item = item, Position = index }).ToList();
                            var episodes = context.Episodes.Where(e => episode_list.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.IsLiveChannelActive).ThenByDescending(e => e.DateAired).ToList();
                            ViewBag.sCount = episodes.Count();
                            int epCount = episodes.Count();
                            foreach (var episode in episodes)
                            {
                                string EpLength = "";
                                if (!(episode.EpisodeLength == null))
                                {
                                    TimeSpan span = new TimeSpan(0, 0, Convert.ToInt32(episode.EpisodeLength) * 60);
                                    EpLength = String.Format("{0}:{1}:{2}", span.Hours.ToString().PadLeft(2, '0'), span.Minutes.ToString().PadLeft(2, '0'), span.Seconds.ToString().PadLeft(2, '0'));
                                }

                                EpisodeDisplay disp = new EpisodeDisplay()
                                {
                                    EpisodeId = episode.EpisodeId,

                                    EpisodeName = episode.EpisodeName,
                                    Description = episode.Description,
                                    DateAired = episode.DateAired,
                                    DateAiredStr = String.Compare(episode.EpisodeName, episode.EpisodeCode, true) == 0 ? String.Format("{0}", episode.DateAired.Value.ToString("MMMM d, yyyy")) : String.Format("{0}", episode.EpisodeName),
                                    EpisodeLength = episode.EpisodeLength,
                                    EpisodeNumber = epCount,
                                    EpLength = EpLength
                                };
                                display.Add(disp);
                                epCount--;
                            }
                        }
                    }
                    cache.Put(cacheKey, display, DataCache.CacheDuration);
                }
            }
            return display;
        }

        [OutputCache(VaryByParam = "id", Duration = 300)]
        public ActionResult GetPreviewEpisodes(int id, int limit)
        {
            DateTime registDt = DateTime.Now;
            var context = new IPTV2Entities();
            var episodesCats = context.EpisodeCategories1.Where(e => e.CategoryId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible).Select(ec => ec.EpisodeId);
            var episodes = context.Episodes.Where(e => episodesCats.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt)
                .OrderByDescending(ec => ec.DateAired).Take(limit);
            return PartialView("_GetPreviewEpisodes", episodes);
        }

        //public PartialViewResult GetLowestPackages(int categoryId, string countryCode)
        //{
        //    ShowPackageProductPrices showPackageProductPrices = new ShowPackageProductPrices();
        //    try
        //    {
        //        showPackageProductPrices = showPackageProductPrices.LoadAllPackageAndProduct(categoryId, countryCode, true);
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    //return PartialView("_GetShowProductPackages", showPackageProductPrices);
        //    return PartialView(showPackageProductPrices);
        //}

        //public PartialViewResult GetLowestPackages2(ShowPackageProductPrices showPackageProductPrices, bool useJQueryClick = false)
        //{
        //    if (useJQueryClick)
        //        return PartialView("GetLowestPackagesJQueryClick", showPackageProductPrices);
        //    return PartialView("GetLowestPackages", showPackageProductPrices);
        //}

        [RequireHttp]
        public ActionResult Details(int? id, string slug)
        {
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            try
            {
                if (id != null)
                {
                    //if (id == GlobalConfig.TFCkatCategoryId)
                    //    return Redirect("/TFCkat");
                    //else 
                    if (id == GlobalConfig.UAAPGreatnessNeverEndsCategoryId)
                        return RedirectToAction("Index", "UAAP");
                    else if (id == GlobalConfig.UAAPLiveStreamEpisodeId)
                        return RedirectToAction("Live", "UAAP");
                    else if (id == GlobalConfig.FPJCategoryId)
                        return RedirectToAction("Index", "FPJ");
                    var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                    if (excludedCategoryIds.Contains((int)id))
                        return RedirectToAction("Index", "Home");
                    var context = new IPTV2Entities();
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                    if (category != null)
                    {
                        if (category.StatusId == GlobalConfig.Visible)
                        {
                            ViewBag.Loved = false; ViewBag.Rated = false; // Social
                            ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                            if (category is Show)
                            {
                                var dbSlug = MyUtility.GetSlug(category.Description);
                                if (String.Compare(dbSlug, slug, false) != 0)
                                    return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });
                                var show = (Show)category;
                                if (show.StartDate < registDt && show.EndDate > registDt)
                                {
                                    string CountryCodeTemp = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                                    string CountryCode = CountryCodeTemp;
                                    if (User.Identity.IsAuthenticated)
                                    {
                                        var UserId = new Guid(User.Identity.Name);
                                        var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                        if (user != null)
                                        {
                                            var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                                            var isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
                                            CountryCode = user.CountryCode;
                                            ViewBag.EmailAddress = user.EMail;
                                            ViewBag.UserId = user.UserId;
                                            ViewBag.CountryCode = CountryCode;

                                            if (!ContextHelper.IsCategoryViewableInUserCountry(show, CountryCode))
                                                return RedirectToAction("Index", "Home");

                                            if (!isEmailAllowed)
                                            {
                                                string CountryCodeBasedOnIpAddress = CountryCodeTemp;
                                                if (!ContextHelper.IsCategoryViewableInUserCountry(show, CountryCodeBasedOnIpAddress))
                                                    return RedirectToAction("Index", "Home");
                                            }

                                            using (profiler.Step("Social Love"))
                                            {
                                                ViewBag.Loved = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_LOVE, id, EngagementContentType.Show);
                                            }
                                            using (profiler.Step("Social Rating"))
                                            {
                                                ViewBag.Rated = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_RATING, id, EngagementContentType.Show);
                                            }

                                            using (profiler.Step("Check for Active Entitlements")) //check for active entitlements based on categoryId
                                            {
                                                ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, show.CategoryId, registDt);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!ContextHelper.IsCategoryViewableInUserCountry(show, CountryCode))
                                            return RedirectToAction("Index", "Home");
                                    }

                                    try
                                    {
                                        var ShowParentCategories = ContextHelper.GetShowParentCategories(show.CategoryId);
                                        if (!String.IsNullOrEmpty(ShowParentCategories))
                                        {
                                            ViewBag.ShowParentCategories = ShowParentCategories;
                                            if (!MyUtility.IsWhiteListed(String.Empty))
                                            {
                                                var ids = MyUtility.StringToIntList(ShowParentCategories);
                                                var alaCarteIds = MyUtility.StringToIntList(GlobalConfig.UXAlaCarteParentCategoryIds);
                                                var DoCheckForGeoIPRestriction = ids.Intersect(alaCarteIds).Count() > 0;
                                                if (DoCheckForGeoIPRestriction)
                                                {
                                                    var ipx = Request.IsLocal ? "221.121.187.253" : String.Empty;
                                                    if (GlobalConfig.isUAT)
                                                        ipx = Request["ip"];
                                                    if (!MyUtility.CheckIfCategoryIsAllowed(show.CategoryId, context, ipx))
                                                    {
                                                        var ReturnCode = new TransactionReturnType()
                                                        {
                                                            StatusHeader = "Content Advisory",
                                                            StatusMessage = "This content is currently not available in your area.",
                                                            StatusMessage2 = "You may select from among the hundreds of shows and movies that are available on the site."
                                                        };
                                                        TempData["ErrorMessage"] = ReturnCode;
                                                        return RedirectToAction("Index", "Home");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception) { }

                                    //Get Episode Count
                                    using (profiler.Step("Episode Count"))
                                    {
                                        ViewBag.EpisodeCount = context.EpisodeCategories1.Count(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && e.Show.CategoryId == show.CategoryId && e.Episode.OnlineStartDate < registDt && e.Episode.OnlineEndDate > registDt);
                                    }

                                    using (profiler.Step("Check Show Type"))
                                    {
                                        if (show is Movie)
                                        {
                                            ViewBag.CategoryType = "Movie";
                                            return RedirectToAction("Details", "Episode", new { id = GetMovieEpisodeId(show.CategoryId) });
                                        }
                                        else if (show is SpecialShow)
                                        {
                                            ViewBag.CategoryType = "Special";
                                            return RedirectToAction("Details", "Episode", new { id = GetMovieEpisodeId(show.CategoryId) });
                                        }
                                        else if (show is LiveEvent)
                                        {
                                            ViewBag.CategoryType = "LiveEvent";
                                            var episodeCategory = context.EpisodeCategories1.FirstOrDefault(e => e.CategoryId == show.CategoryId);
                                            if (episodeCategory != null)
                                                return RedirectToAction("Details", "Live", new { id = episodeCategory.EpisodeId, slug = MyUtility.GetSlug(episodeCategory.Episode.EpisodeName) });
                                        }
                                    }
                                    ViewBag.dbSlug = dbSlug;
                                    ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
                                    if (!Request.Cookies.AllKeys.Contains("version"))
                                        return View("Details2", show);
                                    return View(show);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }

        //public ActionResult GetListOfEpisodes(int? id, [DataSourceRequest]DataSourceRequest request)
        //{
        //    List<EpisodeObject> list = null;
        //    try
        //    {
        //        if (id != null)
        //        {
        //            var registDt = DateTime.Now;
        //            var context = new IPTV2Entities();
        //            var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
        //            if (category != null)
        //            {
        //                if (category.StartDate < registDt && category.EndDate > registDt && category is Show)
        //                {
        //                    var EpisodeList = context.EpisodeCategories1.Where(e => e.CategoryId == category.CategoryId).Select(e => e.EpisodeId);
        //                    if (EpisodeList != null)
        //                    {
        //                        var episodes = context.Episodes.Where(e => EpisodeList.Contains(e.EpisodeId)).OrderByDescending(e => e.DateAired);
        //                        if (episodes != null)
        //                        {
        //                            list = new List<EpisodeObject>();
        //                            foreach (var episode in episodes)
        //                            {
        //                                if (episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt && episode.OnlineStatusId == GlobalConfig.Visible)
        //                                {
        //                                    string img = GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif;
        //                                    if (!String.IsNullOrEmpty(episode.ImageAssets.ImageVideo))
        //                                        img = String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId, episode.ImageAssets.ImageVideo);
        //                                    var tempShowNameWithDate = String.Format("{0} {1}", category.Description, episode.DateAired.Value.ToString("MMMM d, yyyy"));
        //                                    list.Add(new EpisodeObject()
        //                                    {
        //                                        EpisodeId = episode.EpisodeId,
        //                                        Name = String.Compare(episode.EpisodeName, episode.Description, true) == 0 ? category.Description : episode.EpisodeName,
        //                                        DateAiredStr = String.Compare(episode.EpisodeName, episode.EpisodeCode, true) == 0 ? String.Format("{0}", episode.DateAired.Value.ToString("MMMM d, yyyy")) : String.Format("{0}", episode.EpisodeName),
        //                                        Synopsis = MyUtility.Ellipsis(episode.Synopsis, 80),
        //                                        ImgUrl = img,
        //                                        slug = MyUtility.GetSlug(tempShowNameWithDate)
        //                                    });
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        //}

        public PartialViewResult EpisodeList(int? id, int? episodeId, int? NextEpisodeId, int? PreviousEpisodeId, int? EpisodeNumber, int? EpisodeCount, int? pageSize, string partialViewName = "", string href = "")
        {
            List<EpisodeObject> list = null;
            try
            {
                ViewBag.href = href;
                ViewBag.pageSize = pageSize;
                if (id != null)
                {
                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                    if (category != null)
                    {
                        if (category.StartDate < registDt && category.EndDate > registDt && category is Show)
                        {
                            var EpisodeList = context.EpisodeCategories1.Where(e => e.CategoryId == category.CategoryId).Select(e => e.EpisodeId);
                            if (EpisodeList != null)
                            {
                                IQueryable<Episode> episodes;
                                int NumberOfItemsInEpisodeList = pageSize == null ? GlobalConfig.NumberOfItemsInEpisodeList : (int)pageSize;
                                if (episodeId != null)
                                {
                                    ViewBag.AddOne = false;
                                    episodes = context.Episodes.Where(e => EpisodeList.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).ThenByDescending(e => e.EpisodeId);
                                    ViewBag.LatestEpisodeId = episodes.Select(e => e.EpisodeId).First();
                                    var eIndex = episodes.Select(e => e.EpisodeId).ToList().FindIndex(e => e == episodeId);
                                    if (!(eIndex < 0))
                                        if (eIndex <= NumberOfItemsInEpisodeList)
                                            episodes = context.Episodes.Where(e => EpisodeList.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).ThenByDescending(e => e.EpisodeId).Take(NumberOfItemsInEpisodeList);
                                        else
                                        {
                                            ViewBag.AddOne = true;
                                            episodes = (IOrderedQueryable<Episode>)episodes.Skip(eIndex == 0 ? eIndex : eIndex - 1).Take(NumberOfItemsInEpisodeList);
                                        }

                                }
                                else
                                    episodes = context.Episodes.Where(e => EpisodeList.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).ThenByDescending(e => e.EpisodeId).Take(NumberOfItemsInEpisodeList);
                                //var episodes = context.Episodes.Where(e => EpisodeList.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).Take(GlobalConfig.NumberOfItemsInEpisodeList);
                                if (episodes != null)
                                {
                                    /* Check if there are more episodes after */
                                    bool HasMoreEpisodes = false;
                                    try
                                    {
                                        HasMoreEpisodes = context.Episodes.Where(e => EpisodeList.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).Skip(NumberOfItemsInEpisodeList).Take(1).Count() > 0;
                                    }
                                    catch (Exception) { }
                                    ViewBag.HasMoreEpisodes = HasMoreEpisodes;
                                    /* End Check if there are more episodes after */

                                    list = new List<EpisodeObject>();
                                    foreach (var episode in episodes)
                                    {
                                        string img = GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif;
                                        if (!String.IsNullOrEmpty(episode.ImageAssets.ImageVideo))
                                            img = String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId, episode.ImageAssets.ImageVideo);
                                        var tempShowNameWithDate = String.Format("{0} {1}", category.Description, episode.DateAired.Value.ToString("MMMM d, yyyy"));
                                        list.Add(new EpisodeObject()
                                        {
                                            EpisodeId = episode.EpisodeId,
                                            Name = String.Compare(episode.EpisodeName, episode.Description, true) == 0 ? category.Description : episode.EpisodeName,
                                            DateAiredStr = String.Compare(episode.EpisodeName, episode.EpisodeCode, true) == 0 ? String.Format("{0}", episode.DateAired.Value.ToString("MMMM d, yyyy")) : String.Format("{0}", episode.EpisodeName),
                                            Synopsis = episode.Synopsis,
                                            ImgUrl = img,
                                            Show = category.Description,
                                            slug = MyUtility.GetSlug(tempShowNameWithDate)
                                        });
                                    }
                                    ViewBag.ShowName = category.Description;
                                }
                                try
                                {
                                    if (episodeId != null)
                                    {
                                        //int index = list.FindIndex(x => x.EpisodeId == episodeId);
                                        //if (index >= 0)
                                        //    list.RemoveAt(index);
                                        ViewBag.EpisodeId = episodeId;
                                        ViewBag.NextEpisodeId = NextEpisodeId;
                                        ViewBag.PreviousEpisodeId = PreviousEpisodeId;
                                        ViewBag.EpisodeNumber = EpisodeNumber;
                                        ViewBag.EpisodeCount = EpisodeCount;
                                    }
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, list);
            return PartialView(list);
        }

        public PartialViewResult GetMoreEpisodes(int? id, int? episodeId, int? NextEpisodeId, int? PreviousEpisodeId, int? EpisodeNumber, int? EpisodeCount, int? currentEpisodeCount, int? pageSize, int page = 1, string partialViewName = "", bool isEpisodePage = false)
        {
            List<EpisodeObject> list = null;
            try
            {
                ViewBag.page = page + 1;
                ViewBag.pageSize = pageSize;
                if (id != null)
                {
                    int size = GlobalConfig.NumberOfItemsInEpisodeList;
                    if (pageSize != null)
                        size = (int)pageSize;
                    int skipSize = size * page;
                    if (isEpisodePage)
                        skipSize = skipSize - (size - GlobalConfig.NumberOfItemsInEpisodeListInEpisodePage);
                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                    if (category != null)
                    {
                        if (category.StartDate < registDt && category.EndDate > registDt && category is Show)
                        {
                            var EpisodeList = context.EpisodeCategories1.Where(e => e.CategoryId == category.CategoryId).Select(e => e.EpisodeId);
                            if (EpisodeList != null)
                            {
                                IQueryable<Episode> episodes;
                                if (episodeId != null)
                                {
                                    episodes = context.Episodes.Where(e => EpisodeList.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).ThenByDescending(e => e.EpisodeId);
                                    var eIndex = episodes.Select(e => e.EpisodeId).ToList().FindIndex(e => e == episodeId);
                                    if (!(eIndex < 0))
                                        if (eIndex <= GlobalConfig.NumberOfItemsInEpisodeList)
                                            episodes = context.Episodes.Where(e => EpisodeList.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).ThenByDescending(e => e.EpisodeId).Skip(skipSize).Take(size);
                                        else
                                            episodes = (IOrderedQueryable<Episode>)episodes.Skip(eIndex == 0 ? eIndex : eIndex - 1).Skip(size * page).Take(size);
                                }
                                else
                                    episodes = context.Episodes.Where(e => EpisodeList.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).ThenByDescending(e => e.EpisodeId).Skip(skipSize).Take(size);

                                if (episodes != null)
                                {
                                    /* Check if there are more episodes after */
                                    bool HasMoreEpisodes = false;
                                    try
                                    {
                                        HasMoreEpisodes = context.Episodes.Where(e => EpisodeList.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).Skip(skipSize + size).Take(1).Count() > 0;
                                    }
                                    catch (Exception) { }
                                    ViewBag.HasMoreEpisodes = HasMoreEpisodes;
                                    /* End Check if there are more episodes after */

                                    list = new List<EpisodeObject>();
                                    foreach (var episode in episodes)
                                    {
                                        string img = GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif;
                                        if (!String.IsNullOrEmpty(episode.ImageAssets.ImageVideo))
                                            img = String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId, episode.ImageAssets.ImageVideo);
                                        var tempShowNameWithDate = String.Format("{0} {1}", category.Description, episode.DateAired.Value.ToString("MMMM d, yyyy"));
                                        list.Add(new EpisodeObject()
                                        {
                                            EpisodeId = episode.EpisodeId,
                                            Name = String.Compare(episode.EpisodeName, episode.Description, true) == 0 ? category.Description : episode.EpisodeName,
                                            DateAiredStr = String.Compare(episode.EpisodeName, episode.EpisodeCode, true) == 0 ? String.Format("{0}", episode.DateAired.Value.ToString("MMMM d, yyyy")) : String.Format("{0}", episode.EpisodeName),
                                            Synopsis = episode.Synopsis,
                                            ImgUrl = img,
                                            Show = category.Description,
                                            slug = MyUtility.GetSlug(tempShowNameWithDate)
                                        });
                                    }
                                    try
                                    {
                                        if (episodeId != null)
                                        {
                                            //int index = list.FindIndex(x => x.EpisodeId == episodeId);
                                            //if (index >= 0)
                                            //    list.RemoveAt(index);
                                            ViewBag.EpisodeId = episodeId;
                                            ViewBag.NextEpisodeId = NextEpisodeId;
                                            ViewBag.PreviousEpisodeId = PreviousEpisodeId;
                                            ViewBag.EpisodeNumber = EpisodeNumber;
                                            ViewBag.EpisodeCount = EpisodeCount;
                                            ViewBag.LatestEpisodeId = episodes.Select(e => e.EpisodeId).First();
                                        }
                                        ViewBag.currentEpisodeCount = currentEpisodeCount;
                                    }
                                    catch (Exception) { }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, list);
            return PartialView(list);
        }

        //[RequireHttp]
        //public ActionResult Details2(int? id, string slug)
        //{
        //    var profiler = MiniProfiler.Current;
        //    var registDt = DateTime.Now;
        //    try
        //    {
        //        if (id != null)
        //        {
        //            if (id == GlobalConfig.TFCkatCategoryId)
        //                return Redirect("/TFCkat");
        //            else if (id == GlobalConfig.UAAPGreatnessNeverEndsCategoryId)
        //                return RedirectToActionPermanent("Index", "UAAP");
        //            else if (id == GlobalConfig.FPJCategoryId)
        //                return RedirectToActionPermanent("Index", "FPJ");

        //            string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
        //            var context = new IPTV2Entities();
        //            User user = null;
        //            if (User.Identity.IsAuthenticated)
        //            {
        //                var UserId = new Guid(User.Identity.Name);
        //                user = context.Users.FirstOrDefault(u => u.UserId == UserId);
        //                if (user != null)
        //                    CountryCode = user.CountryCode;
        //            }

        //            //Check if CategoryId is part of the service
        //            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
        //            if (!ShowListBasedOnCountryCode.Contains((int)id))
        //                return RedirectToAction("Index", "Home");

        //            var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
        //            if (category != null)
        //            {
        //                if (category.StatusId == GlobalConfig.Visible)
        //                {
        //                    ViewBag.Loved = false; ViewBag.Rated = false; // Social
        //                    ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
        //                    if (category is Show)
        //                    {
        //                        var dbSlug = MyUtility.GetSlug(category.Description);
        //                        if (String.Compare(dbSlug, slug, false) != 0)
        //                            return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });
        //                        var show = (Show)category;
        //                        if (show.StartDate < registDt && show.EndDate > registDt)
        //                        {
        //                            string CountryCodeTemp = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();

        //                            if (User.Identity.IsAuthenticated)
        //                            {
        //                                if (user != null)
        //                                {
        //                                    var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
        //                                    var isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
        //                                    ViewBag.EmailAddress = user.EMail;
        //                                    ViewBag.UserId = user.UserId;
        //                                    ViewBag.CountryCode = CountryCode;

        //                                    if (!ContextHelper.IsCategoryViewableInUserCountry(show, CountryCode))
        //                                        return RedirectToAction("Index", "Home");

        //                                    if (!isEmailAllowed)
        //                                    {
        //                                        string CountryCodeBasedOnIpAddress = CountryCodeTemp;
        //                                        if (!ContextHelper.IsCategoryViewableInUserCountry(show, CountryCodeBasedOnIpAddress))
        //                                            return RedirectToAction("Index", "Home");
        //                                    }

        //                                    using (profiler.Step("Social Love"))
        //                                    {
        //                                        ViewBag.Loved = ContextHelper.HasSocialEngagement(user.UserId, GlobalConfig.SOCIAL_LOVE, id, EngagementContentType.Show);
        //                                    }
        //                                    using (profiler.Step("Social Rating"))
        //                                    {
        //                                        ViewBag.Rated = ContextHelper.HasSocialEngagement(user.UserId, GlobalConfig.SOCIAL_RATING, id, EngagementContentType.Show);
        //                                    }

        //                                    using (profiler.Step("Check for Active Entitlements")) //check for active entitlements based on categoryId
        //                                    {
        //                                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, show.CategoryId, registDt);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (!ContextHelper.IsCategoryViewableInUserCountry(show, CountryCode))
        //                                    return RedirectToAction("Index", "Home");
        //                            }

        //                            //Get Episode Count
        //                            using (profiler.Step("Episode Count"))
        //                            {
        //                                ViewBag.EpisodeCount = context.EpisodeCategories1.Count(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && e.Show.CategoryId == show.CategoryId && e.Episode.OnlineStartDate < registDt && e.Episode.OnlineEndDate > registDt);
        //                            }

        //                            using (profiler.Step("Check Show Type"))
        //                            {
        //                                if (show is Movie)
        //                                {
        //                                    ViewBag.CategoryType = "Movie";
        //                                    return RedirectToAction("Details", "Episode", new { id = GetMovieEpisodeId(show.CategoryId) });
        //                                }
        //                                else if (show is SpecialShow)
        //                                {
        //                                    ViewBag.CategoryType = "Special";
        //                                    return RedirectToAction("Details", "Episode", new { id = GetMovieEpisodeId(show.CategoryId) });
        //                                }
        //                                else if (show is LiveEvent)
        //                                {
        //                                    ViewBag.CategoryType = "LiveEvent";
        //                                    var episodeCategory = context.EpisodeCategories1.FirstOrDefault(e => e.CategoryId == show.CategoryId);
        //                                    if (episodeCategory != null)
        //                                        return RedirectToAction("Details", "Live", new { id = episodeCategory.EpisodeId, slug = MyUtility.GetSlug(episodeCategory.Episode.EpisodeName) });
        //                                }
        //                            }
        //                            ViewBag.dbSlug = dbSlug;
        //                            ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
        //                            return View(show);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    return RedirectToAction("Index", "Home");
        //}

        public PartialViewResult GetCastMembers(Show show)
        {
            try
            {
                var celebrities = show.CelebrityRoles.Where(cr => cr.Celebrity.StatusId == GlobalConfig.Visible).Select(c => c.Celebrity);
                if (celebrities != null)
                {
                    var names = celebrities.Select(c => c.FullName);
                    return PartialView("_GetCastMembers", string.Join(", ", names));
                }
            }
            catch (Exception) { }
            return PartialView("_GetCastMembers", String.Empty);
        }
        public PartialViewResult GetCastMembersLink(Show show)
        {
            try
            {
                var celebrities = show.CelebrityRoles.Where(cr => cr.Celebrity.StatusId == GlobalConfig.Visible).Select(c => c.Celebrity);
                if (celebrities != null)
                {
                    var names = celebrities.Select(c => String.Format("<a href=\"{0}/Celebrity/Profile/{1}\">{2}</a>", GlobalConfig.baseUrl, c.CelebrityId, c.FullName));
                    return PartialView("_GetCastMembers", string.Join(", ", names));
                }
            }
            catch (Exception) { }
            return PartialView("_GetCastMembers", String.Empty);
        }
        public PartialViewResult GetRelatedContent(int? id, int? episodeId, int? NextEpisodeId, int? PreviousEpisodeId, int? EpisodeNumber, int? EpisodeCount, int? pageSize, string partialViewName = "")
        {
            List<EpisodeObject> list = null;
            try
            {
                ViewBag.PageSize = pageSize;
                if (id != null)
                {
                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                    if (category != null)
                    {
                        if (category.StartDate < registDt && category.EndDate > registDt && category is Show)
                        {
                            string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                            var offering = context.Offerings.Find(GlobalConfig.offeringId);
                            var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);

                            //get subcategories of movie category
                            List<int> subCategoryIds = null;
                            try
                            {
                                var movieCategory = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.Movies);
                                if (movieCategory != null)
                                {
                                    if (movieCategory is Category)
                                    {
                                        var movieSubCategories = ((Category)movieCategory).CategoryClassSubCategories.Select(c => c.SubCategory.CategoryId);
                                        if (movieSubCategories != null)
                                        {
                                            subCategoryIds = movieSubCategories.ToList();
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                            var parents = ((Show)category).GetAllParentCategories();
                            if (parents != null)
                            {
                                if (parents.Count() > 0)
                                {
                                    var intersect = subCategoryIds == null ? parents : parents.Intersect(subCategoryIds);
                                    SortedSet<int> showIds = new SortedSet<int>();
                                    foreach (var item in intersect)
                                    {
                                        var parentCategory = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == item);
                                        if (parentCategory is Category)
                                            showIds.UnionWith(service.GetAllOnlineShowIds(CountryCode, (Category)parentCategory));
                                    }

                                    if (showIds.Count() > 0)
                                    {
                                        List<int> eIds = null;
                                        using (var ec = new EngagementsModel.EngagementsEntities())
                                        {
                                            var eShowIds = ec.ShowReactions.Where(s => s.ReactionTypeId == GlobalConfig.SOCIAL_LOVE).GroupBy(s => s.CategoryId)
                                                .Select(s => new { count = s.Count(), id = s.Key }).OrderByDescending(s => s.count).Select(s => s.id);
                                            if (eShowIds != null)
                                                eIds = eShowIds.ToList();

                                        }
                                        if (eIds != null)
                                        {
                                            var shows = context.CategoryClasses.Where(c => showIds.Contains(c.CategoryId));
                                            if (shows != null)
                                            {
                                                Dictionary<int, CategoryClass> d = shows.ToDictionary(x => x.CategoryId);
                                                List<CategoryClass> ordered = new List<CategoryClass>();
                                                foreach (var i in eIds)
                                                {
                                                    if (d.ContainsKey(i))
                                                        ordered.Add(d[i]);
                                                }

                                                list = new List<EpisodeObject>();
                                                foreach (var show in ordered)
                                                {
                                                    if (show is Movie)
                                                    {
                                                        string img = GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif;
                                                        if (!String.IsNullOrEmpty(show.ImagePoster))
                                                            img = String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId, show.ImageMobileLandscape);
                                                        list.Add(new EpisodeObject()
                                                        {
                                                            EpisodeId = show.CategoryId,
                                                            Name = show.Description,
                                                            DateAiredStr = show.StartDate.Value.ToString("MMMM d, yyyy"),
                                                            Synopsis = show.Blurb,
                                                            ImgUrl = img,
                                                            Show = show.Description,
                                                            slug = MyUtility.GetSlug(show.Description)
                                                        });

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, list);
            return PartialView(list);
        }
        public PartialViewResult GetRelatedSpecials(int? id, int? episodeId, int? NextEpisodeId, int? PreviousEpisodeId, int? EpisodeNumber, int? EpisodeCount, int? pageSize, string partialViewName = "")
        {
            List<EpisodeObject> list = null;
            try
            {
                ViewBag.PageSize = pageSize;
                if (id != null)
                {
                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                    if (category != null)
                    {
                        if (category.StartDate < registDt && category.EndDate > registDt && category is Show)
                        {
                            string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                            var offering = context.Offerings.Find(GlobalConfig.offeringId);
                            var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);

                            var specialCurrentCategory = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.SpecialsCurrentCategoryId);
                            if (specialCurrentCategory != null)
                            {
                                if (specialCurrentCategory is Category)
                                {
                                    SortedSet<int> showIds = new SortedSet<int>();
                                    showIds.UnionWith(service.GetAllOnlineShowIds(CountryCode, (Category)specialCurrentCategory));
                                    if (showIds.Count() > 0)
                                    {
                                        showIds.Remove(category.CategoryId); // remove the current id from list;                                        
                                        var shows = context.CategoryClasses.Where(c => showIds.Contains(c.CategoryId)).OrderByDescending(c => c.StartDate);
                                        if (shows != null)
                                        {
                                            list = new List<EpisodeObject>();
                                            foreach (var show in shows)
                                            {
                                                if (show is SpecialShow)
                                                {
                                                    string img = GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif;
                                                    if (!String.IsNullOrEmpty(show.ImagePoster))
                                                        img = String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId, show.ImageMobileLandscape);
                                                    list.Add(new EpisodeObject()
                                                    {
                                                        EpisodeId = show.CategoryId,
                                                        Name = show.Description,
                                                        DateAiredStr = show.StartDate.Value.ToString("MMMM d, yyyy"),
                                                        Synopsis = show.Blurb,
                                                        ImgUrl = img,
                                                        Show = show.Description,
                                                        slug = MyUtility.GetSlug(show.Description)
                                                    });

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, list);
            return PartialView(list);
        }

        [RequireHttp]
        public ActionResult List(int? id, string slug)
        {
            try
            {
                if (id != null)
                {
                    ViewBag.featureType = "show";
                    var context = new IPTV2Entities();
                    var feature = context.Features.FirstOrDefault(f => f.FeatureId == id);
                    if (feature != null)
                        return View("FeatureList", feature);
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }
    }
}
