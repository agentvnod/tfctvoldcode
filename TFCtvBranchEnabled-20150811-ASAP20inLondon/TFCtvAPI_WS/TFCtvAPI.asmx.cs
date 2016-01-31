using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using IPTV2_Model;

namespace TFCtvAPI_WS
{
    /// <summary>
    /// Summary description for TFCtvAPI
    /// </summary>
    [WebService(Namespace = "http://tfc.tv/", Name = "TFCtv API", Description = "This is the TFC.tv API")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
    // [System.Web.Script.Services.ScriptService]
    public class TFCtvAPI : System.Web.Services.WebService
    {
        public AuthenticationHeader Credentials;

        private string _username = ConfigurationManager.AppSettings["ReqUsername"];
        private string _password = ConfigurationManager.AppSettings["ReqPassword"];

        private string SoapHeaderUsername = ConfigurationManager.AppSettings["SoapHeaderUsername"];
        private string SoapHeaderPassword = ConfigurationManager.AppSettings["SoapHeaderPassword"];

        private int offeringId = Convert.ToInt32(ConfigurationManager.AppSettings["offeringId"]);

        private string JapanCountryCode = ConfigurationManager.AppSettings["JapanCountryCode"];

        private string IPTV2EntitiesAzureConnectionString = ConfigurationManager.ConnectionStrings["IPTV2EntitiesAzure"].ConnectionString;

        private bool isProduction = Convert.ToBoolean(ConfigurationManager.AppSettings["isProduction"]);

        private string DefaultCurrencyCode = ConfigurationManager.AppSettings["DefaultCurrencyCode"];

        private string TVECountryWhitelist = ConfigurationManager.AppSettings["TVECountryRestriction"];

        [WebMethod(Description = "Typical computer-generated function thrown by MS.Net")]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod(Description = "Activate a range of Ppcs")]
        //[XmlInclude(typeof(ReqActivatePpc)), XmlInclude(typeof(TFCtvResponse))]
        [SoapHeader("Credentials")]
        public TFCtvResponse TogglePpc(ReqActivatePpc req)
        //public TFCtvResponse ActivatePpc(string PpcStart, string PpcEnd, string ActivatedBy, DateTime ActivatedOn, int StatusId)
        {
            //ReqActivatePpc req = new ReqActivatePpc()
            //{
            //    PpcEnd = PpcEnd,
            //    PpcStart = PpcStart,
            //    ActivatedOn = ActivatedOn,
            //    ActivatedBy = ActivatedBy,
            //    StatusId = StatusId
            //};

            TFCtvResponse resp = null;

            string ip = ConfigurationManager.AppSettings["IpWhiteList"];
            string[] IpAddresses = ip.Split(';');
            bool isWhitelisted = IpAddresses.Contains(HttpContext.Current.Request.UserHostAddress);
            if (!isWhitelisted)
            {
                resp = new TFCtvResponse() { Code = -3001, Message = "Ip address is unauthorized." }; return resp;
            }

            if (!HttpContext.Current.Request.IsLocal)
            {
                //Check SoapHeader
                if (Credentials.Username.ToLower() != SoapHeaderUsername || Credentials.Password != SoapHeaderPassword)
                { resp = new TFCtvResponse() { Code = -3002, Message = "Call is unauthorized." }; return resp; }
            }

            if (req == null)
                resp = new TFCtvResponse() { Code = -3003, Message = "Request parameter is empty." };
            else
            {
                //Check for Ppc details
                if (String.IsNullOrEmpty(req.PpcStart) || String.IsNullOrEmpty(req.PpcEnd) || String.IsNullOrEmpty(req.ActivatedBy) || req.StatusId == null)
                {
                    resp = new TFCtvResponse() { Code = -3004, Message = "Missing required fields." };
                    return resp;
                }

                var context = new IPTV2Entities();
                var ppcs = context.Ppcs.Where(p => String.Compare(p.SerialNumber, req.PpcStart.Replace(" ", "")) >= 0 && String.Compare(p.SerialNumber, req.PpcEnd.Replace(" ", "")) <= 0);
                foreach (var item in ppcs)
                {
                    if (item.StatusId == req.StatusId)
                    {
                        resp = new TFCtvResponse() { Code = -1000, Message = String.Format("Unable to {1} {0}. Pls. check Ppc range.", item.SerialNumber, req.StatusId == 1 ? "activate" : "deactivate") };
                        return resp;
                    }
                    if (req.StatusId == 1)
                    {
                        item.ActivatedBy = req.ActivatedBy;
                        item.ActivatedOn = req.ActivatedOn == null ? DateTime.Now : req.ActivatedOn;
                    }
                    else
                    {
                        item.ActivatedBy = null;
                        item.ActivatedOn = null;
                    }
                    item.StatusId = (int)req.StatusId;
                }
                try
                {
                    if (context.SaveChanges() > 0)
                        resp = new TFCtvResponse() { Code = 0, Message = String.Format("Ppcs have been successfully {0}.", req.StatusId == 1 ? "activated" : "deactivated") };
                    else
                        resp = new TFCtvResponse() { Code = -1002, Message = String.Format("Unable to {0} Ppcs.", req.StatusId == 1 ? "activate" : "deactivate") };
                }
                catch (Exception e)
                {
                    resp = new TFCtvResponse() { Code = -3000, Message = e.InnerException.Message };
                }
            }
            return resp;
        }

        [WebMethod(Description = "Reload via SmartPit")]
        [SoapHeader("Credentials")]
        public TFCtvResponse ReloadWalletViaSmartPit(ReqReloadWalletViaSmartPit req)
        {
            //ReqReloadWalletViaSmartPit req = new ReqReloadWalletViaSmartPit()
            //{
            //    Amount = 10,
            //    GomsCustomerId = 1073970,
            //    GomsTransactionDate = DateTime.Now,
            //    GomsTransactionId = 123456789,
            //    GomsWalletId = 16408
            //};

            TFCtvResponse resp = null;
            DateTime registDt = DateTime.Now;
            string ip = ConfigurationManager.AppSettings["IpWhiteList"];
            string[] IpAddresses = ip.Split(';');
            bool isWhitelisted = IpAddresses.Contains(HttpContext.Current.Request.UserHostAddress);
            if (!isWhitelisted)
            {
                resp = new TFCtvResponse() { Code = -3001, Message = "Ip address is unauthorized." }; return resp;
            }

            if (!HttpContext.Current.Request.IsLocal)
            {
                //Check SoapHeader
                if (Credentials.Username.ToLower() != SoapHeaderUsername || Credentials.Password != SoapHeaderPassword)
                { resp = new TFCtvResponse() { Code = -3002, Message = "Call is unauthorized." }; return resp; }
            }

            if (req == null)
                resp = new TFCtvResponse() { Code = -3003, Message = "Request parameter is empty." };
            else
            {
                if (req.GomsCustomerId == null || req.GomsTransactionDate == null || req.GomsTransactionId == null || req.GomsWalletId == null || req.Amount == null)
                {
                    resp = new TFCtvResponse() { Code = -3004, Message = "Missing required fields." };
                    return resp;
                }

                if (req.Amount != null)
                    if (req.Amount <= 0)
                    {
                        resp = new TFCtvResponse() { Code = -1000, Message = "Amount is not applicable." };
                        return resp;
                    }

                var context = new IPTV2Entities();
                if (isProduction)
                    context.Database.Connection.ConnectionString = IPTV2EntitiesAzureConnectionString;

                var user = context.Users.FirstOrDefault(u => u.GomsCustomerId == req.GomsCustomerId);
                if (user == null)
                {
                    resp = new TFCtvResponse() { Code = -1001, Message = "User does not exist." };
                    return resp;
                }
                if (user.Country == null)
                {
                    resp = new TFCtvResponse() { Code = -1002, Message = "User country is not valid." };
                    return resp;
                }
                else
                {
                    if (user.Country.Code != JapanCountryCode)
                    {
                        resp = new TFCtvResponse() { Code = -1002, Message = "User country is not valid." };
                        return resp;
                    }
                }

                if (String.IsNullOrEmpty(user.SmartPitId))
                {
                    resp = new TFCtvResponse() { Code = -1003, Message = "No Smart Pit Card attached to account." };
                    return resp;
                }

                if (user.Transactions.Count(t => t.GomsTransactionId == req.GomsTransactionId) > 0)
                {
                    resp = new TFCtvResponse() { Code = -1006, Message = "Transaction already exists in TFC.tv." };
                    return resp;
                }

                //var wallet = user.UserWallets.FirstOrDefault(u => u.GomsWalletId == req.GomsWalletId && u.IsActive == true);
                var wallet = user.UserWallets.FirstOrDefault(u => u.Currency == user.Country.CurrencyCode && u.IsActive == true && u.GomsWalletId == req.GomsWalletId);
                if (wallet == null)
                    resp = new TFCtvResponse() { Code = -1004, Message = "Wallet does not exist." };
                else
                {
                    SmartPitReloadTransaction transaction = new SmartPitReloadTransaction()
                    {
                        Currency = user.Country.CurrencyCode,
                        Reference = req.GomsTransactionId.ToString(),
                        UserWallet = wallet,
                        SmartPitId = user.SmartPitId,
                        Amount = (decimal)req.Amount,
                        //Date = registDt,
                        Date = (DateTime)req.GomsTransactionDate,
                        OfferingId = offeringId,
                        GomsTransactionDate = req.GomsTransactionDate,
                        GomsTransactionId = req.GomsTransactionId,
                        StatusId = 1
                    };

                    user.Transactions.Add(transaction);

                    wallet.Balance += (int)req.Amount;
                    //wallet.LastUpdated = registDt;
                    wallet.LastUpdated = (DateTime)req.GomsTransactionDate;

                    try
                    {
                        if (context.SaveChanges() > 0)
                            resp = new TFCtvResponse() { Code = 0, Message = String.Format("Successfully reloaded {0} to user's wallet.", req.Amount) };
                        else
                            resp = new TFCtvResponse() { Code = -1005, Message = "Unable to reload user wallet." };
                    }
                    catch (Exception e)
                    {
                        resp = new TFCtvResponse() { Code = -3000, Message = e.InnerException.Message };
                    }
                }
            }
            return resp;
        }

        [WebMethod(Description = "Update user's SmartPit")]
        [SoapHeader("Credentials")]
        public TFCtvResponse UpdateSmartPit(ReqUpdateSmartPit req)
        {
            TFCtvResponse resp = null;
            DateTime registDt = DateTime.Now;
            string ip = ConfigurationManager.AppSettings["IpWhiteList"];
            string[] IpAddresses = ip.Split(';');
            bool isWhitelisted = IpAddresses.Contains(HttpContext.Current.Request.UserHostAddress);
            if (!isWhitelisted)
            {
                resp = new TFCtvResponse() { Code = -3001, Message = "Ip address is unauthorized." }; return resp;
            }

            if (!HttpContext.Current.Request.IsLocal)
            {
                //Check SoapHeader
                if (Credentials.Username.ToLower() != SoapHeaderUsername || Credentials.Password != SoapHeaderPassword)
                { resp = new TFCtvResponse() { Code = -3002, Message = "Call is unauthorized." }; return resp; }
            }

            if (req == null)
                resp = new TFCtvResponse() { Code = -3003, Message = "Request parameter is empty." };
            else
            {
                if (req.GomsCustomerId == null)
                {
                    resp = new TFCtvResponse() { Code = -3004, Message = "Missing required fields." };
                    return resp;
                }

                var context = new IPTV2Entities();
                if (isProduction)
                    context.Database.Connection.ConnectionString = IPTV2EntitiesAzureConnectionString;

                var user = context.Users.FirstOrDefault(u => u.GomsCustomerId == req.GomsCustomerId);
                if (user == null)
                {
                    resp = new TFCtvResponse() { Code = -1001, Message = "User does not exist." };
                    return resp;
                }
                if (user.Country == null)
                {
                    resp = new TFCtvResponse() { Code = -1002, Message = "User country is not valid." };
                    return resp;
                }
                else
                {
                    if (user.Country.Code != JapanCountryCode)
                    {
                        resp = new TFCtvResponse() { Code = -1002, Message = "User country is not valid." };
                        return resp;
                    }
                }

                user.SmartPitId = req.SmartPitCardNo;
                if (String.IsNullOrEmpty(req.SmartPitCardNo))
                    user.SmartPitRegistrationDate = null;
                try
                {
                    if (context.SaveChanges() > 0)
                        resp = new TFCtvResponse() { Code = 0, Message = String.Format("SmartPit successfully updated. CustomerId: {0}, SmartPit: {1}", req.GomsCustomerId, req.SmartPitCardNo) };
                    else
                        resp = new TFCtvResponse() { Code = -1005, Message = "Unable to update SmartPit Card." };
                }
                catch (Exception e)
                {
                    resp = new TFCtvResponse() { Code = -3000, Message = e.InnerException.Message };
                }
            }
            return resp;
        }

        [WebMethod(Description = "Create TFC.tv Everywhere Entitlement")]
        [SoapHeader("Credentials")]
        public TFCtvResponse CreateTFCtvEverywhereEntitlement(ReqCreateTFCtvEverywhereEntitlement req)
        {
            TFCtvResponse resp = null;
            DateTime registDt = DateTime.Now;
            string ip = ConfigurationManager.AppSettings["IpWhiteList"];
            string[] IpAddresses = ip.Split(';');
            bool isWhitelisted = IpAddresses.Contains(HttpContext.Current.Request.UserHostAddress);
            if (!isWhitelisted)
            {
                resp = new TFCtvResponse() { Code = -3001, Message = "Ip address is unauthorized." }; return resp;
            }

            if (!HttpContext.Current.Request.IsLocal)
            {
                //Check SoapHeader
                if (Credentials.Username.ToLower() != SoapHeaderUsername || Credentials.Password != SoapHeaderPassword)
                { resp = new TFCtvResponse() { Code = -3002, Message = "Call is unauthorized." }; return resp; }
            }

            if (req == null)
                resp = new TFCtvResponse() { Code = -3003, Message = "Request parameter is empty." };
            else
            {
                if (req.GomsCustomerId == null || req.GomsTransactionDate == null || req.GomsTransactionId == null || req.GomsProductId == null || req.GomsProductQuantity == null || req.GomsTFCEverywhereEndDate == null || req.GomsTFCEverywhereStartDate == null || req.EmailAddress == null)
                {
                    resp = new TFCtvResponse() { Code = -3004, Message = "Missing required fields." };
                    return resp;
                }

                if (req.GomsProductId != null)
                    if (req.GomsProductId <= 0)
                    {
                        resp = new TFCtvResponse() { Code = -1000, Message = "GomsProductId is not applicable." };
                        return resp;
                    }

                var context = new IPTV2Entities();
                if (isProduction)
                    context.Database.Connection.ConnectionString = IPTV2EntitiesAzureConnectionString;


                var product = context.Products.FirstOrDefault(p => p.GomsProductId == req.GomsProductId && p.GomsProductQuantity == req.GomsProductQuantity);
                if (product == null)
                {
                    resp = new TFCtvResponse() { Code = -1007, Message = "Product does not exist." };
                    return resp;
                }

                var user = context.Users.FirstOrDefault(u => u.GomsCustomerId == req.GomsCustomerId && String.Compare(u.EMail, req.EmailAddress, true) == 0);
                if (user == null)
                {
                    resp = new TFCtvResponse() { Code = -1001, Message = "User does not exist." };
                    return resp;
                }

                //if (String.Compare(user.EMail, req.EmailAddress, true) != 0)
                //{
                //    resp = new TFCtvResponse() { Code = -1001, Message = "User does not exist." };
                //    return resp;
                //}

                if (user.Country == null)
                {
                    resp = new TFCtvResponse() { Code = -1002, Message = "User country is not valid." };
                    return resp;
                }
                //else // Do we need to check for Japan/US only?
                //{
                //    var countries = TVECountryWhitelist.Split(',');
                //    if (!countries.Contains(user.Country.Code))
                //    {
                //        resp = new TFCtvResponse() { Code = -1002, Message = "User country is not valid." };
                //        return resp;
                //    }
                //}

                if (user.IsTVEverywhere == null)
                {
                    resp = new TFCtvResponse() { Code = -1003, Message = "User is not applicable for TVEverywhere." };
                    return resp;
                }
                else
                {
                    if (user.IsTVEverywhere == false)
                    {
                        resp = new TFCtvResponse() { Code = -1003, Message = "User is not applicable for TVEverywhere." };
                        return resp;
                    }
                }

                var match = System.Text.RegularExpressions.Regex.Match(req.Reference, "Update|Deactivate|Activate|Change Plan", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    if (user.Transactions.Count(t => t.GomsTransactionId == req.GomsTransactionId) > 0)
                    {
                        resp = new TFCtvResponse() { Code = -1006, Message = "Transaction already exists in TFC.tv." };
                        return resp;
                    }
                }


                if (user != null)
                {
                    //user.LastUpdated = registDt;
                    user.LastUpdated = (DateTime)req.GomsTransactionDate;

                    if (user.IsTVEverywhere != null) // Check if set to true
                        if (user.IsTVEverywhere == false)
                            user.IsTVEverywhere = true;

                    ProductPrice productPrice;
                    try
                    {
                        productPrice = product.ProductPrices.FirstOrDefault(i => i.CurrencyCode == user.Country.CurrencyCode);
                    }
                    catch (Exception)
                    {
                        productPrice = product.ProductPrices.FirstOrDefault(i => i.CurrencyCode == DefaultCurrencyCode);
                    }

                    //Create Purchase
                    Purchase purchase = CreatePurchase((DateTime)req.GomsTransactionDate, "TFC Everywhere");
                    //Create Purchase Item
                    PurchaseItem purchaseItem = CreatePurchaseItem(user.UserId, product, productPrice);

                    var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId);
                    //Create Entitlement & EntitlementRequest
                    Entitlement entitlement = user.PackageEntitlements.FirstOrDefault(i => i.PackageId == productPackage.PackageId);

                    //DateTime endDate = registDt;
                    DateTime endDate = (DateTime)req.GomsTFCEverywhereEndDate;


                    if (entitlement != null)
                    {
                        //if (entitlement.EndDate > registDt)
                        //    entitlement.EndDate = entitlement.EndDate.Add(difference);
                        //else
                        //    entitlement.EndDate = registDt.Add(difference);

                        entitlement.EndDate = (DateTime)req.GomsTFCEverywhereEndDate;
                        EntitlementRequest request = new EntitlementRequest()
                        {
                            //DateRequested = registDt,
                            DateRequested = (DateTime)req.GomsTransactionDate,
                            StartDate = (DateTime)req.GomsTFCEverywhereStartDate,
                            EndDate = entitlement.EndDate,
                            Product = productPackage.Product,
                            Source = "TFC Everywhere",
                            ReferenceId = req.Reference
                        };

                        endDate = entitlement.EndDate;
                        user.EntitlementRequests.Add(request);
                        entitlement.LatestEntitlementRequest = request;
                    }
                    else
                    {
                        EntitlementRequest request = new EntitlementRequest()
                        {
                            //DateRequested = registDt,
                            DateRequested = (DateTime)req.GomsTransactionDate,
                            StartDate = (DateTime)req.GomsTFCEverywhereStartDate,
                            EndDate = (DateTime)req.GomsTFCEverywhereEndDate,
                            Product = productPackage.Product,
                            Source = "TFC Everywhere",
                            ReferenceId = req.Reference
                        };

                        PackageEntitlement pkg_entitlement = new PackageEntitlement()
                        {
                            EndDate = (DateTime)req.GomsTFCEverywhereEndDate,
                            Package = (IPTV2_Model.Package)productPackage.Package,
                            OfferingId = offeringId,
                            LatestEntitlementRequest = request
                        };

                        //endDate = item.LicenseEndDate;

                        user.PackageEntitlements.Add(pkg_entitlement);
                    }

                    //Create TFCtvEverywhereTransaction
                    TfcEverywhereTransaction transaction = new TfcEverywhereTransaction()
                    {
                        GomsTFCEverywhereEndDate = (DateTime)req.GomsTFCEverywhereEndDate,
                        GomsTFCEverywhereStartDate = (DateTime)req.GomsTFCEverywhereStartDate,
                        GomsTFCEverywhereSubscriptionId = "N/A",
                        GomsTFCEverywhereServiceId = "N/A",
                        GomsTransactionDate = req.GomsTransactionDate,
                        GomsTransactionId = req.GomsTransactionId,
                        Amount = productPrice.Amount,
                        Currency = productPrice.CurrencyCode,
                        //Date = registDt,
                        Date = (DateTime)req.GomsTransactionDate,
                        Reference = req.Reference,
                        OfferingId = offeringId,
                        StatusId = 1
                    };

                    user.Transactions.Add(transaction);
                }

                try
                {
                    if (context.SaveChanges() > 0)
                        resp = new TFCtvResponse() { Code = 0, Message = String.Format("Successfully created entitlement ending {0} to GomsCustomerId {1}.", req.GomsTFCEverywhereEndDate, req.GomsCustomerId) };
                    else
                        resp = new TFCtvResponse() { Code = -1005, Message = "Unable to create TFC.tv Everywhere entitlement." };
                }
                catch (Exception e)
                {
                    resp = new TFCtvResponse() { Code = -3000, Message = e.InnerException.Message };
                }

            }
            return resp;
        }


