using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFCTV.Helpers;
using IPTV2_Model;
using System.Collections;
using System.Globalization;
using System.Web.Security;
using GOMS_TFCtv;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class LoadController : Controller
    {
        //
        // GET: /Load/

        [RequireHttps]
        public ActionResult PrepaidCard()
        {
            return RedirectToAction("Index", "Load");
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            if (!GlobalConfig.IsPpcReloadModeEnabled)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.PREPAID_CARD_RELOAD_IS_DISABLED;
                return View();
            }

            DateTime registDt = DateTime.Now;
            ViewBag.HasError = false;
            var context = new IPTV2Entities();
            var UserId = new Guid(User.Identity.Name);

            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var userWallet = user.UserWallets.FirstOrDefault(u => u.Currency == user.Country.CurrencyCode && u.IsActive == true);
            if (userWallet == null)
                return RedirectToAction("Index", "Home");

            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            if (user.HasPendingGomsChangeCountryTransaction(offering))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.PENDING_GOMS_CHANGE_COUNTRY;
            }

            if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED;
            }

            ViewBag.User = user;
            ViewBag.UserWallet = userWallet;
            return View();
        }

        [RequireHttps]
        public ActionResult PayPal()
        {
            return RedirectToAction("Index", "Load");
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            if (!GlobalConfig.IsPaypalReloadModeEnabled)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.PAYPAL_RELOAD_IS_DISABLED;
                return View();
            }

            DateTime registDt = DateTime.Now;
            ViewBag.HasError = false;
            var context = new IPTV2Entities();
            var UserId = new Guid(User.Identity.Name);

            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var userWallet = user.UserWallets.FirstOrDefault(u => u.Currency == user.Country.CurrencyCode && u.IsActive == true);
            if (userWallet == null)
                return RedirectToAction("Index", "Home");

            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            if (user.HasPendingGomsChangeCountryTransaction(offering))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.PENDING_GOMS_CHANGE_COUNTRY;
            }

            if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED;
            }

            ViewBag.User = user;
            ViewBag.UserWallet = userWallet;
            return View();
        }

        [RequireHttps]
        public ActionResult CreditCard()
        {
            return RedirectToAction("Index", "Load");
            ArrayList list = new ArrayList();
            for (int i = 0; i < 12; i++)
            {
                SelectListItem item = new SelectListItem()
                {
                    Value = (i + 1).ToString(),
                    Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(i + 1)
                };

                list.Add(item);
            }
            ViewBag.Months = list;

            list = new ArrayList();
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i <= currentYear + 20; i++)
            {
                SelectListItem item = new SelectListItem()
                {
                    Value = i.ToString(),
                    Text = i.ToString()
                };

                list.Add(item);
            }
            ViewBag.Years = list;

            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            if (!GlobalConfig.IsCreditCardReloadModeEnabled)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.CREDIT_CARD_RELOAD_IS_DISABLED;
                return View();
            }

            DateTime registDt = DateTime.Now;
            ViewBag.HasError = false;
            var context = new IPTV2Entities();
            var UserId = new Guid(User.Identity.Name);
            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var userWallet = user.UserWallets.FirstOrDefault(u => u.Currency == user.Country.CurrencyCode && u.IsActive == true);
            if (userWallet == null)
                return RedirectToAction("Index", "Home");

            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            if (user.HasPendingGomsChangeCountryTransaction(offering))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.PENDING_GOMS_CHANGE_COUNTRY;
            }

            if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED;
            }

            //Get Credit Card List
            var ccTypes = user.Country.GetGomsCreditCardTypes();
            if (ccTypes == null)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.CREDIT_CARD_IS_NOT_AVAILABLE_IN_YOUR_AREA;
                return View();
            }

            List<TFCTV.Helpers.CreditCard> clist = new List<TFCTV.Helpers.CreditCard>();
            foreach (var item in ccTypes)
                clist.Add(new TFCTV.Helpers.CreditCard() { value = ((int)item).ToString(), text = item.ToString().Replace('_', ' ') });
            ViewBag.CreditCardList = clist;
            ViewBag.User = user;
            ViewBag.UserWallet = userWallet;

            return View();
        }

        public PartialViewResult Balance()
        {
            ViewBag.HasError = false;
            int NumberOfSubscriptions = 0;
            bool IsPayPalSupported = false;
            bool IsMopaySupported = false;

            if (!MyUtility.isUserLoggedIn())
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.USER_IS_NOT_AUTHENTICATED;
            }
            try
            {
                DateTime registDt = DateTime.Now;
                var context = new IPTV2Entities();
                var UserId = new Guid(User.Identity.Name);

                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                if (user == null)
                {
                    ViewBag.HasError = true;
                    ViewBag.ErrorEncountered = ReloadError.USER_DOES_NOT_EXIST;
                }

                var userWallet = user.UserWallets.FirstOrDefault(u => u.Currency == user.Country.CurrencyCode && u.IsActive == true);
                if (userWallet == null)
                {
                    ViewBag.HasError = true;
                    ViewBag.ErrorEncountered = ReloadError.USER_WALLET_DOES_NOT_EXIST;
                }

                NumberOfSubscriptions = user.Entitlements.Count(e => e.EndDate > registDt && e.OfferingId == GlobalConfig.offeringId);
                ViewBag.NumberOfSubscriptions = NumberOfSubscriptions;

                if (GlobalConfig.IsMECreditCardEnabled)
                {
                    try
                    {
                        if (user.Country.GomsSubsidiaryId != GlobalConfig.MiddleEastGomsSubsidiaryId)
                            ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                        else
                        {
                            //get list of middle east countries allowed for credit card payment
                            var listOfMiddleEastAllowedCountries = GlobalConfig.MECountriesAllowedForCreditCard.Split(',');
                            if (listOfMiddleEastAllowedCountries.Contains(user.Country.Code))
                                ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                            else
                                ViewBag.CreditCardList = null;
                        }
                    }
                    catch (Exception) { ViewBag.CreditCardList = null; }
                }
                else
                    ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                //Check for Paypal  Currency Support
                IsPayPalSupported = user.Country.Currency.IsPayPalSupported;
                try
                {
                    IsMopaySupported = user.Country.MopayButtons.Count(m => m.StatusId == GlobalConfig.Visible) > 0 ? true : false;
                }
                catch (Exception) { }
                ViewBag.IsMopaySupported = IsMopaySupported;
                ViewBag.IsPayPalSupported = IsPayPalSupported;

                return PartialView(userWallet);
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.UNSPECIFIED_ERROR;
                ViewBag.ErrorMessage = e.Message;
            }
            return PartialView();
        }

        [RequireHttps]
        public ActionResult SmartPit()
        {
            return RedirectToAction("Index", "Load");
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            if (!GlobalConfig.IsSmartPitReloadEnabled)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.SMARTPIT_RELOAD_IS_DISABLED;
                return View();
            }

            DateTime registDt = DateTime.Now;
            ViewBag.HasError = false;
            var context = new IPTV2Entities();
            var UserId = new Guid(User.Identity.Name);

            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var userWallet = user.UserWallets.FirstOrDefault(u => u.Currency == user.Country.CurrencyCode && u.IsActive == true);
            if (userWallet == null)
                return RedirectToAction("Index", "Home");

            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            if (user.HasPendingGomsChangeCountryTransaction(offering))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.PENDING_GOMS_CHANGE_COUNTRY;
            }

            if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED;
            }

            if (!String.IsNullOrEmpty(user.SmartPitId))
                ViewBag.SmartPitCardNumber = user.SmartPitId;

            return View();
        }

        public ActionResult ReloadSelection(string pselection)
        {
            bool IsPayPalSupported = false;
            bool IsMopaySupported = false;
            string UserCountryCode = GlobalConfig.DefaultCountry;
            try
            {
                if (MyUtility.isUserLoggedIn())
                {
                    var context = new IPTV2Entities();
                    var UserId = new Guid(User.Identity.Name);
                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                    if (user != null)
                    {
                        IsPayPalSupported = user.Country.Currency.IsPayPalSupported;
                        UserCountryCode = user.CountryCode;
                        try
                        {
                            IsMopaySupported = user.Country.MopayButtons.Count(m => m.StatusId == GlobalConfig.Visible) > 0 ? true : false;
                        }
                        catch (Exception) { }
                    }
                    //Get CreditCard List
                    if (GlobalConfig.IsMECreditCardEnabled)
                    {
                        try
                        {
                            if (user.Country.GomsSubsidiaryId != GlobalConfig.MiddleEastGomsSubsidiaryId)
                                ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                            else
                            {
                                //get list of middle east countries allowed for credit card payment
                                var listOfMiddleEastAllowedCountries = GlobalConfig.MECountriesAllowedForCreditCard.Split(',');
                                if (listOfMiddleEastAllowedCountries.Contains(user.Country.Code))
                                    ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                                else
                                    ViewBag.CreditCardList = null;
                            }
                        }
                        catch (Exception) { ViewBag.CreditCardList = null; }
                    }
                    else
                        ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            ViewBag.Selection = pselection;
            ViewBag.UserCountryCode = UserCountryCode;
            ViewBag.IsPayPalSupported = IsPayPalSupported;
            ViewBag.IsMopaySupported = IsMopaySupported;
            return PartialView();
        }

        public ActionResult ErrorEncountered(ReloadError error)
        {
            string message = String.Empty;
            switch (error)
            {
                case ReloadError.CREDIT_CARD_RELOAD_IS_DISABLED:
                    message = "Credit card payment is currenty disabled."; break;
                case ReloadError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED:
                    message = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day.", GlobalConfig.reloadTransactionMaximumThreshold); break;
                case ReloadError.PAYPAL_RELOAD_IS_DISABLED:
                    message = "Paypal payment is currently disabled."; break;
                case ReloadError.PENDING_GOMS_CHANGE_COUNTRY:
                    message = "We are still processing your recent change in location. Please try again later."; break;
                case ReloadError.PREPAID_CARD_RELOAD_IS_DISABLED:
                    message = "Prepaid Card/ePIN payment is currently disabled."; break;
                case ReloadError.USER_DOES_NOT_EXIST:
                    message = "User does not exist."; break;
                case ReloadError.USER_IS_NOT_AUTHENTICATED:
                    message = "You are currently not authenticated. Please sign in again."; break;
                case ReloadError.USER_WALLET_DOES_NOT_EXIST:
                    message = "It seems we could not locate your wallet. Please contact Customer Support."; break;
                case ReloadError.CREDIT_CARD_IS_NOT_AVAILABLE_IN_YOUR_AREA:
                    message = "Credit card payment is not available in your country."; break;
                case ReloadError.SMARTPIT_RELOAD_IS_DISABLED:
                    message = "SmartPit payment is currently disabled."; break;
                default:
                    message = "The system encountered an unspecified error. Please contact Customer Support."; break;
            }
            var ReturnCode = new TransactionReturnType() { StatusMessage = message };
            return PartialView(ReturnCode);
        }

        [RequireHttp]
        public ActionResult Confirmation(string id)
        {
            if (!Request.IsLocal)
                if (String.Compare(id, "callback", true) != 0)
                    if (TempData["ReloadMode"] == null)
                        return RedirectToAction("Index", "Home");

            ViewBag.ReloadMode = TempData["ReloadMode"] != null ? (string)TempData["ReloadMode"] : "";

            if (String.Compare(id, "callback", true) == 0)
                ViewBag.ReloadMode = "PayPal";

            if (MyUtility.isUserLoggedIn())
            {
                var context = new IPTV2Entities();
                var UserId = new Guid(User.Identity.Name);
                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                if (user != null)
                {
                    var transaction = user.Transactions.LastOrDefault(t => t is ReloadTransaction);
                    if (transaction != null)
                    {
                        var reloadTransaction = (ReloadTransaction)transaction;
                        ViewBag.UserWallet = reloadTransaction.UserWallet;
                        ViewBag.Transaction = reloadTransaction;
                    }
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _PrepaidCard(FormCollection fc)
        {
            Response.ContentType = "application/json";
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Your request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (!GlobalConfig.IsPpcReloadModeEnabled)
                {
                    ReturnCode.StatusCode = (int)ReloadError.PREPAID_CARD_RELOAD_IS_DISABLED;
                    ReturnCode.StatusMessage = "Prepaid Card/ePIN reloading is currently disabled.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (User.Identity.IsAuthenticated)
                {
                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                    Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                    if (user != null)
                    {
                        if (offering != null)
                        {
                            if (user.HasPendingGomsChangeCountryTransaction(offering))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.HasPendingChangeCountryTransaction;
                                ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }

                            if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.MaximumTransactionsExceeded;
                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                        }

                        string serial = fc["serialnumber"];
                        string pin = fc["pin"];
                        serial = serial.Replace(" ", "");
                        pin = pin.Replace(" ", "");
                        if (!String.IsNullOrEmpty(pin))
                            if (!Request.IsLocal)
                                pin = MyUtility.GetSHA1(pin);

                        Ppc.ErrorCode StatusCode = ReloadHelper.ReloadViaPrepaidCard(context, userId, serial, pin);
                        string ErrorMessage = String.Empty;
                        switch (StatusCode)
                        {
                            case Ppc.ErrorCode.Success:
                                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode) && w.IsActive == true);
                                var balance = String.Format("{0} {1}", wallet.Currency, wallet.Balance.ToString("F"));
                                ErrorMessage = String.Format("Your new wallet balance is {0}", balance);
                                TempData["ReloadMode"] = "PREPAIDCARD";
                                ReturnCode.HtmlUri = "/Load/Confirmation";
                                break;
                            default:
                                ErrorMessage = MyUtility.GetPpcErrorMessages(StatusCode);
                                break;
                        }
                        ReturnCode.StatusCode = (int)StatusCode;
                        ReturnCode.StatusMessage = ErrorMessage;
                    }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.NotAuthenticated;
                    ReturnCode.StatusMessage = "Your session has expired. Please login again.";
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                ReturnCode.StatusMessage = e.Message;
            }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _CreditCard(FormCollection fc)
        {
            Response.ContentType = "application/json";
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Your request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (!GlobalConfig.IsCreditCardReloadModeEnabled)
                {
                    ReturnCode.StatusCode = (int)ReloadError.CREDIT_CARD_RELOAD_IS_DISABLED;
                    ReturnCode.StatusMessage = "Credit Card reloading is currently disabled.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (User.Identity.IsAuthenticated)
                {
                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                    Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                    if (user != null)
                    {
                        if (offering != null)
                        {
                            if (user.HasPendingGomsChangeCountryTransaction(offering))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.HasPendingChangeCountryTransaction;
                                ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }

                            if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.MaximumTransactionsExceeded;
                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                        }

                        decimal amount = Convert.ToDecimal(fc["pamount"]);
                        int cctype = Convert.ToInt32(fc["CreditCard"]);
                        string name = fc["cardholdername"];
                        string cardnumber = fc["cardnumber"];
                        string securitycode = fc["securitycode"];
                        string expirymonth = fc["ExpiryMonth"];
                        string expiryyear = fc["ExpiryYear"];
                        string address = fc["address"];
                        string city = fc["city"];
                        string zip = fc["zip"];

                        CreditCardInfo info = new CreditCardInfo()
                        {
                            CardType = (IPTV2_Model.CreditCardType)cctype,
                            Name = name,
                            Number = cardnumber,
                            CardSecurityCode = securitycode,
                            ExpiryMonth = Convert.ToInt32(expirymonth),
                            ExpiryYear = Convert.ToInt32(expiryyear),
                            PostalCode = zip,
                            StreetAddress = String.Format("{0}{1}", address, String.IsNullOrEmpty(city) ? "" : (", " + city))
                        };

                        ErrorResponse response = ReloadHelper.ReloadViaCreditCard2(context, userId, info, amount);
                        string ErrorMessage = String.Empty;
                        switch (response.Code)
                        {
                            case (int)ErrorCodes.IsProductIdInvalidPpc: ErrorMessage = "Please use proper card/ePIN for this product."; break;
                            case (int)ErrorCodes.IsReloadPpc: ErrorMessage = "Card/ePIN is invalid. Type is for reloading wallet."; break;
                            case (int)ErrorCodes.IsSubscriptionPpc: ErrorMessage = "Card/ePIN is invalid. Type is for subscription."; break;
                            case (int)ErrorCodes.IsInvalidCombinationPpc: ErrorMessage = "Invalid serial/pin combination."; break;
                            case (int)ErrorCodes.IsExpiredPpc: ErrorMessage = "Prepaid Card/ePIN is expired."; break;
                            case (int)ErrorCodes.IsInvalidPpc: ErrorMessage = "Serial does not exist."; break;
                            case (int)ErrorCodes.IsUsedPpc: ErrorMessage = "Prepaid Card/ePIN is already used."; break;
                            case (int)ErrorCodes.IsNotValidInCountryPpc: ErrorMessage = "Prepaid Card/ePIN not valid in your country."; break;
                            case (int)ErrorCodes.CreditCardHasExpired: ErrorMessage = "Your credit card has already expired."; break;
                            case (int)ErrorCodes.IsNotValidAmountPpc: ErrorMessage = "Prepaid Card/ePIN credits not enough to buy this product."; break;
                            case (int)ErrorCodes.Success:
                                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode) && w.IsActive == true);
                                var balance = String.Format("{0} {1}", wallet.Currency, wallet.Balance.ToString("F"));
                                ErrorMessage = String.Format("Your new wallet balance is {0}", balance);
                                TempData["ReloadMode"] = "CREDITCARD";
                                ReturnCode.HtmlUri = "/Load/Confirmation";
                                break;
                            default:
                                ErrorMessage = response.Message; break;
                        }
                        ReturnCode.StatusCode = response.Code;
                        ReturnCode.StatusMessage = ErrorMessage;
                    }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.NotAuthenticated;
                    ReturnCode.StatusMessage = "Your session has expired. Please login again.";
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                ReturnCode.StatusMessage = e.Message;
            }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PayPalHandler()
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };

            try
            {
                if (!GlobalConfig.IsPaypalReloadModeEnabled)
                {
                    ReturnCode.StatusCode = (int)ReloadError.PREPAID_CARD_RELOAD_IS_DISABLED;
                    ReturnCode.StatusMessage = "Prepaid Card/ePIN reloading is currently disabled.";
                    throw new Exception(ReturnCode.StatusMessage);
                }

                if (User.Identity.IsAuthenticated)
                {
                    string PDTpostData = "";

                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                    Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                    if (user != null)
                    {
                        if (offering != null)
                        {
                            if (user.HasPendingGomsChangeCountryTransaction(offering))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.HasPendingChangeCountryTransaction;
                                ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                throw new Exception(ReturnCode.StatusMessage);
                            }

                            if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.MaximumTransactionsExceeded;
                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                throw new Exception(ReturnCode.StatusMessage);
                            }
                        }

                        string tx = Request["tx"];
                        string st = Request["st"];
                        string amt = Request["amt"];
                        string cc = Request["cc"];
                        ErrorCodes StatusCode = ErrorCodes.UnknownError;
                        string ErrorMessage = String.Empty;
                        PDTpostData = ReloadHelper.PDTHandler(tx);
                        if (String.IsNullOrEmpty(PDTpostData))
                            return RedirectToAction("Index", "Home");

                        Custom custom = PDTHolder.ParseForReload(PDTpostData);
                        if (GlobalConfig.IsPDTEnabled)
                            StatusCode = ReloadHelper.ReloadViaPayPal(context, userId, custom.Amount, custom.TransactionId);
                        else
                            StatusCode = ErrorCodes.Success;

                        switch (StatusCode)
                        {
                            case ErrorCodes.IsProcessedPayPalTransaction:
                            case ErrorCodes.Success:
                                ErrorMessage = String.Format("Your Transaction ID is {0}", tx);
                                TempData["ReloadMode"] = "PAYPAL";
                                break;
                            default: ErrorMessage = "An unspecified error occurred. Please try again later."; break;
                        }
                        ReturnCode.StatusCode = (int)StatusCode;
                        ReturnCode.StatusMessage = ErrorMessage;
                        if (StatusCode == ErrorCodes.Success)
                            return View();
                        else
                            throw new Exception(ReturnCode.StatusMessage);
                    }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.NotAuthenticated;
                    ReturnCode.StatusMessage = "Your session has expired. Please login again.";
                    throw new Exception(ReturnCode.StatusMessage);
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                ReturnCode.StatusMessage = e.Message;
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _EnrollSmartPit(FormCollection fc)
        {
            Response.ContentType = "application/json";
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError)
            };
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
                    ReturnCode.StatusMessage = "Your request is invalid.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (!GlobalConfig.IsSmartPitReloadEnabled)
                {
                    ReturnCode.StatusCode = (int)ReloadError.SMARTPIT_RELOAD_IS_DISABLED;
                    ReturnCode.StatusMessage = "SmartPit reloading is currently disabled.";
                    return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                }

                if (User.Identity.IsAuthenticated)
                {
                    var registDt = DateTime.Now;
                    var context = new IPTV2Entities();
                    System.Guid userId = new System.Guid(User.Identity.Name);
                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                    Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                    if (user != null)
                    {
                        if (offering != null)
                        {
                            if (user.HasPendingGomsChangeCountryTransaction(offering))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.HasPendingChangeCountryTransaction;
                                ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }

                            if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.MaximumTransactionsExceeded;
                                ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                                return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (String.Compare(user.CountryCode, GlobalConfig.JapanCountryCode, true) != 0)
                        {
                            ReturnCode.StatusCode = (int)ErrorCodes.UnauthorizedCountry;
                            ReturnCode.StatusMessage = "You are not allowed to use this feature.";
                            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                        }

                        if (!String.IsNullOrEmpty(user.SmartPitId))
                        {
                            ReturnCode.StatusCode = (int)ErrorCodes.IsAlreadyEnrolledToSmartPit;
                            ReturnCode.StatusMessage = "You already have an enrolled SmartPit Card Number.";
                            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
                        }


                        var SmartPitCardNumber = String.IsNullOrEmpty(fc["SmartPitCardNumber"]) ? String.Empty : fc["SmartPitCardNumber"];
                        string ErrorMessage = String.Empty;
                        var service = new GomsTfcTv();
                        try
                        {
                            var response = service.EnrollSmartPit(context, user.UserId, SmartPitCardNumber);

                            if (response.IsSuccess)
                            {
                                user.SmartPitId = response.SmartPitCardNo;
                                user.SmartPitRegistrationDate = registDt;
                                if (context.SaveChanges() > 0)
                                {
                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                    ReturnCode.StatusMessage = String.Format("You have successfully {0} a SmartPit Card Number.", String.IsNullOrEmpty(SmartPitCardNumber) ? "generated" : "enrolled");
                                    ReturnCode.info = response.SmartPitCardNo;
                                }
                                else
                                {
                                    ReturnCode.StatusCode = (int)ErrorCodes.EntityUpdateError;
                                    ReturnCode.StatusMessage = "The system encountered an unspecified error. Please contact Customer Support.";
                                }
                            }
                            else
                            {
                                ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                                ReturnCode.StatusMessage = response.StatusMessage;
                            }
                        }
                        catch (Exception e)
                        {
                            MyUtility.LogException(e);
                            ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                            ReturnCode.StatusMessage = e.Message;
                        }
                    }
                }
                else
                {
                    ReturnCode.StatusCode = (int)ErrorCodes.NotAuthenticated;
                    ReturnCode.StatusMessage = "Your session has expired. Please login again.";
                }
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                ReturnCode.StatusMessage = e.Message;
            }
            return Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }

        [RequireHttps]
        public ActionResult Mopay()
        {
            return RedirectToAction("Index", "Load");
            if (!MyUtility.isUserLoggedIn())
                return RedirectToAction("Index", "Home");

            if (!GlobalConfig.IsMopayReloadModeEnabled)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.MOPAY_RELOAD_IS_DISABLED;
                return View();
            }

            DateTime registDt = DateTime.Now;
            ViewBag.HasError = false;
            var context = new IPTV2Entities();
            var UserId = new Guid(User.Identity.Name);

            var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var userWallet = user.UserWallets.FirstOrDefault(u => u.Currency == user.Country.CurrencyCode && u.IsActive == true);
            if (userWallet == null)
                return RedirectToAction("Index", "Home");

            var offering = context.Offerings.Find(GlobalConfig.offeringId);
            if (user.HasPendingGomsChangeCountryTransaction(offering))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.PENDING_GOMS_CHANGE_COUNTRY;
            }

            if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorEncountered = ReloadError.MAXIMUM_TRANSACTION_THRESHOLD_REACHED;
            }

            ViewBag.MopayButtonIds = user.Country.MopayButtons.Where(m => m.StatusId == GlobalConfig.Visible).OrderBy(m => m.Amount).ToList();
            ViewBag.UserWallet = userWallet;
            ViewBag.Userid = User.Identity.Name;
            return View();
        }

        [RequireHttps]
        public ActionResult Index()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home");

                var context = new IPTV2Entities();
                var UserId = new Guid(User.Identity.Name);
                var CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                if (user != null)
                    CountryCode = user.CountryCode;

                //Get User's Wallet
                var wallet = user.UserWallets.FirstOrDefault(u => u.IsActive && String.Compare(u.Currency, user.Country.CurrencyCode, true) == 0);
                if (wallet == null)
                    return RedirectToAction("Index", "Home");
                else
                    ViewBag.UserWallet = wallet;

                //Credit card
                try
                {
                    ArrayList list = new ArrayList();
                    for (int i = 0; i < 12; i++)
                    {
                        SelectListItem item = new SelectListItem()
                        {
                            Value = (i + 1).ToString(),
                            Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(i + 1)
                        };

                        list.Add(item);
                    }
                    ViewBag.Months = list;

                    list = new ArrayList();
                    int currentYear = DateTime.Now.Year;
                    for (int i = currentYear; i <= currentYear + 20; i++)
                    {
                        SelectListItem item = new SelectListItem()
                        {
                            Value = i.ToString(),
                            Text = i.ToString()
                        };

                        list.Add(item);
                    }
                    ViewBag.Years = list;
                    ViewBag.CreditCardAvailability = true;
                    var ccTypes = user.Country.GetGomsCreditCardTypes();

                    if (user.Country.GomsSubsidiaryId != GlobalConfig.MiddleEastGomsSubsidiaryId)
                        ccTypes = user.Country.GetGomsCreditCardTypes();
                    else
                    {
                        var listOfMiddleEastAllowedCountries = GlobalConfig.MECountriesAllowedForCreditCard.Split(',');
                        if (listOfMiddleEastAllowedCountries.Contains(user.Country.Code))
                            ccTypes = user.Country.GetGomsCreditCardTypes();
                        else
                            ccTypes = null;
                    }


                    if (ccTypes == null)
                        ViewBag.CreditCardAvailability = false;
                    else
                    {
                        List<TFCTV.Helpers.CreditCard> clist = new List<TFCTV.Helpers.CreditCard>();
                        foreach (var item in ccTypes)
                            clist.Add(new TFCTV.Helpers.CreditCard() { value = ((int)item).ToString(), text = item.ToString().Replace('_', ' ') });
                        ViewBag.CreditCardList = clist;
                    }
                }
                catch (Exception) { }


                //Mopay
                try
                {
                    if (user.Country.MopayButtons.Count(m => m.StatusId == GlobalConfig.Visible) > 0)
                    {
                        var buttonIds = user.Country.MopayButtons.Where(m => m.StatusId == GlobalConfig.Visible).OrderBy(m => m.Amount);
                        if (buttonIds != null)
                            ViewBag.MopayButtonIds = buttonIds.ToList();
                    }
                }
                catch (Exception) { }

                //SmartPit
                ViewBag.IsSmartPitAvailable = false;
                try
                {
                    if (String.Compare(user.CountryCode, GlobalConfig.JapanCountryCode, true) == 0)
                        ViewBag.IsSmartPitAvailable = true;
                    if (!String.IsNullOrEmpty(user.SmartPitId))
                        ViewBag.SmartPitCardNumber = user.SmartPitId;

                }
                catch (Exception) { }

                ViewBag.IsPayPalSupported = false;
                try
                {
                    ViewBag.IsPayPalSupported = user.Country.Currency.IsPayPalSupported;
                }
                catch (Exception) { }

            }
            catch (Exception e) { MyUtility.LogException(e); }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _LoadViaCreditCard(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "CreditCard",
                TransactionType = "Load"
            };

            string url = Url.Action("Index", "Load").ToString();
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
                    if (GlobalConfig.IsCreditCardReloadModeEnabled)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            var context = new IPTV2Entities();
                            System.Guid userId = new System.Guid(User.Identity.Name);
                            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                            if (user != null)
                            {
                                var offering = context.Offerings.Find(GlobalConfig.offeringId);
                                if (offering != null)
                                {
                                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                                        ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                    else if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                                        ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.reloadTransactionMaximumThreshold);
                                    else
                                    {
                                        decimal amount = Convert.ToDecimal(fc["camount"]);
                                        int cctype = Convert.ToInt32(fc["CreditCard"]);
                                        string name = fc["cardholdername"];
                                        string cardnumber = fc["cardnumber"];
                                        string securitycode = fc["securitycode"];
                                        string expirymonth = fc["ExpiryMonth"];
                                        string expiryyear = fc["ExpiryYear"];
                                        string address = fc["address"];
                                        string city = fc["city"];
                                        string zip = fc["zip"];

                                        CreditCardInfo info = new CreditCardInfo()
                                        {
                                            CardType = (CreditCardType)cctype,
                                            Name = name,
                                            Number = cardnumber,
                                            CardSecurityCode = securitycode,
                                            ExpiryMonth = Convert.ToInt32(expirymonth),
                                            ExpiryYear = Convert.ToInt32(expiryyear),
                                            PostalCode = zip,
                                            StreetAddress = String.Format("{0}{1}", address, String.IsNullOrEmpty(city) ? "" : (", " + city))
                                        };

                                        ErrorResponse response = ReloadHelper.ReloadViaCreditCard2(context, userId, info, amount);
                                        string ErrorMessage = String.Empty;
                                        switch (response.Code)
                                        {
                                            case (int)ErrorCodes.IsProductIdInvalidPpc: ErrorMessage = "Please use proper card/ePIN for this product."; break;
                                            case (int)ErrorCodes.IsReloadPpc: ErrorMessage = "Card/ePIN is invalid. Type is for reloading wallet."; break;
                                            case (int)ErrorCodes.IsSubscriptionPpc: ErrorMessage = "Card/ePIN is invalid. Type is for subscription."; break;
                                            case (int)ErrorCodes.IsInvalidCombinationPpc: ErrorMessage = "Invalid serial/pin combination."; break;
                                            case (int)ErrorCodes.IsExpiredPpc: ErrorMessage = "Prepaid Card/ePIN is expired."; break;
                                            case (int)ErrorCodes.IsInvalidPpc: ErrorMessage = "Serial does not exist."; break;
                                            case (int)ErrorCodes.IsUsedPpc: ErrorMessage = "Prepaid Card/ePIN is already used."; break;
                                            case (int)ErrorCodes.IsNotValidInCountryPpc: ErrorMessage = "Prepaid Card/ePIN is not valid in your country."; break;
                                            case (int)ErrorCodes.CreditCardHasExpired: ErrorMessage = "Your credit card has already expired."; break;
                                            case (int)ErrorCodes.IsNotValidAmountPpc: ErrorMessage = "Prepaid Card/ePIN credits not enough to buy this product."; break;
                                            case (int)ErrorCodes.Success:
                                                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode) && w.IsActive);
                                                string balance = String.Format("Your new wallet balance is {0} {1}.", wallet.Currency, wallet.Balance.ToString("F"));
                                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                ReturnCode.StatusHeader = "You have successfully purchased credits!";
                                                ReturnCode.StatusMessage = String.Format("Congratulations! You have reloaded your E-Wallet. {0}", balance);
                                                ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies.<br>Visit your Free Trial page to see what's available and start watching!";
                                                TempData["ErrorMessage"] = ReturnCode;
                                                return RedirectToAction("Index", "Home"); // successful reload                                                
                                            default:
                                                ErrorMessage = response.Message; break;
                                        }
                                        ReturnCode.StatusMessage = ErrorMessage;
                                    }
                                }
                                else
                                    ReturnCode.StatusMessage = "Service not found. Please contact support.";

                            }
                            else
                                ReturnCode.StatusMessage = "User does not exist.";
                        }
                        else
                            ReturnCode.StatusMessage = "Your session has already expired. Please login again.";
                    }
                    else
                        ReturnCode.StatusMessage = "Prepaid Card/ePIN payment is currenty disabled.";
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                TempData["ErrorMessage"] = ReturnCode;
                url = Request.UrlReferrer.AbsolutePath;
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusMessage = e.Message;
                TempData["ErrorMessage"] = ReturnCode;
            }
            return Redirect(url);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHttps]
        public ActionResult _LoadViaPrepaidCard(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "PrepaidCard",
                TransactionType = "Load"
            };

            string url = Url.Action("Index", "Load").ToString();
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
                    if (GlobalConfig.IsPpcReloadModeEnabled)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            var context = new IPTV2Entities();
                            System.Guid userId = new System.Guid(User.Identity.Name);
                            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                            if (user != null)
                            {
                                var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                                if (offering != null)
                                {
                                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                                        ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                    else if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                                        ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.reloadTransactionMaximumThreshold);
                                    else
                                    {
                                        string serial = fc["serialnumber"];
                                        string pin = fc["pin"];
                                        serial = serial.Replace(" ", "");
                                        pin = pin.Replace(" ", "");
                                        if (!String.IsNullOrEmpty(pin))
                                            if (!Request.IsLocal)
                                                pin = MyUtility.GetSHA1(pin);

                                        Ppc.ErrorCode StatusCode = ReloadHelper.ReloadViaPrepaidCard(context, userId, serial, pin);
                                        string ErrorMessage = String.Empty;
                                        switch (StatusCode)
                                        {
                                            case Ppc.ErrorCode.Success:
                                                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode) && w.IsActive);
                                                string balance = String.Format("Your new wallet balance is {0} {1}.", wallet.Currency, wallet.Balance.ToString("F"));
                                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                ReturnCode.StatusHeader = "You have successfully purchased credits!";
                                                ReturnCode.StatusMessage = String.Format("Congratulations! You have reloaded your E-Wallet. {0}", balance);
                                                ReturnCode.StatusMessage2 = "Pwede ka nang manood ng mga piling Kapamilya shows at movies.<br>Visit your Free Trial page to see what's available and start watching!";
                                                TempData["ErrorMessage"] = ReturnCode;
                                                return RedirectToAction("Index", "Home"); // successful reload
                                            default:
                                                ErrorMessage = MyUtility.GetPpcErrorMessages(StatusCode);
                                                break;
                                        }
                                        ReturnCode.StatusCode = (int)StatusCode;
                                        ReturnCode.StatusMessage = ErrorMessage;
                                    }
                                }
                                else
                                    ReturnCode.StatusMessage = "Service not found. Please contact support.";

                            }
                            else
                                ReturnCode.StatusMessage = "User does not exist.";
                        }
                        else
                            ReturnCode.StatusMessage = "Your session has already expired. Please login again.";
                    }
                    else
                        ReturnCode.StatusMessage = "Prepaid Card/ePIN payment is currenty disabled.";
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                TempData["ErrorMessage"] = ReturnCode;
                url = Request.UrlReferrer.AbsolutePath;
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusMessage = e.Message;
                TempData["ErrorMessage"] = ReturnCode;
            }
            return Redirect(url);
        }

        public ActionResult ConfirmPaypalPayment()
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "Paypal"
            };
            string url = Url.Action("Index", "Home").ToString();
            try
            {
                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                ReturnCode.StatusHeader = "You have successfully purchased credits!";
                ReturnCode.StatusMessage = "Congratulations! You have reloaded your E-Wallet.";
                ReturnCode.StatusMessage2 = "Please check your My Transaction page for payment information.";
                TempData["ErrorMessage"] = ReturnCode;
                return View();
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return Redirect(url);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[RequireHttps]
        public ActionResult _EnrollSmartPitCardNumber(FormCollection fc)
        {
            var ReturnCode = new TransactionReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty,
                info = "SmartPit",
                TransactionType = "Load"
            };

            string url = Url.Action("Index", "Load").ToString();
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
                    if (GlobalConfig.IsSmartPitReloadEnabled)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            var context = new IPTV2Entities();
                            System.Guid userId = new System.Guid(User.Identity.Name);
                            User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                            if (user != null)
                            {
                                var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                                if (offering != null)
                                {
                                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                                        ReturnCode.StatusMessage = "We are processing your recent change in location. Please try again after a few minutes.";
                                    else if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                                        ReturnCode.StatusMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.reloadTransactionMaximumThreshold);
                                    else if (String.Compare(user.CountryCode, GlobalConfig.JapanCountryCode, true) != 0)
                                        ReturnCode.StatusMessage = "This payment mode is not available in your country.";
                                    else if (!String.IsNullOrEmpty(user.SmartPitId))
                                        ReturnCode.StatusMessage = "You already have enrolled a SmartPit Card Number.";
                                    else
                                    {
                                        var SmartPitCardNumber = String.IsNullOrEmpty(fc["SmartPitCardNumber"]) ? String.Empty : fc["SmartPitCardNumber"];
                                        var service = new GomsTfcTv();
                                        var response = service.EnrollSmartPit(context, user.UserId, SmartPitCardNumber);
                                        if (response.IsSuccess)
                                        {
                                            user.SmartPitId = response.SmartPitCardNo;
                                            user.SmartPitRegistrationDate = registDt;
                                            if (context.SaveChanges() > 0)
                                            {
                                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                ReturnCode.StatusHeader = "You have successfully enrolled!";
                                                ReturnCode.StatusMessage = String.Format("You have successfully {0} SmartPit Card Number {1}.", String.IsNullOrEmpty(SmartPitCardNumber) ? "generated" : "enrolled", response.SmartPitCardNo);
                                                ReturnCode.StatusMessage2 = "You can now start reloading your E-Wallet using your SmartPit Card Number.";
                                                TempData["ErrorMessage"] = ReturnCode;
                                                return RedirectToAction("Index", "Home"); // successful reload
                                            }
                                            else
                                                ReturnCode.StatusMessage = "The system encountered an unspecified error. Please contact Customer Support.";

                                        }
                                        else
                                        {
                                            ReturnCode.StatusCode = (int)ErrorCodes.UnknownError;
                                            ReturnCode.StatusMessage = response.StatusMessage;
                                        }

                                    }
                                }
                                else
                                    ReturnCode.StatusMessage = "Service not found. Please contact support.";

                            }
                            else
                                ReturnCode.StatusMessage = "User does not exist.";
                        }
                        else
                            ReturnCode.StatusMessage = "Your session has already expired. Please login again.";
                    }
                    else
                        ReturnCode.StatusMessage = "Prepaid Card/ePIN payment is currenty disabled.";
                }
                else
                    ReturnCode.StatusMessage = "Please fill in all required fields.";

                TempData["ErrorMessage"] = ReturnCode;
                url = Request.UrlReferrer.AbsolutePath;
            }
            catch (Exception e)
            {
                MyUtility.LogException(e);
                ReturnCode.StatusMessage = e.Message;
                TempData["ErrorMessage"] = ReturnCode;
            }
            return Redirect(url);
        }
    }
}
