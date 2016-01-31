using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gigya.Socialize.SDK;
using IPTV2_Model;
using Newtonsoft.Json;
using TFCTV.Helpers;
using TFCTV.Models;

namespace TFCTV.Controllers
{
    public class WishlistController : Controller
    {
        //
        // GET: /Wishlist/

        public ActionResult Index()
        {
            var context = new IPTV2Entities();
            Product product = context.Products.FirstOrDefault(p => p.ProductId == 1 && p.OfferingId == GlobalConfig.offeringId && p.StatusId == GlobalConfig.Visible);
            Wishlist wishlist = new Wishlist()
            {
                UID_s = User.Identity.Name,
                registDt_d = DateTime.Now.ToString("s"),
                ProductId_i = product.ProductId,
                ProductName_s = product.Description
            };

            WishlistModel model = new WishlistModel()
            {
                type = "Wishlist",
                data = wishlist
            };
            string wishlist_json = JsonConvert.SerializeObject(model);
            return Content(wishlist_json, "application/json");
        }

        [HttpPost]
        public ActionResult Add(int? id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = "ERROR";
            collection = MyUtility.setError(errorCode, errorMessage);

            if (id != null)
            {
                if (!IsWishlisted((int)id))
                {
                    if (!User.Identity.IsAuthenticated)
                    {
                        collection = MyUtility.setError(ErrorCodes.NotAuthenticated, "NOT AUTHENTICATED");
                        return Content(MyUtility.buildJson(collection), "application/json");
                    }

                    var context = new IPTV2Entities();
                    Product product = context.Products.FirstOrDefault(p => p.ProductId == id && p.OfferingId == GlobalConfig.offeringId && p.StatusId == GlobalConfig.Visible);
                    if (product != null)
                    {
                        Wishlist wishlist = new Wishlist()
                        {
                            UID_s = User.Identity.Name,
                            registDt_d = DateTime.Now.ToString("s"),
                            ProductId_i = product.ProductId,
                            ProductName_s = product.Description
                        };

                        WishlistModel model = new WishlistModel()
                        {
                            type = "Wishlist",
                            data = wishlist
                        };
                        string wishlist_json = JsonConvert.SerializeObject(model);
                        GSResponse res = GigyaHelpers.createAndSendRequest("gcs.setObjectData", new GSObject(wishlist_json));

                        if (res.GetErrorCode() == 0)
                        {
                            //Publish user action
                            List<ActionLink> actionlinks = new List<ActionLink>();
                            actionlinks.Add(new ActionLink() { text = SNSTemplates.actionlink_text, href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.wishlist_actionlink_href, User.Identity.Name)) });
                            List<MediaItem> mediaItems = new List<MediaItem>();
                            mediaItems.Add(new MediaItem() { type = SNSTemplates.wishlist_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.wishlist_mediaitem_src), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.wishlist_mediaitem_href, User.Identity.Name)) });
                            string gender = GigyaMethods.GetUserInfoByKey(new System.Guid(User.Identity.Name), "gender");
                            UserAction action = new UserAction()
                            {
                                actorUID = User.Identity.Name,
                                userMessage = String.Format(SNSTemplates.wishlist_usermessage, product.Description, gender == "f" ? "her" : "his"),
                                title = SNSTemplates.wishlist_title,
                                subtitle = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.wishlist_subtitle, User.Identity.Name)),
                                linkBack = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.wishlist_linkback, User.Identity.Name)),
                                description = SNSTemplates.wishlist_description,
                                actionLinks = actionlinks,
                                mediaItems = mediaItems
                            };

                            var userId = new Guid(User.Identity.Name);
                            var userData = MyUtility.GetUserPrivacySetting(userId);
                            if (userData.IsExternalSharingEnabled.Contains("true"))
                                GigyaMethods.PublishUserAction(action, userId, "external");
                            //Modify action to suit Internal feed needs
                            mediaItems.Clear();
                            mediaItems.Add(new MediaItem() { type = SNSTemplates.wishlist_mediaitem_type, src = String.Format("{0}{1}", GlobalConfig.AssetsBaseUrl, SNSTemplates.wishlist_mediaitem_src_internal), href = String.Format("{0}{1}", GlobalConfig.baseUrl, String.Format(SNSTemplates.wishlist_mediaitem_href, User.Identity.Name)) });
                            action.description = SNSTemplates.wishlist_description_internal;
                            action.mediaItems = mediaItems;
                            if (userData.IsInternalSharingEnabled.Contains("true"))
                                GigyaMethods.PublishUserAction(action, userId, "internal");
                        }
                        return Content(res.GetData().ToJsonString(), "application/json");
                    }
                }
                else
                {
                    errorCode = ErrorCodes.WishlistItemExists;
                    errorMessage = "ALREADY EXISTS";
                    collection = MyUtility.setError(errorCode, errorMessage);
                }
            }

            return Content(MyUtility.buildJson(collection), "application/json");
        }

        //public ActionResult Search(int? id)
        //{
        //    string query = "select _id, ProductId_i, ProductName_s, registDt_d, UID_s from Wishlist";
        //    if (id != null)
        //        query += " where ProductId_i = " + id.ToString();
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    collection.Add("query", query);
        //    GSResponse res = GigyaHelpers.createAndSendRequest("gcs.search", GigyaHelpers.buildParameter(collection));
        //    return Content(res.GetData().ToJsonString(), "application/json");
        //}

        /// <summary>
        /// Get Details of Wishlisted item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(string id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = "ERROR";
            collection = MyUtility.setError(errorCode, errorMessage);

            if (!String.IsNullOrEmpty(id))
            {
                string query = String.Format(@"select _id, ProductId_i, ProductName_s, registDt_d, UID_s from Wishlist where UID_s = ""{0}"" and _id = ""{1}""", User.Identity.Name, id);
                Dictionary<string, object> gcollection = new Dictionary<string, object>();
                gcollection.Add("query", query);
                GSResponse res = GigyaHelpers.createAndSendRequest("gcs.search", GigyaHelpers.buildParameter(gcollection));
                if (res.GetData().GetInt("objectsCount") > 0)
                    return Content(res.GetData().ToJsonString(), "application/json");
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = "ERROR";
            collection = MyUtility.setError(errorCode, errorMessage);

            if (User.Identity.IsAuthenticated)
            {
                if (BelongsToUser(id))
                {
                    Dictionary<string, object> gcollection = new Dictionary<string, object>();
                    gcollection.Add("id", id);
                    gcollection.Add("type", "Wishlist");
                    GSResponse res = GigyaHelpers.createAndSendRequest("gcs.deleteObjectData", GigyaHelpers.buildParameter(gcollection));
                    return Content(res.GetData().ToJsonString(), "application/json");
                }
                else
                {
                    errorCode = ErrorCodes.WishlistNotFound;
                    errorMessage = "NOT FOUND";
                    collection = MyUtility.setError(errorCode, errorMessage);
                }
            }
            return Content(MyUtility.buildJson(collection), "application/json");
        }

        public ActionResult List(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                ViewBag.UserId = id;
                if (String.Compare(id, User.Identity.Name, true) == 0)
                    ViewBag.isAllowed = "true";
                else
                    ViewBag.isAllowed = "false";
            }
            else
                ViewBag.isAllowed = "true";

            return PartialView("_WishlistPartial");
        }

        public ActionResult _List(string id)
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            ErrorCodes errorCode = ErrorCodes.UnknownError;
            string errorMessage = "ERROR";
            collection = MyUtility.setError(errorCode, errorMessage);

            string userId = String.IsNullOrEmpty(id) ? User.Identity.Name : id.ToLower();
            string query = String.Format(@"select _id, ProductId_i, ProductName_s, registDt_d, UID_s from Wishlist where UID_s = ""{0}""", userId);
            Dictionary<string, object> gcollection = new Dictionary<string, object>();
            gcollection.Add("query", query);
            GSResponse res = GigyaHelpers.createAndSendRequest("gcs.search", GigyaHelpers.buildParameter(gcollection));
            if (res.GetData().GetInt("errorCode") == 0)
            {
                if (res.GetData().GetInt("objectsCount") > 0)
                    return Content(res.GetArray("data", null).ToString(), "application/json");
            }

            //return Content(res.GetData().ToJsonString(), "application/json");
            return Json(collection, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Checker if product has already been wishlisted
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool IsWishlisted(int id)
        {
            string query = String.Format(@"select _id, ProductId_i, ProductName_s, registDt_d, UID_s from Wishlist where UID_s = ""{0}"" and ProductId_i  = {1}", User.Identity.Name, id.ToString());
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("query", query);
            GSResponse res = GigyaHelpers.createAndSendRequest("gcs.search", GigyaHelpers.buildParameter(collection));
            if (res.GetErrorCode() == 0)
            {
                if (res.GetData().GetInt("objectsCount") > 0)
                    return true;
                return false;
            }
            else
                return false;
        }

        //public ActionResult IsWish(int id)
        //{
        //    string query = String.Format(@"select _id, ProductId_i, ProductName_s, registDt_d, UID_s from Wishlist where UID_s = ""{0}"" and ProductId_i  = {1}", User.Identity.Name, id.ToString());
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    collection.Add("query", query);
        //    GSResponse res = GigyaHelpers.createAndSendRequest("gcs.search", GigyaHelpers.buildParameter(collection));
        //    return Content(res.GetData().ToJsonString(), "application/json");
        //}

        private bool BelongsToUser(string id)
        {
            string query = String.Format(@"select _id, ProductId_i, ProductName_s, registDt_d, UID_s from Wishlist where UID_s = ""{0}"" and _id = ""{1}""", User.Identity.Name, id);
            Dictionary<string, object> collection = new Dictionary<string, object>();
            collection.Add("query", query);
            GSResponse res = GigyaHelpers.createAndSendRequest("gcs.search", GigyaHelpers.buildParameter(collection));
            if (res.GetData().GetInt("objectsCount") > 0)
                return true;
            return false;
        }

        //public ActionResult q()
        //{
        //    string query = "select * from Wishlist";
        //    Dictionary<string, object> collection = new Dictionary<string, object>();
        //    collection.Add("query", query);
        //    GSResponse res = GigyaHelpers.createAndSendRequest("gcs.search", GigyaHelpers.buildParameter(collection));
        //    return Content(res.GetData().ToJsonString(), "application/json");
        //}
    }
}