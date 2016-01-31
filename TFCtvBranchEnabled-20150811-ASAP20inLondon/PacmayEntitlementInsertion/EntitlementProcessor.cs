using IPTV2_Model;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacmayEntitlementInsertion
{
    class EntitlementProcessor : IJob
    {
        static int offeringId = Convert.ToInt32(ConfigurationManager.AppSettings["offeringId"]);
        static int PremiumPacMayProductId = Convert.ToInt32(ConfigurationManager.AppSettings["PremiumPacMayProductId"]);
        static int PacMayProductId = Convert.ToInt32(ConfigurationManager.AppSettings["PacMayProductId"]);
        static int KidKulafuCategoryId = Convert.ToInt32(ConfigurationManager.AppSettings["KidKulafuCategoryId"]);
        static int PacMayLiveStreamCategoryId = Convert.ToInt32(ConfigurationManager.AppSettings["PacMayLiveStreamCategoryId"]);
        static int PacMayVODCategoryId = Convert.ToInt32(ConfigurationManager.AppSettings["PacMayVODCategoryId"]);
        static int KidKulafuProductId = Convert.ToInt32(ConfigurationManager.AppSettings["KidKulafuProductId"]);

        static DateTime registDt;
        static DateTime UtcDt;

        public EntitlementProcessor()
        {
            registDt = DateTime.Now;
            UtcDt = DateTime.UtcNow;
        }

        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("PacMay Entitlement Insertion is executing. Time now is " + DateTime.UtcNow);
            Send();
        }

        private void Send()
        {
            //Trace.WriteLine(context.Database.Connection.ConnectionString);
            try
            {
                var context = new IPTV2Entities();
                var PremiumPacMayProduct = context.Products.FirstOrDefault(p => p.ProductId == PremiumPacMayProductId); //get PremiumPacMay

                var KidKulafuProduct = context.Products.FirstOrDefault(p => p.ProductId == KidKulafuProductId);

                var PacMayProduct = context.Products.FirstOrDefault(p => p.ProductId == PacMayProductId); //get pacmay product id
                if (PremiumPacMayProduct != null && PacMayProduct != null && KidKulafuProduct != null)
                {
                    //get breakDate that will be used for the insertion of the other entitlements                    
                    DateTime startDt = registDt;
                    if (PacMayProduct.BreakingDate != null)
                        startDt = (DateTime)PacMayProduct.BreakingDate > registDt ? (DateTime)PacMayProduct.BreakingDate : registDt;

                    DateTime KidKulafu_startDt = registDt;
                    if (KidKulafuProduct.BreakingDate != null)
                        KidKulafu_startDt = (DateTime)KidKulafuProduct.BreakingDate > registDt ? (DateTime)KidKulafuProduct.BreakingDate : registDt;

                    SubscriptionProduct PacMaySubscriptionProduct = (SubscriptionProduct)PacMayProduct;

                    var logs = context.PacMayLogs.Select(p => p.PurchaseId).ToList();

                    var purchaseItems = context.PurchaseItems.Where(p => p.ProductId == PremiumPacMayProductId && !logs.Contains(p.PurchaseId)).ToList();
                    foreach (var item in purchaseItems)
                    {
                        var purchaseId = item.PurchaseId; //get purchaseId  
                        var user = item.User;

                        if (user != null && item.EntitlementRequestId > 0)
                        {
                            //Check if user has Kid Kulafu entitlement
                            var KidKulafu_entitlement = user.ShowEntitlements.FirstOrDefault(s => s.CategoryId == KidKulafuCategoryId);
                            var KidKulafu_request = CreateEntitlementRequest(registDt, registDt, PremiumPacMayProduct, String.Format("Purchase-{0}", purchaseId), String.Format("{0}", purchaseId), KidKulafu_startDt);

                            if (KidKulafu_entitlement != null)
                            {
                                //extend entitlement
                                if (KidKulafu_entitlement.EndDate > KidKulafu_request.StartDate)
                                    KidKulafu_request.StartDate = KidKulafu_entitlement.EndDate;
                                KidKulafu_entitlement.EndDate = GetEntitlementEndDate(PacMaySubscriptionProduct.Duration, PacMaySubscriptionProduct.DurationType, ((KidKulafu_entitlement.EndDate > registDt) ? KidKulafu_entitlement.EndDate : registDt));
                            }
                            else
                            {
                                //create new entitlement & new entitlementrequest
                                ShowEntitlement k_entitlement = CreateShowEntitlement(registDt, KidKulafuCategoryId, PacMaySubscriptionProduct.Duration, PacMaySubscriptionProduct.DurationType, KidKulafu_request);
                                KidKulafu_request.EndDate = k_entitlement.EndDate;
                                user.EntitlementRequests.Add(KidKulafu_request);
                                user.ShowEntitlements.Add(k_entitlement);
                            }

                            //Check if user has VOD entitlement
                            var PacMayVOD_entitlement = user.ShowEntitlements.FirstOrDefault(s => s.CategoryId == PacMayVODCategoryId);
                            var PacMayVOD_request = CreateEntitlementRequest(registDt, registDt, PremiumPacMayProduct, String.Format("Purchase-{0}", purchaseId), String.Format("{0}", purchaseId), startDt);

                            if (PacMayVOD_entitlement != null)
                            {
                                //extend entitlement
                                if (PacMayVOD_entitlement.EndDate > PacMayVOD_request.StartDate)
                                    PacMayVOD_request.StartDate = PacMayVOD_entitlement.EndDate;
                                PacMayVOD_entitlement.EndDate = GetEntitlementEndDate(PacMaySubscriptionProduct.Duration, PacMaySubscriptionProduct.DurationType, ((PacMayVOD_entitlement.EndDate > registDt) ? PacMayVOD_entitlement.EndDate : registDt));
                                //PacMayVOD_entitlement.EndDate = GetEntitlementEndDate(PacMaySubscriptionProduct.Duration, PacMaySubscriptionProduct.DurationType, startDt);
                            }
                            else
                            {
                                //create new entitlement & new entitlementrequest
                                ShowEntitlement p_entitlement = CreateShowEntitlement(startDt, PacMayVODCategoryId, PacMaySubscriptionProduct.Duration, PacMaySubscriptionProduct.DurationType, PacMayVOD_request);
                                PacMayVOD_request.EndDate = p_entitlement.EndDate;
                                user.EntitlementRequests.Add(PacMayVOD_request);
                                user.ShowEntitlements.Add(p_entitlement);
                            }

                            //Check if user has LiveStream entitlement
                            var PacMayLiveStream_entitlement = user.ShowEntitlements.FirstOrDefault(s => s.CategoryId == PacMayLiveStreamCategoryId);
                            var PacMayLiveStream_request = CreateEntitlementRequest(registDt, registDt, PremiumPacMayProduct, String.Format("Purchase-{0}", purchaseId), String.Format("{0}", purchaseId), startDt);

                            if (PacMayLiveStream_entitlement != null)
                            {
                                //extend entitlement
                                if (PacMayLiveStream_entitlement.EndDate > PacMayLiveStream_request.StartDate)
                                    PacMayLiveStream_request.StartDate = PacMayLiveStream_entitlement.EndDate;
                                PacMayLiveStream_entitlement.EndDate = GetEntitlementEndDate(PacMaySubscriptionProduct.Duration, PacMaySubscriptionProduct.DurationType, ((PacMayLiveStream_entitlement.EndDate > registDt) ? PacMayLiveStream_entitlement.EndDate : registDt));
                                //PacMayLiveStream_entitlement.EndDate = GetEntitlementEndDate(PacMaySubscriptionProduct.Duration, PacMaySubscriptionProduct.DurationType, startDt);
                            }
                            else
                            {
                                //create new entitlement & new entitlementrequest
                                ShowEntitlement m_entitlement = CreateShowEntitlement(startDt, PacMayLiveStreamCategoryId, PacMaySubscriptionProduct.Duration, PacMaySubscriptionProduct.DurationType, PacMayLiveStream_request);
                                PacMayLiveStream_request.EndDate = m_entitlement.EndDate;
                                user.EntitlementRequests.Add(PacMayLiveStream_request);
                                user.ShowEntitlements.Add(m_entitlement);
                            }

                            //log to database

                            if (context.SaveChanges() > 0)
                            {
                                PacMayLogs log = new PacMayLogs()
                                {
                                    PurchaseId = purchaseId,
                                    registDt = DateTime.Now,
                                    UserId = user.UserId
                                };
                                context.PacMayLogs.Add(log);
                                context.SaveChanges();
                                Console.WriteLine(String.Format("PurchaseId: {0} has been logged.", purchaseId));
                            }
                        }                        
                    }

                    System.Threading.Thread.Sleep(2000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Outer Exception: " + e.Message);
            }
        }

        private static EntitlementRequest CreateEntitlementRequest(DateTime dt, DateTime endDate, Product product, string source, string reference, DateTime startDate)
        {
            EntitlementRequest request = new EntitlementRequest()
            {
                DateRequested = dt,
                EndDate = endDate,
                StartDate = startDate,
                Product = product,
                Source = source,
                ReferenceId = reference
            };
            return request;
        }

        private static ShowEntitlement CreateShowEntitlement(DateTime startDt, int CategoryId, int Duration, string DurationType, EntitlementRequest request)
        {
            ShowEntitlement entitlement = new ShowEntitlement()
             {
                 EndDate = GetEntitlementEndDate(Duration, DurationType, startDt),
                 CategoryId = CategoryId,
                 OfferingId = offeringId,
                 LatestEntitlementRequest = request
             };
            return entitlement;
        }

        private static DateTime GetEntitlementEndDate(int duration, string interval, DateTime dt)
        {
            DateTime d = DateTime.Now;
            switch (interval)
            {
                case "d": d = dt.AddDays(duration); break;
                case "m": d = dt.AddMonths(duration); break;
                case "y": d = dt.AddYears(duration); break;
                case "h": d = dt.AddHours(duration); break;
                case "mm": d = dt.AddMinutes(duration); break;
                case "s": d = dt.AddSeconds(duration); break;
            }
            return d;
        }
    }

    public class JobInfo
    {
        public string jobName { get; set; }
    }
}
