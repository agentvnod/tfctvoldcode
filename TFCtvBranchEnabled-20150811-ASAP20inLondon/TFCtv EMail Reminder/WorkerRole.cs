using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using IPTV2_Model;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System.Configuration;
using System.Web;

namespace TFCtv_EMail_Reminder
{
    public class WorkerRole : RoleEntryPoint
    {

        static string AssetBaseUrl = RoleEnvironment.GetConfigurationSettingValue("AssetBaseUrl");


        static string subscription3DaysBeforeTemplate = AssetBaseUrl + RoleEnvironment.GetConfigurationSettingValue("subscription3DaysBeforeTemplate");
        static string subscription1DayAfterTemplate = AssetBaseUrl + RoleEnvironment.GetConfigurationSettingValue("subscription1DayAfterTemplate");
        static string subscription14DaysAfterTemplate = AssetBaseUrl + RoleEnvironment.GetConfigurationSettingValue("subscription14DaysAfterTemplate");
        static string subscription30DaysAfterTemplate = AssetBaseUrl + RoleEnvironment.GetConfigurationSettingValue("subscription30DaysAfterTemplate");
        static string recurring4DaysBeforeTemplate = AssetBaseUrl + RoleEnvironment.GetConfigurationSettingValue("recurring4DaysBeforeTemplate");

        static string subscription3DaysBeforeSubject = RoleEnvironment.GetConfigurationSettingValue("subscription3DaysBeforeSubject");
        static string subscription1DayAfterSubject = RoleEnvironment.GetConfigurationSettingValue("subscription1DayAfterSubject");
        static string subscription14DaysAfterSubject = RoleEnvironment.GetConfigurationSettingValue("subscription14DaysAfterSubject");
        static string subscription30DaysAfterSubject = RoleEnvironment.GetConfigurationSettingValue("subscription30DaysAfterSubject");
        static string recurring4DaysBeforeSubject = RoleEnvironment.GetConfigurationSettingValue("recurring4DaysBeforeSubject");

        static string cronExpression = RoleEnvironment.GetConfigurationSettingValue("cronExpression");

        static string cronExpressionRecurring = RoleEnvironment.GetConfigurationSettingValue("cronExpressionRecurring");
        //*/5 * * * *

        static IScheduler sched;

        public override void Run()
        {
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            sched = schedFact.GetScheduler();
            sched.Start();
          
            //Test
            if (Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("IsTestMode")))
                cronExpression = RoleEnvironment.GetConfigurationSettingValue("cronExpressionTest");


            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("TFCtv EMail Reminder entry point called", "Information");

            Trace.TraceInformation("Time now is " + DateTime.UtcNow);

            bool sendEmail = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("sendEmail"));

