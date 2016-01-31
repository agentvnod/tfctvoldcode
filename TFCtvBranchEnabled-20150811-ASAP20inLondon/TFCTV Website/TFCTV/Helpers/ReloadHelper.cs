using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using GOMS_TFCtv;
using IPTV2_Model;
using TFCTV.Models;
using System.Xml;

namespace TFCTV.Helpers
{
    public static class ReloadHelper
    {
        public static Ppc.ErrorCode ReloadViaPrepaidCard(IPTV2Entities context, System.Guid userId, string serial, string pin)
        {
            try
            {
                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                Ppc ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == serial);

                if (ppc == null) //Serial does not exist
                    return Ppc.ErrorCode.InvalidSerialNumber;
                //if (!(ppc.SerialNumber == serial && ppc.Pin == pin)) //Invalid serial/pin combination
                //    return ErrorCodes.IsInvalidCombinationPpc;
                //if (ppc.UserId != null) // Ppc has already been used
                //    return ErrorCodes.IsUsedPpc;
                //if (!(ppc is ReloadPpc)) // Ppc is not of type Subscription
                //    return ErrorCodes.IsSubscriptionPpc;
                //if (registDt > ppc.ExpirationDate) // Ppc is expired
                //    return ErrorCodes.IsExpiredPpc;
                //ReloadPpc rPpc = (ReloadPpc)ppc;
                //if (ppc.Currency.Trim() != MyUtility.GetCurrencyOrDefault(user.CountryCode) && ppc.Currency != "---") // Ppc not valid in your country
                //    return ErrorCodes.IsNotValidInCountryPpc;
                //if (wallet == null)
                //    return ErrorCodes.HasNoWallet;
                //if (wallet.Currency != ppc.Currency.Trim() && ppc.Currency != "---")
                //    return ErrorCodes.WallePpcCurrencyConflict;

                Ppc.ErrorCode validate = Ppc.ValidateReloadPpc(context, ppc.SerialNumber, ppc.Pin, userId);

                if (validate == Ppc.ErrorCode.Success)
                {
                    ReloadPpc rPpc = (ReloadPpc)ppc;
                    PpcReloadTransaction transaction = new PpcReloadTransaction()
                    {
                        Amount = ppc.GetAmount(CurrencyCode),
                        Currency = CurrencyCode, //ppc.Currency.Trim(),
                        Reference = ppc.SerialNumber.ToUpper(),
                        UserWallet = wallet,
                        ReloadPpc = rPpc,
                        Date = registDt,
                        OfferingId = GlobalConfig.offeringId,
                        StatusId = GlobalConfig.Visible
                    };

                    user.Transactions.Add(transaction);

                    //update the Ppc

                    ppc.UserId = userId;
                    ppc.UsedDate = registDt;

                    wallet.Balance += ppc.GetAmount(CurrencyCode);
                    wallet.LastUpdated = registDt;

                    if (context.SaveChanges() > 0)
                    {
                        return Ppc.ErrorCode.Success;
                    }
                    return Ppc.ErrorCode.EntityUpdateError;
                }
                else
                    return validate;
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }

