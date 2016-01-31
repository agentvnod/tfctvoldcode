using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class FooterController : Controller
    {
        [OutputCache(VaryByParam = "menuId", Duration = 180)]
        public ActionResult CreateFooterNav(int? menuId)
        {
            var context = new IPTV2Entities();
            var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
            var service = offering.Services.FirstOrDefault(svc => svc.PackageId == GlobalConfig.serviceId);

            var main_category = service.Categories.FirstOrDefault(cat => cat.CategoryId == menuId);
            if (main_category != null)
            {
                ViewBag.footerNavTitle = main_category.Category.Description;

                return PartialView("_FooterNav", main_category.Category.SubCategories.Where(sc => sc.StatusId == GlobalConfig.Visible).Take(10).OrderBy(cat => cat.CategoryName));
            }
            return PartialView("_FooterNav", null);
        }


        public ActionResult CreateFooterNavBasedOnMenuGroup(string id)
        {
            List<MyMenu> fullMenu = null;
            string jsonString = String.Empty;
            try
            {
                var cache = DataCache.Cache;
                string cacheKey = "JRBMG:O:" + id + ";C:";// +MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode();
                ViewBag.footerNavTitle = id;
                jsonString = (string)cache[cacheKey];
                if (String.IsNullOrEmpty(jsonString))
                {
                    fullMenu = new List<MyMenu>();
                    if (String.IsNullOrEmpty(id))
                        id = "Entertainment";
                    id = id + "MenuIds";
                    string menuId = Settings.GetSettings(id);
                    var menuIdList = MyUtility.StringToIntList(menuId);
                    var context = new IPTV2Entities();
                    var features = context.Features.Where(f => menuIdList.Contains(f.FeatureId) && f.StatusId == GlobalConfig.Visible).ToList();
                    Dictionary<int, Feature> d = features.ToDictionary(x => x.FeatureId);
                    List<Feature> ordered = new List<Feature>();
                    foreach (var i in menuIdList)
                    {
                        if (d.ContainsKey(i))
                            ordered.Add(d[i]);
                    }
                    foreach (var feature in ordered)
                    {
                        var temp = feature.Description.Split('|');
                        var featureItems = feature.FeatureItems.Where(f => f.StatusId == GlobalConfig.Visible).Take(5).ToList();
                        List<MyMenuShows> mms = new List<MyMenuShows>();
                        foreach (var f in featureItems)
                        {
                            if (f is ShowFeatureItem)
                            {
                                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == ((ShowFeatureItem)f).CategoryId);
                                if (category != null)
                                {
                                    if (category is Show)
                                    {
                                        Show show = (Show)category;
                                        MyMenuShows m = new MyMenuShows()
                                        {
                                            name = show.Description,
                                            id = show.CategoryId
                                        };
                                        mms.Add(m);
                                    }
                                }
                            }
                        }

                        MyMenu item = new MyMenu()
                        {
                            name = temp[0],
                            id = Convert.ToInt32(temp[1]),
                            type = temp.Length > 3 ? Convert.ToInt32(temp[3]) : 0,
                            shows = mms
                        };

                        if (item.shows.Count > 0)
                            fullMenu.Add(item);
                    }
                    var cacheDuration = new TimeSpan(0, GlobalConfig.MenuCacheDuration, 0);
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(fullMenu);
                    cache.Put(cacheKey, jsonString, cacheDuration);
                }
                else
                    fullMenu = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MyMenu>>(jsonString);
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
            }
            return PartialView("_FooterNav", fullMenu);
        }
    }
}