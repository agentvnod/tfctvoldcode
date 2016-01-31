using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;
using EngagementsModel;
using IPTV2_Model;
using Newtonsoft.Json;
using StackExchange.Profiling;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class EpisodeController : Controller
    {
        //

        // GET: /Episode/

        //public ActionResult Index()
        //{
        //    return View();
        //}

        //

        // GET: /Episode/Details/5

        //public ActionResult DetailsBAK(int? id, string slug)
        //{
        //    if (id != null)
        //    {
        //        var profiler = MiniProfiler.Current;
        //        var context = new IPTV2Entities();

        //        Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
        //        if (episode == null)
        //            return RedirectToAction("Index", "Home");

        //        DateTime registDt = DateTime.Now;
        //        if (episode.OnlineStartDate > registDt)
        //            return RedirectToAction("Index", "Home");
        //        if (episode.OnlineEndDate < registDt)
        //            return RedirectToAction("Index", "Home");

        //        bool isUserEntitled = false;
        //        var dbSlug = MyUtility.GetSlug(episode.Description);
        //        if (episode.IsLiveChannelActive == true)
        //            return RedirectToActionPermanent("Details", "Live", new { id = id, slug = dbSlug });

        //        //var parentCategories = episode.EpisodeCategories.Where(e => e.Episode.OnlineStatusId == GlobalConfig.Visible).Select(e => e.CategoryId);
        //        var parentCategories = episode.GetParentShows();
        //        if (parentCategories.Count() > 0)
        //        {
        //            if (parentCategories.Contains(GlobalConfig.HalalanNewsAlertsParentCategoryId))
        //                return RedirectToActionPermanent("NewsAlerts", "Halalan2013", new { id = id, slug = dbSlug });
        //            else if (parentCategories.Contains(GlobalConfig.HalalanAdvisoriesParentCategoryId))
        //                return RedirectToActionPermanent("Advisories", "Halalan2013", new { id = id, slug = dbSlug });
        //            else if (parentCategories.Contains(GlobalConfig.TFCkatCategoryId))
        //                return RedirectToActionPermanent("OnDemand", "TFCkat", new { id = id, slug = dbSlug });
        //            else if (parentCategories.Contains(GlobalConfig.UAAPGreatnessNeverEndsCategoryId))
        //                return RedirectToActionPermanent("OnDemand", "UAAP", new { id = id, slug = dbSlug });
        //        }

        //        var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
        //        EpisodeCategory category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && !excludedCategoryIds.Contains(e.CategoryId));

        //        if (category != null)
        //        {

        //            ViewBag.Show = category.Show;

        //            if (!ContextHelper.IsCategoryViewableInUserCountry(category.Show))
        //                return RedirectToAction("Index", "Home");

        //            var tempShowNameWithDate = String.Format("{0} {1}", category.Show.Description, episode.DateAired.Value.ToString("MMM dd, yyyy"));
        //            dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
        //            if (String.Compare(dbSlug, slug, false) != 0)
        //                return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });

        //            //if (category.CategoryId == GlobalConfig.TFCkatCategoryId)
        //            //    return Redirect(String.Format("/TFCkat/{0}", id));

        //            if (MyUtility.isUserLoggedIn())
        //            {
        //                System.Guid userId = new System.Guid(User.Identity.Name);
        //                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
        //                if (user != null)
        //                    ViewBag.EmailAddress = user.EMail;
        //            }

        //            //CHECK USER IF CAN PLAY VIDEO
        //            using (profiler.Step("Check if User is Entitled"))
        //            {
        //                try
        //                {
        //                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
        //                    var premiumAsset = episode.PremiumAssets.FirstOrDefault();
        //                    if (premiumAsset != null)
        //                    {
        //                        var assetTemp = premiumAsset.Asset;
        //                        //isUserEntitled = user.IsEpisodeEntitled(offering, episode, asset, RightsType.Online);
        //                        //isUserEntitled = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
        //                        isUserEntitled = ContextHelper.CanPlayVideo(context, offering, episode, assetTemp, User, Request);
        //                    }
        //                }
        //                catch (Exception) { }
        //            }

        //            ViewBag.IsUserEntitled = isUserEntitled;

        //            //context.EpisodeCategories1.Count(s => s.CategoryId == category.CategoryId && s.Episode.OnlineStatusId == GlobalConfig.Visible);
        //            //var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
        //            //var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
        //            //SortedSet<int> showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode());

        //            //if (!showIds.Contains(category.Show.CategoryId))
        //            //    return RedirectToAction("Index", "Home");

        //            ViewBag.CategoryType = "Show";
        //            if (category.Show is Movie)
        //                ViewBag.CategoryType = "Movie";
        //            else if (category.Show is SpecialShow)
        //                ViewBag.CategoryType = "SpecialShow";
        //            else if (category.Show is WeeklyShow)
        //                ViewBag.CategoryType = "WeeklyShow";
        //            else if (category.Show is DailyShow)
        //                ViewBag.CategoryType = "DailyShow";

        //            //IOrderedEnumerable<EpisodeCategory> episodes = null;
        //            //IOrderedQueryable<EpisodeCategory> episodes = null;
        //            IOrderedQueryable<Episode> episodes = null;

        //            var episode_list = context.EpisodeCategories1.Where(s => s.CategoryId == category.CategoryId && s.Episode.OnlineStatusId == GlobalConfig.Visible).Select(e => e.EpisodeId);
        //            using (profiler.Step("Get Episodes"))
        //            {
        //                //episodes = category.Show.Episodes.Where(e => e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderBy(e => e.Episode.DateAired);
        //                episodes = context.Episodes.Where(e => episode_list.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderBy(e => e.DateAired);
        //            }
        //            var eplist = episodes.ToList();
        //            ViewBag.EpisodeNumber = eplist.IndexOf(episode) + 1;

        //            //var GetNext = eplist.FirstOrDefault(e => e.Episode.EpisodeNumber > episode.EpisodeNumber);

        //            var GetNextEpisodeId = eplist.FirstOrDefault(e => e.DateAired > episode.DateAired);
        //            if (GetNextEpisodeId != null)
        //            {
        //                var GetNext = context.EpisodeCategories1.FirstOrDefault(e => e.EpisodeId == GetNextEpisodeId.EpisodeId);
        //                ViewBag.GetNext = GetNext != null ? GetNext : category;
        //            }
        //            else
        //                ViewBag.GetNext = category;

        //            //var GetPrevious = eplist.LastOrDefault(e => e.Episode.EpisodeNumber < episode.EpisodeNumber);

        //            var GetPreviousEpisodeId = eplist.LastOrDefault(e => e.DateAired < episode.DateAired);
        //            if (GetPreviousEpisodeId != null)
        //            {
        //                var GetPrevious = context.EpisodeCategories1.FirstOrDefault(e => e.EpisodeId == GetPreviousEpisodeId.EpisodeId);
        //                ViewBag.GetPrevious = GetPrevious != null ? GetPrevious : category;
        //            }
        //            else
        //                ViewBag.GetPrevious = category;

        //            ViewBag.EpisodeCount = episodes.Count();

        //            ViewBag.ShowId = category.Show.CategoryId;

        //            int episodeId = episode.EpisodeId;

        //            ViewBag.EpisodeId = episodeId;

        //            Asset asset = episode.PremiumAssets.FirstOrDefault().Asset;

        //            int assetId = asset == null ? 0 : asset.AssetId;

        //            ViewBag.AssetId = assetId;

        //            //ViewBag.VideoUrl = Helpers.Akamai.GetVideoUrl(episodeId, assetId, Request, User);
        //            using (profiler.Step("Has Social Love"))
        //            {
        //                if (MyUtility.isUserLoggedIn() && HasSocialEngagementRecordOnEpisode(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, (int)id))

        //                    ViewBag.Loved = true;
        //            }
        //            using (profiler.Step("Has Social Rating"))
        //            {
        //                if (MyUtility.isUserLoggedIn() && HasSocialEngagementRecordOnEpisode(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_RATING, (int)id))

        //                    ViewBag.Rated = true;
        //            }


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

        //            ViewBag.ShowPackageProductPrices = null;
        //            string countryCode = MyUtility.GetCurrentCountryCodeOrDefault();
        //            ViewBag.CountryCode = countryCode;
        //            if (!isUserEntitled)
        //            {
        //                using (profiler.Step("Get Show Package & Product Prices"))
        //                {
        //                    try
        //                    {
        //                        ViewBag.ShowPackageProductPrices = ContextHelper.GetShowPackageProductPrices(category.Show.CategoryId, countryCode);
        //                    }
        //                    catch (Exception e) { MyUtility.LogException(e); }
        //                }

        //            }
        //            return View(episode);
        //        }

        //        else
        //        {
        //            return RedirectToAction("Index", "Home");
        //        }
        //    }

        //    else return RedirectToAction("Index", "Home");
        //}

        private bool HasSocialEngagementRecordOnEpisode(Guid userId, int reactionTypeId, int episodeId)
        {
            var context = new EngagementsEntities();

            var episode = context.EpisodeReactions.FirstOrDefault(s => s.EpisodeId == episodeId && s.ReactionTypeId == reactionTypeId && s.UserId == userId);

            if (episode == null)

                return false;

            return true;
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult _CheckVideo(int? id)
        {
            //if (Request.UrlReferrer == null)
            //    return RedirectToAction("Index", "Home");
            //else
            //{
            //    string uri = Request.UrlReferrer.AbsoluteUri;
            //    if (String.IsNullOrEmpty(uri))
            //        return RedirectToAction("Index", "Home");
            //}

            Dictionary<string, object> collection = new Dictionary<string, object>();

            ErrorCodes errorCode = ErrorCodes.UnknownError;

            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);

            collection = MyUtility.setError(errorCode, errorMessage);

            var context = new IPTV2Entities();

            Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.OnlineStatusId == GlobalConfig.Visible);

            if (ep != null)
            {
                //EpisodeCategory category = ep.EpisodeCategories.FirstOrDefault(e => e.EpisodeId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible);
                //if (category != null)
                //{
                //    var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                //    var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                //    SortedSet<int> showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode());

                //    if (!showIds.Contains(category.Show.CategoryId))
                //    {
                //        collection = MyUtility.setError(ErrorCodes.VideoNotFound, "This video is not available.");
                //        return Content(MyUtility.buildJson(collection), "application/json");
                //    }
                //}

                DateTime registDt = DateTime.Now;
                if (ep.OnlineStartDate > registDt)
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "Video not found.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                if (ep.OnlineEndDate < registDt)
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "Video not found.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }

                var pAsset = ep.PremiumAssets.FirstOrDefault();
                if (pAsset == null)
                {
                    errorCode = ErrorCodes.AkamaiCdnNotFound;
                    collection = MyUtility.setError(errorCode, "Video url not found.");
                    return Content(MyUtility.buildJson(collection), "application/json");
                }
                Asset asset = pAsset.Asset;

                if (asset != null)
                {
                    int assetId = asset == null ? 0 : asset.AssetId;

                    ViewBag.AssetId = assetId;

                    var clipDetails = Helpers.Akamai.GetAkamaiClipDetails(ep.EpisodeId, assetId, Request, User);

                    if (!String.IsNullOrEmpty(clipDetails.Url))
                    {
                        errorCode = ErrorCodes.Success;

                        collection = MyUtility.setError(errorCode, clipDetails.Url);

                        collection.Add("data", clipDetails);
                    }

                    else
                    {
                        errorCode = ErrorCodes.AkamaiCdnNotFound;
                        collection = MyUtility.setError(errorCode, "Video url not found.");
                        if (Akamai.IsIos(Request))
                        {
                            errorCode = ErrorCodes.IPodPreviewNotAvailable;
                            collection = MyUtility.setError(errorCode, "Preview video is not available.");
                        }
                    }
                }

                else
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "Video not found.");
                }
            }

            else
            {
                errorCode = ErrorCodes.EpisodeNotFound;
                collection = MyUtility.setError(errorCode, "Episode not found.");
            }

            return Content(MyUtility.buildJson(collection), "application/json");
        }

        //public const int PageSize = 10;

        //[OutputCache(NoStore = true, Duration = 0)]
        //public BinaryResult _GetClip(int? id)
        //{
        //    //if (String.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))

        //    //    return RedirectToAction("Index", "Home");

        //    Dictionary<string, object> collection = new Dictionary<string, object>();

        //    ErrorCodes errorCode = ErrorCodes.UnknownError;

        //    string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);

        //    collection = MyUtility.setError(errorCode, errorMessage);

        //    var context = new IPTV2Entities();

        //    Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.OnlineStatusId == GlobalConfig.Visible);

        //    if (ep != null)
        //    {
        //        Asset asset = ep.PremiumAssets.FirstOrDefault().Asset;

        //        if (asset != null)
        //        {
        //            int assetId = asset == null ? 0 : asset.AssetId;

        //            ViewBag.AssetId = assetId;

        //            var clipDetails = Helpers.Akamai.GetAkamaiClipDetails(ep.EpisodeId, assetId, Request, User);

        //            if (!String.IsNullOrEmpty(clipDetails.Url))
        //            {
        //                errorCode = ErrorCodes.Success;

        //                collection = MyUtility.setError(errorCode, clipDetails.Url);

        //                collection.Add("data", clipDetails);
        //            }

        //            else
        //            {
        //                errorCode = ErrorCodes.AkamaiCdnNotFound;

        //                collection = MyUtility.setError(errorCode, "Akamai Url not found.");
        //            }
        //        }

        //        else
        //        {
        //            errorCode = ErrorCodes.VideoNotFound;

        //            collection = MyUtility.setError(errorCode, "Video not found.");
        //        }
        //    }

        //    else
        //    {
        //        errorCode = ErrorCodes.EpisodeNotFound;

        //        collection = MyUtility.setError(errorCode, "Episode not found.");
        //    }

        //    byte[] jsonToBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(MyUtility.buildJson(collection));

        //    return new BinaryResult()

        //    {
        //        ContentType = "application/octet-stream",

        //        IsAttachment = false,

        //        Data = jsonToBytes
        //    };
        //}

        //[OutputCache(NoStore = true, Duration = 0)]

        //public string _GetClip2(int? id)

        //{
        //    //if (String.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))

        //    //    return RedirectToAction("Index", "Home");

        //    Dictionary<string, object> collection = new Dictionary<string, object>();

        //    ErrorCodes errorCode = ErrorCodes.UnknownError;

        //    string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);

        //    collection = MyUtility.setError(errorCode, errorMessage);

        //    var context = new IPTV2Entities();

        //    Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.OnlineStatusId == GlobalConfig.Visible);

        //    string url = "";

        //    if (ep != null)

        //    {
        //        Asset asset = ep.PremiumAssets.FirstOrDefault().Asset;

        //        if (asset != null)

        //        {
        //            int assetId = asset == null ? 0 : asset.AssetId;

        //            ViewBag.AssetId = assetId;

        //            var clipDetails = Helpers.Akamai.GetAkamaiClipDetails(ep.EpisodeId, assetId, Request, User);

        //            if (!String.IsNullOrEmpty(clipDetails.Url))

        //            {
        //                errorCode = ErrorCodes.Success;

        //                collection = MyUtility.setError(errorCode, clipDetails.Url);

        //                collection.Add("data", clipDetails);

        //                url = clipDetails.Url;

        //            }

        //            else

        //            {
        //                errorCode = ErrorCodes.AkamaiCdnNotFound;

        //                collection = MyUtility.setError(errorCode, "Akamai Url not found.");

        //            }

        //        }

        //        else

        //        {
        //            errorCode = ErrorCodes.VideoNotFound;

        //            collection = MyUtility.setError(errorCode, "Video not found.");

        //        }

        //    }

        //    else

        //    {
        //        errorCode = ErrorCodes.EpisodeNotFound;

        //        collection = MyUtility.setError(errorCode, "Episode not found.");

        //    }

        //    WebClient client = new WebClient();

        //    Stream data = client.OpenRead(url);

        //    StreamReader reader = new StreamReader(data);

        //    string s = reader.ReadToEnd();

        //    data.Close();

        //    reader.Close();

        //    return s;

        //}

        //public ActionResult List()
        //{
        //    return View();
        //}

        /// <summary>

        /// Description: Get Episodes List

        /// </summary>

        /// <param name="page"></param>

        /// <returns></returns>

        public ActionResult GetEpisodes(int page, int pagesize)
        {
            var episode_data = new PagedData<JsonFeatureItem>();

            List<JsonFeatureItem> episode_list = new List<JsonFeatureItem>();

            var context = new IPTV2Entities();

            //List<ShowLookUpObject> sluo = (List<ShowLookUpObject>)HttpContext.Cache["ShowList"];

            List<Show> showslist = new List<Show>();

            List<int> dummy = new List<int>();

            int[] showIds;

            var service = context.Offerings.Find(GlobalConfig.offeringId).Services

                .Where(p => p.PackageId == GlobalConfig.serviceId

                    && p.StatusId == GlobalConfig.Visible).Single();

            foreach (var category in service.Categories.Select(p => p.Category))
            {
                //get shows

                foreach (int i in service.GetAllOnlineShowIds("--", category).ToArray())
                {
                    dummy.Add(i);
                }
            }

            showIds = dummy.ToArray();

            int records = context.EpisodeCategories1

                                  .Where(

                                    e => showIds.Contains(e.CategoryId)

                                    && e.Episode.OnlineStatusId == GlobalConfig.Visible

                                  )

                                  .Select(n => n.Episode).Count();

            foreach (var item in context.EpisodeCategories1

                                  .Where(

                                    e => showIds.Contains(e.CategoryId)

                                  )

                                  .Select(n => n.Episode).OrderBy(n => n.EpisodeName).Skip(pagesize * (page - 1))

                                  .Take(pagesize).ToList())
            {
                JsonFeatureItem EpisodeItem = new JsonFeatureItem();

                EpisodeItem.EpisodeName = item.EpisodeName;

                EpisodeItem.EpisodeAirDate = (item.DateAired != null) ? item.DateAired.Value.ToString("MMMM d, yyyy") : "";

                EpisodeItem.EpisodeDescription = item.Description;

                EpisodeItem.EpisodeImageUrl = item.ImageAssets.ImageMedium;

                EpisodeItem.ShowId = item.EpisodeCategories.Single().Show.CategoryId;

                EpisodeItem.ShowName = item.EpisodeCategories.Single().Show.CategoryName;

                EpisodeItem.ShowImageUrl = item.EpisodeCategories.Single().Show.ImageTitle;

                EpisodeItem.EpisodeId = item.EpisodeId;

                episode_list.Add(EpisodeItem);
            }

            episode_data.Data = episode_list;

            episode_data.NumberOfPages = Convert.ToInt32(Math.Ceiling((double)records / pagesize));

            episode_data.CurrentPage = page;

            ViewBag.EpisodeList = episode_data;

            return Json(episode_data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEpisodeInfo(int id)
        {
            var context = new IPTV2Entities();

            EpisodeCategory epicat = context.EpisodeCategories1.FirstOrDefault(e => e.EpisodeId == id);

            JsonFeatureItem js = new JsonFeatureItem();

            js.EpisodeId = epicat.Episode.EpisodeId;

            js.EpisodeDescription = epicat.Episode.Description;

            js.ShowId = epicat.Show.CategoryId;

            js.ShowDescription = epicat.Show.Description;

            js.EpisodeAirDate = string.Format("{0: MMMM d,yyyy }", epicat.Episode.DateAired);

            return this.Json(js, JsonRequestBehavior.AllowGet);
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

        private bool CheckUser_ShowEntitled(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                IPTV2Entities context = new IPTV2Entities();

                System.Guid userId = new System.Guid(User.Identity.Name);

                var user = context.Users.FirstOrDefault(u => u.UserId == userId);

                var show = context.CategoryClasses.FirstOrDefault(p => p.CategoryId == id && p.StatusId == GlobalConfig.Visible);

                var off = context.Offerings.Find(GlobalConfig.offeringId);

                if (show is Show)
                {
                    if (user.IsShowEntitled(off, (Show)show, RightsType.Online))
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

        private List<ProductGroup> GetUpgradableProductGroup(int productgroupid)
        {
            var context = new IPTV2Entities();
            List<ProductGroup> upgradable_productgrouplist = new List<ProductGroup>();

            upgradable_productgrouplist = context.ProductGroupUpgrades
                                          .Where(p => p.ProductGroupId == productgroupid)
                                          .Select(p => p.UpgradeToProductGroup)
                                          .ToList();

            return upgradable_productgrouplist;
        }

        //public PartialViewResult GetPackages(Episode model, int CategoryID)
        //{
        //    if (model != null)
        //    {
        //        string countrycode = MyUtility.GetCurrentCountryCodeOrDefault();
        //        string currencycode = MyUtility.GetCurrencyOrDefault(countrycode);
        //        List<ProductPackage> productpackages = ShowEpisodePackages.GetProducts(model, countrycode) ?? new List<ProductPackage>();
        //        ViewBag.CategoryID = CategoryID;
        //        ViewBag.countrycode = countrycode;
        //        ViewBag.currencycode = currencycode;


        //        return PartialView("_GetEpisodeProductPackages", productpackages); 
        //    }

        //   return null;
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
                    if (id == GlobalConfig.PBB747Cam1EpisodeId || id == GlobalConfig.PBB747Cam2EpisodeId)
                        return Redirect(String.Format("/PinoyBigBrother/PBB737"));

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
                                    if (parentCategories.Contains(GlobalConfig.HalalanNewsAlertsParentCategoryId))
                                        return RedirectToActionPermanent("NewsAlerts", "Halalan2013", new { id = id, slug = dbSlug });
                                    else if (parentCategories.Contains(GlobalConfig.HalalanAdvisoriesParentCategoryId))
                                        return RedirectToActionPermanent("Advisories", "Halalan2013", new { id = id, slug = dbSlug });
                                    //else if (parentCategories.Contains(GlobalConfig.TFCkatCategoryId))
                                    //    return RedirectToActionPermanent("OnDemand", "TFCkat", new { id = id, slug = dbSlug });
                                    else if (parentCategories.Contains(GlobalConfig.UAAPGreatnessNeverEndsCategoryId))
                                        return RedirectToActionPermanent("OnDemand", "UAAP", new { id = id, slug = dbSlug });
                                    else if (parentCategories.Contains(GlobalConfig.KwentoNgPaskoCategoryId))
                                        return Redirect(String.Format("/KwentoNgPasko/{0}/{1}", id, dbSlug));
                                    else if (parentCategories.Contains(GlobalConfig.DigitalShortsCategoryId))
                                        return Redirect(String.Format("/HaloHalo/{0}", id));
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
                                        if (MyUtility.IsDuplicateSession(user, Request.Cookies[System.Web.Security.FormsAuthentication.FormsCookieName]))
                                        {
                                            System.Web.Security.FormsAuthentication.SignOut();
                                            return RedirectToAction("ConcurrentLogin", "Home");
                                        }

                                        var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                                        var isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
                                        if (isEmailAllowed)
                                            CountryCode = user.CountryCode;

                                        if (GlobalConfig.UseJWPlayer)
                                        {
                                            var videoPlaybackObj = MyUtility.MakePlaybackApiRequest(episode.EpisodeId);
                                            if (String.Compare(videoPlaybackObj.errorCode, "0", true) == 0)
                                                ViewBag.VideoPlaybackObj = videoPlaybackObj;
                                        }                                        
                                    }
                                }
                                var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                                //var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                                //episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
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

                                using (profiler.Step("Check for preview asset"))
                                {
                                    var cdnIds = MyUtility.StringToIntList("2,6");
                                    var previewAsset = episode.PreviewAssets.LastOrDefault();
                                    if (previewAsset != null)
                                    {
                                        Asset asset = previewAsset.Asset;
                                        if (asset != null)
                                            if (asset.AssetCdns != null)
                                                if (asset.AssetCdns.Count(a => cdnIds.Contains(a.CdnId)) > 0)
                                                    ViewBag.HasPreviewAsset = true;
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
                                            //for dropdown list
                                            ViewBag.FullEpisodeList = EpIds;

                                            var EpisodeCount = EpIds.Count();
                                            var indexOfCurrentEpisode = EpIds.IndexOf(episode.EpisodeId);
                                            ViewBag.EpisodeNumber = EpisodeCount - indexOfCurrentEpisode;
                                            ViewBag.NextEpisodeId = EpIds.ElementAt(indexOfCurrentEpisode <= 0 ? indexOfCurrentEpisode : indexOfCurrentEpisode - 1); //Next episode id
                                            ViewBag.PreviousEpisodeId = EpIds.ElementAt(indexOfCurrentEpisode >= EpisodeCount - 1 ? indexOfCurrentEpisode : indexOfCurrentEpisode + 1); //Previous episode id
                                            ViewBag.LatestEpisodeId = EpIds.First();
                                            ViewBag.EpisodeCount = EpisodeCount; //Total episode count
                                        }
                                    }
                                }
                                ViewBag.dbSlug = dbSlug;
                                ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
                                MyUtility.SetOptimizelyCookie(context);

                                try
                                {
                                    var ShowParentCategories = ContextHelper.GetShowParentCategories(episodeCategory.Show.CategoryId);
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
                                                if (!MyUtility.CheckIfCategoryIsAllowed(episodeCategory.Show.CategoryId, context, ipx))
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

                                try
                                {
                                    var pkg = context.PackageTypes.FirstOrDefault(p => p.PackageId == GlobalConfig.premiumId);
                                    var listOfShows = pkg.GetAllOnlineShowIds(CountryCode);
                                    bool IsPartOfPremium = listOfShows.Contains(episodeCategory.CategoryId);
                                    ViewBag.IsPartOfPremium = IsPartOfPremium;
                                }
                                catch (Exception) { }

                                if (!Request.Cookies.AllKeys.Contains("version"))
                                {
                                    if (GlobalConfig.UseJWPlayer)
                                    {
                                        if (Request.Cookies.AllKeys.Contains("hplayer"))
                                        {
                                            if (Request.Cookies["hplayer"].Value == "2")
                                                return View("DetailsFlowHLS", episode);
                                            else if (Request.Cookies["hplayer"].Value == "3")
                                                return View("DetailsTheOPlayer", episode);
                                            else if (Request.Cookies["hplayer"].Value == "4")
                                            {
                                                return View("Details4", episode);
                                                //return View("DetailsVideoJS", episode);
                                            }
                                            else if (Request.Cookies["hplayer"].Value == "5")
                                                return View("DetailsFlowHLS2", episode);
                                            else if (Request.Cookies["hplayer"].Value == "6")
                                                return View("DetailsJwplayerAkamai", episode);
                                            else if (Request.Cookies["hplayer"].Value == "7")
                                                return View("DetailsJwplayer7Akamai", episode);
                                            else
                                                return View("Details3", episode);
                                        }
                                        else
                                        {
                                            return View("DetailsJwplayerAkamai", episode);
                                            //return View("DetailsVideoJS", episode);
                                            //return View("Details4", episode);
                                        }

                                    }
                                    else
                                        return View("Details3", episode);
                                }
                                return View(episode);
                            }

                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }

        //public ActionResult Details2(int? id, string slug)
        //{
        //    var profiler = MiniProfiler.Current;
        //    var registDt = DateTime.Now;
        //    try
        //    {
        //        if (id != null)
        //        {
        //            var context = new IPTV2Entities();
        //            var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
        //            if (episode != null)
        //            {
        //                if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
        //                {
        //                    var dbSlug = MyUtility.GetSlug(episode.Description);
        //                    if (episode.IsLiveChannelActive == true)
        //                        return RedirectToActionPermanent("Details", "Live", new { id = id, slug = dbSlug });

        //                    SortedSet<int> parentCategories;
        //                    using (profiler.Step("Check for Parent Shows"))
        //                    {
        //                        var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
        //                        parentCategories = episode.GetParentShows(CacheDuration);
        //                        if (parentCategories.Count() > 0)
        //                        {
        //                            if (parentCategories.Contains(GlobalConfig.HalalanNewsAlertsParentCategoryId))
        //                                return RedirectToActionPermanent("NewsAlerts", "Halalan2013", new { id = id, slug = dbSlug });
        //                            else if (parentCategories.Contains(GlobalConfig.HalalanAdvisoriesParentCategoryId))
        //                                return RedirectToActionPermanent("Advisories", "Halalan2013", new { id = id, slug = dbSlug });
        //                            else if (parentCategories.Contains(GlobalConfig.TFCkatCategoryId))
        //                                return RedirectToActionPermanent("OnDemand", "TFCkat", new { id = id, slug = dbSlug });
        //                            else if (parentCategories.Contains(GlobalConfig.UAAPGreatnessNeverEndsCategoryId))
        //                                return RedirectToActionPermanent("OnDemand", "UAAP", new { id = id, slug = dbSlug });
        //                            else if (parentCategories.Contains(GlobalConfig.FPJParentCategoryId))
        //                                return RedirectToActionPermanent("OnDemand", "FPJ", new { id = id, slug = dbSlug });
        //                            else if (parentCategories.Contains(GlobalConfig.KwentoNgPaskoCategoryId))
        //                                return Redirect(String.Format("/KwentoNgPasko/{0}/{1}", id, dbSlug));
        //                            else if (parentCategories.Contains(GlobalConfig.DigitalShortsCategoryId))
        //                                return Redirect(String.Format("/HaloHalo/{0}", id));
        //                        }
        //                    }
        //                    string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
        //                    EpisodeCategory episodeCategory = null;
        //                    if (GlobalConfig.UseServiceOfferingWhenCheckingEpisodeParentCategory)
        //                    {
        //                        if (User.Identity.IsAuthenticated)
        //                        {
        //                            var UserId = new Guid(User.Identity.Name);
        //                            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
        //                            if (user != null)
        //                            {
        //                                var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
        //                                var isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
        //                                if (isEmailAllowed)
        //                                    CountryCode = user.CountryCode;
        //                            }
        //                        }
        //                        var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
        //                        episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
        //                    }
        //                    else
        //                    {
        //                        var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
        //                        episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => !excludedCategoryIds.Contains(e.CategoryId));
        //                    }
        //                    if (episodeCategory != null)
        //                    {
        //                        if (episodeCategory.Show.EndDate < registDt || episodeCategory.Show.StatusId != GlobalConfig.Visible)
        //                            return RedirectToAction("Index", "Home");

        //                        ViewBag.Loved = false; ViewBag.Rated = false; // Social
        //                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };

        //                        var tempShowNameWithDate = String.Format("{0} {1}", episodeCategory.Show.Description, episode.DateAired.Value.ToString("MMMM d yyyy"));
        //                        dbSlug = MyUtility.GetSlug(tempShowNameWithDate);
        //                        if (String.Compare(dbSlug, slug, false) != 0)
        //                            return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });
        //                        bool isEmailAllowed = false;
        //                        if (User.Identity.IsAuthenticated)
        //                        {
        //                            var UserId = new Guid(User.Identity.Name);
        //                            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
        //                            if (user != null)
        //                            {
        //                                CountryCode = user.CountryCode;
        //                                ViewBag.EmailAddress = user.EMail;
        //                                ViewBag.UserId = user.UserId;
        //                                ViewBag.CountryCode = CountryCode;
        //                                using (profiler.Step("Social Love"))
        //                                {
        //                                    ViewBag.Loved = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_LOVE, id, EngagementContentType.Episode);
        //                                }
        //                                using (profiler.Step("Social Rating"))
        //                                {
        //                                    ViewBag.Rated = ContextHelper.HasSocialEngagement(UserId, GlobalConfig.SOCIAL_RATING, id, EngagementContentType.Episode);
        //                                }
        //                                using (profiler.Step("Check for Active Entitlements")) //check for active entitlements based on categoryId
        //                                {
        //                                    ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt);
        //                                }
        //                                using (profiler.Step("FPJ Category Check"))
        //                                {
        //                                    if (user.IsTVEverywhere == false)
        //                                    {
        //                                        var service = context.Offerings.Find(GlobalConfig.offeringId).Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
        //                                        var fpjCategory = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.FPJCategoryId);
        //                                        var showCategoryIds = service.GetAllOnlineShowIds(CountryCode, (Category)fpjCategory);
        //                                        if (showCategoryIds.Contains(episodeCategory.Show.CategoryId))
        //                                            return RedirectToActionPermanent("Details", "FPJ", new { id = id, slug = dbSlug });
        //                                    }
        //                                }

        //                                var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
        //                                isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
        //                            }
        //                        }
        //                        else
        //                        {
        //                            using (profiler.Step("Check for Active Entitlements (Not Logged In)")) //check for active entitlements based on categoryId
        //                            {
        //                                ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
        //                            }
        //                        }

        //                        if (!ContextHelper.IsCategoryViewableInUserCountry(episodeCategory.Show, CountryCode))
        //                            if (!isEmailAllowed)
        //                                return RedirectToAction("Index", "Home");

        //                        ViewBag.Show = episodeCategory.Show;
        //                        ViewBag.CategoryType = "Show";
        //                        if (episodeCategory.Show is Movie)
        //                            ViewBag.CategoryType = "Movie";
        //                        else if (episodeCategory.Show is SpecialShow)
        //                            ViewBag.CategoryType = "SpecialShow";
        //                        else if (episodeCategory.Show is WeeklyShow)
        //                            ViewBag.CategoryType = "WeeklyShow";
        //                        else if (episodeCategory.Show is DailyShow)
        //                            ViewBag.CategoryType = "DailyShow";

        //                        ViewBag.EpisodeNumber = 0;
        //                        if (!(episodeCategory.Show is Movie || episodeCategory.Show is SpecialShow))
        //                        {
        //                            using (profiler.Step("Get Episode Number"))
        //                            {
        //                                var listOfEpisodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == episodeCategory.CategoryId).Select(e => e.EpisodeId);
        //                                var episodeIdsOrderedByDate = context.Episodes.Where(e => listOfEpisodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderByDescending(e => e.DateAired).Select(e => e.EpisodeId);
        //                                if (episodeIdsOrderedByDate != null)
        //                                {
        //                                    var EpIds = episodeIdsOrderedByDate.ToList();
        //                                    var EpisodeCount = EpIds.Count();
        //                                    var indexOfCurrentEpisode = EpIds.IndexOf(episode.EpisodeId);
        //                                    ViewBag.EpisodeNumber = EpisodeCount - indexOfCurrentEpisode;
        //                                    ViewBag.NextEpisodeId = EpIds.ElementAt(indexOfCurrentEpisode <= 0 ? indexOfCurrentEpisode : indexOfCurrentEpisode - 1); //Next episode id
        //                                    ViewBag.PreviousEpisodeId = EpIds.ElementAt(indexOfCurrentEpisode >= EpisodeCount - 1 ? indexOfCurrentEpisode : indexOfCurrentEpisode + 1); //Previous episode id
        //                                    ViewBag.EpisodeCount = EpisodeCount; //Total episode count
        //                                }
        //                            }
        //                        }
        //                        ViewBag.dbSlug = dbSlug;
        //                        ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable(); // Check if page is Ajax Crawlable
        //                        return View(episode);
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    return RedirectToAction("Index", "Home");
        //}

        [RequireHttp]
        public ActionResult List(int? id, string slug)
        {
            try
            {
                if (id != null)
                {
                    ViewBag.featureType = "episode";
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