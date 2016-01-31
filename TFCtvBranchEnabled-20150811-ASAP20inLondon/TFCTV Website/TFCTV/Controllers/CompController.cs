using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class CompController : Controller
    {
        //
        // GET: /Comp/

        public ActionResult Try(int id = 1)
        {
            if (!Request.IsLocal)
                if (!GlobalConfig.isUAT)
                    return RedirectToAction("Index", "Home");
            try
            {
                int episodeId = 68379; //MMK
                if (id == 2)
                    episodeId = 55089; //Air
                else if (id == 3)
                    episodeId = 41623; //ANC
                else if (id == 4)
                    episodeId = 56674; //Annaliza
                else if (id == 5)
                    episodeId = 28126; //Every Breath U Take

                var context = new IPTV2Entities();
                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == episodeId);
                var registDt = DateTime.Now;
                if (episode != null)
                {
                    SortedSet<int> parentCategories;
                    var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                    parentCategories = episode.GetParentShows(CacheDuration);
                    string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                    var EMail = "hkt@hkt.com";
                    var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EMail, true) == 0);
                    if (user != null)
                    {
                        ViewBag.User = user;
                        CountryCode = user.CountryCode;
                    }

                    var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                    var category = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                    if (category != null)
                    {
                        ViewBag.Show = category.Show;
                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                        if (user != null)
                        {
                            ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt);
                        }
                        else
                        {
                            ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
                        }


                        var previewAsset = episode.PreviewAssets.LastOrDefault();
                        if (previewAsset != null)
                        {
                            Asset asset = previewAsset.Asset;
                            if (asset != null)
                                if (asset.AssetCdns != null)
                                    if (asset.AssetCdns.Count(a => a.CdnId == 2) > 0)
                                        ViewBag.HasPreviewAsset = true;
                        }
                        return View(episode);
                    }
                }
            }
            catch (Exception)
            {
            }
            return RedirectToAction("Index", "Home");

        }

        public ActionResult AirTry()
        {
            if (!Request.IsLocal)
                if (!GlobalConfig.isUAT)
                    return RedirectToAction("Index", "Home");

            try
            {
                int episodeId = 55089; //Air
                var context = new IPTV2Entities();
                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == episodeId);
                var registDt = DateTime.Now;
                if (episode != null)
                {
                    SortedSet<int> parentCategories;
                    var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                    parentCategories = episode.GetParentShows(CacheDuration);
                    string CountryCode = "HK";
                    //var EMail = "hkt@hkt.com";
                    //var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EMail, true) == 0);
                    //if (user != null)
                    //{
                    //    ViewBag.User = user;
                    //    CountryCode = user.CountryCode;
                    //}

                    var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                    var category = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                    if (category != null)
                    {
                        ViewBag.Show = category.Show;
                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);

                        var previewAsset = episode.PreviewAssets.LastOrDefault();
                        if (previewAsset != null)
                        {
                            Asset asset = previewAsset.Asset;
                            if (asset != null)
                                if (asset.AssetCdns != null)
                                    if (asset.AssetCdns.Count(a => a.CdnId == 2) > 0)
                                        ViewBag.HasPreviewAsset = true;
                        }                        
                        return View(episode);
                    }
                }

            }
            catch (Exception) { }
            return RedirectToAction("Index", "Home");
        }




        public ActionResult VodTry(string slug, int id = 1)
        {
            if (!Request.IsLocal)
                if (!GlobalConfig.isUAT)
                    return RedirectToAction("Index", "Home");

            try
            {
                int episodeId = 62097;
                var context = new IPTV2Entities();
                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == episodeId);
                var registDt = DateTime.Now;
                if (episode != null)
                {
                    SortedSet<int> parentCategories;
                    var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                    parentCategories = episode.GetParentShows(CacheDuration);
                    string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                    if (!String.IsNullOrEmpty(slug))
                        CountryCode = slug;
                    //var EMail = "hkt@hkt.com";
                    //var user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, EMail, true) == 0);
                    //if (user != null)
                    //{
                    //    ViewBag.User = user;
                    //    CountryCode = user.CountryCode;
                    //}

                    var ShowListBasedOnCountryCode = ContextHelper.GetAllShowsBasedOnCountryCode(context, CountryCode, true);
                    var category = episode.EpisodeCategories.FirstOrDefault(e => ShowListBasedOnCountryCode.Contains(e.CategoryId));
                    if (category != null)
                    {
                        ViewBag.Show = category.Show;
                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
                        //if (user != null)
                        //{
                        //    ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(user, parentCategories, registDt);
                        //}
                        //else
                        //{
                        //    ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);
                        //}

                        ViewBag.HasActiveSubscriptionBasedOnCategoryId = ContextHelper.HasActiveSubscriptionBasedOnCategoryId(null, parentCategories, registDt, CountryCode);

                        var previewAsset = episode.PreviewAssets.LastOrDefault();
                        if (previewAsset != null)
                        {
                            Asset asset = previewAsset.Asset;
                            if (asset != null)
                                if (asset.AssetCdns != null)
                                    if (asset.AssetCdns.Count(a => a.CdnId == 2) > 0)
                                        ViewBag.HasPreviewAsset = true;
                        }
                        ViewBag.Id = id;
                        return View(episode);
                    }
                }

            }
            catch (Exception) { }
            return RedirectToAction("Index", "Home");
        }
    }
}
