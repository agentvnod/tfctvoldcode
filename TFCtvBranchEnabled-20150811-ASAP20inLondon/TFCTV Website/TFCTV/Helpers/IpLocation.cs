using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.ApplicationServer.Caching;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TFCTV.Helpers
{
    public class IpLocation
    {
        static MemoryStream maxMindDbStream = null;
        static DataCache cache;

        public IpLocation()
        {
            if (maxMindDbStream == null)
                Init();
            Maxmind.Utility.Init(maxMindDbStream);
        }

        private static void Init()
        {
            while (cache == null)
            {
                try
                {
                    DataCacheFactory cacheFactory = new DataCacheFactory();
                    cache = cacheFactory.GetDefaultCache();
                    Trace.WriteLine("IpLocation cache initialized.");
                }
                catch (Exception e)
                {
                    cache = null;
                    System.Threading.Thread.Sleep(2000);
                    Trace.WriteLine("Cannot initialize IpLocation cache. " + e.Message);
                }
            }

            while (maxMindDbStream == null)
            {

                //CloudBlobContainer container = new CloudBlobContainer(new Uri(Settings.GetSettings("MaxMindContainer")), AzureStorage.BlobClient.Credentials);
                CloudBlobContainer container = AzureStorage.BlobClient.GetContainerReference(Settings.GetSettings("MaxMindContainer"));
                var maxMindDbBlob = container.GetBlockBlobReference(Settings.GetSettings("MaxMindDatabase"));                                
                try
                {
                    maxMindDbStream = new MemoryStream();
                    if (RoleEnvironment.IsAvailable)
                    {
                        maxMindDbBlob.DownloadToStream(maxMindDbStream);
                        Trace.WriteLine("MaxMind DB transferred to memory.");
                    }
                    else
                    {
                        using (FileStream filestream = File.OpenRead(HttpContext.Current.Server.MapPath(Settings.GetSettings("GeoIpPath"))))
                        {
                            maxMindDbStream.SetLength(filestream.Length);
                            filestream.Read(maxMindDbStream.GetBuffer(), 0, (int)filestream.Length);
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Cannot initialize MaxMind database. " + e.Message);
                    maxMindDbStream = null;
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }

        public Maxmind.Country GetCountry(string ip)
        {
            Maxmind.Country country = null;
            if (RoleEnvironment.IsAvailable)
                country = (Maxmind.Country)cache.Get("IPC:" + ip);
            if (country == null)
            {
                country = Maxmind.Utility.getCountry(ip);
                if (RoleEnvironment.IsAvailable) cache.Put("IPC:" + ip, country);
            }
            // country = Maxmind.Utility.getCountry(ip);
            return (country);
        }

        public Maxmind.Location GetLocation(string ip)
        {
            Maxmind.Location location = null;
            if (RoleEnvironment.IsAvailable)
                location = (Maxmind.Location)cache.Get("IPL:" + ip);
            if (location == null)
            {
                location = Maxmind.Utility.getLocation(ip);
                if (RoleEnvironment.IsAvailable) cache.Put("IPL:" + ip, location);
            }
            // location = Maxmind.Utility.getLocation(ip);
            return (location);
        }
    }

    public class IpCountry
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }
}