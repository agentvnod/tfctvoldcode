using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Helpers;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class SearchController : Controller
    {
        //
        // GET: /Search/

        // GET: /Samples/DefaultSearch
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public ActionResult Results(string query, string page)
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("Results2");
            return View("GoogleCustomSearchResultsUI");
            //bool googleSiteSearch = GlobalConfig.GoogleSiteSearch;
            //bool bingSiteSearch = GlobalConfig.BingSiteSearch;
            ////string page = Request.QueryString["page"];
            //if (googleSiteSearch)
            //{
            //    var googleCustomSiteSearchClient = new GoogleCustomSiteSearchClient();
            //    int startIndex = 0;
            //    if (!String.IsNullOrEmpty(page))
            //    {
            //        if (Convert.ToInt32(page) > 10)
            //            page = (GlobalConfig.CustomSearchResultCount / GlobalConfig.CustomSearchCount).ToString();
            //        startIndex = (Convert.ToInt32(page) * GlobalConfig.CustomSearchCount) - GlobalConfig.CustomSearchCount + 1;
            //    }

            //    var model = googleCustomSiteSearchClient.RunSearch(query, startIndex > 0 ? startIndex.ToString() : null);
            //    //if (model == null)
            //    //    return RedirectToAction("Results", "Search", new { query = query });
            //    return View("GoogleCustomSiteSearchResult", model);
            //}
            //else if (bingSiteSearch)
            //{
            //    var bingSiteSearchClient = new BingSiteSearchClient();
            //    //var model = bingSiteSearchClient.RunSearch(query, Request.QueryString["page"], String.Empty);
            //    var model = bingSiteSearchClient.RunSearch(query, page, String.Empty);
            //    if (model.Web == null)
            //        return RedirectToAction("Results", "Search", new { query = query });
            //    return View("BingCustomSiteSearchResult", model);
            //}
            //else
            //{
            //    return View("SearchResultNotAvailable");
            //}
        }

        public ActionResult BingCustomSiteSearchPagination(int totalPages, int currentPage, int offset)
        {
            ViewBag.TotalPages = totalPages;
            ViewBag.Offset = offset;
            ViewBag.DisplayCount = GlobalConfig.BingSiteSearchCount;
            return PartialView("_BingCustomSiteSearchPagingPartial");
        }

        //public ActionResult Search(string query, string page)
        //{
        //    var googleCustomSiteSearchClient = new GoogleCustomSiteSearchClient();
        //    int startIndex = 0;
        //    if (!String.IsNullOrEmpty(page))
        //    {
        //        if (Convert.ToInt32(page) > 10)
        //            page = (Convert.ToInt32(ConfigurationManager.AppSettings["CustomSearchResultCount"]) / Convert.ToInt32(ConfigurationManager.AppSettings["CustomSearchCount"])).ToString();
        //        startIndex = (Convert.ToInt32(page) * Convert.ToInt32(ConfigurationManager.AppSettings["CustomSearchCount"])) - Convert.ToInt32(ConfigurationManager.AppSettings["CustomSearchCount"]) + 1;
        //    }

        //    var model = googleCustomSiteSearchClient.RunSearch(query, startIndex > 0 ? startIndex.ToString() : null);
        //    if (model == null)
        //        return RedirectToAction("Search", "Search", new { query = query });
        //    return View("CustomSiteSearch", model);
        //}

        public ActionResult GoogleCustomSiteSearchPagination(int totalPages, int page, int nextPage, int previousPage)
        {
            ViewBag.TotalPages = totalPages;
            ViewBag.NextPage = nextPage;
            ViewBag.PreviousPage = previousPage;
            ViewBag.DisplayCount = Convert.ToInt32(Settings.GetSettings("CustomSearchCount"));
            ViewBag.Offset = (page * Convert.ToInt32(Settings.GetSettings("CustomSearchCount"))) - Convert.ToInt32(Settings.GetSettings("CustomSearchCount")) + 1;
            ViewBag.ResultCount = Convert.ToInt32(Settings.GetSettings("CustomSearchResultCount"));
            return PartialView("_GoogleCustomSiteSearchPagingPartial");
        }
    }
}