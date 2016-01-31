using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Helpers;
using IPTV2_Model;
using StackExchange.Profiling;
namespace TFCTV.Controllers
{
    public class FPJController : Controller
    {
        //
        // GET: /FPJ/

        public ActionResult Index()
        {
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            try
            {
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.FPJCategoryId);
                if (category != null)
                {
                    if (category.StatusId == GlobalConfig.Visible && category.StartDate < registDt && category.EndDate > registDt)
                    {
                        var item = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FPJProductId);
                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                        if (User.Identity.IsAuthenticated)
                        {
                            var UserId = new Guid(User.Identity.Name);
                            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                            if (user != null)
                            {
                                var offering = context.Offerings.Find(GlobalConfig.offeringId);
                                using (profiler.Step("Check for Active Entitlements")) //check for active entitlements based on categoryId
                                {
                                    ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnProducts(context, user, offering, item);
                                }
                            }
                            var productitem = (PackageSubscriptionProduct)item;
                            var productprice = productitem.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, user.Country.CurrencyCode, true) == 0);
                            if (productprice == null)
                                productprice = productitem.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, GlobalConfig.DefaultCurrency, true) == 0);
                            ViewBag.PriceCurrency = String.Format("{0} {1}", productprice.CurrencyCode, productprice.Amount.ToString("F"));
                        }
                        if (!Request.Cookies.AllKeys.Contains("version"))
                            return View("Index2");
                        return View(GetFPJThematicbundles());

                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Library()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("Library2", GetFPJThematicbundles());
            return View(GetFPJThematicbundles());
        }

        const int pageSize = 20;
        public ActionResult LibraryA(int id = 1)
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("LibraryA2");
            int page = id;
            var context = new IPTV2Entities();
            var registDt = DateTime.Now;
            try
            {
                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);

                var fpjCategory = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.FPJCategoryId);
                var showCategoryIds = service.GetAllOnlineShowIds(MyUtility.GetCountryCodeViaIpAddressWithoutProxy(), (Category)fpjCategory);
                var episodeIds = context.EpisodeCategories1.Where(e => showCategoryIds.Contains(e.CategoryId) && e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderBy(e => e.EpisodeId).Select(e => e.EpisodeId);
                var episodeDelimited = episodeIds.Skip((page - 1) * pageSize).Take(pageSize);

                var totalCount = episodeIds.Count();
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                var totalPage = Math.Ceiling((double)totalCount / pageSize);
                ViewBag.TotalPages = totalPage;
                ViewBag.TotalCount = totalCount;
                var maxCount = page * pageSize > totalCount ? totalCount : page * pageSize;
                ViewBag.OutOf = String.Format("{0} - {1}", (page * pageSize) + 1 - pageSize, maxCount);
                var episodes = context.Episodes.Where(e => episodeDelimited.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);

                ViewBag.Previous = page == 1 ? String.Empty : (page - 1) == 1 ? String.Empty : (page - 1).ToString();
                ViewBag.Next = page == (int)totalPage ? (int)totalPage : page + 1;

                if ((page * pageSize) + 1 - pageSize > totalCount)
                    return RedirectToAction("LibraryA", "FPJ", new { id = String.Empty });
                if (episodes != null)
                    return View(episodes.OrderBy(e => e.EpisodeName));
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
            }
            return RedirectToAction("Index", "Home");
        }

        private List<FPJThematicBundle> GetFPJThematicbundles()
        {
            List<FPJThematicBundle> FPJBundles = new List<FPJThematicBundle>();
            var context = new IPTV2Entities();
            var featureIds = MyUtility.StringToIntList(GlobalConfig.FPJThematicBundleFeatureIds);
            var features = context.Features.Where(f => featureIds.Contains(f.FeatureId) && f.StatusId == GlobalConfig.Visible);
            if (features != null)
            {
                try
                {
                    foreach (var item in features)
                    {
                        var FPJBundle = new FPJThematicBundle();
                        FPJBundle.BundleName = item.Title;
                        FPJBundle.BundleDescription = item.Description;
                        FPJBundle.featuredTitles = new List<FPJFeaturedMovie>();
                        FPJBundle.featureId = item.FeatureId;
                        foreach (var y in item.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible))
                        {
                            if (y is EpisodeFeatureItem)
                            {
                                EpisodeFeatureItem f = (EpisodeFeatureItem)y;
                                FPJFeaturedMovie featuredMovie = new FPJFeaturedMovie();
                                if (f.Episode.EpisodeNumber != null)
                                    if (f.Episode.EpisodeNumber == 1)
                                        FPJBundle.featureBanner = String.IsNullOrEmpty(f.Episode.ImageAssets.ImageHeader) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, f.EpisodeId, f.Episode.ImageAssets.ImageHeader);
                                featuredMovie.episodeNumber = f.Episode.EpisodeNumber ?? default(int);
                                featuredMovie.movieEpisodeID = f.EpisodeId;
                                featuredMovie.movieImage = String.IsNullOrEmpty(f.Episode.ImageAssets.ImageBackground) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, f.EpisodeId, f.Episode.ImageAssets.ImageBackground);
                                featuredMovie.movieTitle = f.Episode.EpisodeName;

                                FPJBundle.featuredTitles.Add(featuredMovie);
                            }
                        }
                        FPJBundle.featuredTitles = FPJBundle.featuredTitles.OrderBy(f => f.episodeNumber).ToList();
                        FPJBundles.Add(FPJBundle);
                    }
                }
                catch (Exception e) { MyUtility.LogException(e); }
            }
            return FPJBundles;
        }

        [RequireHttp]
        public ActionResult Details(int? id, string slug)
        {
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            try
            {
                if (id != null)
                {
                    var context = new IPTV2Entities();
                    var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                    if (episode != null)
                    {
                        if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                        {
                            var dbSlug = MyUtility.GetSlug(episode.Description);
                            SortedSet<int> parentCategories;
                            using (profiler.Step("Check for Parent Shows"))
                            {
                                var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                                parentCategories = episode.GetParentShows(CacheDuration);
                            }
                            string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                            EpisodeCategory episodeCategory = null;
                            if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                            {
                                var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                            }
                            else
                            {
                                var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                            }
                            if (episodeCategory != null)
                            {
                                if (episodeCategory.Show.EndDate < registDt || episodeCategory.Show.StatusId != GlobalConfig.Visible)
                                    return RedirectToAction("Index", "Home");

                                ViewBag.Loved = false; ViewBag.Rated = false; // Social
                                ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };

                                var tempShowNameWithDate = String.Format("{0} {1}", episodeCategory.Show.Description, episode.DateAired.Value.ToString("MMMM d yyyy"));
                                dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
                                if (String.Compare(dbSlug, slug, false) != 0)
                                    return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });

                                if (User.Identity.IsAuthenticated)
                                {
                                    var UserId = new Guid(User.Identity.Name);
                                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                    if (user != null)
                                    {
                                        CountryCode = user.CountryCode;
                                        ViewBag.EmailAddress = user.EMail;
                                        ViewBag.UserId = user.UserId;
                                        ViewBag.CountryCode = CountryCode;
                                        using (profiler.Step("Social Love"))
                                        {
                                            ViewBag.Loved = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_LOVE, id, EngagementContentType.Episode);
                                        }
                                        using (profiler.Step("Social Rating"))
                                        {
                                            ViewBag.Rated = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_RATING, id, EngagementContentType.Episode);
                                        }
                                        using (profiler.Step("Check for Active Entitlements")) //check for active entitlements based on categoryId
                                        {
                                            ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt);
                                        }
                                        if (user.IsTVEverywhere == false)
                                        {
                                            var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                                            var fpjCategory = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.FPJCategoryId);
                                            var showCategoryIds = service.GetAllOnlineShowIds(CountryCode, (Category)fpjCategory);
                                            if (!showCategoryIds.Contains(episodeCategory.Show.CategoryId))
                                                return RedirectToActionPermanent("Details", "Episode", new { id = id, slug = dbSlug });
                                        }
                                    }
                                }
                                else
                                {
                                    using (profiler.Step("Check for Active Entitlements (Not Logged In)")) //check for active entitlements based on categoryId
                                    {
                                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
                                    }
                                }

                                if (!ContextHelper.IsCategoryViewableInUserCountry(episodeCategory.Show, CountryCode))
                                    return RedirectToAction("Index", "Home");

                                ViewBag.Show = episodeCategory.Show;
                                ViewBag.CategoryType = "Show";
                                if (episodeCategory.Show is Movie)
                                    ViewBag.CategoryType = "Movie";
                                ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
                                return View(episode);
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }

        public PartialViewResult GetCasts(int? id)
        {
            var context = new IPTV2Entities();
            try
            {
                Episode episode = context.Episodes.Find(id);
                var celebrities = episode.CelebrityRoles.Select(c => c.Celebrity);
                if (celebrities.Count() > 0)
                    return PartialView("_GetCasts", celebrities);
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
            }
            return null;
        }

        //public ActionResult Index2()
        //{
        //    var profiler = MiniProfiler.Current;
        //    var registDt = DateTime.Now;
        //    try
        //    {
        //        var context = new IPTV2Entities();
        //        var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.FPJCategoryId);
        //        if (category != null)
        //        {
        //            if (category.StatusId == GlobalConfig.Visible && category.StartDate < registDt && category.EndDate > registDt)
        //            {
        //                var item = context.Products.FirstOrDefault(p => p.ProductId == GlobalConfig.FPJProductId);
        //                ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
        //                if (User.Identity.IsAuthenticated)
        //                {
        //                    var UserId = new Guid(User.Identity.Name);
        //                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
        //                    if (user != null)
        //                    {
        //                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
        //                        using (profiler.Step("Check for Active Entitlements")) //check for active entitlements based on categoryId
        //                        {
        //                            ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnProducts(context, user, offering, item);
        //                        }
        //                    }
        //                    var productitem = (PackageSubscriptionProduct)item;
        //                    var productprice = productitem.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, user.Country.CurrencyCode, true) == 0);
        //                    if (productprice == null)
        //                        productprice = productitem.ProductPrices.FirstOrDefault(p => String.Compare(p.CurrencyCode, GlobalConfig.DefaultCurrency, true) == 0);
        //                    ViewBag.PriceCurrency = String.Format("{0} {1}", productprice.CurrencyCode, productprice.Amount.ToString("F"));
        //                }
        //                return View();

        //            }
        //        }
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    return RedirectToAction("Index", "Home");
        //}
        public PartialViewResult GetFPJThemes()
        {
            return PartialView(GetFPJThematicbundles());
        }

        public PartialViewResult GetFPJContent(string id, string partialViewName, int? pageSize, int page = 0, bool is_active = false, bool isOrdered = false)
        {
            var registDt = DateTime.Now;
            List<Episode> obj = null;
            try
            {
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.FPJCategoryId);
                if (category != null)
                {
                    if (category is Category)
                    {
                        string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                        var showIds = service.GetAllOnlineShowIds(CountryCode, (Category)category);
                        var episodeIds = context.EpisodeCategories1.Where(e => showIds.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                        var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                        if (episodes != null)
                        {
                            if (isOrdered)
                                episodes = episodes.OrderBy(e => e.EpisodeName);
                            else
                                episodes = episodes.OrderByDescending(e => e.DateAired).ThenByDescending(e => e.AuditTrail.CreatedOn);
                            obj = pageSize == null ? episodes.ToList() : episodes.Skip(page).Take((int)pageSize).ToList();
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }

            if (!String.IsNullOrEmpty(partialViewName))
            {
                ViewBag.id = id;
                ViewBag.IsActive = is_active;
                ViewBag.isOrdered = isOrdered;
                return PartialView(partialViewName, obj);
            }
            return PartialView(obj);
        }

        public PartialViewResult GetMoreFPJContent(int? pageSize, int page = 0, bool isOrdered = false)
        {
            var registDt = DateTime.Now;
            List<Episode> obj = null;
            int skipSize = 0;
            int size = GlobalConfig.FeatureItemsPageSize;
            try
            {
                if (pageSize != null)
                    size = (int)pageSize;
                skipSize = size * page;
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.FPJCategoryId);
                if (category != null)
                {
                    if (category is Category)
                    {
                        string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                        var showIds = service.GetAllOnlineShowIds(CountryCode, (Category)category);
                        var episodeIds = context.EpisodeCategories1.Where(e => showIds.Contains(e.CategoryId)).Select(e => e.EpisodeId);
                        var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt);
                        if (episodes != null)
                        {
                            if (isOrdered)
                                episodes = episodes.OrderBy(e => e.EpisodeName);
                            else
                                episodes = episodes.OrderByDescending(e => e.DateAired).ThenByDescending(e => e.AuditTrail.CreatedOn);
                            obj = pageSize == null ? episodes.ToList() : episodes.Skip(skipSize).Take((int)pageSize).ToList();
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            ViewBag.page = page + 1;
            ViewBag.pageSize = pageSize;
            return PartialView(obj);
        }

        public PartialViewResult GetFPJFeaturedContent(int featureId, string id, string partialViewName, int? pageSize, int page = 0, bool is_active = false)
        {
            List<HomepageFeatureItem> jfi = null;
            List<HomepageFeatureItem> obj = null;
            string jsonString = String.Empty;
            var registDt = DateTime.Now;
            int size = GlobalConfig.FeatureItemsPageSize;
            int skipSize = 0;
            try
            {
                if (pageSize != null)
                    size = (int)pageSize;
                skipSize = size * page;
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var cache = DataCache.Cache;
                string cacheKey = "JRGL:O:" + featureId + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var feature = context.Features.FirstOrDefault(f => f.FeatureId == featureId && f.StatusId == GlobalConfig.Visible);
                    if (feature != null)
                    {
                        var featureItems = context.FeatureItems.Where(f => f.FeatureId == featureId && f.StatusId == GlobalConfig.Visible).OrderByDescending(f => f.AuditTrail.UpdatedOn);
                        jfi = new List<HomepageFeatureItem>();
                        foreach (var f in featureItems)
                        {
                            if (f is EpisodeFeatureItem)
                            {
                                if (((EpisodeFeatureItem)f).Episode != null)
                                {
                                    var episode = ((EpisodeFeatureItem)f).Episode;
                                    if (episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt && episode.OnlineStatusId == GlobalConfig.Visible)
                                    {
                                        EpisodeCategory epCategory = null;
                                        if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                                        {
                                            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                            epCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                                        }
                                        else
                                        {
                                            var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                            epCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                                        }
                                        if (epCategory != null)
                                        {
                                            Show show = epCategory.Show;
                                            if (show != null)
                                            {
                                                string img = String.IsNullOrEmpty(episode.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, episode.EpisodeId.ToString(), episode.ImageAssets.ImageVideo);
                                                string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                                HomepageFeatureItem j = new HomepageFeatureItem()
                                                {
                                                    id = episode.EpisodeId,
                                                    description = episode.Description,
                                                    name = episode.EpisodeName,
                                                    airdate = (episode.DateAired != null) ? episode.DateAired.Value.ToString("MMMM d, yyyy") : "",
                                                    show_id = show.CategoryId,
                                                    show_name = String.Compare(episode.EpisodeName, episode.EpisodeCode) == 0 ? show.CategoryName : episode.EpisodeName,
                                                    imgurl = img,
                                                    show_imgurl = showImg,
                                                    blurb = HttpUtility.HtmlEncode(episode.Synopsis),
                                                    slug = MyUtility.GetSlug(episode.IsLiveChannelActive == true ? episode.Description : String.Format("{0} {1}", show.Description, episode.DateAired.Value.ToString("MMMM d yyyy")))
                                                };
                                                jfi.Add(j);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (f is ShowFeatureItem)
                            {
                                Show show = ((ShowFeatureItem)f).Show;
                                string img = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    id = show.CategoryId,
                                    name = show.Description,
                                    blurb = HttpUtility.HtmlEncode(show.Blurb),
                                    imgurl = img,
                                    slug = MyUtility.GetSlug(show.Description)
                                };
                                jfi.Add(j);
                            }
                            else if (f is CelebrityFeatureItem)
                            {
                                Celebrity person = ((CelebrityFeatureItem)f).Celebrity;
                                string img = String.IsNullOrEmpty(person.ImageUrl) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.CelebrityImgPath, person.CelebrityId.ToString(), person.ImageUrl);
                                HomepageFeatureItem j = new HomepageFeatureItem()
                                {
                                    name = person.FullName,
                                    id = person.CelebrityId,
                                    imgurl = img,
                                    description = person.Height,
                                    blurb = HttpUtility.HtmlEncode(person.Description),
                                    slug = MyUtility.GetSlug(person.FullName)
                                };
                                jfi.Add(j);
                            }
                        }
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                        cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                if (jfi != null)
                    obj = jfi.Skip(skipSize).Take(size).ToList();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            ViewBag.id = id;
            ViewBag.IsActive = is_active;
            ViewBag.page = page + 1;
            ViewBag.pageSize = pageSize;
            ViewBag.featureId = featureId;
            return PartialView(obj);
        }
        //public ActionResult Library2()
        //{
        //    return View(GetFPJThematicbundles());
        //}
        //public ActionResult LibraryA2()
        //{
        //    return View();
        //}

        public ActionResult OnDemand(int? id, string slug)
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return RedirectToActionPermanent("Details", "Episode", new { id = id, slug = slug });

            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            try
            {
                if (id != null)
                {
                    var context = new IPTV2Entities();
                    var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                    if (episode != null)
                    {
                        if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                        {
                            var dbSlug = MyUtility.GetSlug(episode.Description);
                            if (episode.IsLiveChannelActive == true)
                                return RedirectToActionPermanent("Details", "Live", new { id = id, slug = dbSlug });

                            SortedSet<int> parentCategories;
                            using (profiler.Step("Check for Parent Shows"))
                            {
                                var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                                parentCategories = episode.GetParentShows(CacheDuration);
                                if (parentCategories.Count() > 0)
                                {
                                    if (!parentCategories.Contains(GlobalConfig.FPJParentCategoryId))
                                        return RedirectToActionPermanent("Details", "Episode", new { id = id, slug = dbSlug });
                                }
                            }
                            string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                            EpisodeCategory episodeCategory = null;
                            if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
                            {
                                if (User.Identity.IsAuthenticated)
                                {
                                    var UserId = new Guid(User.Identity.Name);
                                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                    if (user != null)
                                    {
                                        var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                                        var isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
                                        if (isEmailAllowed)
                                            CountryCode = user.CountryCode;
                                    }
                                }
                                var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                            }
                            else
                            {
                                var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                                episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
                            }
                            if (episodeCategory != null)
                            {
                                if (episodeCategory.Show.EndDate < registDt || episodeCategory.Show.StatusId != GlobalConfig.Visible)
                                    return RedirectToAction("Index", "Home");

                                ViewBag.Loved = false; ViewBag.Rated = false; // Social
                                ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };

                                var tempShowNameWithDate = String.Format("{0} {1}", episodeCategory.Show.Description, episode.DateAired.Value.ToString("MMMM d yyyy"));
                                dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
                                if (String.Compare(dbSlug, slug, false) != 0)
                                    return RedirectToActionPermanent("OnDemand", new { id = id, slug = dbSlug });
                                bool isEmailAllowed = false;
                                if (User.Identity.IsAuthenticated)
                                {
                                    var UserId = new Guid(User.Identity.Name);
                                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                    if (user != null)
                                    {
                                        CountryCode = user.CountryCode;
                                        ViewBag.EmailAddress = user.EMail;
                                        ViewBag.UserId = user.UserId;
                                        ViewBag.CountryCode = CountryCode;
                                        using (profiler.Step("Social Love"))
                                        {
                                            ViewBag.Loved = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_LOVE, id, EngagementContentType.Episode);
                                        }
                                        using (profiler.Step("Social Rating"))
                                        {
                                            ViewBag.Rated = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_RATING, id, EngagementContentType.Episode);
                                        }
                                        using (profiler.Step("Check for Active Entitlements")) //check for active entitlements based on categoryId
                                        {
                                            ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt);
                                        }
                                        using (profiler.Step("FPJ Category Check"))
                                        {
                                            if (user.IsTVEverywhere == false)
                                            {
                                                var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                                                var fpjCategory = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.FPJCategoryId);
                                                var showCategoryIds = service.GetAllOnlineShowIds(CountryCode, (Category)fpjCategory);
                                                if (showCategoryIds.Contains(episodeCategory.Show.CategoryId))
                                                    return RedirectToActionPermanent("OnDemand", "FPJ", new { id = id, slug = dbSlug });
                                            }
                                        }

                                        var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                                        isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
                                    }
                                }
                                else
                                {
                                    using (profiler.Step("Check for Active Entitlements (Not Logged In)")) //check for active entitlements based on categoryId
                                    {
                                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
                                    }
                                }

                                if (!ContextHelper.IsCategoryViewableInUserCountry(episodeCategory.Show, CountryCode))
                                    if (!isEmailAllowed)
                                        return RedirectToAction("Index", "Home");

                                ViewBag.Show = episodeCategory.Show;
                                ViewBag.CategoryType = "Show";
                                if (episodeCategory.Show is Movie)
                                    ViewBag.CategoryType = "Movie";
                                else if (episodeCategory.Show is SpecialShow)
                                    ViewBag.CategoryType = "SpecialShow";
                                else if (episodeCategory.Show is WeeklyShow)
                                    ViewBag.CategoryType = "WeeklyShow";
                                else if (episodeCategory.Show is DailyShow)
                                    ViewBag.CategoryType = "DailyShow";

                                ViewBag.EpisodeNumber = 0;
                                if (!(episodeCategory.Show is Movie || episodeCategory.Show is SpecialShow))
                                {
                                    using (profiler.Step("Get Episode Number"))
                                    {
                                        var listOfEpisodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == episodeCategory.CategoryId).Select(e => e.EpisodeId);
                                        var episodeIdsOrderedByDate = context.Episodes.Where(e => listOfEpisodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).Select(e => e.EpisodeId);
                                        if (episodeIdsOrderedByDate != null)
                                        {
                                            var EpIds = episodeIdsOrderedByDate.ToList();
                                            var EpisodeCount = EpIds.Count();
                                            var indexOfCurrentEpisode = EpIds.IndexOf(episode.EpisodeId);
                                            ViewBag.EpisodeNumber = EpisodeCount - indexOfCurrentEpisode;
                                            ViewBag.NextEpisodeId = EpIds.ElementAt(indexOfCurrentEpisode <= 0 ? indexOfCurrentEpisode : indexOfCurrentEpisode - 1); //Next episode id
                                            ViewBag.PreviousEpisodeId = EpIds.ElementAt(indexOfCurrentEpisode >= EpisodeCount - 1 ? indexOfCurrentEpisode : indexOfCurrentEpisode + 1); //Previous episode id
                                            ViewBag.EpisodeCount = EpisodeCount; //Total episode count
                                        }
                                    }
                                }
                                ViewBag.dbSlug = dbSlug;
                                ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
                                return View(episode);
                            }

                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }
    }
}
