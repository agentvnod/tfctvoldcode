using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCtv_Admin_Website.Helpers;

namespace TFCtv_Admin_Website.Controllers
{
    public class SettlementController : Controller
    {
        //
        // GET: /Settlement/

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Users = "albinul")]
        public ActionResult Subscription()
        {
            var context = new IPTV2Entities();
            var products = context.Products.Where(item => item.GomsProductId > 0 && item.GomsProductId != null && item.GomsProductQuantity == 1);
            ViewBag.Currencies = context.Currencies;
            if (products != null)
                return View(products);
            return RedirectToAction("Index");
        }

        [Authorize(Users = "albinul")]
        public ActionResult _Subscription(FormCollection f)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection = MyUtility.SetError(ErrorCode.UnidentifiedError, String.Empty);

            var email = f["EmailAddress"];
            var pId = f["Product"];
            var payment_mode = f["PaymentMode"];
            var reference = f["Reference"];
            var amt = f["Amount"];
            var currency = f["Currency"];
            var edt = f["EndDate"];
            var OverrideDuration = MyUtility.GetCheckBoxValue(Request, "OverrideDuration");
            var isRefund = MyUtility.GetCheckBoxValue(Request, "IsRefund");
            var IncludeWalletLoad = MyUtility.GetCheckBoxValue(Request, "IncludeWalletLoad");
            var registDt = DateTime.Now;
            try
            {
                int Duration = 0;
                string DurationType = String.Empty;
                DateTime endDt = registDt;

                if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(pId) || String.IsNullOrEmpty(payment_mode) || String.IsNullOrEmpty(reference) || String.IsNullOrEmpty(amt) || String.IsNullOrEmpty(currency))
                    throw new TFCtvMissingRequiredFields();

                if (isRefund && OverrideDuration)
                    throw new TFCtvUnidentifiedError("Is this a refund & Override product's duration can't be checked at the same time.");

                currency = currency.ToUpper();
                reference = reference.ToUpper();

                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(item => item.EMail.ToLower() == email.ToLower());
                if (user == null)
                    throw new TFCtvUserDoesNotExist();

                decimal amount;
                bool amt_result = decimal.TryParse(amt, out amount);
                if (!amt_result)
                    throw new TFCtvUnidentifiedError("Unable to convert Amount to decimal. User input was invalid.");

                int productId;
                bool pId_result = Int32.TryParse(pId, out productId);
                if (!pId_result)
                    throw new TFCtvUnidentifiedError("Unable to convert ProductId to Int32. User input was invalid.");

                if (OverrideDuration)
                {
                    DateTime overridingEndDate;
                    bool overrideDuration_result = DateTime.TryParse(edt, out overridingEndDate);
                    if (!overrideDuration_result)
                        throw new TFCtvUnidentifiedError("Unable to convert End Date to DateTime. User input was invalid.");
                }

                var currency_count = context.Currencies.Count(item => item.Code.ToUpper() == currency);
                if (currency_count == 0)
                    throw new TFCtvUnidentifiedError("Currency does not exist on our list. User input was invalid.");
                if (currency != Global.TrialCurrency)
                    if (user.Country.CurrencyCode.ToUpper() != currency)
                        throw new TFCtvUnidentifiedError("Currency does not match current user's currency.");

                var offering = context.Offerings.Find(Global.OfferingId);
                if (user.HasPendingGomsChangeCountryTransaction(offering))
                    throw new TFCtvUnidentifiedError("Change in location transaction found. Please retry later.");

                var product = context.Products.FirstOrDefault(item => item.ProductId == productId);

                if (product == null)
                    throw new TFCtvProductDoesNotExist();

                if (product is SubscriptionProduct)
                {
                    bool insertTransaction = false;
                    if (product is PackageSubscriptionProduct)
                    {
                        var package_subscription = (PackageSubscriptionProduct)product;
                        Duration = package_subscription.Duration;
                        DurationType = package_subscription.DurationType;

                        if (isRefund) //Refunding a subscription
                            Duration *= -1;

                        //Get Package
                        var package = package_subscription.Packages.FirstOrDefault();
                        if (package == null)
                            throw new TFCtvObjectIsNull("Package");

                        PackageEntitlement entitlement = null;

                        //Check entitlement for package
                        var package_entitlement = user.PackageEntitlements.FirstOrDefault(item => item.PackageId == package.PackageId);

                        if (isRefund) //Refunding a subscription
                            registDt = package_entitlement.EndDate;

                        endDt = MyUtility.GetEntitlementEndDate(Duration, DurationType, registDt);

                        if (package_entitlement != null)
                        {
                            endDt = MyUtility.GetEntitlementEndDate(Duration, DurationType, package_entitlement.EndDate > registDt ? package_entitlement.EndDate : registDt);
                            package_entitlement.EndDate = endDt;
                        }
                        else
                        {
                            entitlement = new PackageEntitlement()
                            {
                                EndDate = endDt,
                                Package = (Package)package.Package,
                                OfferingId = Global.OfferingId,
                            };
                            user.PackageEntitlements.Add(entitlement);
                        }

                        EntitlementRequest request = CreateEntitlementRequest(registDt, endDt, package.Product, "cPanel Settlement", reference);
                        if (request != null)
                        {
                            if (entitlement != null)
                                entitlement.LatestEntitlementRequest = request;
                            else
                                user.EntitlementRequests.Add(request);
                            insertTransaction = true;
                        }
                    }
                    else if (product is ShowSubscriptionProduct)
                    {
                        var show_subscription = (ShowSubscriptionProduct)product;
                        Duration = show_subscription.Duration;
                        DurationType = show_subscription.DurationType;

                        if (isRefund) //Refuding a subscription
                            Duration *= -1;

                        //Get Show
                        var category = show_subscription.Categories.FirstOrDefault();
                        if (category == null)
                            throw new TFCtvObjectIsNull("Category");

                        ShowEntitlement entitlement = null;

                        //Check entitlement for Category/Show
                        var show_entitlement = user.ShowEntitlements.FirstOrDefault(item => item.CategoryId == category.CategoryId);

                        if (isRefund) //Refunding a subscription
                            registDt = show_entitlement.EndDate;

                        endDt = MyUtility.GetEntitlementEndDate(Duration, DurationType, registDt);

                        if (show_entitlement != null)
                        {
                            endDt = MyUtility.GetEntitlementEndDate(Duration, DurationType, show_entitlement.EndDate > registDt ? show_entitlement.EndDate : registDt);
                            show_entitlement.EndDate = endDt;
                        }
                        else
                        {
                            entitlement = new ShowEntitlement()
                            {
                                EndDate = endDt,
                                Show = category.Show,
                                OfferingId = Global.OfferingId,
                            };
                            user.ShowEntitlements.Add(entitlement);
                        }

                        EntitlementRequest request = CreateEntitlementRequest(registDt, endDt, category.Product, "cPanel Settlement", reference);
                        if (request != null)
                        {
                            if (entitlement != null)
                                entitlement.LatestEntitlementRequest = request;
                            else
                                user.EntitlementRequests.Add(request);
                            insertTransaction = true;
                        }
                    }
                    else if (product is EpisodeSubscriptionProduct)
                    {
                        var episode_subscription = (EpisodeSubscriptionProduct)product;
                        Duration = episode_subscription.Duration;
                        DurationType = episode_subscription.DurationType;

                        if (isRefund) //Refuding a subscription
                            Duration *= -1;

                        //Get Episode
                        var episode = episode_subscription.Episodes.FirstOrDefault();
                        if (episode == null)
                            throw new TFCtvObjectIsNull("Episode");

                        EpisodeEntitlement entitlement = null;

                        //Check entitlement for Category/Show
                        var episode_entitlement = user.EpisodeEntitlements.FirstOrDefault(item => item.EpisodeId == episode.EpisodeId);

                        if (isRefund) //Refunding a subscription
                            registDt = episode_entitlement.EndDate;

                        endDt = MyUtility.GetEntitlementEndDate(Duration, DurationType, registDt);

                        if (episode_entitlement != null)
                        {
                            endDt = MyUtility.GetEntitlementEndDate(Duration, DurationType, episode_entitlement.EndDate > registDt ? episode_entitlement.EndDate : registDt);
                            episode_entitlement.EndDate = endDt;
                        }
                        else
                        {
                            entitlement = new EpisodeEntitlement()
                            {
                                EndDate = endDt,
                                Episode = episode.Episode,
                                OfferingId = Global.OfferingId,
                            };
                            user.EpisodeEntitlements.Add(entitlement);
                        }

                        EntitlementRequest request = CreateEntitlementRequest(registDt, endDt, episode.Product, "cPanel Settlement", reference);
                        if (request != null)
                        {
                            if (entitlement != null)
                                entitlement.LatestEntitlementRequest = request;
                            else
                                user.EntitlementRequests.Add(request);
                            insertTransaction = true;
                        }
                    }

                    //Create Purchase & Purchase Items
                    Purchase purchase = null; //CreatePurchase(registDt, "Settlement");
                    PurchaseItem purchase_item = null; //CreatePurchaseItem(user.UserId, product, amount, currency);

                    //user.Purchases.Add(purchase);
                    //user.PurchaseItems.Add(purchase_item);

                    //Insert transaction
                    if (insertTransaction)
                    {
                        switch (Convert.ToInt32(payment_mode))
                        {
                            case 1: // Prepaid Card
                                purchase = CreatePurchase(registDt, "Settlement via Prepaid Card");
                                user.Purchases.Add(purchase);
                                purchase_item = CreatePurchaseItem(user.UserId, product, amount, currency);
                                purchase.PurchaseItems.Add(purchase_item);

                                Ppc Ppc = context.Ppcs.FirstOrDefault(item => item.SerialNumber.ToUpper() == reference);
                                if (Ppc == null)
                                    throw new TFCtvObjectIsNull("Prepaid Card");
                                if (!(Ppc is SubscriptionPpc))
                                    throw new TFCtvEntityFrameworkError("Prepaid Card is not of type: Subscription.");

                                PpcPaymentTransaction pTransaction = new PpcPaymentTransaction()
                                {
                                    Currency = currency,
                                    Reference = reference,
                                    Amount = Convert.ToDecimal(amount),
                                    Product = product,
                                    Purchase = purchase,
                                    SubscriptionPpc = (SubscriptionPpc)Ppc,
                                    Date = registDt,
                                    OfferingId = Global.OfferingId
                                };
                                user.Transactions.Add(pTransaction);
                                break;
                            case 2: // E-Wallet
                                purchase = CreatePurchase(registDt, "Settlement via Wallet");
                                user.Purchases.Add(purchase);
                                purchase_item = CreatePurchaseItem(user.UserId, product, amount, currency);
                                purchase.PurchaseItems.Add(purchase_item);

                                var wallet = user.UserWallets.FirstOrDefault(item => item.IsActive == true);
                                if (IncludeWalletLoad)
                                    wallet.Balance += amount;

                                WalletPaymentTransaction wTransaction = new WalletPaymentTransaction()
                                {
                                    Currency = currency,
                                    Reference = reference,
                                    Amount = Convert.ToDecimal(amount),
                                    Date = registDt,
                                    User = user,
                                    OfferingId = Global.OfferingId
                                };
                                user.Transactions.Add(wTransaction);
                                wallet.WalletPaymentTransactions.Add(wTransaction);
                                break;
                            case 3: // Credit Card
                                purchase = CreatePurchase(registDt, "Settlement via Credit Card");
                                user.Purchases.Add(purchase);
                                purchase_item = CreatePurchaseItem(user.UserId, product, Convert.ToDecimal(amount), currency);
                                purchase.PurchaseItems.Add(purchase_item);

                                CreditCardPaymentTransaction cTransaction = new CreditCardPaymentTransaction()
                                {
                                    Amount = Convert.ToDecimal(amount),
                                    Currency = currency,
                                    Reference = reference,
                                    Date = registDt,
                                    Purchase = purchase,
                                    OfferingId = Global.OfferingId
                                };
                                user.Transactions.Add(cTransaction);
                                break;
                            case 4: // Paypal
                                purchase = CreatePurchase(registDt, "Settlement via Paypal");
                                user.Purchases.Add(purchase);
                                purchase_item = CreatePurchaseItem(user.UserId, product, Convert.ToDecimal(amount), currency);
                                purchase.PurchaseItems.Add(purchase_item);

                                PaypalPaymentTransaction ppTransaction = new PaypalPaymentTransaction()
                                {
                                    Currency = currency,
                                    Reference = reference,
                                    Amount = Convert.ToDecimal(amount),
                                    User = user,
                                    Date = registDt,
                                    OfferingId = Global.OfferingId
                                };
                                user.Transactions.Add(ppTransaction);
                                break;
                            case 5: // Migration
                                MigrationTransaction mTransaction = new MigrationTransaction()
                                {
                                };
                                user.Transactions.Add(mTransaction);
                                break;
                            default: break;
                        }

                        if (context.SaveChanges() > 0)
                        {
                            collection = MyUtility.SetError(ErrorCode.Success, "You have successfully settled a complaint.");
                            // Success
                        }
                    }
                }
            }
            catch (TFCtvException e)
            {
                collection = MyUtility.SetError(e.StatusCode, e.StatusMessage);
            }
            catch (Exception e)
            {
                collection = MyUtility.SetError(ErrorCode.UnidentifiedError, e.Message);
            }

            return Content(MyUtility.BuildJSON(collection), "application/json");
        }

        private static PurchaseItem CreatePurchaseItem(System.Guid userId, Product product, decimal Amount, string CurrencyCode)
        {
            PurchaseItem item = new PurchaseItem()
            {
                RecipientUserId = userId,
                ProductId = product.ProductId,
                Price = Amount,
                Currency = CurrencyCode,
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
}