using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
using System.Xml;

using TFCTV.Helpers;
using IPTV2_Model;
namespace TFCTV.Controllers
{
    public class MopayController : Controller
    {
        //
        // GET: /Mopay/

        public ActionResult Index()
        {
            return View();
        }
        ////////////////////////////////////////MOPAY
        /// <summary>
        /// accepts xml validation from mopay
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public ActionResult Notify(FormCollection fc)
        {
            if (GlobalConfig.IsMopayListenerEnabled)
            {
                try
                {
                    var context = new IPTV2Entities();
                    /////////////convert input stream to string then check if input stream is null or empty
                    System.IO.Stream str;
                    str = Request.InputStream;

                    if (str.Length <= 0)
                        throw new Exception("Input stream is null or empty");
                    else
                    {
                        string country = string.Empty;
                        decimal amount = 0;
                        string currency = string.Empty;
                        string guid = string.Empty;
                        string userid = string.Empty;
                        string status = string.Empty;
                        string refid = string.Empty;
                        string mopayError = string.Empty;
                        string mopayErrorCode = string.Empty;

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(str);
                        XmlNode mopaysessionNode = xmlDoc.SelectSingleNode("mopay-notification");
                        XmlNode paysessionNode = mopaysessionNode.SelectSingleNode("payment-session");
                        foreach (XmlAttribute att in paysessionNode.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "country":
                                    if (!string.IsNullOrEmpty(att.Value))
                                        country = (att.Value);
                                    break;
                                case "amount":
                                    if (!string.IsNullOrEmpty(att.Value))
                                        amount = Convert.ToDecimal(att.Value);
                                    break;
                                case "currency":
                                    if (!string.IsNullOrEmpty(att.Value))
                                        currency = (att.Value);
                                    break;
                            }
                        }

                        foreach (XmlNode chldNode in paysessionNode.ChildNodes)
                        {
                            switch (chldNode.Name)
                            {
                                case "guid":
                                    if (!string.IsNullOrEmpty(chldNode.InnerText))
                                        guid = (chldNode.InnerText);
                                    break;
                                case "myid":
                                    if (!string.IsNullOrEmpty(chldNode.InnerText))
                                        userid = (chldNode.InnerText);
                                    break;
                                case "status":
                                    if (!string.IsNullOrEmpty(chldNode.InnerText))
                                        status = (chldNode.InnerText);
                                    break;
                                case "error":
                                    if (!string.IsNullOrEmpty(chldNode.InnerText))
                                    {
                                        mopayError = chldNode.SelectSingleNode("message").InnerText;
                                        mopayErrorCode = chldNode.SelectSingleNode("errorcode").InnerText;
                                    }
                                    break;
                                case "transaction":
                                    refid = chldNode.Attributes["id"].Value; break;
                            }
                        }

                        ////////////evaluate parameters if empty 
                        if (string.IsNullOrEmpty(currency) || string.IsNullOrEmpty(userid) || amount <= 0)
                        {
                            throw new Exception("Required field is null or empty.");
                        }
                        else if (string.IsNullOrEmpty(refid))
                        {
                            MyUtility.LogException(new Exception("MoPay: Transaction id is empty."));
                            return new HttpStatusCodeResult(200);
                        }
                        else
                        {
                            MopayTransactionRequest mopayRequest;
                            mopayRequest = context.MopayTransactionRequests.FirstOrDefault(m => String.Compare(m.GUID, guid, true) == 0);
                            if (mopayRequest == null)
                            {
                                mopayRequest = new MopayTransactionRequest()
                                {
                                    GUID = guid,
                                    UserId = new Guid(userid),
                                    ReferenceId = refid,
                                    Currency = currency,
                                    Amount = amount,
                                    DateCreated = DateTime.Now,
                                    NumberOfAttempts = 0
                                };
                                context.MopayTransactionRequests.Add(mopayRequest);
                                context.SaveChanges();
                            }

                            if (String.Compare(status, "SUCCESS", true) == 0)
                            {
                                var userId = new Guid(userid);
                                var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                                if (user != null)
                                {
                                    var errorcode = ReloadHelper.ReloadViaMopay(userId, amount, currency, refid, guid);
                                    if (errorcode == ErrorCodes.Success)
                                    {
                                        mopayRequest.ErrorCode = ErrorCodes.Success.ToString();
                                        mopayRequest.ErrorMessage = "SUCCESS";
                                        context.SaveChanges();
                                        return new HttpStatusCodeResult(200);
                                    }
                                }
                            }
                            else if (String.Compare(status, "ERROR", true) == 0)
                            {
                                mopayRequest.ErrorCode = mopayErrorCode;
                                mopayRequest.ErrorMessage = mopayError;
                                mopayRequest.UpdatedOn = System.DateTime.Now;
                                mopayRequest.NumberOfAttempts += 1;
                                context.SaveChanges();
                                if (mopayRequest.NumberOfAttempts < 3)
                                    throw new Exception(mopayError);
                                else
                                    return new HttpStatusCodeResult(200);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MyUtility.LogException(e);
                }
                return new HttpStatusCodeResult(500);
            }
            else
                return new HttpStatusCodeResult(404);
        }
    }
}
