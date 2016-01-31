using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;
using StackExchange.Profiling;
using Newtonsoft.Json;

namespace TFCTV.Controllers
{
    public class PackagesController : Controller
    {
        //
        // GET: /Packages/
        public ActionResult Index()
        {
            return RedirectToAction("Details", "Subscribe");
            //return RedirectToAction("Index", "Home");
            //Premium
            var context = new IPTV2Entities();
            string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
            string currencycode = MyUtility.GetCurrencyOrDefault(countrycode);

            //test if currentcurrycode is available on the ProductPricelist
            if (context.ProductPrices.FirstOrDefault(p => p.CurrencyCode == currencycode) == null)
            {
                currencycode = GlobalConfig.DefaultCurrency;
            }

            var datalist = context.ProductPackages.Join(
                 context.ProductPrices,
                 prpk => prpk.ProductId,
                 prp => prp.ProductId,
                 (prpk, prp) => new { ProductPackages = prpk, ProductPrices = prp })
                 .Where(p => p.ProductPackages.PackageId == GlobalConfig.premiumId
                  && p.ProductPrices.CurrencyCode == currencycode
                  && p.ProductPackages.Product.DurationType == "m"
                  && p.ProductPackages.Product.Duration == 1
                  && p.ProductPrices.Product.IsForSale)
                 .Select(p =>
                     new
                     {
                         packagename = p.ProductPackages.Package.PackageName,
                         duration = p.ProductPackages.Product.Duration,
                         durationtype = p.ProductPackages.Product.DurationType,
                         packageid = p.ProductPackages.PackageId,
                         amount = p.ProductPrices.Amount,
                         productname = p.ProductPackages.Product.Name,
                         productid = p.ProductPackages.ProductId,
                         // countrycode = p.ProductPrices.CountryCode,
                         symbol = p.ProductPrices.Currency.Symbol,
                         currencycode = p.ProductPrices.CurrencyCode,
                         isleft = p.ProductPrices.Currency.IsLeft
                     }
                 ).OrderBy(p => new { p.amount }).FirstOrDefault();
            if (datalist != null)
            {
                if (datalist.currencycode == "AED" || datalist.currencycode == "SAR")
                {
                    ViewBag.LowestSubscription = datalist.currencycode + " " + datalist.amount.ToString("F") + " / Month";
                }
                else
                {
                    if (datalist.isleft)
                    {
                        ViewBag.LowestSubscription = datalist.symbol + datalist.amount.ToString("F") + " / Month";
                    }
                    else
                    {
                        ViewBag.LowestSubscription = datalist.amount.ToString("F") + datalist.symbol + " / Month";
                    }
                }
            }

            var data = context.ProductPrices.Join(
                 context.ProductShows,
                 pp => pp.ProductId,
                 ps => ps.ProductId,
                 (pp, ps) => new { ProductPrices = pp, ProductShows = ps })
                 .Where(p => p.ProductShows.Show.StatusId == GlobalConfig.Visible
                     && p.ProductShows.Product.OfferingId == GlobalConfig.offeringId
                     && p.ProductPrices.CurrencyCode == currencycode
                     && p.ProductShows.Product.DurationType == "m"
                     && p.ProductShows.Product.Duration == 1
                     && p.ProductPrices.Product.IsForSale)
                 .OrderBy(pp => pp.ProductPrices.Amount)
                 .Select(p => new
                 {
                     amount = p.ProductPrices.Amount,
                     currencycode = p.ProductPrices.CurrencyCode,
                     symbol = p.ProductPrices.Currency.Symbol,
                     isleft = p.ProductPrices.Currency.IsLeft
                 }).FirstOrDefault();

            if (data != null)
            {
                if (data.currencycode == "AED" || data.currencycode == "SAR")
                {
                    ViewBag.LowestAlacarte = data.currencycode + " " + data.amount.ToString("F") + " / Video";
                }
                else
                {
                    if (data.isleft)
                    {
                        ViewBag.LowestAlacarte = data.symbol + data.amount.ToString("F") + " / Video";
                    }
                    else
                    {
                        ViewBag.LowestAlacarte = data.amount.ToString("F") + data.symbol + " / Video";
                    }
                }
            }
            return View();
        }

