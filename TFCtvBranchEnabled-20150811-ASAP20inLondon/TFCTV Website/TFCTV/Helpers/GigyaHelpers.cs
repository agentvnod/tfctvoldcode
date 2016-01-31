using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gigya.Socialize.SDK;
using Newtonsoft.Json;

namespace TFCTV.Helpers
{
    public class GigyaHelpers
    {
        public static string buildJson(Dictionary<string, string> collection)
        {
            return JsonConvert.SerializeObject(collection, Formatting.None);
        }

        public static GSRequest createRequest(string method, GSObject obj)
        {
            return new GSRequest(GlobalConfig.GSapikey, GlobalConfig.GSsecretkey, method, obj, true);
        }

        public static GSObject buildParameter(Dictionary<string, string> collection)
        {
            string json = GigyaHelpers.buildJson(collection);
            return new GSObject(json);
        }

        public static GSObject buildParameter(Dictionary<string, object> collection)
        {
            string json = MyUtility.buildJson(collection);
            return new GSObject(json);
        }

        public static GSResponse createAndSendRequest(string method, GSObject obj)
        {
            GSRequest req = new GSRequest(GlobalConfig.GSapikey, GlobalConfig.GSsecretkey, method, obj, true);
            return req.Send();
        }

        public static HttpCookie buildCookie(GSResponse res)
        {
            HttpCookie cookie = new HttpCookie(res.GetString("cookieName", string.Empty));
            cookie.Domain = res.GetString("cookieDomain", string.Empty);
            cookie.Path = res.GetString("cookiePath", string.Empty);
            cookie.Value = res.GetString("cookieValue", string.Empty);
            //cookie.Expires = DateTime.MinValue;
            //cookie.HttpOnly = true;
            return cookie;
        }

        public static void setCookie(GSResponse res, ControllerContext context)
        {
            HttpCookie cookie = new HttpCookie(res.GetString("cookieName", string.Empty));
            //cookie.Domain = res.GetString("cookieDomain", string.Empty);
            //cookie.Path = res.GetString("cookiePath", string.Empty);
            cookie.Value = res.GetString("cookieValue", string.Empty);
            //cookie.Expires = DateTime.MinValue;
            cookie.Expires = DateTime.Now.AddDays(30);
            //cookie.HttpOnly = true;
            context.HttpContext.Response.Cookies.Add(cookie);
        }
    }

    //public class GSLogOutParam
    //{
    //    public string paramName { get; set; }

    //    public string paramValue { get; set; }
    //}

