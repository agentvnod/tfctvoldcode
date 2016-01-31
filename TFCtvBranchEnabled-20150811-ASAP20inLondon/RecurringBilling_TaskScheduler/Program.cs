using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using System.Configuration;
using Quartz.Impl;
using System.Threading;
using RecurringBilling_TS;

namespace RecurringBilling_TaskScheduler
{
    class Program
    {
        static IScheduler sched;
        static string cronExpressionRecurring = ConfigurationManager.AppSettings["cronExpressionRecurring"];

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Recurring Billing Task Scheduler!");

            ISchedulerFactory schedFact = new StdSchedulerFactory();
            sched = schedFact.GetScheduler();
            sched.Start();

            bool processRecurring = Convert.ToBoolean(ConfigurationManager.AppSettings["processRecurring"]);
            if (processRecurring)
            {
                Console.WriteLine("Creating job...");
                EmailInformation information = new EmailInformation() { jobName = "ProcessRecurring" };                
                CreateScheduledJob(information);
            }

            while (true)
            {
                Thread.Sleep(7200000);
                Console.WriteLine("Thread is sleeping. DateTime: " + DateTime.UtcNow);
            }

        }

        private static void CreateScheduledJob(EmailInformation information)
        {
            IJobDetail jobDetail = JobBuilder.Create<RecurringProcessor>()
                      .WithIdentity(String.Format("myJob-{0}", information.jobName), "Group1")
                      .Build();

            jobDetail.JobDataMap["Information"] = information;

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(String.Format("DailyTrigger-{0}", information.jobName), "Group1")
                .StartNow()
                .WithCronSchedule(cronExpressionRecurring)                
                .Build();

            sched.ScheduleJob(jobDetail, trigger);

            var nextFireTime = trigger.GetNextFireTimeUtc();
            Console.WriteLine("Scheduled job for recurring billing has been created.");
            Console.WriteLine("Next Fire Time: " + nextFireTime.Value);
        }
    }
}
