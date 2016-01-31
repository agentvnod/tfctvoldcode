using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Gigya.Socialize.SDK;
using IPTV2_Model;
using Newtonsoft.Json.Linq;
using TFCTV.Helpers;
using System.Threading;

namespace TFCTV.Controllers
{
    public class PaypalController : Controller
    {
        //
        // GET: /Paypal/

        public ActionResult IPNListener(FormCollection fc)
        {
            if (Request.IsLocal)
                fc = (FormCollection)Request.QueryString;
            //string keyname;
            //string keyvalue;
            //string fileFullPath1 = @"E:\StagingSites\tfc.tv\App_Data\Paypal.txt";
            //TextWriter w1 = new StreamWriter(fileFullPath1, true);
            //for (int i = 0; i <= fc.Count - 1; i++)
            //{
            //    keyname = fc.AllKeys[i];
            //    keyvalue = fc[i];
            //    w1.WriteLine(i + ": " + keyname + ": " + keyvalue);
            //}
            //w1.WriteLine("This is the transaction ID" + fc["txn_id"]);
            //w1.WriteLine("This is the transaction ID from Forms list" + Request.Form["txn_id"]);
            //w1.Close();

            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);
            collection = MyUtility.setError(ErrorCodes.UnknownError);
            DateTime registDt = DateTime.Now;

            if (!Request.IsLocal)
                if (!IsValidIPNRequest(Request))
                    return new HttpStatusCodeResult(200);

