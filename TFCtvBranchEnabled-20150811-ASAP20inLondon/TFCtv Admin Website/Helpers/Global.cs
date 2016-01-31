using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace TFCtv_Admin_Website.Helpers
{
    public class Global
    {
        public static int OfferingId = Convert.ToInt32(ConfigurationManager.AppSettings["offeringId"]);
        public static string TrialCurrency = ConfigurationManager.AppSettings["TrialCurrency"];
        public static string DefaultCurrency = ConfigurationManager.AppSettings["DefaultCurrency"];
        public static string DefaultCountry = ConfigurationManager.AppSettings["DefaultCountry"];
        public static int AnonymousDefaultPackageId = Convert.ToInt32(ConfigurationManager.AppSettings["AnonymousDefaultPackageId"]);
        public static string hlsPrefixPattern = ConfigurationManager.AppSettings["hlsPrefixPattern"];
        public static string hlsSuffixPattern = ConfigurationManager.AppSettings["hlsSuffixPattern"];
        public static string zeriPrefixPattern = ConfigurationManager.AppSettings["zeriPrefixPattern"];
        public static string zeriSuffixPattern = ConfigurationManager.AppSettings["zeriSuffixPattern"];
        public static string httpPrefixPatternMobile = ConfigurationManager.AppSettings["httpPrefixPatternMobile"];
        public static string AkamaiTokenKey = ConfigurationManager.AppSettings["AkamaiTokenKey"];
        public static string AkamaiIosTokenKey = ConfigurationManager.AppSettings["AkamaiIosTokenKey"];
        public static string IpWhitelist = ConfigurationManager.AppSettings["IpWhitelist"];
        public static string UserWhitelist = ConfigurationManager.AppSettings["UserWhitelist"];
        // Progressive Media Bitrates
        public static string PMDHDBitrate = ConfigurationManager.AppSettings["PMDHDBitrate"];
        public static string PMDHighBitrate = ConfigurationManager.AppSettings["PMDHighBitrate"];
        public static string PMDLowBitrate = ConfigurationManager.AppSettings["PMDLowBitrate"];

        public static string hlsProgressivePrefixPattern = ConfigurationManager.AppSettings["hlsProgressivePrefixPattern"];
        public static string hlsProgressiveSuffixPattern = ConfigurationManager.AppSettings["hlsProgressiveSuffixPattern"];
        public static string zeriProgressivePrefixPattern = ConfigurationManager.AppSettings["zeriProgressivePrefixPattern"];
        public static string zeriProgressiveSuffixPattern = ConfigurationManager.AppSettings["zeriProgressiveSuffixPattern"];
        public static string httpProgressivePrefixPatternMobile = ConfigurationManager.AppSettings["httpProgressivePrefixPatternMobile"];
        public static string PMDSalt = ConfigurationManager.AppSettings["PMDSalt"];
        public static int AkamaiAddSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["AkamaiAddSeconds"]);
        public static bool IsIpWhitelistingEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["IsIpWhitelistingEnabled"]);

        public static string GSapikey = ConfigurationManager.AppSettings["GSapikey"];
        public static string GSsecretkey = ConfigurationManager.AppSettings["GSsecretkey"];
    }
}