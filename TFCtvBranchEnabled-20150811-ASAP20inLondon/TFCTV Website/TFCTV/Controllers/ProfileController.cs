using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Gigya.Socialize.SDK;
using IPTV2_Model;
using TFCTV.Helpers;
using EngagementsModel;
using StackExchange.Profiling;

namespace TFCTV.Controllers
{
    public class ProfileController : Controller
    {
        //
        // GET: /Profile/

        [RequireHttp]
        public ActionResult Index(string id, string slug)
        {
            var profiler = MiniProfiler.Current;
            ViewBag.sameUser = false;
            var isLoggedInUser = false;
            if (String.IsNullOrEmpty(id)) // id is empty
                if (MyUtility.isUserLoggedIn()) // if user is logged in, set id to the authenticated user.
                {
                    id = User.Identity.Name;
                    isLoggedInUser = true;
                }
                else // no id, no user is authenticated, redirect to homepage.
                    return RedirectToAction("Index", "Home");

            var context = new IPTV2Entities();
            Guid userId = new System.Guid();
            try
            {
                userId = new System.Guid(id); //Checks if the id parameter is in guid format to avoid exception error
            }
            catch (FormatException)
            {
                if (MyUtility.isUserLoggedIn()) // if user is logged in, set id to the authenticated user.
                    userId = new System.Guid(User.Identity.Name);
                else // no id, no user is authenticated, redirect to homepage.
                    return RedirectToAction("Index", "Home");
            }

            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null) // If context returns a user, return view.
            {
                var fullName = String.Format("{0} {1}", user.FirstName, user.LastName);
                var dbSlug = MyUtility.GetSlug(fullName);
                ViewBag.dbSlug = dbSlug;
                if (!isLoggedInUser)
                    if (String.Compare(dbSlug, slug, false) != 0)
                        return RedirectToActionPermanent("Index", new { id = id, slug = dbSlug });

                string gender = GigyaMethods.GetUserInfoByKey(userId, "gender");
                ViewBag.gender = gender;
                ViewBag.viewedUserUserId = userId;
                ViewBag.userName = MyUtility.GetFullName();
                ViewBag.userCity = user.City;
                ViewBag.userCountry = context.Countries.FirstOrDefault(c => c.Code == user.CountryCode).Description;

                ViewBag.UserData = MyUtility.GetUserPrivacySetting(user.UserId);

                if (userId.ToString() == User.Identity.Name)
                    ViewBag.sameUser = true;
                if (!Request.Cookies.AllKeys.Contains("version"))
                {
                    try
                    {
                        Dictionary<string, string> collection = new Dictionary<string, string>();
                        GSObject obj = null;
                        GSResponse res = null;
                        using (profiler.Step("socialize.getFeed"))
                        {
                            collection.Add("uid", user.UserId.ToString());
                            collection.Add("feedID", "UserAction");
                            obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(collection));
                            res = GigyaHelpers.createAndSendRequest("socialize.getFeed", obj);
                            var resp = Newtonsoft.Json.JsonConvert.DeserializeObject<FeedObj>(res.GetData().ToJsonString());
                            ViewBag.FeedObj = resp;
                        }

                        //clear collection
                        using (profiler.Step("socialize.getUserInfo"))
                        {
                            collection.Clear();
                            collection.Add("uid", user.UserId.ToString());
                            obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(collection));
                            res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", obj);
                            var resp2 = Newtonsoft.Json.JsonConvert.DeserializeObject<GetUserInfoObj>(res.GetData().ToJsonString());
                            ViewBag.UserInfoObj = resp2;
                        }
                    }
                    catch (Exception) { }
                    return View("Index2", user);
                }
                return View(user);
            }

            return RedirectToAction("Index", "Home");
        }

        public PartialViewResult AboutMe(User user)
        {
            try
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("UID", user.UserId.ToString());
                collection.Add("fields", "about");
                GSObject obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(collection));
                GSResponse res = GigyaHelpers.createAndSendRequest("gcs.getUserData", obj);
            }
            catch (Exception) { }
            return PartialView();
        }

        public PartialViewResult GetFriends(User user)
        {
            try
            {
                Dictionary<string, string> collection = new Dictionary<string, string>();
                collection.Add("uid", user.UserId.ToString());
                GSObject obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(collection));
                GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getFriendsInfo", obj);
                var resp = Newtonsoft.Json.JsonConvert.DeserializeObject<GigyaFriendsInfo>(res.GetData().ToJsonString());
                return PartialView(resp);
            }
            catch (Exception) { }
            return PartialView();
        }

        public PartialViewResult GetFeed(User user, FeedObj feedObj, bool useTabTemplate = false, string group = "friends")
        {
            try
            {
                if (feedObj != null)
                {
                    ViewBag.group = group;
                    if (useTabTemplate)
                        return PartialView("GetFeedTab", feedObj);
                    else
                        return PartialView(feedObj);
                }

                Dictionary<string, string> collection = new Dictionary<string, string>();
                collection.Add("uid", user.UserId.ToString());
                collection.Add("feedID", "UserAction");
                GSObject obj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(collection));
                GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getFeed", obj);
                var resp = Newtonsoft.Json.JsonConvert.DeserializeObject<FeedObj>(res.GetData().ToJsonString());
                if (useTabTemplate)
                {
                    ViewBag.group = group;
                    return PartialView("GetFeedTab", resp);
                }
                return PartialView(resp);
            }
            catch (Exception) { }
            if (useTabTemplate)
            {
                ViewBag.group = group;
                return PartialView("GetFeedTab");
            }
            return PartialView();
        }

        public PartialViewResult GetUserLovedShows(User user)
        {
            List<CategoryClass> list = null;
            try
            {
                using (var socialContext = new EngagementsEntities())
                {
                    using (var context = new IPTV2Entities())
                    {
                        var showIds = socialContext.ShowReactions.Where(s => s.UserId == user.UserId && s.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                            .Select(s => s.CategoryId);

                        if (showIds != null)
                        {
                            var ids = showIds.ToList();
                            var shows = context.CategoryClasses.Where(c => ids.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible);
                            if (shows != null)
                                list = shows.ToList();
                        }
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return PartialView(list);
        }

        public PartialViewResult GetUserLovedCelebrities(User user)
        {
            List<Celebrity> list = null;
            try
            {
                using (var socialContext = new EngagementsEntities())
                {
                    using (var context = new IPTV2Entities())
                    {
                        var celebrityIds = socialContext.CelebrityReactions.Where(c => c.UserId == user.UserId && c.ReactionTypeId == GlobalConfig.SOCIAL_LOVE)
                            .Select(c => c.CelebrityId);

                        if (celebrityIds != null)
                        {
                            var ids = celebrityIds.ToList();
                            var celebrities = context.Celebrities.Where(c => ids.Contains(c.CelebrityId) && c.StatusId == GlobalConfig.Visible);
                            if (celebrities != null)
                                list = celebrities.ToList();
                        }
                    }
                }
            }
            catch (Exception) { }
            return PartialView(list);
        }

    }
}