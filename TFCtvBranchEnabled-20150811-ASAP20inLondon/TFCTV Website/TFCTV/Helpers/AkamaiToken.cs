using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.Akamai.EdgeAuth;
using IPTV2_Model;
using System.Text.RegularExpressions;

namespace TFCTV.Helpers
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

        public static bool IsIos(HttpRequestBase req, bool useIsMobileDevice = false)
        {
            if (useIsMobileDevice)
                return req.Browser.IsMobileDevice;
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

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetails(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, VideoQualityCdnReference? quality = null)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();
            var uCountryCode = countryCode;

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id
            if (quality != null)
                cdnId = (int)quality;

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        uCountryCode = user.CountryCode;
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);

                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpPrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    if (isIos)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                        //string limitUrl = "b=-" + (isIpad ? "5000,1100" : "5000,700") + "-"; // iPad up to 1Mbps, iPhone/iPod up to 500Kbps
                        //videoUrl = (videoUrl.EndsWith(".m3u8") ? "?" : "&") + limitUrl;
                        if (isIpad)
                            videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                        else
                            videoUrl = videoUrl.Replace(",800000,1000000,1300000,1500000,", ","); // max 500kbps
                    }
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    }

                    //CHECK IF COUNTRY IS AU or NZ                                        
                    if (String.Compare(uCountryCode, "AU", true) == 0 || String.Compare(uCountryCode, "NZ", true) == 0)
                        videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                    //END OF CHECK IF COUNTRY IS AU or NZ

                    // videoUrl = !isMobileUse ? assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern) : assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            //DateTime baseTime = new DateTime(1900, 1, 1, 0, 0, 0);
                            //DateTime startTime = new DateTime(1900, 1, 1, asset.SnippetStart.Value.Hours, asset.SnippetStart.Value.Minutes, asset.SnippetStart.Value.Seconds);
                            //DateTime endTime = new DateTime(1900, 1, 1, asset.SnippetEnd.Value.Hours, asset.SnippetEnd.Value.Minutes, asset.SnippetEnd.Value.Seconds);
                            //snippetStart = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, startTime));
                            //snippetEnd = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, endTime));
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;

                        // don't pass a URL if iOS, no preview yet
                        // UNCOMMENT
                        //if (isIos)
                        //    videoUrl = string.Empty;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
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

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                    // clipDetails.PromptToSubscribe = (clipDetails != null);
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetailsByUserId(int episodeId, int assetId, HttpRequestBase req, Guid userId)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = userId.ToString() };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpPrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    if (isIos)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                        //string limitUrl = "b=-" + (isIpad ? "5000,1100" : "5000,700") + "-"; // iPad up to 1Mbps, iPhone/iPod up to 500Kbps
                        //videoUrl = (videoUrl.EndsWith(".m3u8") ? "?" : "&") + limitUrl;
                        if (isIpad)
                            videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                        else
                            videoUrl = videoUrl.Replace(",800000,1000000,1300000,1500000,", ","); // max 500kbps
                    }
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    }

                    // videoUrl = !isMobileUse ? assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern) : assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            //DateTime baseTime = new DateTime(1900, 1, 1, 0, 0, 0);
                            //DateTime startTime = new DateTime(1900, 1, 1, asset.SnippetStart.Value.Hours, asset.SnippetStart.Value.Minutes, asset.SnippetStart.Value.Seconds);
                            //DateTime endTime = new DateTime(1900, 1, 1, asset.SnippetEnd.Value.Hours, asset.SnippetEnd.Value.Minutes, asset.SnippetEnd.Value.Seconds);
                            //snippetStart = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, startTime));
                            //snippetEnd = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, endTime));
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;

                        // don't pass a URL if iOS, no preview yet
                        // UNCOMMENT
                        //if (isIos)
                        //    videoUrl = string.Empty;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
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

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                    // clipDetails.PromptToSubscribe = (clipDetails != null);
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiProgressiveClipDetails(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, Progressive progressive, VideoQualityCdnReference? quality = null)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id
            if (quality != null)
                cdnId = (int)quality;

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);
                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        //if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                        canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsProgressivePrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsProgressiveSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriProgressivePrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriProgressiveSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpProgressivePrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    if (isIos)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, String.Empty);
                    }
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, String.Empty).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    }

                    if (progressive == Progressive.High)
                    {
                        //videoUrl = videoUrl.Replace(",150000,300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDHighBitrate); // max 800Kbps
                        //videoUrl = videoUrl.Replace(",300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDHighBitrate); // max 800Kbps
                        if (episode.EpisodeId == GlobalConfig.JDCEpisodeId)
                            videoUrl = ReplaceWithPMDBitRate(videoUrl, GlobalConfig.PMDHDBitrate);
                        else
                            videoUrl = ReplaceWithPMDBitRate(videoUrl, GlobalConfig.PMDHighBitrate);
                    }
                    else
                    {
                        //videoUrl = videoUrl.Replace(",150000,300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDLowBitrate); // max 500Kbps
                        //videoUrl = videoUrl.Replace(",300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDLowBitrate); // max 500Kbps
                        videoUrl = ReplaceWithPMDBitRate(videoUrl, GlobalConfig.PMDLowBitrate);
                    }


                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
                    tokenConfig.Acl = "/*";
                    tokenConfig.IP = ipAddress;
                    tokenConfig.PreEscapeAcl = false;
                    tokenConfig.IsUrl = false;
                    tokenConfig.SessionID = string.Empty;
                    tokenConfig.Payload = asset.AssetId.ToString() + ((clipDetails.SubClip == null) ? string.Empty : ":" + snippetStart.ToString() + ":" + snippetEnd.ToString());
                    tokenConfig.Salt = string.Empty;
                    tokenConfig.FieldDelimiter = '~';

                    var token = AkamaiTokenGenerator.GenerateToken(tokenConfig);

                    TimeSpan tsPMD = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));

                    if (!String.IsNullOrEmpty(videoUrl))
                        videoUrl = new AkamaiPMDTokenGenerator(videoUrl, (long)GlobalConfig.AkamaiPMDAddSeconds, GlobalConfig.PMDSalt, "", (long)ts.TotalSeconds, "").AuthUrl;
                    //videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "hdnea=" + token;

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    if (!canPlay && (progressive == Progressive.High || progressive == Progressive.Low)) // If user is not entitled, and cannot play, remove PMDlink
                        videoUrl = String.Empty;

                    if (!String.IsNullOrEmpty(videoUrl))
                    {
                        if (isIos)
                            videoUrl = String.Format("{0}{1}", httpPrefixPatternMobile, videoUrl);
                        else
                            videoUrl = String.Format("{0}{1}", zeriPrefixPattern, videoUrl);
                    }
                    clipDetails.Url = videoUrl;
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiProgressiveClipDetails_M3U8(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, Progressive progressive, VideoQualityCdnReference? quality = null)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();
            string uCountryCode = countryCode;

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id
            if (quality != null)
                cdnId = (int)quality;

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        uCountryCode = user.CountryCode;
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);
                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        //if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                        canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsProgressivePrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsProgressiveSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriProgressivePrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriProgressiveSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpProgressivePrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, String.Empty).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    var ProgressiveHDBitrate = GlobalConfig.PMDHDBitrate;
                    var ProgressiveHighBitrate = GlobalConfig.PMDHighBitrate;
                    var ProgressiveLowBitrate = GlobalConfig.PMDLowBitrate;
                    try
                    {
                        var mp4capableObj = MyUtility.CheckIfMp4Compatible(req, episode.EpisodeId, "4.4");
                        if (mp4capableObj.UseMp4ForPlayback)
                        {
                            int platform = req.Browser.IsMobileDevice ? (int)Platform.Mobile : (int)Platform.Desktop;
                            var bitrate = context.CountryBitrates.FirstOrDefault(c => String.Compare(uCountryCode, c.CountryCode, true) == 0 && c.Platform == platform);
                            if (bitrate != null)
                            {
                                if (bitrate.ProgressiveHDBitrate != null)
                                    ProgressiveHDBitrate = String.Format("{0}", bitrate.ProgressiveHDBitrate);
                                if (bitrate.ProgressiveHighBitrate != null)
                                    ProgressiveHighBitrate = String.Format("{0}", bitrate.ProgressiveHighBitrate);
                                if (bitrate.ProgressiveLowBitrate != null)
                                    ProgressiveLowBitrate = String.Format("{0}", bitrate.ProgressiveLowBitrate); ;
                            }
                        }
                    }
                    catch (Exception) { }

                    if (progressive == Progressive.High)
                    {
                        //videoUrl = videoUrl.Replace(",150000,300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDHighBitrate); // max 800Kbps
                        //videoUrl = videoUrl.Replace(",300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDHighBitrate); // max 800Kbps
                        if (episode.EpisodeId == GlobalConfig.JDCEpisodeId)
                            videoUrl = ReplaceWithPMDBitRate(videoUrl, ProgressiveHDBitrate);
                        else
                            videoUrl = ReplaceWithPMDBitRate(videoUrl, ProgressiveHighBitrate);
                    }
                    else
                    {
                        //videoUrl = videoUrl.Replace(",150000,300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDLowBitrate); // max 500Kbps
                        //videoUrl = videoUrl.Replace(",300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDLowBitrate); // max 500Kbps
                        videoUrl = ReplaceWithPMDBitRate(videoUrl, ProgressiveLowBitrate);
                    }


                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
                    tokenConfig.Acl = "/*";
                    tokenConfig.IP = ipAddress;
                    tokenConfig.PreEscapeAcl = false;
                    tokenConfig.IsUrl = false;
                    tokenConfig.SessionID = string.Empty;
                    tokenConfig.Payload = asset.AssetId.ToString() + ((clipDetails.SubClip == null) ? string.Empty : ":" + snippetStart.ToString() + ":" + snippetEnd.ToString());
                    tokenConfig.Salt = string.Empty;
                    tokenConfig.FieldDelimiter = '~';

                    var token = AkamaiTokenGenerator.GenerateToken(tokenConfig);

                    TimeSpan tsPMD = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));

                    if (!String.IsNullOrEmpty(videoUrl))
                        videoUrl = new AkamaiPMDTokenGenerator(videoUrl, (long)GlobalConfig.AkamaiPMDAddSeconds, GlobalConfig.PMDSalt, "", (long)ts.TotalSeconds, "").AuthUrl;
                    //videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "hdnea=" + token;

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    if (!canPlay && (progressive == Progressive.High || progressive == Progressive.Low)) // If user is not entitled, and cannot play, remove PMDlink
                        videoUrl = String.Empty;

                    if (!String.IsNullOrEmpty(videoUrl))
                    {
                        if (isIos)
                            videoUrl = String.Format("{0}{1}", httpPrefixPatternMobile, videoUrl);
                        else
                            videoUrl = String.Format("{0}{1}", zeriPrefixPattern, videoUrl);
                    }
                    clipDetails.Url = videoUrl;
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiLiveEventClipDetails(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, bool isMobileAllowed, bool removeIpFromToken = false)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id

            if (!req.Browser.IsMobileDevice)
                cdnId = 3; // akamai hdn
            if (GlobalConfig.IsIosHLSCdnEnabled)
            {
                if (!req.Browser.IsMobileDevice && !MyUtility.IsDeviceHtml5Capable(req))
                    cdnId = 3; // akamai hdn
                else // get IOS stream
                {
                    if (MyUtility.IsAndroid(req) && !MyUtility.IsDeviceHtml5Capable(req)) { cdnId = 3; }
                    else { cdnId = 5; }
                }
            }


            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsLiveStreamPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsLiveStreamSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriLiveStreamPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriLiveStreamSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpLiveStreamPrefixPatternMobile;

                    //if (req.Browser.IsMobileDevice)
                    //{
                    //    videoUrl = assetCdn.CdnReference.Replace(zeriPrefixPattern, httpPrefixPatternMobile);
                    //}
                    //else
                    //{
                    //    videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    //}

                    if (cdnId == 3) //Akamai HDN means Flash based.
                        videoUrl = assetCdn.CdnReference;
                    else
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);


                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                        clipDetails.PromptToSubscribe = true;
                    else
                        clipDetails.PromptToSubscribe = false;

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
                    if (IsIos(req, true))
                        tokenConfig.Key = Helpers.GlobalConfig.AkamaiIosTokenKey;

                    //check if episode id should use the new ios token key
                    var episodeIdList = MyUtility.StringToIntList(GlobalConfig.LiveEpisodeIdsToUseIosTokenKey2);
                    if (episodeIdList.Contains(episode.EpisodeId))
                        tokenConfig.Key = Helpers.GlobalConfig.AkamaiIosTokenKey2;
                    var episodeIdList2 = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToUseIosTokenKey3);
                    if (episodeIdList2.Contains(episode.EpisodeId))
                        tokenConfig.Key = Helpers.GlobalConfig.AkamaiIosTokenKey3;

                    tokenConfig.Acl = "/*";
                    if (!removeIpFromToken)
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

                    if (!isMobileAllowed)
                        if (req.Browser.IsMobileDevice) //Apple devices
                            if (clipDetails.SubClip != null) // isPreview
                                videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetails(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, bool isIosAllowed)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpPrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    if (isIos)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                        //string limitUrl = "b=-" + (isIpad ? "5000,1100" : "5000,700") + "-"; // iPad up to 1Mbps, iPhone/iPod up to 500Kbps
                        //videoUrl = (videoUrl.EndsWith(".m3u8") ? "?" : "&") + limitUrl;
                        if (isIpad)
                            videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                        else
                            videoUrl = videoUrl.Replace(",800000,1000000,1300000,1500000,", ","); // max 500kbps
                    }
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    }

                    // videoUrl = !isMobileUse ? assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern) : assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            //DateTime baseTime = new DateTime(1900, 1, 1, 0, 0, 0);
                            //DateTime startTime = new DateTime(1900, 1, 1, asset.SnippetStart.Value.Hours, asset.SnippetStart.Value.Minutes, asset.SnippetStart.Value.Seconds);
                            //DateTime endTime = new DateTime(1900, 1, 1, asset.SnippetEnd.Value.Hours, asset.SnippetEnd.Value.Minutes, asset.SnippetEnd.Value.Seconds);
                            //snippetStart = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, startTime));
                            //snippetEnd = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, endTime));
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;

                        // don't pass a URL if iOS, no preview yet
                        // UNCOMMENT
                        //if (isIos)
                        //    videoUrl = string.Empty;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
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

                    if (!isIosAllowed)
                        if (isIos) //Apple devices
                            if (clipDetails.SubClip != null) // isPreview
                                videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                    // clipDetails.PromptToSubscribe = (clipDetails != null);
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiProgressiveViaAdaptiveClipDetails(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, Progressive progressive, VideoQualityCdnReference? quality = null)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id
            if (quality != null)
                cdnId = (int)quality;

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);

                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpPrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    if (isIos)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                        //string limitUrl = "b=-" + (isIpad ? "5000,1100" : "5000,700") + "-"; // iPad up to 1Mbps, iPhone/iPod up to 500Kbps
                        //videoUrl = (videoUrl.EndsWith(".m3u8") ? "?" : "&") + limitUrl;
                        if (isIpad)

                            videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                        else
                            videoUrl = videoUrl.Replace(",800000,1000000,1300000,1500000,", ","); // max 500kbps
                    }
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    }

                    //CHECK IF COUNTRY IS AU or NZ
                    var ipCountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                    if (String.Compare(ipCountryCode, "AU", true) == 0 || String.Compare(ipCountryCode, "NZ", true) == 0)
                        videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                    //END OF CHECK IF COUNTRY IS AU or NZ

                    if (progressive == Progressive.High)
                    {
                        if (episode.EpisodeId == GlobalConfig.JDCEpisodeId)
                            videoUrl = ReplaceWithPMDBitRate(videoUrl, String.Format(",{0},", GlobalConfig.PMDViaAdaptiveHDBitrate));
                        else
                            videoUrl = ReplaceWithPMDBitRate(videoUrl, String.Format(",{0},", GlobalConfig.PMDViaAdaptiveHighBitrate));
                    }
                    else
                    {
                        videoUrl = ReplaceWithPMDBitRate(videoUrl, String.Format(",{0},", GlobalConfig.PMDViaAdaptiveLowBitrate));
                    }

                    // videoUrl = !isMobileUse ? assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern) : assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            //DateTime baseTime = new DateTime(1900, 1, 1, 0, 0, 0);
                            //DateTime startTime = new DateTime(1900, 1, 1, asset.SnippetStart.Value.Hours, asset.SnippetStart.Value.Minutes, asset.SnippetStart.Value.Seconds);
                            //DateTime endTime = new DateTime(1900, 1, 1, asset.SnippetEnd.Value.Hours, asset.SnippetEnd.Value.Minutes, asset.SnippetEnd.Value.Seconds);
                            //snippetStart = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, startTime));
                            //snippetEnd = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, endTime));
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;

                        // don't pass a URL if iOS, no preview yet
                        // UNCOMMENT
                        //if (isIos)
                        //    videoUrl = string.Empty;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
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

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                    // clipDetails.PromptToSubscribe = (clipDetails != null);
                }
            }
            return (clipDetails);
        }

        private static string ReplaceWithPMDBitRate(string input, string bitrate)
        {
            string result = String.Empty;
            var pattern = @",(\d+,){1,}";
            try
            {
                result = Regex.Replace(input, pattern, bitrate);
            }
            catch (Exception) { }
            return result;
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetailsHD(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();
            var uCountryCode = countryCode;
            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 6; // akamaiHD cdn id

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        uCountryCode = user.CountryCode;
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);

                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpPrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    if (isIos)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                        //string limitUrl = "b=-" + (isIpad ? "5000,1100" : "5000,700") + "-"; // iPad up to 1Mbps, iPhone/iPod up to 500Kbps
                        //videoUrl = (videoUrl.EndsWith(".m3u8") ? "?" : "&") + limitUrl;
                        if (isIpad)
                            videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                        else
                            videoUrl = videoUrl.Replace(",800000,1000000,1300000,1500000,", ","); // max 500kbps
                    }
                    //else if (MyUtility.IsAndroid(req) && MyUtility.IsDeviceHtml5Capable(req))
                    //{
                    //    videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    //    videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps                        
                    //}
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    }

                    //CHECK IF COUNTRY IS AU or NZ                                        
                    if (String.Compare(uCountryCode, "AU", true) == 0 || String.Compare(uCountryCode, "NZ", true) == 0)
                        videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                    //END OF CHECK IF COUNTRY IS AU or NZ

                    // videoUrl = !isMobileUse ? assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern) : assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            //DateTime baseTime = new DateTime(1900, 1, 1, 0, 0, 0);
                            //DateTime startTime = new DateTime(1900, 1, 1, asset.SnippetStart.Value.Hours, asset.SnippetStart.Value.Minutes, asset.SnippetStart.Value.Seconds);
                            //DateTime endTime = new DateTime(1900, 1, 1, asset.SnippetEnd.Value.Hours, asset.SnippetEnd.Value.Minutes, asset.SnippetEnd.Value.Seconds);
                            //snippetStart = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, startTime));
                            //snippetEnd = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, endTime));
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;

                        // don't pass a URL if iOS, no preview yet
                        // UNCOMMENT
                        //if (isIos)
                        //    videoUrl = string.Empty;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
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

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                    // clipDetails.PromptToSubscribe = (clipDetails != null);
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetails_M3U8(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, VideoQualityCdnReference? quality = null)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();
            string uCountryCode = countryCode;

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id
            if (quality != null)
                cdnId = (int)quality;

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        uCountryCode = user.CountryCode;
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);

                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpPrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    //if (req.Browser.IsMobileDevice)
                    //{
                    //    if ((String.Compare(uCountryCode, "HK", true) == 0) || (String.Compare(countryCode, "HK", true) == 0))
                    //        videoUrl = videoUrl.Replace(",500000,800000,1000000,1300000,1500000,", ","); // If HK, maximum of 300kbps only
                    //}

                    if (isIos)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                        if (isIpad)
                            videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                        else
                            videoUrl = videoUrl.Replace(",800000,1000000,1300000,1500000,", ","); // max 500kbps
                    }
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    }

                    if (!String.IsNullOrEmpty(videoUrl))
                        videoUrl = videoUrl.Trim();

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = req.IsLocal ? 259200 : 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
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
                    {
                        //if (req.Browser.IsMobileDevice)
                        //{
                        //    //videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "__b__=150";
                        //}
                        int platform = req.Browser.IsMobileDevice ? (int)Platform.Mobile : (int)Platform.Desktop;
                        var bitrate = context.CountryBitrates.FirstOrDefault(c => String.Compare(uCountryCode, c.CountryCode, true) == 0 && c.Platform == platform);
                        if (bitrate != null)
                            if (bitrate.LowerLimit != null && bitrate.UpperLimit != null)
                                videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + String.Format("b={0}-{1}", bitrate.LowerLimit, bitrate.UpperLimit);
                        videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "hdnea=" + token;
                    }

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiLiveEventClipDetails_M3U8(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, bool isMobileAllowed, string CountryCodeOverride = "", bool RemoveIpFromToken = false)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();
            if (!String.IsNullOrEmpty(CountryCodeOverride))
                countryCode = CountryCodeOverride;
            string uCountryCode = countryCode;
            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 3; // akamai LIVE cdnId
            cdnId = GlobalConfig.UseJWPlayer ? 7 : 3;

            //if HLS is enabled, get IOS HLS cdnId
            if (GlobalConfig.IsIosHLSCdnEnabled)
                if (req.Browser.IsMobileDevice)
                    cdnId = 5;

            //Check if asset is available or not. If not revert back to the original cdnId (akamai LIVE)
            try
            {
                if (context.AssetCdns.Count(a => a.AssetId == assetId && a.CdnId == cdnId) <= 0)
                    cdnId = GlobalConfig.UseJWPlayer ? 7 : 3;
            }
            catch (Exception) { }

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        uCountryCode = user.CountryCode;
                        if (!String.IsNullOrEmpty(CountryCodeOverride))
                            uCountryCode = CountryCodeOverride;
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsLiveStreamPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsLiveStreamSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriLiveStreamPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriLiveStreamSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpLiveStreamPrefixPatternMobile;

                    if (cdnId == 3) //Akamai HDN means Flash based.
                        videoUrl = assetCdn.CdnReference;
                    else
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);

                    if (!String.IsNullOrEmpty(videoUrl))
                    {
                        videoUrl = videoUrl.Trim();
                    }


                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                        clipDetails.PromptToSubscribe = true;
                    else
                        clipDetails.PromptToSubscribe = false;

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = req.IsLocal ? 259200 : 300;
                    //tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
                    //if (IsIos(req, true))
                    //    tokenConfig.Key = Helpers.GlobalConfig.AkamaiIosTokenKey;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiIosTokenKey;

                    //check if episode id should use the new ios token key
                    var episodeIdList = MyUtility.StringToIntList(GlobalConfig.LiveEpisodeIdsToUseIosTokenKey2);
                    if (episodeIdList.Contains(episode.EpisodeId))
                        tokenConfig.Key = Helpers.GlobalConfig.AkamaiIosTokenKey2;
                    var episodeIdList2 = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToUseIosTokenKey3);
                    if (episodeIdList2.Contains(episode.EpisodeId))
                        tokenConfig.Key = Helpers.GlobalConfig.AkamaiIosTokenKey3;

                    tokenConfig.Acl = "/*";
                    if (!RemoveIpFromToken)
                        tokenConfig.IP = ipAddress;
                    tokenConfig.PreEscapeAcl = false;
                    tokenConfig.IsUrl = false;
                    tokenConfig.SessionID = string.Empty;
                    tokenConfig.Payload = asset.AssetId.ToString() + ((clipDetails.SubClip == null) ? string.Empty : ":" + snippetStart.ToString() + ":" + snippetEnd.ToString());
                    tokenConfig.Salt = string.Empty;
                    tokenConfig.FieldDelimiter = '~';

                    var token = AkamaiTokenGenerator.GenerateToken(tokenConfig);

                    if (!String.IsNullOrEmpty(videoUrl))
                    {
                        //if (req.Browser.IsMobileDevice)
                        //{                           
                        //    //videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "__b__=150";
                        //}
                        int platform = req.Browser.IsMobileDevice ? (int)Platform.Mobile : (int)Platform.Desktop;
                        var bitrate = context.CountryBitrates.FirstOrDefault(c => String.Compare(uCountryCode, c.CountryCode, true) == 0 && c.Platform == platform);
                        if (bitrate != null)
                            if (bitrate.LowerLimit != null && bitrate.UpperLimit != null)
                                videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + String.Format("b={0}-{1}", bitrate.LowerLimit, bitrate.UpperLimit);
                        videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "hdnea=" + token;
                    }

                    if (!isMobileAllowed)
                        if (req.Browser.IsMobileDevice) //Apple devices
                            if (clipDetails.SubClip != null) // isPreview
                                videoUrl = String.Empty;
                    clipDetails.Url = videoUrl;
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetailsHD_M3U8(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();
            string uCountryCode = countryCode;

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 6; // akamaiHD cdn id

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        uCountryCode = user.CountryCode;
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);

                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpPrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    //if (req.Browser.IsMobileDevice)
                    //{
                    //    if ((String.Compare(uCountryCode, "HK", true) == 0) || (String.Compare(countryCode, "HK", true) == 0))
                    //        videoUrl = videoUrl.Replace(",500000,800000,1000000,1300000,1500000,", ","); // If HK, maximum of 300kbps only
                    //}

                    if (isIos)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                        if (isIpad)
                            videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                        else
                            videoUrl = videoUrl.Replace(",800000,1000000,1300000,1500000,", ","); // max 500kbps
                    }
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    }

                    if (!String.IsNullOrEmpty(videoUrl))
                        videoUrl = videoUrl.Trim();

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = req.IsLocal ? 259200 : 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
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
                    {
                        //if (req.Browser.IsMobileDevice)
                        //{                           
                        //    //videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "__b__=150";
                        //}
                        int platform = req.Browser.IsMobileDevice ? (int)Platform.Mobile : (int)Platform.Desktop;
                        var bitrate = context.CountryBitrates.FirstOrDefault(c => String.Compare(uCountryCode, c.CountryCode, true) == 0 && c.Platform == platform);
                        if (bitrate != null)
                            if (bitrate.LowerLimit != null && bitrate.UpperLimit != null)
                                videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + String.Format("b={0}-{1}", bitrate.LowerLimit, bitrate.UpperLimit);
                        videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "hdnea=" + token;
                    }

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiProgressiveClipDetailsFIX(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, Progressive progressive, VideoQualityCdnReference? quality = null)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id
            if (quality != null)
                cdnId = (int)quality;

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);
                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        //if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                        canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsProgressivePrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsProgressiveSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriProgressivePrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriProgressiveSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpProgressivePrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    //if (!req.Browser.IsMobileDevice)
                    //{
                    //    videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, String.Empty);
                    //}
                    //else
                    //{
                    //    videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, String.Empty).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    //}
                    videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, String.Empty).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    if (progressive == Progressive.High)
                    {
                        //videoUrl = videoUrl.Replace(",150000,300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDHighBitrate); // max 800Kbps
                        //videoUrl = videoUrl.Replace(",300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDHighBitrate); // max 800Kbps
                        if (episode.EpisodeId == GlobalConfig.JDCEpisodeId)
                            videoUrl = ReplaceWithPMDBitRate(videoUrl, GlobalConfig.PMDHDBitrate);
                        else
                            videoUrl = ReplaceWithPMDBitRate(videoUrl, GlobalConfig.PMDHighBitrate);
                    }
                    else
                    {
                        //videoUrl = videoUrl.Replace(",150000,300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDLowBitrate); // max 500Kbps
                        //videoUrl = videoUrl.Replace(",300000,500000,800000,1000000,1300000,1500000,", GlobalConfig.PMDLowBitrate); // max 500Kbps
                        videoUrl = ReplaceWithPMDBitRate(videoUrl, GlobalConfig.PMDLowBitrate);
                    }


                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
                    tokenConfig.Acl = "/*";
                    tokenConfig.IP = ipAddress;
                    tokenConfig.PreEscapeAcl = false;
                    tokenConfig.IsUrl = false;
                    tokenConfig.SessionID = string.Empty;
                    tokenConfig.Payload = asset.AssetId.ToString() + ((clipDetails.SubClip == null) ? string.Empty : ":" + snippetStart.ToString() + ":" + snippetEnd.ToString());
                    tokenConfig.Salt = string.Empty;
                    tokenConfig.FieldDelimiter = '~';

                    var token = AkamaiTokenGenerator.GenerateToken(tokenConfig);

                    TimeSpan tsPMD = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));

                    if (!String.IsNullOrEmpty(videoUrl))
                        videoUrl = new AkamaiPMDTokenGenerator(videoUrl, (long)GlobalConfig.AkamaiPMDAddSeconds, GlobalConfig.PMDSalt, "", (long)ts.TotalSeconds, "").AuthUrl;
                    //videoUrl += (videoUrl.IndexOf('?') > 0 ? "&" : "?") + "hdnea=" + token;

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    if (!canPlay && (progressive == Progressive.High || progressive == Progressive.Low)) // If user is not entitled, and cannot play, remove PMDlink
                        videoUrl = String.Empty;

                    if (!String.IsNullOrEmpty(videoUrl))
                    {
                        if (isIos)
                            videoUrl = String.Format("{0}{1}", httpPrefixPatternMobile, videoUrl);
                        else
                            videoUrl = String.Format("{0}{1}", zeriPrefixPattern, videoUrl);
                    }
                    clipDetails.Url = videoUrl;
                }
            }
            return (clipDetails);
        }
        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetailsFIX(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, VideoQualityCdnReference? quality = null)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id
            if (quality != null)
                cdnId = (int)quality;

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);

                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpPrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    if (MyUtility.IsDeviceHtml5Capable(req) || isIos)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                        //string limitUrl = "b=-" + (isIpad ? "5000,1100" : "5000,700") + "-"; // iPad up to 1Mbps, iPhone/iPod up to 500Kbps
                        //videoUrl = (videoUrl.EndsWith(".m3u8") ? "?" : "&") + limitUrl;
                        if (isIpad)
                            videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                        else
                            videoUrl = videoUrl.Replace(",800000,1000000,1300000,1500000,", ","); // max 500kbps
                    }
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    }

                    // videoUrl = !isMobileUse ? assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern) : assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            //DateTime baseTime = new DateTime(1900, 1, 1, 0, 0, 0);
                            //DateTime startTime = new DateTime(1900, 1, 1, asset.SnippetStart.Value.Hours, asset.SnippetStart.Value.Minutes, asset.SnippetStart.Value.Seconds);
                            //DateTime endTime = new DateTime(1900, 1, 1, asset.SnippetEnd.Value.Hours, asset.SnippetEnd.Value.Minutes, asset.SnippetEnd.Value.Seconds);
                            //snippetStart = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, startTime));
                            //snippetEnd = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, endTime));
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;

                        // don't pass a URL if iOS, no preview yet
                        // UNCOMMENT
                        //if (isIos)
                        //    videoUrl = string.Empty;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
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

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                    // clipDetails.PromptToSubscribe = (clipDetails != null);
                }
            }
            return (clipDetails);
        }
        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetailsHDFIX(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 6; // akamaiHD cdn id

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);

                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpPrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    if (MyUtility.IsDeviceHtml5Capable(req) || isIos)
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                        //string limitUrl = "b=-" + (isIpad ? "5000,1100" : "5000,700") + "-"; // iPad up to 1Mbps, iPhone/iPod up to 500Kbps
                        //videoUrl = (videoUrl.EndsWith(".m3u8") ? "?" : "&") + limitUrl;
                        if (isIpad)
                            videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                        else
                            videoUrl = videoUrl.Replace(",800000,1000000,1300000,1500000,", ","); // max 500kbps
                    }
                    else
                    {
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    }

                    // videoUrl = !isMobileUse ? assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern) : assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            //DateTime baseTime = new DateTime(1900, 1, 1, 0, 0, 0);
                            //DateTime startTime = new DateTime(1900, 1, 1, asset.SnippetStart.Value.Hours, asset.SnippetStart.Value.Minutes, asset.SnippetStart.Value.Seconds);
                            //DateTime endTime = new DateTime(1900, 1, 1, asset.SnippetEnd.Value.Hours, asset.SnippetEnd.Value.Minutes, asset.SnippetEnd.Value.Seconds);
                            //snippetStart = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, startTime));
                            //snippetEnd = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, endTime));
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;

                        // don't pass a URL if iOS, no preview yet
                        // UNCOMMENT
                        //if (isIos)
                        //    videoUrl = string.Empty;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
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

                    if (isIos) //Apple devices
                        if (clipDetails.SubClip != null) // isPreview
                            videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                    // clipDetails.PromptToSubscribe = (clipDetails != null);
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiClipDetailsIWANTV(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, VideoQualityCdnReference? quality = null, bool removeIpFromToken = false)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id
            if (quality != null)
                cdnId = (int)quality;

            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            //canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);

                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "AkamaiToken canPlay IP Whitelisting"); }
                                }
                            }
                            else
                                canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpPrefixPatternMobile;
                    bool isIpad = IsIpad(req);
                    bool isIos = IsIos(req);

                    var hlsProtocol = GlobalConfig.hlsProtocol;
                    var httpProtocol = GlobalConfig.httpProtocol;
                    var hdsFolder = GlobalConfig.hdsFolder;
                    var hlsFolder = GlobalConfig.hlsFolder;

                    //change protocol
                    videoUrl = assetCdn.CdnReference.Replace(hlsProtocol, httpProtocol);

                    if (isIos || MyUtility.IsDeviceHtml5Capable(req))
                    {
                        //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                        //string limitUrl = "b=-" + (isIpad ? "5000,1100" : "5000,700") + "-"; // iPad up to 1Mbps, iPhone/iPod up to 500Kbps
                        //videoUrl = (videoUrl.EndsWith(".m3u8") ? "?" : "&") + limitUrl;
                        if (isIpad || (MyUtility.IsAndroid(req) && MyUtility.IsDeviceHtml5Capable(req)))
                            videoUrl = videoUrl.Replace(",1300000,1500000,", ","); // max 1Mbps
                        else
                            videoUrl = videoUrl.Replace(",800000,1000000,1300000,1500000,", ","); // max 500kbps
                    }
                    else
                    {
                        //change /i/ to /z/
                        videoUrl = assetCdn.CdnReference.Replace(hlsFolder, hdsFolder).Replace(hlsSuffixPattern, zeriSuffixPattern);
                        //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    }

                    // videoUrl = !isMobileUse ? assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern) : assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);
                    //videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);

                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                    {
                        if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
                        {
                            //DateTime baseTime = new DateTime(1900, 1, 1, 0, 0, 0);
                            //DateTime startTime = new DateTime(1900, 1, 1, asset.SnippetStart.Value.Hours, asset.SnippetStart.Value.Minutes, asset.SnippetStart.Value.Seconds);
                            //DateTime endTime = new DateTime(1900, 1, 1, asset.SnippetEnd.Value.Hours, asset.SnippetEnd.Value.Minutes, asset.SnippetEnd.Value.Seconds);
                            //snippetStart = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, startTime));
                            //snippetEnd = Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, baseTime, endTime));
                            snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
                            snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
                        }
                        else
                        {
                            snippetStart = 0;
                            snippetEnd = GlobalConfig.snippetEnd;
                        }
                        clipDetails.PromptToSubscribe = true;

                        // don't pass a URL if iOS, no preview yet
                        // UNCOMMENT
                        //if (isIos)
                        //    videoUrl = string.Empty;
                    }
                    else
                    {
                        clipDetails.PromptToSubscribe = false;
                    }

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
                    tokenConfig.Acl = "/*";
                    if (!removeIpFromToken)
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

                    //if (isIos) //Apple devices
                    //    if (clipDetails.SubClip != null) // isPreview
                    //        videoUrl = String.Empty;

                    clipDetails.Url = videoUrl;
                    // clipDetails.PromptToSubscribe = (clipDetails != null);
                }
            }
            return (clipDetails);
        }

        public static AkamaiFlowPlayerPluginClipDetails GetAkamaiLiveEventClipDetailsIWANTV(int episodeId, int assetId, HttpRequestBase req, System.Security.Principal.IPrincipal thisUser, bool isMobileAllowed, bool removeIpFromToken = false)
        {
            AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            var offeringId = Helpers.GlobalConfig.offeringId;
            var videoUrl = string.Empty;
            var canPlay = false;
            var countryCode = Helpers.MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();

            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringId);

            var episode = context.Episodes.Find(episodeId);
            var asset = context.Assets.Find(assetId);

            var cdnId = 2; // akamai's cdn id

            if (!req.Browser.IsMobileDevice)
                cdnId = 3; // akamai hdn
            if (GlobalConfig.IsIosHLSCdnEnabled)
            {
                if (!req.Browser.IsMobileDevice && !MyUtility.IsDeviceHtml5Capable(req))
                    cdnId = 3; // akamai hdn
                else // get IOS stream
                {
                    if (MyUtility.IsAndroid(req) && !MyUtility.IsDeviceHtml5Capable(req)) { cdnId = 3; }
                    else { cdnId = 5; }
                }
            }


            if ((episode != null) & (asset != null))
            {
                clipDetails = new AkamaiFlowPlayerPluginClipDetails { EpisodeId = episodeId, AssetId = assetId, UserId = (thisUser.Identity.IsAuthenticated ? thisUser.Identity.Name : null) };

                canPlay = clipDetails.IsFree = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null) | (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                // check with anonymous default package
                var packageId = GlobalConfig.AnonymousDefaultPackageId;
                if (!canPlay)
                {
                    canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);
                }
                else
                {
                    clipDetails.IsFree = true;
                }

                // check user's access rights
                if (!canPlay && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = Helpers.GlobalConfig.LoggedInDefaultPackageId;
                        canPlay = User.IsAssetEntitled(context, offeringId, packageId, episodeId, assetId, countryCode, RightsType.Online);

                        if (!canPlay)
                        {
                            // check if user has entitlements that can play the video
                            canPlay = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                        {
                            clipDetails.IsFree = true;
                        }
                    }
                }

                // get asset URL
                var assetCdn = asset.AssetCdns.FirstOrDefault(a => a.CdnId == cdnId);
                if (assetCdn != null)
                {
                    var hlsPrefixPattern = GlobalConfig.hlsLiveStreamPrefixPattern;
                    var hlsSuffixPattern = GlobalConfig.hlsLiveStreamSuffixPattern;
                    var zeriPrefixPattern = GlobalConfig.zeriLiveStreamPrefixPattern;
                    var zeriSuffixPattern = GlobalConfig.zeriLiveStreamSuffixPattern;
                    var httpPrefixPatternMobile = GlobalConfig.httpLiveStreamPrefixPatternMobile;

                    //if (req.Browser.IsMobileDevice)
                    //{
                    //    videoUrl = assetCdn.CdnReference.Replace(zeriPrefixPattern, httpPrefixPatternMobile);
                    //}
                    //else
                    //{
                    //    videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, zeriPrefixPattern).Replace(hlsSuffixPattern, zeriSuffixPattern);
                    //}

                    if (cdnId == 3) //Akamai HDN means Flash based.
                        videoUrl = assetCdn.CdnReference;
                    else
                        videoUrl = assetCdn.CdnReference.Replace(hlsPrefixPattern, httpPrefixPatternMobile);


                    string ipAddress = string.Empty;
                    if (!req.IsLocal)
                        ipAddress = req.GetUserHostAddressFromCloudflare();

                    int snippetStart = 0;
                    int snippetEnd = 0;

                    if (!canPlay)
                        clipDetails.PromptToSubscribe = true;
                    else
                        clipDetails.PromptToSubscribe = false;

                    clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;

                    TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(GlobalConfig.AkamaiAddSeconds));
                    double unixTime = ts.TotalSeconds;

                    var tokenConfig = new AkamaiTokenConfig();
                    tokenConfig.TokenAlgorithm = Algorithm.HMACSHA256;
                    tokenConfig.StartTime = Convert.ToUInt32(unixTime);
                    tokenConfig.Window = 300;
                    tokenConfig.Key = Helpers.GlobalConfig.AkamaiTokenKey;
                    if (IsIos(req, true))
                        tokenConfig.Key = Helpers.GlobalConfig.AkamaiIosTokenKey;

                    //check if episode id should use the new ios token key
                    var episodeIdList = MyUtility.StringToIntList(GlobalConfig.LiveEpisodeIdsToUseIosTokenKey2);
                    if (episodeIdList.Contains(episode.EpisodeId))
                        tokenConfig.Key = Helpers.GlobalConfig.AkamaiIosTokenKey2;
                    var episodeIdList2 = MyUtility.StringToIntList(GlobalConfig.EpisodeIdsToUseIosTokenKey3);
                    if (episodeIdList2.Contains(episode.EpisodeId))
                        tokenConfig.Key = Helpers.GlobalConfig.AkamaiIosTokenKey3;

                    tokenConfig.Acl = "/*";
                    if (!removeIpFromToken)
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

                    if (!isMobileAllowed)
                        if (req.Browser.IsMobileDevice) //Apple devices
                            if (clipDetails.SubClip != null) // isPreview
                                videoUrl = String.Empty;

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
