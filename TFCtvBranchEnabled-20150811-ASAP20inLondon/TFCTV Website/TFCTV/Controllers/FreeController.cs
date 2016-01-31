using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCTV.Helpers;

namespace TFCTV.Controllers
{
    public class FreeController : Controller
    {
        //
        // GET: /Free/

        public ActionResult Compensation201411(string id)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "Compensation Claim",
                TransactionType = "CompensationClaimTransaction"
            };
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    ReturnCode.StatusMessage = "You are missing some required information.";
                    return RedirectToAction("Index", "Home");
                }

                int freeProductId = GlobalConfig.Compensation201411ProductId1Month;
                var categoryid = GlobalConfig.Compensation201411CategoryId1Month;
                int takeamount = 2;

                var subscriptionproductids = MyUtility.StringToIntList(GlobalConfig.SubscriptionProductIds);
                var subscriptionProductIdsfor3days = MyUtility.StringToIntList(GlobalConfig.SubscriptionProductIds1Month);
                var subscriptionProductIdsfor7days = MyUtility.StringToIntList(GlobalConfig.SubscriptionProductIds12Month).Union(MyUtility.StringToIntList(GlobalConfig.SubscriptionProductIds3Month));
                var context = new IPTV2Entities();
                DateTime registDt = DateTime.Now;
                var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.Compensation2014PromoId && p.StatusId == GlobalConfig.Visible && registDt > p.StartDate && registDt < p.EndDate);
                if (promo != null)
                {
                    Guid userid;
                    Guid activationkey;
                    var uGuid = Guid.TryParse(id.Substring(0, 36), out userid);
                    var aGuid = Guid.TryParse(id.Substring(36, 36), out activationkey);
                    if (uGuid && aGuid)
                    {
                        var user = context.Users.FirstOrDefault(u => u.UserId == userid && u.ActivationKey == activationkey);
                        if (user != null)
                        {
                            var userpromo = context.UserPromos.FirstOrDefault(u => u.PromoId == GlobalConfig.Compensation2014PromoId && u.UserId == user.UserId);
                            if (userpromo != null)
                            {
                                if (userpromo.AuditTrail.UpdatedOn == null)
                                {
                                    var purchaseitem = context.PurchaseItems.OrderByDescending(p => p.PurchaseId).FirstOrDefault(p => p.RecipientUserId == userid && subscriptionproductids.Contains(p.ProductId));
                                    if (purchaseitem != null)
                                    {
                                        if ((subscriptionProductIdsfor7days.Contains(purchaseitem.ProductId)))
                                        {
                                            freeProductId = GlobalConfig.Compensation201411ProductId3and12Month;
                                            categoryid = GlobalConfig.Compensation201411CategoryId3and12Month;
                                            takeamount = 4;
                                        }

                                        ViewBag.targetUserId = userid.ToString();
                                        ViewBag.freeProductId = freeProductId;
                                        var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                                        var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);

                                        Category category = (Category)context.CategoryClasses.FirstOrDefault(c => c.CategoryId == categoryid && c.StatusId == GlobalConfig.Visible);
                                        if (category != null)
                                        {
                                            SortedSet<int> showIds = service.GetAllOnlineShowIds(MyUtility.getCountry(Request.GetUserHostAddressFromCloudflare()).getCode(), category);
                                            if (showIds.Count() == 0)
                                            {
                                                ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                                                ReturnCode.StatusMessage = "Shows not available.";
                                                return RedirectToAction("Index", "Home");
                                            }

                                            int[] setofShows = showIds.ToArray();
                                            var list = context.CategoryClasses.Where(c => setofShows.Contains(c.CategoryId) && c.StartDate <= registDt && c.EndDate >= registDt && c.StatusId == GlobalConfig.Visible).OrderBy(c => c.CategoryName).ThenBy(c => c.StartDate).ToList().Take(takeamount);

                                            List<CategoryShowListDisplay> catList = new List<CategoryShowListDisplay>();

                                            foreach (var item in list)
                                            {
                                                string img = String.IsNullOrEmpty(item.ImagePoster) ? GlobalConfig.AssetsBaseUrl + GlobalConfig.BlankGif : String.Format("{0}{1}/{2}", GlobalConfig.ShowImgPath, item.CategoryId.ToString(), item.ImagePoster);
                                                catList.Add(new CategoryShowListDisplay
                                                {
                                                    CategoryId = item.CategoryId,
                                                    Description = item.Description,
                                                    ImagePoster = img,
                                                    AiredDate = item.StartDate,
                                                });
                                            }
                                            return View(catList);
                                        }
                                        else
                                        {
                                            ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                                            ReturnCode.StatusMessage = "Package not found.";
                                        }
                                    }
                                    else
                                    {
                                        ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                                        ReturnCode.StatusMessage = "User's last paid purchase not found.";
                                    }
                                }
                                else
                                {
                                    ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                                    ReturnCode.StatusMessage = "You have already claimed this package.";
                                }
                            }
                            else
                            {
                                ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                                ReturnCode.StatusMessage = "You are not eligible for this compensation.";
                            }
                        }
                        else
                        {
                            ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                            ReturnCode.StatusMessage = "Target user not found.";
                        }
                    }
                    else
                    {
                        ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                        ReturnCode.StatusMessage = "Invalid link.";
                    }
                }
                else
                {
                    ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                    ReturnCode.StatusMessage = "Compensation program is disabled.";
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                ReturnCode.StatusMessage = e.Message;
            }
            if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                TempData["ErrorMessage"] = ReturnCode;
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult _Claim201411(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "Compensation Claim",
                TransactionType = "CompensationClaimTransaction"
            };
            var registDt = DateTime.Now;
            var userId = fc["targetUserId"];
            if (String.IsNullOrEmpty(userId))
                ReturnCode.StatusMessage = "You are missing some required information.";
            else
            {
                try
                {
                    var context = new IPTV2Entities();
                    var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(userId));
                    var subscriptionproductids = MyUtility.StringToIntList(GlobalConfig.SubscriptionProductIds);
                    var subscriptionProductIdsfor3days = MyUtility.StringToIntList(GlobalConfig.SubscriptionProductIds1Month);
                    var subscriptionProductIdsfor7days = MyUtility.StringToIntList(GlobalConfig.SubscriptionProductIds12Month).Union(MyUtility.StringToIntList(GlobalConfig.SubscriptionProductIds3Month));
                    var freeProductId = GlobalConfig.Compensation201411ProductId1Month;
                    var freeProductIdPremium = GlobalConfig.Compensatory3DaysProductId;
                    if (user != null)
                    {
                        var userpromo = context.UserPromos.FirstOrDefault(u => u.PromoId == GlobalConfig.Compensation2014PromoId && u.UserId == user.UserId);
                        if (userpromo != null)
                        {
                            if (userpromo.AuditTrail.UpdatedOn == null)
                            {
                                var purchaseitem = context.PurchaseItems.OrderByDescending(p => p.PurchaseId).FirstOrDefault(p => p.RecipientUserId == user.UserId && subscriptionproductids.Contains(p.ProductId));
                                if (purchaseitem != null)
                                {
                                    if ((subscriptionProductIdsfor7days.Contains(purchaseitem.ProductId)))
                                    {
                                        freeProductId = GlobalConfig.Compensation201411ProductId3and12Month;
                                        freeProductIdPremium = GlobalConfig.Compensatory7DaysProductId;

                                    }

                                    var success = PaymentHelper.PayViaWallet(context, user.UserId, freeProductId, SubscriptionProductType.Package, user.UserId, null);
                                    success = PaymentHelper.PayViaWallet(context, user.UserId, freeProductIdPremium, SubscriptionProductType.Package, user.UserId, null);
                                    if (success.Code == 0)
                                    {
                                        userpromo.AuditTrail.UpdatedOn = registDt;
                                        if (context.SaveChanges() > 0)
                                        {
                                            ReturnCode.StatusCode = 0;
                                            ReturnCode.StatusHeader = "Your Free Package starts now!";
                                            ReturnCode.StatusMessage = "Pwede ka nang manood ng mga piling Kapamilya shows at movies. Visit your My Subscriptions Page to see your free entitlements.";
                                        }
                                        else
                                        {
                                            ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                                            ReturnCode.StatusMessage = "Our system encountered an unidentified error. Please try again later.";
                                        }
                                    }
                                    else
                                    {
                                        ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                                        ReturnCode.StatusMessage = success.Message;
                                    }
                                }


                                else
                                {
                                    ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                                    ReturnCode.StatusMessage = "User's last paid purchase not found.";
                                }
                            }
                            else
                            {
                                ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                                ReturnCode.StatusMessage = "You have already claimed this package.";
                            }
                        }
                        else
                        {
                            ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                            ReturnCode.StatusMessage = "You are not eligible for this compensation.";
                        }
                    }
                    else
                    {
                        ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                        ReturnCode.StatusMessage = "Target user not found.";
                    }
                }
                catch (Exception e)
                {
                    MyUtility.LogException(e);
                    ReturnCode.StatusHeader = "Oops! There seems to be a problem.";
                    ReturnCode.StatusMessage = e.Message;
                }
            }
            if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                TempData["ErrorMessage"] = ReturnCode;
            return RedirectToAction("Index", "Home");
        }



    }
}
