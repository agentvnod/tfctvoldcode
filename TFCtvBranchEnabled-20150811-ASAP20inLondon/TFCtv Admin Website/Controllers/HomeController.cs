using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCNowModel;
using TFCtv_Admin_Website.Helpers;
using Newtonsoft.Json;

namespace TFCtv_Admin_Website.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Kayang Kaya Basta't Sama-sama!";

            string url = "http://www.iheartquotes.com/api/v1/random?format=json&source=oneliners";
            var jObj = JsonConvert.DeserializeObject<QuotableQuote>(MyUtility.GetQuotableQuote(url));
            ViewBag.QuotableQuote = jObj.quote;
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public string Test()
        {

            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile("o1234567", "SHA1");

        }


        [Authorize]
        public ActionResult PPS()
        {
            var absnow_context = new ABSNowEntities();
            var context = new IPTV2Entities();
            DateTime registDt = DateTime.Now;

            var customers = absnow_context.TFCNowRetailMigrationTFCTVs.Where(t => t.LicenseEndDate > registDt && t.IsMigrated == 0);
            var customerIds = customers.Select(t => t.Email.ToLower()).ToArray();
            var users = context.Users.Where(u => customerIds.Contains(u.TfcNowUserName.ToLower()));

            var TFCnowUsernames = users.Select(t => t.TfcNowUserName.ToLower()).ToArray();

            var migratedUsers = customers.Where(c => TFCnowUsernames.Contains(c.Email.ToLower())).OrderBy(c => c.Email);
            var ctp = migratedUsers.Count();

            var MigratedUsersWithGoms = migratedUsers.Join(absnow_context.TFCNowRetailMigrationIDs, license => license.TFCnowPackageID, package => package.TFCnowPackageID, (license, package) => new { license, package })
                .Select(m => new { Id = m.license.ID, Email = m.license.Email, LicenseEndDate = m.license.LicenseEndDate, TFCnowPackageId = m.license.TFCnowPackageID, GOMSInternalId = m.package.GOMSInternalID })
                .OrderBy(m => m.Email);

            List<MigratedUser> list = new List<MigratedUser>();
            foreach (var item in MigratedUsersWithGoms)
            {
                var user = context.Users.FirstOrDefault(u => u.TfcNowUserName.ToLower() == item.Email.ToLower());
                if (user == null)
                    throw new TFCtvObjectIsNull("User");
                Product product = context.Products.FirstOrDefault(p => p.GomsProductId == item.GOMSInternalId);
                if (product == null)
                    throw new TFCtvObjectIsNull("Product");

                DateTime endDate = (DateTime)item.LicenseEndDate;
                if (product is ShowSubscriptionProduct)
                {
                    var subscription = (ShowSubscriptionProduct)product;
                    if (subscription.ALaCarteSubscriptionTypeId == 2) // Pay per serye. Extend to 1 year.
                    {
                        endDate = registDt.AddYears(1);
                    }

                    TimeSpan difference = endDate.Subtract(registDt);

                    var category = subscription.Categories.FirstOrDefault();
                    if (category == null)
                        throw new TFCtvObjectIsNull("Category");
                    ShowEntitlement entitlement = null;
                    //Check entitlement for Category/Show
                    var show_entitlement = user.ShowEntitlements.FirstOrDefault(s => s.CategoryId == category.CategoryId);

                    if (show_entitlement != null)
                    {
                        show_entitlement.EndDate = show_entitlement.EndDate.Add(difference);
                        endDate = show_entitlement.EndDate;
                    }
                    else
                    {
                        entitlement = new ShowEntitlement()
                        {
                            EndDate = endDate,
                            Show = category.Show,
                            OfferingId = Global.OfferingId,
                        };
                        user.ShowEntitlements.Add(entitlement);
                    }
                    string reference = String.Format("TNRMTFCTV-{0}", item.Id);

                    EntitlementRequest request = CreateEntitlementRequest(registDt, endDate, category.Product, "cPanel Migration", reference);

                    if (request != null)
                    {
                        if (entitlement != null)
                            entitlement.LatestEntitlementRequest = request;
                        else
                            user.EntitlementRequests.Add(request);
                    }
                    MigrationTransaction transaction = new MigrationTransaction()
                    {
                        Amount = 0,
                        Currency = Global.DefaultCurrency,
                        Date = registDt,
                        OfferingId = Global.OfferingId,
                        Reference = reference,
                        MigratedProductId = (int)product.ProductId
                    };
                    user.Transactions.Add(transaction);

                    list.Add(new MigratedUser()
                    {
                        Id = item.Id,
                        Email = item.Email,
                        LicenseEndDate = item.LicenseEndDate,
                        GOMSInternalId = item.GOMSInternalId,
                        TFCnowPackageId = item.TFCnowPackageId,
                        EntitlementId = show_entitlement != null ? show_entitlement.EntitlementId : entitlement.EntitlementId,
                        EntitlementRequestId = request.EntitlementRequestId,
                        TransactionId = transaction.TransactionId
                    });

                    using (var ctx = new ABSNowEntities())
                    {
                        var taggedLicense = ctx.TFCNowRetailMigrationTFCTVs.FirstOrDefault(t => t.ID == item.Id);
                        if (taggedLicense != null)
                        {
                            taggedLicense.IsMigrated = 1;
                            ctx.SaveChanges();
                        }
                    }
                }
            }

            if (context.SaveChanges() > 0)
            {
                return this.Json(list, JsonRequestBehavior.AllowGet);
            }

            //return this.Json(list, JsonRequestBehavior.AllowGet);
            return this.Json(null, JsonRequestBehavior.AllowGet);
        }

        private static EntitlementRequest CreateEntitlementRequest(DateTime registDt, DateTime endDt, Product product, string Source, string reference)
        {
            EntitlementRequest request = new EntitlementRequest()
            {
                DateRequested = registDt,
                EndDate = endDt,
                Product = product,
                Source = Source,
                ReferenceId = reference
            };
            return request;
        }
    }

    public class C1
    {
        public string customerEmail { get; set; }

        public string userEmail { get; set; }
    }

    public class MigratedUser
    {
        public int? Id { get; set; }

        public string Email { get; set; }

        public DateTime? LicenseEndDate { get; set; }

        public int? TFCnowPackageId { get; set; }

        public int? GOMSInternalId { get; set; }

        public int TransactionId { get; set; }

        public int EntitlementId { get; set; }

        public int EntitlementRequestId { get; set; }
    }
}