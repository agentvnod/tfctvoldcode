using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class EventsController : Controller
    {
        //
        // GET: /Event/

        public ActionResult Index(string id)
        {
            ViewBag.IsAllowed = false;
            DateTime registDt = DateTime.Now;
            ViewBag.Key = id;

            //check if regcookie
            Boolean testRegC = false;
            try
            {
                var dt = DateTime.Parse(Request.Cookies["rcDate"].Value);
                if (registDt.Subtract(dt).Days < 45)
                { testRegC = true; }
            }
            catch (Exception) { }

            if (!String.IsNullOrEmpty(id))
            {
                //check if key exists
                var context = new IPTV2Entities();
                var onlineEvent = context.OnlineEvents.FirstOrDefault(p => String.Compare(p.Code, id, true) == 0);
                if (onlineEvent != null)
                {
                    if (onlineEvent.EndDate != null)
                    {
                        ViewBag.PageTitle = onlineEvent.Name;
                        ViewBag.MetaDescription = onlineEvent.Description;

                        if (onlineEvent.StatusId == GlobalConfig.Visible && onlineEvent.StartDate > registDt)
                        {
                            ViewBag.NotExist = true;
                            return View();
                        }

                        string timeZone = String.IsNullOrEmpty(onlineEvent.Timezone) ? "UTC" : onlineEvent.Timezone;
                        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                        DateTime ed = (DateTime)onlineEvent.EndDate;
                        var localTime = TimeZoneInfo.ConvertTimeFromUtc(ed, zone);
                        var timezoneName = TimeZoneNames.TimeZoneNames.GetAbbreviationsForTimeZone(zone.Id, "en-US");
                        var abbrTimeZone = timezoneName.Standard;
                        if (zone.IsDaylightSavingTime(localTime))
                        {
                            abbrTimeZone = timezoneName.Daylight;
                            //localTime = localTime.AddHours(1);
                        }
                        ViewBag.EndDate = String.Format("{0} {1}", localTime.ToString("MM/dd/yyyy hh:mm tt"), abbrTimeZone);
                    }

                    //check if key is active 
                    if (onlineEvent.StatusId == GlobalConfig.Visible && onlineEvent.StartDate < registDt && onlineEvent.EndDate > registDt)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            ViewBag.IsLoggedIn = true;
                            return View();
                        }
                        else if (testRegC)
                        {
                            ViewBag.IsRegistered = true;
                            return View();
                        }

                        HttpCookie preBlackCookie = new HttpCookie("vntycok");
                        preBlackCookie.Expires = DateTime.Now.AddDays(1);
                        Response.Cookies.Add(preBlackCookie);
                        ViewBag.IsAllowed = true;
                    }
                    else
                    {
                        ViewBag.IsOver = true;
                    }
                }
                else
                    ViewBag.NotExist = true;
            }
            else
                return RedirectToAction("Index", "Home");
            return View();
        }

    }
}
