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
    public class Url
    {
        public string type { get; set; }

        public string template { get; set; }
    }

    public class NextPage
    {
        public string title { get; set; }

        public string totalResults { get; set; }

        public string searchTerms { get; set; }

        public int count { get; set; }

        public int startIndex { get; set; }

        public string inputEncoding { get; set; }

        public string outputEncoding { get; set; }

        public string safe { get; set; }

        public string cx { get; set; }

        public string filter { get; set; }
    }

    public class PreviousPage
    {
        public string title { get; set; }

        public string totalResults { get; set; }

        public string searchTerms { get; set; }

        public int count { get; set; }

        public int startIndex { get; set; }

        public string inputEncoding { get; set; }

        public string outputEncoding { get; set; }

        public string safe { get; set; }

        public string cx { get; set; }

        public string filter { get; set; }
    }

    public class Request
    {
        public string title { get; set; }

        public string totalResults { get; set; }

        public string searchTerms { get; set; }

        public int count { get; set; }

        public int startIndex { get; set; }

        public string inputEncoding { get; set; }

        public string outputEncoding { get; set; }

        public string safe { get; set; }

        public string cx { get; set; }
    }

    public class Queries
    {
        public List<NextPage> nextPage { get; set; }

        public List<Request> request { get; set; }

        public List<PreviousPage> previousPage { get; set; }
    }

    public class Context
    {
        public string title { get; set; }
    }

    public class SearchInformation
    {
        public double searchTime { get; set; }

        public string formattedSearchTime { get; set; }

        public string totalResults { get; set; }

        public string formattedTotalResults { get; set; }
    }

    public class CseImage
    {
        public string src { get; set; }
    }

    //public class Show
    //{
    //    public string url { get; set; }

    //    public string title { get; set; }

    //    public string description { get; set; }

    //    public string type { get; set; }

    //    public string image { get; set; }

    //    public string site_name { get; set; }
    //}

    //public class Metatag
    //{
    //    public string ogurl { get; set; }

    //    public string ogtitle { get; set; }

    //    public string ogdescription { get; set; }

    //    public string ogtype { get; set; }

    //    public string ogimage { get; set; }

    //    public string ogsite_name { get; set; }
    //}

    //public class Pagemap
    //{
    //    public List<CseImage> cse_image { get; set; }

    //    public List<Show> Show { get; set; }

    //    public List<Metatag> metatags { get; set; }
    //}

    public class Item
    {
        public string kind { get; set; }

        public string title { get; set; }

        public string htmlTitle { get; set; }

        public string link { get; set; }

        public string displayLink { get; set; }

        public string snippet { get; set; }

        public string htmlSnippet { get; set; }

        public string cacheId { get; set; }

        public string formattedUrl { get; set; }

        public string htmlFormattedUrl { get; set; }

        // public Pagemap pagemap { get; set; }
    }

    public class CustomSearchApiResponse
    {
        public string kind { get; set; }

        public Url url { get; set; }

        public Queries queries { get; set; }

        public Context context { get; set; }

        public SearchInformation searchInformation { get; set; }

        public List<Item> items { get; set; }
    }

    public class GoogleCustomSiteSearchClient
    {
        private string CustomSearchJsonApiUrl;
        private string CustomSearchApiKey;
        private string CustomSearchCX;
        private string CustomSearchFilter;
        private string CustomSearchQueryPiece;
        private string CustomSearchCount;
        private string CustomSearchStartIndex;
        private string CustomSearchFormat;

        public GoogleCustomSiteSearchClient()
            : this(

                Settings.GetSettings("CustomSearchJsonApiUrl"),
                Settings.GetSettings("CustomSearchApiKey"),
                Settings.GetSettings("CustomSearchCX"),
                Settings.GetSettings("CustomSearchFilter"),
                Settings.GetSettings("CustomSearchQueryPiece"),
                Settings.GetSettings("CustomSearchCount"),
                Settings.GetSettings("CustomSearchStartIndex"),
                Settings.GetSettings("CustomSearchFormat"))
        { }

        public GoogleCustomSiteSearchClient(string CustomSearchJsonApiUrl, string CustomSearchApiKey, string CustomSearchCX, string CustomSearchFilter, string CustomSearchQueryPiece, string CustomSearchCount, string CustomSearchStartIndex, string CustomSearchFormat)
        {
            this.CustomSearchJsonApiUrl = CustomSearchJsonApiUrl; //
            this.CustomSearchApiKey = CustomSearchApiKey; //
            this.CustomSearchCX = CustomSearchCX; //
            this.CustomSearchFilter = CustomSearchFilter; //
            this.CustomSearchQueryPiece = CustomSearchQueryPiece;
            this.CustomSearchCount = CustomSearchCount; //
            this.CustomSearchStartIndex = CustomSearchStartIndex; //
            this.CustomSearchFormat = CustomSearchFormat; //
        }

        public CustomSearchApiResponse RunSearch(string query, string startIndex)
        {
            startIndex = String.IsNullOrEmpty(startIndex) ? this.CustomSearchStartIndex : startIndex;
            var url = string.Format("{0}?key={1}&alt={2}&cx={3}&filter={4}&num={5}&start={6}&q={7} {8}", this.CustomSearchJsonApiUrl, this.CustomSearchApiKey, this.CustomSearchFormat, this.CustomSearchCX, this.CustomSearchFilter, this.CustomSearchCount, startIndex, this.CustomSearchQueryPiece, query);

            var result = string.Empty;
            var webRequest = WebRequest.Create(url);
            webRequest.Timeout = 4000;
            try
            {
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
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        return new CustomSearchApiResponse();
                    }
                }
                if (string.IsNullOrEmpty(result))
                    return null;
                var javaScriptSerializer = new JavaScriptSerializer();
                var apiResponse = javaScriptSerializer.Deserialize<CustomSearchApiResponse>(result);
                return apiResponse;
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    WebException webEx = (WebException)e;
                    if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Forbidden)
                        return new CustomSearchApiResponse();
                }
                return null;
            }
        }
    }
}