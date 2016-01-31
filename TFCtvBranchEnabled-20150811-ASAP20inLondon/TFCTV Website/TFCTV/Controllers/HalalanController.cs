using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;
using StackExchange.Profiling;

namespace TFCTV.Controllers
{
    public class HalalanController : Controller
    {
        //
        // GET: /Halalan/

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
            try
            {

                var cache = DataCache.Cache;
                string cacheKey = "HAL2013DISP:C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();

                List<HalalanShowAndChannelContainer> ordered = (List<HalalanShowAndChannelContainer>)cache[cacheKey];

                if (ordered == null)
                {
                    ordered = new List<HalalanShowAndChannelContainer>();
                    DateTime registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);

                    List<HalalanShowAndChannelContainer> list = new List<HalalanShowAndChannelContainer>();

                    //get list of shows under the halalan parent category id
                    SortedSet<int> showIds = null;
                    var HalalanParentCategoryId = GlobalConfig.HalalanParentCategoryId;
                    var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == HalalanParentCategoryId && c.StatusId == GlobalConfig.Visible);
                    if (category != null)
                        if (category is Category)
                            showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode(), (Category)category);
                    int[] setofShows = showIds.ToArray();
                    var shows = context.CategoryClasses.Where(c => setofShows.Contains(c.CategoryId) && c.StartDate <= registDt && c.EndDate >= registDt && c.StatusId == GlobalConfig.Visible).OrderBy(c => c.CategoryName).ThenBy(c => c.StartDate).ToList();
                    if (shows != null)
                    {
                        foreach (var item in shows)
                        {
                            var src =
                                String.IsNullOrEmpty(item.ImagePoster) ? String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, GlobalConfig.BlankGif) :
                                String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, item.CategoryId, item.ImagePoster);
                            list.Add(new HalalanShowAndChannelContainer()
                            {
                                Description = item.Blurb,
                                DisplayName = item.Description,
                                Id = item.CategoryId,
                                type = "Show",
                                Url = String.Format("/Show/Details/{0}/{1}", item.CategoryId, MyUtility.GetSlug(item.Description)),
                                src = src
                            });
                        }
                    }

                    //get list of channels
                    var HalalanChannelIds = MyUtility.StringToIntList(GlobalConfig.HalalanChannelIds);
                    var channels = context.Channels.Where(c => HalalanChannelIds.Contains(c.ChannelId) && c.OnlineStatusId == GlobalConfig.Visible && c.OnlineStartDate <= registDt && c.OnlineEndDate >= registDt);
                    if (channels != null)
                    {
                        foreach (var item in channels)
                        {
                            list.Add(new HalalanShowAndChannelContainer()
                            {
                                Description = item.Blurb,
                                DisplayName = item.Description,
                                Id = item.ChannelId,
                                type = "Channel",
                                Url = String.Format("/Channel/Details/{0}", item.ChannelId),
                                src = String.Format("{0}/content/images/channels/{1}.jpg", GlobalConfig.AssetsBaseUrl, item.ChannelId)
                            });
                        }
                    }

                    //Order the list
                    //var HalalangOrderedListIds = MyUtility.StringToIntList(GlobalConfig.HalalanOrderedListIds);
                    //Dictionary<int, HalalanShowAndChannelContainer> d = list.ToDictionary(x => x.Id);
                    //ordered = new List<HalalanShowAndChannelContainer>();
                    //foreach (var i in HalalangOrderedListIds)
                    //{
                    //    if (d.ContainsKey(i))
                    //        ordered.Add(d[i]);
                    //}
                    ordered = list;
                    cache.Put(cacheKey, ordered, DataCache.CacheDuration);
                }

                if (ordered.Count() > 0)
                    return View(ordered);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception) { return RedirectToAction("Index", "Home"); }
        }

        public ActionResult FAQ()
        {
            return RedirectToAction("Index", "Home");
            return View();
        }

        public ActionResult NewsAlerts(int? id, string slug)
        {
            return RedirectToAction("Index", "Home");
            var profiler = MiniProfiler.Current;
            var context = new IPTV2Entities();

            if (id == null)
                return RedirectToAction("Index", "Halalan2013");

            EpisodeCategory category = context.EpisodeCategories1.FirstOrDefault(e => e.CategoryId == GlobalConfig.HalalanNewsAlertsParentCategoryId && e.EpisodeId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible);
            if (category != null)
            {
                DateTime registDt = DateTime.Now;
                if (category.Episode.OnlineStartDate > registDt)
                    return RedirectToAction("Index", "Home");
                if (category.Episode.OnlineEndDate < registDt)
                    return RedirectToAction("Index", "Home");

                var dbSlug = MyUtility.GetSlug(category.Episode.EpisodeName);
                if (String.Compare(dbSlug, slug, false) != 0)
                    return RedirectToActionPermanent("NewsAlerts", "Halalan2013", new { id = id, slug = dbSlug });

                ViewBag.Loved = false;
                bool isUserEntitled = false;
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, category.EpisodeId, EngagementContentType.Episode);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }

                //CHECK USER IF CAN PLAY VIDEO
                using (profiler.Step("Check if User is Entitled"))
                {
                    try
                    {
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var premiumAsset = category.Episode.PremiumAssets.FirstOrDefault();
                        if (premiumAsset != null)
                        {
                            var asset = premiumAsset.Asset;
                            isUserEntitled = ContextHelper.CanPlayVideo(context, offering, category.Episode, asset, User, Request);
                        }
                    }
                    catch (Exception) { }
                }

                ViewBag.IsUserEntitled = isUserEntitled;

                return View(category);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Advisories(int? id, string slug)
        {
            return RedirectToAction("Index", "Home");
            var profiler = MiniProfiler.Current;
            var context = new IPTV2Entities();

            if (id == null)
                return RedirectToAction("Index", "Halalan2013");

            EpisodeCategory category = context.EpisodeCategories1.FirstOrDefault(e => e.CategoryId == GlobalConfig.HalalanAdvisoriesParentCategoryId && e.EpisodeId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible);

            if (category != null)
            {
                DateTime registDt = DateTime.Now;
                if (category.Episode.OnlineStartDate > registDt)
                    return RedirectToAction("Index", "Home");
                if (category.Episode.OnlineEndDate < registDt)
                    return RedirectToAction("Index", "Home");

                var dbSlug = MyUtility.GetSlug(category.Episode.EpisodeName);
                if (String.Compare(dbSlug, slug, false) != 0)
                    return RedirectToActionPermanent("Advisories", "Halalan2013", new { id = id, slug = dbSlug });

                ViewBag.Loved = false;
                bool isUserEntitled = false;
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, category.EpisodeId, EngagementContentType.Episode);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }

                //CHECK USER IF CAN PLAY VIDEO
                using (profiler.Step("Check if User is Entitled"))
                {
                    try
                    {
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var premiumAsset = category.Episode.PremiumAssets.FirstOrDefault();
                        if (premiumAsset != null)
                        {
                            var asset = premiumAsset.Asset;
                            isUserEntitled = ContextHelper.CanPlayVideo(context, offering, category.Episode, asset, User, Request);
                        }
                    }
                    catch (Exception) { }
                }

                ViewBag.IsUserEntitled = isUserEntitled;

                return View(category);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
