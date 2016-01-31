using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using IPTV2_Model;
using Newtonsoft.Json;
using TFCTV.Helpers;
using System.Text.RegularExpressions;

namespace TFCTV.Controllers
{
    public class JsonpResult : ActionResult
    {
        private readonly object _obj;

        public JsonpResult(object obj)
        {
            _obj = obj;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var serializer = new JavaScriptSerializer();
            var callbackname = context.HttpContext.Request["callback"];
            var jsonp = string.Format("{0}({1})", callbackname, serializer.Serialize(_obj));
            var response = context.HttpContext.Response;
            response.ContentType = "application/json";
            response.Write(jsonp);
        }
    }

    public class IWantvController : Controller
    {
        //
        // GET: /IWantv/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _CheckVideo(int? id)
        {
            //if (String.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))
            //    return RedirectToAction("Index", "Home");

            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = MyUtility.getErrorMessage(ErrorCodes.UnknownError);

            bool access = false;
            int[] ids = { 21799, 21801, 21802, 21803 };
            foreach (int d in ids)
            {
                if (d == id)
                {
                    access = true;
                }
            }

            if (!access)
            {
                collection = MyUtility.setError(errorCode, "Episode not found.");

                return new JsonpResult(collection);
            }

            collection = MyUtility.setError(errorCode, errorMessage);

            var context = new IPTV2Entities();
            Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id && e.OnlineStatusId == GlobalConfig.Visible);
            if (ep != null)
            {
                Asset asset = ep.PremiumAssets.FirstOrDefault().Asset;
                if (asset != null)
                {
                    int assetId = asset == null ? 0 : asset.AssetId;
                    ViewBag.AssetId = assetId;
                    var clipDetails = Helpers.Akamai.GetAkamaiClipDetails(ep.EpisodeId, assetId, Request, User);

                    if (!String.IsNullOrEmpty(clipDetails.Url))
                    {
                        errorCode = ErrorCodes.Success;
                        collection = MyUtility.setError(errorCode, clipDetails.Url);
                        collection.Add("data", clipDetails);
                    }
                    else
                    {
                        errorCode = ErrorCodes.AkamaiCdnNotFound;
                        collection = MyUtility.setError(errorCode, "Akamai Url not found.");
                    }
                }
                else
                {
                    errorCode = ErrorCodes.VideoNotFound;
                    collection = MyUtility.setError(errorCode, "Video not found.");
                }
            }
            else
            {
                errorCode = ErrorCodes.EpisodeNotFound;
                collection = MyUtility.setError(errorCode, "Episode not found.");
            }
            return new JsonpResult(collection);
        }

