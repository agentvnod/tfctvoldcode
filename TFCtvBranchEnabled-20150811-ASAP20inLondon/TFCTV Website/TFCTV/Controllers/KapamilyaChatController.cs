using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StackExchange.Profiling;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class KapamilyaChatController : Controller
    {
        //
        // GET: /KapamilyaChat/

        public ActionResult Details(int? id, string slug)
        {
            //int id = GlobalConfig.KapamilyaChatLiveEventEpisodeId;
            if (id == null)
                return RedirectToAction("Index", "Home");

            if (GlobalConfig.IsKapamilyaChatRedirectToLiveEnabled)
                return RedirectToAction("Details", "Live", new { id = id, slug = slug });

            var profiler = MiniProfiler.Current;
            var context = new IPTV2Entities();

            Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.OnlineStatusId == GlobalConfig.Visible);
            if (episode == null)
                return RedirectToAction("Index", "Home");

            //Check if episode is a Live Event
            if (episode.IsLiveChannelActive != true)
                return RedirectToAction("Index", "Home");

            DateTime registDt = DateTime.Now;

            if (episode.OnlineStartDate > registDt)
                return RedirectToAction("Index", "Home");
            if (episode.OnlineEndDate < registDt)
                return RedirectToAction("Index", "Home");

            var dbSlug = MyUtility.GetSlug(episode.EpisodeName);
            if (String.Compare(dbSlug, slug, false) != 0)
                return RedirectToActionPermanent("Details", new { id = id, slug = dbSlug });

            bool isUserEntitled = false;

            EpisodeCategory category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible && e.Show is LiveEvent);
            if (category == null)
                category = episode.EpisodeCategories.FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible);

            if (category != null)
            {
                ViewBag.Show = category.Show;
                if (MyUtility.isUserLoggedIn())
                {
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
                        var premiumAsset = episode.PremiumAssets.FirstOrDefault();
                        if (premiumAsset != null)
                        {
                            var assetTemp = premiumAsset.Asset;
                            isUserEntitled = ContextHelper.CanPlayVideo(context, offering, episode, assetTemp, User, Request);
                        }
                    }
                    catch (Exception) { }
                }

                ViewBag.IsUserEntitled = isUserEntitled;
                ViewBag.CategoryType = "Show";
                if (category.Show is Movie)
                    ViewBag.CategoryType = "Movie";
                else if (category.Show is SpecialShow)
                    ViewBag.CategoryType = "SpecialShow";
                else if (category.Show is WeeklyShow)
                    ViewBag.CategoryType = "WeeklyShow";
                else if (category.Show is DailyShow)
                    ViewBag.CategoryType = "DailyShow";
                else if (category.Show is LiveEvent)
                    ViewBag.CategoryType = "LiveEvent";

                ViewBag.ShowId = category.Show.CategoryId;
                ViewBag.EpisodeId = episode.EpisodeId;

                Asset asset = episode.PremiumAssets.FirstOrDefault().Asset;
                int assetId = asset == null ? 0 : asset.AssetId;

                ViewBag.AssetId = assetId;
                //ViewBag.VideoUrl = Helpers.Akamai.GetVideoUrl(episode.EpisodeId, assetId, Request, User);

                using (profiler.Step("Has Social Love"))
                {
                    if (MyUtility.isUserLoggedIn())
                        ViewBag.Loved = ContextHelper.HasSocialEngagement(new Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, episode.EpisodeId, EngagementContentType.Episode);
                }
                using (profiler.Step("Has Social Rating"))
                {
                    if (MyUtility.isUserLoggedIn())
                        ViewBag.Rated = ContextHelper.HasSocialEngagement(new Guid(User.Identity.Name), GlobalConfig.SOCIAL_RATING, episode.EpisodeId, EngagementContentType.Episode);
                }

                /**** Check for Free Trial ****/
                bool showFreeTrialImage = false;
                using (profiler.Step("Check for Early Bird"))
                {
                    if (GlobalConfig.IsEarlyBirdEnabled)
                    {
                        if (MyUtility.isUserLoggedIn())
                        {

                            var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(User.Identity.Name));
                            if (user != null)
                            {
                                var offering = context.Offerings.Find(GlobalConfig.offeringId);
                                if (user.IsFirstTimeSubscriber(offering, true, MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds), context))
                                    showFreeTrialImage = true;
                            }
                            //showFreeTrialImage = true;
                        }
                    }
                }
                ViewBag.ShowFreeTrialImage = showFreeTrialImage;
                return View(episode);
            }
            else
                return RedirectToAction("Index", "Home");
        }

    }
}
