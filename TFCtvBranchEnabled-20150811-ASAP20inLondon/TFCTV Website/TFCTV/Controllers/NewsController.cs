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
    public class NewsController : Controller
    {
        //
        // GET: /News/

        public ActionResult Index()
        {
            return RedirectToActionPermanent("News", "Category");
            return View();
        }

        [HttpGet]
        [OutputCache(VaryByParam = "*", Duration = 180)]
        public JsonResult GetFeaturedShow(int id)
        {
            var context = new IPTV2Entities();
            var featureItems = context.Features.Find(id).FeatureItems.ToList();
            List<JsonFeatureItem> jfi = new List<JsonFeatureItem>();
            foreach (ShowFeatureItem f in featureItems)
            {
                if (f is ShowFeatureItem)
                {
                    Show show = f.Show;
                    JsonFeatureItem j = new JsonFeatureItem() { ShowId = show.CategoryId, ShowName = show.Description, ShowDescription = show.Description, ShowImageUrl = String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, show.CategoryId.ToString(), show.ImagePoster) };
                    jfi.Add(j);
                }
            }
            return this.Json(jfi, JsonRequestBehavior.AllowGet);
        }
    }
}