        public static ErrorCodes ReloadViaPayPal(IPTV2Entities context, System.Guid userId, decimal Amount, string TransactionID)
        {
            try
            {
                //var context = new IPTV2Entities();
                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);


                if (GlobalConfig.UsePayPalIPNLog)
                {
                    if (context.PaypalIPNLogs.Count(t => String.Compare(t.UniqueTransactionId, TransactionID, true) == 0) > 0)
                        return ErrorCodes.IsProcessedPayPalTransaction;
                }
                else
                {
                    Transaction ppt = context.Transactions.FirstOrDefault(t => String.Compare(t.Reference, TransactionID, true) == 0);
                    if (ppt != null)
                        return ErrorCodes.IsProcessedPayPalTransaction;
                }

                //Transaction ppt = context.Transactions.FirstOrDefault(t => String.Compare(t.Reference, TransactionID, true) == 0);
                //if (ppt != null)
                //    return ErrorCodes.IsProcessedPayPalTransaction;

                PaypalReloadTransaction transaction = new PaypalReloadTransaction()
                {
                    Currency = wallet.Currency,
                    Reference = TransactionID,
                    Amount = Amount,
                    UserWallet = wallet,
                    Date = registDt,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                var ipnLog = new PaypalIPNLog()
                {
                    User = user,
                    UniqueTransactionId = TransactionID,
                    TransactionDate = registDt
                };

                user.PaypalIPNLogs.Add(ipnLog);

                user.Transactions.Add(transaction);

                wallet.Balance += Amount;
                wallet.LastUpdated = registDt;

                if (context.SaveChanges() > 0)
                {
                    return ErrorCodes.Success;
                }
                return ErrorCodes.EntityUpdateError;
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }

        public static ErrorCodes ReloadViaPayPal(IPTV2Entities context, System.Guid userId, decimal Amount, string TransactionID, string CurrencyCode)
        {
            try
            {
                //var context = new IPTV2Entities();
                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == CurrencyCode);
                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);


                if (GlobalConfig.UsePayPalIPNLog)
                {
                    if (context.PaypalIPNLogs.Count(t => String.Compare(t.UniqueTransactionId, TransactionID, true) == 0) > 0)
                        return ErrorCodes.IsProcessedPayPalTransaction;
                }
                else
                {
                    Transaction ppt = context.Transactions.FirstOrDefault(t => String.Compare(t.Reference, TransactionID, true) == 0);
                    if (ppt != null)
                        return ErrorCodes.IsProcessedPayPalTransaction;
                }

                //Transaction ppt = context.Transactions.FirstOrDefault(t => String.Compare(t.Reference, TransactionID, true) == 0);
                //if (ppt != null)
                //    return ErrorCodes.IsProcessedPayPalTransaction;

                if (wallet == null)
                {
                    wallet = new UserWallet()
                    {
                        Currency = CurrencyCode,
                        IsActive = false,
                        LastUpdated = registDt,
                        Balance = Amount
                    };
                    user.UserWallets.Add(wallet);
                }

                else
                {
                    wallet.Balance += Amount;
                    wallet.LastUpdated = registDt;
                }

                PaypalReloadTransaction transaction = new PaypalReloadTransaction()
                {
                    Currency = wallet.Currency,
                    Reference = TransactionID,
                    Amount = Amount,
                    UserWallet = wallet,
                    Date = registDt,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                user.Transactions.Add(transaction);

                if (context.SaveChanges() > 0)
                {
                    return ErrorCodes.Success;
                }
                return ErrorCodes.EntityUpdateError;
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }

        public static string PDTHandler(string tx)
        {
            string result = "";
            string postData = string.Format("cmd=_notify-synch&tx={0}&at={1}", tx, GlobalConfig.PDTToken);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GlobalConfig.PayPalSubmitUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            StreamWriter sw = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
            sw.Write(postData);
            sw.Close();

            StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream());
            result = sr.ReadToEnd();
            sr.Close();
            if (result.StartsWith("SUCCESS"))
            {
                return result;
            }

            return null;
        }

        public static ErrorCodes ReloadViaCreditCard(IPTV2Entities context, System.Guid userId, CreditCardInfo info, decimal amount)
        {
            try
            {
                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                if (info == null) { }
                if (String.IsNullOrEmpty(info.Number)) { }
                if (String.IsNullOrEmpty(info.CardSecurityCode)) { }
                if (String.IsNullOrEmpty(info.Name)) { }
                if (String.IsNullOrEmpty(info.StreetAddress)) { }
                if (String.IsNullOrEmpty(info.PostalCode)) { }

                CreditCardReloadTransaction transaction = new CreditCardReloadTransaction()
                {
                    Amount = amount,
                    Currency = CurrencyCode,
                    Reference = info.CardType.ToString().Replace("_", " ").ToUpper(),
                    UserWallet = wallet,
                    Date = registDt,
                    StatusId = GlobalConfig.Visible
                };

                //user.Transactions.Add(transaction);

                var gomsService = new GomsTfcTv();

                var response = gomsService.ReloadWalletViaCreditCard(context, userId, transaction, info);

                if (response.IsSuccess)
                {
                    return ErrorCodes.Success;
                }

                ErrorCodes code = ErrorCodes.UnknownError;
                switch (response.StatusCode)
                {
                    case "7": code = ErrorCodes.CreditCardHasExpired; break;
                    default: code = ErrorCodes.EntityUpdateError; break;
                }
                return code;
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }

        public static ErrorResponse ReloadViaCreditCard2(IPTV2Entities context, System.Guid userId, CreditCardInfo info, decimal amount)
        {
            ErrorResponse resp = new ErrorResponse();
            try
            {
                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == MyUtility.GetCurrencyOrDefault(user.CountryCode));
                string CurrencyCode = MyUtility.GetCurrencyOrDefault(user.CountryCode);
                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);

                if (info == null) { }
                if (String.IsNullOrEmpty(info.Number)) { }
                if (String.IsNullOrEmpty(info.CardSecurityCode)) { }
                if (String.IsNullOrEmpty(info.Name)) { }
                if (String.IsNullOrEmpty(info.StreetAddress)) { }
                if (String.IsNullOrEmpty(info.PostalCode)) { }

                CreditCardReloadTransaction transaction = new CreditCardReloadTransaction()
                {
                    Amount = amount,
                    Currency = CurrencyCode,
                    Reference = info.CardType.ToString().Replace("_", " ").ToUpper(),
                    UserWallet = wallet,
                    Date = registDt,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                //user.Transactions.Add(transaction);

                var gomsService = new GomsTfcTv();

                var response = gomsService.ReloadWalletViaCreditCard(context, userId, transaction, info);

                if (response.IsSuccess)
                {
                    resp.Code = (int)ErrorCodes.Success;
                    resp.Message = "Successful";
                    return resp;
                }
                resp.Code = Convert.ToInt32(response.StatusCode);
                resp.Message = response.StatusMessage;
                return resp;
            }

            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }

