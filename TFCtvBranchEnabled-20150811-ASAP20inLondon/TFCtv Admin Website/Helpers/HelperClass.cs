using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFCtv_Admin_Website.Helpers
{
    public class QuotableQuote
    {
        public string json_class { get; set; }
        public List<string> tags { get; set; }
        public string quote { get; set; }
        public string link { get; set; }
        public string source { get; set; }
    }

    public class UserObj
    {
        public ErrorCode code { get; set; }
        public string message { get; set; }
        public UserO user { get; set; }
        public GetUserInfoObj gUser { get; set; }
    }

    public class UserO
    {
        public System.Guid UserId { get; set; }
        public string EMail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCode { get; set; }
        public string RegistrationDate { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string DateVerified { get; set; }
        public Nullable<bool> IsTVEverywhere { get; set; }
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
}