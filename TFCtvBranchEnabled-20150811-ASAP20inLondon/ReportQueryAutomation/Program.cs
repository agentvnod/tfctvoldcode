using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using System.Configuration;
using Quartz.Impl;
using System.Threading;


namespace ReportQueryAutomation
{
    class Program
    {
        static IScheduler sched;
        static string cronExpressionRecurring = "0 0/1 * 1/1 * ? *";
        //static string cronExpressionRecurring = "0 0 12 1/1 * ? *";

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Report Server Updating Scheduler!");

            ISchedulerFactory schedFact = new StdSchedulerFactory();
            sched = schedFact.GetScheduler();
            sched.Start();


            Console.WriteLine("Creating job...");

            CreateScheduledJob();


            //while (true)
            //{
            //    Thread.Sleep(60000);
            //    Console.WriteLine("Thread is sleeping. DateTime: " + DateTime.UtcNow);
            //}

        }

        private static void CreateScheduledJob()
        {
            IJobDetail jobDetail = JobBuilder.Create<ScheduledUpdate>()
                      .WithIdentity(String.Format("myJob-{0}", "ReportQueryJob"), "Group1")
                      .Build();



            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(String.Format("DailyTrigger-{0}", "ReportQueryJob"), "Group1")
                .StartNow()
                //.WithCronSchedule(cronExpressionRecurring)
                .Build();

            sched.ScheduleJob(jobDetail, trigger);

            var nextFireTime = trigger.GetNextFireTimeUtc();
            Console.WriteLine("Scheduled job for report server update has been created.");
            Console.WriteLine("Next Fire Time: " + nextFireTime.Value);
        }
    }
}
