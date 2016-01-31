using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TFCTV.Controllers
{
    public class MoviesController : Controller
    {
        //
        // GET: /Movies/

        public ActionResult Index()
        {
            return RedirectToActionPermanent("Movies", "Category");
            //return View();
        }


        /// <summary>
        /// Description: Get list of Genre
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGenre()
        {

            return Content(null);
        }
    }
}
