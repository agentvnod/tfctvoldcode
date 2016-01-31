using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class BetaController : Controller
    {
        //
        // GET: /Beta/

        public ActionResult Index()
        {
            if (MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public ActionResult _SignUp(FormCollection f)
        {
            var context = new IPTV2Entities();
            Dictionary<string, object> collection = new Dictionary<string, object>();
            if (String.IsNullOrEmpty(f["email"]))
            {
                collection.Add("errorCode", -1);
                collection.Add("errorMessage", "Please fill up your email address.");
                return Content(MyUtility.buildJson(collection), "application/json");
                // Do something
            }
            string email = f["email"];

            RegexUtilities util = new RegexUtilities();
            //if (!MyUtility.isEmail(email))
            if (!util.IsValidEmail(email))
            {
                collection.Add("errorCode", (int)ErrorCodes.IsNotValidEmail);
                collection.Add("errorMessage", "Invalid email format.");
                return Content(MyUtility.buildJson(collection), "application/json");
            }
            var tester = context.BetaTesters.FirstOrDefault(b => b.EMailAddress == email);
            if (tester == null) // New sign up
            {
                context.BetaTesters.Add(new BetaTester() { EMailAddress = email, DateSent = DateTime.Now, InvitationKey = System.Guid.NewGuid(), InvitedBy = System.Guid.Parse("9B4216E8-69BA-4548-9552-4CD065E58D3E") });
                int result = context.SaveChanges();
                if (result > 0)
                {
                    //Success
                    collection.Add("errorCode", 0);
                    collection.Add("errorMessage", "Thank you for signing up!");
                }
                else
                {
                    //Fail
                    collection.Add("errorCode", -2);
                    collection.Add("errorMessage", "The system encountered an unidentified error. Please try again.");
                }
            }
            else
            {
                // USer has signed up
                collection.Add("errorCode", -3);
                collection.Add("errorMessage", "You have already signed up.");
            }

            return Content(MyUtility.buildJson(collection), "application/json");
        }

        public ActionResult Verify(string id)
        {
            if (String.IsNullOrEmpty(id))
                return new HttpNotFoundResult();
            System.Guid g;
            if (!System.Guid.TryParse(id, out g))
                return new HttpNotFoundResult();

            var context = new IPTV2Entities();
            var tester = context.BetaTesters.FirstOrDefault(b => b.InvitationKey == new System.Guid(id) && b.DateClaimed == null);
            if (tester != null)
            {
                //tester.DateClaimed = DateTime.Now;
                //tester.IpAddress = Request.GetUserHostAddressFromCloudflare();
                //if (context.SaveChanges() > 0)
                //{
                //    HttpCookie cookie = new HttpCookie("testerid");
                //    cookie.Value = id;
                //    cookie.Expires = DateTime.Now.AddYears(10);
                //    this.ControllerContext.HttpContext.Response.SetCookie(cookie);
                //    //return RedirectToAction("Index", "Home");
                //    return RedirectToAction("Register", "User");
                //}

                HttpCookie cookie = new HttpCookie("testerid");
                cookie.Value = id;
                cookie.Expires = DateTime.Now.AddYears(10);
                this.ControllerContext.HttpContext.Response.SetCookie(cookie);
                return RedirectToAction("Register", "User", new { iid = id });
            }
            else
                return Content("Key already used.", "text/html");

            //return new HttpNotFoundResult();
        }

        public void Check()
        {
            HttpCookie cookie = new HttpCookie("testerid");
            cookie = this.ControllerContext.HttpContext.Request.Cookies["testerid"];
            BetaTester tester = null;
            bool isAllowed = false;
            if (cookie != null)
            {
                if (cookie.Value.Length != 0)
                {
                    var context = new IPTV2Entities();
                    tester = context.BetaTesters.FirstOrDefault(b => b.InvitationKey == new System.Guid(cookie.Value));
                    if (tester != null)
                    {
                        //                        return Content(String.Empty, "text/html");
                        isAllowed = true;
                    }
                }
            }

            string ip = GlobalConfig.IpWhiteList;
            string[] IpAddresses = ip.Split(';');
            bool isWhitelisted = IpAddresses.Contains(Request.GetUserHostAddressFromCloudflare());
            if (isWhitelisted)
            {
                isAllowed = true;
                //return Content(String.Empty, "text/html");
            }

            if (isSearchSpider())
                isAllowed = true;

            //If Ip address is from Allowed Countries
            string allowedcountries = GlobalConfig.AllowedCountries;
            string[] countries = allowedcountries.Split(';');
            bool isCountryUnrestricted = countries.Contains(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode());
            if (isCountryUnrestricted)
                isAllowed = true;

            if (MyUtility.isUserLoggedIn())
                isAllowed = true;

            if (!isAllowed)
                ControllerContext.HttpContext.Response.Redirect("/Beta");
            //string meta = @"<meta http-equiv=""refresh"" content=""0; url=/Beta/Index"">";
            //return Content(meta, "text/html");
            //return RedirectToAction("Index");

            //ControllerContext.HttpContext.Response.StatusCode = 404;
            //throw new HttpException(404, "Page not found");
        }

        private bool isSearchSpider()
        {
            return System.Text.RegularExpressions.Regex.IsMatch(Request.UserAgent, "bingbot|googlebot|msnbot", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }
}