        public static string IPNHandler(HttpRequestBase requestBase)
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
            return strResponse;
        }
        public static ErrorCodes ReloadViaMopay(System.Guid userId, decimal Amount, string CurrencyCode, string refid, string Guid)
        {
            try
            {

                var context = new IPTV2Entities();
                DateTime registDt = DateTime.Now;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                Offering offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
                UserWallet wallet = user.UserWallets.FirstOrDefault(w => w.Currency == CurrencyCode);
                Currency currency = context.Currencies.FirstOrDefault(c => c.Code == CurrencyCode);

                if (wallet == null)
                {
                    wallet = new UserWallet()
                    {
                        Currency = CurrencyCode,
                        IsActive = false,
                        LastUpdated = registDt,
                        Balance = Amount
                    };
                    user.UserWallets.Add(wallet);
                }

                else
                {
                    wallet.Balance += Amount;
                    wallet.LastUpdated = registDt;
                }

                MopayReloadTransaction transaction = new MopayReloadTransaction()
                {
                    Currency = wallet.Currency,
                    Reference = refid,
                    Amount = Amount,
                    UserWallet = wallet,
                    Date = registDt,
                    OfferingId = GlobalConfig.offeringId,
                    StatusId = GlobalConfig.Visible
                };

                user.Transactions.Add(transaction);

                if (context.SaveChanges() > 0)
                {
                    try
                    {
                        //post confirm to mopay
                        string poststatus = string.Empty;
                        string guid = string.Empty;
                        string message = string.Empty;

                        string postparam = string.Format("cid={0}&password={1}?guid={2}&externaluid={3}", GlobalConfig.MopayCID, GlobalConfig.MopayPassword, Guid, userId.ToString());
                        WebRequest request = WebRequest.Create(GlobalConfig.MopayDeliveryConfirmationURL);
                        request.Method = "POST";
                        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postparam);
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.ContentLength = byteArray.Length;
                        Stream dataStream = request.GetRequestStream();
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        dataStream.Close();
                        WebResponse response = request.GetResponse();
                        dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                        reader.Close();
                        dataStream.Close();
                        response.Close();

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(responseFromServer);
                        XmlNode messageNode = xmlDoc.SelectSingleNode("message");
                        message = messageNode.Value;
                        XmlNode productDeliveryConfirmationNode = xmlDoc.SelectSingleNode("product-delivery-confirmation");
                        foreach (XmlNode chldNode in productDeliveryConfirmationNode.ChildNodes)
                        {
                            switch (chldNode.Name)
                            {
                                case "guid":
                                    if (!string.IsNullOrEmpty(chldNode.InnerText))
                                        guid = (chldNode.InnerText);
                                    break;
                                case "status":
                                    if (!string.IsNullOrEmpty(chldNode.InnerText))
                                        poststatus = (chldNode.InnerText);
                                    break;

                            }
                        }
                        MopayTransactionRequest mopaTranRequest = context.MopayTransactionRequests.FirstOrDefault(m => String.Compare(m.GUID, guid) == 0);
                        mopaTranRequest.ErrorMessage = message;
                        mopaTranRequest.UpdatedOn = System.DateTime.Now;
                        context.SaveChanges();
                    }
                    catch (Exception) { }
                    return ErrorCodes.Success;
                }
                return ErrorCodes.EntityUpdateError;
            }
            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }
    }
}