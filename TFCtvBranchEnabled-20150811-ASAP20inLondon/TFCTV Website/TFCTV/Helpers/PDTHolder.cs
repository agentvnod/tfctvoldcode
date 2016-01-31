using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace TFCTV.Helpers
{
    public class PDTHolder
    {
        public double GrossTotal { get; set; }

        public int InvoiceNumber { get; set; }

        public string PaymentStatus { get; set; }

        public string PayerFirstName { get; set; }

        public double PaymentFee { get; set; }

        public string BusinessEmail { get; set; }

        public string PayerEmail { get; set; }

        public string TxToken { get; set; }

        public string PayerLastName { get; set; }

        public string ReceiverEmail { get; set; }

        public string ItemName { get; set; }

        public string Currency { get; set; }

        public string TransactionId { get; set; }

        public string SubscriberId { get; set; }

        public string Custom { get; set; }

        public static Custom Parse(string postData)
        {
            //string key, value;
            Custom custom = new Custom();
            try
            {
                string[] strArray = postData.Split('\n');
                for (int i = 1; i < strArray.Length; i++)
                {
                    if (strArray[i].StartsWith("custom"))
                    {
                        string[] param = strArray[i].Split('=');
                        string[] values = HttpUtility.UrlDecode(param[1]).Split('&');
                        if (values.Length > 1)
                        {
                            custom.productId = Convert.ToInt32(values[0]);
                            custom.subscriptionType = values[1];
                        }
                        else
                            custom.Amount = Convert.ToDecimal(values[0]);
                        if (values.Length == 3)
                            custom.WishlistId = values[2];
                        if (values.Length == 4)
                        {
                            custom.WishlistId = values[2];
                            custom.cpId = values[3];
                        }
                        if (values.Length == 5)
                        {
                            custom.WishlistId = values[2];
                            custom.cpId = values[3];
                            custom.userId = values[4];
                        }

                    }
                    if (strArray[i].StartsWith("txn_id"))
                    {
                        string[] param = strArray[i].Split('=');
                        custom.TransactionId = HttpUtility.UrlDecode(param[1]);
                    }

                    if (strArray[i].StartsWith("subscr_id"))
                    {
                        string[] param = strArray[i].Split('=');
                        custom.subscr_id = HttpUtility.UrlDecode(param[1]);
                    }
                }
                return custom;
            }
            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }

        public static Custom ParseForReload(string postData)
        {
            Custom custom = new Custom();
            try
            {
                string[] strArray = postData.Split('\n');
                for (int i = 1; i < strArray.Length; i++)
                {
                    if (strArray[i].StartsWith("custom"))
                    {
                        string[] param = strArray[i].Split('=');
                        string[] values = HttpUtility.UrlDecode(param[1]).Split('&');
                        if (values.Length > 1)
                        {
                            custom.Amount = Convert.ToDecimal(values[0]);
                            custom.userId = values[1];
                        }
                    }
                    if (strArray[i].StartsWith("txn_id"))
                    {
                        string[] param = strArray[i].Split('=');
                        custom.TransactionId = HttpUtility.UrlDecode(param[1]);
                    }
                    if (strArray[i].StartsWith("mc_currency"))
                    {
                        string[] param = strArray[i].Split('=');
                        custom.CurrencyCode = HttpUtility.UrlDecode(param[1]);
                    }
                    if (strArray[i].StartsWith("mc_gross"))
                    {
                        string[] param = strArray[i].Split('=');
                        custom.Amount = Convert.ToDecimal(HttpUtility.UrlDecode(param[1]));
                    }
                }
                return custom;
            }
            catch (Exception e) { Debug.WriteLine(e.InnerException); throw; }
        }
    }

    public class Custom
    {
        public int productId { get; set; }

        public string subscriptionType { get; set; }

        public string TransactionId { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; }

        public string WishlistId { get; set; }

        public string cpId { get; set; }

        public string userId { get; set; }

        public string subscr_id { get; set; }
    }
}