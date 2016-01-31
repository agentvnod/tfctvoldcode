using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Helpers;
using IPTV2_Model;
using StackExchange.Profiling;

using Newtonsoft.Json;
using System.Diagnostics;
using System.Web.Security;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.IO;

namespace TFCTV.Controllers
{
    public class AirController : Controller
    {
        //
        // GET: /Air/
        [RequireHttp]
        public ActionResult Index()
        {
            var profiler = MiniProfiler.Current;
            try
            {
                try
                {
                    if (Request.QueryString.Count > 0)
                        if (!Request.QueryString.AllKeys.Contains("utm"))
                            return Redirect("/WatchNow");
                }
                catch (Exception) { }

                int ProjectAirEpisodeId = GlobalConfig.ProjectAirEpisodeId;
                DateTime registDt = DateTime.Now;
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                bool isEmailAllowed = false;
                var context = new IPTV2Entities();

                if (context.Promos.Count(p => p.PromoId == GlobalConfig.ProjectAirPromoId && p.StartDate < registDt && p.EndDate > registDt && p.StatusId == GlobalConfig.Visible) <= 0)
                    return RedirectToAction("Index", "Home");
                //get live episode details
                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == ProjectAirEpisodeId);
                if (episode != null)
                {
                    if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                    {
                        if (episode.IsLiveChannelActive == true)
                        {
                            //get parent categories of episode
                            SortedSet<int> parentCategories;
                            using (profiler.Step("Check for Parent Shows"))
                            {
                                var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                                parentCategories = episode.GetParentShows(CacheDuration);
                            }
                            //If user is logged in, get country code of user

                            if (User.Identity.IsAuthenticated)
                            {
                                var UserId = new Guid(User.Identity.Name);
                                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                                if (user != null)
                                {
                                    if (MyUtility.IsWhiteListed(user.EMail))
                                        CountryCode = user.CountryCode;
                                    //check if email is allowed to bypass the countrycode checking
                                    var allowedEmails = GlobalConfig.EmailsAllowedToBypassIPBlock.Split(',');
                                    isEmailAllowed = allowedEmails.Contains(user.EMail.ToLower());
                                    ViewBag.User = user;
                                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                                    //if (user.HasActiveSubscriptions() && !user.IsFirstTimeSubscriber(offering))                                    
                                    if (!user.IsFirstTimeSubscriber(offering))
                                    {
                                        if (user.HasActiveSubscriptions())
                                        {
                                            try
                                            {
                                                if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("air"))
                                                {
                                                    HttpCookie airCookie = new HttpCookie("air");
                                                    airCookie.Expires = DateTime.Now.AddDays(-1);
                                                    Response.Cookies.Add(airCookie);
                                                }
                                            }
                                            catch (Exception) { }
                                            return RedirectToAction("Index", "Home");
                                        }
                                    }
                                }
                                using (profiler.Step("Check for Active Entitlements (Authenticated)"))
                                {
                                    ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt);
                                }
                            }
                            else
                            {
                                //check for active entitlements based on categoryId
                                using (profiler.Step("Check for Active Entitlements (Not Authenticated)"))
                                {
                                    ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
                                }
                            }

                            //get parent show of the episode
                            var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                            var episodeCategory = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));

                            if (episodeCategory != null)
                            {
                                if (episodeCategory.Show.EndDate < registDt || episodeCategory.Show.StatusId != GlobalConfig.Visible)
                                    return RedirectToAction("Index", "Home");

                                //check if show is viewable in country
                                if (!Request.IsLocal)
                                    if (!isEmailAllowed)
                                        if (!ContextHelper.IsCategoryViewableInUserCountry(episodeCategory.Show, CountryCode))
                                            return RedirectToAction("Index", "Home");

                                //get shows to be displayed via page_parent_category
                                var categoryClass = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.ProjectAirDisplayCategoryId);
                                if (categoryClass != null)
                                    if (categoryClass.CategoryClassSubCategories != null)
                                        ViewBag.CategoryClassSubCategories = categoryClass.CategoryClassSubCategories.OrderBy(p => p.Priority).Select(p => p.SubCategory).ToList();

                                //check if page is Ajax Crawlable
                                ViewBag.IsAjaxCrawlable = Request.IsAjaxCrawlingCapable();

                                //get Show
                                ViewBag.Show = episodeCategory.Show;
                                ViewBag.CountryCode = CountryCode;

