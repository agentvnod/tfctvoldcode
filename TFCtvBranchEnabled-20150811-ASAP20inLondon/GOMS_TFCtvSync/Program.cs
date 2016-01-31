using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GOMS_TFCtv;
using IPTV2_Model;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Configuration;
using Quartz;
using Quartz.Impl;


namespace GOMS_TFCtvSync
{
    class Program
    {
        static IScheduler sched;
        static string cronExpression = ConfigurationManager.AppSettings["cronExpression"];

        static void Main(string[] args)
        {
            Console.WriteLine("Startup...");
            int waitDuration = 10;
            int maxThreads = Convert.ToInt32(ConfigurationManager.AppSettings["MaxThreads"]); ;
            bool loopThru = true;
            bool isSingleTransaction = false;

            //Create job to update exchange rate (daily)

            if (args.Length == 0)
            {
                ISchedulerFactory schedFact = new StdSchedulerFactory();
                sched = schedFact.GetScheduler();
                sched.Start();
                CreateScheduledJob();
            }

            while (loopThru)
            {
                try
                {
                    Console.WriteLine("Waiting...");
                    Thread.Sleep(TimeSpan.FromSeconds(waitDuration));
                    int offeringId = 2;
                    Console.WriteLine("Initializing Iptv2Context service...");
                    using (var context = new IPTV2Entities())
                    {
                        var offering = context.Offerings.Find(offeringId);
                        Console.WriteLine("Initializing GomsTfcTv service...");
                        var service = new GomsTfcTv();
                        Console.WriteLine("Process start.");
                        // Update Forex
                        Console.WriteLine("Updating FOREX...");
                        service.GetExchangeRates(context, "USD");
                        Console.WriteLine("Finished FOREX...");


                        // Process Single transactionsint check;                        
                        if (args.Length > 0)
                        {
                            try
                            {
                                // Process Single transactions
                                service.ProcessSinglePendingTransactionInGoms(context, offering, Convert.ToInt32(args[0]));
                                isSingleTransaction = true;
                            }
                            catch (Exception)
                            {
                                loopThru = false;
                            }
                        }
                        else
                            DoProcessByIncrement(offeringId, maxThreads);
                    }
                    Console.WriteLine("Process finish.");
                    if (isSingleTransaction)
                        Environment.Exit(0);
                    //DoProcess(offeringId, maxThreads);
                    //DoProcessByIncrement(offeringId, maxThreads);
                    // Process Transactions
                    // service.ProcessAllPendingTransactionsInGoms(context, offering);
                    // Process Single transactions
                    // service.ProcessSinglePendingTransactionInGoms(context, offering, Convert.ToInt32(args[0]));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error: " + ex.Message);
                    Trace.TraceError("fail in Run", ex);
                }
            }
        }

        private static void CreateScheduledJob()
        {
            IJobDetail jobDetail = JobBuilder.Create<ExchangeRate>()
                      .WithIdentity("myJob-UpdateExchangeRate", "Group1")
                      .Build();


            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("DailyTrigger-UpdateExchangeRate", "Group1")
                .StartNow()
                .WithCronSchedule(cronExpression)
                .Build();

            sched.ScheduleJob(jobDetail, trigger);

            var nextFireTime = trigger.GetNextFireTimeUtc();
            Console.WriteLine("Scheduled job for updating Forex has been created.");
            Console.WriteLine("Next Fire Time: " + nextFireTime.Value);
        }

        public class UserForProcessing
        {
            public int Status = 0;
            public Guid UserId { get; set; }
            public int OfferingId { get; set; }
            public int Count { get; set; }
        }

        public static List<UserForProcessing> GetUserIdsToProcess(int offeringId)
        {
            try
            {
                List<UserForProcessing> userIds = null;
                using (var context = new IPTV2Entities())
                {
                    var offering = context.Offerings.Find(offeringId);
                    var userIdList = from t in context.Transactions
                                     where t.GomsTransactionId == null
                                     //&& (from s in context.Users
                                     //     where s.StatusId == 1
                                     //     select (s.UserId)).Contains(t.UserId)
                                     group t by t.UserId into u
                                     orderby u.Count() descending
                                     select new UserForProcessing { UserId = u.Key, OfferingId = offeringId, Count = u.Count() };
                    userIds = userIdList.ToList();
                }
                return userIds;
            }
            catch (Exception) { throw; }
        }

        public static void ProcessUser(Object stateInfo)
        {
            try
            {
                UserForProcessing user = (UserForProcessing)stateInfo;
                using (var context = new IPTV2Entities())
                {
                    var thisUser = context.Users.Find(user.UserId);
                    var offering = context.Offerings.Find(user.OfferingId);
                    var service = new GomsTfcTv();
                    service.ProcessAllPendingTransactionsInGoms(context, offering, thisUser);
                    //var rand = new Random();
                    //Thread.Sleep(rand.Next(2000));
                    //Console.WriteLine("Processing for user {0}-{1}", user.UserId, System.DateTime.Now);
                    user.Status = 1;
                }
            }
            catch (Exception) { throw; }
        }

        public static void DoProcess(int offeringId, int maxThreads)
        {
            var userIds = GetUserIdsToProcess(offeringId);
            int workerThreadCount;
            int completionPortThreadCount;
            ThreadPool.SetMaxThreads(maxThreads, maxThreads);

            foreach (var user in userIds)
            {
                var thisUser = (UserForProcessing)user;
                //Console.WriteLine("Queueing user {0} - {1}", thisUser.UserId, System.DateTime.Now);
                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessUser), user);
            }

            var notProcessed = userIds.Count(u => u.Status == 0);
            while (notProcessed > 0)
            {
                ThreadPool.GetAvailableThreads(out workerThreadCount, out completionPortThreadCount);
                Console.WriteLine("Threads available: {0}, not processed: {1}", workerThreadCount, notProcessed);
                Thread.Sleep(1000);
                notProcessed = userIds.Count(u => u.Status == 0);
            }
        }

        public static void DoProcessByIncrement(int offeringId, int maxThreads)
        {
            try
            {
                int increment = Convert.ToInt32(ConfigurationManager.AppSettings["UserIncrement"]); ;
                int index = 0;
                int take = increment;
                var allUserIDs = GetUserIdsToProcess(offeringId);
                int userIDCount = allUserIDs.Count;

                do
                {
                    if (index > userIDCount)
                        take = userIDCount - (index - increment);
                    var userIds = allUserIDs.Skip(index).Take(take);
                    int workerThreadCount;
                    int completionPortThreadCount;
                    ThreadPool.SetMaxThreads(maxThreads, maxThreads);

                    foreach (var user in userIds)
                    {
                        var thisUser = (UserForProcessing)user;
                        //Console.WriteLine("Queueing user {0} - {1}", thisUser.UserId, System.DateTime.Now);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessUser), user);
                    }

                    var notProcessed = userIds.Count(u => u.Status == 0);
                    while (notProcessed > 0)
                    {
                        ThreadPool.GetAvailableThreads(out workerThreadCount, out completionPortThreadCount);
                        Console.WriteLine("Threads available: {0}, not processed: {1}", workerThreadCount, notProcessed);
                        Thread.Sleep(1000);
                        notProcessed = userIds.Count(u => u.Status == 0);
                    }

                    index += increment;
                } while (index <= (userIDCount + increment));
            }
            catch (Exception) { throw; }
        }
    }
}

