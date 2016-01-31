using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Helpers;
using IPTV2_Model;

namespace TFCTV.Controllers
{
    public class KuwentongPaskoController : Controller
    {
        //
        // GET: /KwentoNgPasko/

        public ActionResult Video(int? id)
        {
            var context = new IPTV2Entities();
            EpisodeCategory category;
            if (id == null)
                category = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.KwentoNgPaskoCategoryId).OrderBy(e => Guid.NewGuid()).FirstOrDefault(e => e.Episode.OnlineStatusId == GlobalConfig.Visible);
            else
                category = context.EpisodeCategories1.FirstOrDefault(e => e.CategoryId == GlobalConfig.KwentoNgPaskoCategoryId && e.EpisodeId == id && e.Episode.OnlineStatusId == GlobalConfig.Visible);

            if (category != null)
            {
                if (MyUtility.isUserLoggedIn())
                {
                    ViewBag.Loved = ContextHelper.HasSocialEngagement(new System.Guid(User.Identity.Name), GlobalConfig.SOCIAL_LOVE, category.EpisodeId, EngagementContentType.Episode);
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                        ViewBag.EmailAddress = user.EMail;
                }
                return View(category);
            }
            return RedirectToAction("Index", "Home");
        }

        const int pageSize = 12;
        public ActionResult List(int id = 1)
        {
            int page = id;

            var context = new IPTV2Entities();
            var episodeIds = context.EpisodeCategories1.Where(e => e.CategoryId == GlobalConfig.KwentoNgPaskoCategoryId && e.Episode.OnlineStatusId == GlobalConfig.Visible).OrderByDescending(e => e.Episode.AuditTrail.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => e.EpisodeId);

            var totalCount = context.EpisodeCategories1.Count(e => e.CategoryId == GlobalConfig.KwentoNgPaskoCategoryId && e.Episode.OnlineStatusId == GlobalConfig.Visible);
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
                return RedirectToAction("List", "KuwentongPasko", new { id = String.Empty });
            if (episodes != null)
                return View(episodes);
            return RedirectToAction("Index", "Home");
        }
    }
}