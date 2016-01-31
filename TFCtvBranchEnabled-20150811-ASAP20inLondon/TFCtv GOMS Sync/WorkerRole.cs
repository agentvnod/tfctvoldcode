using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using GOMS_TFCtv;
using IPTV2_Model;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace TFCtv_GOMS_Sync
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {

            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("$projectname$ entry point called", "Information");

            int waitDuration = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("LoopWaitDurationInSeconds"));
            int fillCacheDurationInMinutes = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("FillCacheDurationInMinutes"));
            bool fillCache = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("FillCache"));
            while (true)
            {
                try
                {
                    Thread.Sleep(TimeSpan.FromSeconds(waitDuration));
                    int offeringId = 2;
                    var service = new GomsTfcTv();
                    var context = new IPTV2Entities();
                    var offering = context.Offerings.Find(offeringId);
                    Trace.WriteLine("Process start.");
                    // Update Forex
                    // service.GetExchangeRates(context, "USD");
                    // Process Transactions
                    // service.ProcessAllPendingTransactionsInGoms(context, offering);
                    Trace.WriteLine("Process finish.");
                }
                catch (Exception ex)
                {
                    Trace.TraceError("fail in Run", ex);
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += RoleEnvironmentChanging;

            return base.OnStart();
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