        public JsonResult Asap20(int? id)
        {
            var ReturnCode = new ServiceReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            try
            {
                if (!Request.IsLocal)
                {
                    Regex rx = new Regex(GlobalConfig.Asap20InLondonAllowedDomains);
                    if (!rx.IsMatch(Request.Url.Host))
                    {
                        ReturnCode.StatusMessage = String.Format("Unauthorized: {0}", Request.Url.Host);
                        return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception) { }
            //if (!Request.IsLocal)
            //    if (!GlobalConfig.isUAT)
            //        if (!Request.IsAjaxRequest())
            //        {
            //            ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
            //            ReturnCode.StatusMessage = "Request is not valid.";
            //            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
            //        }

            if (id == null)
                id = GlobalConfig.Asap20InLondonEpisodeId;

            DateTime registDt = DateTime.Now;
            try
            {
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();

                //if (MyUtility.IsWhiteListed(String.Empty))
                //    CountryCode = "PH";
                //else
                //{
                //    try
                //    {
                //        var requestingIp = Request.GetUserHostAddressFromCloudflare();
                //        var whiteListedIp = GlobalConfig.IpWhiteList.Split(',');
                //        if (whiteListedIp.Contains(requestingIp) && GlobalConfig.isUAT)
                //            CountryCode = "PH";
                //    }
                //    catch (Exception) { }
                //}

                //if (String.Compare(CountryCode, "PH", true) != 0)
                //{
                //    ReturnCode.StatusMessage = String.Format("Unauthorized: {0}", CountryCode);
                //    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                //}

                using (var context = new IPTV2Entities())
                {
                    Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                    if (episode != null)
                    {
                        if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                        {
                            Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                            var HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = true, Within5DaysOrLess = false };
                            if (HasActiveSubscriptionBasedOnCategoryId.HasSubscription)
                            {
                                var premiumAsset = episode.PremiumAssets.LastOrDefault();
                                if (premiumAsset != null)
                                {
                                    Asset asset = premiumAsset.Asset;
                                    if (asset != null)
                                    {
                                        int assetId = asset == null ? 0 : asset.AssetId;
                                        if (episode.IsLiveChannelActive == true) //isLiveEvent
                                        {
                                            if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(premiumAsset))
                                            {
                                                if (Request.Browser.IsMobileDevice)
                                                {
                                                    ReturnCode.StatusCode = (int)ErrorCodes.IsNotAvailableOnMobileDevices;
                                                    ReturnCode.StatusMessage = "This stream is not available on this device";
                                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails(episode.EpisodeId, assetId, Request, User, true, removeIpFromToken: true);
                                            //clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, true, CountryCodeOverride: CountryCode, RemoveIpFromToken: true);                                            
                                        }
                                        else
                                        {
                                            bool HasHD = ContextHelper.DoesEpisodeHaveAkamaiHDCdnReferenceBasedOnAsset(episode);
                                            if (HasHD)
                                                clipDetails = Helpers.Akamai.GetAkamaiClipDetailsIWANTV(episode.EpisodeId, assetId, Request, User, VideoQualityCdnReference.HighDefinition, removeIpFromToken: true);
                                            else
                                                clipDetails = Helpers.Akamai.GetAkamaiClipDetailsIWANTV(episode.EpisodeId, assetId, Request, User, VideoQualityCdnReference.StandardDefinition, removeIpFromToken: true);
                                        }

                                        if (clipDetails != null)
                                        {
                                            if (!String.IsNullOrEmpty(clipDetails.Url))
                                            {
                                                ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                ReturnCode.StatusMessage = "OK";
                                                ReturnCode.info = clipDetails.Url;
                                                return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                            }
                                        }

                                        ReturnCode.StatusMessage = "Clip not available.";
                                        return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                    }
                                    ReturnCode.StatusMessage = "Asset not available.";
                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                }
                            }
                            ReturnCode.StatusMessage = "Access to this clip is restricted.";
                            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                        }
                    }
                    ReturnCode.StatusMessage = "Episode does not exist.";
                }
            }
            catch (Exception e) { ReturnCode.StatusMessage = e.Message; }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Asap20HLS()
        {
            var ReturnCode = new ServiceReturnType()
            {
                StatusCode = (int)ErrorCodes.UnknownError,
                StatusMessage = String.Empty
            };

            try
            {
                if (!Request.IsLocal)
                {
                    Regex rx = new Regex(GlobalConfig.Asap20InLondonAllowedDomains);
                    if (!rx.IsMatch(Request.Url.Host))
                    {
                        ReturnCode.StatusMessage = String.Format("Unauthorized: {0}", Request.Url.Host);
                        return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception) { }
            //if (!Request.IsLocal)
            //    if (!GlobalConfig.isUAT)
            //        if (!Request.IsAjaxRequest())
            //        {
            //            ReturnCode.StatusCode = (int)ErrorCodes.IsInvalidRequest;
            //            ReturnCode.StatusMessage = "Request is not valid.";
            //            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
            //        }

            int id = GlobalConfig.Asap20InLondonEpisodeId;
            DateTime registDt = DateTime.Now;
            try
            {
                string CountryCode = MyUtility.GetCountryCodeViaIpAddressWithoutProxy();
                using (var context = new IPTV2Entities())
                {
                    Episode episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                    if (episode != null)
                    {
                        if (episode.OnlineStatusId == GlobalConfig.Visible && episode.OnlineStartDate < registDt && episode.OnlineEndDate > registDt)
                        {
                            Offering offering = context.Offerings.Find(GlobalConfig.offeringId);
                            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                            var HasActiveSubscriptionBasedOnCategoryId = new CheckSubscriptionReturnObject() { HasSubscription = true, Within5DaysOrLess = false };
                            if (HasActiveSubscriptionBasedOnCategoryId.HasSubscription)
                            {
                                var premiumAsset = episode.PremiumAssets.LastOrDefault();
                                if (premiumAsset != null)
                                {
                                    Asset asset = premiumAsset.Asset;
                                    if (asset != null)
                                    {
                                        int assetId = asset == null ? 0 : asset.AssetId;
                                        if (episode.IsLiveChannelActive == true) //isLiveEvent
                                        {
                                            if (!ContextHelper.DoesEpisodeHaveIosCdnReferenceBasedOAsset(premiumAsset))
                                            {
                                                if (Request.Browser.IsMobileDevice)
                                                {
                                                    ReturnCode.StatusCode = (int)ErrorCodes.IsNotAvailableOnMobileDevices;
                                                    ReturnCode.StatusMessage = "This stream is not available on this device";
                                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            //clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails(episode.EpisodeId, assetId, Request, User, true, removeIpFromToken: true);
                                            clipDetails = Helpers.Akamai.GetAkamaiLiveEventClipDetails_M3U8(episode.EpisodeId, assetId, Request, User, true, CountryCodeOverride: String.Empty, RemoveIpFromToken: true);
                                            if (clipDetails != null)
                                                if (!String.IsNullOrEmpty(clipDetails.Url))
                                                {
                                                    ReturnCode.StatusCode = (int)ErrorCodes.Success;
                                                    ReturnCode.StatusMessage = "OK";
                                                    ReturnCode.info = clipDetails.Url;
                                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                                }
                                        }
                                        ReturnCode.StatusMessage = "Clip not available.";
                                        return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                    }
                                    ReturnCode.StatusMessage = "Asset not available.";
                                    return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                                }
                            }
                            ReturnCode.StatusMessage = "Access to this clip is restricted.";
                            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
                        }
                    }
                    ReturnCode.StatusMessage = "Episode does not exist.";
                }
            }
            catch (Exception e) { ReturnCode.StatusMessage = e.Message; }
            return this.Json(ReturnCode, JsonRequestBehavior.AllowGet);
        }
    }
}