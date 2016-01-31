using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using System.Configuration;
using Quartz.Impl;
using System.Threading;

namespace VideoEngagementsTableCreator
{
    class Program
    {
        static IScheduler sched;
        static string cronExpression = ConfigurationManager.AppSettings["cronExpression"];

        static void Main(string[] args)
        {
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            sched = schedFact.GetScheduler();
            sched.Start();
            bool IsTableCreationEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["IsTableCreationEnabled"]);
            if (IsTableCreationEnabled)
            {
                Console.WriteLine("Creating job...");
                CreateScheduledJob();
            }
            while (true)
            {
                Thread.Sleep(7200000);
                Console.WriteLine("Thread is sleeping. DateTime: " + DateTime.UtcNow);
            }
        }

        private static void CreateScheduledJob()
        {
            IJobDetail jobDetail = JobBuilder.Create<VideoEngagementsTable>()
                      .WithIdentity("myJob-TableCreator", "Group1")
                      .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("DailyTrigger-TableCreator", "Group1")
                .StartNow()
                .WithCronSchedule(cronExpression)
                .Build();

            sched.ScheduleJob(jobDetail, trigger);

            var nextFireTime = trigger.GetNextFireTimeUtc();
            Console.WriteLine("Scheduled job for Table creation has been created.");
            Console.WriteLine("Next Fire Time: " + nextFireTime.Value);
        }
    }
}
