using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PacmayEntitlementInsertion
{
    class Program
    {
        static IScheduler sched;
        static string cronExpressionRecurring = ConfigurationManager.AppSettings["cronExpressionRecurring"];

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to PacMay Entitlement Insertion Task Scheduler!");

            ISchedulerFactory schedFact = new StdSchedulerFactory();
            sched = schedFact.GetScheduler();
            sched.Start();

            bool processRecurring = Convert.ToBoolean(ConfigurationManager.AppSettings["processRecurring"]);
            if (processRecurring)
            {
                Console.WriteLine("Creating job...");
                JobInfo information = new JobInfo() { jobName = "ProcessRecurring" };
                CreateScheduledJob(information);
            }

            while (true)
            {
                Thread.Sleep(7200000);
                Console.WriteLine("Thread is sleeping. DateTime: " + DateTime.UtcNow);
            }
        }

        private static void CreateScheduledJob(JobInfo information)
        {
            IJobDetail jobDetail = JobBuilder.Create<EntitlementProcessor>()
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
            Console.WriteLine("Scheduled job for PacMay Entitlement Insertion has been created.");
            Console.WriteLine("Next Fire Time: " + nextFireTime.Value);
        }
    }
}
