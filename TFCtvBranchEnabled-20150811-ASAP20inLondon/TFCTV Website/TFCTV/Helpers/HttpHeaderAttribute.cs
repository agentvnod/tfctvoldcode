using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using System.Web.Security;

namespace TFCTV.Helpers
{
    ///
    /// Represents an attribute that is used to add HTTP Headers to a Controller Action response.
    ///
    public class HttpHeaderAttribute : ActionFilterAttribute
    {
        ///
        /// Gets or sets the name of the HTTP Header.
        ///
        /// The name.
        public string Name { get; set; }

        ///
        /// Gets or sets the value of the HTTP Header.
        ///
        /// The value.
        public string Value { get; set; }

        ///
        /// Initializes a new instance of the  class.
        ///
        /// The name.
        /// The value.
        public HttpHeaderAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.AppendHeader(Name, Value);
            base.OnResultExecuted(filterContext);
        }
    }


    //public class DetectMultipleLogin : ActionFilterAttribute
    //{

    //    public override void OnResultExecuted(ResultExecutedContext filterContext)
    //    {
    //        if (MyUtility.isUserLoggedIn())
    //        {
    //            var context = new IPTV2Entities();
    //            var UserId = new Guid(filterContext.RequestContext.HttpContext.User.Identity.Name);
    //            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
    //            if (user != null)
    //            {
    //                HttpCookie authCookie = filterContext.RequestContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
    //                if (!String.IsNullOrEmpty(user.SessionId))
    //                {
    //                    if (String.Compare(user.SessionId, authCookie.Value, true) != 0)
    //                    {
    //                        FormsAuthentication.SignOut();

    //                    }
    //                }        
    //            }

    //        }            
    //        base.OnResultExecuted(filterContext);
    //    }
    //}

    public class RequireHttp : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // If the request has arrived via HTTPS...
            if (filterContext.HttpContext.Request.IsSecureConnection)
            {
                filterContext.Result = new RedirectResult(filterContext.HttpContext.Request.Url.ToString().Replace("https:", "http:")); // Go on, bugger off "s"!
                filterContext.Result.ExecuteResult(filterContext);
            }
            base.OnActionExecuting(filterContext);
        }
    }

    public class RequireHttpsOnProductionOnly : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.HttpContext != null && filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }
            else if (filterContext.HttpContext != null && GlobalConfig.isUAT) { return; }

            base.OnAuthorization(filterContext);
        }
    }
}