                                //get list of countries
                                if (!User.Identity.IsAuthenticated)
                                {
                                    var ExcludedCountriesFromRegistrationDropDown = GlobalConfig.ExcludedCountriesFromRegistrationDropDown.Split(',');
                                    List<Country> countries = null;
                                    if (GlobalConfig.UseCountryListInMemory)
                                    {
                                        if (GlobalConfig.CountryList != null)
                                            countries = GlobalConfig.CountryList.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                                        else
                                            countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                                    }
                                    else
                                        countries = context.Countries.Where(c => !ExcludedCountriesFromRegistrationDropDown.Contains(c.Code)).OrderBy(c => c.Description).ToList();
                                    ViewBag.ListOfCountries = countries;
                                    //get list of states
                                    var location = MyUtility.GetLocationViaIpAddressWithoutProxy();
                                    ViewBag.location = location;
                                }

                                try
                                {
                                    DateTime utc = DateTime.Now.ToUniversalTime();
                                    DateTime gmt = utc.AddHours(8); //convert to GMT
                                    var dow = (int)gmt.DayOfWeek;
                                    var channels = GlobalConfig.ProjectAirProgramScheduleChannelIds.Split(','); //Channel Id (Sunday-Saturday)
                                    var psChannelId = Convert.ToInt32(channels[dow]); //ProgramSchedule Channel Id

                                    var sked = context.ProgramSchedules.Where(p => p.ChannelId == psChannelId);
                                    if (sked != null)
                                        if (sked.Count() > 0)
                                        {
                                            ViewBag.ProgramGuide = sked.ToList();
                                            var military_time = gmt.ToString("HH:mm");
                                            var current_show = sked.FirstOrDefault(s => military_time.CompareTo(s.StartTime) >= 0 && military_time.CompareTo(s.EndTime) <= 0);
                                            if (current_show != null)
                                                ViewBag.CurrentlyShowing = current_show;
                                        }

                                }
                                catch (Exception) { }
                                try
                                {
                                    if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("air"))
                                    {
                                        HttpCookie airCookie = new HttpCookie("air");
                                        airCookie.Value = "true";
                                        airCookie.Expires = DateTime.Now.AddYears(5);
                                        Response.Cookies.Add(airCookie);
                                    }
                                }
                                catch (Exception) { }
                                return View(episode);
                            }
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }

        public PartialViewResult BuildSection(int id, string sectionTitle, string containerId, int? pageSize, int page = 0, string featureType = "episode", bool removeShowAll = true, bool isFeature = false, bool ShowAllItems = false, string partialViewName = "")
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
                string cacheKey = "JRAIR:O:" + id.ToString() + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Microsoft.ApplicationServer.Caching.DataCacheException) { DataCache.Refresh(); }
                catch (Exception) { }

                ViewBag.FeatureType = featureType;
                ViewBag.SectionTitle = sectionTitle;
                ViewBag.id = id;
                ViewBag.containerId = containerId;
                ViewBag.pageSize = size;
                ViewBag.RemoveShowAll = removeShowAll;
                ViewBag.IsFeature = isFeature;

                var context = new IPTV2Entities();
                if (String.IsNullOrEmpty(jsonString))
                {
                    //get movie parent category
                    var movie_category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                    if (movie_category != null)
                    {
                        var movieCategoryIds = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, false, null, null, (Category)movie_category);
                        //remove online premiere & adult content
                        try
                        {
                            var onlinepremiere_category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.OnlinePremiereCategoryId);
                            if (onlinepremiere_category != null)
                                movieCategoryIds = movieCategoryIds.Except(ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, false, null, null, (Category)onlinepremiere_category));
                            var adultcontent_category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.AdultContentCategoryId);
                            if (adultcontent_category != null)
                                movieCategoryIds = movieCategoryIds.Except(ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, false, null, null, (Category)adultcontent_category));
                        }
                        catch (Exception) { }

                        //get all movies
                        int[] setofMovies = movieCategoryIds.ToArray();
                        var list = context.CategoryClasses.Where(c => setofMovies.Contains(c.CategoryId) && c.StartDate <= registDt && c.EndDate >= registDt && c.StatusId == GlobalConfig.Visible).OrderBy(c => Guid.NewGuid()).Take((int)pageSize).ToList();
                        jfi = new List<HomepageFeatureItem>();
                        foreach (var item in list)
                        {
                            Show show = (Show)item;
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
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jfi);
                        cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                    }
                }
                else
                    jfi = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HomepageFeatureItem>>(jsonString);
                ViewBag.LinkSlug = String.Empty;
                obj = jfi;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            if (!String.IsNullOrEmpty(partialViewName))
                return PartialView(partialViewName, obj);
            return PartialView(obj);
        }

        public PartialViewResult GetProgramSchedule(List<ProgramSchedule> obj)
        {
            try
            {
                if (obj != null)
                    return PartialView(obj);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(null);
        }

        public PartialViewResult GetCurrentlyShowing(ProgramSchedule obj)
        {
            try
            {
                if (obj != null)
                    return PartialView(obj);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(null);
        }
    }
}