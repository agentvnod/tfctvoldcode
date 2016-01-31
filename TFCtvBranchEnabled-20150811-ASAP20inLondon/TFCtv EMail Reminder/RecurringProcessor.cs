using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using System.Diagnostics;
using IPTV2_Model;
using GOMS_TFCtv;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Configuration;

namespace TFCtv_EMail_Reminder
{
    public class RecurringProcessor : IJob
    {
        static int offeringId = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("offeringId"));
        static string DefaultCurrencyCode = RoleEnvironment.GetConfigurationSettingValue("DefaultCurrencyCode");
        static DateTime registDt;

        public RecurringProcessor()
        {
            registDt = DateTime.Now;
        }

        public void Execute(IJobExecutionContext context)
        {
            Trace.TraceInformation("Recurring Billing is executing. Time now is " + DateTime.UtcNow);
            Send();
        }

        //private void Send()
        //{
        //    //Trace.WriteLine(context.Database.Connection.ConnectionString);
        //    try
        //    {
        //        var context = new IPTV2Entities();
        //        context.Database.Connection.ConnectionString = RoleEnvironment.GetConfigurationSettingValue("Iptv2Entities");
        //        Trace.TraceInformation("Fetching users for recurring...");
        //        DateTime dtRecur = registDt.Date.AddDays(Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("numOfDaysRecurringProcess")));
        //        var recurringBillings = context.RecurringBillings.Where(r => r.StatusId == 1 && r.NextRun < dtRecur);
        //        Trace.TraceInformation(String.Format("Total Users For Recurring: {0}", recurringBillings.Count()));

        //        if (recurringBillings != null)
        //        {                    
        //            var gomsService = new GomsTfcTv();

        //            //Set Goms Values via Azure                    
        //            gomsService.UserId = RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvUserId");
        //            gomsService.Password = RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvPassword");
        //            gomsService.ServiceUserId = RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvServiceUserId");
        //            gomsService.ServicePassword = RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvServicePassword");
        //            gomsService.ServiceUrl = RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvServiceUrl");
        //            gomsService.ServiceId = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvServiceId"));
        //            gomsService.GSapikey = RoleEnvironment.GetConfigurationSettingValue("GSapikey");
        //            gomsService.GSsecretkey = RoleEnvironment.GetConfigurationSettingValue("GSsecretkey");

        //            foreach (var i in recurringBillings)
        //            {
        //                Trace.TraceInformation(String.Format("Processing user {0} with productId {1}, endDate {2}", i.User.EMail, i.Product.Description, i.EndDate));
        //                try
        //                {
        //                    ProductPrice priceOfProduct = i.Product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == i.User.Country.CurrencyCode);
        //                    if (priceOfProduct == null)
        //                        priceOfProduct = i.Product.ProductPrices.FirstOrDefault(p => p.CurrencyCode == DefaultCurrencyCode);

        //                    Purchase purchase = CreatePurchase(registDt, "Payment via Credit Card");
        //                    i.User.Purchases.Add(purchase);

        //                    PurchaseItem item = CreatePurchaseItem(i.UserId, i.Product, priceOfProduct);
        //                    purchase.PurchaseItems.Add(item);

        //                    var cardType = i.User.CreditCards.LastOrDefault(c => c.StatusId == 1).CardType;
        //                    CreditCardPaymentTransaction transaction = new CreditCardPaymentTransaction()
        //                    {
        //                        Amount = priceOfProduct.Amount,
        //                        Currency = priceOfProduct.CurrencyCode,
        //                        Reference = cardType.ToUpper(),
        //                        Date = registDt,
        //                        Purchase = purchase,
        //                        OfferingId = offeringId,
        //                        StatusId = 1
        //                    };
        //                    var response = gomsService.CreateOrderViaRecurringPayment(context, i.UserId, transaction);
        //                    if (response.IsSuccess)
        //                    {
        //                        DateTime endDate = registDt;
        //                        item.SubscriptionProduct = (SubscriptionProduct)i.Product;
        //                        if (item.SubscriptionProduct is PackageSubscriptionProduct)
        //                        {
        //                            PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)i.Product;

        //                            foreach (var package in subscription.Packages)
        //                            {
        //                                string packageName = package.Package.Description;
        //                                string ProductNameBought = packageName;

        //                                PackageEntitlement currentPackage = i.User.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);

        //                                EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, i.Product, String.Format("{0}-{1}", "CC", cardType), response.TransactionId.ToString(), registDt);
        //                                if (currentPackage != null)
        //                                {
        //                                    request.StartDate = currentPackage.EndDate;
        //                                    currentPackage.EndDate = GetEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));