    public class GigyaUserInfo
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
    }

    public class GigyaNotifyLoginInfo
    {
        public string siteUID { get; set; }
        public string cid { get; set; }
        public int sessionExpiration { get; set; }
        public bool newUser { get; set; }
        public string userInfo { get; set; }
    }

    public class GigyaUserData
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }

    public class GigyaUserDataInfo
    {
        public string UID { get; set; }
        public string data { get; set; }
    }

    public class FeedObjAction
    {
        public string actorNickame { get; set; }
        public string actorUID { get; set; }
        public string actionName { get; set; }
        public List<object> targets { get; set; }
        public List<object> images { get; set; }
        public string iconURL { get; set; }
        public string title { get; set; }
        public string linkBack { get; set; }
        public string userMessage { get; set; }
        public string description { get; set; }
        public List<ActionLink> actionLinks { get; set; }
        public List<MediaItem> mediaItems { get; set; }
    }

    public class FeedObjSender
    {
        public string photoURL { get; set; }
        public string profileURL { get; set; }
        public string loginProvider { get; set; }
        public string name { get; set; }
    }

    public class FeedObjItem
    {
        public FeedObjAction action { get; set; }
        public string timestamp { get; set; }
        public FeedObjSender sender { get; set; }
    }

    public class FeedObjFeedItem
    {
        public List<FeedObjItem> items { get; set; }
        public bool ready { get; set; }
    }

    public class FeedObj
    {
        public FeedObjFeedItem everyone { get; set; }
        public FeedObjFeedItem me { get; set; }
        public FeedObjFeedItem friends { get; set; }
        public long nextTS { get; set; }
        public int statusCode { get; set; }
        public int errorCode { get; set; }
        public string statusReason { get; set; }
        public string callId { get; set; }
    }

    public class GigyaIdentity
    {
        public string provider { get; set; }
        public string providerUID { get; set; }
        public bool isLoginIdentity { get; set; }
        public string nickname { get; set; }
        public string photoURL { get; set; }
        public string thumbnailURL { get; set; }
    }

    public class GigyaFriend
    {
        public string UID { get; set; }
        public bool isSiteUser { get; set; }
        public bool isSiteUID { get; set; }
        public List<GigyaIdentity> identities { get; set; }
        public string nickname { get; set; }
        public string photoURL { get; set; }
        public string thumbnailURL { get; set; }
    }

    public class GigyaFriendsInfo
    {
        public List<GigyaFriend> friends { get; set; }
        public int statusCode { get; set; }
        public int errorCode { get; set; }
        public string statusReason { get; set; }
        public string callId { get; set; }
    }

    public class UserInfoEducation
    {
        public string school { get; set; }
        public string schoolType { get; set; }
        public string startYear { get; set; }
        public string fieldOfStudy { get; set; }
        public string degree { get; set; }
        public string endYear { get; set; }
    }

    public class UserInfoInterest
    {
        public string name { get; set; }
        public string category { get; set; }
    }

    public class UserInfoActivity
    {
        public string name { get; set; }
        public string category { get; set; }
    }

    public class UserInfoMusic
    {
        public string name { get; set; }
        public string category { get; set; }
    }

    public class UserInfoMovie
    {
        public string name { get; set; }
        public string category { get; set; }
    }

    public class UserInfoTelevision
    {
        public string name { get; set; }
        public string category { get; set; }
    }

    public class UserInfoFavorites
    {
        public List<UserInfoInterest> interests { get; set; }
        public List<UserInfoActivity> activities { get; set; }
        public List<UserInfoMusic> music { get; set; }
        public List<UserInfoMovie> movies { get; set; }
        public List<UserInfoTelevision> television { get; set; }
    }

    public class UserInfoHometown
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class UserInfoLike
    {
        public string name { get; set; }
        public string category { get; set; }
    }

    public class UserInfoWork
    {
        public string company { get; set; }
        public string companyID { get; set; }
        public string title { get; set; }
        public string startDate { get; set; }
        public string industry { get; set; }
        public bool? isCurrent { get; set; }
    }

    public class UserInfoCertification
    {
        public string name { get; set; }
    }

    public class UserInfoDate
    {
        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }
    }

    public class UserInfoPatent
    {
        public string title { get; set; }
        public UserInfoDate date { get; set; }
    }

    public class UserInfoPublication
    {
        public string title { get; set; }
        public UserInfoDate date { get; set; }
    }

    public class UserInfoSkill
    {
        public string skill { get; set; }
    }

    public class UserInfoIdentity
    {
        public string provider { get; set; }
        public string providerUID { get; set; }
        public bool isLoginIdentity { get; set; }
        public string nickname { get; set; }
        public string photoURL { get; set; }
        public string thumbnailURL { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string age { get; set; }
        public string birthDay { get; set; }
        public string birthMonth { get; set; }
        public string birthYear { get; set; }
        public string email { get; set; }
        public string city { get; set; }
        public string profileURL { get; set; }
        public string proxiedEmail { get; set; }
        public bool allowsLogin { get; set; }
        public bool isExpiredSession { get; set; }
        public string bio { get; set; }
        public List<UserInfoEducation> education { get; set; }
        public UserInfoFavorites favorites { get; set; }
        public UserInfoHometown hometown { get; set; }
        public string languages { get; set; }
        public List<UserInfoLike> likes { get; set; }
        public string locale { get; set; }
        public string religion { get; set; }
        public int timezone { get; set; }
        public List<UserInfoWork> work { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string address { get; set; }
        public List<UserInfoCertification> certifications { get; set; }
        public string honors { get; set; }
        public string industry { get; set; }
        public List<UserInfoPatent> patents { get; set; }
        public List<UserInfoPublication> publications { get; set; }
        public List<UserInfoSkill> skills { get; set; }
        public string specialties { get; set; }
    }

    public class GetUserInfoObj
    {
        public string UID { get; set; }
        public bool isSiteUser { get; set; }
        public bool isConnected { get; set; }
        public bool isLoggedIn { get; set; }
        public bool isTempUser { get; set; }
        public string loginProvider { get; set; }
        public string loginProviderUID { get; set; }
        public bool isSiteUID { get; set; }
        public List<UserInfoIdentity> identities { get; set; }
        public string nickname { get; set; }
        public string photoURL { get; set; }
        public string thumbnailURL { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string age { get; set; }
        public string birthDay { get; set; }
        public string birthMonth { get; set; }
        public string birthYear { get; set; }
        public string email { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string profileURL { get; set; }
        public string proxiedEmail { get; set; }
        public string capabilities { get; set; }
        public string providers { get; set; }
        public int statusCode { get; set; }
        public int errorCode { get; set; }
        public string statusReason { get; set; }
        public string bio { get; set; }
        public List<UserInfoEducation> education { get; set; }
        public UserInfoFavorites favorites { get; set; }
        public UserInfoHometown hometown { get; set; }
        public string languages { get; set; }
        public List<UserInfoLike> likes { get; set; }
        public string locale { get; set; }
        public string religion { get; set; }
        public int timezone { get; set; }
        public List<UserInfoWork> work { get; set; }
        public string address { get; set; }
        public List<UserInfoCertification> certifications { get; set; }
        public string honors { get; set; }
        public string industry { get; set; }
        public List<UserInfoPatent> patents { get; set; }
        public List<UserInfoPublication> publications { get; set; }
        public List<UserInfoSkill> skills { get; set; }
        public string specialties { get; set; }
    }

    public class DeleteAccountObj
    {
        public int statusCode { get; set; }
        public int errorCode { get; set; }
        public string statusReason { get; set; }
        public string callId { get; set; }
    }

    public class GigyaUserDataInfo2
    {
        public string UID { get; set; }
        public string profile { get; set; }
        public string data { get; set; }
    }

    public class GigyaUserData2
    {
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string state { get; set; }
    }

    public class GigyaUserData3
    {
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string phone { get; set; }
    }
}