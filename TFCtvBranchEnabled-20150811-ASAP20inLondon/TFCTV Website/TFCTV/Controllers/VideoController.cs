using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class VideoController : Controller
    {
        //
        // GET: /Video/

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetClipDetails(int episodeId, int assetId)
        {
            var clipDetails = Helpers.Akamai.GetAkamaiClipDetails(episodeId, assetId, Request, User);
            return new JsonResult() { Data = clipDetails };
        }
    }
}