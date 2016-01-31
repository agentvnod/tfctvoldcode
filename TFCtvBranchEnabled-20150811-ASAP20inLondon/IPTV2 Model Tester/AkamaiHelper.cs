using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.Akamai.EdgeAuth;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    public class Akamai
    {

        public static string GetVideoUrl(int episodeId, int assetId, string userId)
        {
            string videoUrl = null;

            var clipDetails = GetAkamaiClipDetails(episodeId, assetId, userId);

            if (clipDetails != null)
            {
                videoUrl = clipDetails.Url;
            }

            return (videoUrl);
        }


        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetails(int episodeId, int assetId, string userId)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = 2; // Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = "US"; // Helpers.MyUtility.getCountry(req.UserHostAddress).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = userId };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package

                var packageId = 58; // GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode,RightsType.Online);
                }

                // check user's access rights
                if (!canPlay && userId != null)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(userId));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = 50; //  Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode,RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            canPlay = user.CanPlayVideo(offering, episode, asset,RightsType.Online);
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = "hls://o1-i.akamaihd.net/i/";
                    var hlsSuffixPattern = "/master.m3u8";
                    var zeriPrefixPattern = "http://o1-f.akamaihd.net/z/";
                    var zeriSuffixPattern = "/manifest.f4m";

                    videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    string ipAddress = string.Empty;
                    //if (!req.IsLocal)
                    //{
                    //    ipAddress = req.UserHostAddress;
                    //}

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            snippetStart = asset.SnippetStart.Value.Seconds;
                            snippetEnd = asset.SnippetEnd.Value.Seconds;
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = 30;
                        }
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = "1cb57a04160477a119e57791f27a0706";// Helpers.GlobalConfig.AkamaiTokenKey;
                    tokenConfig.Acl = "/*";
                    tokenConfig.IP = ipAddress;
                    tokenConfig.PreEscapeAcl = false;
                    tokenConfig.IsUrl = false;
                    tokenConfig.SessionID = string.Empty;
                    tokenConfig.Payload = asset.AssetId.ToString() + ((clipDetails.SubClip == null) ? string.Empty : ":" + snippetStart.ToString() + ":" + snippetEnd.ToString());
                    tokenConfig.Salt = string.Empty;
                    tokenConfig.FieldDelimiter = '~';

                    var token = AkamaiTokenGenerator.GenerateToken(tokenConfig);

                    videoUrl += "?hdnea=" + token;
                    clipDetails.Url = videoUrl;
                    clipDetails.PromptToSubscribe = (clipDetails != null);

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