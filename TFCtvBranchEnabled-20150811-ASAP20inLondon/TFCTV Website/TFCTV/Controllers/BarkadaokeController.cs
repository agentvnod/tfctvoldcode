using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Models.Youtube;
using TFCTV.Helpers;
using IPTV2_Model;

namespace TFCTV.Controllers
{
    public class BarkadaokeController : Controller
    {
        //
        // GET: /Barkadaoke/
        public ActionResult Index()
        {
            if (MyUtility.isUserLoggedIn())
                ViewBag.CallId = MyUtility.ConvertToTimestamp(DateTime.Now).ToString().Replace('.', '-').PadRight(14, '0') + Guid.NewGuid().ToString().ToLower();
            return View();
        }

        public ActionResult Gallery(int id = 1)
        {
            var youtubeAPIClient = new YoutubeAPIClient();
            int page = id;
            var model = youtubeAPIClient.GetVideos(page.ToString());
            if (model.data.items != null) 
            {
                int totalCount = model != null ? model.data.items.Count() : 0;
                int pageSize = Convert.ToInt32(youtubeAPIClient.YoutubeAPIVideoFeedMaxResults);

                var totalPage = Math.Ceiling((double)totalCount / pageSize);

                ViewBag.Previous = page == 1 ? String.Empty : (page - 1) == 1 ? String.Empty : (page - 1).ToString();
                ViewBag.Next = page == (int)totalPage ? (int)totalPage : page + 1;

                var maxCount = page * pageSize > totalCount ? totalCount : page * pageSize;
                ViewBag.OutOf = String.Format("{0} - {1}", (page * pageSize) + 1 - pageSize, maxCount);
                ViewBag.TotalPages = totalPage;
                ViewBag.TotalCount = totalCount;
            }            
            return View(model);
        }

        public ActionResult Video(string id)
        {
            if (String.IsNullOrEmpty(id))
                return RedirectToAction("Gallery", "Barkadaoke");

            var youtubeAPIClient = new YoutubeAPIClient();

            var model = youtubeAPIClient.GetVideoDetails(id);
            if (model != null)
            {


                var DBMuser = MyUtility.DBMGetUser(id);
                if (DBMuser.result != null)
                {
                    var userId = new Guid(DBMuser.result.uid);
                    var context = new IPTV2Entities();
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                    {
                        ViewBag.FirstName = user.FirstName;
                        ViewBag.LastName = user.LastName;
                    }

                }

                if (MyUtility.isUserLoggedIn())
                {
                    var context = new IPTV2Entities();
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, id, EngagementContentType.Youtube);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                    ViewBag.VideoId = id;
                }
                return View(model);

            }

            return RedirectToAction("Gallery", "Barkadaoke");
        }

        public ActionResult Prizes()
        {
            return View();
        }

        public ActionResult Mechanics()
        {
            return View();
        }

    }
}