        //                                    endDate = currentPackage.EndDate;
        //                                    currentPackage.LatestEntitlementRequest = request;
        //                                    request.EndDate = endDate;
        //                                }
        //                                else
        //                                {
        //                                    PackageEntitlement entitlement = CreatePackageEntitlement(request, subscription, package, registDt);
        //                                    request.EndDate = entitlement.EndDate;
        //                                    i.User.PackageEntitlements.Add(entitlement);
        //                                }
        //                                i.User.EntitlementRequests.Add(request);
        //                                item.EntitlementRequest = request; //UPDATED: November 22, 2012                                        
        //                            }

        //                            i.EndDate = endDate;
        //                            i.NextRun = endDate.AddDays(-3).Date;
        //                            i.UpdatedOn = registDt;

        //                        }
        //                    }
        //                    else
        //                        throw new Exception(response.StatusMessage);
        //                }
        //                catch (Exception e)
        //                {
        //                    Trace.TraceError(e.Message);
        //                    i.GomsRemarks = e.Message;
        //                    //    TFCTV.Helpers.MyUtility.LogException(e, "Recurring Billing Scheduled Task"); 
        //                }
        //            }
        //            context.SaveChanges();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //TFCTV.Helpers.MyUtility.LogException(e);
        //        Trace.TraceInformation("Error: " + e.Message);
        //        Trace.TraceInformation("Inner Exception: " + e.InnerException.Message);
        //    }
        //}


        private void Send()
        {
            //Trace.WriteLine(context.Database.Connection.ConnectionString);
            try
            {
                Trace.TraceInformation("Fetching users for recurring...");
                DateTime dtRecur = registDt.Date.AddDays(Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("numOfDaysRecurringProcess")));
                var recurringBillings = GetUsersEligibleForRenewal(dtRecur);
                Trace.TraceInformation(String.Format("Total Users For Recurring: {0}", recurringBillings.Count()));

                if (recurringBillings != null)
                {
                    using (var context = new IPTV2Entities())
                    {
                        context.Database.Connection.ConnectionString = RoleEnvironment.GetConfigurationSettingValue("Iptv2Entities");
                        var gomsService = new GomsTfcTv();
                        SetGomsServiceVariables(gomsService);
                        try
                        {
                            gomsService.TestConnect();
                            Console.WriteLine("Test Connect success");
                        }
                        catch (Exception) { Console.WriteLine("Test Connect failed."); }

                        foreach (var i in recurringBillings)
                        {
                            var user = context.Users.FirstOrDefault(u => u.UserId == i.UserId);
                            var recurringBilling = context.RecurringBillings.Find(i.RecurringBillingId);
                            var product = context.Products.Find(i.ProductId);
                            Trace.TraceInformation(String.Format("Processing user {0} with productId {1}, endDate {2}", user.EMail, product.Description, i.EndDate));
                            try
                            {
                                ProductPrice priceOfProduct = context.ProductPrices.FirstOrDefault(p => p.CurrencyCode == user.Country.CurrencyCode && p.ProductId == product.ProductId);
                                if (priceOfProduct == null)
                                    priceOfProduct = context.ProductPrices.FirstOrDefault(p => p.CurrencyCode == DefaultCurrencyCode && p.ProductId == product.ProductId);

                                Purchase purchase = CreatePurchase(registDt, "Payment via Credit Card");
                                user.Purchases.Add(purchase);

                                PurchaseItem item = CreatePurchaseItem(user.UserId, product, priceOfProduct);
                                purchase.PurchaseItems.Add(item);

                                var cardType = user.CreditCards.LastOrDefault(c => c.StatusId == 1).CardType;
                                CreditCardPaymentTransaction transaction = new CreditCardPaymentTransaction()
                                {
                                    Amount = priceOfProduct.Amount,
                                    Currency = priceOfProduct.CurrencyCode,
                                    Reference = cardType.ToUpper(),
                                    Date = registDt,
                                    Purchase = purchase,
                                    OfferingId = offeringId,
                                    StatusId = 1
                                };

                                var response = gomsService.CreateOrderViaRecurringPayment(context, user.UserId, transaction);
                                if (response.IsSuccess)
                                {
                                    DateTime endDate = registDt;
                                    item.SubscriptionProduct = (SubscriptionProduct)product;
                                    if (item.SubscriptionProduct is PackageSubscriptionProduct)
                                    {
                                        PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;

                                        foreach (var package in subscription.Packages)
                                        {
                                            string packageName = package.Package.Description;
                                            string ProductNameBought = packageName;

                                            PackageEntitlement currentPackage = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == package.PackageId);

                                            EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, product, String.Format("{0}-{1}", "CC", cardType), response.TransactionId.ToString(), registDt);
                                            if (currentPackage != null)
                                            {
                                                request.StartDate = currentPackage.EndDate;
                                                currentPackage.EndDate = GetEntitlementEndDate(subscription.Duration, subscription.DurationType, ((currentPackage.EndDate > registDt) ? currentPackage.EndDate : registDt));

                                                endDate = currentPackage.EndDate;
                                                currentPackage.LatestEntitlementRequest = request;
                                                request.EndDate = endDate;
                                            }
                                            else
                                            {
                                                PackageEntitlement entitlement = CreatePackageEntitlement(request, subscription, package, registDt);
                                                request.EndDate = entitlement.EndDate;
                                                user.PackageEntitlements.Add(entitlement);
                                            }
                                            user.EntitlementRequests.Add(request);
                                            item.EntitlementRequest = request; //UPDATED: November 22, 2012                                        
                                        }

                                        recurringBilling.EndDate = endDate;
                                        recurringBilling.NextRun = endDate.AddDays(-3).Date;
                                        recurringBilling.UpdatedOn = registDt;
                                        recurringBilling.GomsRemarks = String.Empty;
                                        recurringBilling.NumberOfAttempts = 0;

                                    }
                                    Trace.TraceInformation(user.EMail + ": renewal process complete!");
                                }
                                else
                                {
                                    recurringBilling.GomsRemarks = response.StatusMessage;
                                    recurringBilling.NumberOfAttempts += 1;
                                    throw new Exception(user.EMail + ": " + response.StatusMessage);
                                }
                            }
                            catch (Exception e)
                            {
                                Trace.TraceError("INNER ERROR: " + e.Message);
                            }

                            if (context.SaveChanges() > 0)
                                Trace.TraceInformation("Saving changes...");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("ERROR: " + e.Message);
            }
        }


