using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;

namespace TFCTV.Controllers
{
    public class NewsletterController : Controller
    {
        //
        // GET: /Newsletter/

        public ActionResult Beta(string email, string firstname, string key)
        {
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(key))
                return RedirectToAction("Index", "Home");

            var context = new IPTV2Entities();

            var tester = context.BetaTesters.FirstOrDefault(i => i.EMailAddress == email && i.InvitationKey == new Guid(key) && i.DateClaimed == null && String.IsNullOrEmpty(i.IpAddress));
            if (tester == null)
                return RedirectToAction("Index", "Home");

            ViewBag.InvitationKey = String.Format("{0}{1}", "/Beta/Verify/", tester.InvitationKey.ToString());
            ViewBag.FirstName = String.IsNullOrEmpty(firstname) ? "Hello Kapamilya" : firstname;

            return View();
        }

        public ActionResult FreeTrial(string email, string serial, string pin)
        {
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(serial) || String.IsNullOrEmpty(pin))
                return RedirectToAction("Index", "Home");

            var context = new IPTV2Entities();

            var user = context.Users.FirstOrDefault(i => String.Compare(i.EMail, email, true) == 0);
            if (user == null)
                return RedirectToAction("Index", "Home");

            ViewBag.FirstName = String.IsNullOrEmpty(user.FirstName) ? "Hello Kapamilya" : user.FirstName;
            ViewBag.Serial = serial;
            ViewBag.Pin = pin;

            return View();
        }
    }
}