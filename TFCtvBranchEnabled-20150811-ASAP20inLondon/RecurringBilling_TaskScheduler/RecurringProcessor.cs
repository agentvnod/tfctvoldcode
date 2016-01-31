using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using System.Diagnostics;
using IPTV2_Model;
using GOMS_TFCtv;
using System.Configuration;
using System.Data;
//using SendGridMail;
//using SendGridMail.Transport;
using System.Threading;
using SendGrid;

namespace RecurringBilling_TS
{
    public class RecurringProcessor : IJob
    {
        static int offeringId = Convert.ToInt32(ConfigurationManager.AppSettings["offeringId"]);
        static string DefaultCurrencyCode = ConfigurationManager.AppSettings["DefaultCurrencyCode"];
        static bool isProduction = Convert.ToBoolean(ConfigurationManager.AppSettings["isProduction"]);
        static bool isSendEmailEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["isSendEmailEnabled"]);

        static int addDays = Convert.ToInt32(ConfigurationManager.AppSettings["numOfDaysRecurringProcess"]);
        static int numberOfAttempts = Convert.ToInt32(ConfigurationManager.AppSettings["numberOfAttempts"]);

        static string ExtendSubscriptionBodyWithAutoRenewTextOnly = ConfigurationManager.AppSettings["ExtendSubscriptionBodyWithAutoRenewTextOnly"];
        static string NoReplyEmail = ConfigurationManager.AppSettings["NoReplyEmail"];
        static string SendGridUsername = ConfigurationManager.AppSettings["SendGridUsername"];
        static string SendGridPassword = ConfigurationManager.AppSettings["SendGridPassword"];
        static string SendGridSmtpHost = ConfigurationManager.AppSettings["SendGridSmtpHost"];
        static int SendGridSmtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SendGridSmtpPort"]);
        static string AutoRenewFailureBodyTextOnly = ConfigurationManager.AppSettings["AutoRenewFailureBodyTextOnly"];
        static bool IsSendConsolidatedReportsEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSendConsolidatedReportsEnabled"]);
        static string consolidatedReportReceivers = ConfigurationManager.AppSettings["consolidatedReportReceivers"];
        static string toRecipient = ConfigurationManager.AppSettings["toRecipient"];

        static DateTime registDt;
        static DateTime UtcDt;

