using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCtv_Admin_Website.Helpers;

namespace TFCtv_Admin_Website.Controllers
{
    [HandleError(ExceptionType = typeof(TFCtvUnauthorizedAccess), View = "UnauthorizedAccess")]
    public class PreviewController : Controller
    {
        //
        // GET: /Preview/
        //[Authorize(Roles = "Administrator,Marketing,Producer,Publisher,Video Editor")]        
        public ActionResult Index()
        {
            bool isAllowed = false;
            isAllowed = MyUtility.IsIpAddressAllowed(Request);
            if (!isAllowed)
                isAllowed = MyUtility.IsUserAllowed();
            if (isAllowed)
                return View("BetaPlayer");
            throw new TFCtvUnauthorizedAccess();
        }


        //[Authorize(Roles = "Administrator, Video Editor")]
        public ActionResult BetaPlayer()
        {
            bool isAllowed = false;
            isAllowed = MyUtility.IsIpAddressAllowed(Request);
            isAllowed = Global.IsIpWhitelistingEnabled;
            if (isAllowed)
                return View();
            throw new TFCtvUnauthorizedAccess();
        }

        public ActionResult Air()
        {
            return View();
        }

    }
}
