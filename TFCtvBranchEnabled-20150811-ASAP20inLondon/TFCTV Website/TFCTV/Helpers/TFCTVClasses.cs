using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IPTV2_Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TFCTV.Helpers
{
    #region "Helper Classes"

    /// <summary>
    /// Description: Used to store the Json data used for the Features functionality
    /// </summary>
    public class JsonFeatureItem
    {
        public int? EpisodeId { get; set; }

        public string EpisodeName { get; set; }

        public string EpisodeAirDate { get; set; }

        public string EpisodeDescription { get; set; }

        public string EpisodeImageUrl { get; set; }

        public int? ShowId { get; set; }

        public string ShowName { get; set; }

        public string ShowDescription { get; set; }

        public string ShowImageUrl { get; set; }

        public string ShowBannerUrl { get; set; }

        public string CelebrityFullName { get; set; }

        public string Blurb { get; set; }

        public int parentId { get; set; }
        public string parent { get; set; }

        public int CelebrityId { get; set; }
        public string CelebrityImgUrl { get; set; }
        public string CelebrityDescription { get; set; }
    }

    /// <summary>
    /// Description: Used to store the traversed information (Category > SubCategory > Show) on Start up
    /// </summary>
    public class ShowLookUpObject
    {
        public int MainCategoryId { get; set; }

        public int? SubCategoryId { get; set; }

        public int ShowId { get; set; }

        public string MainCategory { get; set; }

        public string SubCategory { get; set; }

        public string Show { get; set; }

        public int? EpisodeId { get; set; }

        public string EpisodeName { get; set; }

        public string ShowType { get; set; }
    }

    /// <summary>
    /// Description: Used to store the traversed information (Category > SubCategory > Show) on Start up
    /// </summary>
    public class JsonCarouselItem
    {
        public string BannerImageUrl { get; set; }

        public int CarouselSlideId { get; set; }

        public string Name { get; set; }

        public string Header { get; set; }

        public string Blurb { get; set; }

        public string ButtonLabel { get; set; }

        public string TargetUrl { get; set; }
    }

    public class EpisodeDisplay
    {
        public int EpisodeId { get; set; }

        public int? EpisodeNumber { get; set; }

        public string EpisodeName { get; set; }

        public string Description { get; set; }

        public DateTime? DateAired { get; set; }

        public string DateAiredStr { get; set; }

        public int? EpisodeLength { get; set; }

        public string EpLength { get; set; }
    }
    public class EpisodeDisplayMiniImage
    {
        public int EpisodeId { get; set; }

        public int? EpisodeNumber { get; set; }

        public string EpisodeName { get; set; }

        public string Description { get; set; }

        public DateTime? DateAired { get; set; }

        public string DateAiredStr { get; set; }

        public int? EpisodeLength { get; set; }

        public string EpLength { get; set; }

        public string ImageSmall { get; set; }
    }

    public class PackageProductUpgradeDisplay
    {
        public int? PackageId { get; set; }

        public int? CurrentPackageId { get; set; }

        public string PackageName { get; set; }

        public int? CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string ProductName { get; set; }

        public int? ProductId { get; set; }

        public int? ALaCarteSubscriptionTypeId { get; set; }

        public int? CurrentProductId { get; set; }

        public string DurationType { get; set; }

        public int? Duration { get; set; }
    }

    public class UserEntitlementDisplay
    {
        public int EntitlementId { get; set; }

        public int? PackageId { get; set; }

        public string PackageName { get; set; }

        public int? CategoryId { get; set; }

        public string CategoryName { get; set; }

        public int? EpisodeId { get; set; }

        public string EpisodeName { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string ProductName { get; set; }

        public int? ProductId { get; set; }

        public int? ALaCarteSubscriptionTypeId { get; set; }

        public string DurationType { get; set; }

        public int Duration { get; set; }
    }

    public class EntitlementDisplay
    {
        public int EntitlementId { get; set; }

        public int? PackageId { get; set; }

        public string PackageName { get; set; }

        public int? CategoryId { get; set; }

        public string CategoryName { get; set; }

        public int? EpisodeId { get; set; }

        public string EpisodeName { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string Content { get; set; }

        public string ExpiryDateStr { get; set; }
    }

    public class TransactionDisplay
    {
        public int TransactionId { get; set; }

        public string Currency { get; set; }

        public decimal Amount { get; set; }

        public string Reference { get; set; }

        public string PaymentType { get; set; }

        public string PpcId { get; set; }

        public int ProductId { get; set; }

        public string TransactionType { get; set; }

        public string ReloadType { get; set; }

        public string ProductName { get; set; }

        public DateTime TransactionDate { get; set; }

        public string Method { get; set; }

        public string TransactionDateStr { get; set; }
    }

    [Serializable]
    public class CategoryShowListDisplay
    {
        public int CategoryId { get; set; }

        public string ImagePoster { get; set; }

        public string Description { get; set; }

        public DateTime? AiredDate { get; set; }

        public Decimal? Ratings { get; set; }

        public int? EpisodeCount { get; set; }

        public int? TotalLikes { get; set; }

        public int? TotalLoves { get; set; }

        public int? TotalComments { get; set; }
    }

    [Serializable]
    public class ShowD
    {
        public int id { get; set; }

        public string name { get; set; }
    }

    public class CategoryD
    {
        public int id { get; set; }

        public string name { get; set; }

        public IList<ShowD> shows { get; set; }
    }

    public class UserAction
    {
        public string actorUID { get; set; }

        public string userMessage { get; set; }

        public string subtitle { get; set; }

        public string title { get; set; }

        public string linkBack { get; set; }

        public string description { get; set; }

        public List<ActionLink> actionLinks { get; set; }

        public List<MediaItem> mediaItems { get; set; }
    }

    public class MediaItem
    {
        public string src { get; set; }

        public string href { get; set; }

        public string type { get; set; }
    }

    public class ActionLink
    {
        public string text { get; set; }

        public string href { get; set; }
    }

    //[Serializable]
    public class MyMenu
    {
        public int id { get; set; }

        public string name { get; set; }

        public virtual List<MyMenuShows> shows { get; set; }

        public int? type { get; set; }
        public int showcount { get; set; }
    }

    public class MyMenuShows
    {
        public string name { get; set; }

        public int id { get; set; }

        public string image { get; set; }

        public string blurb { get; set; }

        public int? type { get; set; }

        public virtual List<JsonFeatureItem> features { get; set; }

    }

    public class MyMainMenu
    {
        public int id { get; set; }

        public string name { get; set; }

        public virtual List<MyMenu> menu { get; set; }
    }

    public class CreditCard
    {
        public string text { get; set; }

        public string value { get; set; }
    }

    public class MyEntitledContentDisplay
    {
        public int id { get; set; }

        public string type { get; set; }

        public string title { get; set; }

        public string blurb { get; set; }

        public string parent { get; set; }

        public string image { get; set; }

        public string ExpiryDate { get; set; }
    }


    public class MostLovedShowsDisplay
    {
        public string categoryName { get; set; }

        public int categoryId { get; set; }

        public int totalLove { get; set; }
        public string slug { get; set; }
    }


    public class MostLovedEpisodesDisplay
    {
        public int showId { get; set; }

        public string showName { get; set; }

        public string dateAired { get; set; }

        public string episodeName { get; set; }

        public int episodeId { get; set; }

        public int totalLove { get; set; }
        public string slug { get; set; }
        public string showSlug { get; set; }
    }


    public class MostLovedCelebritiesDisplay
    {
        public int celebrityId { get; set; }

        public string celebrityName { get; set; }

        public int totalLove { get; set; }

        public string slug { get; set; }
    }


    public class TopReviewersDisplay
    {
        public string userId { get; set; }

        public string userName { get; set; }

        public string userPhoto { get; set; }

        public int totalReview { get; set; }
    }

    public class UserData
    {
        public string ProfileSetting { get; set; }
        public string ProfilePageShare { get; set; }
        public string EveryoneActivityFeedShare { get; set; }
        public string SocialNetworkShare { get; set; }
        public string IsInternalSharingEnabled { get; set; }
        public string IsExternalSharingEnabled { get; set; }
        public string IsProfilePrivate { get; set; }

    }

    [Serializable]
    public class hmUserData
    {
        public string FirstName { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
    }

    [Serializable]
    public class hmCountryCount
    {
        public string CountryCode { get; set; }
        public int count { get; set; }
    }

    [Serializable]
    public class CategoryWithPreview
    {
        public CategoryClass Preview { get; set; }
        public CategoryClass Full { get; set; }

    }

    public class HalalanShowAndChannelContainer
    {
        public int Id { get; set; }
        public string type { get; set; }
        public string Url { get; set; }
        public string DisplayName { get; set; }
        public string src { get; set; }
        public string Description { get; set; }
    }


    public class RecurringBillingContainer
    {
        public User user { get; set; }
        public Product product { get; set; }
        public Package package { get; set; }
        public Entitlement entitlement { get; set; }
    }


    public class RecurringBillingDisplay
    {
        public int RecurringBillingId { get; set; }

        public Guid UserId { get; set; }

        public int ProductId { get; set; }

        public int PackageId { get; set; }

        public string ProductName { get; set; }
        public string PackageName { get; set; }

        public DateTime EndDate { get; set; }

        public string EndDateStr { get; set; }

        public int StatusId { get; set; }

        public DateTime NextRun { get; set; }

        public string NextRunStr { get; set; }

        public bool isDisabled { get; set; }
        public string PaymentType { get; set; }
    }
    public class TransactionReturnType
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string HtmlUri { get; set; }
        public string Recipient { get; set; }
        public string WId { get; set; }
        public string CCStatusMessage { get; set; }
        public string info { get; set; }
        public string StatusMessage2 { get; set; }
        public string StatusHeader { get; set; }
        public string TransactionType { get; set; }
        public string info1 { get; set; }
        public string info2 { get; set; }
        public string info3 { get; set; }
        public string info4 { get; set; }
        public string info5 { get; set; }
        public string info6 { get; set; }
        public string info7 { get; set; }
    }

    public class RecurringBillingReturnValue
    {
        public bool value { get; set; }
        public RecurringBillingContainer container { get; set; }
    }


    public class ProductGroupType
    {
        public ProductSubscriptionType productSubscriptionType { get; set; }
        public int type { get; set; }
    }

    public class SubscriptionProductA
    {
        public SubscriptionProduct product { get; set; }
        public Package package { get; set; }
        public Show show { get; set; }
        public ProductGroup productGroup { get; set; }
        public ProductPrice productPrice { get; set; }
        public int contentCount { get; set; }
        public string contentDescription { get; set; }
        public bool isPackage { get; set; }
        public List<string> ListOfDescription { get; set; }
        public string ShowDescription { get; set; }
        public SubscriptionProduct regularProduct { get; set; }
        public ProductPrice regularProductPrice { get; set; }
        public ProductGroup regularProductGroup { get; set; }
        public bool IsUserEnrolledToSameRecurringPackage { get; set; }
        public SubscriptionProduct freeProduct { get; set; }
        public ProductPrice freeProductPrice { get; set; }
        public ProductGroup freeProductGroup { get; set; }
        public string Blurb { get; set; }
    }

    public class PackageContentSummary
    {
        public int PackageId { get; set; }
        public int ContentCount { get; set; }
    }

    public class ShowListDisplay
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string ImgUrl { get; set; }
        public string ParentCategoryName { get; set; }
        public int ParentCategoryId { get; set; }
    }

    public class EpisodeObject
    {
        public string Show { get; set; }
        public int EpisodeId { get; set; }
        public string Name { get; set; }
        public string Synopsis { get; set; }
        public string ImgUrl { get; set; }
        public string DateAiredStr { get; set; }
        public string slug { get; set; }
    }

    public class CheckSubscriptionReturnObject
    {
        public bool HasSubscription { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public bool Within5DaysOrLess { get; set; }
        public int NumberOfDaysLeft { get; set; }
        public bool IsFreeEntitlement { get; set; }
    }

    public class GigyaResponseData
    {
        [JsonProperty("errorCode")]
        public int errorCode { get; set; }
        [JsonProperty("errorMessage")]
        public string errorMessage { get; set; }
        [JsonProperty("errorDetails")]
        public string errorDetails { get; set; }
        [JsonProperty("callId")]
        public string callId { get; set; }
        [JsonProperty("statusCode")]
        public int statusCode { get; set; }
        [JsonProperty("statusReason")]
        public string statusReason { get; set; }
        [JsonProperty("UID")]
        public Guid? UID { get; set; }
        [JsonProperty("next")]
        public string next { get; set; }

        //gm.getChallengeStatus
        [JsonProperty("achievements")]
        public IEnumerable<GigyaChallengeStatusAchievement> achievements { get; set; }

        //gm.getActionsLog
        [JsonProperty("actions")]
        public IEnumerable<GigyaActionsLog> actions { get; set; }

        //gm.getTopUsers
        [JsonProperty]
        public IEnumerable<GigyaTopUsers> users { get; set; }

        //gm.getVariants
        [JsonProperty("variants")]
        public IEnumerable<GigyaChallengeVariants> variants { get; set; }
    }


    public class GigyaChallengeStatusAchievement
    {
        public string badgeURL { get; set; }
        public string challengeID { get; set; }
        public bool isNewLevel { get; set; }
        public int level { get; set; }
        public int points7Days { get; set; }
        public int pointsTotal { get; set; }
        public string levelDescription { get; set; }
        public string levelTitle { get; set; }
    }

    public class GigyaActionsLog
    {
        public string time { get; set; }
        public string actionID { get; set; }
        public int points { get; set; }
        public string ip { get; set; }
        public string challengeID { get; set; }
        public string VariantId { get; set; }
        public string description { get; set; }
    }

    public class GigyaTopUsers
    {
        public string UID { get; set; }
        public string name { get; set; }
        public string photoURL { get; set; }
        public int rank { get; set; }
        public string type { get; set; }
        public IEnumerable<GigyaChallengeStatusAchievement> achievements { get; set; }
    }

    public class GigyaActionAttribute
    {
        [JsonProperty("id")]
        public List<string> id { get; set; }
        [JsonProperty("description")]
        public List<string> description { get; set; }
        [JsonProperty("type")]
        public List<string> type { get; set; }
        [JsonProperty("link")]
        public List<string> link { get; set; }
    }

    public class GigyaActionSingleAttribute
    {
        [JsonProperty("description")]
        public List<string> description { get; set; }
    }

    public class GigyaChallengeVariants
    {
        public string variantID { get; set; }
        public GigyaActionAttribute actionAttributes { get; set; }
    }

    public class GigyaCombinedActionsLog
    {
        public DateTime time { get; set; }
        public int points { get; set; }
        public string description { get; set; }
        public string timeStr { get; set; }
    }

    public class PaypalManageRecurringPaymentsProfileObj
    {
        public string PROFILEID { get; set; }
        public string TIMESTAMP { get; set; }
        public string CORRELATIONID { get; set; }
        public string ACK { get; set; }
        public string VERSION { get; set; }
        public string BUILD { get; set; }
        public string L_ERRORCODE0 { get; set; }
        public string L_SHORTMESSAGE0 { get; set; }
        public string L_LONGMESSAGE0 { get; set; }
        public string L_SEVERITYCODE0 { get; set; }
    }
    public class FPJThematicBundle
    {
        public string BundleName { get; set; }
        public string BundleDescription { get; set; }
        public string featureBanner { get; set; }
        public int featureId { get; set; }
        public List<FPJFeaturedMovie> featuredTitles { get; set; }

    }
    public class FPJFeaturedMovie
    {
        public string movieImage { get; set; }
        public string movieTitle { get; set; }
        public int movieEpisodeID { get; set; }
        public int episodeNumber { get; set; }
    }

    public class CelebrityDisplay
    {
        public int CelebrityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageUrl { get; set; }

    }

    public class HomepageFeatureItem
    {
        public int? id { get; set; }

        public string name { get; set; }

        public string airdate { get; set; }

        public string description { get; set; }

        public string imgurl { get; set; }

        public int? show_id { get; set; }

        public string show_name { get; set; }

        public string show_imgurl { get; set; }

        public string blurb { get; set; }

        public string slug { get; set; }


    }

    public class GigyaGetCommentsResponseData
    {
        [JsonProperty("errorCode")]
        public int errorCode { get; set; }
        [JsonProperty("errorMessage")]
        public string errorMessage { get; set; }
        [JsonProperty("errorDetails")]
        public string errorDetails { get; set; }
        [JsonProperty("callId")]
        public string callId { get; set; }
        [JsonProperty("statusCode")]
        public int statusCode { get; set; }
        [JsonProperty("statusReason")]
        public string statusReason { get; set; }
        [JsonProperty("next")]
        public string next { get; set; }
        [JsonProperty("commentCount")]
        public int commentCount;
        [JsonProperty("comments")]
        public string comments;
    }

    public class TVEContentListObj
    {
        public string MainCategory { get; set; }
        public int MainCategoryId { get; set; }
        public List<ShowD> shows { get; set; }
    }

    public class GigyaShareObj
    {
        public string title { get; set; }
        public string href { get; set; }
        public string img { get; set; }
        public string description { get; set; }
        public string containerId { get; set; }
        public string elementType { get; set; }
        public int id { get; set; }
        public string show_title { get; set; }
        public string playlist_title { get; set; }
        public string dateaired { get; set; }
    }

    public class GigyaWidgetUIObj
    {
        public string streamID { get; set; }
        public string categoryID { get; set; }
        public string containerId { get; set; }
        public string linkedCommentsUI { get; set; }
        public bool IsAjaxCrawlable { get; set; }
        public GigyaShareObj ShareObj { get; set; }
    }

    public class FlowPlayerObj
    {
        public bool IsLiveStream { get; set; }
        public bool IsMobileDeviceHtml5Capable { get; set; }
    }

    public class Html5CapableObj
    {
        public string playbackUri { get; set; }
        public bool IsMobileDeviceHtml5Capable { get; set; }
    }

    public class Mp4CapableObj
    {
        public string playbackUri { get; set; }
        public bool UseMp4ForPlayback { get; set; }
    }

    public class TfcTvApiPlaybackObj
    {
        public int PlayTypeId { get; set; }
        public string UserId { get; set; }
        public string DateTime { get; set; }
        public int EpisodeId { get; set; }
        public int CategoryId { get; set; }
        public int Duration { get; set; }
        public int Length { get; set; }
        public int AssetId { get; set; }
        public int StartPosition { get; set; }
        public int LastPosition { get; set; }
        public int MinBitrate { get; set; }
        public int MaxBitrate { get; set; }
        public int AvgBitrate { get; set; }
        public int BufferCount { get; set; }
        public int TotalBufferDuration { get; set; }
        public string DeviceHeader { get; set; }
        public string ClientIp { get; set; }
        public bool IsPreview { get; set; }
    }

    public class JWPObject
    {
        public string playbackUri { get; set; }
        public string ScreenImage { get; set; }
        public int EpisodeId { get; set; }
        public string title { get; set; }
        public Show show { get; set; }
        public string clipType { get; set; } //vod or live
        public string ErrorFontSize { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasSubscription { get; set; }
        public string ScreenText { get; set; }
        public string playbackHigh { get; set; }
        public string playbackLow { get; set; }
        public bool? IsFree { get; set; }
        public int LastPosition { get; set; }
    }

    public class JWPObject2
    {
        public string playbackUri { get; set; }
        public string highUri { get; set; }
        public string lowUri { get; set; }
        public string ScreenImage { get; set; }
        public string ScreenText { get; set; }
        public int EpisodeId { get; set; }
        public string title { get; set; }
        public string type { get; set; } //vod or live
        public bool preview { get; set; }
        public bool? free { get; set; }
        public int lastpos { get; set; }
        public bool mobile { get; set; }
    }

    public class ReturnObj
    {
        public bool IsSuccess { get; set; }
        public string StatusMessage { get; set; }
    }

    public class AirLoginReturnObj
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string sessionToken { get; set; }
        public string sessionSecret { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }

    public class ProgramSchedDisplay
    {
        public int Pid { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
    }

    public class VideoPlaybackObj
    {
        public int PlayTypeId { get; set; }
        public string UserId { get; set; }
        public string DateTime { get; set; }
        public int EpisodeId { get; set; }
        public int CategoryId { get; set; }
        public int Duration { get; set; }
        public int Length { get; set; }
        public int AssetId { get; set; }
        public int StartPosition { get; set; }
        public int LastPosition { get; set; }
        public int MinBitrate { get; set; }
        public int MaxBitrate { get; set; }
        public int AvgBitrate { get; set; }
        public int BufferCount { get; set; }
        public int TotalBufferDuration { get; set; }
        public string DeviceHeader { get; set; }
        public object ClientIp { get; set; }
        public bool IsPreview { get; set; }
    }

    public class VideoApiPlaybackObj
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public VideoPlaybackObj videoPlayback { get; set; }
    }

    public class ServiceReturnType
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string StatusMessage2 { get; set; }
        public string info { get; set; }
        public string info1 { get; set; }
        public string info2 { get; set; }
        public string info3 { get; set; }
        public string info4 { get; set; }
        public string info5 { get; set; }
        public string info6 { get; set; }
        public string info7 { get; set; }
    }
    public class ITEResponse
    {
        [JsonProperty("statuscode")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("packagetype")]
        public string packageType { get; set; }
        [JsonProperty("startdate")]
        public DateTime startDate { get; set; }
        [JsonProperty("expirationdate")]
        public DateTime expirationDate { get; set; }
        [JsonProperty("tfctvUserId")]
        public string tfctvUserId { get; set; }
    }

    public class PacMayVideoObj
    {
        public string DateAired { get; set; }
        public string Title { get; set; }
        public string url { get; set; }
        public string img { get; set; }
        public int EpisodeId { get; set; }
    }

    public class UserDataObj
    {
        public UserDataData data { get; set; }
        public int statusCode { get; set; }
        public int errorCode { get; set; }
        public string statusReason { get; set; }
        public string callId { get; set; }
    }

    public class UserDataData
    {
        public string about { get; set; }
        public string aboutme { get; set; }
        public string status { get; set; }
        public List<idIdentity> _identities { get; set; }
        public string _photoURL { get; set; }
        public string _thumbnailURL { get; set; }
        public string _firstName { get; set; }
        public string _lastName { get; set; }
        public string _gender { get; set; }
        public int _age { get; set; }
        public int _birthDay { get; set; }
        public int _birthMonth { get; set; }
        public int _birthYear { get; set; }
        public string _email { get; set; }
        public string _city { get; set; }
        public string _profileURL { get; set; }
        public string _proxiedEmail { get; set; }
        public string _bio { get; set; }
        public object _education { get; set; }
        public string _hometown { get; set; }
        public List<string> _interestedIn { get; set; }
        public string _languages { get; set; }
        public List<string> _likes { get; set; }
        public string _locale { get; set; }
        public string _name { get; set; }
        public string _politicalView { get; set; }
        public string _relationshipStatus { get; set; }
        public string _religion { get; set; }
        public string _timezone { get; set; }
        public string _username { get; set; }
        public string _verified { get; set; }
        public string _work { get; set; }
        public double _iRank { get; set; }
        public string _id { get; set; }
        public string City { get; set; }
        public string ConfirmPassword { get; set; }
        public string CountryCode { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string State { get; set; }
    }

    public class idIdentity
    {
        public string age { get; set; }
        public bool allowsLogin { get; set; }
        public string birthDay { get; set; }
        public string birthMonth { get; set; }
        public string birthYear { get; set; }
        public string city { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string gender { get; set; }
        public bool isExpiredSession { get; set; }
        public bool isLoginIdentity { get; set; }
        public string lastName { get; set; }
        public string lastUpdated { get; set; }
        public object lastUpdatedTimestamp { get; set; }
        public string oldestDataUpdated { get; set; }
        public object oldestDataUpdatedTimestamp { get; set; }
        public string photoURL { get; set; }
        public string profileURL { get; set; }
        public string provider { get; set; }
        public string providerUID { get; set; }
        public string proxiedEmail { get; set; }
        public string thumbnailURL { get; set; }
    }


    public class idData
    {
        public string IsExternalSharingEnabled { get; set; }
        public string IsInternalSharingEnabled { get; set; }
        public string IsProfilePrivate { get; set; }
    }

    public class idProfile
    {
        public string city { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string state { get; set; }
    }

    public class idAccountObj
    {
        public string callId { get; set; }
        public string created { get; set; }
        public long createdTimestamp { get; set; }
        public idData data { get; set; }
        public int errorCode { get; set; }
        public List<idIdentity> identities { get; set; }
        public bool isLockedOut { get; set; }
        public string lastLogin { get; set; }
        public long lastLoginTimestamp { get; set; }
        public string lastUpdated { get; set; }
        public long lastUpdatedTimestamp { get; set; }
        public string loginProvider { get; set; }
        public string oldestDataUpdated { get; set; }
        public long oldestDataUpdatedTimestamp { get; set; }
        public idProfile profile { get; set; }
        public string signatureTimestamp { get; set; }
        public string socialProviders { get; set; }
        public int statusCode { get; set; }
        public string statusReason { get; set; }
        public string time { get; set; }
        public string UID { get; set; }
        public string UIDSignature { get; set; }
    }

    public class NetsuiteReturnObj
    {
        public bool IsUnderMaintenance { get; set; }
        public string StatusMessage { get; set; }
    }

    public class StreamSenseObj
    {
        public string playlist { get; set; }
        public string program { get; set; }
        public int id { get; set; }
        public DateTime? dateaired { get; set; }
        public bool IsEpisode { get; set; }
    }

    public class RecommendedItemLink
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    public class RecommendedItemProperty
    {
        public string name { get; set; }
        public string text { get; set; }
    }

    public class RecommendedItem
    {
        public string id { get; set; }
        public List<string> tag { get; set; }
        //public List<object> link { get; set; }
        public List<RecommendedItemLink> link { get; set; }
        public List<RecommendedItemProperty> property { get; set; }
    }

    public class RecommendedItemObj
    {
        public string version { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public RecommendedItem item { get; set; }
    }


    public class RecommendedShows
    {
        public string ImageMobileLandscape { get; set; }
        public string ImageBanner { get; set; }
        public string ImageTitleBanner { get; set; }
        public string Title { get; set; }
        public string ShowId { get; set; }
        public string ImageMobileIcon { get; set; }
        public string StatusId { get; set; }
        public string Blurb { get; set; }
        public string ImagePoster { get; set; }
        public string ImageTitle { get; set; }
        public string ImagePath { get; set; }
        public string ItemType { get; set; }
        public string text { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public string count { get; set; }
        public List<RecommendedShows> recommended { get; set; }
    }

    public class RecommendedShowObj
    {
        public string version { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public Item item { get; set; }
    }

    public class TfcTvApiCelebrityReactionPostObj
    {
        public int ReactionTypeId { get; set; }
        public int CelebrityId { get; set; }
        public Guid UserId { get; set; }
        public string DateTime { get; set; }
        public int Rating { get; set; }
    }

    public class TfcTvApiCategoryReactionPostObj
    {
        public int ReactionTypeId { get; set; }
        public int CategoryId { get; set; }
        public Guid UserId { get; set; }
        public string DateTime { get; set; }
        public int Rating { get; set; }
    }

    public class TfcTvApiEpisodeReactionPostObj
    {
        public int ReactionTypeId { get; set; }
        public int EpisodeId { get; set; }
        public int CategoryId { get; set; }
        public Guid UserId { get; set; }
        public string DateTime { get; set; }
        public int Rating { get; set; }
    }


    public class AirPlusEntitlementObj
    {
        public DateTime? EndDate { get; set; }
        public string EndDateStr { get; set; }
        public int NumberOfDaysLeft { get; set; }
        public bool IsExpired { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }

    public class AirPlusAdsObj
    {
        public string copy1 { get; set; }
        public string copy2 { get; set; }
        public string cta { get; set; }
        public string img { get; set; }
        public int designtype { get; set; }
    }

    public class ReturnCodeObj
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }

    public class SocializeReturnCodeObj
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public TFCTV.Models.SynapseToken tk { get; set; }
        public TFCTV.Models.SynapseCookie gs { get; set; }
    }

    public class StoreFrontC
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string BusinessName { get; set; }
        public string BusinessPhone { get; set; }
        public string City { get; set; }
        public string ContactPerson { get; set; }
        public string CountryCode { get; set; }
        public string EMailAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string MobilePhone { get; set; }
        public string State { get; set; }
        public int StoreFrontId { get; set; }
        public string WebSiteUrl { get; set; }
        public string ZipCode { get; set; }
    }


    #endregion "Helper Classes"
}