        public ActionResult Subscription(string id)
        {
            return RedirectToAction("Details", "Subscribe");

            //return RedirectToAction("Index", "Home");
            ViewBag.PremiumId = GlobalConfig.premiumId;
            ViewBag.LiteId = GlobalConfig.liteId;
            ViewBag.MovieId = GlobalConfig.movieId;
            ViewBag.Current = id;

            int freeTrialPackageId = 0;
            var list = MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds);

            var registDt = DateTime.Now;

            if (MyUtility.isUserLoggedIn())
            {
                var context = new IPTV2Entities();
                var userId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    var package = user.PackageEntitlements.FirstOrDefault(p => list.Contains(p.PackageId) && p.EndDate > registDt);
                    if (package != null)
                        freeTrialPackageId = package.PackageId;
                }
            }

            //try
            //{
            //    int weekNumber = DateTime.Now.GetWeekOfMonth();
            //    //if (weekNumber == 5) weekNumber = 1;
            //    try { freeTrialPackageId = list.ElementAt(weekNumber - 1); }
            //    catch (ArgumentOutOfRangeException) { freeTrialPackageId = list.ElementAt(0); }
            //}
            //catch (Exception)
            //{
            //    freeTrialPackageId = list.ElementAt(0);
            //}

            ViewBag.FreeTrialPackageId = freeTrialPackageId;
            return View();
        }

        public ActionResult Compare()
        {
            //return RedirectToAction("Index", "Home");
            string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
            ViewBag.HideLitePackage = true;
            var context = new IPTV2Entities();
            List<PackageType> package_list = context.ProductPackages
                                        .Where(p => p.Product.IsForSale && p.Package.StatusId == GlobalConfig.Visible)
                                        .AsEnumerable()
                                        .Where(p => p.Product.IsAllowed(countrycode))
                                        .Select(p => p.Package)
                                        .ToList();
            foreach (var item in package_list)
            {
                if (item.PackageId == GlobalConfig.liteId)
                {
                    ViewBag.HideLitePackage = false;
                }
            }

            // = "US" == MyUtility.GetCurrentCountryCodeOrDefault() ? true : false;
            return View();
        }

        private List<ProductShow> GetShows(int?
            alacarteID, string search, int? page)
        {
            List<ProductShow> showslist = new List<ProductShow>();

            var context = new IPTV2Entities();
            string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();

            var prodShow = context.ProductShows
                .Where(p => p.Product.ALaCarteSubscriptionTypeId == alacarteID
                    && p.Product.IsForSale
                    && p.Product.OfferingId == GlobalConfig.offeringId
                    && p.Product.StatusId == GlobalConfig.Visible
                    && p.Show.CategoryName.Contains(string.IsNullOrEmpty(search) ? p.Show.CategoryName : search))
                .AsEnumerable()
                .Where(p => p.Product.IsAllowed(countrycode));
            if (prodShow.Count() > 0)
            {
                var prodShowC = prodShow.Where(ps => ps.Product.ProductPrices.Where(pp => pp.CurrencyCode == countrycode).Count() > 0).ToList();
                if (prodShowC.Count() > 0)
                {   //use user current currency
                    showslist = prodShowC;
                }
                else
                {   //use default currency
                    showslist = prodShow.Where(ps => ps.Product.ProductPrices.Where(pp => pp.CurrencyCode == GlobalConfig.DefaultCurrency).Count() > 0).ToList();
                }
            }

            return showslist;
        }

        public ActionResult PerSerye(string search, int? page)
        {
            return RedirectToAction("Details", "Subscribe");
            //return RedirectToAction("Index", "Home");
            var showslist = GetShows(2, search, page);

            ViewBag.Search = search;

            return View(showslist);
        }

        public ActionResult PerShow(string search, int? page)
        {
            return RedirectToAction("Details", "Subscribe");
            //return RedirectToAction("Index", "Home");
            var showslist = GetShows(1, search, page);

            ViewBag.Search = search;
            return PartialView(showslist);
        }

        public ActionResult PerMovie(string search, int? page)
        {
            return RedirectToAction("Details", "Subscribe");
            //return RedirectToAction("Index", "Home");
            var showslist = GetShows(4, search, page);

            ViewBag.Search = search;
            return View(showslist);
        }

        public ActionResult PerSpecial(string search, int? page)
        {
            return RedirectToAction("Details", "Subscribe");
            //return RedirectToAction("Index", "Home");
            var showslist = GetShows(3, search, page);

            ViewBag.Search = search;
            return View(showslist);
        }

        public ActionResult PerSports(string search, int? page)
        {
            return RedirectToAction("Details", "Subscribe");
            //return RedirectToAction("Index", "Home");
            var showslist = GetShows(5, search, page);
            ViewBag.Search = search;
            return View(showslist);
        }

        #region AJAX Actions For Subscription

        /// <summary>
        /// Description: Get All Packages under a particular Offering
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPackages()
        {
            var context = new IPTV2Entities();

            Dictionary<string, int?> packagelist = new Dictionary<string, int?>();
            IPTV2_Model.Package package = new IPTV2_Model.Package();
            string CountryCode = MyUtility.GetCurrentCountryCodeOrDefault();
            foreach (var item in context.PackageTypes.Where(ptype => ptype.OfferingId == GlobalConfig.offeringId && ptype.Country == CountryCode))
            {
                if (item is Package)
                    packagelist.Add(item.PackageName, item.PackageId);
            }

            return Json(packagelist, JsonRequestBehavior.AllowGet);
        }

        private string GetAmount(int productid)
        {
            var context = new IPTV2Entities();
            string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
            string currencycode = MyUtility.GetCurrencyOrDefault(countrycode);

            var data = context.ProductPrices
                .Where(p => p.ProductId == productid
                && p.CurrencyCode == currencycode
                && p.Product.OfferingId == GlobalConfig.offeringId
                && p.Product.StatusId == GlobalConfig.Visible
                && p.Product.IsForSale)
                .AsEnumerable()
                .FirstOrDefault(p => p.Product.IsAllowed(countrycode));

            if (data != null)
            {
                if (data.Currency.IsLeft)
                {
                    return String.Format("{0}{1}", data.Currency.Code, data.Amount.ToString("F"));
                }
                return String.Format("{0}{1}", data.Amount.ToString("F"), data.Currency.Code);
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

                return String.Format("{0}{1}", data.Currency.Code, data.Amount.ToString("F"));
            }
        }

        private bool IsAllowed(int packageid, string countrycode)
        {
            var context = new IPTV2Entities();
            //check if package is allowed
            if (context.ProductPackages
                .Where(pp => pp.PackageId == packageid && pp.Package.StatusId == GlobalConfig.Visible)
                .AsEnumerable()
                .Where(pp => pp.Product.IsAllowed(countrycode))
                .Count() == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Description: Get All Products under a particular Package
        /// </summary>
        /// <param name="packageid"></param>
        /// <returns></returns>
        public ActionResult GetProducts(int packageid)
        {
            return RedirectToAction("Details", "Subscribe");
            var context = new IPTV2Entities();
            string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
            string currencycode = MyUtility.GetCurrencyOrDefault(countrycode);

            List<ProductGroup> upgradable_productgrouplist = new List<ProductGroup>();
            List<int> productGrpIds = new List<int>();
            List<ProductPackage> upgradable_productlist = new List<ProductPackage>();
            List<PackageProductUpgradeDisplay> upgradable_packagelist = new List<PackageProductUpgradeDisplay>();

            if (!this.IsAllowed(packageid, countrycode))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            var datalist = context.ProductPackages
                                    .Where(pd => pd.PackageId == packageid
                                        && pd.Product.ProductPrices.Where(pp => pp.CurrencyCode == currencycode
                                        && pp.ProductId == pd.ProductId).Count() > 0
                                        && pd.Product.IsForSale
                                    );
            if (datalist != null)
            {
                datalist = context.ProductPackages
                               .Where(pd => pd.PackageId == packageid
                                   && pd.Product.ProductPrices.Where(pp => pp.CurrencyCode == GlobalConfig.DefaultCurrency
                                   && pp.ProductId == pd.ProductId).Count() > 0
                                   && pd.Product.IsForSale
                               );
            }

            //Check if datalist has no records
            if (datalist == null)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            //Check if user has Package Entitlements
            foreach (var item in UserPackages.GetUser_EntitlementsList())
            {
                if (item is PackageEntitlement)
                {
                    var pck = (PackageEntitlement)item;

                    int productGrpID = context.ProductPackages
                    .FirstOrDefault(p => p.PackageId == pck.PackageId)
                    .Product
                    .ProductGroupId;

                    upgradable_productgrouplist.AddRange(
                                      UserPackages.GetUpgradableProductGroup(productGrpID)
                                      );

                    productGrpIds.AddRange(upgradable_productgrouplist.Select(p => p.ProductGroupId).ToArray());

                    if (upgradable_productgrouplist.Count() > 0)
                    {
                        //user curreny currency
                        upgradable_productlist = context.ProductPackages
                                       .Where(pd => productGrpIds.Contains(pd.Product.ProductGroupId)
                                           && pd.Product.IsForSale
                                        )
                                        .AsEnumerable()
                                        .Where(p => p.Product.IsAllowed(countrycode))
                                        .OrderBy(pp => pp.PackageId)
                                        .ThenByDescending(pp => pp.Product.DurationType)
                                        .ThenByDescending(ps => ps.Product.Duration)
                                        .ToList();

                        var upgradable_list = upgradable_productlist.GroupBy(pc => new { pc.PackageId, pc.Package.PackageName })
                                                      .Select(group => new
                                                      {
                                                          group.Key.PackageId,
                                                          group.Key.PackageName
                                                      });
                        foreach (var upgrade_item in upgradable_list)
                        {
                            upgradable_packagelist.Add(new PackageProductUpgradeDisplay
                            {
                                CurrentPackageId = (pck.PackageId),
                                PackageId = upgrade_item.PackageId,
                                PackageName = upgrade_item.PackageName,
                                CurrentProductId = (item.LatestEntitlementRequest != null ?
                                                    (int?)item.LatestEntitlementRequest.ProductId
                                                    : null)
                            });
                        }
                    }
                }
            }

            var data = datalist
                .Select(p => new
              {
                  packagename = p.Package.PackageName,
                  duration = p.Product.Duration,
                  durationtype = p.Product.DurationType,
                  packageid = p.PackageId,
                  amount = ((decimal?)p.Product.ProductPrices.FirstOrDefault(pr => pr.ProductId == p.ProductId && pr.CurrencyCode == currencycode).Amount)
                          ?? p.Product.ProductPrices.FirstOrDefault(pr => pr.ProductId == p.ProductId && pr.CurrencyCode == GlobalConfig.DefaultCurrency).Amount,
                  productname = p.Product.Name,
                  productid = p.ProductId,
                  countrycode = "",
                  symbol = p.Product.ProductPrices.FirstOrDefault(pr => pr.ProductId == p.ProductId && pr.CurrencyCode == currencycode).Currency.Symbol ??
                           p.Product.ProductPrices.FirstOrDefault(pr => pr.ProductId == p.ProductId && pr.CurrencyCode == GlobalConfig.DefaultCurrency).Currency.Symbol,

                  isleft = ((bool?)p.Product.ProductPrices.FirstOrDefault(pr => pr.ProductId == p.ProductId && pr.CurrencyCode == currencycode).Currency.IsLeft) ??
                           p.Product.ProductPrices.FirstOrDefault(pr => pr.ProductId == p.ProductId && pr.CurrencyCode == GlobalConfig.DefaultCurrency).Currency.IsLeft,

                  currencycode = p.Product.ProductPrices.FirstOrDefault(pr => pr.ProductId == p.ProductId && pr.CurrencyCode == currencycode).Currency.Code ??
                           p.Product.ProductPrices.FirstOrDefault(pr => pr.ProductId == p.ProductId && pr.CurrencyCode == GlobalConfig.DefaultCurrency).Currency.Code,

                  currentpackageid = (int?)null,
                  currentproductid = (int?)null,
                  upgrade_packageId = (int?)null,
                  upgrade_packagename = (string)null
              }).OrderBy(p => new { p.durationtype, p.duration }).ToList();

            if (upgradable_packagelist.Count() > 0)
            {
                var returndata = from d in data
                                 from up in upgradable_packagelist.Where(up => up.PackageId == d.packageid).DefaultIfEmpty()
                                 orderby d.durationtype, d.duration
                                 select new
                                 {
                                     duration = d.duration
                                   ,
                                     durationtype = d.durationtype
                                   ,
                                     packageid = d.packageid
                                   ,
                                     packagename = d.packagename
                                   ,
                                     amount = d.amount
                                   ,
                                     productid = d.productid
                                   ,
                                     productname = d.productname
                                   ,
                                     countrycode = d.countrycode
                                   ,
                                     symbol = d.symbol
                                   ,
                                     isleft = d.isleft
                                   ,
                                     currencycode = d.currencycode
                                   ,
                                     currentpackageid = (up == null ? (int?)null : up.CurrentPackageId)
                                   ,
                                     currentproductid = (up == null ? (int?)null : up.CurrentProductId)
                                   ,
                                     upgrade_packageId = (up == null ? (int?)null : up.PackageId)
                                   ,
                                     upgrade_packagename = (up == null ? (string)null : up.PackageName)
                                 };

                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Description: Get Shows that belongs to a particular Package
        /// </summary>
        /// <param name="packageid"></param>
        /// <returns></returns>
        public ActionResult GetShows(int packageid)
        {
            List<ShowLookUpObject> datalist = null;
            var cache = DataCache.Cache;
            string cacheKey = "PKGDLGTSHOWS:0;P:" + packageid;
            datalist = (List<ShowLookUpObject>)cache[cacheKey];

            ////No Lite Package on US
            //if (countrycode == "US" && packageid == GlobalConfig.liteId)
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}


            if (datalist == null)
            {
                string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
                DateTime registDt = DateTime.Now;
                datalist = new List<ShowLookUpObject>();

                var context = new IPTV2Entities();
                if (!this.IsAllowed(packageid, countrycode))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.StatusId == GlobalConfig.Visible);
                var maincategory = context.PackageCategories.Where(pc => pc.PackageId == packageid).Select(p => p.Category);
                var subcategory = maincategory.Select(p => p.SubCategories);
                int[] category_ids = context.PackageCategories.Where(pc => pc.PackageId == packageid).Select(p => p.CategoryId).ToArray();

                //for movies
                foreach (var movie in maincategory)
                {
                    if (movie.SubCategories.Count() == 0)
                    {
                        int[] movieIds = service.GetAllOnlineShowIds(MyUtility.GetCurrentCountryCodeOrDefault(), movie).ToArray();
                        foreach (var item in context.CategoryClasses
                                    .Where(s => movieIds.Contains(s.CategoryId)
                                        && s.StatusId == GlobalConfig.Visible))
                        {
                            if (item is Show)
                            {
                                var show = (Show)item;
                                ShowLookUpObject data = new ShowLookUpObject();
                                data.Show = show.Description;
                                data.ShowId = show.CategoryId;
                                data.MainCategory = movie.Description;
                                data.MainCategoryId = movie.CategoryId;
                                //data.SubCategory = show.CategoryClassSubCategories.FirstOrDefault().SubCategory.CategoryName;
                                //data.SubCategoryId = show.CategoryClassSubCategories.FirstOrDefault().SubCategory.CategoryId;
                                //datalist.Add(data);
                                if (show.StartDate != null && show.EndDate != null)
                                {
                                    if (show.StartDate < registDt && show.EndDate > registDt)
                                        datalist.Add(data);
                                }
                                else
                                    datalist.Add(data);
                            }
                        }
                    }
                }

                //for shows
                foreach (var subitems in subcategory)
                {
                    foreach (Category item in subitems)
                    {
                        int[] showIds = service.GetAllOnlineShowIds(MyUtility.GetCurrentCountryCodeOrDefault(), item).ToArray();

                        foreach (var cat in context.CategoryClasses
                                    .Where(s => showIds.Contains(s.CategoryId)
                                        && s.StatusId == GlobalConfig.Visible))
                        {
                            if (cat is Show)
                            {
                                var show = (Show)cat;
                                ShowLookUpObject data = new ShowLookUpObject();
                                data.Show = show.Description;
                                data.ShowId = show.CategoryId;
                                data.MainCategory = item.Description;
                                data.MainCategoryId = item.CategoryId;
                                //data.SubCategory = show.CategoryClassSubCategories.FirstOrDefault().SubCategory.CategoryName;
                                //data.SubCategoryId = show.CategoryClassSubCategories.FirstOrDefault().SubCategory.CategoryId;
                                //datalist.Add(data);
                                if (show.StartDate != null && show.EndDate != null)
                                {
                                    if (show.StartDate < registDt && show.EndDate > registDt)
                                        datalist.Add(data);
                                }
                                else
                                    datalist.Add(data);
                            }
                        }
                    }
                }

                var cacheDuration = new TimeSpan(0, 60, 0);
                cache.Put(cacheKey, datalist, cacheDuration);
            }


            //Arrange MMK to be on Top;
            List<ShowLookUpObject> clone_datalist = datalist.Where(p => p.ShowId == 310).ToList();
            if (clone_datalist.Count() > 0)
            {
                int mmk_main_id = clone_datalist.FirstOrDefault().MainCategoryId;
                clone_datalist.AddRange(datalist.Where(p => p.MainCategoryId == mmk_main_id && p.ShowId != 310).ToList());
                clone_datalist.AddRange(datalist.Where(p => p.MainCategoryId != mmk_main_id).OrderBy(p => p.MainCategoryId).ToList());
            }
            else
            {
                clone_datalist = datalist;
            }

            return Json(clone_datalist, JsonRequestBehavior.AllowGet);
        }

        #endregion AJAX Actions For Subscription



        //public PartialViewResult GetLowestPackages(string countryCode)
        //{
        //    PackageProductPrices packageProductPrices = new PackageProductPrices();
        //    try
        //    {
        //        var packageIds = MyUtility.StringToIntList(GlobalConfig.TFCTVPackageIds);
        //        packageProductPrices = packageProductPrices.LoadAllPackageAndProduct(packageIds, countryCode, true);
        //        //if (packageProductPrices == null)
        //        //    return null;
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    //return PartialView("_GetShowProductPackages", showPackageProductPrices);
        //    return PartialView(packageProductPrices);
        //}

        public ActionResult TVE(int? id)
        {
            if (!GlobalConfig.IsTVERegistrationEnabled)
                return RedirectToAction("Index", "Home");
            if (!MyUtility.IsTVEAllowedInCurrentCountry())
                return RedirectToAction("Index", "Home");

            var profiler = MiniProfiler.Current;

            if (!Request.Cookies.AllKeys.Contains("version"))
            {
                try
                {
                    List<Category> subcategories = null;
                    var context = new IPTV2Entities();
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.TFCEverywhereParentCategoryId);
                    if (category != null)
                    {
                        if (category is Category)
                        {
                            var parent = (Category)category;
                            if (parent.SubCategories.Count(s => s.StatusId == GlobalConfig.Visible) > 0)
                            {
                                var list = parent.SubCategories.Where(s => s.StatusId == GlobalConfig.Visible);
                                if (list != null)
                                {
                                    subcategories = list.ToList();
                                    List<Category> ordered = null;
                                    using (profiler.Step("Arrange List of Categories"))
                                    {
                                        try
                                        {
                                            ordered = new List<Category>();
                                            var menuids = MyUtility.StringToIntList(GlobalConfig.TVEMenuIds);
                                            Dictionary<int, Category> d = subcategories.ToDictionary(x => x.CategoryId);
                                            foreach (var i in menuids)
                                            {
                                                if (d.ContainsKey(i))
                                                    ordered.Add(d[i]);
                                            }
                                        }
                                        catch (Exception) { ordered = subcategories; }
                                    }
                                    return View("TVE2", ordered);
                                }
                            }
                        }
                    }
                }
                catch (Exception e) { MyUtility.LogException(e); }
            }

            else
            {
                try
                {
                    ViewBag.hasStreamingLink = false;
                    ViewBag.ChannelId = 0;
                    List<Category> subcategories = null;
                    var context = new IPTV2Entities();
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.TFCEverywhereParentCategoryId);
                    if (category != null)
                    {
                        if (category is Category)
                        {
                            var parent = (Category)category;
                            if (parent.SubCategories.Count(s => s.StatusId == GlobalConfig.Visible) > 0)
                            {
                                Category firstCategory = null;
                                subcategories = parent.SubCategories.Where(s => s.StatusId == GlobalConfig.Visible).ToList();
                                List<Category> ordered = null;
                                using (profiler.Step("Arrange List of Categories"))
                                {
                                    //Arrange List, failover on subcategories
                                    try
                                    {
                                        ordered = new List<Category>();
                                        var menuids = MyUtility.StringToIntList(GlobalConfig.TVEMenuIds);
                                        Dictionary<int, Category> d = subcategories.ToDictionary(x => x.CategoryId);
                                        foreach (var i in menuids)
                                        {
                                            if (d.ContainsKey(i))
                                                ordered.Add(d[i]);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        ordered = subcategories;
                                    }
                                }
                                ViewBag.ListOfTFCEverwhereSubCategories = ordered;
                                if (id == null)
                                {
                                    firstCategory = subcategories.First();
                                    id = firstCategory.CategoryId;
                                }
                                else
                                {
                                    var tempCat = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
                                    if (tempCat != null)
                                        if (tempCat is Category)
                                            firstCategory = (Category)tempCat;
                                }

                                ViewBag.categoryChannelId = id;

                                try
                                {
                                    var categoryIdList = MyUtility.StringToIntList(GlobalConfig.StreamingTVECategoryIds);
                                    var hasStreamingLink = categoryIdList.Contains((int)id);
                                    ViewBag.hasStreamingLink = hasStreamingLink;
                                    //var channelId = 0;
                                    var episodeId = 0;
                                    if (hasStreamingLink)
                                    {
                                        var parts = GlobalConfig.TVEStreamingEpisodeCounterpart.Split(';');
                                        foreach (var part in parts)
                                        {
                                            var temp = part.Split('|');
                                            var MultiChannelId = Convert.ToInt32(temp[0]);
                                            if (MultiChannelId == id)
                                            {
                                                //channelId = Convert.ToInt32(temp[1]);
                                                episodeId = Convert.ToInt32(temp[1]);
                                                break;
                                            }
                                        }
                                        //ViewBag.ChannelId = channelId;
                                        ViewBag.EpisodeId = episodeId;

                                    }
                                }
                                catch (Exception) { }


                                if (firstCategory != null)
                                    return View(firstCategory);
                            }
                        }
                    }
                    else
                        return RedirectToAction("Index", "Home");

                }
                catch (Exception) { }
                return View();
            }
            return RedirectToAction("Index", "Home");
        }


        public ActionResult GetChannelContent(int? id)
        {

            List<ShowLookUpObject> list = new List<ShowLookUpObject>();
            try
            {

                if (!GlobalConfig.IsTVERegistrationEnabled)
                    return Json(list, JsonRequestBehavior.AllowGet);

                DateTime registDt = DateTime.Now;
                var context = new IPTV2Entities();

                var countryCode = MyUtility.GetCurrentCountryCodeOrDefault();

                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.StatusId == GlobalConfig.Visible);
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
                if (category != null)
                {
                    if (category.CategoryClassParentCategories.Count(c => c.ParentCategory.CategoryId == GlobalConfig.TFCEverywhereParentCategoryId) <= 0)
                        return Json(list, JsonRequestBehavior.AllowGet);

                    if (category is Category)
                    {
                        var parent = (Category)category;
                        var subCategoryies = parent.SubCategories;
                        if (subCategoryies.Count() > 0)
                        {
                            foreach (var subCategory in subCategoryies)
                            {
                                int[] showIds = service.GetAllOnlineShowIds(countryCode, subCategory).ToArray();
                                var shows = context.CategoryClasses.Where(c => showIds.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible && c is Show);
                                foreach (var item in shows)
                                {
                                    if (item is Show)
                                    {
                                        var show = (Show)item;
                                        ShowLookUpObject data = new ShowLookUpObject();
                                        data.Show = show.Description;
                                        data.ShowId = show.CategoryId;
                                        data.MainCategory = subCategory.Description;
                                        data.MainCategoryId = subCategory.CategoryId;
                                        if (show.StartDate < registDt && show.EndDate > registDt)
                                            list.Add(data);
                                    }
                                    else if (item is Category)
                                    {
                                        // Loop thru the category again
                                        var subCategories2 = ((Category)item).SubCategories;
                                        if (subCategories2.Count() > 0)
                                        {
                                            foreach (var subCategory2 in subCategories2)
                                            {
                                                int[] showIds2 = service.GetAllOnlineShowIds(countryCode, subCategory2).ToArray();
                                                var shows2 = context.CategoryClasses.Where(c => showIds2.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible && c is Show);
                                                foreach (var item2 in shows2)
                                                {
                                                    if (item2 is Show)
                                                    {
                                                        var show2 = (Show)item2;
                                                        ShowLookUpObject data = new ShowLookUpObject();
                                                        data.Show = show2.Description;
                                                        data.ShowId = show2.CategoryId;
                                                        data.MainCategory = subCategory2.Description;
                                                        data.MainCategoryId = subCategory2.CategoryId;
                                                        if (show2.StartDate < registDt && show2.EndDate > registDt)
                                                            list.Add(data);
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
                else
                    return RedirectToAction("Index", "Home");

            }
            catch (Exception) { }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TVE2(int? id)
        {
            var profiler = MiniProfiler.Current;
            try
            {
                List<Category> subcategories = null;
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.TFCEverywhereParentCategoryId);
                if (category != null)
                {
                    if (category is Category)
                    {
                        var parent = (Category)category;
                        if (parent.SubCategories.Count(s => s.StatusId == GlobalConfig.Visible) > 0)
                        {
                            var list = parent.SubCategories.Where(s => s.StatusId == GlobalConfig.Visible);
                            if (list != null)
                            {
                                subcategories = list.ToList();
                                List<Category> ordered = null;
                                using (profiler.Step("Arrange List of Categories"))
                                {
                                    try
                                    {
                                        ordered = new List<Category>();
                                        var menuids = MyUtility.StringToIntList(GlobalConfig.TVEMenuIds);
                                        Dictionary<int, Category> d = subcategories.ToDictionary(x => x.CategoryId);
                                        foreach (var i in menuids)
                                        {
                                            if (d.ContainsKey(i))
                                                ordered.Add(d[i]);
                                        }
                                    }
                                    catch (Exception) { ordered = subcategories; }
                                }
                                return View(ordered);
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }

        public PartialViewResult BuildTVEContentList(Category category, int ctr = 0)
        {
            List<TVEContentListObj> list = null;
            string jsonString = String.Empty;
            try
            {
                DateTime registDt = DateTime.Now;
                var context = new IPTV2Entities();
                var CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();

                var cache = DataCache.Cache;
                string cacheKey = "BTVECL:O:" + category.CategoryId.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    if (category != null)
                    {
                        if (category is Category)
                        {
                            var parent = (Category)category;
                            var subCategoryies = parent.SubCategories;
                            if (subCategoryies.Count() > 0)
                            {
                                list = new List<TVEContentListObj>();
                                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.StatusId == GlobalConfig.Visible);
                                try
                                {
                                    foreach (var subCategory in subCategoryies)
                                    {
                                        try
                                        {
                                            if (subCategory is Category)
                                            {
                                                var obj = new TVEContentListObj()
                                                {
                                                    MainCategory = subCategory.Description,
                                                    MainCategoryId = subCategory.CategoryId,
                                                    shows = new List<ShowD>()
                                                };
                                                var showIds = service.GetAllOnlineShowIds(CountryCode, subCategory);
                                                if (showIds.Count() > 0)
                                                {
                                                    var shows = context.CategoryClasses.Where(c => showIds.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible);
                                                    foreach (var item in shows)
                                                    {
                                                        if (item is Show && item.StartDate < registDt && item.EndDate > registDt)
                                                        {
                                                            var show = (Show)item;
                                                            obj.shows.Add(new ShowD() { id = show.CategoryId, name = show.Description });
                                                        }
                                                    }
                                                    if (obj.shows.Count() > 0)
                                                        list.Add(obj);
                                                }
                                            }
                                        }
                                        catch (Exception) { }
                                    }
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                    cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                }
                else
                    list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TVEContentListObj>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            ViewBag.Category = category;
            ViewBag.IsActive = ctr == 0 ? true : false;
            return PartialView(list);
        }
    }
}