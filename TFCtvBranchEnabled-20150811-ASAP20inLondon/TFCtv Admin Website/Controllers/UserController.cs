using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TFCtv_Admin_Website.Helpers;

namespace TFCtv_Admin_Website.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/

        //public ActionResult Index()
        //{
        //    MembershipCreateStatus status;
        //    Membership.CreateUser("aacosta", "june101973", "Amabel_Acosta@abs-cbn.com", "What is your mother's maiden name?", "Amabel Acosta", true, out status);            
        //    var collection = Membership.GetAllUsers();            
        //    return View(collection);            
        //}

        public ActionResult Index()
        {
            bool isAllowed = false;
            isAllowed = MyUtility.IsIpAddressAllowed(Request);
            if (!isAllowed)
                isAllowed = MyUtility.IsUserAllowed();
            if (isAllowed)
                return View();
            throw new TFCtvUnauthorizedAccess();
        }

    }
}
