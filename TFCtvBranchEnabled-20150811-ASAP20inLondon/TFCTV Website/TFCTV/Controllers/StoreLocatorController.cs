using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class StoreLocatorController : Controller
    {
        //
        // GET: /StoreLocator/
        [RequireHttp]
        public ActionResult Index()
        {
            try
            {
                var context = new IPTV2Entities();
                string userIp = Request.GetUserHostAddressFromCloudflare();
                if (Request.IsLocal)
                    userIp = "219.101.156.70";
                if (GlobalConfig.isUAT)
                {
                    if (!String.IsNullOrEmpty(Request["ip"]))
                        userIp = Request["ip"];
                }

                var ipLocation = MyUtility.getLocation(userIp);
                GeoLocation location = new GeoLocation() { Latitude = ipLocation.latitude, Longitude = ipLocation.longitude };
                SortedSet<StoreFrontDistance> result;
                if (GlobalConfig.maximumDistance != 0)
                    result = StoreFront.GetNearestStores(context, GlobalConfig.offeringId, location, true, GlobalConfig.maximumDistance);
                else
                    result = StoreFront.GetNearestStores(context, GlobalConfig.offeringId, location, true);

                List<StoreFront> stores = new List<StoreFront>();
                if (result != null)
                {
                    foreach (var item in result)
                        stores.Add(item.Store);
                    //var result = context.StoreFronts.Where(s => s.StatusId == GlobalConfig.Visible).ToList();
                    ViewBag.Location = ipLocation;
                    if (!Request.Cookies.AllKeys.Contains("version"))
                        return View("Index2", stores);
                    return View(stores);
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult GetStore(string id)
        {
            var context = new IPTV2Entities();
            var result = context.StoreFronts.Where(s => (s.BusinessName.Contains(id) || s.Address1.Contains(id) || s.City.Contains(id)) && s.StatusId == GlobalConfig.Visible && s.OfferingId == GlobalConfig.offeringId);
            List<StoreFront> list = new List<StoreFront>();
            foreach (var item in result)
            {
                list.Add(new StoreFront()
                {
                    StoreFrontId = item.StoreFrontId,
                    BusinessName = item.BusinessName,
                    ContactPerson = item.ContactPerson,
                    Address1 = item.Address1,
                    Address2 = item.Address2,
                    City = item.City,
                    State = item.State,
                    ZipCode = item.ZipCode,
                    CountryCode = item.CountryCode,
                    BusinessPhone = item.BusinessPhone,
                    EMailAddress = item.EMailAddress,
                    WebSiteUrl = item.WebSiteUrl,
                    MobilePhone = item.MobilePhone,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude
                });
            }
            return this.Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}