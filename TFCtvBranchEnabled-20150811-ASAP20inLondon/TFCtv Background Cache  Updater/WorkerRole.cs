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
using System.Configuration;
using System.Web;
using EngagementsModel;

namespace TFCtv_Background_Cache__Updater
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
            string categoryIdsToCache = RoleEnvironment.GetConfigurationSettingValue("CategoryIdsToCache");
            var categoryList = GetIdListFromString(categoryIdsToCache);
            var cacheDuration = new TimeSpan(0, fillCacheDurationInMinutes, 0);
            int offeringId = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("OfferingId"));
            int serviceId = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("ServiceId"));
            int SOCIAL_LOVE = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("SocialLove"));
            int SOCIAL_RATING = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("SocialRating"));
            int FreeTvCategoryId = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("FreeTvCategoryId"));
            int Entertainment = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue("entertainmentCategoryId"));
            string AssetsBaseUrl = RoleEnvironment.GetConfigurationSettingValue("AssetsBaseUrl");
            string DefaultCurrency = RoleEnvironment.GetConfigurationSettingValue("DefaultCurrency");
            bool isGetPackagesFillCacheEnabled = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("isGetPackagesFillCacheEnabled"));
            bool isSynapseFillCacheEnabled = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("isSynapseFillCacheEnabled"));
            bool isPackageFeatureCacheEnabled = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("isPackageFeatureCacheEnabled"));
            bool isSubscriptionCacheEnabled = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("isSubscriptionCacheEnabled"));
            // Menu Group
            var listOfMenuNames = RoleEnvironment.GetConfigurationSettingValue("MenuNames").Split(',');

            // Testing
            //fillCache = true;

            while (true)
            {
                try
                {
                    Trace.TraceInformation("Process start...");

                    var context = new IPTV2Entities();
                    context.Database.Connection.ConnectionString = RoleEnvironment.GetConfigurationSettingValue("IPTV2EntitiesConnectionString");
                    var socialContext = new EngagementsEntities();
                    socialContext.Database.Connection.ConnectionString = RoleEnvironment.GetConfigurationSettingValue("EngagementsEntitiesConnectionString");
                    var offering = context.Offerings.Find(offeringId);
                    if (fillCache)
                    {
                        Trace.TraceInformation("Package Cache - Loading...");
                        var packageCacheRefresher = new PackageCacheRefresher();
                        packageCacheRefresher.FillCache(context, offeringId, RightsType.Online, cacheDuration);
                        Trace.TraceInformation("Package Cache - Loading Completed...");

                        Trace.TraceInformation("Category Cache - Loading...");
                        var categoryCacheRefresher = new CategoryCacheRefresher();
                        categoryCacheRefresher.FillCache(context, offeringId, serviceId, categoryList, RightsType.Online, cacheDuration);
                        Trace.TraceInformation("Category Cache - Loading Completed...");

                        Trace.TraceInformation("MenuGroupCacheRefresher Cache - Loading...");
                        var menuGroupCacheRefresher = new MenuGroupCacheRefresher();
                        menuGroupCacheRefresher.FillCache(context, offeringId, serviceId, cacheDuration, listOfMenuNames);
                        Trace.TraceInformation("MenuGroupCacheRefresher Cache - Loading Completed...");

                        if (isPackageFeatureCacheEnabled)
                        {
                            Trace.TraceInformation("PackageFeatureCacheRefresher Cache - Loading...");
                            var packageFeatureCacheRefresher = new PackageFeatureCacheRefresher();
                            packageFeatureCacheRefresher.FillCache(context, cacheDuration, offeringId, serviceId);
                            Trace.TraceInformation("PackageFeatureCacheRefresher Cache - Loading Completed...");
                        }

                        Trace.TraceInformation("EngagementCacheRefresher Cache - Loading...");
                        var engagementCacheRefresher = new EngagementCacheRefresher();
                        Trace.TraceInformation("Most Love Episode Cache - Loading...");
                        engagementCacheRefresher.FillMostLovedEpisodeCache(context, socialContext, offeringId, serviceId, cacheDuration, SOCIAL_LOVE, FreeTvCategoryId);
                        Trace.TraceInformation("Most Love Episode Cache - Loading Completed...");
                        Trace.TraceInformation("Most Love Show Cache - Loading...");
                        engagementCacheRefresher.FillMostLovedShowCache(context, socialContext, offeringId, serviceId, cacheDuration, SOCIAL_LOVE, Entertainment);
                        Trace.TraceInformation("Most Love Show Cache - Loading Completed...");
                        Trace.TraceInformation("Most Love Celebrity Cache - Loading...");
                        engagementCacheRefresher.FillMostLovedCelebrityCache1(context, socialContext, offeringId, serviceId, cacheDuration, SOCIAL_LOVE);
                        Trace.TraceInformation("Most Love Celebrity Cache - Loading Completed...");
                        Trace.TraceInformation("Top Reviewers Cache - Loading...");
                        engagementCacheRefresher.FillTopReviewersCache1(context, socialContext, offeringId, serviceId, cacheDuration, SOCIAL_RATING, AssetsBaseUrl);
                        Trace.TraceInformation("Top Reviewers Cache - Loading Completed...");
                        Trace.TraceInformation("EngagementCacheRefresher Cache - Loading Completed...");

                        if (isSubscriptionCacheEnabled)
                        {
                            Trace.TraceInformation("SubscriptionProductC Cache - Loading...");
                            var subscriptionProductCCacheRefresher = new SubscriptionProductCCacheRefresher();
                            subscriptionProductCCacheRefresher.FillCache(context, offeringId, RightsType.Online, cacheDuration);
                            Trace.TraceInformation("SubscriptionProductC Cache - Loading Completed...");
                        }

                        if (isSynapseFillCacheEnabled)
                        {
                            Trace.TraceInformation("SynapseCacheRefresher Cache - Loading...");
                            var synapseCacheRefresher = new SynapseCacheRefresher();
                            synapseCacheRefresher.FillCache(context, cacheDuration, categoryList, offeringId, serviceId);
                            Trace.TraceInformation("SynapseCacheRefresher Cache - Loading Completed...");
                        }

                        //Trace.TraceInformation("All Subscription Products - Loading...");
                        //packageCacheRefresher.FillCacheOfAllSubscriptionProducts(context, offeringId, RightsType.Online, cacheDuration);
                        //Trace.TraceInformation("All Subscription Products - Loading Completed...");
                        if (isGetPackagesFillCacheEnabled)
                        {
                            Trace.TraceInformation("All Ala Carte Products - Loading...");
                            packageCacheRefresher.FillCacheOfAllAlaCarteProducts(context, offeringId, DefaultCurrency, RightsType.Online, cacheDuration);
                            Trace.TraceInformation("All Ala Carte Products - Loading Completed...");

                            Trace.TraceInformation("All Package Products - Loading...");
                            packageCacheRefresher.FillCacheOfAllPackageProducts(context, offeringId, DefaultCurrency, RightsType.Online, cacheDuration);
                            Trace.TraceInformation("All Package Products - Loading Completed...");

                            Trace.TraceInformation("All Package And Ala Carte Product - Loading...");
                            packageCacheRefresher.FillCacheOfAllPackageAndProduct(context, offeringId, DefaultCurrency, RightsType.Online, cacheDuration);
                            Trace.TraceInformation("All Package And Ala Carte Product - Loading Completed...");
                        }



                    }


                    Trace.TraceInformation("Process end...");
                }
                catch (Exception ex)
                {
                    Trace.TraceError("fail in Run: " + ex.Message, ex);
                }
                Thread.Sleep(waitDuration);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            //Commented out rebooting of instance

            try
            {
                if (!IsRunningInDevFabric())
                {
                    DiagnosticMonitorConfiguration dmConfig = DiagnosticMonitor.GetDefaultInitialConfiguration();
                    // Configure the collection  of cache diagnostic data.
                    Microsoft.ApplicationServer.Caching.AzureCommon.CacheDiagnostics.ConfigureDiagnostics(dmConfig);
                    DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", dmConfig);
                }
            }
            catch (Exception) { }

            RoleEnvironment.Changing += RoleEnvironmentChanging;


            if (IsRunningInDevFabric())
            {
                Trace.WriteLine("Running in DevFabric");

                // update connection string settings
                var connectionStringKey = "Iptv2Entities";
                var iptv2ConnectionString = RoleEnvironment.GetConfigurationSettingValue(connectionStringKey);
                if (iptv2ConnectionString != null)
                {
                    var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var section = (ConnectionStringsSection)configFile.GetSection("connectionStrings");
                    if (section.ConnectionStrings["Iptv2Entities"] != null)
                    {
                        section.ConnectionStrings["Iptv2Entities"].ConnectionString = HttpUtility.HtmlDecode(iptv2ConnectionString);
                        configFile.Save();
                        ConfigurationManager.RefreshSection("connectionStrings");
                    }
                }

                var engagementsEntities = "EngagementsEntities";
                var engagementsEntitiesConnectionString = RoleEnvironment.GetConfigurationSettingValue(engagementsEntities);
                if (engagementsEntities != null)
                {
                    var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var section = (ConnectionStringsSection)configFile.GetSection("connectionStrings");
                    if (section.ConnectionStrings["EngagementsEntities"] != null)
                    {
                        section.ConnectionStrings["EngagementsEntities"].ConnectionString = HttpUtility.HtmlDecode(engagementsEntitiesConnectionString);
                        configFile.Save();
                        ConfigurationManager.RefreshSection("EngagementsEntities");
                    }
                }
            }


            return base.OnStart();
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                e.Cancel = true;
            }
        }

        private bool IsRunningInDevFabric()
        {
            // easiest check: try translate deployment ID into guid
            Guid guidId;
            if (Guid.TryParse(RoleEnvironment.DeploymentId, out guidId))
                return false;   // valid guid? We're in Azure Fabric
            return true;        // can't parse into guid? We're in Dev Fabric
        }


        public static IList<int> GetIdListFromString(string idList)
        {
            string[] values = idList.Split(',');

            List<int> ids = new List<int>(values.Length);

            foreach (string s in values)
            {
                int i;

                if (int.TryParse(s, out i))
                {
                    ids.Add(i);
                }
            }

            return ids;
        }

    }
}
