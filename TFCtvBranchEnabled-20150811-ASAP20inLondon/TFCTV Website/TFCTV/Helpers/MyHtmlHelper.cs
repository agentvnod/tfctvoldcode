using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Text;
using System.Text.RegularExpressions;

namespace TFCTV.Helpers
{
    public static class MyHtmlHelper
    {
        public static IHtmlString JsBuilder(this HtmlHelper htmlHelper, UrlHelper url, string value, bool removeProtocol = true)
        {
            TagBuilder builder = new TagBuilder("script");
            //builder.Attributes.Add("src", url.Content("~/Scripts/" + value));
            if (GlobalConfig.IsAssetsEnabled)
            {
                string protocol = "^http[s]*:";
                builder.Attributes.Add("src", url.Content(String.Format("{0}/scripts/{1}", removeProtocol ? Regex.Replace(GlobalConfig.AssetsBaseUrl, protocol, String.Empty) : GlobalConfig.AssetsBaseUrl, value)));
            }
            else
                builder.Attributes.Add("src", url.Content("~/Scripts/" + value));
            builder.Attributes.Add("type", "text/javascript");
            builder.Attributes.Add("language", "JavaScript");
            return htmlHelper.Raw(builder.ToString());
        }

        public static IHtmlString JsBuilder(this HtmlHelper htmlHelper, UrlHelper url, string value, ContentSource source)
        {
            TagBuilder builder = new TagBuilder("script");
            switch (source)
            {
                case ContentSource.Site:
                    builder.Attributes.Add("src", url.Content("~/Scripts/" + value));
                    break;
                case ContentSource.CDN:
                    if (GlobalConfig.IsCDNEnabled)
                        builder.Attributes.Add("src", url.Content(String.Format("{0}/Scripts/{1}", GlobalConfig.CDNBaseUrl, value)));
                    else
                        builder.Attributes.Add("src", url.Content("~/cdn/Scripts/") + value);
                    break;
                case ContentSource.Assets:
                    builder.Attributes.Add("src", url.Content(String.Format("{0}/scripts/{1}", GlobalConfig.AssetsBaseUrl, value)));
                    break;
            }
            builder.Attributes.Add("type", "text/javascript");
            builder.Attributes.Add("language", "JavaScript");
            return htmlHelper.Raw(builder.ToString());
        }

        public static IHtmlString CssBuilder(this HtmlHelper htmlHelper, UrlHelper url, string value, bool removeProtocol = true)
        {
            TagBuilder builder = new TagBuilder("link");
            if (GlobalConfig.IsAssetsEnabled)
            {
                string protocol = "^http[s]*:";
                builder.Attributes.Add("href", url.Content(String.Format("{0}/content/{1}", removeProtocol ? Regex.Replace(GlobalConfig.AssetsBaseUrl, protocol, String.Empty) : GlobalConfig.AssetsBaseUrl, value)));
            }
            else
                builder.Attributes.Add("href", url.Content("~/Content/" + value));
            builder.Attributes.Add("rel", "stylesheet");
            builder.Attributes.Add("type", "text/css");
            return htmlHelper.Raw(builder.ToString(TagRenderMode.SelfClosing));
        }

        public static IHtmlString CssBuilder(this HtmlHelper htmlHelper, UrlHelper url, string value, ContentSource source)
        {
            TagBuilder builder = new TagBuilder("link");
            switch (source)
            {
                case ContentSource.Site:
                    builder.Attributes.Add("href", url.Content("~/Content/" + value));
                    break;
                case ContentSource.CDN:
                    if (GlobalConfig.IsCDNEnabled)
                        builder.Attributes.Add("href", url.Content(String.Format("{0}/Content/{1}", GlobalConfig.CDNBaseUrl, value)));
                    else
                        builder.Attributes.Add("href", url.Content("~/cdn/Content/") + value);
                    break;
                case ContentSource.Assets:
                    builder.Attributes.Add("href", @url.Content(String.Format("{0}/content/{1}", GlobalConfig.AssetsBaseUrl, value)));
                    break;
            }
            builder.Attributes.Add("rel", "stylesheet");
            builder.Attributes.Add("type", "text/css");
            return htmlHelper.Raw(builder.ToString(TagRenderMode.SelfClosing));
        }