        private void SetGomsServiceVariables(GomsTfcTv gomsService)
        {
            gomsService.UserId = RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvUserId");
            gomsService.Password = RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvPassword");
            gomsService.ServiceUserId = RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvServiceUserId");
            gomsService.ServicePassword = RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvServicePassword");
            gomsService.ServiceUrl = RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvServiceUrl");
            gomsService.ServiceId = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("GomsTfcTvServiceId"));
            gomsService.GSapikey = RoleEnvironment.GetConfigurationSettingValue("GSapikey");
            gomsService.GSsecretkey = RoleEnvironment.GetConfigurationSettingValue("GSsecretkey");
        }

        private List<RecurringBilling> GetUsersEligibleForRenewal(DateTime dtRecur)
        {
            var context = new IPTV2Entities();
            var numberOfAttempts = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("numberOfAttempts"));
            var recurringBillings = context.RecurringBillings.Where(r => r.StatusId == 1 && r.NextRun < dtRecur && r.NumberOfAttempts < numberOfAttempts);
            if (recurringBillings != null)
                return recurringBillings.ToList();
            return null;
        }

        private static PackageEntitlement CreatePackageEntitlement(EntitlementRequest request, PackageSubscriptionProduct subscription, ProductPackage package, DateTime registDt)
        {
            PackageEntitlement entitlement = new PackageEntitlement()
            {
                EndDate = GetEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
                Package = (Package)package.Package,
                OfferingId = offeringId,
                LatestEntitlementRequest = request
            };
            return entitlement;
        }

        private static ShowEntitlement CreateShowEntitlement(EntitlementRequest request, ShowSubscriptionProduct subscription, ProductShow show, DateTime registDt)
        {
            ShowEntitlement entitlement = new ShowEntitlement()
            {
                EndDate = GetEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
                Show = (Show)show.Show,
                OfferingId = offeringId,
                LatestEntitlementRequest = request
            };
            return entitlement;
        }

        private EpisodeEntitlement CreateEpisodeEntitlement(EntitlementRequest request, EpisodeSubscriptionProduct subscription, ProductEpisode episode, DateTime registDt)
        {
            EpisodeEntitlement entitlement = new EpisodeEntitlement()
            {
                EndDate = GetEntitlementEndDate(subscription.Duration, subscription.DurationType, registDt),
                Episode = (Episode)episode.Episode,
                OfferingId = offeringId,
                LatestEntitlementRequest = request
            };
            return entitlement;
        }

        private EntitlementRequest CreateEntitlementRequest(DateTime registDt, DateTime endDate, Product product, string source, string reference, DateTime startDate)
        {
            EntitlementRequest request = new EntitlementRequest()
            {
                DateRequested = registDt,
                EndDate = endDate,
                StartDate = startDate,
                Product = product,
                Source = source,
                ReferenceId = reference
            };
            return request;
        }

        private PurchaseItem CreatePurchaseItem(System.Guid userId, Product product, ProductPrice priceOfProduct)
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

        private Purchase CreatePurchase(DateTime registDt, string remarks)
        {
            Purchase purchase = new Purchase()
            {
                Date = registDt,
                Remarks = remarks
            };
            return purchase;
        }

        private static DateTime GetEntitlementEndDate(int duration, string interval, DateTime registDt)
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
    }
}
