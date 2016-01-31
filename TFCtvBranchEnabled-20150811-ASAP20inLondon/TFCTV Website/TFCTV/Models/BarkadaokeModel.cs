using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using TFCTV.Helpers;

namespace TFCTV.Models.Youtube
{

    public class Thumbnail
    {
        public string sqDefault { get; set; }
        public string hqDefault { get; set; }
    }

    public class Player
    {
        public string @default { get; set; }
        public string mobile { get; set; }
    }

    //public class Content
    //{
    //    public string __invalid_name__1 { get; set; }
    //    public string __invalid_name__5 { get; set; }
    //    public string __invalid_name__6 { get; set; }
    //}

    public class AccessControl
    {
        public string comment { get; set; }
        public string commentVote { get; set; }
        public string videoRespond { get; set; }
        public string rate { get; set; }
        public string embed { get; set; }
        public string list { get; set; }
        public string autoPlay { get; set; }
        public string syndicate { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public string uploaded { get; set; }
        public string updated { get; set; }
        public string uploader { get; set; }
        public string category { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Thumbnail thumbnail { get; set; }
        public Player player { get; set; }
        //public Content content { get; set; }
        public int duration { get; set; }
        public double rating { get; set; }
        public string likeCount { get; set; }
        public int ratingCount { get; set; }
        public int viewCount { get; set; }
        public int favoriteCount { get; set; }
        public int commentCount { get; set; }
        public AccessControl accessControl { get; set; }
    }

    public class Data
    {
        public string updated { get; set; }
        public int totalItems { get; set; }
        public int startIndex { get; set; }
        public int itemsPerPage { get; set; }
        public List<Item> items { get; set; }
    }

    public class YoutubeAPIVideoFeedResponse
    {
        public string apiVersion { get; set; }
        public Data data { get; set; }
        public Error error { get; set; }
    }

    public class Error2
    {
        public string domain { get; set; }
        public string code { get; set; }
        public string internalReason { get; set; }
    }

    public class Error
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Error2> errors { get; set; }
    }

    public class SingleVideoData
    {
        public string id { get; set; }
        public string uploaded { get; set; }
        public string updated { get; set; }
        public string uploader { get; set; }
        public string category { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Thumbnail thumbnail { get; set; }
        public Player player { get; set; }
        public int duration { get; set; }
        public double rating { get; set; }
        public string likeCount { get; set; }
        public int ratingCount { get; set; }
        public int viewCount { get; set; }
        public int favoriteCount { get; set; }
        public int commentCount { get; set; }
        public AccessControl accessControl { get; set; }
    }

    public class YoutubeAPISingleVideoResponse
    {
        public string apiVersion { get; set; }
        public SingleVideoData data { get; set; }
        public Error error { get; set; }
    }

    public class DBMResult
    {
        public string id { get; set; }
        public string uid { get; set; }
        public string vid { get; set; }
    }

    public class DBMGetUserResponse
    {
        public DBMResult result { get; set; }
    }
   
    public class Video
    {
        public string id { get; set; }
        public string uploaded { get; set; }
        public string updated { get; set; }
        public string uploader { get; set; }
        public string category { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Thumbnail thumbnail { get; set; }
        public Player player { get; set; }
        public int duration { get; set; }
        public int viewCount { get; set; }
        public int favoriteCount { get; set; }
        public int commentCount { get; set; }
        public AccessControl accessControl { get; set; }
    }

    public class ItemPlaylist
    {
        public string id { get; set; }
        public int position { get; set; }
        public string author { get; set; }
        public Video video { get; set; }
    }

    public class DataPlaylist
    {
        public string id { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Thumbnail thumbnail { get; set; }        
        public int totalItems { get; set; }
        public int startIndex { get; set; }
        public int itemsPerPage { get; set; }
        public List<ItemPlaylist> items { get; set; }
    }

    public class YoutubeAPIPlaylistResponse
    {
        public string apiVersion { get; set; }
        public DataPlaylist data { get; set; }
    }

