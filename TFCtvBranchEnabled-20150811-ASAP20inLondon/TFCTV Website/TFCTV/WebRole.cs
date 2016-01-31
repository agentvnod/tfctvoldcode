using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.ApplicationServer.Caching;

namespace TFCtv_WebRole
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            try
            {
                if (!IsRunningInDevFabric())
                {
                    DiagnosticMonitorConfiguration dmConfig = DiagnosticMonitor.GetDefaultInitialConfiguration();
                    // Configure the collection  of cache diagnostic data.
                    Microsoft.ApplicationServer.Caching.AzureCommon.CacheDiagnostics.ConfigureDiagnostics(dmConfig);
                    DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", dmConfig);
                }
                else
                    DataCacheClientLogManager.SetSink(DataCacheTraceSink.DiagnosticSink, System.Diagnostics.TraceLevel.Verbose);
            }
            catch (Exception) { }

            RoleEnvironment.Changed += RoleEnvironmentChanged;
            return base.OnStart();
        }

        private void RoleEnvironmentChanged(object sender, RoleEnvironmentChangedEventArgs e)
        {
            //var settingChanges = e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>();
            //if (settingChanges.Count() > 0) // Configuration setting values changed
            //{
            // Initialize all GlobalConfig values
            TFCTV.Helpers.GlobalConfig.Initialize();
            //}
        }

        private bool IsRunningInDevFabric()
        {
            // easiest check: try translate deployment ID into guid
            Guid guidId;
            if (Guid.TryParse(RoleEnvironment.DeploymentId, out guidId))
                return false;   // valid guid? We're in Azure Fabric
            return true;        // can't parse into guid? We're in Dev Fabric
        }
    }
}
