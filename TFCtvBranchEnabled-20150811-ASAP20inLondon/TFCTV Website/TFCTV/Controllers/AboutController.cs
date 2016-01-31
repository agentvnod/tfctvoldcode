using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace TFCTV.Controllers
{
    public class AboutController : Controller
    {
        //
        // GET: /About/

        public ActionResult Index()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("Index2");
            return View();
        }
    }
}