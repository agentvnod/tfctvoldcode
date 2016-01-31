using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TFCTV.Helpers
{
    static class NameValueCollectionExtensions
    {
        public static string ToQueryString(this NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            return string.Join("&", array);
        }
    }

    public static class ExtensionMethods
    {
        public static int Floor(this int i, int divisor)
        {
            return ((int)Math.Floor(i / new decimal(divisor))) * divisor;
        }

        public static bool IsAjaxCrawlingCapable(this HttpRequestBase request)
        {
            try
            {
                if (request.QueryString.AllKeys.Contains("_escaped_fragment_"))
                    return true;
                else
                {
                    var array = request.QueryString.GetValues(null);
                    if (array != null)
                    {
                        if (array.Length > 0)
                            if (array.Contains("_escaped_fragment_"))
                                return true;
                    }
                }
            }
            catch (Exception) { }
            return false;
        }

        public static DateTime TimeStampStringToDateTime(this string timestamp)
        {
            DateTime dt = DateTime.Now;
            try
            {
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dt = origin.AddMilliseconds(Convert.ToInt64(timestamp));
            }
            catch (Exception) { }
            return dt;
        }
    }

    public class EscapeQuoteConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString().Replace("'", "\u0027").Replace("&", "\u0026"));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = JToken.Load(reader).Value<string>();
            return value.Replace("\u0027", "'").Replace("\u0026", "&");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}