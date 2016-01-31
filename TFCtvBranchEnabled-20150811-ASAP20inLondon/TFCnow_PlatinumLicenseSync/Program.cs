using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFCNowModel;
using IPTV2_Model;
using System.Diagnostics;
using System.Threading;
using System.Configuration;



namespace TFCnow_PlatinumLicenseSync
{
    class Program
    {

        public static int OfferingId = Convert.ToInt32(ConfigurationManager.AppSettings["OfferingId"]);
        public static int PlatinumProductId = Convert.ToInt32(ConfigurationManager.AppSettings["PlatinumProductId"]);
        public static int TFCnowPlatinumProductId = Convert.ToInt32(ConfigurationManager.AppSettings["TFCnowPlatinumProductId"]);
        public static string DefaultCurrencyCode = ConfigurationManager.AppSettings["DefaultCurrencyCode"];
        public static int PremiumPackageId = Convert.ToInt32(ConfigurationManager.AppSettings["PremiumPackageId"]);

        public static bool IsExtendPlatinumEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["IsExtendPlatinumEnabled"]);
        public static bool IsExtendTFCnowEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["IsExtendTFCnowEnabled"]);
        public static int duration = Convert.ToInt32(ConfigurationManager.AppSettings["waitDuration"]);

        static void Main(string[] args)
        {
            int waitDuration = duration;
            int ctr = 1;
            while (true)
            {
                try
                {
                    if (ctr != 1)
                        Thread.Sleep(TimeSpan.FromSeconds(waitDuration));
                    Console.WriteLine(String.Format("Run #{0}. Start.", ctr));
                    if (IsExtendPlatinumEnabled)
                        RunThis();
                    if (IsExtendTFCnowEnabled)
                        ExtendTFCnowLicenses();
                    Console.WriteLine(String.Format("Run #{0}. Complete.", ctr));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.InnerException.Message);
                    Trace.TraceError("fail in Run", ex);
                }
                ctr++;
            }

        }

        private static void ExtendTFCnowLicenses()
        {
            DateTime registDt = DateTime.Now;
            var absnow_context = new ABSNowEntities();
            var context = new IPTV2Entities();

            var migrated_users = context.Users.Where(i => !String.IsNullOrEmpty(i.TfcNowUserName)).OrderBy(i => i.EMail);

            Console.WriteLine(String.Format("Total migrated users: {0}", migrated_users.Count()));
            int ctr = 1;
            foreach (var user in migrated_users)
            {
                //Get Entitlements
                Console.WriteLine(String.Format("{1} TFC.tv Email Address: {0}", user.EMail, ctr));
                Console.WriteLine(String.Format("TFCnow Email Address: {0}", user.TfcNowUserName));
                ctr++;
                var entitlements = user.PackageEntitlements.Where(i => i.EndDate > registDt);
                if (entitlements != null)
                {
                    foreach (var item in entitlements)
                    {
                        Console.WriteLine(String.Format("Package Id: {0}", item.PackageId));
                        Console.WriteLine(String.Format("Expiration Date: {0}", item.EndDate));
                        int TFCnowPackageId = GetTFCnowPackageId(item.PackageId);
                        absnow_context.TFCnowExtendLicense(user.TfcNowUserName, TFCnowPackageId, item.EndDate);
                    }
                }
                //absnow_context.SaveChanges();
            }
        }

        private static int GetTFCnowPackageId(int value)
        {
            switch (value)
            {
                case 47: return 1427;  //Premium
                case 48: return 1424;  //Lite
                case 49: return 45;  //Movie Channel
                default: return 1427;
            }
        }

        private static void RunThis()
        {
            DateTime registDt = DateTime.Now;
            var absnow_context = new ABSNowEntities();
            var context = new IPTV2Entities();

            var processed_licenses = absnow_context.LicensePurchasedProcesseds.ToList();
            Console.WriteLine(String.Format("Processed Licenses: {0}", processed_licenses.Count()));
            //var platinum_users = absnow_context.CustomerLicensePurchased1.Where(i => i.PackageID == TFCnowPlatinumProductId && i.LicenseEndDate > registDt) // && i.EmailAddress == "a1lyn.galang@gmail.com")
            //    .OrderByDescending(i => i.LicenseEndDate)
            //    .AsEnumerable()
            //    .GroupBy(i => i.EmailAddress)
            //     .Select(i => new
            //     {
            //         EmailAddress = i.Key,
            //         PackageID = i.Max(ii => ii.PackageID),
            //         CustomerID = i.Max(ii => ii.CustomerID),
            //         LicenseEndDate = i.Max(ii => ii.LicenseEndDate)
            //     }).ToList();

            var processedLicenses = processed_licenses.Select(i => i.LicensePurchasedID).ToArray();
            absnow_context.CommandTimeout = 120;
            var platinum_users = absnow_context.vwCustomerLicensePurchaseds.Where(i => !processedLicenses.Contains(i.LicensePurchasedID)).ToList();
            Console.WriteLine(String.Format("Platinum Users: {0}", platinum_users.Count()));
            var productPlatinum = context.Products.FirstOrDefault(i => i.ProductId == PlatinumProductId);
            var PremiumPackage = context.ProductPackages.FirstOrDefault(i => i.PackageId == PremiumPackageId);

            ProductPrice productPrice;
            if (platinum_users != null)
            {
                int counter = 1;
                foreach (var item in platinum_users)
                {
                    Console.WriteLine(String.Format("{1}. PROCESSING {0}", item.EmailAddress, counter));
                    //var purchaseId = absnow_context.CustomerLicensePurchased1.Where(i => i.EmailAddress.ToLower() == item.EmailAddress.ToLower() && i.PackageID == TFCnowPlatinumProductId).OrderByDescending(i => i.LicenseEndDate).First();
                    Console.WriteLine(String.Format("LicensePurchase ID: {0}", item.LicensePurchasedID));
                    counter++;
                    //if (!processed_licenses.Select(i => i.LicensePurchasedID).Contains(purchaseId.LicensePurchasedID))
                    //{
                    var difference = item.LicenseEndDate.Subtract(registDt);

                    var user = context.Users.FirstOrDefault(i => i.TfcNowUserName.ToLower() == item.EmailAddress.ToLower());
                    if (user != null)
                    {
                        Console.WriteLine(String.Format("TAGGED TFCnow {0} TO TFCtv {1}", item.EmailAddress, user.EMail));
                        try
                        {
                            productPrice = productPlatinum.ProductPrices.FirstOrDefault(i => i.CurrencyCode == user.Country.CurrencyCode);
                        }
                        catch (Exception)
                        {
                            productPrice = productPlatinum.ProductPrices.FirstOrDefault(i => i.CurrencyCode == DefaultCurrencyCode);
                        }

                        user.LastUpdated = registDt;


                        //Create Purchase
                        Purchase purchase = CreatePurchase(registDt, "TFC.tv Everywhere");
                        //Create Purchase Item
                        PurchaseItem purchaseItem = CreatePurchaseItem(user.UserId, productPlatinum, productPrice);

                        //Create Entitlement & EntitlementRequest
                        Entitlement entitlement = user.PackageEntitlements.FirstOrDefault(i => i.PackageId == PremiumPackageId);

                        DateTime endDate = registDt;

                        if (entitlement != null)
                        {

                            if (entitlement.EndDate > registDt)
                                entitlement.EndDate = entitlement.EndDate.Add(difference);
                            else
                                entitlement.EndDate = registDt.Add(difference);

                            //entitlement.EndDate = item.LicenseEndDate;

                            EntitlementRequest request = new EntitlementRequest()
                            {
                                DateRequested = registDt,
                                EndDate = entitlement.EndDate,
                                Product = PremiumPackage.Product,
                                Source = "TFC.tv Everywhere",
                                ReferenceId = String.Format("{0}", item.LicensePurchasedID)
                                //ReferenceId = String.Format("{0}", String.Empty)
                            };

                            endDate = entitlement.EndDate;

                            user.EntitlementRequests.Add(request);
                        }
                        else
                        {

                            EntitlementRequest request = new EntitlementRequest()
                            {
                                DateRequested = registDt,
                                EndDate = item.LicenseEndDate,
                                Product = PremiumPackage.Product,
                                Source = "TFC.tv Everywhere",
                                ReferenceId = String.Format("{0}", item.LicensePurchasedID)
                                //ReferenceId = String.Format("{0}", String.Empty)
                            };

                            PackageEntitlement pkg_entitlement = new PackageEntitlement()
                            {
                                EndDate = item.LicenseEndDate,
                                Package = (IPTV2_Model.Package)PremiumPackage.Package,
                                OfferingId = OfferingId,
                                LatestEntitlementRequest = request
                            };

                            //endDate = item.LicenseEndDate;

                            user.PackageEntitlements.Add(pkg_entitlement);

                        }

                        //Create TFCtvEverywhereTransaction
                        TfcEverywhereTransaction transaction = new TfcEverywhereTransaction()
                        {
                            GomsTFCEverywhereEndDate = endDate,
                            GomsTFCEverywhereStartDate = registDt,
                            GomsTFCEverywhereSubscriptionId = "N/A",
                            GomsTFCEverywhereServiceId = "N/A",
                            Amount = productPrice.Amount,
                            Currency = productPrice.CurrencyCode,
                            Date = registDt,
                            Reference = String.Format("TVE-{0}", item.TransactionID),
                            OfferingId = OfferingId,
                            StatusId = 1
                        };

                        user.Transactions.Add(transaction);

                        //Console.WriteLine(String.Format("TFCtv {0} LICENSE END DATE UPDATED", user.EMail));

                        if (context.SaveChanges() > 0)
                        {
                            Console.WriteLine(String.Format("TFCtv {0} LICENSE END DATE UPDATED", user.EMail));
                            LicensePurchasedProcessed receipt = new LicensePurchasedProcessed();
                            receipt.LicensePurchasedID = item.LicensePurchasedID;
                            receipt.CreateDate = registDt;
                            absnow_context.LicensePurchasedProcesseds.AddObject(receipt);
                            absnow_context.SaveChanges();
                        }
                    }
                    else
                        Console.WriteLine(String.Format("{0} NOT FOUND. SKIPPING.", item.EmailAddress));

                    //}
                }

                //absnow_context.SaveChanges();
            }
        }

        private static PackageEntitlement CreatePackageEntitlement(EntitlementRequest request, PackageSubscriptionProduct subscription, ProductPackage package, DateTime registDt)
        {
            PackageEntitlement entitlement = new PackageEntitlement()
            {
                EndDate = getEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
                Package = (IPTV2_Model.Package)package.Package,
                OfferingId = OfferingId,
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

        private static DateTime getEntitlementEndDate(int duration, string interval, DateTime registDt)
        {
            DateTime d = DateTime.Now;
            switch (interval)
            {
                case "d": d = registDt.AddDays(duration); break;
                case "m": d = registDt.AddMonths(duration); break;
                case "y": d = registDt.AddYears(duration); break;
                case "h": d = registDt.AddHours(duration); break;
                case "mm": d = registDt.AddMinutes(duration); break;
                case "s": d = registDt.AddSeconds(duration); break;
            }
            return d;
        }

        private static string ConvertToString(Guid value)
        {
            return value.ToString();
        }
    }
}
