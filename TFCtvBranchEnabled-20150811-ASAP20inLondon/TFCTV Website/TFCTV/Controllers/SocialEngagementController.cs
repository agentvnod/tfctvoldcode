using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EngagementsModel;
using Gigya.Socialize.SDK;
using IPTV2_Model;
using TFCTV.Helpers;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class SocialEngagementController : Controller
    {
        //
        // GET: /SocialEngagement/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CelebrityReactions(Guid userId, int reactionTypeId, int celebrityId, string action)
        {
            var context = new EngagementsEntities();
            int returnValue = 0;

            var celebrity = context.CelebrityReactions.FirstOrDefault(c => c.CelebrityId == celebrityId && c.ReactionTypeId == reactionTypeId && c.UserId == userId);
            //var celebrity = context.CelebrityReactions.Where(c => c.CelebrityId == celebrityId && c.ReactionTypeId == reactionTypeId && c.UserId == userId).FirstOrDefault();
            if (celebrity == null)
            {
                if (!String.IsNullOrEmpty(action.ToString()) && action.ToString().Equals("add"))
                {
                    CelebrityReaction celebrityReaction = new CelebrityReaction();
                    celebrityReaction.UserId = userId;
                    celebrityReaction.CelebrityId = celebrityId;
                    celebrityReaction.ReactionTypeId = reactionTypeId;
                    celebrityReaction.DateTime = DateTime.Now;
                    context.CelebrityReactions.Add(celebrityReaction);

                    returnValue = context.SaveChanges();
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(action.ToString()) && action.ToString().Equals("remove"))
                {
                    context.CelebrityReactions.Remove(celebrity);
                    returnValue = context.SaveChanges();
                }
            }
            return Json(new { result = returnValue }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ShowReactions(Guid userId, int reactionTypeId, int categoryId, string action)
        {
            var context = new EngagementsEntities();
            int returnValue = 0;

            var show = context.ShowReactions.FirstOrDefault(s => s.CategoryId == categoryId && s.ReactionTypeId == reactionTypeId && s.UserId == userId);
            if (show == null)
            {
                if (!String.IsNullOrEmpty(action.ToString()) && action.ToString().Equals("add"))
                {
                    ShowReaction showReaction = new ShowReaction();
                    showReaction.UserId = userId;
                    showReaction.CategoryId = categoryId;
                    showReaction.ReactionTypeId = reactionTypeId;
                    showReaction.DateTime = DateTime.Now;
                    context.ShowReactions.Add(showReaction);

                    returnValue = context.SaveChanges();
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(action.ToString()) && action.ToString().Equals("remove"))
                {
                    context.ShowReactions.Remove(show);
                    returnValue = context.SaveChanges();
                }
            }
            return Json(new { result = returnValue }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EpisodeReactions(Guid userId, int reactionTypeId, int episodeId, string action)
        {
            var context = new EngagementsEntities();
            int returnValue = 0;

            var episode = context.EpisodeReactions.FirstOrDefault(e => e.EpisodeId == episodeId && e.ReactionTypeId == reactionTypeId && e.UserId == userId);
            if (episode == null)
            {
                if (!String.IsNullOrEmpty(action.ToString()) && action.ToString().Equals("add"))
                {
                    EpisodeReaction episodeReaction = new EpisodeReaction();
                    episodeReaction.UserId = userId;
                    episodeReaction.EpisodeId = episodeId;
                    episodeReaction.ReactionTypeId = reactionTypeId;
                    episodeReaction.DateTime = DateTime.Now;
                    context.EpisodeReactions.Add(episodeReaction);

                    returnValue = context.SaveChanges();
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(action.ToString()) && action.ToString().Equals("remove"))
                {
                    context.EpisodeReactions.Remove(episode);
                    returnValue = context.SaveChanges();
                }
            }
            return Json(new { result = returnValue }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMostLovedMovies()
        {
            var socialContext = new EngagementsEntities();
            var context = new IPTV2Entities();

            var mostLovedShowReactions = socialContext.ShowReactions
                .Where(c => c.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                .GroupBy(c => c.CategoryId)
                .Select(c => new
                {
                    CategoryId = c.Key,
                    TotalLoved = c.Count()
                }).ToList().OrderByDescending(c => c.TotalLoved);

            var mostLovedMovies = mostLovedShowReactions.Join(
                    context.CategoryClasses,
                    sr => sr.CategoryId,
                    cc => cc.CategoryId,
                    (sr, cc) => new { ShowReaction = sr, CategoryClasses = cc })
                    .Where(cc => cc.CategoryClasses.CategoryClassParentCategories.Where(ccp => ccp.ParentCategory.CategoryClassParentCategories.Where(pc => pc.ParentId == GlobalConfig.Movies).Count() > 0).Count() > 0)
                    .Select(show => new
                    {
                        categoryName = show.CategoryClasses.CategoryName,
                        categoryId = show.CategoryClasses.CategoryId,
                        totalLove = show.ShowReaction.TotalLoved
                    }).OrderByDescending(count => count.totalLove).Take(5);

            if (mostLovedMovies != null)
                return Json(mostLovedMovies, JsonRequestBehavior.AllowGet);

            return null;
        }

        public ActionResult GetMostLovedShows()
        {
            var cache = DataCache.Cache;
            string cacheKey = "GMLS:O:;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();

            var socialContext = new EngagementsEntities();
            var context = new IPTV2Entities();

            var mostLovedShowReactions = socialContext.ShowReactions
                .Where(c => c.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                .GroupBy(c => c.CategoryId)
                .Select(c => new
                {
                    CategoryId = c.Key,
                    TotalLoved = c.Count()
                }).ToList().OrderByDescending(c => c.TotalLoved);

            var mostLovedShows = mostLovedShowReactions.Join(
                    context.CategoryClasses,
                    sr => sr.CategoryId,
                    cc => cc.CategoryId,
                    (sr, cc) => new { ShowReaction = sr, CategoryClasses = cc })
                    .Where(cc => cc.CategoryClasses.CategoryClassParentCategories.Where(ccp => ccp.ParentCategory.CategoryClassParentCategories.Where(pc => pc.ParentId != GlobalConfig.Movies).Count() > 0).Count() > 0)
                    .Select(show => new
                    {
                        categoryName = show.CategoryClasses.CategoryName,
                        categoryId = show.CategoryClasses.CategoryId,
                        totalLove = show.ShowReaction.TotalLoved
                    }).Take(5);

            if (mostLovedShows != null)
                return Json(mostLovedShows, JsonRequestBehavior.AllowGet);

            return null;
        }

        public ActionResult GetMostLovedEpisodes()
        {
            var socialContext = new EngagementsEntities();
            var context = new IPTV2Entities();

            var mostLovedEpisodeReactions = socialContext.EpisodeReactions
                .Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                .GroupBy(e => e.EpisodeId)
                .Select(e => new
                {
                    EpisodeId = e.Key,
                    TotalLoved = e.Count()
                }).ToList().OrderByDescending(e => e.TotalLoved).Take(5);

            var mostLovedEpisodes = mostLovedEpisodeReactions.Join(
                    context.Episodes,
                    er => er.EpisodeId,
                    ee => ee.EpisodeId,
                    (er, ee) => new { EpisodeReactions = er, Episodes = ee })
                    .Select(episode => new
                    {
                        showId = episode.Episodes.EpisodeCategories.FirstOrDefault().Show.CategoryId,
                        showName = episode.Episodes.EpisodeCategories.FirstOrDefault().Show.Description,
                        dateAired = episode.Episodes.DateAired.Value.ToString("MMM d, yyyy"),
                        episodeName = episode.Episodes.Description,
                        episodeId = episode.Episodes.EpisodeId,
                        totalLove = episode.EpisodeReactions.TotalLoved
                    });

            if (mostLovedEpisodes != null)
                return Json(mostLovedEpisodes, JsonRequestBehavior.AllowGet);
            return null;
        }

        public ActionResult GetMostLovedCelebrities()
        {
            var socialContext = new EngagementsEntities();
            var context = new IPTV2Entities();

            var mostLovedCelebrityReactions = socialContext.CelebrityReactions
                .Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                .GroupBy(celeb => celeb.CelebrityId)
                .Select(celeb => new
                {
                    celebrityId = celeb.Key,
                    totalLoved = celeb.Count()
                }).ToList().OrderByDescending(celeb => celeb.totalLoved).Take(5);

            var mostLovedCelebrities = mostLovedCelebrityReactions.Join(
                context.Celebrities,
                cr => cr.celebrityId,
                cc => cc.CelebrityId,
                (cr, cc) => new { CelebrityReaction = cr, Celebrity = cc })
                .Select(celebrity => new
                {
                    celebrityName = celebrity.Celebrity.FullName,
                    celebrityId = celebrity.Celebrity.CelebrityId,
                    totalLove = celebrity.CelebrityReaction.totalLoved
                });

            if (mostLovedCelebrities != null)
                return Json(mostLovedCelebrities, JsonRequestBehavior.AllowGet);

            return null;
        }

        public ActionResult GetTopReviewers()
        {
            var socialContext = new EngagementsEntities();
            var context = new IPTV2Entities();

            var topReviewersAll = socialContext.EpisodeReactions
                .Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_RATING)
                .Select(user => new { user.UserId, user.Reactionid })
                .Union(socialContext.ShowReactions.Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_RATING)
                .Select(user => new { user.UserId, user.Reactionid })).GroupBy(user => user.UserId)
                .Select(user => new
                {
                    userId = user.Key,
                    totalReview = user.Count()
                }).ToList().OrderByDescending(user => user.totalReview).Take(5);

            var topReviewers = topReviewersAll.Join(
                context.Users,
                tr => tr.userId,
                user => user.UserId,
                (tr, user) => new { TopReviewer = tr, User = user })
                .Select(u => new
                {
                    userInternalId = u.User.InternalUserId,
                    userId = u.User.UserId,
                    gigyaId = u.User.GigyaUID,
                    userName = u.User.FirstName + " " + u.User.LastName,
                    totalReview = u.TopReviewer.totalReview,
                    userPhoto = GetUserImage(u.User.UserId)
                });

            if (topReviewers != null)
                return Json(topReviewers, JsonRequestBehavior.AllowGet);

            return null;
        }

        public ActionResult GetUserLovedShows(Guid id)
        {
            var socialContext = new EngagementsEntities();
            var context = new IPTV2Entities();

            var userLovedShows = socialContext.ShowReactions
                .Where(show => show.UserId == id && show.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                .Select(show => new
                {
                    categoryId = show.CategoryId
                }).Distinct().ToList();

            var lovedShows = userLovedShows.Join(
                    context.CategoryClasses,
                    ls => ls.categoryId,
                    cc => cc.CategoryId,
                    (sr, cc) => new { ShowReaction = sr, CategoryClasses = cc })
                    .Select(show => new
                    {
                        ShowName = show.CategoryClasses.CategoryName,
                        ShowId = show.CategoryClasses.CategoryId,
                        ShowImageUrl = GlobalConfig.ShowImgPath + show.CategoryClasses.CategoryId + "/" + show.CategoryClasses.ImagePoster,
                    });

            if (lovedShows != null)
                return Json(lovedShows, JsonRequestBehavior.AllowGet);

            return null;
        }

        public ActionResult GetUserLovedCelebrities(Guid id)
        {
            var socialContext = new EngagementsEntities();
            var context = new IPTV2Entities();

            var userLovedCelebrities = socialContext.CelebrityReactions
                .Where(show => show.UserId == id && show.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                .Select(show => new
                {
                    celebrityId = show.CelebrityId
                }).Distinct().ToList();

            var lovedCelebrities = userLovedCelebrities.Join(
                    context.Celebrities,
                    cr => cr.celebrityId,
                    celeb => celeb.CelebrityId,
                    (cr, celeb) => new { CelebrityReaction = cr, Celebrities = celeb })
                    .Select(celebrity => new
                    {
                        CelebrityFullName = celebrity.Celebrities.FullName,
                        ShowId = celebrity.Celebrities.CelebrityId,
                        ShowImageUrl = GlobalConfig.CelebrityImgPath + celebrity.Celebrities.CelebrityId + "/" + celebrity.Celebrities.ImageUrl,
                    });

            if (lovedCelebrities != null)
                return Json(lovedCelebrities, JsonRequestBehavior.AllowGet);

            return null;
        }

        private string GetUserImage(Guid uid)
        {
            Dictionary<string, string> collection = new Dictionary<string, string>();
            collection.Add("uid", uid.ToString());
            var imageUrl = "";
            try
            {
                GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(collection));
                imageUrl = res.GetData().GetString("thumbnailURL");
            }
            catch (Exception)
            {
                imageUrl = String.Format("{0}/{1}", GlobalConfig.AssetsBaseUrl, "content/images/social/profilephoto.png");
            }
            return imageUrl;
            //return res.GetData().ToJsonString();
        }

        public JsonResult GetUserInfoTest(Guid id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("UID", id.ToString());
            collection.Add("fields", "status");
            GSResponse res = GigyaHelpers.createAndSendRequest("gcs.getUserData", GigyaHelpers.buildParameter(collection));

            return Json(res.GetData().ToJsonString(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetUserStatus(Guid id, string userStatus, string userName)
        {
            if (id == new Guid(User.Identity.Name))
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("UID", id.ToString());
                collection.Add("data", new { status = userStatus });
                //GSResponse res = GigyaHelpers.createAndSendRequest("gcs.setUserData", GigyaHelpers.buildParameter(collection));
                GSResponse res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", GigyaHelpers.buildParameter(collection));               
                //Share to Social Network site.
                List<ActionLink> actionlinks = new List<ActionLink>();
                actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.status_actionlink_href, User.Identity.Name)) });
                List<MediaItem> mediaItems = new List<MediaItem>();
                mediaItems.Add(new MediaItem() { type = SNSTemplates.status_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.status_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.status_mediaitem_href, User.Identity.Name)) });
                string gender = GigyaMethods.GetUserInfoByKey(new System.Guid(User.Identity.Name), "gender");
                UserAction action = new UserAction()
                {
                    actorUID = User.Identity.Name,
                    userMessage = String.Format(SNSTemplates.status_usermessage, gender == "f" ? "her" : "his"),
                    title = SNSTemplates.status_title_external,
                    subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.status_subtitle),
                    linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.status_linkback, User.Identity.Name)),
                    description = String.Format(SNSTemplates.status_description_external, "\"" + userStatus + "\""),
                    actionLinks = actionlinks,
                    mediaItems = mediaItems
                };
                var userId = new Guid(User.Identity.Name);
                var userData = MyUtility.GetUserPrivacySetting(userId);

                if (userData.IsExternalSharingEnabled.Contains("true"))
                    GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "external");

                //Modify action to suit Internal feed needs
                action.userMessage = "";
                mediaItems.Clear();
                mediaItems.Add(new MediaItem() { type = SNSTemplates.status_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.status_mediaitem_src), href = String.Format(SNSTemplates.status_mediaitem_href, User.Identity.Name) });
                action.description = String.Format(SNSTemplates.status_description_internal, "\"" + userStatus + "\"");
                action.title = String.Format(SNSTemplates.status_title_internal, userName.Split(' ')[0]);
                action.mediaItems = mediaItems;
                if (userData.IsInternalSharingEnabled.Contains("true"))
                    GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "internal");

                return Content(res.GetData().ToJsonString(), "application/json");
            }

            return Json(new { errorCode = -1 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetUserAboutInfo(Guid id, string userAboutInfo, string userName)
        {
            if (id == new Guid(User.Identity.Name))
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("UID", id.ToString());
                collection.Add("data", new { about = userAboutInfo });
                //GSResponse res = GigyaHelpers.createAndSendRequest("gcs.setUserData", GigyaHelpers.buildParameter(collection));
                GSResponse res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", GigyaHelpers.buildParameter(collection));                

                //Share to Social Network site.
                List<ActionLink> actionlinks = new List<ActionLink>();
                actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text, href = String.Format(SNSTemplates.status_actionlink_href, User.Identity.Name) });
                List<MediaItem> mediaItems = new List<MediaItem>();
                mediaItems.Add(new MediaItem() { type = SNSTemplates.status_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.status_mediaitem_src), href = String.Format(SNSTemplates.status_mediaitem_href, User.Identity.Name) });
                string gender = GigyaMethods.GetUserInfoByKey(new System.Guid(User.Identity.Name), "gender");
                UserAction action = new UserAction()
                {
                    actorUID = User.Identity.Name,
                    userMessage = String.Format(SNSTemplates.about_description_internal, gender == "f" ? "her" : "him"),
                    title = SNSTemplates.status_title_external,
                    subtitle = SNSTemplates.status_subtitle,
                    linkBack = String.Format(SNSTemplates.status_linkback, User.Identity.Name),
                    description = "\"" + userAboutInfo + "\"",
                    actionLinks = actionlinks,
                    mediaItems = mediaItems
                };

                var userId = new Guid(User.Identity.Name);
                var userData = MyUtility.GetUserPrivacySetting(userId);
                if (userData.IsInternalSharingEnabled.Contains("true"))
                    GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "internal");

                return Content(res.GetData().ToJsonString(), "application/json");
            }

            return Json(new { errorCode = -1 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserProfileInfo(Guid id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("UID", id.ToString());
            collection.Add("fields", "status,about");

            GSResponse res = GigyaHelpers.createAndSendRequest("gcs.getUserData", GigyaHelpers.buildParameter(collection));

            return Content(res.GetData().ToJsonString(), "application/json");
        }

        public ActionResult GetFriendsInfo(Guid? id)
        {
            Dictionary<string, string> collection = new Dictionary<string, string>();
            if (id == null)
                return this.Json(collection, JsonRequestBehavior.AllowGet);

            collection.Add("uid", id.ToString());
            collection.Add("siteUsersOnly", "true");
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getFriendsInfo", GigyaHelpers.buildParameter(collection));
            int errorCode = Int32.Parse(res.GetData().GetString("errorCode"));

            if (errorCode != 0)
            {
                res = GigyaHelpers.createAndSendRequest("socialize.getFriendsInfo", GigyaHelpers.buildParameter(collection));
            }

            return Content(res.GetData().ToJsonString(), "application/json");
        }

        public ActionResult GetUserFeeds(Guid id, long timestamp, string group, string limit)
        {
            //group = me|friends|everyone
            Dictionary<string, string> collection = new Dictionary<string, string>();
            collection.Add("uid", id.ToString());
            if (timestamp != 0)
                collection.Add("startTS", timestamp.ToString());
            collection.Add("feedID", "UserAction");
            collection.Add("groups", group);
            collection.Add("limit", limit);
            collection.Add("format", "json");
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getFeed", GigyaHelpers.buildParameter(collection));

            var gsResponse = res.GetData();

            for (int i = 0; i < res.GetData().GetObject(group).GetArray("items").Length; i++)
            {
                if (gsResponse.GetObject(group).GetArray("items").GetObject(i).GetObject("action").ContainsKey("actorUID"))
                {
                    var userId = gsResponse.GetObject(group).GetArray("items").GetObject(i).GetObject("action").GetString("actorUID");
                    var userImage = "";
                    try
                    {
                        userImage = GetUserImage(new System.Guid(userId));
                    }
                    catch (Exception)
                    {
                    }

                    gsResponse.GetObject(group).GetArray("items").GetObject(i).GetObject("sender").Put("photoURL", userImage);
                }
            }

            return Content(gsResponse.ToJsonString(), "application/json");
        }

        public ActionResult GetUserInfo(string uid)
        {
            Dictionary<string, string> collection = new Dictionary<string, string>();
            collection.Add("uid", uid);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(collection));

            return Content(res.GetData().ToJsonString(), "application/json");
        }

        public ActionResult GetMostLovedShows1()
        {
            List<MostLovedShowsDisplay> mostLovedShows = null;
            string jsonString = String.Empty;
            var cache = DataCache.Cache;
            string cacheKey = "SEGMLS1:O;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            try
            {
                if (String.IsNullOrEmpty(jsonString))
                {
                    mostLovedShows = new List<MostLovedShowsDisplay>();
                    var socialContext = new EngagementsEntities();
                    var context = new IPTV2Entities();
                    var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                    Category category = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.Entertainment && c.StatusId == GlobalConfig.Visible);
                    if (category != null)
                    {
                        SortedSet<int> showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode(), category);
                        var showReactions = socialContext.ShowReactionSummaries
                        .Where(c => c.ReactionTypeId == GlobalConfig.SOCIAL_LOVE && showIds.Contains(c.CategoryId))
                        .Select(c => new
                        {
                            categoryId = c.CategoryId,
                            totalLove = c.Total7Days
                        }).ToList().OrderByDescending(c => c.totalLove).Take(5);

                        try
                        {
                            foreach (var item in showReactions)
                            {
                                var categoryClass = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == item.categoryId);
                                if (categoryClass != null)
                                {
                                    mostLovedShows.Add(new MostLovedShowsDisplay()
                                    {
                                        categoryId = item.categoryId,
                                        totalLove = item.totalLove,
                                        categoryName = categoryClass.Description
                                    });
                                }
                            }
                            var cacheDuration = new TimeSpan(4, 0, 0);
                            jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(mostLovedShows);
                            cache.Put(cacheKey, jsonString, cacheDuration);
                        }
                        catch (Exception) { }
                    }
                }
                else
                    mostLovedShows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MostLovedShowsDisplay>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(mostLovedShows, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMostLovedEpisodes1()
        {
            List<MostLovedEpisodesDisplay> mostLovedEpisodes = null;
            string jsonString = String.Empty;
            var cache = DataCache.Cache;
            string cacheKey = "SEGMLE1:O;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            try
            {
                if (String.IsNullOrEmpty(jsonString))
                {
                    mostLovedEpisodes = new List<MostLovedEpisodesDisplay>();
                    var socialContext = new EngagementsEntities();
                    var context = new IPTV2Entities();
                    var mostLovedEpisodeReactions = socialContext.EpisodeReactionSummaries
                        .Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                        .Select(e => new
                        {
                            EpisodeId = e.EpisodeId,
                            TotalLoved = e.Total7Days
                        }).ToList().OrderByDescending(e => e.TotalLoved).Take(20);

                    try
                    {
                        int count = 0;
                        foreach (var item in mostLovedEpisodeReactions)
                        {
                            Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == item.EpisodeId && e.OnlineStatusId == GlobalConfig.Visible);
                            if (episode != null)
                            {
                                var category = episode.EpisodeCategories.FirstOrDefault(ec => ec.CategoryId != GlobalConfig.FreeTvCategoryId);
                                if (category != null)
                                {
                                    mostLovedEpisodes.Add(new MostLovedEpisodesDisplay()
                                    {
                                        showId = category.CategoryId,
                                        showName = String.Compare(episode.EpisodeName, episode.EpisodeCode, true) == 0 ? category.Show.Description : episode.EpisodeName,
                                        dateAired = episode.DateAired.Value.ToString("MMM d, yyyy"),
                                        episodeId = episode.EpisodeId,
                                        episodeName = String.Compare(episode.EpisodeName, episode.EpisodeCode, true) == 0 ? episode.DateAired.Value.ToString("MMM d, yyyy") : episode.EpisodeName,
                                        totalLove = item.TotalLoved
                                    });
                                    count++;
                                }
                            }
                            if (count > 4)
                                break;
                        }
                        var cacheDuration = new TimeSpan(4, 0, 0);
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(mostLovedEpisodes);
                        cache.Put(cacheKey, jsonString, cacheDuration);
                    }
                    catch (Exception) { }
                }
                else
                    mostLovedEpisodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MostLovedEpisodesDisplay>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(mostLovedEpisodes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTopReviewers1()
        {
            List<TopReviewersDisplay> topReviewers = null;
            string jsonString = String.Empty;
            var cache = DataCache.Cache;
            //string cacheKey = "SEGTR1:O;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            string cacheKey = "SEGTR1:O;C:";
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            try
            {
                if (String.IsNullOrEmpty(jsonString))
                {
                    topReviewers = new List<TopReviewersDisplay>();
                    var socialContext = new EngagementsEntities();
                    var context = new IPTV2Entities();
                    var topReviewersAll = socialContext.EpisodeReactions
                        .Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_RATING)
                        .Select(user => new { user.UserId, user.Reactionid })
                        .Union(socialContext.ShowReactions.Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_RATING)
                        .Select(user => new { user.UserId, user.Reactionid })).GroupBy(user => user.UserId)
                        .Select(user => new
                        {
                            userId = user.Key,
                            totalReview = user.Count()
                        }).ToList().OrderByDescending(user => user.totalReview).Take(5);

                    try
                    {
                        foreach (var item in topReviewersAll)
                        {
                            var user = context.Users.FirstOrDefault(u => u.UserId == item.userId);
                            if (user != null)
                            {
                                topReviewers.Add(new TopReviewersDisplay()
                                {
                                    userId = item.userId.ToString(),
                                    userName = String.Format("{0} {1}", user.FirstName, user.LastName),
                                    userPhoto = GetUserImage(item.userId),
                                    totalReview = item.totalReview
                                });
                            }
                        }
                        var cacheDuration = new TimeSpan(4, 0, 0);
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(topReviewers);
                        cache.Put(cacheKey, jsonString, cacheDuration);
                    }
                    catch (Exception) { }
                }
                else
                    topReviewers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TopReviewersDisplay>>(jsonString);
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
            }
            return Json(topReviewers, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMostLovedCelebrities1()
        {
            List<MostLovedCelebritiesDisplay> mostLovedCelebrities = null;
            string jsonString = String.Empty;
            var cache = DataCache.Cache;
            //string cacheKey = "SEGMLC1:O;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            string cacheKey = "SEGMLC1:O;C:";
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            try
            {
                if (String.IsNullOrEmpty(jsonString))
                {
                    mostLovedCelebrities = new List<MostLovedCelebritiesDisplay>();
                    var socialContext = new EngagementsEntities();
                    var context = new IPTV2Entities();
                    var mostLovedCelebrityReactions = socialContext.CelebrityReactionSummaries
                        .Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                        .Select(celeb => new
                        {
                            celebrityId = celeb.CelebrityId,
                            totalLoved = celeb.Total7Days
                        }).ToList().OrderByDescending(celeb => celeb.totalLoved).Take(20);

                    try
                    {
                        int count = 0;
                        foreach (var item in mostLovedCelebrityReactions)
                        {
                            var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == item.celebrityId && c.StatusId == GlobalConfig.Visible);
                            if (celebrity != null)
                            {
                                mostLovedCelebrities.Add(new MostLovedCelebritiesDisplay()
                                {
                                    celebrityId = item.celebrityId,
                                    celebrityName = celebrity.FullName,
                                    totalLove = item.totalLoved
                                });
                                count++;
                            }
                            if (count > 4)
                                break;
                        }
                        var cacheDuration = new TimeSpan(4, 0, 0);
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(mostLovedCelebrities);
                        cache.Put(cacheKey, jsonString, cacheDuration);
                    }
                    catch (Exception) { }
                }
                else
                    mostLovedCelebrities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MostLovedCelebritiesDisplay>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Json(mostLovedCelebrities, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateReaction(Guid userId, int reactionTypeId, string idx, string action, string type)
        {
            int returnValue = 0;
            try
            {
                if (!String.IsNullOrEmpty(type))
                {
                    var context = new EngagementsEntities();
                    DateTime registDt = DateTime.Now;
                    switch (type)
                    {
                        case "show":
                            {
                                var showId = Convert.ToInt32(idx);
                                var show = context.ShowReactions.FirstOrDefault(i => i.CategoryId == showId && i.ReactionTypeId == reactionTypeId && i.UserId == userId);
                                if (show == null)
                                {
                                    if (!String.IsNullOrEmpty(action) && String.Compare(action, "add", true) == 0)
                                    {
                                        var reaction = new ShowReaction()
                                        {
                                            UserId = userId,
                                            CategoryId = showId,
                                            ReactionTypeId = reactionTypeId,
                                            DateTime = registDt
                                        };
                                        context.ShowReactions.Add(reaction);
                                    }
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(action) && String.Compare(action, "remove", true) == 0)
                                        context.ShowReactions.Remove(show);
                                }
                                returnValue = context.SaveChanges();
                                break;
                            }
                        case "episode":
                            {
                                var episodeId = Convert.ToInt32(idx);
                                var episode = context.EpisodeReactions.FirstOrDefault(i => i.EpisodeId == episodeId && i.ReactionTypeId == reactionTypeId && i.UserId == userId);
                                if (episode == null)
                                {
                                    if (!String.IsNullOrEmpty(action) && String.Compare(action, "add", true) == 0)
                                    {
                                        var reaction = new EpisodeReaction()
                                        {
                                            UserId = userId,
                                            EpisodeId = episodeId,
                                            ReactionTypeId = reactionTypeId,
                                            DateTime = registDt
                                        };
                                        context.EpisodeReactions.Add(reaction);
                                    }
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(action) && String.Compare(action, "remove", true) == 0)
                                        context.EpisodeReactions.Remove(episode);
                                }
                                returnValue = context.SaveChanges();
                                break;
                            }
                        case "celebrity":
                            {
                                var celebrityId = Convert.ToInt32(idx);
                                var celebrity = context.CelebrityReactions.FirstOrDefault(i => i.CelebrityId == celebrityId && i.ReactionTypeId == reactionTypeId && i.UserId == userId);
                                if (celebrity == null)
                                {
                                    if (!String.IsNullOrEmpty(action) && String.Compare(action, "add", true) == 0)
                                    {
                                        var reaction = new CelebrityReaction()
                                        {
                                            UserId = userId,
                                            CelebrityId = celebrityId,
                                            ReactionTypeId = reactionTypeId,
                                            DateTime = registDt
                                        };
                                        context.CelebrityReactions.Add(reaction);
                                    }
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(action) && String.Compare(action, "remove", true) == 0)
                                        context.CelebrityReactions.Remove(celebrity);
                                }
                                returnValue = context.SaveChanges();
                                break;
                            }
                        case "channel":
                            {
                                var channelId = Convert.ToInt32(idx);
                                var channel = context.ChannelReactions.FirstOrDefault(i => i.ChannelId == channelId && i.ReactionTypeId == reactionTypeId && i.UserId == userId);
                                if (channel == null)
                                {
                                    if (!String.IsNullOrEmpty(action) && String.Compare(action, "add", true) == 0)
                                    {
                                        var reaction = new ChannelReaction()
                                        {
                                            UserId = userId,
                                            ChannelId = channelId,
                                            ReactionTypeId = reactionTypeId,
                                            DateTime = registDt
                                        };
                                        context.ChannelReactions.Add(reaction);
                                    }
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(action) && String.Compare(action, "remove", true) == 0)
                                        context.ChannelReactions.Remove(channel);
                                }
                                returnValue = context.SaveChanges();
                                break;
                            }
                        case "youtube":
                            {
                                var youtubeId = idx;
                                var youtube = context.YouTubeReactions.FirstOrDefault(i => i.YouTubeId == youtubeId && i.ReactionTypeId == reactionTypeId && i.UserId == userId);
                                if (youtube == null)
                                {
                                    if (!String.IsNullOrEmpty(action) && String.Compare(action, "add", true) == 0)
                                    {
                                        var reaction = new YouTubeReaction()
                                        {
                                            UserId = userId,
                                            YouTubeId = youtubeId,
                                            ReactionTypeId = reactionTypeId,
                                            DateTime = registDt
                                        };
                                        context.YouTubeReactions.Add(reaction);
                                    }
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(action) && String.Compare(action, "remove", true) == 0)
                                        context.YouTubeReactions.Remove(youtube);
                                }
                                returnValue = context.SaveChanges();
                                break;
                            }
                    }
                }
            }
            catch (Exception e) { return Json(new { result = e.InnerException.Message }, JsonRequestBehavior.AllowGet); }
            return Json(new { result = returnValue }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetLovedCount(int? ID, string type)
        {
            int love = 0;
            var socialContext = new EngagementsEntities();
            switch (type)
            {
                case "show":
                    {
                        var lovesCountSummary = socialContext.ShowReactionSummaries.FirstOrDefault(i => i.CategoryId == ID && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                        love = (int)(lovesCountSummary == null ? 0 : lovesCountSummary.Total);
                        break;
                    }
                case "celebrity":
                    {
                        var lovesCountSummary = socialContext.CelebrityReactionSummaries.FirstOrDefault(i => i.CelebrityId == ID && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                        love = (int)(lovesCountSummary == null ? 0 : lovesCountSummary.Total);
                        break;
                    }
                case "episode":
                    {
                        var lovesCountSummary = socialContext.EpisodeReactionSummaries.FirstOrDefault(i => i.EpisodeId == ID && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                        love = (int)(lovesCountSummary == null ? 0 : lovesCountSummary.Total);
                        break;
                    }
                case "channel":
                    {
                        var lovesCountSummary = socialContext.ChannelReactionSummaries.FirstOrDefault(i => i.ChannelId == ID && i.ReactionTypeId == GlobalConfig.SOCIAL_LOVE);
                        love = (int)(lovesCountSummary == null ? 0 : lovesCountSummary.Total);
                        break;
                    }

            }

            return Json(love, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult GetReviewers()
        {
            List<TopReviewersDisplay> topReviewers = null;
            string jsonString = String.Empty;
            var cache = DataCache.Cache;
            string cacheKey = "SEGTR1:O;C:";
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            try
            {
                if (String.IsNullOrEmpty(jsonString))
                {
                    topReviewers = new List<TopReviewersDisplay>();
                    var socialContext = new EngagementsEntities();
                    var context = new IPTV2Entities();
                    var topReviewersAll = socialContext.EpisodeReactions
                        .Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_RATING)
                        .Select(user => new { user.UserId, user.Reactionid })
                        .Union(socialContext.ShowReactions.Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_RATING)
                        .Select(user => new { user.UserId, user.Reactionid })).GroupBy(user => user.UserId)
                        .Select(user => new
                        {
                            userId = user.Key,
                            totalReview = user.Count()
                        }).OrderByDescending(user => user.totalReview).Take(5);

                    try
                    {
                        foreach (var item in topReviewersAll)
                        {
                            var user = context.Users.FirstOrDefault(u => u.UserId == item.userId);
                            if (user != null)
                            {
                                topReviewers.Add(new TopReviewersDisplay()
                                {
                                    userId = item.userId.ToString(),
                                    userName = String.Format("{0} {1}", user.FirstName, user.LastName),
                                    userPhoto = GetUserImage(item.userId),
                                    totalReview = item.totalReview
                                });
                            }
                        }
                        var cacheDuration = new TimeSpan(4, 0, 0);
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(topReviewers);
                        cache.Put(cacheKey, jsonString, cacheDuration);
                    }
                    catch (Exception) { }
                }
                else
                    topReviewers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TopReviewersDisplay>>(jsonString);
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
            }
            return PartialView(topReviewers);
        }

        public PartialViewResult GetLovedShows()
        {
            List<MostLovedShowsDisplay> mostLovedShows = null;
            string jsonString = String.Empty;
            var cache = DataCache.Cache;
            string cacheKey = "SEGMLS1:O;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            try
            {
                if (String.IsNullOrEmpty(jsonString))
                {
                    mostLovedShows = new List<MostLovedShowsDisplay>();
                    var socialContext = new EngagementsEntities();
                    var context = new IPTV2Entities();
                    var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                    Category category = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.Entertainment && c.StatusId == GlobalConfig.Visible);
                    if (category != null)
                    {
                        SortedSet<int> showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode(), category);
                        var showReactions = socialContext.ShowReactionSummaries
                        .Where(c => c.ReactionTypeId == GlobalConfig.SOCIAL_LOVE && showIds.Contains(c.CategoryId))
                        .Select(c => new
                        {
                            categoryId = c.CategoryId,
                            totalLove = c.Total7Days
                        }).ToList().OrderByDescending(c => c.totalLove).Take(5);

                        try
                        {
                            foreach (var item in showReactions)
                            {
                                var categoryClass = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == item.categoryId);
                                if (categoryClass != null)
                                {
                                    mostLovedShows.Add(new MostLovedShowsDisplay()
                                    {
                                        categoryId = item.categoryId,
                                        totalLove = item.totalLove,
                                        categoryName = categoryClass.Description,
                                        slug = MyUtility.GetSlug(categoryClass.Description)
                                    });
                                }
                            }
                            var cacheDuration = new TimeSpan(4, 0, 0);
                            jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(mostLovedShows);
                            cache.Put(cacheKey, jsonString, cacheDuration);
                        }
                        catch (Exception) { }
                    }
                }
                else
                    mostLovedShows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MostLovedShowsDisplay>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(mostLovedShows);
        }

        public PartialViewResult GetLovedCelebrities()
        {
            List<MostLovedCelebritiesDisplay> mostLovedCelebrities = null;
            string jsonString = String.Empty;
            var cache = DataCache.Cache;
            string cacheKey = "SEGMLC1:O;C:";
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            try
            {
                if (String.IsNullOrEmpty(jsonString))
                {
                    mostLovedCelebrities = new List<MostLovedCelebritiesDisplay>();
                    var socialContext = new EngagementsEntities();
                    var context = new IPTV2Entities();
                    var mostLovedCelebrityReactions = socialContext.CelebrityReactionSummaries
                        .Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                        .Select(celeb => new
                        {
                            celebrityId = celeb.CelebrityId,
                            totalLoved = celeb.Total7Days
                        }).ToList().OrderByDescending(celeb => celeb.totalLoved).Take(20);

                    try
                    {
                        int count = 0;
                        foreach (var item in mostLovedCelebrityReactions)
                        {
                            var celebrity = context.Celebrities.FirstOrDefault(c => c.CelebrityId == item.celebrityId && c.StatusId == GlobalConfig.Visible);
                            if (celebrity != null)
                            {
                                mostLovedCelebrities.Add(new MostLovedCelebritiesDisplay()
                                {
                                    celebrityId = item.celebrityId,
                                    celebrityName = celebrity.FullName,
                                    totalLove = item.totalLoved,
                                    slug = MyUtility.GetSlug(celebrity.FullName)
                                });
                                count++;
                            }
                            if (count > 4)
                                break;
                        }
                        var cacheDuration = new TimeSpan(4, 0, 0);
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(mostLovedCelebrities);
                        cache.Put(cacheKey, jsonString, cacheDuration);
                    }
                    catch (Exception) { }
                }
                else
                    mostLovedCelebrities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MostLovedCelebritiesDisplay>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(mostLovedCelebrities);
        }

        public PartialViewResult GetLovedEpisodes()
        {
            List<MostLovedEpisodesDisplay> mostLovedEpisodes = null;
            string jsonString = String.Empty;
            var cache = DataCache.Cache;
            string cacheKey = "SEGMLE1:O;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            try
            {
                if (String.IsNullOrEmpty(jsonString))
                {
                    mostLovedEpisodes = new List<MostLovedEpisodesDisplay>();
                    var socialContext = new EngagementsEntities();
                    var context = new IPTV2Entities();
                    var mostLovedEpisodeReactions = socialContext.EpisodeReactionSummaries
                        .Where(rtId => rtId.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                        .Select(e => new
                        {
                            EpisodeId = e.EpisodeId,
                            TotalLoved = e.Total7Days
                        }).ToList().OrderByDescending(e => e.TotalLoved).Take(20);

                    try
                    {
                        int count = 0;
                        foreach (var item in mostLovedEpisodeReactions)
                        {
                            Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == item.EpisodeId && e.OnlineStatusId == GlobalConfig.Visible);
                            if (episode != null)
                            {
                                var category = episode.EpisodeCategories.FirstOrDefault(ec => ec.CategoryId != GlobalConfig.FreeTvCategoryId);
                                if (category != null)
                                {
                                    mostLovedEpisodes.Add(new MostLovedEpisodesDisplay()
                                    {
                                        showId = category.CategoryId,
                                        showName = String.Compare(episode.EpisodeName, episode.EpisodeCode, true) == 0 ? category.Show.Description : episode.EpisodeName,
                                        dateAired = episode.DateAired.Value.ToString("MMMM d, yyyy"),
                                        episodeId = episode.EpisodeId,
                                        episodeName = String.Compare(episode.EpisodeName, episode.EpisodeCode, true) == 0 ? episode.DateAired.Value.ToString("MMM d, yyyy") : episode.EpisodeName,
                                        totalLove = item.TotalLoved,
                                        slug = MyUtility.GetSlug(episode.IsLiveChannelActive == true ? episode.Description : String.Format("{0} {1}", category.Show.Description, episode.DateAired.Value.ToString("MMMM d yyyy"))),
                                        showSlug = MyUtility.GetSlug(category.Show.Description)
                                    });
                                    count++;
                                }
                            }
                            if (count > 4)
                                break;
                        }
                        var cacheDuration = new TimeSpan(4, 0, 0);
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(mostLovedEpisodes);
                        cache.Put(cacheKey, jsonString, cacheDuration);
                    }
                    catch (Exception) { }
                }
                else
                    mostLovedEpisodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MostLovedEpisodesDisplay>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(mostLovedEpisodes);
        }

        public PartialViewResult GetLovedMovies()
        {
            List<MostLovedShowsDisplay> mostLovedShows = null;
            string jsonString = String.Empty;
            var cache = DataCache.Cache;
            string cacheKey = "SEGMLMS1:O;C:" + MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
            try { jsonString = (string)cache[cacheKey]; }
            catch (Exception) { }
            try
            {
                if (String.IsNullOrEmpty(jsonString))
                {
                    mostLovedShows = new List<MostLovedShowsDisplay>();
                    var socialContext = new EngagementsEntities();
                    var context = new IPTV2Entities();
                    var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                    Category category = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == GlobalConfig.Movies && c.StatusId == GlobalConfig.Visible);
                    if (category != null)
                    {
                        SortedSet<int> showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode(), category);
                        var showReactions = socialContext.ShowReactionSummaries
                        .Where(c => c.ReactionTypeId == GlobalConfig.SOCIAL_LOVE && showIds.Contains(c.CategoryId))
                        .Select(c => new
                        {
                            categoryId = c.CategoryId,
                            totalLove = c.Total7Days
                        }).ToList().OrderByDescending(c => c.totalLove).Take(5);

                        try
                        {
                            foreach (var item in showReactions)
                            {
                                var categoryClass = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == item.categoryId);
                                if (categoryClass != null)
                                {
                                    mostLovedShows.Add(new MostLovedShowsDisplay()
                                    {
                                        categoryId = item.categoryId,
                                        totalLove = item.totalLove,
                                        categoryName = categoryClass.Description
                                    });
                                }
                            }
                            var cacheDuration = new TimeSpan(4, 0, 0);
                            jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(mostLovedShows);
                            cache.Put(cacheKey, jsonString, cacheDuration);
                        }
                        catch (Exception) { }
                    }
                }
                else
                    mostLovedShows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MostLovedShowsDisplay>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(mostLovedShows);
        }
    }

}