        //public static IHtmlString CssBuilder(this HtmlHelper htmlHelper, UrlHelper url, string value, bool removeProtocol = true)
        //{
        //    TagBuilder builder = new TagBuilder("link");
        //    if (GlobalConfig.IsAssetsEnabled)
        //    {
        //        string protocol = "^http[s]*:";
        //        builder.Attributes.Add("href", url.Content(String.Format("{0}/content/{1}", removeProtocol ? Regex.Replace(GlobalConfig.AssetsBaseUrl, protocol, String.Empty) : GlobalConfig.AssetsBaseUrl, value)));
        //    }
        //    else
        //        builder.Attributes.Add("href", url.Content("~/Content/" + value));
        //    builder.Attributes.Add("rel", "stylesheet");
        //    builder.Attributes.Add("type", "text/css");
        //    return htmlHelper.Raw(builder.ToString(TagRenderMode.SelfClosing));
        //}

        public static IHtmlString ImageBuilder(this HtmlHelper htmlHelper, UrlHelper url, string image, [System.Runtime.InteropServices.Optional, System.Runtime.InteropServices.DefaultParameterValue(null)] object htmlAttributes, bool removeProtocol = true)
        {
            TagBuilder builder = new TagBuilder("img");
            if (GlobalConfig.IsAssetsEnabled)
            {
                string protocol = "^http[s]*:";
                builder.Attributes.Add("src", url.Content(String.Format("{0}/content/images/{1}", removeProtocol ? Regex.Replace(GlobalConfig.AssetsBaseUrl, protocol, String.Empty) : GlobalConfig.AssetsBaseUrl, image)));
            }
            else
                builder.Attributes.Add("src", url.Content("~/Content/images/") + image);
            builder.Attributes.Add("border", "0");
            builder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            return htmlHelper.Raw(builder.ToString(TagRenderMode.SelfClosing));
        }

        public static IHtmlString ImageBuilder(this HtmlHelper htmlHelper, UrlHelper url, string image, [System.Runtime.InteropServices.Optional, System.Runtime.InteropServices.DefaultParameterValue(null)] object htmlAttributes, ContentSource source)
        {
            TagBuilder builder = new TagBuilder("img");
            switch (source)
            {
                case ContentSource.Site:
                    builder.Attributes.Add("src", url.Content("~/Content/images/") + image);
                    break;
                case ContentSource.CDN:
                    if (GlobalConfig.IsCDNEnabled)
                        builder.Attributes.Add("src", url.Content(String.Format("{0}/Content/images/{1}", GlobalConfig.CDNBaseUrl, image)));
                    else
                        builder.Attributes.Add("src", url.Content("~/cdn/Content/images/") + image);
                    break;
                case ContentSource.Assets:
                    builder.Attributes.Add("src", url.Content(String.Format("{0}/content/images/{1}", GlobalConfig.AssetsBaseUrl, image)));
                    break;

            }
            builder.Attributes.Add("border", "0");
            builder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            return htmlHelper.Raw(builder.ToString(TagRenderMode.SelfClosing));
        }

        public static IHtmlString ContentBuilder(this HtmlHelper htmlHelper, UrlHelper url, string value, bool removeProtocol = true)
        {
            StringBuilder builder = new StringBuilder();
            if (GlobalConfig.IsAssetsEnabled)
            {
                string protocol = "^http[s]*:";
                builder.Append(url.Content(String.Format("{0}/{1}", removeProtocol ? Regex.Replace(GlobalConfig.AssetsBaseUrl, protocol, String.Empty) : GlobalConfig.AssetsBaseUrl, value)));
            }
            else
                builder.Append(url.Content(String.Format("~/{0}", value)));
            return htmlHelper.Raw(builder.ToString());
        }

        public static IHtmlString JavascriptEncode(this HtmlHelper htmlHelper, string item)
        {
            return new HtmlString(HttpUtility.JavaScriptStringEncode(item));
        }
    }
}