            try
            {
                //Thread.Sleep(2000);              
                string txn_type = fc["txn_type"];
                if (String.Compare(txn_type, "subscr_cancel", true) == 0)
                {
                    string cancel_subscr_id = fc["subscr_id"];
                    if (PaymentHelper.CancelPaypalRecurringOnTFCtv(cancel_subscr_id))
                        return new HttpStatusCodeResult(200);
                    else
                        return new HttpStatusCodeResult(400);
                }

                var context = new IPTV2Entities();
                //check the payment_status is Completed
                string payment_status = fc["payment_status"];

                //FIX MISSING PAYMENT STATUS ON SUBSCRIBE_SIGNUP

                if (String.Compare(payment_status, "Completed", true) == 0 || String.Compare(payment_status, "Pending", true) == 0)
                {
                    //check that txn_id has not been previously processed
                    string transactionId = fc["txn_id"];
                    if (String.IsNullOrEmpty(transactionId))
                        return new HttpStatusCodeResult(200);

                    if (GlobalConfig.UsePayPalIPNLog)
                    {
                        if (context.PaypalIPNLogs.Count(t => String.Compare(t.UniqueTransactionId, transactionId, true) == 0) > 0)
                            return new HttpStatusCodeResult(200);
                    }
                    else
                    {
                        if (context.Transactions.Count(t => String.Compare(t.Reference, transactionId, true) == 0) > 0)
                            return new HttpStatusCodeResult(200);
                    }


                    //if (transaction == null)
                    //{
                    //check that receiver_email is your Primary PayPal email
                    string receiver_email = fc["receiver_email"];
                    string custom = String.Empty;
                    if (String.Compare(receiver_email, GlobalConfig.PayPalBusiness, true) == 0)
                    {
                        custom = HttpUtility.UrlDecode(fc["custom"]);
                        Guid userId;
                        User user = null;
                        string payer_email = String.Empty;
                        if (!String.IsNullOrEmpty(custom))
                        {
                            int productId = 0;
                            string wishlistId = String.Empty;
                            int cpId = 0;

                            var strCustomArray = custom.Split('&');
                            switch (strCustomArray.Length)
                            {
                                case 1: // Old Reload
                                    //payer_email = fc["payer_email"];
                                    //goto FIXRELOAD;
                                    break;

                                case 2: //Reload
                                    //FIXRELOAD:
                                    decimal Amount = Convert.ToDecimal(fc["mc_gross"]);
                                    string CurrencyCode = fc["mc_currency"];
                                    if (String.IsNullOrEmpty(payer_email))
                                    {
                                        userId = new Guid(strCustomArray[1]);
                                        user = context.Users.FirstOrDefault(u => u.UserId == userId);
                                    }
                                    else
                                    {
                                        user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, payer_email, true) == 0);
                                        userId = user.UserId;
                                    }

                                    if (user != null)
                                    {
                                        errorCode = ReloadHelper.ReloadViaPayPal(context, userId, Amount, transactionId, CurrencyCode);
                                        switch (errorCode)
                                        {
                                            case ErrorCodes.IsProcessedPayPalTransaction: errorMessage = "This transaction has already been processed."; break;
                                            case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                                            case ErrorCodes.Success: errorMessage = "SUCCESS"; break;
                                            default: errorMessage = "Unknown error."; break;
                                        }
                                        collection["errorCode"] = (int)errorCode;
                                        collection["errorMessage"] = errorMessage;
                                    }

                                    //string fileFullPath = @"E:\StagingSites\tfc.tv\App_Data\Paypal.txt";
                                    //TextWriter w = new StreamWriter(fileFullPath, true);
                                    //w.WriteLine(errorMessage);
                                    //w.Close();
                                    break;
                                case 4: // Old Buy
                                    //payer_email = fc["payer_email"];
                                    //goto FIXSUBSCRIPTION;
                                    break;
                                case 5: //Buy
                                    //FIXSUBSCRIPTION:
                                    productId = 0;
                                    wishlistId = String.Empty;
                                    cpId = 0;
                                    //string subscriptionType;

                                    if (!String.IsNullOrEmpty(strCustomArray[0]))
                                        productId = Convert.ToInt32(strCustomArray[0]);
                                    //if (!String.IsNullOrEmpty(strCustomArray[1]))
                                    //    subscriptionType = strCustomArray[1];
                                    if (!String.IsNullOrEmpty(strCustomArray[2]))
                                        wishlistId = strCustomArray[2];
                                    if (!String.IsNullOrEmpty(strCustomArray[3]))
                                        cpId = Convert.ToInt32(strCustomArray[3]);

                                    //if (!String.IsNullOrEmpty(strCustomArray[4]))
                                    if (String.IsNullOrEmpty(payer_email))
                                    {
                                        userId = new Guid(strCustomArray[4]);
                                        user = context.Users.FirstOrDefault(u => u.UserId == userId);
                                    }
                                    else
                                    {
                                        user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, payer_email, true) == 0);
                                        userId = user.UserId;
                                    }

                                    if (productId > 0 && userId != null)
                                    {
                                        Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                                        if (product == null)
                                        {
                                            // log error
                                        }

                                        string productName = product.Name;
                                        Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                        SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);

                                        User recipient = null;
                                        if (!String.IsNullOrEmpty(wishlistId))
                                        {
                                            GSArray gsarray = GigyaMethods.GetWishlistDetails(wishlistId);
                                            if (gsarray != null)
                                            {
                                                JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                                                System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                                                recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                                            }
                                        }

                                        string subscr_id = fc["subscr_id"];
                                        ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                                        errorCode = PaymentHelper.PayViaPayPal(context, userId, product.ProductId, subscriptionType, transactionId, recipient == null ? userId : recipient.UserId, cpId, !String.IsNullOrEmpty(subscr_id) ? true : false);
                                        switch (errorCode)
                                        {
                                            case ErrorCodes.IsProcessedPayPalTransaction:
                                                errorMessage = "This transaction has already been processed."; break;
                                            case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                                            case ErrorCodes.Success:
                                                {
                                                    //Project Black
                                                    PaymentHelper.logProjectBlackUserPromo(context, userId, productId);
                                                    //Xoom2
                                                    //PaymentHelper.logUserPromo(context, userId, GlobalConfig.Xoom2PromoId);
                                                    if (!String.IsNullOrEmpty(subscr_id))
                                                        PaymentHelper.AddToPaypalRecurringBilling(context, product, offering, user, registDt, subscr_id);

                                                    if (!String.IsNullOrEmpty(wishlistId))
                                                    {
                                                        GigyaMethods.DeleteWishlist(wishlistId); // Delete from Wishlist
                                                        SendGiftShareToSocialNetwork(product, recipient);
                                                        ReceiveGiftShareToSocialNetwork(product, recipient, user);
                                                    }
                                                    errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", transactionId); break;
                                                }
                                            default: errorMessage = "Unknown error."; break;
                                        }
                                        collection["errorCode"] = (int)errorCode;
                                        collection["errorMessage"] = errorMessage;
                                    }
                                    //string fileFullPath1 = @"E:\StagingSites\tfc.tv\App_Data\Paypal.txt";
                                    //TextWriter w1 = new StreamWriter(fileFullPath1, true);
                                    //w1.WriteLine(errorCode + ' ' + errorMessage);
                                    //w1.Close();
                                    break;
                                case 6:
                                    //recurring with trial period
                                    productId = 0;
                                    wishlistId = String.Empty;
                                    cpId = 0;
                                    int trialProductId = 0;
                                    //string subscriptionType;

