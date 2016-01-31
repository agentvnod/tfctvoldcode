using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;
using System.Diagnostics;
using SendGrid;
using System.IO;
using System.Net;
using TFCTV.Helpers;
using Quartz;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace TFCtv_EMail_Reminder
{

    public class EmailReminder : IJob
    {
        static int offeringId = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("offeringId"));
        static string SendGridUsername = RoleEnvironment.GetConfigurationSettingValue("SendGridUsername");
        static string SendGridPassword = RoleEnvironment.GetConfigurationSettingValue("SendGridPassword");
        static string SendGridSmtpHost = RoleEnvironment.GetConfigurationSettingValue("SendGridSmtpHost");
        static int SendGridSmtpPort = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("SendGridSmtpPort"));
        static string fromEmail = RoleEnvironment.GetConfigurationSettingValue("fromEmail");

        public EmailReminder() { }
        //public EmailReminder(int offeringId, string fromEmail, string SendGridUsername, string SendGridPassword, string SendGridSmtpHost, int SendGridSmtpPort)
        //{
        //    this.offeringId = offeringId;
        //    this.fromEmail = fromEmail;
        //    this.SendGridUsername = SendGridUsername;
        //    this.SendGridPassword = SendGridPassword;
        //    this.SendGridSmtpPort = SendGridSmtpPort;
        //}

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            EmailInformation information = (EmailInformation)dataMap.Get("Information");
            Trace.TraceInformation("Number of days: " + information.numberOfDays);
            Trace.TraceInformation("EmaiReminder is executing. Time now is " + DateTime.UtcNow);
            Send(information);
        }

        private void Send(EmailInformation information)
        {
            var context = new IPTV2Entities();
            context.Database.Connection.ConnectionString = RoleEnvironment.GetConfigurationSettingValue("Iptv2Entities");
            Trace.WriteLine(context.Database.Connection.ConnectionString);
            try
            {                
                if (information.isRecurring)
                {

                    var usersWithRecurring = EmailReminder.GetRecurringUsers(context, information.numberOfDays, offeringId);
                    if (usersWithRecurring != null)
                    {
                        var emailTemplateHTML = information.Template;
                        foreach (var item in usersWithRecurring)
                        {
                            var template = emailTemplateHTML;
                            var textBody = template.Replace("[firstname]", item.User.FirstName);
                            textBody = textBody.Replace("[productname]", item.Product.Description);
                            if (item.Product is SubscriptionProduct)
                            {
                                var sproduct = (SubscriptionProduct)item.Product;
                                textBody = textBody.Replace("[newexpiry]", GetEntitlementEndDate(sproduct.Duration, sproduct.DurationType, item.EndDate.Value).ToString("MMMM dd, yyyy"));
                            }
                            Trace.TraceInformation("Trying to send email to " + item.User.EMail);
                            EmailReminder.SendEmailViaSendGrid(item.User.EMail, fromEmail, information.Subject, String.Empty, TFCtv_EMail_Reminder.MailType.TextOnly, textBody, SendGridUsername, SendGridPassword, SendGridSmtpHost, SendGridSmtpPort);
                        }
                    }
                }
                else
                {
                    var users = EmailReminder.GetUsers(context, information.numberOfDays, offeringId);
                    if (users != null)
                    {
                        var emailTemplateHTML = information.Template;
                        foreach (var user in users)
                        {
                            var template = emailTemplateHTML;
                            var htmlBody = template.Replace("[firstname]", user.FirstName);

                            var dealers = EmailReminder.GetDealerNearUser(context, user, offeringId);

                            htmlBody = htmlBody.Replace("[dealers]", dealers);

                            Trace.TraceInformation("Trying to send email to  " + user.EMail);
                            //Trace.WriteLine(htmlBody);

                            EmailReminder.SendEmailViaSendGrid(user.EMail, fromEmail, information.Subject, htmlBody, TFCtv_EMail_Reminder.MailType.HtmlOnly, String.Empty, SendGridUsername, SendGridPassword, SendGridSmtpHost, SendGridSmtpPort);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Error: " + e.Message);
                Trace.TraceInformation("Inner Exception: " + e.InnerException.Message);
            }
        }

        public static string GetEmailTemplate(string url)
        {
            var template = String.Empty;
            var webRequest = WebRequest.Create(url);
            webRequest.Timeout = 100000;
            while (String.IsNullOrEmpty(template))
            {
                try
                {
                    using (var response = webRequest.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var receiveStream = response.GetResponseStream();
                            if (receiveStream != null)
                            {
                                var stream = new StreamReader(receiveStream);
                                template = stream.ReadToEnd();
                            }
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            //return template;
                        }
                    }
                }
                catch (Exception)
                {
                    //if (e is WebException)
                    //{
                    //    WebException webEx = (WebException)e;
                    //    if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Forbidden)
                    //        return template;
                    //}
                }
            }

            return template;
        }

        public static IQueryable<User> GetUsers(IPTV2Entities context, int numberOfDays, int offeringId)
        {
            int verified = 1;
            try
            {
                var registDt = DateTime.Now;
                var userIdsWithExpiringEntitlements = context.Entitlements.Where(e => System.Data.Entity.DbFunctions.DiffDays(e.EndDate, registDt) == numberOfDays && e.OfferingId == offeringId).GroupBy(e => e.UserId)
                    .Select(e => e.Key);

                var userIdsWithEntitlements = context.Entitlements.Where(e => e.EndDate > registDt && e.OfferingId == offeringId).GroupBy(e => e.UserId)
                    .Select(e => e.Key);


                var userList = userIdsWithExpiringEntitlements.Except(userIdsWithEntitlements);

                if (Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("IsTestMode")))
                {
                    var TestEmails = RoleEnvironment.GetConfigurationSettingValue("TestEmails");
                    var TestEmailsArray = TestEmails.Split(',');
                    userList = context.Users.Where(u => TestEmailsArray.Contains(u.EMail)).Select(u => u.UserId);
                }

                var count = userList.Count();

                Trace.TraceInformation("Total Users Found: " + count);

                if (count > 0)
                {
                    var users = context.Users.Where(u => userList.Contains(u.UserId) && u.StatusId == verified);
                    return users;
                }
            }
            catch (Exception) { }
            return null;
        }

        public static string GetDealerNearUser(IPTV2Entities context, User user, int offeringId)
        {
            string dealers = String.Empty;
            try
            {
                string userIp = user.RegistrationIp;
                if (!String.IsNullOrEmpty(userIp))
                {
                    var ipLocation = MyUtility.getLocation(userIp);
                    GeoLocation location = new GeoLocation() { Latitude = ipLocation.latitude, Longitude = ipLocation.longitude };
                    SortedSet<StoreFrontDistance> result;
                    if (Convert.ToInt32(Settings.GetSettings("maximumDistance")) != 0)
                        result = StoreFront.GetNearestStores(context, offeringId, location, true, 1000);
                    else
                        result = StoreFront.GetNearestStores(context, offeringId, location, true);
                    StringBuilder sb = new StringBuilder();
                    var ctr = 0;
                    foreach (var item in result)
                    {
                        var email = item.Store.EMailAddress;
                        var fullAddress = String.Format("{0}, {1}, {2} {3}", item.Store.Address1, item.Store.City, item.Store.State, item.Store.ZipCode);
                        string li = String.Format("<li>{0}<br />Address: {1}<br />Phone: {2}</li>", item.Store.BusinessName, fullAddress, item.Store.BusinessPhone);
                        sb.AppendLine(li);
                        ctr++;
                        if (ctr + 1 > 3)
                            break;
                    }

                    dealers = sb.ToString();
                }
            }
            catch (Exception e) { Trace.WriteLine(e.Message); }
            return dealers;
        }

        public static void SendEmailViaSendGrid(string to, string from, string subject, string htmlBody, MailType type, string textBody, string SendGridUsername, string SendGridPassword, string SendGridSmtpHost, int SendGridSmtpPort)
        {
            try
            {
                //var message = SendGrid.GenerateInstance();
                var message = new SendGridMessage();
                message.AddTo(to);
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
                Trace.TraceInformation("Successfully sent email to " + to);
            }
            catch (Exception)
            {
                Trace.TraceInformation("Unable to send to " + to);
                throw;
            }
        }

        public static IQueryable<RecurringBilling> GetRecurringUsers(IPTV2Entities context, int numberOfDays, int offeringId)
        {
            int count = 0;
            try
            {
                var registDt = DateTime.Now.Date.AddDays(Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("numOfDaysRecurringUsers")));
                var usersWithRecurring = context.RecurringBillings.Where(r => r.OfferingId == offeringId && r.StatusId == 1 && System.Data.Entity.DbFunctions.DiffDays(r.EndDate.Value, registDt) == numberOfDays);

                if (Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("IsTestMode")))
                {
                    var TestEmails = RoleEnvironment.GetConfigurationSettingValue("TestEmails");
                    var TestEmailsArray = TestEmails.Split(',');
                    var userList = context.Users.Where(u => TestEmailsArray.Contains(u.EMail)).Select(u => u.UserId);

                    List<RecurringBilling> tempList = new List<RecurringBilling>();

                    int productId = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("TestProductId"));
                    Product product = context.Products.Find(productId);
                    var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId);

                    foreach (var item in userList)
                    {
                        var rb = new RecurringBilling()
                        {
                            User = context.Users.FirstOrDefault(u => u.UserId == item),
                            Product = product,
                            Package = (Package)productPackage.Package,
                            Offering = product.Offering,
                            EndDate = DateTime.Now.Date,
                            NextRun = DateTime.Now.AddDays(-3).Date,
                            StatusId = 1
                        };
                        tempList.Add(rb);
                    }
                    usersWithRecurring = tempList.AsQueryable();
                }
                else
                {
                    var lastSentEmail = DateTime.Now;
                    foreach (var user in usersWithRecurring)
                    {
                        user.LastSentEmail = lastSentEmail;
                    }
                    context.SaveChanges();
                }
                count = usersWithRecurring.Count();
                Trace.TraceInformation("Total Users Found: " + count);

                if (count > 0)
                    return usersWithRecurring;

            }
            catch (Exception) { }
            return null;
        }

        public static DateTime GetEntitlementEndDate(int duration, string interval, DateTime registDt)
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
