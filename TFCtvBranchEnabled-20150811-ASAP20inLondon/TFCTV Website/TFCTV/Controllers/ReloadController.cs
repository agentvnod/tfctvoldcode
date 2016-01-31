using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DevTrends.MvcDonutCaching;
using GOMS_TFCtv;
using IPTV2_Model;
using TFCTV.Helpers;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class ReloadController : Controller
    {
        //
        // GET: /Reload/
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Process()
        {
            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                if (user != null)
                {
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var registDt = DateTime.Now;

                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                        return PartialView("PaymentTemplates/_BuyPendingChangeCountryErrorPartial");

                    if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                        return PartialView("PaymentTemplates/_BuyErrorExceededMaximumThresholdPartial");
                }

                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                if (currency.IsLeft)
                    ViewBag.currentBalance = String.Format("{0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
                else
                    ViewBag.currentBalance = String.Format("{0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
                ViewBag.currentBalance = String.Format("{0} {1}", currency.Code, wallet.Balance.ToString("F"));
                //ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                //UPDATED  05/07/2013
                if (GlobalConfig.IsMECreditCardEnabled)
                {
                    try
                    {
                        if (user.Country.GomsSubsidiaryId != GlobalConfig.MiddleEastGomsSubsidiaryId)
                            ViewBag.CreditCardList = user.Country.GetGomsCreditCardTypes();
                        else
                        {
                            var listOfMiddleEastAllowedCountries = GlobalConfig.MECountriesAllowedForCreditCard.Split(','); //get list of middle east countries allowed for credit card payment
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
                ViewBag.IsPayPalSupported = currency.IsPayPalSupported;

                return PartialView("_ReloadProcessPartial");
            }
            else
            {
                //return RedirectToAction("Index", "Home");                
                return PartialView("PaymentTemplates/_BuyAuthenticationErrorPartial");
            }
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                if (currency.IsLeft)
                    ViewBag.currentBalance = String.Format("{0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
                else
                    ViewBag.currentBalance = String.Format("{0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult PrepaidCard()
        {
            if (!GlobalConfig.IsPpcReloadModeEnabled)
                return PartialView("PaymentTemplates/_BuyErrorPartial");

            if (MyUtility.isUserLoggedIn())
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var registDt = DateTime.Now;

                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                        return PartialView("PaymentTemplates/_BuyPendingChangeCountryErrorPartial");

                    if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                        return PartialView("PaymentTemplates/_BuyErrorExceededMaximumThresholdPartial");
                }
            }

            return PartialView("_ReloadViaPrepaidCardPartial");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult CreditCard()
        {
            if (!GlobalConfig.IsCreditCardReloadModeEnabled)
                return PartialView("PaymentTemplates/_CreditCardDownTimePartial");
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

            var context = new IPTV2Entities();
            User user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(User.Identity.Name));

            if (user != null)
            {
                var offering = context.Offerings.Find(GlobalConfig.offeringId);
                var registDt = DateTime.Now;

                if (user.HasPendingGomsChangeCountryTransaction(offering))
                    return PartialView("PaymentTemplates/_BuyPendingChangeCountryErrorPartial");

                if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                    return PartialView("PaymentTemplates/_BuyErrorExceededMaximumThresholdPartial");
            }

            var ccTypes = user.Country.GetGomsCreditCardTypes();
            List<TFCTV.Helpers.CreditCard> clist = new List<TFCTV.Helpers.CreditCard>();
            foreach (var item in ccTypes)
                clist.Add(new TFCTV.Helpers.CreditCard() { value = ((int)item).ToString(), text = item.ToString().Replace('_', ' ') });
            ViewBag.CreditCardList = clist;
            ViewBag.Currency = user.Country.Currency.Code;

            return PartialView("_ReloadViaCreditCardPartial");
        }

        //[HttpPost]
        //public ContentResult ReloadViaCreditCard(FormCollection fc)
        //{
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    ErrorCodes errorCode = ErrorCodes.UnknownError;
        //    string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
        //    collection.Add("errorCode", (int)errorCode);
        //    collection.Add("errorMessage", errorMessage);

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var context = new IPTV2Entities();
        //        System.Guid userId = new System.Guid(User.Identity.Name);
        //        User user = context.Users.FirstOrDefault(u => u.UserId == userId);

        //        if (user != null)
        //        {
        //            decimal amount = Convert.ToDecimal(fc["amount"]);
        //            int cctype = Convert.ToInt32(fc["CreditCard"]);
        //            string name = fc["cardholdername"];
        //            string cardnumber = fc["cardnumber"];
        //            string securitycode = fc["securitycode"];
        //            string expirymonth = fc["ExpiryMonth"];
        //            string expiryyear = fc["ExpiryYear"];
        //            string address = fc["address"];
        //            string city = fc["city"];
        //            string zip = fc["zip"];

        //            CreditCardInfo info = new CreditCardInfo()
        //            {
        //                CardType = (IPTV2_Model.CreditCardType)cctype,
        //                Name = name,
        //                Number = cardnumber,
        //                CardSecurityCode = securitycode,
        //                ExpiryMonth = Convert.ToInt32(expirymonth),
        //                ExpiryYear = Convert.ToInt32(expiryyear),
        //                PostalCode = zip,
        //                StreetAddress = String.Format("{0}{1}", address, String.IsNullOrEmpty(city) ? "" : (", " + city))
        //            };

        //            errorCode = ReloadHelper.ReloadViaCreditCard(context, userId, info, amount);
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
        //                case ErrorCodes.CreditCardHasExpired: errorMessage = "Your credit card has already expired."; break;
        //                case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
        //                case ErrorCodes.Success:

        //                    UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
        //                    string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
        //                    Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
        //                    string NewWalletBalance = "";
        //                    if (currency.IsLeft)
        //                        NewWalletBalance = String.Format("{0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
        //                    else
        //                        NewWalletBalance = String.Format("{0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
        //                    ViewBag.newBalance = NewWalletBalance;

        //                    errorMessage = "Congratulations! You have now topped up your wallet. New wallet balance is " + NewWalletBalance;
        //                    break;
        //                default: errorMessage = "The system encounted an unidentified error. Please try again."; break;
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

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult PayPal()
        {
            if (!GlobalConfig.IsPaypalReloadModeEnabled)
                return PartialView("PaymentTemplates/_BuyErrorPartial");
            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                if (user != null)
                {
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var registDt = DateTime.Now;

                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                        return PartialView("PaymentTemplates/_BuyPendingChangeCountryErrorPartial");

                    if (user.HasExceededMaximumPaymentTransactionsForTheDay(GlobalConfig.paymentTransactionMaximumThreshold, registDt))
                        return PartialView("PaymentTemplates/_BuyErrorExceededMaximumThresholdPartial");
                }

                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                if (user != null)
                {
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    if (user.HasPendingGomsChangeCountryTransaction(offering))
                        return PartialView("PaymentTemplates/_BuyPendingChangeCountryErrorPartial");

                    string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                    Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                    if (!currency.IsPayPalSupported)
                        return PartialView("PaymentTemplates/_BuyErrorPartial");
                    ViewBag.isLeft = currency.IsLeft;
                    ViewBag.Currency = wallet.Currency;
                    ViewBag.CurrencySymbol = currency.Symbol;
                    ViewBag.User = user;
                    ViewBag.returnURL = GlobalConfig.PayPalReloadReturnUrl;
                    return PartialView("_ReloadViaPayPalPartial");
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ContentResult ReloadViaPrepaidCard(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            Ppc.ErrorCode errorCode = Ppc.ErrorCode.UnknownError;
            string errorMessage = MyUtility.getPpcError(errorCode);
            collection.Add("errorCode", (int)errorCode);
            collection.Add("errorMessage", errorMessage);

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

                    var registDt = DateTime.Now;
                    if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                    {
                        errorMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                        collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }

                    string serial = fc["serialnumber"];
                    string pin = fc["pin"];
                    serial = serial.Replace(" ", "");
                    pin = pin.Replace(" ", "");
                    if (!String.IsNullOrEmpty(pin))
                        pin = MyUtility.GetSHA1(pin);
                    errorCode = ReloadHelper.ReloadViaPrepaidCard(context, userId, serial, pin);
                    switch (errorCode)
                    {
                        case Ppc.ErrorCode.Success:
                            UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                            string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                            Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                            string NewWalletBalance = "";
                            if (currency.IsLeft)
                                NewWalletBalance = String.Format("{0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
                            else
                                NewWalletBalance = String.Format("{0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
                            NewWalletBalance = String.Format("{0} {1}", currency.Code, wallet.Balance.ToString("F"));
                            ViewBag.newBalance = NewWalletBalance;

                            errorMessage = "Congratulations! You have now topped up your wallet. New wallet balance is " + NewWalletBalance;
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
                collection.Add("errorMessage", "Your session has expired. Please login again.");
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        public ActionResult PayPalHandler()
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection.Add("errorCode", 1);
            collection.Add("errorMessage", errorMessage);

            string PDTpostData = "";

            if (User.Identity.IsAuthenticated)
            {
                var context = new IPTV2Entities();
                System.Guid userId = new System.Guid(User.Identity.Name);
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);

                string tx = Request["tx"];
                string st = Request["st"];
                string amt = Request["amt"];
                string cc = Request["cc"];

                //Call PDT of Paypal

                PDTpostData = ReloadHelper.PDTHandler(tx);
                if (String.IsNullOrEmpty(PDTpostData))
                    return RedirectToAction("Index", "Home");

                Custom custom = PDTHolder.ParseForReload(PDTpostData);

                if (user != null)
                {
                    /** Begin Commented out due to doubling of record ***/
                    //errorCode = ReloadHelper.ReloadViaPayPal(context, userId, custom.Amount, custom.TransactionId);
                    /** End Commented out due to doubling of record ***/
                    if (GlobalConfig.IsPDTEnabled)
                        errorCode = ReloadHelper.ReloadViaPayPal(context, userId, custom.Amount, custom.TransactionId);
                    else
                        errorCode = ErrorCodes.Success;

                    switch (errorCode)
                    {
                        case ErrorCodes.IsProcessedPayPalTransaction:
                            //errorMessage = "This transaction has already been processed.";
                            DisplayUserBalance(context, user);
                            errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", tx);
                            break;
                        case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                        case ErrorCodes.Success:
                            //UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                            //string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                            //Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                            //string NewWalletBalance = "";
                            //if (currency.IsLeft)
                            //    NewWalletBalance = String.Format("{0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
                            //else
                            //    NewWalletBalance = String.Format("{0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
                            //NewWalletBalance = String.Format("{0} {1}", currency.Code, wallet.Balance.ToString("F"));
                            //ViewBag.newBalance = NewWalletBalance;
                            DisplayUserBalance(context, user);
                            errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", tx); break;
                        default: errorMessage = "Unknown error."; break;
                    }
                    collection["errorCode"] = (int)errorCode;
                    collection["errorMessage"] = errorMessage;
                }
            }
            else
            {
                collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
                collection["errorMessage"] = "Your session has expired. Please login again.";
            }

            ViewBag.errorCode = collection["errorCode"];
            ViewBag.errorMessage = collection["errorMessage"];

            return PartialView("_PayPalReloadReturn");
        }

        private void DisplayUserBalance(IPTV2Entities context, User user)
        {
            UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
            string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
            Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
            string NewWalletBalance = "";
            if (currency.IsLeft)
                NewWalletBalance = String.Format("{0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
            else
                NewWalletBalance = String.Format("{0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
            NewWalletBalance = String.Format("{0} {1}", currency.Code, wallet.Balance.ToString("F"));
            ViewBag.newBalance = NewWalletBalance;
        }

        [HttpPost]
        public ContentResult ReloadViaCreditCard(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection.Add("errorCode", (int)errorCode);
            collection.Add("errorMessage", errorMessage);

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

                    var registDt = DateTime.Now;
                    if (user.HasExceededMaximumReloadTransactionsForTheDay(GlobalConfig.reloadTransactionMaximumThreshold, registDt))
                    {
                        errorMessage = String.Format("You have exceeded the maximum number of transactions ({0}) allowed per day. Please try again later.", GlobalConfig.paymentTransactionMaximumThreshold);
                        collection = MyUtility.setError(ErrorCodes.HasPendingChangeCountryTransaction, errorMessage);
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }

                    decimal amount = Convert.ToDecimal(fc["amount"]);
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
                    switch (response.Code)
                    {
                        case (int)ErrorCodes.IsProductIdInvalidPpc: errorMessage = "Please use proper card for this product."; break;
                        case (int)ErrorCodes.IsReloadPpc: errorMessage = "Card is invalid. Type is for reloading wallet."; break;
                        case (int)ErrorCodes.IsSubscriptionPpc: errorMessage = "Card is invalid. Type is for subscription."; break;
                        case (int)ErrorCodes.IsInvalidCombinationPpc: errorMessage = "Invalid serial/pin combination."; break;
                        case (int)ErrorCodes.IsExpiredPpc: errorMessage = "Prepaid card is expired."; break;
                        case (int)ErrorCodes.IsInvalidPpc: errorMessage = "Serial does not exist."; break;
                        case (int)ErrorCodes.IsUsedPpc: errorMessage = "Ppc is already used."; break;
                        case (int)ErrorCodes.IsNotValidInCountryPpc: errorMessage = "Ppc not valid in your country."; break;
                        case (int)ErrorCodes.CreditCardHasExpired: errorMessage = "Your credit card has already expired."; break;
                        case (int)ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                        case (int)ErrorCodes.Success:

                            UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                            string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                            Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                            string NewWalletBalance = "";
                            if (currency.IsLeft)
                                NewWalletBalance = String.Format("{0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
                            else
                                NewWalletBalance = String.Format("{0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
                            NewWalletBalance = String.Format("{0} {1}", currency.Code, wallet.Balance.ToString("F"));
                            ViewBag.newBalance = NewWalletBalance;

                            errorMessage = "Congratulations! You have now topped up your wallet. New wallet balance is " + NewWalletBalance;
                            break;
                        default: errorMessage = response.Message; break;
                    }
                    collection["errorCode"] = response.Code;
                    collection["errorMessage"] = errorMessage;
                }
            }
            else
            {
                collection["errorCode"] = (int)ErrorCodes.NotAuthenticated;
                collection.Add("errorMessage", "Your session has expired. Please login again.");
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        public ActionResult IPNListener(FormCollection fc)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(ErrorCodes.UnknownError);

            string PostData = ReloadHelper.IPNHandler(Request);
            switch (PostData)
            {
                case "VERIFIED":
                    try
                    {
                        var context = new IPTV2Entities();
                        //check the payment_status is Completed
                        string payment_status = fc["payment_status"];
                        if (String.Compare(payment_status, "Complete", true) == 0)
                        {
                            //check that txn_id has not been previously processed
                            string transactionId = fc["txn_id"];

                            bool doesExist = false;

                            if (GlobalConfig.UsePayPalIPNLog)
                                doesExist = context.PaypalIPNLogs.Count(t => String.Compare(t.UniqueTransactionId, transactionId, true) == 0) > 0;
                            else
                                doesExist = context.Transactions.Count(t => String.Compare(t.Reference, transactionId, true) == 0) > 0;

                            //var transaction = context.Transactions.FirstOrDefault(t => t.Reference == transactionId);
                            //if (transaction == null)
                            if (!doesExist)
                            {
                                //check that receiver_email is your Primary PayPal email
                                string receiver_email = fc["receiver_email"];
                                if (String.Compare(receiver_email, GlobalConfig.PayPalBusiness, true) == 0)
                                {
                                    //check that payment_amount/payment_currency are correct
                                    string cust = fc["custom"];
                                    var custom = cust.Split('&');
                                    Guid userId = Guid.NewGuid();
                                    decimal amount = 0;
                                    if (custom.Length > 1)
                                    {
                                        userId = new Guid(custom[1]);
                                        try
                                        {
                                            amount = Convert.ToDecimal(custom[0]);
                                        }
                                        catch (Exception) { amount = 0; }
                                    }
                                    //process payment
                                    User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                                    if (user != null)
                                    {
                                        string mc_currency = fc["mc_currency"];
                                        errorCode = ReloadHelper.ReloadViaPayPal(context, userId, amount, transactionId, mc_currency);
                                        switch (errorCode)
                                        {
                                            case ErrorCodes.IsProcessedPayPalTransaction: errorMessage = "This transaction has already been processed."; break;
                                            case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                                            case ErrorCodes.Success:
                                                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                                                string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                                                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                                                string NewWalletBalance = "";
                                                if (currency.IsLeft)
                                                    NewWalletBalance = String.Format("{0}{1}", currency.Symbol, wallet.Balance.ToString("F"));
                                                else
                                                    NewWalletBalance = String.Format("{0}{1}", wallet.Balance.ToString("F"), currency.Symbol);
                                                NewWalletBalance = String.Format("{0} {1}", currency.Code, wallet.Balance.ToString("F"));
                                                ViewBag.newBalance = NewWalletBalance;
                                                errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", transactionId); break;
                                            default: errorMessage = "Unknown error."; break;
                                        }
                                        collection["errorCode"] = (int)errorCode;
                                        collection["errorMessage"] = errorMessage;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception) { }

                    break;

                case "INVALID":
                    //log for manual investigation

                    break;
                default:
                    //log response/ipn data for manual investigation

                    break;
            }

            return new HttpStatusCodeResult(200);
        }
    }
}