                                    if (!String.IsNullOrEmpty(strCustomArray[0]))
                                        trialProductId = Convert.ToInt32(strCustomArray[0]);
                                    if (!String.IsNullOrEmpty(strCustomArray[5]))
                                        productId = Convert.ToInt32(strCustomArray[5]);

                                    //if (!String.IsNullOrEmpty(strCustomArray[1]))
                                    //    subscriptionType = strCustomArray[1];
                                    if (!String.IsNullOrEmpty(strCustomArray[2]))
                                        wishlistId = strCustomArray[2];
                                    if (!String.IsNullOrEmpty(strCustomArray[3]))
                                        cpId = Convert.ToInt32(strCustomArray[3]);

                                    //if (!String.IsNullOrEmpty(strCustomArray[4]))
                                    if (String.IsNullOrEmpty(payer_email))
                                    {
                                        userId = new Guid(strCustomArray[4]);
                                        user = context.Users.FirstOrDefault(u => u.UserId == userId);
                                    }
                                    else
                                    {
                                        user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, payer_email, true) == 0);
                                        userId = user.UserId;
                                    }

                                    //sign up
                                    //if (String.Compare(txn_type, "subscr_signup", true) == 0)
                                    //{
                                    //    string subscr_id = fc["subscr_id"];
                                    //    var regularProduct = context.Products.FirstOrDefault(p => p.ProductId == productId);
                                    //    if (regularProduct != null)
                                    //    {
                                    //        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                                    //        PaymentHelper.AddToPaypalRecurringBilling(context, regularProduct, offering, user, registDt, subscr_id);
                                    //    }

                                    //}
                                    //else if (String.Compare(txn_type, "subscr_payment", true) == 0)
                                    //string signup_amount = fc["mc_amount1"];
                                    //bool isSignupAmountADecimal = false;
                                    //decimal signup_amount_decimal = -1;
                                    //try
                                    //{
                                    //    isSignupAmountADecimal = decimal.TryParse(signup_amount, out signup_amount_decimal);
                                    //}
                                    //catch (Exception) { }
                                    if (String.Compare(txn_type, "subscr_payment", true) == 0 || (String.Compare(txn_type, "subscr_signup", true) == 0) || (String.Compare(txn_type, "subscr_payment", true) == 0))
                                    {
                                        string subscr_id = fc["subscr_id"];
                                        //check if subscr_id is in recurring already                                       
                                        var paypalRecurringBilling = user.RecurringBillings.FirstOrDefault(r => r is PaypalRecurringBilling && r.StatusId == GlobalConfig.Visible && String.Compare(((PaypalRecurringBilling)r).SubscriberId, subscr_id, true) == 0);
                                        if (paypalRecurringBilling != null) // EXISTING, GIVE REGULAR PRICE
                                        {
                                            //set the product to normal product (get product from recurring table)
                                            productId = paypalRecurringBilling.ProductId;

                                            if (productId > 0 && userId != null)
                                            {
                                                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                                                if (product != null)
                                                {
                                                    string productName = product.Name;
                                                    Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                                    SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);

                                                    User recipient = null;
                                                    if (!String.IsNullOrEmpty(wishlistId))
                                                    {
                                                        GSArray gsarray = GigyaMethods.GetWishlistDetails(wishlistId);
                                                        if (gsarray != null)
                                                        {
                                                            JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                                                            System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                                                            recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                                                        }
                                                    }

                                                    ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                                                    errorCode = PaymentHelper.PayViaPayPal(context, userId, product.ProductId, subscriptionType, transactionId, recipient == null ? userId : recipient.UserId, cpId, !String.IsNullOrEmpty(subscr_id) ? true : false);
                                                    switch (errorCode)
                                                    {
                                                        case ErrorCodes.IsProcessedPayPalTransaction:
                                                            errorMessage = "This transaction has already been processed."; break;
                                                        case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                                                        case ErrorCodes.Success:
                                                            {
                                                                //Project Black
                                                                PaymentHelper.logProjectBlackUserPromo(context, userId, trialProductId);
                                                                //Xoom2
                                                                //PaymentHelper.logUserPromo(context, userId, GlobalConfig.Xoom2PromoId);
                                                                if (!String.IsNullOrEmpty(subscr_id))
                                                                    PaymentHelper.AddToPaypalRecurringBilling(context, product, offering, user, registDt, subscr_id);
                                                                errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", transactionId); break;
                                                            }
                                                        default: errorMessage = "Unknown error."; break;
                                                    }
                                                    collection["errorCode"] = (int)errorCode;
                                                    collection["errorMessage"] = errorMessage;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (trialProductId > 0 && userId != null)
                                            {
                                                Product product = context.Products.FirstOrDefault(p => p.ProductId == trialProductId);
                                                if (product != null)
                                                {
                                                    string productName = product.Name;
                                                    Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                                    SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);

                                                    User recipient = null;
                                                    if (!String.IsNullOrEmpty(wishlistId))
                                                    {
                                                        GSArray gsarray = GigyaMethods.GetWishlistDetails(wishlistId);
                                                        if (gsarray != null)
                                                        {
                                                            JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                                                            System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                                                            recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                                                        }
                                                    }

                                                    ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                                                    errorCode = PaymentHelper.PayViaPayPal(context, userId, product.ProductId, subscriptionType, transactionId, recipient == null ? userId : recipient.UserId, cpId, !String.IsNullOrEmpty(subscr_id) ? true : false);
                                                    switch (errorCode)
                                                    {
                                                        case ErrorCodes.IsProcessedPayPalTransaction:
                                                            errorMessage = "This transaction has already been processed."; break;
                                                        case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                                                        case ErrorCodes.Success:
                                                            {
                                                                //Project Black
                                                                PaymentHelper.logProjectBlackUserPromo(context, userId, trialProductId);
                                                                //Xoom2
                                                                //PaymentHelper.logUserPromo(context, userId, GlobalConfig.Xoom2PromoId);
                                                                if (!String.IsNullOrEmpty(subscr_id))
                                                                {
                                                                    product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                                                                    if (product != null)
                                                                        PaymentHelper.AddToPaypalRecurringBilling(context, product, offering, user, registDt, subscr_id);
                                                                }

                                                                errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", transactionId); break;
                                                            }
                                                        default: errorMessage = "Unknown error."; break;
                                                    }
                                                    collection["errorCode"] = (int)errorCode;
                                                    collection["errorMessage"] = errorMessage;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 7:
                                    productId = 0;
                                    wishlistId = String.Empty;
                                    cpId = 0;
                                    int freeProductId = 0;
                                    //string subscriptionType;

                                    if (String.Compare(strCustomArray[6], "xoom", true) == 0)
                                    {
                                        if (!String.IsNullOrEmpty(strCustomArray[0]))
                                            productId = Convert.ToInt32(strCustomArray[0]);
                                        if (!String.IsNullOrEmpty(strCustomArray[5]))
                                            freeProductId = Convert.ToInt32(strCustomArray[5]);
                                        if (!String.IsNullOrEmpty(strCustomArray[2]))
                                            wishlistId = strCustomArray[2];
                                        if (!String.IsNullOrEmpty(strCustomArray[3]))
                                            cpId = Convert.ToInt32(strCustomArray[3]);

                                        if (String.IsNullOrEmpty(payer_email))
                                        {
                                            userId = new Guid(strCustomArray[4]);
                                            user = context.Users.FirstOrDefault(u => u.UserId == userId);
                                        }
                                        else
                                        {
                                            user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, payer_email, true) == 0);
                                            userId = user.UserId;
                                        }

                                        transactionId = fc["txn_id"] ?? fc["subscr_id"] ?? fc["ipn_track_id"];
                                        string subscr_id = fc["subscr_id"];
                                        //check if subscr_id is in recurring already                                       
                                        var paypalRecurringBilling = user.RecurringBillings.FirstOrDefault(r => r is PaypalRecurringBilling && r.StatusId == GlobalConfig.Visible && String.Compare(((PaypalRecurringBilling)r).SubscriberId, subscr_id, true) == 0);
                                        if (paypalRecurringBilling != null)
                                        {
                                            //set the product to normal product (get product from recurring table)
                                            productId = paypalRecurringBilling.ProductId;

                                            if (productId > 0 && userId != null)
                                            {
                                                Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                                                if (product != null)
                                                {
                                                    string productName = product.Name;
                                                    Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                                    SubscriptionProductType subscriptionType = ContextHelper.GetProductType(product);

                                                    User recipient = null;
                                                    if (!String.IsNullOrEmpty(wishlistId))
                                                    {
                                                        GSArray gsarray = GigyaMethods.GetWishlistDetails(wishlistId);
                                                        if (gsarray != null)
                                                        {
                                                            JObject o = JObject.Parse(gsarray.GetObject(0).ToJsonString());
                                                            System.Guid recipientUserId = new System.Guid(o["UID_s"].ToString());
                                                            recipient = context.Users.FirstOrDefault(u => u.UserId == recipientUserId);
                                                        }
                                                    }

                                                    ProductPrice priceOfProduct = product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                                                    errorCode = PaymentHelper.PayViaPayPal(context, userId, product.ProductId, subscriptionType, transactionId, recipient == null ? userId : recipient.UserId, cpId, !String.IsNullOrEmpty(subscr_id) ? true : false);
                                                    switch (errorCode)
                                                    {
                                                        case ErrorCodes.IsProcessedPayPalTransaction:
                                                            errorMessage = "This transaction has already been processed."; break;
                                                        case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                                                        case ErrorCodes.Success:
                                                            {
                                                                //Project Black
                                                                //PaymentHelper.logProjectBlackUserPromo(context, userId, productId);
                                                                //Xoom2
                                                                PaymentHelper.logUserPromo(context, userId, GlobalConfig.Xoom2PromoId);
                                                                if (!String.IsNullOrEmpty(subscr_id))
                                                                    PaymentHelper.AddToPaypalRecurringBilling(context, product, offering, user, registDt, subscr_id);
                                                                errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", transactionId); break;
                                                            }
                                                        default: errorMessage = "Unknown error."; break;
                                                    }
                                                    collection["errorCode"] = (int)errorCode;
                                                    collection["errorMessage"] = errorMessage;
                                                }
                                            }
                                        }
                                        else // GIVE FREE PRODUCT AND SIGN UP TO RECURRING
                                        {
                                            if (freeProductId > 0 && userId != null)
                                            {
                                                Product freeProduct = context.Products.FirstOrDefault(p => p.ProductId == freeProductId);
                                                Product regularProduct = context.Products.FirstOrDefault(p => p.ProductId == productId);
                                                if (freeProduct != null && regularProduct != null)
                                                {
                                                    string productName = freeProduct.Name;
                                                    Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                                    SubscriptionProductType subscriptionType = ContextHelper.GetProductType(freeProduct);
                                                    User recipient = null;
                                                    ProductPrice priceOfProduct = freeProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                                                    errorCode = PaymentHelper.PayViaPayPal(context, userId, freeProduct.ProductId, subscriptionType, transactionId, recipient == null ? userId : recipient.UserId, cpId, !String.IsNullOrEmpty(subscr_id) ? true : false);
                                                    switch (errorCode)
                                                    {
                                                        case ErrorCodes.IsProcessedPayPalTransaction:
                                                            errorMessage = "This transaction has already been processed."; break;
                                                        case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                                                        case ErrorCodes.Success:
                                                            {
                                                                //Xoom2
                                                                PaymentHelper.logUserPromo(context, userId, GlobalConfig.Xoom2PromoId);
                                                                if (!String.IsNullOrEmpty(subscr_id))
                                                                    PaymentHelper.AddToPaypalRecurringBilling(context, regularProduct, offering, user, registDt, subscr_id);
                                                                errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", transactionId); break;
                                                            }
                                                        default: errorMessage = "Unknown error."; break;
                                                    }
                                                    collection["errorCode"] = (int)errorCode;
                                                    collection["errorMessage"] = errorMessage;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    //}
                }
                else  //no payment status (RECURRING SIGNUP ONLY)
                {
                    if (String.Compare(txn_type, "subscr_signup", true) == 0)
                    {
                        string receiver_email = fc["receiver_email"];
                        string custom = String.Empty;
                        if (String.Compare(receiver_email, GlobalConfig.PayPalBusiness, true) == 0)
                        {
                            custom = HttpUtility.UrlDecode(fc["custom"]);
                            Guid userId;
                            User user = null;
                            string payer_email = String.Empty;
                            if (!String.IsNullOrEmpty(custom))
                            {
                                var strCustomArray = custom.Split('&');
                                switch (strCustomArray.Length)
                                {
                                    case 7:
                                        if (String.Compare(strCustomArray[6], "xoom", true) == 0)
                                        {
                                            //recurring with trial period
                                            int freeProductId = 0;
                                            string wishlistId = String.Empty;
                                            int cpId = 0;
                                            int productId = 0;
                                            //string subscriptionType;

                                            if (!String.IsNullOrEmpty(strCustomArray[0]))
                                                productId = Convert.ToInt32(strCustomArray[0]);
                                            if (!String.IsNullOrEmpty(strCustomArray[5]))
                                                freeProductId = Convert.ToInt32(strCustomArray[5]);

                                            if (!String.IsNullOrEmpty(strCustomArray[2]))
                                                wishlistId = strCustomArray[2];
                                            if (!String.IsNullOrEmpty(strCustomArray[3]))
                                                cpId = Convert.ToInt32(strCustomArray[3]);

                                            if (String.IsNullOrEmpty(payer_email))
                                            {
                                                userId = new Guid(strCustomArray[4]);
                                                user = context.Users.FirstOrDefault(u => u.UserId == userId);
                                            }
                                            else
                                            {
                                                user = context.Users.FirstOrDefault(u => String.Compare(u.EMail, payer_email, true) == 0);
                                                userId = user.UserId;
                                            }

                                            string subscr_id = fc["subscr_id"];
                                            string transactionId = fc["txn_id"] ?? fc["subscr_id"] ?? fc["ipn_track_id"];
                                            //check if subscr_id is in recurring already                                       
                                            var paypalRecurringBilling = user.RecurringBillings.FirstOrDefault(r => r is PaypalRecurringBilling && r.StatusId == GlobalConfig.Visible && String.Compare(((PaypalRecurringBilling)r).SubscriberId, subscr_id, true) == 0);
                                            if (paypalRecurringBilling == null) // GIVE FREE PRODUCT AND SIGN UP TO RECURRING
                                            {
                                                if (freeProductId > 0 && userId != null)
                                                {
                                                    Product freeProduct = context.Products.FirstOrDefault(p => p.ProductId == freeProductId);
                                                    Product product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                                                    if (freeProduct != null && product != null)
                                                    {
                                                        string productName = freeProduct.Name;
                                                        Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                                                        SubscriptionProductType subscriptionType = ContextHelper.GetProductType(freeProduct);
                                                        User recipient = null;
                                                        ProductPrice priceOfProduct = freeProduct.ProductPrices.FirstOrDefault(p => p.CurrencyCode == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                                                        errorCode = PaymentHelper.PayViaPayPal(context, userId, freeProduct.ProductId, subscriptionType, transactionId, recipient == null ? userId : recipient.UserId, cpId, !String.IsNullOrEmpty(subscr_id) ? true : false);
                                                        switch (errorCode)
                                                        {
                                                            case ErrorCodes.IsProcessedPayPalTransaction:
                                                                errorMessage = "This transaction has already been processed."; break;
                                                            case ErrorCodes.IsNotValidAmountPpc: errorMessage = "Ppc credits not enough to buy this product."; break;
                                                            case ErrorCodes.Success:
                                                                {
                                                                    //Xoom2
                                                                    PaymentHelper.logUserPromo(context, userId, GlobalConfig.Xoom2PromoId);
                                                                    if (!String.IsNullOrEmpty(subscr_id))
                                                                        PaymentHelper.AddToPaypalRecurringBilling(context, product, offering, user, registDt, subscr_id);
                                                                    errorMessage = String.Format(@"Your Transaction ID is <span id=""transactionid"">{0}</span>", transactionId); break;
                                                                }
                                                            default: errorMessage = "Unknown error."; break;
                                                        }
                                                        collection["errorCode"] = (int)errorCode;
                                                        collection["errorMessage"] = errorMessage;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    default: break;
                                }
                            }
                        }
                    }
                }
                return new HttpStatusCodeResult(200);
            }
            catch (Exception e)
            {
                MyUtility.LogException(e, "Paypal IPN Transaction Error");
                //string fileFullPath = @"E:\StagingSites\tfc.tv\App_Data\Paypal.txt";
                //TextWriter w = new StreamWriter(fileFullPath, true);
                //w.WriteLine(e.Message + " | " + e.InnerException.Message + " | " + e.StackTrace);
                //w.Close();

                return new HttpStatusCodeResult(400);
            }
        }

        private static bool IsValidIPNRequest(HttpRequestBase requestBase)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(GlobalConfig.PayPalSubmitUrl);

            //Set values for the request back
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            byte[] param = requestBase.BinaryRead(requestBase.ContentLength);
            string strRequest = System.Text.Encoding.ASCII.GetString(param);
            strRequest += "&cmd=_notify-validate";
            req.ContentLength = strRequest.Length;

            //for proxy
            //WebProxy proxy = new WebProxy(new Uri("http://url:port#"));
            //req.Proxy = proxy;

            //Send the request to PayPal and get the response
            StreamWriter streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII);
            streamOut.Write(strRequest);
            streamOut.Close();
            StreamReader streamIn = new StreamReader(req.GetResponse().GetResponseStream());
            string strResponse = streamIn.ReadToEnd();
            streamIn.Close();

            if (String.Compare(strResponse, "VERIFIED", true) == 0)
                return true;
            else
            {
                try
                {
                    string fileFullPath = @"E:\StagingSites\tfc.tv\App_Data\Paypal.txt";
                    TextWriter w = new StreamWriter(fileFullPath, true);
                    w.WriteLine(strResponse);
                    w.Close();
                    //Log errors
                }
                catch (Exception) { }
                return false;
            }
        }

        private void SendGiftShareToSocialNetwork(Product product, User recipient)
        {
            //Publish user action
            List<ActionLink> actionlinks = new List<ActionLink>();
            actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.sgift_actionlink_href) });
            List<MediaItem> mediaItems = new List<MediaItem>();
            mediaItems.Add(new MediaItem() { type = SNSTemplates.sgift_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.sgift_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.sgift_mediaitem_href, User.Identity.Name)) });
            UserAction action = new UserAction()
            {
                actorUID = User.Identity.Name,
                userMessage = String.Format(SNSTemplates.sgift_usermessage_external, product.Description, String.Format("{0} {1}", recipient.FirstName, recipient.LastName)),
                title = SNSTemplates.sgift_title,
                subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.sgift_subtitle),
                linkBack = String.Format(SNSTemplates.sgift_linkback, User.Identity.Name),
                description = SNSTemplates.sgift_description,
                actionLinks = actionlinks,
                mediaItems = mediaItems
            };

            GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "external");
            //Modify action to suit Internal feed needs
            mediaItems.Clear();
            mediaItems.Add(new MediaItem() { type = SNSTemplates.sgift_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.sgift_mediaitem_src_internal), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.sgift_mediaitem_href, User.Identity.Name)) });
            action.userMessage = String.Format(SNSTemplates.sgift_usermessage, product.Description, recipient.UserId, String.Format("{0} {1}", recipient.FirstName, recipient.LastName));
            action.description = String.Format(SNSTemplates.sgift_description_internal, product.Description, recipient.UserId, String.Format("{0} {1}", recipient.FirstName, recipient.LastName));
            action.mediaItems = mediaItems;
            GigyaMethods.PublishUserAction(action, new System.Guid(User.Identity.Name), "internal");
        }

        private void ReceiveGiftShareToSocialNetwork(Product product, User recipient, User user)
        {
            //Publish user action
            List<ActionLink> actionlinks = new List<ActionLink>();
            actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.rgift_actionlink_href) });
            List<MediaItem> mediaItems = new List<MediaItem>();
            mediaItems.Add(new MediaItem() { type = SNSTemplates.rgift_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.rgift_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.rgift_mediaitem_href, recipient.UserId.ToString())) });
            UserAction action = new UserAction()
            {
                actorUID = recipient.UserId.ToString(),
                userMessage = String.Format(SNSTemplates.rgift_usermessage_external, product.Description, String.Format("{0} {1}", user.FirstName, user.LastName)),
                title = SNSTemplates.rgift_title,
                subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.rgift_subtitle),
                linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.rgift_linkback, recipient.UserId.ToString())),
                description = SNSTemplates.rgift_description,
                actionLinks = actionlinks,
                mediaItems = mediaItems
            };

            GigyaMethods.PublishUserAction(action, recipient.UserId, "external");
            //Modify action to suit Internal feed needs
            mediaItems.Clear();
            mediaItems.Add(new MediaItem() { type = SNSTemplates.rgift_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.baseUrl, SNSTemplates.rgift_mediaitem_src_internal), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.rgift_mediaitem_href, recipient.UserId.ToString())) });
            action.userMessage = String.Format(SNSTemplates.rgift_usermessage, product.Description, user.UserId, String.Format("{0} {1}", user.FirstName, user.LastName));
            action.description = String.Format(SNSTemplates.rgift_description_internal, product.Description, user.UserId, String.Format("{0} {1}", user.FirstName, user.LastName));
            action.mediaItems = mediaItems;
            GigyaMethods.PublishUserAction(action, recipient.UserId, "internal");
        }
    }
}