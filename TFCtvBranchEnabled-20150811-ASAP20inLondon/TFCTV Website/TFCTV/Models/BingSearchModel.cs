using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using TFCTV.Helpers;

namespace TFCTV.Models
{
    public class BingApiSearchResponse
    {
        public SearchResponse SearchResponse { get; set; }
    }

    public class SearchResponse
    {
        public float Version { get; set; }

        public Query Query { get; set; }

        public WebResponseType Web { get; set; }
    }

    public class Query
    {
        public string SearchTerms { get; set; }
    }

    public class WebResponseType
    {
        public int Total { get; set; }

        public int Offset { get; set; }

        public List<WebResult> Results { get; set; }

        public WebResponseType()
        {
            this.Results = new List<WebResult>();
        }
    }

    public class WebResult
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public string CacheUrl { get; set; }

        public string DisplayUrl { get; set; }

        public DateTime DateTime { get; set; }
    }

    public class BingSiteSearchClient
    {
        private string bingJsonApiUrl;
        private string bingApiKey;
        private string bingSiteQueryPiece;
        private string bingSiteSearchCount;
        private string bingSiteSearchOffset;

        public BingSiteSearchClient()
            : this(
                Settings.GetSettings("BingJsonApiUrl"),
                Settings.GetSettings("BingApiKey"),
                Settings.GetSettings("BingSiteQueryPiece"),
                Settings.GetSettings("BingSiteSearchCount"),
                "0")

        { }

        public BingSiteSearchClient(string bingJsonApiUrl, string bingApiKey, string bingSiteQueryPiece, string bingSiteSearchCount, string bingSiteSearchOffset)
        {
            this.bingJsonApiUrl = bingJsonApiUrl;
            this.bingApiKey = bingApiKey;
            this.bingSiteQueryPiece = bingSiteQueryPiece;
            this.bingSiteSearchCount = bingSiteSearchCount;
            this.bingSiteSearchOffset = String.IsNullOrEmpty(bingSiteSearchOffset) ? "0" : bingSiteSearchOffset;
        }

        public SearchResponse RunSearch(string query, string page, string count)
        {
            int Pagei = String.IsNullOrEmpty(page) ? 0 : Convert.ToInt32(page) - 1;
            int offset = !String.IsNullOrEmpty(page) ? (Pagei <= 0 ? Pagei : Pagei * Convert.ToInt32(this.bingSiteSearchCount) + 1) : Convert.ToInt32(this.bingSiteSearchOffset);
            offset = !String.IsNullOrEmpty(page) ? (Pagei > 0 ? Pagei * Convert.ToInt32(this.bingSiteSearchCount) : 0) : 0;
            offset = offset < 0 ? 0 : offset;
            var url = string.Format("{0}?Appid={1}&sources=web&web.offset={4}&web.count={5}&Options=EnableHighlighting&query={2} {3}",
                this.bingJsonApiUrl, this.bingApiKey, this.bingSiteQueryPiece, query, offset, String.IsNullOrEmpty(count) ? this.bingSiteSearchCount : count);
            var result = string.Empty;
            var webRequest = WebRequest.Create(url);
            webRequest.Timeout = 2000;
            using (var response = webRequest.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var receiveStream = response.GetResponseStream();
                    if (receiveStream != null)
                    {
                        var stream = new StreamReader(receiveStream);
                        result = stream.ReadToEnd();
                    }
                }
            }
            if (string.IsNullOrEmpty(result))
                return null;
            var javaScriptSerializer = new JavaScriptSerializer();
            var apiResponse = javaScriptSerializer.Deserialize<BingApiSearchResponse>(result);
            return apiResponse.SearchResponse;
        }
    }
}