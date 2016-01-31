using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Helpers;
using Newtonsoft.Json;
using IPTV2_Model;
using System.Net;
using System.Text;
using System.IO;
using System.Globalization;


namespace TFCTV.Controllers
{
    public class ITEController : Controller
    {
        //
        // GET: /ITE/

        [RequireHttp]
        public ActionResult Index()
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.NotAuthenticated,
                StatusMessage = String.Empty,
                info = "IT&E Validation",
                TransactionType = "Validation"
            };

            if (User.Identity.IsAuthenticated)
            {
                DateTime registDt = DateTime.Now;
                IPTV2Entities context = new IPTV2Entities();
                var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.ITEPromoId && p.StartDate < registDt && registDt < p.EndDate && p.StatusId == GlobalConfig.Visible);
                if (promo != null)
                    return View();
                else { ReturnCode.StatusCode = (int)ErrorCodes.UnauthorizedCountry; }
            }
            ReturnCode.StatusMessage = MyUtility.getErrorMessage((ErrorCodes)ReturnCode.StatusCode);
            if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
            {
                TempData["ErrorMessage"] = ReturnCode;
            }
            return RedirectToAction("Index", "Home");
        }

        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _Validate(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "IT&E Validation",
                TransactionType = "Validation"
            };
            ITEResponseError iteRespErr = ITEResponseError.UNKNOWN_ERROR;

            DateTime registDt = DateTime.Now;
            Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
            bool isMissingRequiredFields = false;
            foreach (var x in tmpCollection)
            {
                if (String.IsNullOrEmpty(x.Value))
                {
                    isMissingRequiredFields = true;
                    break;
                }
            }
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (!isMissingRequiredFields) // process form
                    {
                        string activationCode = fc["activation_code"];
                        string phoneNumber = fc["phone_num"];
                        int idType = Convert.ToInt16(ITEIdType.PhoneNumber);
                        string userID = User.Identity.Name;

                        IPTV2Entities context = new IPTV2Entities();
                        //check first if combination already exists                    
                        if (context.ITEDetails.Count(i => i.UserId == new Guid(userID)) <= 0)
                        {
                            if (context.ITEDetails.Count(i => i.ITEId == phoneNumber) <= 0)
                            {
                                var request = (HttpWebRequest)WebRequest.Create(GlobalConfig.ITEValidateURL);

                                var postData = String.Format("idtype={0}&accountani={1}&validationcode={2}&tfcTVUserID={3}", idType.ToString(), phoneNumber, activationCode, userID);
                                var data = Encoding.ASCII.GetBytes(postData);

                                request.Method = "POST";
                                request.ContentType = "application/x-www-form-urlencoded";
                                request.ContentLength = data.Length;

                                using (var stream = request.GetRequestStream())
                                {
                                    stream.Write(data, 0, data.Length);
                                }

                                var response = (HttpWebResponse)request.GetResponse();
                                //post to ite validation
                                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                                // get response
                                ITEResponse iteResp = JsonConvert.DeserializeObject<ITEResponse>(responseString);
                                if (iteResp.StatusCode == 0)
                                {   //add entitlement                                                                        
                                    //var errResp = PaymentHelper.PayViaWallet(context, new Guid(userID), Convert.ToInt16(ITEPackageType.LITEPACKAGE), SubscriptionProductType.Package, new Guid(userID), null, String.Format("ACTIVATE IT&E ({0})", phoneNumber));

                                    var UserId = new Guid(userID);
                                    var subscriptionEndDate = iteResp.expirationDate;
                                    var IteId = phoneNumber.Trim();
                                    var errResp = PaymentHelper.PayViaWalletWithEndDate(context, UserId, (int)ITEPackageType.LITEPACKAGE, SubscriptionProductType.Package, UserId, null, subscriptionEndDate, String.Format("ACTIVATE IT&E ({0})", IteId));

                                    //once successfull insert to iteusers
                                    if (errResp.Code == (int)ErrorCodes.Success)
                                    {
                                        ITEDetail iteUser = new ITEDetail();
                                        iteUser.ITEId = IteId;
                                        iteUser.UserId = UserId;
                                        context.ITEDetails.Add(iteUser);
                                        context.SaveChanges();

                                        iteRespErr = ITEResponseError.VALIDATION_SUCCESS;
                                        ReturnCode.StatusMessage = MyUtility.getITEError(iteRespErr);
                                        if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
                                        {
                                            TempData["ErrorMessage"] = ReturnCode;
                                        }
                                        //redirect to homepage with success or failure message
                                        return RedirectToAction("Index", "Home");
                                    }
                                    else
                                    {
                                        ReturnCode.StatusCode = errResp.Code;
                                        ReturnCode.StatusMessage = MyUtility.getErrorMessage((ErrorCodes)errResp.Code);
                                    }
                                }
                                else
                                    iteRespErr = (ITEResponseError)iteResp.StatusCode;
                            }
                            else
                                iteRespErr = ITEResponseError.ACCOUNTANI_ALREADY_USED;
                        }
                        else
                            iteRespErr = ITEResponseError.TFCTV_ACCOUNT_ALREADY_ACTIVATED;

                    }
                    else
                        iteRespErr = ITEResponseError.ACCOUNTANI_SHOULD_BE_DEFINED;
                }
                else
                    iteRespErr = ITEResponseError.REGISTRATION_REQUIRED;
                ReturnCode.StatusMessage = MyUtility.getITEError(iteRespErr);
            }
            catch (Exception e)
            {
                MyUtility.LogException(e); iteRespErr = ITEResponseError.UNKNOWN_ERROR;
                if (GlobalConfig.isUAT)
                    ReturnCode.StatusMessage = e.Message;
                else
                    ReturnCode.StatusMessage = MyUtility.getITEError(iteRespErr);
            }

            if (!String.IsNullOrEmpty(ReturnCode.StatusMessage))
            {
                TempData["ErrorMessage"] = ReturnCode;
            }
            //redirect to homepage with success or failure message
            return RedirectToAction("Index", "ITE");
        }

        //[RequireHttps]
        [HttpPost]
        public ActionResult iteDeactivate(FormCollection fc) //int idType, string iteId, string tfctvUserId, string packageType, DateTime deactivationDate, string reason
        {
            ITEResponseError iteRespErr = ITEResponseError.UNKNOWN_ERROR;
            var iteResp = new ITEResponse()
            {
                StatusCode = (int)ITEResponseError.UNKNOWN_ERROR,
                Message = MyUtility.getITEError(iteRespErr)
            };
            try
            {
                try
                {
                    var requestingIp = Request.GetUserHostAddressFromCloudflare();
                    var whiteListedIp = GlobalConfig.ITEWhitelistedIp.Split(',');
                    if (!whiteListedIp.Contains(requestingIp))
                    {
                        iteRespErr = ITEResponseError.IP_NOT_ALLOWED;
                        iteResp.Message = String.Format("{0},{1},{2},{3},{4}", (int)iteRespErr, iteRespErr.ToString(), String.Empty, String.Empty, String.Empty);
                        return this.Content(iteResp.Message, "plain/text");
                    }
                }
                catch (Exception) { }

                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;
                foreach (var x in tmpCollection)
                {
                    if (String.IsNullOrEmpty(x.Value))
                    {
                        isMissingRequiredFields = true;
                        break;
                    }
                }

                if (!isMissingRequiredFields) // process form
                {
                    int idType = Convert.ToInt32(fc["idType"]);
                    string iteId = fc["iteId"];
                    string tfctvUserId = fc["tfctvUserId"];
                    string packageType = fc["packageType"];
                    DateTime deactivationDate = Convert.ToDateTime(fc["deactivationDate"]);
                    string reason = fc["reason"];

                    var registDt = System.DateTime.Now;
                    IPTV2Entities context = new IPTV2Entities();
                    ITEDetail iteUser = context.ITEDetails.FirstOrDefault(i => String.Compare(i.ITEId, iteId) == 0 && String.Compare(i.UserId.ToString(), tfctvUserId) == 0);
                    //check if iteuser exists
                    if (iteUser != null)
                    {
                        int packageId = ContextHelper.GetPackageFromProductId((int)ITEPackageType.LITEPACKAGE);
                        var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(tfctvUserId));
                        var iteEntitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == packageId);
                        int latestITEEntitlementReqId = (int)iteEntitlement.LatestEntitlementRequestId;
                        var purchaseItem = context.PurchaseItems.FirstOrDefault(p => p.EntitlementRequestId == latestITEEntitlementReqId);
                        var iteTransaction = iteUser.User.Transactions.FirstOrDefault(t => t is WalletPaymentTransaction && ((WalletPaymentTransaction)t).PurchaseId == purchaseItem.PurchaseId);

                        //check user has free entitlement
                        if (iteEntitlement != null)
                        {
                            iteEntitlement.EndDate = deactivationDate;
                            var cancellation = new CancellationTransaction()
                            {
                                CancellationRemarks = reason,
                                GomsTransactionId = -1000,
                                GomsTransactionDate = registDt,
                                OriginalTransactionId = iteTransaction != null ? iteTransaction.TransactionId : 0,
                                StatusId = 1,
                                OfferingId = GlobalConfig.offeringId,
                                Date = registDt,
                                Amount = 0,
                                Currency = purchaseItem.Currency,
                                Reference = String.Format("DEACTIVATE IT&E ({0})", user.ITEDetail.ITEId)
                            };
                            user.Transactions.Add(cancellation);
                            if (context.SaveChanges() > 0)
                            {
                                iteRespErr = ITEResponseError.SUCCESS;
                                iteResp.StatusCode = (int)ITEResponseError.SUCCESS;
                                //iteResp.Message = String.Format(MyUtility.getITEError(ITEResponseError.SUCCESS), ITEPackageType.LITEPACKAGE.ToString(), deactivationDate.ToShortDateString());
                                iteResp.Message = String.Format("{0},{1},{2},{3},{4}", iteResp.StatusCode, iteRespErr.ToString(), ITEPackageType.LITEPACKAGE.ToString(), iteEntitlement.EndDate.ToShortDateString(), user.UserId.ToString());
                                return this.Content(iteResp.Message, "plain/text");
                            }
                        }
                        else
                            iteRespErr = ITEResponseError.ACCOUNTANI_NOT_FOUND;
                    }
                    else
                        iteRespErr = ITEResponseError.ACCOUNTANI_NOT_FOUND;
                }
                else
                    iteRespErr = ITEResponseError.MISSING_PARAMETERS;

                iteResp.StatusCode = (int)iteRespErr;
                //iteResp.Message = MyUtility.getITEError(iteRespErr);
                iteResp.Message = String.Format("{0},{1},{2},{3},{4}", iteResp.StatusCode, iteRespErr.ToString(), String.Empty, String.Empty, String.Empty);
            }
            catch (Exception e) { MyUtility.LogException(e); iteRespErr = ITEResponseError.UNKNOWN_ERROR; }
            //return this.Json(iteResp, JsonRequestBehavior.AllowGet);
            return this.Content(iteResp.Message, "plain/text");
        }

        //[RequireHttps]
        [HttpPost]
        public ActionResult iteActivate(FormCollection fc) //int idType, string iteId, string tfctvUserId, string packageType, DateTime subscriptionEndDate, string comments
        {
            ITEResponseError iteRespErr = ITEResponseError.UNKNOWN_ERROR;
            var iteResp = new ITEResponse()
            {
                StatusCode = (int)ITEResponseError.UNKNOWN_ERROR,
                Message = MyUtility.getITEError(iteRespErr)
            };
            try
            {
                try
                {
                    var requestingIp = Request.GetUserHostAddressFromCloudflare();
                    var whiteListedIp = GlobalConfig.ITEWhitelistedIp.Split(',');
                    if (!whiteListedIp.Contains(requestingIp))
                    {
                        iteRespErr = ITEResponseError.IP_NOT_ALLOWED;
                        iteResp.Message = String.Format("{0},{1},{2},{3},{4}", (int)iteRespErr, iteRespErr.ToString(), String.Empty, String.Empty, String.Empty);
                        return this.Content(iteResp.Message, "plain/text");
                    }
                }
                catch (Exception) { }

                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;
                foreach (var x in tmpCollection)
                {
                    if (String.IsNullOrEmpty(x.Value))
                    {
                        isMissingRequiredFields = true;
                        break;
                    }
                }

                if (!isMissingRequiredFields) // process form
                {

                    int idType = Convert.ToInt32(fc["idType"]);
                    string iteId = fc["iteId"];
                    string tfctvUserId = fc["tfctvUserId"];
                    string packageType = fc["packageType"];
                    DateTime subscriptionEndDate = Convert.ToDateTime(fc["subscriptionEndDate"]);
                    string comments = fc["comments"];

                    IPTV2Entities context = new IPTV2Entities();
                    ITEDetail iteUser = context.ITEDetails.FirstOrDefault(i => String.Compare(i.ITEId, iteId) == 0 && String.Compare(i.UserId.ToString(), tfctvUserId) == 0);
                    //check if iteuser exists
                    if (iteUser != null)
                    {
                        int packageId = ContextHelper.GetPackageFromProductId((int)ITEPackageType.LITEPACKAGE);
                        var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(tfctvUserId));
                        var packageEntitlements = user.PackageEntitlements;
                        var iteEntitlement = packageEntitlements.FirstOrDefault(p => p.PackageId == packageId);
                        //check user has free entitlement
                        if (iteEntitlement != null)
                        {
                            var errResp = PaymentHelper.PayViaWalletWithEndDate(context, user.UserId, (int)ITEPackageType.LITEPACKAGE, SubscriptionProductType.Package, user.UserId, null, subscriptionEndDate, String.Format("RENEW IT&E ({0})", user.ITEDetail.ITEId));
                            if (errResp.Code == (int)ErrorCodes.Success)
                            {
                                iteRespErr = ITEResponseError.SUCCESS;
                                iteResp.StatusCode = (int)ITEResponseError.SUCCESS;
                                //iteResp.Message = String.Format(MyUtility.getITEError(ITEResponseError.SUCCESS), ITEPackageType.LITEPACKAGE.ToString(), iteEntitlement.EndDate.ToShortDateString());
                                iteResp.Message = String.Format("{0},{1},{2},{3},{4}", iteResp.StatusCode, iteRespErr.ToString(), ITEPackageType.LITEPACKAGE.ToString(), iteEntitlement.EndDate.ToShortDateString(), user.UserId.ToString());
                                return this.Content(iteResp.Message, "plain/text");
                                //return this.Json(iteResp, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                            iteRespErr = ITEResponseError.ACCOUNTANI_NOT_FOUND;
                    }
                    else
                        iteRespErr = ITEResponseError.ACCOUNTANI_NOT_FOUND;
                }
                else
                    iteRespErr = ITEResponseError.MISSING_PARAMETERS;

                iteResp.StatusCode = (int)iteRespErr;
                //iteResp.Message = MyUtility.getITEError(iteRespErr);
                iteResp.Message = String.Format("{0},{1},{2},{3},{4}", iteResp.StatusCode, iteRespErr.ToString(), String.Empty, String.Empty, String.Empty);
            }
            catch (Exception e) { MyUtility.LogException(e); iteRespErr = ITEResponseError.UNKNOWN_ERROR; }
            //return this.Json(iteResp, JsonRequestBehavior.AllowGet);
            return this.Content(iteResp.Message, "plain/text");
        }

        //[RequireHttps]
        [HttpPost]
        public ActionResult iteGetUser(FormCollection fc) //int idType, string iteId
        //public ActionResult iteGetUser(int idType, string iteId) //int idType, string iteId
        {
            ITEResponseError iteRespErr = ITEResponseError.UNKNOWN_ERROR;
            var iteResp = new ITEResponse()
            {
                StatusCode = (int)ITEResponseError.UNKNOWN_ERROR,
                Message = MyUtility.getITEError(iteRespErr)
            };
            try
            {
                try
                {
                    var requestingIp = Request.GetUserHostAddressFromCloudflare();
                    var whiteListedIp = GlobalConfig.ITEWhitelistedIp.Split(',');
                    if (!whiteListedIp.Contains(requestingIp))
                    {
                        iteRespErr = ITEResponseError.IP_NOT_ALLOWED;
                        iteResp.Message = String.Format("{0},{1},{2},{3},{4},{5}", (int)iteRespErr, iteRespErr.ToString(), String.Empty, String.Empty, String.Empty, String.Empty);
                        return this.Content(iteResp.Message, "plain/text");
                    }
                }
                catch (Exception) { }

                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;
                foreach (var x in tmpCollection)
                {
                    if (String.IsNullOrEmpty(x.Value))
                    {
                        isMissingRequiredFields = true;
                        break;
                    }
                }

                if (!isMissingRequiredFields) // process form
                {
                    int idType = Convert.ToInt32(fc["idType"]);
                    string iteId = fc["iteId"];

                    IPTV2Entities context = new IPTV2Entities();
                    ITEDetail iteUser = context.ITEDetails.FirstOrDefault(i => i.ITEId == iteId);
                    //check if iteuser exists
                    if (iteUser != null)
                    {
                        int packageId = ContextHelper.GetPackageFromProductId((int)ITEPackageType.LITEPACKAGE);
                        var user = context.Users.FirstOrDefault(u => u.UserId == iteUser.UserId);
                        var packageEntitlements = user.PackageEntitlements;
                        var iteEntitlement = packageEntitlements.FirstOrDefault(p => p.PackageId == packageId);
                        //check user has free entitlement
                        if (iteEntitlement != null)
                        {
                            iteRespErr = ITEResponseError.SUCCESS;
                            iteResp.StatusCode = (int)ITEResponseError.SUCCESS;
                            //iteResp.Message = String.Format(MyUtility.getITEError(ITEResponseError.SUCCESS), ITEPackageType.LITEPACKAGE.ToString(), iteEntitlement.EndDate.ToShortDateString());
                            iteResp.Message = String.Format("{0},{1},{2},{3},{4},{5}", iteResp.StatusCode, iteRespErr.ToString(), ITEPackageType.LITEPACKAGE.ToString(), iteEntitlement.EndDate.ToShortDateString(), user.UserId.ToString(), user.EMail);
                            iteResp.tfctvUserId = iteUser.UserId.ToString();
                            iteResp.packageType = ITEPackageType.LITEPACKAGE.ToString();
                            iteResp.startDate = (DateTime)iteEntitlement.LatestEntitlementRequest.StartDate;
                            iteResp.expirationDate = iteEntitlement.EndDate;
                            return this.Content(iteResp.Message, "plain/text");
                            //return this.Json(iteResp, JsonRequestBehavior.AllowGet);
                        }
                        else
                            iteRespErr = ITEResponseError.ACCOUNTANI_NOT_FOUND;
                    }
                    else
                        iteRespErr = ITEResponseError.ACCOUNTANI_NOT_FOUND;
                }
                else
                    iteRespErr = ITEResponseError.MISSING_PARAMETERS;

                iteResp.StatusCode = (int)iteRespErr;
                iteResp.Message = String.Format("{0},{1},{2},{3},{4},{5}", iteResp.StatusCode, iteRespErr.ToString(), String.Empty, String.Empty, String.Empty, String.Empty);
            }
            catch (Exception e) { MyUtility.LogException(e); iteRespErr = ITEResponseError.UNKNOWN_ERROR; }
            return this.Content(iteResp.Message, "plain/text");
        }


        //[RequireHttps]
        [HttpPost]
        public ActionResult iteUnlinkAccount(FormCollection fc) //int idType, string iteId, string tfctvUserId
        {
            ITEResponseError iteRespErr = ITEResponseError.UNKNOWN_ERROR;
            var iteResp = new ITEResponse()
            {
                StatusCode = (int)ITEResponseError.UNKNOWN_ERROR,
                Message = MyUtility.getITEError(iteRespErr)
            };

            try
            {
                try
                {
                    var requestingIp = Request.GetUserHostAddressFromCloudflare();
                    var whiteListedIp = GlobalConfig.ITEWhitelistedIp.Split(',');
                    if (!whiteListedIp.Contains(requestingIp))
                    {
                        iteRespErr = ITEResponseError.IP_NOT_ALLOWED;
                        iteResp.Message = String.Format("{0},{1},{2},{3},{4}", (int)iteRespErr, iteRespErr.ToString(), String.Empty, String.Empty, String.Empty);
                        return this.Content(iteResp.Message, "plain/text");
                    }
                }
                catch (Exception) { }

                Dictionary<string, string> tmpCollection = fc.AllKeys.ToDictionary(k => k, v => fc[v]);
                bool isMissingRequiredFields = false;
                foreach (var x in tmpCollection)
                {
                    if (String.IsNullOrEmpty(x.Value))
                    {
                        isMissingRequiredFields = true;
                        break;
                    }
                }

                if (!isMissingRequiredFields) // process form
                {
                    int idType = Convert.ToInt32(fc["idType"]);
                    string iteId = fc["iteId"];
                    string tfctvUserId = fc["tfctvUserId"];

                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    var user = context.Users.FirstOrDefault(u => u.UserId == new Guid(tfctvUserId));
                    if (user != null)
                    {
                        var iteDetail = user.ITEDetail;
                        if (String.Compare(iteDetail.ITEId, iteId, true) == 0)
                        {
                            //Create Cancellation Transaction
                            CancellationTransaction cancellation = new CancellationTransaction()
                            {
                                CancellationRemarks = "Unlink IT&E account",
                                GomsTransactionId = -1000,
                                GomsTransactionDate = registDt,
                                OriginalTransactionId = 0,
                                StatusId = 1,
                                OfferingId = GlobalConfig.offeringId,
                                Date = registDt,
                                Amount = 0,
                                Currency = user.Country.CurrencyCode,
                                Reference = String.Format("UNLINK IT&E ({0})", user.ITEDetail.ITEId)
                            };

                            user.Transactions.Add(cancellation);
                            context.ITEDetails.Remove(iteDetail);

                            if (context.SaveChanges() > 0)
                            {
                                iteRespErr = ITEResponseError.SUCCESS;
                                iteResp.StatusCode = (int)iteRespErr;
                                iteResp.Message = "You have unlink your IT&E account from TFC.tv";
                                iteResp.Message = String.Format("{0},{1},{2},{3},{4}", iteResp.StatusCode, iteRespErr.ToString(), String.Empty, String.Empty, String.Empty);
                                return this.Content(iteResp.Message, "plain/text");
                            }
                            else
                                iteRespErr = ITEResponseError.UNABLE_TO_COMMIT;
                        }
                        else
                            iteRespErr = ITEResponseError.ITEID_DO_NOT_MATCH;
                    }
                    else
                        iteRespErr = ITEResponseError.USER_NOT_FOUND;
                }
                else
                    iteRespErr = ITEResponseError.MISSING_PARAMETERS;

                iteResp.StatusCode = (int)iteRespErr;
                //iteResp.Message = MyUtility.getITEError(iteRespErr);
                iteResp.Message = String.Format("{0},{1},{2},{3}", iteResp.StatusCode, iteRespErr.ToString(), String.Empty, String.Empty);
            }
            catch (Exception e) { MyUtility.LogException(e); iteRespErr = ITEResponseError.UNKNOWN_ERROR; }
            //return this.Json(iteResp, JsonRequestBehavior.AllowGet);
            return this.Content(iteResp.Message, "plain/text");
        }
    }
}