        [WebMethod(Description = "Unassociate TFC.tv Everywhere")]
        [SoapHeader("Credentials")]
        public TFCtvResponse UnassociateTVEverywhere(ReqUnassociateTVEverywhere req)
        {
            TFCtvResponse resp = null;
            DateTime registDt = DateTime.Now;
            string ip = ConfigurationManager.AppSettings["IpWhiteList"];
            string[] IpAddresses = ip.Split(';');
            bool isWhitelisted = IpAddresses.Contains(HttpContext.Current.Request.UserHostAddress);
            if (!isWhitelisted)
            {
                resp = new TFCtvResponse() { Code = -3001, Message = "Ip address is unauthorized." }; return resp;
            }

            if (!HttpContext.Current.Request.IsLocal)
            {
                //Check SoapHeader
                if (Credentials.Username.ToLower() != SoapHeaderUsername || Credentials.Password != SoapHeaderPassword)
                { resp = new TFCtvResponse() { Code = -3002, Message = "Call is unauthorized." }; return resp; }
            }

            if (req == null)
                resp = new TFCtvResponse() { Code = -3003, Message = "Request parameter is empty." };
            else
            {
                if (req.GomsCustomerId == null || req.GomsTransactionDate == null || req.GomsTransactionId == null)
                {
                    resp = new TFCtvResponse() { Code = -3004, Message = "Missing required fields." };
                    return resp;
                }

                var context = new IPTV2Entities();
                if (isProduction)
                    context.Database.Connection.ConnectionString = IPTV2EntitiesAzureConnectionString;


                var user = context.Users.FirstOrDefault(u => u.GomsCustomerId == req.GomsCustomerId);
                if (user == null)
                {
                    resp = new TFCtvResponse() { Code = -1001, Message = "User does not exist." };
                    return resp;
                }

                if (user.Country == null)
                {
                    resp = new TFCtvResponse() { Code = -1002, Message = "User country is not valid." };
                    return resp;
                }
                //else // Do we need to check for Japan/US only?
                //{
                //    var countries = TVECountryWhitelist.Split(',');
                //    if (!countries.Contains(user.Country.Code))
                //    {
                //        resp = new TFCtvResponse() { Code = -1002, Message = "User country is not valid." };
                //        return resp;
                //    }
                //}

                var product = context.Products.FirstOrDefault(p => p.GomsProductId == req.GomsProductId && p.GomsProductQuantity == req.GomsProductQuantity);
                if (product == null)
                {
                    resp = new TFCtvResponse() { Code = -1007, Message = "GOMS Product does not exist." };
                    return resp;
                }

                if (user.IsTVEverywhere == null)
                {
                    resp = new TFCtvResponse() { Code = -1003, Message = "User is not TFC.tv Everywhere associated." };
                    return resp;
                }
                else
                {
                    if (user.IsTVEverywhere == false)
                    {
                        resp = new TFCtvResponse() { Code = -1003, Message = "User is not TFC.tv Everywhere associated." };
                        return resp;
                    }
                }

                //if (user.Transactions.Count(t => t.GomsTransactionId == req.GomsTransactionId) > 0)
                //{
                //    resp = new TFCtvResponse() { Code = -1006, Message = "Transaction already exists in TFC.tv." };
                //    return resp;
                //}


                if (user != null)
                {

                    var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId);
                    if (productPackage == null)
                    {
                        resp = new TFCtvResponse() { Code = -1006, Message = "TFC.tv package does not exist." };
                        return resp;
                    }

                    var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == productPackage.Package.PackageId);
                    if (entitlement != null)
                        entitlement.EndDate = (DateTime)req.GomsTransactionDate;

                    //user.LastUpdated = registDt;
                    user.LastUpdated = (DateTime)req.GomsTransactionDate;
                    user.IsTVEverywhere = false; // Set to false

                    string CurrencyCode = DefaultCurrencyCode;
                    Country country = context.Countries.FirstOrDefault(c => c.Code == user.CountryCode);
                    if (country != null)
                    {
                        Currency currency = context.Currencies.FirstOrDefault(c => c.Code == country.CurrencyCode);
                        if (currency != null) CurrencyCode = currency.Code;
                    }

                    var transaction = user.Transactions.LastOrDefault(t => t.GetType() != typeof(TfcEverywhereTransaction) && t.GomsTransactionId == req.GomsTransactionId);
                    if (transaction == null)
                    {
                        resp = new TFCtvResponse() { Code = -1004, Message = "TFC.tv Everywhere transaction not found." };
                        return resp;
                    }
                    var cancellation = new CancellationTransaction()
                    {
                        CancellationRemarks = req.Reference,
                        GomsTransactionId = -1000,
                        GomsTransactionDate = req.GomsTransactionDate,
                        OriginalTransactionId = transaction.TransactionId,
                        StatusId = 1,
                        OfferingId = offeringId,
                        Date = (DateTime)req.GomsTransactionDate,
                        Amount = 0,
                        Currency = CurrencyCode,
                        Reference = req.Reference
                    };

                    user.Transactions.Add(cancellation);
                }

