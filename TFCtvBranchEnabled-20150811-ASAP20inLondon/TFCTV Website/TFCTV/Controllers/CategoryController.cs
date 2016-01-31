using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EngagementsModel;
using IPTV2_Model;
using StackExchange.Profiling;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class CategoryController : Controller
    {
        //
        // GET: /Category/

        [RequireHttp]
        public ActionResult List(int? id, string slug)
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
            {
                if (id == null)
                    return RedirectToAction("Shows");
                try
                {
                    using (var context = new IPTV2Entities())
                    {
                        var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
                        if (category != null)
                        {
                            if (category is Category)
                            {
                                try
                                {
                                    var parents = category.CategoryClassParentCategories.Select(c => c.ParentCategory.Description);
                                    var daxParent = parents.Intersect(GlobalConfig.DaxAllowedParentCategories.Split(','), StringComparer.InvariantCultureIgnoreCase);
                                    ViewBag.ParentCategory = string.Join(",", daxParent).ToLower();
                                }
                                catch (Exception) { }

                                ViewBag.id = id;
                                ViewBag.SectionTitle = category.Description;
                                return View("List2", category);
                            }
                        }
                    }
                }
                catch (Exception) { }
            }

            try
            {
                if (id == null)
                    return RedirectToAction("Index", "Home");

                if (id != null)
                    if (id == GlobalConfig.OnlinePremiereCategoryId)
                        return RedirectPermanent("/OnlinePremiere");


                var profiler = MiniProfiler.Current;

                var cache = DataCache.Cache;
                //string cacheKey = "CATLIST:O:" + id.ToString() + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();

                var socialcontext = new EngagementsEntities();
                var context = new IPTV2Entities();
                var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                DateTime registDt = DateTime.Now;
                //Category category = (Category)cache[cacheKey];

                //if (category == null)
                //{
                Category category = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
                if (category != null)
                {
                    var dbSlug = MyUtility.GetSlug(category.Description);
                    if (String.Compare(dbSlug, slug, false) != 0)
                        return RedirectToActionPermanent("List", new { id = id, slug = dbSlug });

                    SortedSet<int> showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode(), category);
                    ViewBag.Category = category.Description;
                    ViewBag.CategoryModel = category;

                    if (showIds.Count() == 0)
                        return RedirectToAction("Index", "Home");

                    int[] setofShows = showIds.ToArray();
                    var list = context.CategoryClasses.Where(c => setofShows.Contains(c.CategoryId) && c.StartDate <= registDt && c.EndDate >= registDt && c.StatusId == GlobalConfig.Visible).OrderBy(c => c.CategoryName).ThenBy(c => c.StartDate).ToList();
                    // cache.Put(cacheKey, list, DataCache.CacheDuration);
                    //using (profiler.Step("Episode Count"))
                    //{
                    //    var social_list = socialcontext.ShowReactions
                    //        .GroupBy(c => new { c.CategoryId, c.ReactionTypeId }, (key, group) => new
                    //        {
                    //            CategoryId = key.CategoryId,
                    //            TotalLoves = key.ReactionTypeId == GlobalConfig.SOCIAL_LOVE ? group.Count() : 0,
                    //            TotalComments = key.ReactionTypeId == GlobalConfig.SOCIAL_COMMENT ? group.Count() : 0
                    //        }).GroupBy(c => c.CategoryId).ToList();
                    //}

                    string cacheKey2 = "CATLISTDISP:O:" + id.ToString() + ";C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();

                    List<CategoryShowListDisplay> catList = (List<CategoryShowListDisplay>)cache[cacheKey2];
                    if (catList == null)
                    {
                        //List<CategoryShowListDisplay> catList = new List<CategoryShowListDisplay>();
                        catList = new List<CategoryShowListDisplay>();
                        using (profiler.Step("Sort By Module"))
                        {
                            foreach (var item in list)
                            {
                                var ratingsCountSummary = socialcontext.ShowReactionSummaries.FirstOrDefault(i => i.CategoryId == item.CategoryId && i.ReactionTypeId == GlobalConfig.SOCIAL_RATING);
                                var likesCountSummary = socialcontext.ShowReactionSummaries.FirstOrDefault(i => i.CategoryId == item.CategoryId && i.ReactionTypeId == GlobalConfig.SOCIAL_LIKE);
                                var lovesCountSummary = socialcontext.ShowReactionSummaries.FirstOrDefault(i => i.CategoryId == item.CategoryId && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                                var commentsCountSummary = socialcontext.ShowReactionSummaries.FirstOrDefault(i => i.CategoryId == item.CategoryId && i.ReactionTypeId == GlobalConfig.SOCIAL_COMMENT);
                                catList.Add(new CategoryShowListDisplay
                                {
                                    CategoryId = item.CategoryId,
                                    Description = item.Description,
                                    ImagePoster = item.ImagePoster,
                                    AiredDate = item.StartDate,
                                    Ratings = (Decimal?)(ratingsCountSummary == null ? 0 : ratingsCountSummary.Total),
                                    TotalLikes = (int?)(likesCountSummary == null ? 0 : likesCountSummary.Total),
                                    TotalLoves = (int?)(lovesCountSummary == null ? 0 : lovesCountSummary.Total),
                                    TotalComments = (int?)(commentsCountSummary == null ? 0 : commentsCountSummary.Total)
                                });
                            }
                        }
                        var cacheDuration = new TimeSpan(0, GlobalConfig.MenuCacheDuration, 0);
                        cache.Put(cacheKey2, catList, cacheDuration);
                    }
                    ViewBag.dbSlug = dbSlug;
                    return View(catList);
                }
            }
            //}

            catch (Exception) { throw; }

            return RedirectToAction("Index", "Home");
        }

        [RequireHttp]
        public ActionResult Shows()
        {
            return View();
        }

        [RequireHttp]
        public ActionResult Movies()
        {
            return View();
        }

        [RequireHttp]
        public ActionResult News()
        {
            return View();
        }

        [RequireHttp]
        public ActionResult Live()
        {
            return View();
        }

        [RequireHttp]
        public ActionResult List2(int? id)
        {
            if (id == null)
                return RedirectToAction("Shows");
            try
            {
                using (var context = new IPTV2Entities())
                {
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
                    if (category != null)
                    {
                        if (category is Category)
                        {
                            ViewBag.id = id;
                            ViewBag.SectionTitle = category.Description;
                            return View();
                        }
                    }
                }
            }
            catch (Exception) { }
            return RedirectToAction("Index", "Home");

        }

        public ActionResult BuildSectionCategory(int id, string sectionTitle, string containerId, string partialViewName, int? pageSize, int page = 0, string featureType = "episode", bool removeShowAll = false)
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
                string cacheKey = "BSCL:O:" + id.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
                if (String.IsNullOrEmpty(jsonString))
                {
                    if (category != null)
                    {
                        if (category is Category)
                        {
                            if (String.IsNullOrEmpty(containerId))
                            {
                                try
                                {
                                    char[] arr = sectionTitle.ToCharArray();
                                    arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c)
                                                                      || char.IsWhiteSpace(c)
                                                                      || c == '-')));
                                    containerId = String.Format("{0}_section", new string(arr));
                                }
                                catch (Exception)
                                {
                                    containerId = String.Format("{0}_section", category.Description.Substring(0, 4));
                                }

                            }

                            var offering = context.Offerings.Find(GlobalConfig.offeringId);
                            var service = offering.Services.FirstOrDefault(o => o.PackageId == GlobalConfig.serviceId);
                            var showIds = service.GetAllOnlineShowIds(CountryCode, (Category)category).ToList();
                            showIds = showIds.Union(MyUtility.getIncludedShowIDsForMenu(category.CategoryId)).ToList();
                            if (showIds.Count() > 0)
                            {
                                var shows = context.CategoryClasses.Where(c => showIds.Contains(c.CategoryId));
                                if (shows != null)
                                {
                                    shows = shows.OrderBy(s => s.Description); // order alphabetically
                                    jfi = new List<HomepageFeatureItem>();
                                    foreach (var cat in shows)
                                    {
                                        if (cat is Show)
                                        {
                                            if (cat.StatusId == GlobalConfig.Visible && cat.StartDate < registDt && cat.EndDate > registDt)
                                            {
                                                Show show = (Show)cat;
                                                if (show.IsOnlineAllowed(CountryCode))
                                                {
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
                                            }
                                        }
                                    }
                                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                                    cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                                }
                            }
                        }
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                ViewBag.FeatureType = featureType;
                if (String.IsNullOrEmpty(sectionTitle))
                    sectionTitle = category.Description;
                ViewBag.SectionTitle = sectionTitle;
                ViewBag.id = id;
                ViewBag.containerId = containerId;
                ViewBag.pageSize = size;
                ViewBag.RemoveShowAll = removeShowAll;
                ViewBag.CategoryClass = category;
                //if (jfi != null)
                //    obj = jfi.Skip(skipSize).Take(size).ToList();
                obj = jfi;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, obj);
            return PartialView("BuildSection", obj);
        }

        public PartialViewResult LoadMoreItems(int id, int page, int? pageSize, string featureType = "episode")
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
                string cacheKey = "BSCL:O:" + id.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    var context = new IPTV2Entities();
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id && c.StatusId == GlobalConfig.Visible);
                    if (category != null)
                    {
                        if (category is Category)
                        {
                            var offering = context.Offerings.Find(GlobalConfig.offeringId);
                            var service = offering.Services.FirstOrDefault(o => o.PackageId == GlobalConfig.serviceId);
                            var showIds = service.GetAllOnlineShowIds(CountryCode, (Category)category);
                            if (showIds.Count() > 0)
                            {
                                var shows = context.CategoryClasses.Where(c => showIds.Contains(c.CategoryId));
                                if (shows != null)
                                {
                                    jfi = new List<HomepageFeatureItem>();
                                    foreach (var cat in shows)
                                    {
                                        if (cat is Show)
                                        {
                                            if (cat.StatusId == GlobalConfig.Visible && cat.StartDate < registDt && cat.EndDate > registDt)
                                            {
                                                Show show = (Show)cat;
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
                                        }
                                    }
                                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                                    cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                                }
                            }
                        }
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                ViewBag.page = page + 1;
                ViewBag.FeatureType = featureType;
                ViewBag.id = id;
                ViewBag.pageSize = size;
                if (jfi != null)
                    obj = jfi.Skip(skipSize).Take(size).ToList();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(obj);
        }
    }
}