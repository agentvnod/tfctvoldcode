using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace TFCtv_Admin_Website.Helpers
{
    public class MyUtility
    {
        public static Dictionary<string, object> SetError(ErrorCode code, string message)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary["StatusCode"] = code;
            dictionary["StatusMessage"] = message;
            return dictionary;
        }

        public static DateTime GetEntitlementEndDate(int duration, string interval, DateTime registDt)
        {
            DateTime d = DateTime.Now;
            switch (interval)
            {
                case "d": d = registDt.AddDays(duration); break;
                case "m": d = registDt.AddMonths(duration); break;
                case "y": d = registDt.AddYears(duration); break;
                case "h": d = registDt.AddHours(duration); break;
                case "mm": d = registDt.AddMinutes(duration); break;
                case "s": d = registDt.AddSeconds(duration); break;
            }
            return d;
        }

        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        public static string BuildJSON(Dictionary<string, object> collection)
        {
            return JsonConvert.SerializeObject(collection, Formatting.None);
        }

        public static bool GetCheckBoxValue(HttpRequestBase req, string name)
        {
            return Convert.ToBoolean(req.Form.GetValues(name).First());
        }

        public static bool IsIpAddressAllowed(HttpRequestBase req)
        {
            try
            {
                var listOfIpAddresses = Global.IpWhitelist;
                var list = listOfIpAddresses.Split(';');
                return list.Contains(req.UserHostAddress);
            }
            catch (Exception) { return false; }
        }

        public static bool IsUserAllowed()
        {
            try
            {
                var listOfUsers = Global.UserWhitelist;
                var list = listOfUsers.Split(',');
                return list.Contains(HttpContext.Current.User.Identity.Name);
            }
            catch (Exception) { return false; }
        }

        public static string GetQuotableQuote(string url)
        {
            string s = String.Empty;
            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                Stream data = client.OpenRead(url);
                StreamReader reader = new StreamReader(data);
                s = reader.ReadToEnd();

                data.Close();
                reader.Close();
            }
            catch (Exception) { }
            return s;
        }

        public static string ReplaceWithPMDBitRate(string input, string bitrate)
        {
            string result = String.Empty;
            var pattern = @",(\d+,){1,}";
            try
            {
                result = System.Text.RegularExpressions.Regex.Replace(input, pattern, bitrate);
            }
            catch (Exception) { }
            return result;
        }
    }
}