            bool sendEmail3DaysBefore = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("sendEmail3DaysBefore"));
            bool sendEmail1DayAfter = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("sendEmail1DayAfter"));
            bool sendEmail14DaysAfter = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("sendEmail14DaysAfter"));
            bool sendEmail30DaysAfter = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("sendEmail30DaysAfter"));
            bool sendEmail4DaysBeforeRecurringBilling = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("sendEmail4DaysBeforeRecurring"));


            if (sendEmail)
            {
                if (sendEmail3DaysBefore)
                {
                    Trace.TraceInformation("Sending Emails (3 days before)");
                    Trace.TraceInformation("Template: " + subscription3DaysBeforeTemplate);

                    var emailTemplateHTML = EmailReminder.GetEmailTemplate(subscription3DaysBeforeTemplate);
                    EmailInformation information = new EmailInformation() { numberOfDays = -3, Subject = subscription3DaysBeforeSubject, Template = emailTemplateHTML, isRecurring = false, jobName = "3DaysBefore" };
                    CreateScheduledJob(information);
                }

                if (sendEmail1DayAfter)
                {
                    Trace.TraceInformation("Sending Emails (1 day after)");
                    Trace.TraceInformation("Template: " + subscription1DayAfterTemplate);

                    var emailTemplateHTML = EmailReminder.GetEmailTemplate(subscription1DayAfterTemplate);
                    EmailInformation information = new EmailInformation() { numberOfDays = 1, Subject = subscription1DayAfterSubject, Template = emailTemplateHTML, isRecurring = false, jobName = "1DayAfter" };
                    CreateScheduledJob(information);
                }

                if (sendEmail14DaysAfter)
                {
                    Trace.TraceInformation("Sending Emails (14 days after)");
                    Trace.TraceInformation("Template: " + subscription14DaysAfterTemplate);

                    var emailTemplateHTML = EmailReminder.GetEmailTemplate(subscription14DaysAfterTemplate);
                    EmailInformation information = new EmailInformation() { numberOfDays = 14, Subject = subscription14DaysAfterSubject, Template = emailTemplateHTML, isRecurring = false, jobName = "4DaysAfter" };
                    CreateScheduledJob(information);
                }

                if (sendEmail30DaysAfter)
                {
                    Trace.TraceInformation("Sending Emails (30 days after)");
                    Trace.TraceInformation("Template: " + subscription30DaysAfterTemplate);

                    var emailTemplateHTML = EmailReminder.GetEmailTemplate(subscription30DaysAfterTemplate);
                    EmailInformation information = new EmailInformation() { numberOfDays = 30, Subject = subscription30DaysAfterSubject, Template = emailTemplateHTML, isRecurring = false, jobName = "14DaysAfter" };
                    CreateScheduledJob(information);
                }

                if (sendEmail4DaysBeforeRecurringBilling)
                {
                    Trace.TraceInformation("Sending Emails (Recurring Billings 4 days before)");
                    Trace.TraceInformation("Template: " + recurring4DaysBeforeTemplate);

                    var emailTemplateHTML = EmailReminder.GetEmailTemplate(recurring4DaysBeforeTemplate);
                    EmailInformation information = new EmailInformation() { numberOfDays = -4, Subject = recurring4DaysBeforeSubject, Template = emailTemplateHTML, isRecurring = true, jobName = "4DaysBeforeRecurring" };
                    CreateScheduledJob(information);

                }

                //if (sendEmail3DaysBefore)
                //    SendMail(context, subscription3DaysBeforeTemplate, -3, subscription3DaysBeforeSubject);
                //if (sendEmail1DayAfter)
                //    SendMail(context, subscription1DayAfterTemplate, 1, subscription1DayAfterSubject);
                //if (sendEmail7DaysBefore)
                //    SendMail(context, subscription14DaysAfterTemplate, 14, subscription14DaysAfterSubject);
            }

            Trace.TraceInformation("TFCtv Recurring Billing entry point called", "Information");
            bool processRecurring = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("processRecurring"));
            if (processRecurring)
            {
                Trace.TraceInformation("Processing Recurring Billing");
                EmailInformation information = new EmailInformation() { jobName = "ProcessRecurring" };
                //CreateRecurringScheduledJob(information);
                CreateScheduledJob(information);
            }

            while (true)
            {
                Thread.Sleep(10000);
                Trace.WriteLine("Thread is sleeping. DateTime: " + DateTime.UtcNow);
                //Trace.WriteLine("Working", "Information");
            }
        }


        public static void CreateScheduledJob(EmailInformation information)
        {
            //EmailReminder reminder = new EmailReminder(offeringId, fromEmail, SendGridUsername, SendGridPassword, SendGridSmtpHost, SendGridSmtpPort);
            //EmailReminder reminder = new EmailReminder();
            //JobDetailImpl jobDetail = new JobDetailImpl(String.Format("myJob-{0}", numberOfDays), null, reminder.GetType());
            //CronTriggerImpl trigger = new CronTriggerImpl(String.Format("DailyTrigger-{0}", numberOfDays), "Group1", cronExpression);

            IJobDetail jobDetail = null;
            var jobCronExpression = cronExpression;
            if (String.Compare(information.jobName, "ProcessRecurring", true) == 0)
            {
                jobDetail = JobBuilder.Create<RecurringProcessor>()
                     .WithIdentity(String.Format("myJob-{0}", information.jobName), "Group1")
                     .Build();
                jobCronExpression = cronExpressionRecurring;
            }
            else
            {
                jobDetail = JobBuilder.Create<EmailReminder>()
                        .WithIdentity(String.Format("myJob-{0}", information.jobName), "Group1")
                        .Build();
                jobCronExpression = cronExpression;

            }

            //IJobDetail jobDetail = JobBuilder.Create<EmailReminder>()
            //          .WithIdentity(String.Format("myJob-{0}", information.jobName), "Group1")
            //          .Build();

            jobDetail.JobDataMap["Information"] = information;

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(String.Format("DailyTrigger-{0}", information.jobName), "Group1")
                .StartNow()
                .WithCronSchedule(jobCronExpression)
                .Build();

            sched.ScheduleJob(jobDetail, trigger);

            var nextFireTime = trigger.GetNextFireTimeUtc();
            Trace.TraceInformation("Next Fire Time:" + nextFireTime.Value);
        }


        //public static void CreateRecurringScheduledJob(EmailInformation information)
        //{

        //    IJobDetail jobDetail1 = JobBuilder.Create<RecurringProcessor>()
        //            .WithIdentity(String.Format("myJob-{0}", information.jobName), "Group1")
        //            .Build();

        //    ITrigger trigger1 = TriggerBuilder.Create()
        //        .WithIdentity(String.Format("DailyTrigger-{0}", information.jobName), "Group1")
        //        .StartNow()
        //        .WithCronSchedule(cronExpressionRecurring)
        //        .Build();


        //    sched.ScheduleJob(jobDetail1, trigger1);

        //    var nextFireTime = trigger1.GetNextFireTimeUtc();
        //    Trace.TraceInformation("Next Fire Time:" + nextFireTime.Value);
        //}

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            DiagnosticMonitor.Start("TfcTvInternalStorage");
            //RoleEnvironment.Changing += RoleEnvironmentChanging;

            //if (IsRunningInDevFabric())
            //{
            //    //Trace.WriteLine("Running in DevFabric");
            //    Trace.WriteLine("Running in Windows Azure");

            //    // update connection string settings
            //    var connectionStringKey = "Iptv2EntitiesFull";
            //    var iptv2ConnectionString = RoleEnvironment.GetConfigurationSettingValue(connectionStringKey);
            //    if (iptv2ConnectionString != null)
            //    {
            //        var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //        var section = (ConnectionStringsSection)configFile.GetSection("connectionStrings");
            //        if (section.ConnectionStrings["Iptv2Entities"] != null)
            //        {
            //            section.ConnectionStrings["Iptv2Entities"].ConnectionString = HttpUtility.HtmlDecode(iptv2ConnectionString);
            //            configFile.Save();
            //            ConfigurationManager.RefreshSection("connectionStrings");
            //        }
            //    }               
            //}

            return base.OnStart();
        }

        private bool IsRunningInDevFabric()
        {
            // easiest check: try translate deployment ID into guid
            Guid guidId;
            if (Guid.TryParse(RoleEnvironment.DeploymentId, out guidId))
                return false;   // valid guid? We're in Azure Fabric
            return true;        // can't parse into guid? We're in Dev Fabric
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                e.Cancel = true;
            }
        }        
    }
}