    public class YoutubeAPIClient
    {
        private string YoutubeAPIVideoFeedUrl;
        private string YoutubeAPIVideoFeedUser;
        private string YoutubeAPIVideoFeedStartIndex;
        public string YoutubeAPIVideoFeedMaxResults;
        private string YoutubeAPISingleVideoUrl;
        private string YoutubeAPIPlaylistFeedUrl;
        private string YoutubeAPIPlaylistID;

        public YoutubeAPIClient()
            : this(

                Settings.GetSettings("YoutubeAPIVideoFeedUrl"),
                Settings.GetSettings("YoutubeAPIVideoFeedUser"),
                Settings.GetSettings("YoutubeAPIVideoFeedStartIndex"),
                Settings.GetSettings("YoutubeAPIVideoFeedMaxResults"),
                Settings.GetSettings("YoutubeAPISingleVideoUrl"),
                Settings.GetSettings("YoutubeAPIPlaylistFeedUrl"),
                Settings.GetSettings("YoutubeAPIPlaylistID"))
        { }

        public YoutubeAPIClient(string YoutubeAPIVideoFeedUrl, string YoutubeAPIVideoFeedUser, string YoutubeAPIVideoFeedStartIndex, string YoutubeAPIVideoFeedMaxResults, string YoutubeAPISingleVideoUrl, string YoutubeAPIPlaylistFeedUrl, string YoutubeAPIPlaylistID)
        {
            this.YoutubeAPIVideoFeedUrl = YoutubeAPIVideoFeedUrl; //
            this.YoutubeAPIVideoFeedUser = YoutubeAPIVideoFeedUser; //                    
            this.YoutubeAPIVideoFeedStartIndex = YoutubeAPIVideoFeedStartIndex; //
            this.YoutubeAPIVideoFeedMaxResults = YoutubeAPIVideoFeedMaxResults;
            this.YoutubeAPISingleVideoUrl = YoutubeAPISingleVideoUrl;
            this.YoutubeAPIPlaylistFeedUrl = YoutubeAPIPlaylistFeedUrl;
            this.YoutubeAPIPlaylistID = YoutubeAPIPlaylistID;
        }

        public YoutubeAPISingleVideoResponse GetVideoDetails(string videoId)
        {
            var mainUrl = String.Format(YoutubeAPISingleVideoUrl, videoId);
            var url = String.Format("{0}?v=2&alt=jsonc", mainUrl);
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
                        return new YoutubeAPISingleVideoResponse();
                    }
                }
                if (string.IsNullOrEmpty(result))
                    return null;
                var javaScriptSerializer = new JavaScriptSerializer();
                var apiResponse = javaScriptSerializer.Deserialize<YoutubeAPISingleVideoResponse>(result);
                return apiResponse;
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    WebException webEx = (WebException)e;
                    if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Forbidden)
                        return new YoutubeAPISingleVideoResponse();
                }
                return null;
            }

        }

        public YoutubeAPIPlaylistResponse GetVideos(string page)
        {
            page = String.IsNullOrEmpty(page) ? this.YoutubeAPIVideoFeedStartIndex : page;
            var startIndex = (Convert.ToInt32(page) - 1) * Convert.ToInt32(YoutubeAPIVideoFeedMaxResults) + 1;

            var mainUrl = String.Format(YoutubeAPIPlaylistFeedUrl, YoutubeAPIPlaylistID);
            var url = String.Format("{0}?v=2&alt=jsonc&max-results={1}&start-index={2}", mainUrl, YoutubeAPIVideoFeedMaxResults, startIndex);

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
                        return new YoutubeAPIPlaylistResponse();
                    }
                }
                if (string.IsNullOrEmpty(result))
                    return null;
                var javaScriptSerializer = new JavaScriptSerializer();
                var apiResponse = javaScriptSerializer.Deserialize<YoutubeAPIPlaylistResponse>(result);
                return apiResponse;
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    WebException webEx = (WebException)e;
                    if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.Forbidden)
                        return new YoutubeAPIPlaylistResponse();
                }
                return null;
            }
        }
    }
}