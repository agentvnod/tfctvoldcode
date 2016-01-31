using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.Akamai.EdgeAuth;
using IPTV2_Model;

namespace TFCtv_Admin_Website.Helpers
{
    public class Akamai
    {
        const string iPadUA = "";

        public static string GetVideoUrl(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser)
        {
            string videoUrl = null;

            var clipDetails = GetAkamaiClipDetails(episodeId, assetId, req, thisUser);

            if (clipDetails != null)
            {
                videoUrl = clipDetails.Url;
            }

            return (videoUrl);
        }

        public static bool IsIos(HttpRequestBase req)
        {
            return IsIos(req.UserAgent);
        }

        public static bool IsIos(string userAgent)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(userAgent, "iPad|iPhone|iPod");
        }

        public static bool IsIpad(HttpRequestBase req)
        {
            return IsIpad(req.UserAgent);
        }

        public static bool IsIpad(string userAgent)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(userAgent, "iPad");
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetails(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, StreamType streamType = StreamType.ADAPTIVE)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = new AkamaiFlowPlayerPluginClipDetails();

            var offeringId = Global.OfferingId;
            var videoUrl = string.Empty;
            var countryCode = Global.DefaultCountry;

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id

            if ((episode != null) & (asset != null))
            {
                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = streamType == StreamType.ADAPTIVE ? Global.hlsPrefixPattern : Global.hlsProgressivePrefixPattern;
                    var hlsSuffixPattern = streamType == StreamType.ADAPTIVE ? Global.hlsSuffixPattern : Global.hlsProgressiveSuffixPattern;
                    var zeriPrefixPattern = streamType == StreamType.ADAPTIVE ? Global.zeriPrefixPattern : Global.zeriProgressivePrefixPattern;
                    var zeriSuffixPattern = streamType == StreamType.ADAPTIVE ? Global.zeriSuffixPattern : Global.zeriProgressiveSuffixPattern;
                    var httpPrefixPatternMobile = streamType == StreamType.ADAPTIVE ? Global.httpPrefixPatternMobile : Global.httpProgressivePrefixPatternMobile;

                    if (req.Browser.IsMobileDevice)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    }
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    }
                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.UserHostAddress;

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    clipDetails.IsFree = true;
                    clipDetails.EpisodeId = episode.EpisodeId;
                    clipDetails.AssetId = asset.AssetId;
                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(15));
                    if (streamType == StreamType.ADAPTIVE)
                    {
                        clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;
                        double unixTime = ts.TotalSeconds;

                        var tokenConfig = new AkamaiTokenConfig();
                        tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                        tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                        tokenConfig.Window = 300;
                        tokenConfig.Key = Global.AkamaiTokenKey;
                        tokenConfig.Acl = "/*";
                        tokenConfig.IP = ipAddress;
                        tokenConfig.PreEscapeAcl = false;
                        tokenConfig.IsUrl = false;
                        tokenConfig.SessionID = string.Empty;
                        tokenConfig.Payload = asset.AssetId.ToString() + ((clipDetails.SubClip == null) ? string.Empty : ":" + snippetStart.ToString() + ":" + snippetEnd.ToString());
                        tokenConfig.Salt = string.Empty;
                        tokenConfig.FieldDelimiter = '~';

                        var token = AkamaiTokenGenerator.GenerateToken(tokenConfig);
                        if (!String.IsNullOrEmpty(videoUrl))
                            videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "hdnea=" + token;
                    }
                    else
                    {
                        switch (streamType)
                        {
                            case StreamType.PLAY_HIGH:
                                videoUrl = MyUtility.ReplaceWithPMDBitRate(videoUrl, Global.PMDHighBitrate);
                                break;
                            case StreamType.PLAY_LOW:
                                videoUrl = MyUtility.ReplaceWithPMDBitRate(videoUrl, Global.PMDLowBitrate);
                                break;
                            case StreamType.PLAY_IN_HD:
                                videoUrl = MyUtility.ReplaceWithPMDBitRate(videoUrl, Global.PMDHDBitrate);
                                break;
                        }
                        if (!req.Browser.IsMobileDevice)
                            videoUrl = new AkamaiPMDTokenGenerator(videoUrl, (long)Global.AkamaiAddSeconds, Global.PMDSalt, "", (long)ts.TotalSeconds, "").AuthUrl;
                        else
                            videoUrl = String.Empty;

                    }
                    clipDetails.Url = videoUrl;
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetails_LiveEvent(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, StreamType streamType = StreamType.ADAPTIVE)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = new AkamaiFlowPlayerPluginClipDetails();

            var offeringId = Global.OfferingId;
            var videoUrl = string.Empty;
            var countryCode = Global.DefaultCountry;

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 5; // akamai's cdn id

            if ((episode != null) & (asset != null))
            {
                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = streamType == StreamType.ADAPTIVE ? Global.hlsPrefixPattern : Global.hlsProgressivePrefixPattern;
                    var hlsSuffixPattern = streamType == StreamType.ADAPTIVE ? Global.hlsSuffixPattern : Global.hlsProgressiveSuffixPattern;
                    var httpPrefixPatternMobile = streamType == StreamType.ADAPTIVE ? Global.httpPrefixPatternMobile : Global.httpProgressivePrefixPatternMobile;
                    videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.UserHostAddress;

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    clipDetails.IsFree = true;
                    clipDetails.EpisodeId = episode.EpisodeId;
                    clipDetails.AssetId = asset.AssetId;
                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(15));
                    if (streamType == StreamType.ADAPTIVE)
                    {
                        clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;
                        double unixTime = ts.TotalSeconds;

                        var tokenConfig = new AkamaiTokenConfig();
                        tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                        tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                        tokenConfig.Window = 300;
                        tokenConfig.Key = Global.AkamaiIosTokenKey;
                        tokenConfig.Acl = "/*";
                        tokenConfig.IP = ipAddress;
                        tokenConfig.PreEscapeAcl = false;
                        tokenConfig.IsUrl = false;
                        tokenConfig.SessionID = string.Empty;
                        tokenConfig.Payload = asset.AssetId.ToString() + ((clipDetails.SubClip == null) ? string.Empty : ":" + snippetStart.ToString() + ":" + snippetEnd.ToString());
                        tokenConfig.Salt = string.Empty;
                        tokenConfig.FieldDelimiter = '~';

                        var token = AkamaiTokenGenerator.GenerateToken(tokenConfig);
                        if (!String.IsNullOrEmpty(videoUrl))
                            videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "hdnea=" + token;
                    }
                    else
                    {
                        switch (streamType)
                        {
                            case StreamType.PLAY_HIGH:
                                videoUrl = MyUtility.ReplaceWithPMDBitRate(videoUrl, Global.PMDHighBitrate);
                                break;
                            case StreamType.PLAY_LOW:
                                videoUrl = MyUtility.ReplaceWithPMDBitRate(videoUrl, Global.PMDLowBitrate);
                                break;
                            case StreamType.PLAY_IN_HD:
                                videoUrl = MyUtility.ReplaceWithPMDBitRate(videoUrl, Global.PMDHDBitrate);
                                break;
                        }
                        if (!req.Browser.IsMobileDevice)
                            videoUrl = new AkamaiPMDTokenGenerator(videoUrl, (long)Global.AkamaiAddSeconds, Global.PMDSalt, "", (long)ts.TotalSeconds, "").AuthUrl;
                        else
                            videoUrl = String.Empty;

                    }
                    clipDetails.Url = videoUrl;
                }
            }
            return (clipDetails);
        }

    }

    public class AkamaiFlowPlayerPluginClipDetails
    {
        public string Url { get; set; }

        public int EpisodeId { get; set; }

        public int AssetId { get; set; }

        public SubClip SubClip { get; set; }

        public bool PromptToSubscribe { get; set; }

        public bool IsFree { get; set; }

        public string UserId { get; set; }
    }

    public class SubClip
    {
        public int Start { get; set; }

        public int End { get; set; }
    }
}