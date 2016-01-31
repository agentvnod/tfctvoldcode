using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Maxmind
{
    public class Utility
    {
        static MemoryStream ms;
        static LookupService ls;

        static Utility()
        {
        }

        public static void Init(Stream stream)
        {
            if (ls == null)
            {
                ls = new LookupService(stream);
            }

        }

        public static void Init(CloudPageBlob blob)
        {
            if (ls == null)
            {
                ms = new MemoryStream();
                var geoIpFileStream = blob.OpenRead();
                geoIpFileStream.CopyTo(ms);
                ls = new LookupService(ms);                
            }

        }

        public static Maxmind.Country getCountry(string ip)
        {
            return (ls == null) ? null : ls.getCountry(ip);
        }

        public static Maxmind.Location getLocation(string ip)
        {
            return (ls == null) ? null : ls.getLocation(ip);
        }

    }
}