                try
                {
                    if (context.SaveChanges() > 0)
                        resp = new TFCtvResponse() { Code = 0, Message = String.Format("Successfully unassociated TFC.tv Everywhere from GomsCustomerId {0}.", req.GomsCustomerId) };
                    else
                        resp = new TFCtvResponse() { Code = -1005, Message = "Unable to unassociate TFC.tv Everywhere." };
                }
                catch (Exception e)
                {
                    resp = new TFCtvResponse() { Code = -3000, Message = e.InnerException.Message };
                }

            }
            return resp;
        }

        private static PackageEntitlement CreatePackageEntitlement(EntitlementRequest request, PackageSubscriptionProduct subscription, ProductPackage package, DateTime endDate, int offeringId)
        {
            PackageEntitlement entitlement = new PackageEntitlement()
            {
                EndDate = endDate,
                Package = (IPTV2_Model.Package)package.Package,
                OfferingId = offeringId,
                LatestEntitlementRequest = request
            };
            return entitlement;
        }



        private static EntitlementRequest CreateEntitlementRequest(DateTime registDt, DateTime endDate, Product product, string source, string reference)
        {
            EntitlementRequest request = new EntitlementRequest()
            {
                DateRequested = registDt,
                EndDate = endDate,
                Product = product,
                Source = source,
                ReferenceId = reference
            };
            return request;
        }

        private static PurchaseItem CreatePurchaseItem(System.Guid userId, Product product, ProductPrice priceOfProduct)
        {
            PurchaseItem item = new PurchaseItem()
            {
                RecipientUserId = userId,
                ProductId = product.ProductId,
                Price = priceOfProduct.Amount,
                Currency = priceOfProduct.CurrencyCode,
                Remarks = product.Name
            };
            return item;
        }

        private static Purchase CreatePurchase(DateTime registDt, string remarks)
        {
            Purchase purchase = new Purchase()
            {
                Date = registDt,
                Remarks = remarks
            };
            return purchase;
        }
    }

    [Serializable]
    public class ReqActivatePpc
    {
        public string PpcStart { get; set; }

        public string PpcEnd { get; set; }

        public string ActivatedBy { get; set; }

        public DateTime? ActivatedOn { get; set; }

        public int? StatusId { get; set; }
    }

    [Serializable]
    public class ReqReloadWalletViaSmartPit
    {
        public int? GomsWalletId { get; set; }

        public int? GomsCustomerId { get; set; }

        public DateTime? GomsTransactionDate { get; set; }

        public int? GomsTransactionId { get; set; }

        public decimal? Amount { get; set; }
    }

    [Serializable]
    public class ReqUpdateSmartPit
    {
        public int? GomsCustomerId { get; set; }

        public string SmartPitCardNo { get; set; }
    }

    [Serializable]
    public class ReqCreateTFCtvEverywhereEntitlement
    {
        public int? GomsProductId { get; set; }
        public int? GomsProductQuantity { get; set; }
        public int? GomsCustomerId { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? GomsTransactionDate { get; set; }
        public int? GomsTransactionId { get; set; }
        public string Reference { get; set; }
        public DateTime? GomsTFCEverywhereStartDate { get; set; }
        public DateTime? GomsTFCEverywhereEndDate { get; set; }

    }

    [Serializable]
    public class ReqUnassociateTVEverywhere
    {
        public int? GomsCustomerId { get; set; }
        public DateTime? GomsTransactionDate { get; set; }
        public int? GomsTransactionId { get; set; }
        public string Reference { get; set; }
        public int? GomsProductId { get; set; }
        public int? GomsProductQuantity { get; set; }
    }

    [Serializable]
    public class TFCtvResponse
    {
        public int Code { get; set; }

        public string Message { get; set; }
    }

    public class AuthenticationHeader : SoapHeader
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}