        public RecurringProcessor()
        {
            registDt = DateTime.Now;
            UtcDt = DateTime.UtcNow;
        }

        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Recurring Billing is executing. Time now is " + DateTime.UtcNow);
            Send();
        }

        private void Send()
        {
            //Trace.WriteLine(context.Database.Connection.ConnectionString);
            try
            {
                int successfullyProcessed = 0;
                int failedProcessed = 0;

                var failedEmails = new StringBuilder();
                var emailBody = new StringBuilder();
                List<Int32> listOfFailedRecurringBillingId = new List<Int32>();

                emailBody.AppendLine(@"Recurring Billing Consolidated Report\r\n\r\n");
                emailBody.AppendLine(String.Format(@"Current date: {0}\r\n\r\n\r\n", registDt));

                Console.WriteLine("Fetching users for recurring billing...");

                DateTime dtRecur = registDt.Date.AddDays(addDays);
                var recurringBillings = GetUsersEligibleForRenewal(dtRecur);
                Console.WriteLine(String.Format("Total users eligible for renewal: {0}", recurringBillings.Count()));

                if (recurringBillings != null)
                {
                    if (recurringBillings.Count > 0)
                    {

                        using (var context = new IPTV2Entities())
                        {
                            var gomsService = new GomsTfcTv();
                            try
                            {
                                gomsService.TestConnect();
                                Console.WriteLine("Test Connect success");
                            }
                            catch (Exception) { Console.WriteLine("Test Connect failed."); }

                            foreach (var i in recurringBillings)
                            {
                                using (var context3 = new IPTV2Entities())
                                {
                                    var user = context3.Users.FirstOrDefault(u => u.UserId == i.UserId);
                                    var recurringBilling = context3.RecurringBillings.Find(i.RecurringBillingId);
                                    var product = context3.Products.Find(i.ProductId);

                                    string productName = String.Empty;

                                    Console.WriteLine(String.Format("Processing user {0} with productId {1}, endDate {2}", user.EMail, product.Description, recurringBilling.EndDate));
                                    try
                                    {
                                        ProductPrice priceOfProduct = context3.ProductPrices.FirstOrDefault(p => p.CurrencyCode == user.Country.CurrencyCode && p.ProductId == product.ProductId);
                                        if (priceOfProduct == null)
                                            priceOfProduct = context3.ProductPrices.FirstOrDefault(p => p.CurrencyCode == DefaultCurrencyCode && p.ProductId == product.ProductId);

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
                                        var response = gomsService.CreateOrderViaRecurringPayment(context3, user.UserId, transaction);
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
                                                    productName = ProductNameBought;

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
                                                recurringBilling.GomsRemarks = null;
                                                recurringBilling.NumberOfAttempts = 0;

                                            }
                                            Console.WriteLine(user.EMail + ": renewal process complete!");
                                            if (context3.SaveChanges() > 0)
                                            {
                                                successfullyProcessed += 1;
                                                Console.WriteLine("Saving changes...");
                                            }


                                            //Send email to user;
                                            try { SendConfirmationEmails(user, user, transaction, productName, product, endDate, registDt, "Credit Card", (DateTime)endDate.AddDays(-4).Date); }
                                            catch (Exception e) { recurringBilling.GomsRemarks = e.Message; }

                                        }
                                        else
                                        {
                                            using (var context2 = new IPTV2Entities())
                                            {
                                                var failedRecurring = context2.RecurringBillings.FirstOrDefault(r => r.RecurringBillingId == recurringBilling.RecurringBillingId);
                                                if (failedRecurring != null)
                                                {
                                                    failedRecurring.GomsRemarks = response.StatusMessage;
                                                    failedRecurring.NumberOfAttempts += 1;
                                                    listOfFailedRecurringBillingId.Add(failedRecurring.RecurringBillingId);
                                                    if (failedRecurring.NumberOfAttempts == 2)
                                                    {
                                                        //Send failure email
                                                        try { SendConfirmationEmails(user, user, transaction, productName, product, registDt, registDt, "Credit Card", registDt, response.StatusMessage); }
                                                        catch (Exception e) { failedRecurring.GomsRemarks = e.Message; }
                                                    }

                                                    string failedSpecificsCopy = String.Format("RBId: {0}\r\n\r\nEmail: {1}\r\n\r\nProduct: {2}\r\n\r\nError: {3}\r\n\r\n", failedRecurring.RecurringBillingId, user.EMail, product.Description, response.StatusMessage);
                                                    failedEmails.AppendLine(failedSpecificsCopy);
                                                    failedEmails.AppendLine("--------------------------------------------------\r\n\r\n");
                                                    failedProcessed += 1;

                                                    if (context2.SaveChanges() > 0)
                                                        Console.WriteLine("Saving error...");
                                                }
                                            }
                                            throw new Exception(user.EMail + ": " + response.StatusMessage);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Inner Exception: " + e.Message);
                                    }
                                }

                            }
                            //if (context.SaveChanges() > 0)
                            //{
                            //    Console.WriteLine("Saving changes...");
                            Console.WriteLine("Processing of eligible users for recurring billing has completed");
                            //}

                            //Finish the copy of the email
                            emailBody.AppendLine(String.Format(@"Total number of successful transactions: {0}\r\n\r\n", successfullyProcessed));
                            emailBody.AppendLine(String.Format(@"Total number of failed transactions (GOMS): {0}\r\n\r\n\r\n", failedProcessed));

                            if (failedEmails.Length > 0)
                            {
                                emailBody.AppendLine(@"Specifics can be found below\r\n\r\n\r\n");
                                emailBody.AppendLine(String.Format(@"{0}\r\n\r\n\r\n", failedEmails.ToString()));
                            }
                            emailBody.AppendLine("=================================================================\r\n\r\n\r\n");

                            //Get total numbers of recurring billing that have reached processing threshold
                            //var reachedThreshold = context.RecurringBillings.Where(r => r.StatusId == 1 && r.NumberOfAttempts == 3);
                            //if (reachedThreshold != null)
                            //{
                            //    emailBody.AppendLine(String.Format(@"Total number of failed transactions (MAX ATTEMPT): {0}\r\n\r\n", reachedThreshold.Count()));
                            //    foreach (var item in reachedThreshold)
                            //    {
                            //        string failedSpecificsCopy = String.Format("RBId: {0}\r\n\r\nEmail: {1}\r\n\r\nProduct: {2}\r\n\r\nLast error received: {3}\r\n\r\n", item.RecurringBillingId, item.User.EMail, item.Product.Description, item.GomsRemarks);
                            //        emailBody.AppendLine(failedSpecificsCopy);
                            //        emailBody.AppendLine("--------------------------------------------------\r\n\r\n");
                            //    }
                            //}

                            failedEmails.AppendLine("=================================================================\r\n\r\n\r\n");

                            emailBody.AppendLine("Report ends here.");

                            var newEmailBody = new StringBuilder();
                            newEmailBody.AppendLine("<!DOCTYPE html><html><body style=\"font-family: \"Trebuchet MS\", Arial, sans-serif;color:#000; font-size: 14px;\">");
                            newEmailBody.AppendLine("<h3>Report summary</h3>");
                            newEmailBody.AppendLine(String.Format("DateTime of processing (UTC): {0}", UtcDt));
                            newEmailBody.AppendLine(String.Format("<p style=\"font-size: 16px; font-weight: bold;\">Total number of successful transactions: {0}</p>", successfullyProcessed));
                            newEmailBody.AppendLine(String.Format("<p style=\"font-size: 16px; font-weight: bold;\">Total number of failed transactions: {0}</p>", failedProcessed));
                            newEmailBody.AppendLine(CreateConsolidatedReport(1, "GOMS", listOfFailedRecurringBillingId));
                            newEmailBody.AppendLine("<hr />");
                            newEmailBody.AppendLine(CreateConsolidatedReport(3, "MAX ATTEMPT", null));
                            newEmailBody.AppendLine("<p style=\"font-size: 16px; font-weight: bold;\">Report ends here.</p>");
                            newEmailBody.AppendLine("</body></html>");

                            if (IsSendConsolidatedReportsEnabled)
                            {
                                var receivers = consolidatedReportReceivers.Split(',');
                                try
                                {
                                    //SendEmailViaSendGrid(null, NoReplyEmail, "TFC.tv Recurring Billing: Consolidated Report", emailBody.ToString(), MailType.TextOnly, emailBody.ToString(), receivers);
                                    //SendEmailViaSendGrid(null, NoReplyEmail, "TFC.tv Recurring Billing: Consolidated Report", newEmailBody.ToString(), MailType.HtmlOnly, newEmailBody.ToString(), receivers);
                                    SendEmailViaSendGrid(toRecipient, NoReplyEmail, "TFC.tv Recurring Billing: Consolidated Report", newEmailBody.ToString(), MailType.HtmlOnly, newEmailBody.ToString(), receivers);
                                    Console.WriteLine("Sending of consolidated report is successful!");
                                }
                                catch (Exception) { Console.WriteLine("Sending of consolidated report failed!"); }
                            }

                            //CANCELL ALL RECURRING
                            try
                            {
                                var cancellation_list = GetUsersEligibleForCancellation(dtRecur);
                                foreach (var i in cancellation_list)
                                {
                                    try { gomsService.CancelRecurringPayment(i.User, i.Product); }
                                    catch (Exception) { }
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                    else
                        Console.WriteLine("Nothing to process..");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Outer Exception: " + e.Message);
            }
        }


        private List<RecurringBilling> GetUsersEligibleForRenewal(DateTime dtRecur)
        {
            var context = new IPTV2Entities();
            var recurringBillings = context.RecurringBillings.Where(r => r.StatusId == 1 && r.NextRun < dtRecur && r.NumberOfAttempts < numberOfAttempts && r is CreditCardRecurringBilling);
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

        private static void SendConfirmationEmails(User user, User recipient, Transaction transaction, string ProductNameBought, Product product, DateTime endDt, DateTime registDt, string mode, DateTime? autoRenewReminderDate = null, string GomsError = null)
        {
            if (isSendEmailEnabled)
            {
                string emailBody = String.Empty;
                string mailSubject = String.Empty;
                string toEmail = String.Empty;

                emailBody = String.Format(ExtendSubscriptionBodyWithAutoRenewTextOnly, user.FirstName, ProductNameBought, endDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.TransactionId, product.Name, registDt.ToString("MM/dd/yyyy hh:mm:ss tt"), transaction.Amount.ToString("F2"), transaction.Currency, mode, transaction.Reference, ((DateTime)autoRenewReminderDate).ToString("MMMM dd, yyyy"));
                mailSubject = String.Format("Your {0} has been extended", ProductNameBought);
                toEmail = user.EMail;
                if (!String.IsNullOrEmpty(GomsError))
                {
                    emailBody = String.Format(AutoRenewFailureBodyTextOnly, user.FirstName, ProductNameBought, GomsError);
                    mailSubject = "Unable to Auto-Renew your TFC.tv Subscription";
                }

                try
                {
                    if (!String.IsNullOrEmpty(toEmail))
                        SendEmailViaSendGrid(toEmail, NoReplyEmail, mailSubject, emailBody, MailType.TextOnly, emailBody);
                }
                catch (Exception e)
                {
                    throw new Exception(String.Format("SendGrid: {0}", e.Message));
                }
            }
        }


        public static void SendEmailViaSendGrid(string to, string from, string subject, string htmlBody, MailType type, string textBody, string[] multipleTo = null)
        {
            try
            {
                //var message = SendGrid.GenerateInstance();
                var message = new SendGridMessage();
                if (String.IsNullOrEmpty(to))
                    message.AddTo(multipleTo);
                else
                    message.AddTo(to);

                //if (multipleTo != null)
                //    message.AddTo(multipleTo);
                //else
                //    message.AddTo(to);

                message.From = new System.Net.Mail.MailAddress(from);
                message.Subject = subject;
                if (type == MailType.TextOnly)
                    message.Text = textBody.Replace(@"\r\n", Environment.NewLine);
                else if (type == MailType.HtmlOnly)
                    message.Html = htmlBody;
                else
                {
                    message.Html = htmlBody;
                    message.Text = textBody;
                }

                //Dictionary<string, string> collection = new Dictionary<string, string>();
                //collection.Add("header", "header");
                //message.Headers = collection;

                message.EnableOpenTracking();
                message.EnableClickTracking();
                message.DisableUnsubscribe();
                message.DisableFooter();
                message.EnableBypassListManagement();
                //var transportInstance = SMTP.GenerateInstance(new System.Net.NetworkCredential(SendGridUsername, SendGridPassword), SendGridSmtpHost, SendGridSmtpPort);
                var transportInstance = new Web(new System.Net.NetworkCredential(SendGridUsername, SendGridPassword));
                transportInstance.Deliver(message);
                if (String.IsNullOrEmpty(to))
                    Console.WriteLine("SendGrid: Email was sent successfully to " + multipleTo);
                else
                    Console.WriteLine("SendGrid: Email was sent successfully to " + to);
            }
            catch (Exception)
            {
                if (String.IsNullOrEmpty(to))
                    Console.WriteLine("SendGrid: Unable to send email to " + multipleTo);
                else
                    Console.WriteLine("SendGrid: Unable to send email to " + to);
                throw;
            }
        }

        private static string CreateConsolidatedReport(int numberOfAttempts, string errorType, List<Int32> listOfFailedRecurringBillingId)
        {
            var sb = new StringBuilder();

            //instantiate string builders for each region
            var sbUS = new StringBuilder();
            var sbEU = new StringBuilder();
            var sbAU = new StringBuilder();
            var sbCA = new StringBuilder();
            var sbME = new StringBuilder();
            var sbAP = new StringBuilder();
            var sbJP = new StringBuilder();

            //instantiate headers
            sbUS.AppendLine("<p style=\"font-size: 16px; font-weight: bold;\">GLB : US</p>");
            sbEU.AppendLine("<p style=\"font-size: 16px; font-weight: bold;\">GLB : EU</p>");
            sbAU.AppendLine("<p style=\"font-size: 16px; font-weight: bold;\">GLB : AU</p>");
            sbCA.AppendLine("<p style=\"font-size: 16px; font-weight: bold;\">GLB : CA</p>");
            sbME.AppendLine("<p style=\"font-size: 16px; font-weight: bold;\">GLB : ME</p>");
            sbAP.AppendLine("<p style=\"font-size: 16px; font-weight: bold;\">GLB : AP</p>");
            sbJP.AppendLine("<p style=\"font-size: 16px; font-weight: bold;\">GLB : JP</p>");

            //instantiate table structure
            var tableStart = "<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border: 1px solid #00a651; border-right: 0px; border-bottom: 0px;\">";
            var tableEnd = "</table>";

            sbUS.AppendLine(tableStart);
            sbEU.AppendLine(tableStart);
            sbAU.AppendLine(tableStart);
            sbCA.AppendLine(tableStart);
            sbME.AppendLine(tableStart);
            sbAP.AppendLine(tableStart);
            sbJP.AppendLine(tableStart);

            //instantiate table headers
            string headers = "Recurring Billing Id,EMail Address,Product,# of attempts,Goms Error";

            sbUS.AppendLine(CreateHeader(headers));
            sbEU.AppendLine(CreateHeader(headers));
            sbAU.AppendLine(CreateHeader(headers));
            sbCA.AppendLine(CreateHeader(headers));
            sbME.AppendLine(CreateHeader(headers));
            sbAP.AppendLine(CreateHeader(headers));
            sbJP.AppendLine(CreateHeader(headers));

            var context = new IPTV2Entities();
            sb.AppendLine(String.Format("<h1>Failed transactions ({0})</h1>", errorType));

            IQueryable<RecurringBilling> reachedThreshold = null;
            if (listOfFailedRecurringBillingId != null)
            {
                if (listOfFailedRecurringBillingId.Count() > 0)
                    reachedThreshold = context.RecurringBillings.Where(r => r.StatusId == 1 && listOfFailedRecurringBillingId.Contains(r.RecurringBillingId));
                else
                    reachedThreshold = context.RecurringBillings.Where(r => r.StatusId == 1 && r.NumberOfAttempts == numberOfAttempts);
            }
            else
                reachedThreshold = context.RecurringBillings.Where(r => r.StatusId == 1 && r.NumberOfAttempts == numberOfAttempts);

            if (reachedThreshold != null)
            {
                foreach (var i in reachedThreshold)
                {
                    if (i.User.Country != null)
                    {
                        switch (i.User.Country.GomsSubsidiaryId)
                        {
                            case 11: //US
                                {
                                    sbUS.Append("<tr>");
                                    sbUS.Append(CreateCell(i.RecurringBillingId));
                                    sbUS.Append(CreateCell(i.User.EMail));
                                    sbUS.Append(CreateCell(i.Product.Name));
                                    sbUS.Append(CreateCell(i.NumberOfAttempts));
                                    sbUS.Append(CreateCell(i.GomsRemarks));
                                    sbUS.AppendLine("</tr>");
                                    break;
                                }
                            case 6: //EU
                            case 8:
                            case 9:
                                {
                                    sbEU.Append("<tr>");
                                    sbEU.Append(CreateCell(i.RecurringBillingId));
                                    sbEU.Append(CreateCell(i.User.EMail));
                                    sbEU.Append(CreateCell(i.Product.Name));
                                    sbEU.Append(CreateCell(i.NumberOfAttempts));
                                    sbEU.Append(CreateCell(i.GomsRemarks));
                                    sbEU.AppendLine("</tr>");
                                    break;
                                }
                            case 12: //ME
                                {
                                    sbME.Append("<tr>");
                                    sbME.Append(CreateCell(i.RecurringBillingId));
                                    sbME.Append(CreateCell(i.User.EMail));
                                    sbME.Append(CreateCell(i.Product.Name));
                                    sbME.Append(CreateCell(i.NumberOfAttempts));
                                    sbME.Append(CreateCell(i.GomsRemarks));
                                    sbME.AppendLine("</tr>");
                                    break;
                                }
                            case 14: //AP
                                {
                                    sbAP.Append("<tr>");
                                    sbAP.Append(CreateCell(i.RecurringBillingId));
                                    sbAP.Append(CreateCell(i.User.EMail));
                                    sbAP.Append(CreateCell(i.Product.Name));
                                    sbAP.Append(CreateCell(i.NumberOfAttempts));
                                    sbAP.Append(CreateCell(i.GomsRemarks));
                                    sbAP.AppendLine("</tr>");
                                    break;
                                }
                            case 2: //AU
                                {
                                    sbAU.Append("<tr>");
                                    sbAU.Append(CreateCell(i.RecurringBillingId));
                                    sbAU.Append(CreateCell(i.User.EMail));
                                    sbAU.Append(CreateCell(i.Product.Name));
                                    sbAU.Append(CreateCell(i.NumberOfAttempts));
                                    sbAU.Append(CreateCell(i.GomsRemarks));
                                    sbAU.AppendLine("</tr>");
                                    break;
                                }
                            case 10: //CA
                                {
                                    sbCA.Append("<tr>");
                                    sbCA.Append(CreateCell(i.RecurringBillingId));
                                    sbCA.Append(CreateCell(i.User.EMail));
                                    sbCA.Append(CreateCell(i.Product.Name));
                                    sbCA.Append(CreateCell(i.NumberOfAttempts));
                                    sbCA.Append(CreateCell(i.GomsRemarks));
                                    sbCA.AppendLine("</tr>");
                                    break;
                                }
                            case 13: //JP
                                {
                                    sbJP.Append("<tr>");
                                    sbJP.Append(CreateCell(i.RecurringBillingId));
                                    sbJP.Append(CreateCell(i.User.EMail));
                                    sbJP.Append(CreateCell(i.Product.Name));
                                    sbJP.Append(CreateCell(i.NumberOfAttempts));
                                    sbJP.Append(CreateCell(i.GomsRemarks));
                                    sbJP.AppendLine("</tr>");
                                    break;
                                }
                            default: { break; }
                        }
                    }
                    if (String.Compare("MAX ATTEMPT", errorType, true) == 0)
                    {
                        if (i.NumberOfAttempts == 3)
                        {
                            i.StatusId = 3; // Removed
                            i.UpdatedOn = DateTime.Now.Date;
                        }
                    }
                }
                if (String.Compare("MAX ATTEMPT", errorType, true) == 0)
                {
                    try
                    {
                        if (context.SaveChanges() > 0)
                            Console.WriteLine("Tagged user's who have reached maximum threshold as removed.");
                    }
                    catch (Exception) { }
                }

                //close all tables
                sbUS.AppendLine(tableEnd);
                sbEU.AppendLine(tableEnd);
                sbAU.AppendLine(tableEnd);
                sbCA.AppendLine(tableEnd);
                sbME.AppendLine(tableEnd);
                sbAP.AppendLine(tableEnd);
                sbJP.AppendLine(tableEnd);

                //Append all errors to a single string builder
                sb.AppendLine(sbUS.ToString());
                sb.AppendLine(sbEU.ToString());
                sb.AppendLine(sbAU.ToString());
                sb.AppendLine(sbCA.ToString());
                sb.AppendLine(sbME.ToString());
                sb.AppendLine(sbAP.ToString());
                sb.AppendLine(sbJP.ToString());
            }
            else
                sb.AppendLine("<p style=\"font-size: 16px; font-weight: bold;\">No transactionswith maximum attempts found during this run.</p>");
            return sb.ToString();
        }


        private static string CreateCell(object s)
        {
            return String.Format("<td style=\"border-right: 1px solid #00a651; border-bottom: 1px solid #00a651;\">{0}</td>", s);
        }


        private static string CreateHeader(string headers)
        {
            var sb = new StringBuilder();
            var h = headers.Split(',');
            sb.Append("<tr style=\"font-weight:bold;\">");
            foreach (var i in h)
            {
                sb.Append(CreateCell(i));
            }
            sb.Append("</tr>");
            return sb.ToString();
        }

        //  MAX ATTEMPT
        private List<RecurringBilling> GetUsersEligibleForCancellation(DateTime dtRecur)
        {
            var context = new IPTV2Entities();
            var recurringBillings = context.RecurringBillings.Where(r => r.StatusId == 3 && r.NextRun < dtRecur && r.NumberOfAttempts < numberOfAttempts && r is CreditCardRecurringBilling);
            if (recurringBillings != null)
                return recurringBillings.ToList();
            return null;
        }
    }

    public enum MailType
    {
        TextOnly = 0,
        HtmlOnly = 1,
        Both = 2
    }


    public class EmailInformation
    {
        public string jobName { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
        public int numberOfDays { get; set; }
        public bool isRecurring { get; set; }
    }
}
