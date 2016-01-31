using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class OfferController : Controller
    {
        //
        // GET: /Offer/

        public ActionResult Index(string id)
        {
            try
            {
                if (!String.IsNullOrEmpty(id))
                {
                    if (String.Compare(id, "survey") == 0)
                    {
                        if (!User.Identity.IsAuthenticated)
                        {
                            TempData["LoginErrorMessage"] = "Please sign in to claim your FREE TFC.tv 1-month subscription.";
                            TempData["RedirectUrl"] = Request.Url.PathAndQuery;
                            return RedirectToAction("Index", "Home");
                        }
                        survey();
                        return RedirectToAction("Index", "Home");
                    }
                    else
                        if (String.Compare(id, "xoom") == 0)
                        {
                            var ReturnCode = new TransactionReturnType()
                            {
                                StatusCode = (int)ErrorCodes.UnknownError,
                                StatusMessage = String.Empty,
                                info = "Promo",
                                TransactionType = "Xoom Promo"
                            };
                            if (!User.Identity.IsAuthenticated)
                            {
                                //drop cookie
                                HttpCookie xoomCookie = new HttpCookie("xoom");
                                xoomCookie.Expires = DateTime.Now.AddDays(1);
                                Response.Cookies.Add(xoomCookie);

                                TempData["LoginErrorMessage"] = "Please register to avail of this promo.";
                                TempData["RedirectUrl"] = Request.Url.PathAndQuery;
                                return RedirectToAction("Register", "User");
                            }
                            else
                            {
                                DateTime registDt = DateTime.Now;
                                var context = new IPTV2Entities();
                                var user = context.Users.Find(new Guid(User.Identity.Name));
                                var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.Xoom2PromoId && p.StatusId == GlobalConfig.Visible && p.StartDate < registDt && p.EndDate > registDt);
                                if (promo != null)
                                {
                                    if (user.StatusId == GlobalConfig.Visible)
                                    {
                                        bool userPurchased = false;
                                        bool isTVE = false;
                                        if (user.IsTVEverywhere != null)
                                            isTVE = (bool)user.IsTVEverywhere;
                                        var entitlements = context.Entitlements.Where(e => e.EndDate > registDt && e.UserId == user.UserId);
                                        if (entitlements != null && entitlements.Count() > 0)
                                        {
                                            foreach (Entitlement e in entitlements)
                                            {
                                                if (e is PackageEntitlement)
                                                {
                                                    var packageEnt = (PackageEntitlement)e;
                                                    if (packageEnt.PackageId == GlobalConfig.PremiumPackageId)
                                                    {
                                                        var product = packageEnt.LatestEntitlementRequest.Product;
                                                        if (product.GetPrice(user.Country) > 0)
                                                            userPurchased = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        if (!userPurchased && !isTVE)
                                        {
                                            var userPromo = context.UserPromos.FirstOrDefault(u => u.UserId == user.UserId && u.PromoId == GlobalConfig.Xoom2PromoId);
                                            if (userPromo == null)
                                            {
                                                userPromo = new UserPromo();
                                                userPromo.UserId = user.UserId;
                                                userPromo.PromoId = GlobalConfig.Xoom2PromoId;
                                                userPromo.AuditTrail.CreatedOn = registDt;
                                                context.UserPromos.Add(userPromo);
                                                context.SaveChanges();

                                            }
                                            return Redirect(String.Format("/Subscribe/Details{0}", Request.Url.Query));
                                        }
                                        else
                                        {
                                            ReturnCode.StatusHeader = "This promo is available only to XOOM Customers";
                                            ReturnCode.StatusMessage = "with no existing TFC.tv Premium subscriptions or TFC Everywhere account.";
                                        }
                                    }
                                    else
                                    {
                                        ReturnCode.StatusHeader = "You have not yet verified your email address";
                                        ReturnCode.StatusMessage = "Please verify your newly registered account via the email address you provided.";
                                    }
                                }
                                else
                                {
                                    ReturnCode.StatusMessage = "Promo is no longer active";
                                }
                                if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                                {
                                    TempData["ErrorMessage"] = ReturnCode;
                                }
                                return Redirect(String.Format("/Home/Index/{0}", Request.Url.Query));
                            }
                        }
                        else
                        {
                            if (!User.Identity.IsAuthenticated)
                            {
                                TempData["LoginErrorMessage"] = "Please sign in to avail of this promo.";
                                TempData["RedirectUrl"] = Request.Url.PathAndQuery;
                                return RedirectToAction("Index", "Home");
                            }
                            return Redirect(String.Format("/Subscribe/Details/{0}{1}", id, Request.Url.Query));
                        }
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                var ReturnCode = new TransactionReturnType()
                {
                    StatusCode = (int)ErrorCodes.UnknownError,
                    StatusMessage = e.Message,
                    StatusHeader = "Oops! There seems to be a problem."
                };

                TempData["ErrorMessage"] = ReturnCode;
            }
            return RedirectToAction("Index", "Home");
        }

        private void survey()
        {
            DateTime registDt = DateTime.Now;
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "Promo",
                TransactionType = "Survey Compensation"
            };
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    TempData["LoginErrorMessage"] = "Please sign in to claim your FREE TFC.tv 1-month subscription.";
                    TempData["RedirectUrl"] = Request.Url.PathAndQuery;
                }
                else
                {
                    var context = new IPTV2Entities();
                    var user = context.Users.Find(new Guid(User.Identity.Name));
                    var promos = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.SurveyPromoId && p.StatusId == GlobalConfig.Visible && p.StartDate < registDt && p.EndDate > registDt);
                    if (promos != null)
                    {
                        var userpromo = context.UserPromos.FirstOrDefault(u => u.PromoId == GlobalConfig.SurveyPromoId && u.UserId == new Guid(User.Identity.Name));
                        if (userpromo != null)
                        {
                            if (userpromo.AuditTrail.UpdatedOn == null)
                            {
                                PaymentHelper.PayViaWallet(context, user.UserId, GlobalConfig.PremiumComplimentary1Month, SubscriptionProductType.Package, user.UserId, null);
                                userpromo.AuditTrail.UpdatedOn = registDt;
                                context.SaveChanges();
                                ReturnCode.StatusHeader = "You have successfully claimed your FREE subscription!";
                                ReturnCode.StatusMessage = "Congratulations! Pwede mo na ulit mapanood ang mga piling Kapamilya shows at movies with your FREE 1-month Premium subscription.";
                            }
                            else
                            {
                                //ReturnCode.StatusHeader = "You have already claimed your free subscription. Please check your MY Subscriptions page.";
                                ReturnCode.StatusMessage = "You have already claimed your free subscription. Please check your MY Subscriptions page.";
                            }
                        }
                        else
                        {
                            //ReturnCode.StatusHeader = "You are not qualified for this offer.";
                            ReturnCode.StatusMessage = "You are not qualified for this offer.";
                        }
                    }
                    else
                    {
                        //ReturnCode.StatusHeader = "You are not qualified for this offer.";
                        ReturnCode.StatusMessage = "The promo has already ended.";
                    }
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                ReturnCode.StatusMessage = e.Message;
            }
            if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
            {
                TempData["ErrorMessage"] = ReturnCode;
            }
        }
    }
}
