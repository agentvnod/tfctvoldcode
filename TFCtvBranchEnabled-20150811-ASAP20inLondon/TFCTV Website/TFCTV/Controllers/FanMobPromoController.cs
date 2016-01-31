using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Helpers;
using IPTV2_Model;

namespace TFCTV.Controllers
{
    public class FanMobPromoController : Controller
    {
        //
        // GET: /AnnivPromo/

        [RequireHttp]
        public ActionResult Index()
        {
            if (!ContextHelper.isPromoEnabled(GlobalConfig.TFCtvFirstYearAnniversaryPromoId))
                return RedirectToAction("Winners", "FanMobPromo");
            bool userJoined = false;
            userJoined = ContextHelper.IsUserPartOfPromo(GlobalConfig.TFCtvFirstYearAnniversaryPromoId, User.Identity.IsAuthenticated ? new Guid(User.Identity.Name) : Guid.Empty);
            ViewBag.UserJoined = userJoined;
            return View();
        }

        public ActionResult Mechanics()
        {
            bool userJoined = ContextHelper.IsUserPartOfPromo(GlobalConfig.TFCtvFirstYearAnniversaryPromoId, User.Identity.IsAuthenticated ? new Guid(User.Identity.Name) : Guid.Empty);
            ViewBag.UserJoined = userJoined;
            return View();
        }

        public ActionResult Winners()
        {
            bool userJoined = ContextHelper.IsUserPartOfPromo(GlobalConfig.TFCtvFirstYearAnniversaryPromoId, User.Identity.IsAuthenticated ? new Guid(User.Identity.Name) : Guid.Empty);
            ViewBag.UserJoined = userJoined;
            return View();
        }
        public ActionResult EnterPromo()
        {
            return RedirectToAction("Index", "Home");
            //if (!ContextHelper.isPromoEnabled(GlobalConfig.TFCtvFirstYearAnniversaryPromoId))
            //    return RedirectToAction("Index", "Home");
            //Boolean isConnected = false;
            //if (ContextHelper.IsUserPartOfPromo(GlobalConfig.TFCtvFirstYearAnniversaryPromoId, User.Identity.IsAuthenticated ? new Guid(User.Identity.Name) : Guid.Empty))
            //    return RedirectToAction("Index"); ;
            //if (User.Identity.IsAuthenticated)
            //    isConnected = isSociallyConnected();
            //ViewBag.isSociallyConnected = isConnected;
            //return View();
        }
        public ActionResult Profile()
        {
            if (!ContextHelper.isPromoEnabled(GlobalConfig.TFCtvFirstYearAnniversaryPromoId))
                return RedirectToAction("Index", "Home");
            Boolean userJoined = false;
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index");
            else
            {
                userJoined = ContextHelper.IsUserPartOfPromo(GlobalConfig.TFCtvFirstYearAnniversaryPromoId, User.Identity.IsAuthenticated ? new Guid(User.Identity.Name) : Guid.Empty);
                if (!userJoined)
                    return RedirectToAction("Index");
            }
            ViewBag.UserJoined = userJoined;
            try
            {
                var context = new IPTV2Entities();
                var UserId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                return View(user);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return RedirectToAction("Index");
        }

        public Boolean isSociallyConnected()
        {
            Boolean isConnected = false;
            if (User.Identity.IsAuthenticated)
            {
                var providers = GigyaMethods.GetUserInfoByKey(new Guid(User.Identity.Name), "providers");
                var providerList = providers.Split(',');
                if (providerList != null)
                    if (providerList.Count() > 0)
                        if (providerList.Count(p => String.Compare(p, "site", true) != 0) > 0)
                            isConnected = true;
            }
            return isConnected;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult _EnterPromo(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };
            DateTime registDt = DateTime.Now;
            int promoID = GlobalConfig.TFCtvFirstYearAnniversaryPromoId;
            var userID = new System.Guid(User.Identity.Name);

            try
            {
                var context = new IPTV2Entities();
                Promo promo = context.Promos.FirstOrDefault(p => p.PromoId == promoID && p.StatusId == GlobalConfig.Visible && p.EndDate > registDt && p.StartDate < registDt);
                if (promo != null)
                {
                    UserPromo userPromo = context.UserPromos.FirstOrDefault(u => u.UserId == userID && u.PromoId == promoID);
                    if (userPromo == null)

                        userPromo = new UserPromo()
                        {
                            PromoId = promoID,
                            UserId = userID,
                            AuditTrail = new AuditTrail() { CreatedOn = DateTime.Now }
                        };
                    context.UserPromos.Add(userPromo);
                    if (context.SaveChanges() > 0)
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                        ReturnCode.StatusMessage = "You have successfully joined the promo.";
                        //adds points for each presconnected SNS
                        var providers = GigyaMethods.GetUserInfoByKey(userID, "providers");
                        var providerList = providers.Split(',');
                        foreach (string p in providerList)
                        {
                            if (!(String.Compare(p, "site") == 0))
                            {
                                GigyaActionSingleAttribute actionAttribute = new GigyaActionSingleAttribute();
                                {
                                    actionAttribute.description = new List<string> { "You connected to " + p };
                                }
                                GigyaMethods.NotifyAction(userID, AnniversaryPromo.AnnivPromo_LinkingSNS.ToString(), actionAttribute);
                            }
                        }
                    }
                    else
                    {
                        ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                        ReturnCode.StatusMessage = "You have already joined the promo.";
                    }
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
            }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }
    }
}
