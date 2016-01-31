using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gigya.Socialize.SDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TFCTV.Helpers
{
    public class GigyaMethods
    {
        public static GSArray GetWishlistDetails(string id)
        {
            string query = String.Format(@"select _id, ProductId_i, ProductName_s, registDt_d, UID_s from Wishlist where _id = ""{0}""", id);
            Dictionary<string, object> gcollection = new Dictionary<string, object>();
            gcollection.Add("query", query);
            GSResponse res = GigyaHelpers.createAndSendRequest("gcs.search", GigyaHelpers.buildParameter(gcollection));
            try
            {
                if (res.GetData().GetInt("objectsCount") > 0)
                    return res.GetArray("data", null);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static string DeleteWishlist(string id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("id", id);
            collection.Add("type", "Wishlist");
            GSResponse res = GigyaHelpers.createAndSendRequest("gcs.deleteObjectData", GigyaHelpers.buildParameter(collection));
            return res.GetData().ToJsonString();
        }

        public static string PublishUserAction(UserAction userAction, System.Guid userId)
        {
            var action = JsonConvert.SerializeObject(userAction);
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("userAction", action);
            collection.Add("scope", "both");
            collection.Add("enabledProviders", GlobalConfig.SocialProvidersList);
            collection.Add("privacy", "public");
            collection.Add("shortURLs", "whenRequired");
            collection.Add("uid", userId.ToString());
            collection.Add("feedID", "UserAction");
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.publishUserAction", GigyaHelpers.buildParameter(collection));
            return res.GetData().ToJsonString();
        }

        public static string PublishUserAction(UserAction userAction, System.Guid userId, string scope)
        {
            var action = JsonConvert.SerializeObject(userAction);
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("userAction", action);
            collection.Add("scope", scope);
            if (String.Compare(scope, "external", true) == 0)
                collection.Add("enabledProviders", GlobalConfig.SocialProvidersList);
            collection.Add("shortURLs", "whenRequired");
            collection.Add("uid", userId.ToString());
            if (String.Compare(scope, "internal", true) == 0)
            {
                collection.Add("privacy", "public");
                collection.Add("feedID", "UserAction");
            }
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.publishUserAction", GigyaHelpers.buildParameter(collection));
            return res.GetData().ToJsonString();
        }


        public static string PublishUserAction(UserAction userAction, System.Guid userId, string scope, string privacy)
        {
            var action = JsonConvert.SerializeObject(userAction);
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("userAction", action);
            collection.Add("scope", scope);
            if (String.Compare(scope, "external", true) == 0)
                collection.Add("enabledProviders", GlobalConfig.SocialProvidersList);
            collection.Add("shortURLs", "whenRequired");
            collection.Add("UID", userId.ToString());
            collection.Add("privacy", privacy);
            collection.Add("feedID", "UserAction");
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.publishUserAction", GigyaHelpers.buildParameter(collection));
            return res.GetData().ToJsonString();
        }

        public static string GetUserInfo(System.Guid userId)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("uid", userId.ToString());
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(collection));
            return res.GetData().ToJsonString();
        }

        public static string GetPhoto(System.Guid userId)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("uid", userId);
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(collection));
            return res.GetString("thumbnailURL", String.Empty);
        }
        public static string GetUserInfoByKey(System.Guid userId, string key)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("uid", userId.ToString());
            GSResponse res = GigyaHelpers.createAndSendRequest("socialize.getUserInfo", GigyaHelpers.buildParameter(collection));
            return res.GetString(key, String.Empty);
        }

        public static string SetUserData(Guid userId, UserData userData)
        {
            var data = JsonConvert.SerializeObject(userData);
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("UID", userId.ToString());
            collection.Add("data", data);
            //collection.Add("updateBehavior", "arraySet");
            //GSResponse res = GigyaHelpers.createAndSendRequest("gcs.setUserData", GigyaHelpers.buildParameter(collection));
            GSResponse res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", GigyaHelpers.buildParameter(collection));            
            return res.GetData().ToJsonString();
        }

        public static GSObject GetUserData(Guid userId, string key)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("UID", userId.ToString());
            //collection.Add("fields", key);
            //GSResponse res = GigyaHelpers.createAndSendRequest("gcs.getUserData", GigyaHelpers.buildParameter(collection));
            GSResponse res = GigyaHelpers.createAndSendRequest("ids.getAccountInfo", GigyaHelpers.buildParameter(collection));
            return res.GetObject("data", null);

        }

        public static GigyaResponseData GetChallengeStatus(Guid UserId)
        {
            GigyaResponseData responseData = null;
            try
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("UID", UserId.ToString());
                GSResponse res = GigyaHelpers.createAndSendRequest("gm.getChallengeStatus", GigyaHelpers.buildParameter(collection));
                responseData = JsonConvert.DeserializeObject<GigyaResponseData>(res.GetData().ToJsonString());
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return responseData;
        }

        public static GigyaResponseData GetActionsLog(Guid UserId)
        {
            GigyaResponseData responseData = null;
            try
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("UID", UserId.ToString());
                GSResponse res = GigyaHelpers.createAndSendRequest("gm.getActionsLog", GigyaHelpers.buildParameter(collection));
                responseData = JsonConvert.DeserializeObject<GigyaResponseData>(res.GetData().ToJsonString());
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return responseData;
        }

        public static GigyaResponseData GetTopUsers(Guid? UserId, string challenge, int totalCount = 10, bool httpStatusCodes = false)
        {
            GigyaResponseData responseData = null;
            Guid guidTester;
            try
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                if (UserId != null)
                    collection.Add("UID", UserId);
                collection.Add("challenge", challenge);
                collection.Add("totalCount", totalCount);
                collection.Add("period", "all");
                collection.Add("httpStatusCodes", httpStatusCodes);
                GSResponse res = GigyaHelpers.createAndSendRequest("gm.getTopUsers", GigyaHelpers.buildParameter(collection));
                responseData = JsonConvert.DeserializeObject<GigyaResponseData>(res.GetData().ToJsonString());
                var guidusers = responseData.users.Where(r => Guid.TryParse(r.UID, out guidTester));
                responseData.users = guidusers;
                //responseData = responseDataCushion;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return responseData;
        }

        public static GigyaResponseData GetVariantsbyVariantIDs(IEnumerable<string> variantIDs)
        {

            GigyaResponseData responseData = null;
            if (variantIDs.Count() == 0)
                return responseData;
            string variantID = string.Join(",", variantIDs);
            try
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("variantID", variantID);
                collection.Add("limit", 20);
                collection.Add("challengeID", GlobalConfig.GigyaPromoChallengeID);
                GSResponse res = GigyaHelpers.createAndSendRequest("gm.getChallengeVariants", GigyaHelpers.buildParameter(collection));
                responseData = JsonConvert.DeserializeObject<GigyaResponseData>(res.GetData().ToJsonString());
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return responseData;
        }

        public static GigyaResponseData NotifyAction(Guid UserId, string action, GigyaActionSingleAttribute actionAttribute)
        {
            GigyaResponseData responseData = null;
            try
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("UID", UserId.ToString());
                collection.Add("action", action);
                if (actionAttribute != null)
                    collection.Add("actionAttributes", actionAttribute);
                GSResponse res = GigyaHelpers.createAndSendRequest("gm.notifyAction", GigyaHelpers.buildParameter(collection));
                responseData = JsonConvert.DeserializeObject<GigyaResponseData>(res.GetData().ToJsonString());
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return responseData;
        }
        public static GigyaResponseData GetVariantIDsbyUser(Guid UserId, AnniversaryPromo id)
        {
            GigyaResponseData responseData = null;
            List<string> action = new List<string>();
            switch (id)
            {
                case AnniversaryPromo.AnnivPromo_PostingCOMMENTS:
                case AnniversaryPromo.AnnivPromo_PostingREVIEW:
                    action.Add(AnniversaryPromo.AnnivPromo_PostingCOMMENTS.ToString());
                    action.Add(AnniversaryPromo.AnnivPromo_PostingREVIEW.ToString());
                    break;
                case AnniversaryPromo.AnnivPromo_ViewingFREE:
                case AnniversaryPromo.AnnivPromo_ViewingPAID:
                    action.Add(AnniversaryPromo.AnnivPromo_ViewingFREE.ToString());
                    action.Add(AnniversaryPromo.AnnivPromo_ViewingPAID.ToString()); break;
                default: action.Add(id.ToString()); break;
            }
            try
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("UID", UserId.ToString());
                GSResponse res = GigyaHelpers.createAndSendRequest("gm.getActionsLog", GigyaHelpers.buildParameter(collection));
                responseData = JsonConvert.DeserializeObject<GigyaResponseData>(res.GetData().ToJsonString());
                //because free/paid and comment/review  are joined in summary
                responseData.actions = responseData.actions.Where(r => !String.IsNullOrEmpty(r.VariantId) && action.Contains(r.actionID)).OrderByDescending(r => r.time);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return responseData;
        }
        public static int GetPointsLogByAction(Guid UserId, AnniversaryPromo id)
        {
            var points = 0;
            GigyaResponseData responseData = null;
            List<string> action = new List<string>();

            switch (id)
            {
                case AnniversaryPromo.AnnivPromo_PostingCOMMENTS:
                case AnniversaryPromo.AnnivPromo_PostingREVIEW:
                    action.Add(AnniversaryPromo.AnnivPromo_PostingCOMMENTS.ToString());
                    action.Add(AnniversaryPromo.AnnivPromo_PostingREVIEW.ToString());
                    break;
                case AnniversaryPromo.AnnivPromo_ViewingFREE:
                case AnniversaryPromo.AnnivPromo_ViewingPAID:
                    action.Add(AnniversaryPromo.AnnivPromo_ViewingFREE.ToString());
                    action.Add(AnniversaryPromo.AnnivPromo_ViewingPAID.ToString()); break;
                default: action.Add(id.ToString()); break;
            }

            try
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("UID", UserId.ToString());
                GSResponse res = GigyaHelpers.createAndSendRequest("gm.getActionsLog", GigyaHelpers.buildParameter(collection));
                responseData = JsonConvert.DeserializeObject<GigyaResponseData>(res.GetData().ToJsonString());
                //because free/paid and comment/review  are joined in summary
                points = responseData.actions.Where(r => !String.IsNullOrEmpty(r.VariantId) && action.Contains(r.actionID)).Sum(r => r.points);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return points;
        }

        public static GigyaGetCommentsResponseData GetComments(string CategoryId, string StreamId)
        {
            GigyaGetCommentsResponseData responseData = null;
            try
            {
                Dictionary<string, object> collection = new Dictionary<string, object>();
                collection.Add("categoryID", CategoryId);
                collection.Add("streamID", StreamId);
                collection.Add("dataFormat", "html");
                GSResponse res = GigyaHelpers.createAndSendRequest("comments.getComments", GigyaHelpers.buildParameter(collection));
                responseData = JsonConvert.DeserializeObject<GigyaGetCommentsResponseData>(res.GetData().ToJsonString());
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return responseData;
        }

        public static string SetUserData(Guid userId, UserData privacyData, GigyaUserData2 userData)
        {
            var pData = JsonConvert.SerializeObject(privacyData);
            var uData = JsonConvert.SerializeObject(userData);
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("UID", userId.ToString());
            collection.Add("profile", uData);
            collection.Add("data", pData);
            //collection.Add("updateBehavior", "arraySet");
            //GSResponse res = GigyaHelpers.createAndSendRequest("gcs.setUserData", GigyaHelpers.buildParameter(collection));
            GSResponse res = GigyaHelpers.createAndSendRequest("ids.setAccountInfo", GigyaHelpers.buildParameter(collection));
            return res.GetData().ToJsonString();
        }
    }
}