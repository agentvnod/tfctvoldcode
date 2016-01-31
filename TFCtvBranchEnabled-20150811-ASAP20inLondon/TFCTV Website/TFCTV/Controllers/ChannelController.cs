using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class ChannelController : Controller
    {
        //
        // GET: /Channel/

        public ActionResult Details(int? id)
        {
            if (id == null)
                return RedirectToAction("Index", "Home");

            //if (id == 26 || id == 25)
            //    return Redirect("/LiveSpecials");

            var context = new IPTV2Entities();
            var channel = context.Channels.FirstOrDefault(c => c.ChannelId == id && c.Deleted == false);
            if (channel == null)
                return RedirectToAction("Index", "Home");

            ViewBag.isViewable = false;
            if (MyUtility.isUserLoggedIn())
            {
                var userId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    //var packageIds = MyUtility.StringToIntList(GlobalConfig.PackageIdsWithChannelAccess);
                    //int count = user.PackageEntitlements.Count(u => packageIds.Contains(u.PackageId) && u.EndDate > DateTime.Now);
                    int count = 0;
                    if (GlobalConfig.IsANCFreeStreamEnabled)
                    {
                        if (id == GlobalConfig.ANCChannelId)
                        {
                            DateTime registDt = DateTime.Now;
                            if (registDt > GlobalConfig.ANCFreeStreamStartDt && registDt < GlobalConfig.ANCFreeStreamEndDt)
                                count = user.Entitlements.Count(u => u.EndDate > DateTime.Now);
                            else
                            {
                                var packageIds = MyUtility.StringToIntList(GlobalConfig.PackageIdsWithChannelAccess);
                                count = user.PackageEntitlements.Count(u => packageIds.Contains(u.PackageId) && u.EndDate > DateTime.Now);
                            }
                        }
                        else
                        {
                            var packageIds = MyUtility.StringToIntList(GlobalConfig.PackageIdsWithChannelAccess);
                            count = user.PackageEntitlements.Count(u => packageIds.Contains(u.PackageId) && u.EndDate > DateTime.Now);
                        }
                    }
                    else
                    {
                        var packageIds = MyUtility.StringToIntList(GlobalConfig.PackageIdsWithChannelAccess);
                        count = user.PackageEntitlements.Count(u => packageIds.Contains(u.PackageId) && u.EndDate > DateTime.Now);
                    }

                    if (count > 0)
                        ViewBag.isViewable = true;
                }
            }

            return View(channel);
        }
    }
}