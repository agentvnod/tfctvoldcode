using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using IPTV2_Model;
using TFCTV.Helpers;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class PpcController : Controller
    {
        //
        // GET: /Ppc/

        [RequireHttps]
        public ActionResult Index()
        {
            if (!Request.Cookies.AllKeys.Contains("version"))
                return View("Index2");
            return View();
        }

        [HttpPost]
        public ActionResult Activate(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            Ppc.ErrorCode errorCode = Ppc.ErrorCode.UnknownError;
            string errorMessage = MyUtility.getPpcError(errorCode);
            collection.Add("errorCode", (int)errorCode);
            collection.Add("errorMessage", errorMessage);
            DateTime registDt = DateTime.Now;

            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                if (user != null)
                {
                    Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                    {
                        errorMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                        collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }

                    string serial = fc["serialnumber"];
                    string pin = fc["pin"];
                    serial = serial.Replace(" ", "");
                    pin = pin.Replace(" ", "");
                    if (!String.IsNullOrEmpty(pin))
                        pin = MyUtility.GetSHA1(pin);

                    //Check if Card if Reload or Prepaid
                    Ppc ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serial);

                    if (ppc == null) //Serial does not exist
                        errorCode = Ppc.ErrorCode.InactivePpc;
                    else if (!(ppc.SerialNumber == serial && ppc.Pin == pin)) //Invalid serial/pin combination
                        errorCode = Ppc.ErrorCode.InvalidSerialPinCombination;
                    else if (ppc.UserId != null) // Ppc has already been used
                        errorCode = Ppc.ErrorCode.PpcAlreadyUsed;
                    else if (registDt > ppc.ExpirationDate) // Ppc is expired
                        errorCode = Ppc.ErrorCode.IsExpiredPpc;

                    if (ppc is ReloadPpc)
                    {
                        if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                        {
                            errorMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                            collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                        errorCode = ReloadHelper.ReloadViaPrepaidCard(context, userId, serial, pin);
                    }
                    else if (ppc is SubscriptionPpc)
                    {
                        if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                        {
                            errorMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                            collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }

                        SubscriptionPpc sPpc = (SubscriptionPpc)ppc;
                        Product product = context.Products.FirstOrDefault(p => p.ProductId == sPpc.ProductId);
                        if (product == null)
                        {
                            //return RedirectToAction("Index", "Home");
                            errorMessage = "This product does not exist.";
                            collection = MyUtility.setError(ErrorCodes.ProductIsNull, errorMessage);
                            return Content(MyUtility.buildJson(collection), "application/json");
                        }
                        if (sPpc.IsRegular)
                        {
                            if (!product.IsForSale)
                            {
                                errorMessage = "This product is currently not for sale.";
                                collection = MyUtility.setError(ErrorCodes.ProductIsNotPurchaseable, errorMessage);
                                return Content(MyUtility.buildJson(collection), "application/json");
                            }
                        }
                        SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                        ErrorResponse response = PaymentHelper.PayViaPrepaidCard(context, userId, sPpc.ProductId, subscriptionType, serial, pin, userId, null);
                        errorCode = (Ppc.ErrorCode)response.Code;
                    }

                    switch (errorCode)
                    {
                        case Ppc.ErrorCode.Success:
                            if (ppc is ReloadPpc)
                            {
                                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                                string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                                string NewWalletBalance = "";
                                if (currency.IsLeft)
                                    NewWalletBalance = String.Format("{0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
                                else
                                    NewWalletBalance = String.Format("{0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
                                ViewBag.newBalance = NewWalletBalance;

                                errorMessage = "Congratulations! You have now topped up your wallet. New wallet balance is " + NewWalletBalance;
                            }
                            else if (ppc is SubscriptionPpc)
                            {
                                errorMessage = "Congratulations! You have successfully subscribed to our service.";
                            }
                            break;
                        default: errorMessage = MyUtility.getPpcError(errorCode); break;
                    }
                    collection["errorCode"] = (int)errorCode;
                    collection["errorMessage"] = errorMessage;
                }
            }
            else
            {
                collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
                collection["errorMessage"] = "Please login first.";
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        //[HttpPost]
        //public ActionResult Activate(FormCollection fc)
        //{
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    ErrorCodes errorCode = ErrorCodes.UnknownError;
        //    string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
        //    collection.Add("errorCode", (int)errorCode);
        //    collection.Add("errorMessage", errorMessage);
        //    DateTime registDt = DateTime.Now;

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var context = new IPTV2Entities();
        //        System.Guid userId = new System.Guid(User.Identity.Name);
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);

        //        if (user != null)
        //        {
        //            string serial = fc["serialnumber"];
        //            string pin = fc["pin"];
        //            serial = serial.Replace(" ", "");
        //            pin = pin.Replace(" ", "");
        //            if (!String.IsNullOrEmpty(pin))
        //                pin = MyUtility.GetSHA1(pin);

        //            //Check if Card if Reload or Prepaid
        //            Ppc ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serial);

        //            Ppc.ErrorCode error = Ppc.Validate(context, serial, pin, user.Country);
        //            if (error == Ppc.ErrorCode.Success)
        //            {
        //                if (ppc is ReloadPpc)
        //                    errorCode = ReloadHelper.ReloadViaPrepaidCard(context, userId, serial, pin);
        //                else if (ppc is SubscriptionPpc)
        //                {
        //                    SubscriptionPpc sPpc = (SubscriptionPpc)ppc;
        //                    Product product = context.Products.FirstOrDefault(p => p.ProductId == sPpc.ProductId);
        //                    if (product == null)
        //                    {
        //                        return RedirectToAction("Index", "Home");
        //                    }
        //                    SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
        //                    errorCode = PaymentHelper.PayViaPrepaidCard(context, userId, sPpc.ProductId, subscriptionType, serial, pin);
        //                }
        //            }
        //            else
        //            {
        //                // Get error
        //            }

        //            switch (errorCode)
        //            {
        //                case ErrorCodes.IsProductIdInvalidPpc: errorMessage = "Please use proper card for this product."; break;
        //                case ErrorCodes.IsReloadPpc: errorMessage = "Card is invalid. Type is for reloading wallet."; break;
        //                case ErrorCodes.IsSubscriptionPpc: errorMessage = "Card is invalid. Type is for subscription."; break;
        //                case ErrorCodes.IsInvalidCombinationPpc: errorMessage = "Invalid serial/pin combination."; break;
        //                case ErrorCodes.IsExpiredPpc: errorMessage = "Prepaid card is expired."; break;
        //                case ErrorCodes.IsInvalidPpc: errorMessage = "Serial does not exist."; break;
        //                case ErrorCodes.IsUsedPpc: errorMessage = "Ppc is already used."; break;
        //                case ErrorCodes.IsNotValidInCountryPpc: errorMessage = "Ppc not valid in your country."; break;
        //                case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
        //                case ErrorCodes.Success:

        //                    if (ppc is ReloadPpc)
        //                    {
        //                        UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //                        string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
        //                        Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
        //                        string NewWalletBalance = "";
        //                        if (currency.IsLeft)
        //                            NewWalletBalance = String.Format("{0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
        //                        else
        //                            NewWalletBalance = String.Format("{0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
        //                        ViewBag.newBalance = NewWalletBalance;

        //                        errorMessage = "Congratulations! You have now topped up your wallet. New wallet balance is " + NewWalletBalance;
        //                    }
        //                    else if (ppc is SubscriptionPpc)
        //                    {
        //                        errorMessage = "Congratulations! You have now bought this product.";
        //                    }

        //                    break;
        //                default: errorMessage = "Unknown error."; break;
        //            }
        //            collection["errorCode"] = (int)errorCode;
        //            collection["errorMessage"] = errorMessage;
        //        }
        //    }
        //    else
        //    {
        //        collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
        //        collection.Add("errorMessage", "Your session has expired. Please login again.");
        //    }
        //    return Content(MyUtility.buildJson(collection), "application/json");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult ActivatePpc(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            string url = Url.Action("Index", "Ppc").ToString();
            try
            {
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
                if (!isMissingRequiredFields) // process form
                {
                    string serial = fc["serial"];
                    string pin = fc["pin"];
                    //remove whitespaces
                    serial = serial.Replace(" ", String.Empty);
                    pin = pin.Replace(" ", String.Empty);
                    if (!Request.IsLocal)
                        pin = MyUtility.GetSHA1(pin); // encrypt

                    if (User.Identity.IsAuthenticated)
                    {
                        Ppc.ErrorCode code;
                        var context = new IPTV2Entities();
                        System.Guid userId = new System.Guid(User.Identity.Name);
                        User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                            if (user.HasPendingGomsChangeCountryTransaction(offering))
                                ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                            else
                            {
                                var ppc = context.Ppcs.FirstOrDefault(p => String.Compare(p.SerialNumber, serial, true) == 0);
                                if (ppc != null)
                                {
                                    if (!(String.Compare(ppc.SerialNumber, serial, true) == 0 && String.Compare(ppc.Pin, pin, false) == 0))
                                        ReturnCode.StatusMessage = "Invalid serial/pin combination.";
                                    else if (ppc.ExpirationDate < registDt)
                                        ReturnCode.StatusMessage = "Prepaid Card/ePIN is already expired.";
                                    else if (ppc.UserId != null)
                                        ReturnCode.StatusMessage = "Prepaid Card/ePIN is already used.";
                                    else
                                    {
                                        if (ppc is ReloadPpc)
                                        {
                                            if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                            else // process reloadPpc                                            
                                            {
                                                code = ReloadHelper.ReloadViaPrepaidCard(context, userId, serial, pin);
                                                if (code == Ppc.ErrorCode.Success)
                                                {
                                                    var wallet = user.UserWallets.FirstOrDefault(w => String.Compare(w.Currency, user.Country.CurrencyCode, true) == 0);
                                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                    ReturnCode.StatusMessage = String.Format("You have successfully topped up your wallet. Your wallet balance is {0} {1}.", wallet.Currency, wallet.Balance);
                                                }
                                                else
                                                    ReturnCode.StatusMessage = MyUtility.GetPpcErrorMessages(code);
                                            }

                                        }
                                        else if (ppc is SubscriptionPpc)
                                        {
                                            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                            else
                                            {
                                                SubscriptionPpc sPpc = (SubscriptionPpc)ppc;
                                                Product product = context.Products.FirstOrDefault(p => p.ProductId == sPpc.ProductId);
                                                if (product == null)
                                                    ReturnCode.StatusMessage = "This product does not exist.";
                                                else
                                                {
                                                    //if (sPpc.IsRegular)
                                                    //    if (!product.IsForSale)
                                                    //    {
                                                    //        ReturnCode.StatusMessage = "This product is currently not for sale.";
                                                    //        TempData["ErrorMessage"] = ReturnCode;
                                                    //        url = Request.UrlReferrer.AbsolutePath;
                                                    //        return Redirect(url);
                                                    // }
                                                    SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);
                                                    ErrorResponse response = PaymentHelper.PayViaPrepaidCard(context, userId, sPpc.ProductId, subscriptionType, serial, pin, userId, null);
                                                    code = (Ppc.ErrorCode)response.Code;
                                                    if (code == Ppc.ErrorCode.Success)
                                                    {
                                                        ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                        ReturnCode.StatusHeader = "Activate Prepaid Card/ePIN Complete!";
                                                        ReturnCode.StatusMessage = String.Format("You have successfully subscribed to {0}.", product.Description);
                                                        ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies.<br>Visit your Free Trial page to see what's available and start watching!";
                                                        TempData["ErrorMessage"] = ReturnCode;
                                                        return RedirectToAction("Index", "Home"); //successful activation of ppc
                                                    }
                                                    else
                                                        ReturnCode.StatusMessage = MyUtility.GetPpcErrorMessages(code);
                                                }
                                            }
                                        }
                                        else
                                            ReturnCode.StatusMessage = "Unable to retrieve this type of Prepaid Card/ePIN.";
                                    }
                                }
                                else
                                    ReturnCode.StatusMessage = "Prepaid Card/ePIN does not exist.";
                            }
                        }
                        else
                            ReturnCode.StatusMessage = "User does not exist.";
                    }
                    else
                        ReturnCode.StatusMessage = "Your session has already expired. Please login again.";
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                TempData["ErrorMessage"] = ReturnCode;
                url = Request.UrlReferrer.AbsolutePath;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Redirect(url);
        }
    }
}