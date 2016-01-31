using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPTV2_Model;
using TFCtv_Admin_Website.Helpers;
using Gigya.Socialize.SDK;

namespace TFCtv_Admin_Website.Controllers
{
    public class AjaxController : Controller
    {
        //
        // GET: /Ajax/

        public int CheckEmail(string email)
        {
            if (String.IsNullOrEmpty(email))
                return 0;
            var context = new IPTV2Entities();
            var count = context.Users.Count(item => item.EMail.ToLower() == email.ToLower());
            return count;
        }

        public ActionResult GetAsset(int? id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection = MyUtility.SetError(ErrorCode.UnidentifiedError, String.Empty);
            try
            {
                if (id == null)
                {
                    collection = MyUtility.SetError(ErrorCode.MissingRequiredFields, "Please provide an Episode Id.");
                    return Content(MyUtility.BuildJSON(collection), "application/json");
                }

                var context = new IPTV2Entities();

                Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);

                if (ep != null)
                {
                    var premiumAsset = ep.PremiumAssets.FirstOrDefault();

                    if (premiumAsset != null)
                    {
                        Asset asset = ep.PremiumAssets.FirstOrDefault().Asset;

                        if (asset != null)
                        {
                            int assetId = asset == null ? 0 : asset.AssetId;

                            ViewBag.AssetId = assetId;

                            AkamaiFlowPlayerPluginClipDetails clipDetails = null;
                            if (ep.IsLiveChannelActive == true)
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetails_LiveEvent(ep.EpisodeId, assetId, Request, User);
                            else
                                clipDetails = Helpers.Akamai.GetAkamaiClipDetails(ep.EpisodeId, assetId, Request, User);


                            if (!String.IsNullOrEmpty(clipDetails.Url))
                            {
                                collection = MyUtility.SetError(ErrorCode.Success, clipDetails.Url);
                                collection.Add("data", clipDetails);
                            }

                            else
                            {
                                collection = MyUtility.SetError(ErrorCode.ObjectIsNull, "Asset url is not available.");
                            }
                        }
                        else
                        {
                            collection = MyUtility.SetError(ErrorCode.ObjectIsNull, "Asset url is not available.");
                        }
                    }
                    else
                    {
                        collection = MyUtility.SetError(ErrorCode.ObjectIsNull, "Premium asset is not available.");
                    }
                }
                else
                {
                    collection = MyUtility.SetError(ErrorCode.ObjectIsNull, "Episode is not available.");
                }
            }
            catch (Exception e) { collection = MyUtility.SetError(ErrorCode.UnidentifiedError, e.Message); }

            return Content(MyUtility.BuildJSON(collection), "application/json");
        }


        public ActionResult GetMedia(int? id, int p = 0) // 0 - adapt, 1 - high, 2 - low, 3 - hd
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection = MyUtility.SetError(ErrorCode.UnidentifiedError, String.Empty);

            if (id == null)
            {
                collection = MyUtility.SetError(ErrorCode.MissingRequiredFields, "Please provide an Episode Id.");
                return Content(MyUtility.BuildJSON(collection), "application/json");
            }
            try
            {
                var context = new IPTV2Entities();
                Episode ep = context.Episodes.FirstOrDefault(e => e.EpisodeId == id);
                if (ep != null)
                {
                    var premiumAsset = ep.PremiumAssets.FirstOrDefault();
                    if (premiumAsset != null)
                    {
                        Asset asset = ep.PremiumAssets.FirstOrDefault().Asset;
                        if (asset != null)
                        {
                            int assetId = asset == null ? 0 : asset.AssetId;
                            ViewBag.AssetId = assetId;
                            var clipDetails = Helpers.Akamai.GetAkamaiClipDetails(ep.EpisodeId, assetId, Request, User, (StreamType)p);
                            if (!String.IsNullOrEmpty(clipDetails.Url))
                            {
                                collection = MyUtility.SetError(ErrorCode.Success, clipDetails.Url);
                                collection.Add("data", clipDetails);
                            }
                            else
                                collection = MyUtility.SetError(ErrorCode.ObjectIsNull, "Asset url is not available.");
                        }
                        else
                            collection = MyUtility.SetError(ErrorCode.ObjectIsNull, "Asset url is not available.");
                    }
                    else
                        collection = MyUtility.SetError(ErrorCode.ObjectIsNull, "Premium asset is not available.");
                }
                else
                    collection = MyUtility.SetError(ErrorCode.ObjectIsNull, "Episode is not available.");
            }
            catch (Exception e)
            {
                collection = MyUtility.SetError(ErrorCode.UnidentifiedError, e.Message);
            }
            return Content(MyUtility.BuildJSON(collection), "application/json");
        }

        public JsonResult GetUser(string id)
        {
            UserObj obj = new UserObj() { code = ErrorCode.UnidentifiedError };
            try
            {
                Guid UserId;
                try
                {
                    UserId = Guid.Parse(id);
                }
                catch (Exception) { throw new Exception("UserId is invalid"); }
                var context = new IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                if (user != null)
                {
                    obj.user = new UserO
                    {
                        City = user.City,
                        CountryCode = user.CountryCode,
                        UserId = user.UserId,
                        DateVerified = user.DateVerified.HasValue ? user.DateVerified.Value.ToString("MM/dd/yyyy hh:mm:ss tt") : String.Empty,
                        EMail = user.EMail,
                        FirstName = user.FirstName,
                        IsTVEverywhere = user.IsTVEverywhere
                        ,
                        LastName = user.LastName,
                        RegistrationDate = user.RegistrationDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        State = user.State
                    };
                    obj.code = ErrorCode.Success;
                }

                //Gigya
                try
                {
                    Dictionary<string, object> collection = new Dictionary<string, object>();
                    collection.Add("UID", user.UserId.ToString());
                    GSObject gsObj = new GSObject(Newtonsoft.Json.JsonConvert.SerializeObject(collection));
                    GSRequest req = new GSRequest(Global.GSapikey, Global.GSsecretkey, "socialize.getUserInfo", gsObj, true); ;
                    GSResponse res = req.Send();
                    obj.gUser = Newtonsoft.Json.JsonConvert.DeserializeObject<GetUserInfoObj>(res.GetData().ToJsonString());
                }
                catch (Exception) { }
            }
            catch (Exception e) { obj.message = e.Message; }
            return this.Json(obj, JsonRequestBehavior.AllowGet);
        }
    }
}