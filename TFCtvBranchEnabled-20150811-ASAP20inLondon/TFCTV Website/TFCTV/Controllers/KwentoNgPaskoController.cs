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
    public class KwentoNgPaskoController : Controller
    {
        //
        // GET: /KwentoNgPasko/

        const int pageSize = 12;
        public ActionResult List(int id = 1)
        {
            int page = id;
            int categoryId = GlobalConfig.KwentoNgPaskoCategoryId;
            try
            {
                var context = new IPTV2Entities();
                var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == categoryId && e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderByDescending(e => e.Episode.AuditTrail.CreatedOn)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => e.EpisodeId);

                var totalCount = context.EpisodeCategories1.Count(e => e.CategoryId == categoryId && e.Episode.OnlineStatusId == GlobalConfig.Visible);
                if (totalCount == 0)
                    return RedirectToAction("Index");
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                var totalPage = Math.Ceiling((double)totalCount / pageSize);
                ViewBag.TotalPages = totalPage;
                ViewBag.TotalCount = totalCount;
                var maxCount = page * pageSize > totalCount ? totalCount : page * pageSize;
                ViewBag.OutOf = String.Format("{0} - {1}", (page * pageSize) + 1 - pageSize, maxCount);
                var episodes = context.Episodes.Where(e => episodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible);

                ViewBag.Previous = page == 1 ? String.Empty : (page - 1) == 1 ? String.Empty : (page - 1).ToString();
                ViewBag.Next = page == (int)totalPage ? (int)totalPage : page + 1;

                if ((page * pageSize) + 1 - pageSize > totalCount)
                    return RedirectToAction("List", new { id = String.Empty });
                if (episodes != null)
                    return View(episodes);
            }
            catch (Exception e) { MyUtility.LogException(e); }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Details(int? id, string slug)
        {
            var profiler = MiniProfiler.Current;
            var registDt = DateTime.Now;
            try
            {
                var context = new IPTV2Entities();
                Episode episode;
                if (id != null)
                    episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                else
                {
                    var listOfEpisodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.KwentoNgPaskoCategoryId).Select(c => c.EpisodeId);
                    episode = context.Episodes.Where(e => listOfEpisodeIds.Contains(e.EpisodeId) && e.OnlineStatusId == GlobalConfig.Visible && e.OnlineStartDate < registDt && e.OnlineEndDate > registDt).OrderBy(e => Guid.NewGuid()).FirstOrDefault();
                    id = episode.EpisodeId;
                }


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
                                else if (parentCategories.Contains(GlobalConfig.TFCkatCategoryId))
                                    return RedirectToActionPermanent("OnDemand", "TFCkat", new { id = id, slug = dbSlug });
                                else if (parentCategories.Contains(GlobalConfig.UAAPGreatnessNeverEndsCategoryId))
                                    return RedirectToActionPermanent("OnDemand", "UAAP", new { id = id, slug = dbSlug });
                                else if (parentCategories.Contains(GlobalConfig.DigitalShortsCategoryId) && !parentCategories.Contains(GlobalConfig.KwentoNgPaskoCategoryId))
                                    return Redirect(String.Format("/HaloHalo/{0}", id));
                            }
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

                            var tempShowNameWithDate = String.Format("{0} {1}", episodeCategory.Show.Description, episode.DateAired.Value.ToString("MMMM d, yyyy"));
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
                            else if (episodeCategory.Show is SpecialShow)
                                ViewBag.CategoryType = "SpecialShow";
                            else if (episodeCategory.Show is WeeklyShow)
                                ViewBag.CategoryType = "WeeklyShow";
                            else if (episodeCategory.Show is DailyShow)
                                ViewBag.CategoryType = "DailyShow";
                            return View(episode);
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }

    }
}