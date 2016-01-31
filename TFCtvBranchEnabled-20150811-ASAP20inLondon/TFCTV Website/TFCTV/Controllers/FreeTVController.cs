using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class FreeTVController : Controller
    {
        //
        // GET: /FreeTV/

        public ActionResult Index(int? id, int? episodeid)
        {
            ViewBag.FeaturedId = id == null ? GlobalConfig.FreeTVPlayListId : id;

            ViewBag.EpisodeId = episodeid;
            return View();
        }

        public PartialViewResult GetPlayList(int? id)
        {
            var context = new IPTV2Entities();
            if (id == null)
            {
                id = GlobalConfig.FreeTVPlayListId;
            }
            var feature = context.Features.FirstOrDefault(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible);
            if (feature == null)
                return null;

            List<FeatureItem> featureItems = context.FeatureItems.Where(f => f.FeatureId == id && f.StatusId == GlobalConfig.Visible).ToList();
            List<JsonFeatureItem> jfi = new List<JsonFeatureItem>();
            foreach (EpisodeFeatureItem f in featureItems)
            {
                if (f is EpisodeFeatureItem)
                {
                    Episode ep = context.Episodes.Find(f.EpisodeId);
                    if (ep != null)
                    {
                        Show show = ep.EpisodeCategories.FirstOrDefault(e => e.CategoryId != GlobalConfig.FreeTvCategoryId).Show;
                        if (show != null)
                        {
                            string img = String.IsNullOrEmpty(ep.ImageAssets.ImageVideo) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.EpisodeImgPath, ep.EpisodeId.ToString(), ep.ImageAssets.ImageVideo);
                            string showImg = String.IsNullOrEmpty(show.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster);
                            JsonFeatureItem j = new JsonFeatureItem() { EpisodeId = ep.EpisodeId, EpisodeDescription = ep.Description, EpisodeName = ep.EpisodeName, EpisodeAirDate = (ep.DateAired != null) ? ep.DateAired.Value.ToString("MMMM d, yyyy") : "", ShowId = show.CategoryId, ShowName = show.CategoryName, EpisodeImageUrl = img, ShowImageUrl = showImg, Blurb = MyUtility.Ellipsis(ep.Synopsis, 80) };
                            jfi.Add(j);
                        }
                    }
                }
            }
            ViewBag.FeaturedId = id;
            return PartialView("_GetPlayList", jfi);
        }

        /// <summary>
        /// Description: Gets the Top Free TV videos
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(VaryByParam = "id", Duration = 180)]
        public JsonResult GetTopVideos(int id)
        {
            var context = new IPTV2Entities();

            return null;
        }
    }
}