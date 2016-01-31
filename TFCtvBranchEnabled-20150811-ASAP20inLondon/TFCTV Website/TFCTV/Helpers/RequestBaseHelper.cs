using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFCTV.Helpers
{
    public static class RequestBaseHelper
    {
        public static string GetUserHostAddressFromCloudflare(this HttpRequestBase request)
        {
            return request.ServerVariables["HTTP_CF_CONNECTING_IP"] ?? request.UserHostAddress;
        }

        public static string GetUserHostAddressFromCloudflare(this HttpRequest request)
        {
            return request.ServerVariables["HTTP_CF_CONNECTING_IP"] ?? request.UserHostAddress;
        }

        public static string GetUserHostCountryFromCloudflare(this HttpRequestBase request)
        {
            var code = request.ServerVariables["HTTP_CF_IPCOUNTRY"];
            if (String.IsNullOrEmpty(code) || String.Compare(code, "XX", true) == 0)
                code = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
            return code;
        }

        public static string GetUserHostCountryFromCloudflare(this HttpRequest request)
        {
            var code = request.ServerVariables["HTTP_CF_IPCOUNTRY"];
            if (String.IsNullOrEmpty(code) || String.Compare(code, "XX", true) == 0)
                code = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
            return code;
        }




    }
}