using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TFCTV.Helpers
{
    public static class MyHtmlHelper
    {
        public static IHtmlString JsBuilder(this HtmlHelper htmlHelper, UrlHelper url, string value)
        {
            TagBuilder builder = new TagBuilder("script");
            builder.Attributes.Add("src", url.Content("~/Scripts/" + value));
            builder.Attributes.Add("type", "text/javascript");
            builder.Attributes.Add("language", "JavaScript");
            return htmlHelper.Raw(builder.ToString());
        }

        public static IHtmlString CssBuilder(this HtmlHelper htmlHelper, UrlHelper url, string value)
        {
            TagBuilder builder = new TagBuilder("link");
            builder.Attributes.Add("href", url.Content("~/Content/" + value));
            builder.Attributes.Add("rel", "stylesheet");
            builder.Attributes.Add("type", "text/css");
            return htmlHelper.Raw(builder.ToString(TagRenderMode.SelfClosing));
        }

        public static IHtmlString ImageBuilder(this HtmlHelper htmlHelper, UrlHelper url, string image, [System.Runtime.InteropServices.Optional, System.Runtime.InteropServices.DefaultParameterValue(null)] object htmlAttributes)
        {
            TagBuilder builder = new TagBuilder("img");
            builder.Attributes.Add("src", @url.Content("~/Content/images/") + image);
            builder.Attributes.Add("border", "0");
            builder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            return htmlHelper.Raw(builder.ToString(TagRenderMode.SelfClosing));
        }
    }
}