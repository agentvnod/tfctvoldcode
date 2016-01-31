using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace TFCTV.Helpers
{
    public static class GlobalConfig
    {
        static GlobalConfig()
        {
            Initialize();
        }

        public static int offeringId;
        public static int serviceId;

        public static int premiumId;
        public static int liteId;
        public static int movieId;

        public static int AnonymousDefaultPackageId;
        public static int LoggedInDefaultPackageId;

        public static string GSapikey;
        public static string GSsecretkey;

        public static Int16 Visible;
        public static Int16 Hidden;
        public static Int16 Deleted;

        public static int Entertainment;
        public static int Movies;
        public static int News;
        //public static int Specials;
        //public static int Drama;
        //public static int Sport;

        #region Home Featured IDS

        public static int LatestFullEpisodes;
        public static int FreeTV;
        public static int MostViewed;
        public static int LatestShows;
        public static int FeaturedCelebrities;

        #endregion Home Featured IDS

        #region Movies Featured IDS

        public static int LatestMovies;
        public static int StaffPicksMovies;
        public static int FeaturedPersonMovies;
        public static int MostPopularMovies;

        #endregion Movies Featured IDS

        #region News Featured IDS

        public static int FeaturedJournalists;
        public static int LatestNewsFullEpisodes;
        public static int NewsFreeTV;
        public static int FeaturedRegionalNews;
        public static int FeaturedCurrentAffairs;

        #endregion News Featured IDS

        #region Carousel IDS

        public static int CarouselEntertainmentId;
        public static int CarouselMovieId;
        public static int CarouselNewsId;

        #endregion Carousel IDS

        #region FreeTV IDS

        public static int FreeTVMostViewedId;
        public static int FreeTVMostSharedId;
        public static int FreeTVTopLikedId;
        public static int FreeTVPlayListId;
        public static int FreeTVComedyId;
        public static int FreeTVDramaId;
        public static int FreeTVSportsId;

        #endregion FreeTV IDS

        public static string SHA1Encryption;

        public static string PDTToken;
        public static string PayPalSubmitUrl;
        public static string PayPalBusiness;
        public static string PayPalReturnUrl;
        public static string PayPalReloadReturnUrl;

        public static string AkamaiTokenKey;

        public static string DefaultCurrency;
        public static string DefaultCountry;
        public static string TrialCurrency;

        public static string GeoIpPath;

        public static string CarouselImgPath;
        public static string ShowImgPath;
        public static string EpisodeImgPath;
        public static string CelebrityImgPath;

        public static string BlankGif;

        public static string baseUrl;

        public static int FAQ;

        public static int SOCIAL_COMMENT;
        public static int SOCIAL_RATING;
        public static int SOCIAL_SHARE;
        public static int SOCIAL_LIKE;
        public static int SOCIAL_LOVE;

        public static int maximumDistance;

        public static string hlsPrefixPattern;
        public static string hlsSuffixPattern;
        public static string zeriPrefixPattern;
        public static string zeriSuffixPattern;
        public static string httpPrefixPatternMobile;

        public static int LitePackageId;
        public static int PremiumPackageId;
        public static string IpWhiteList;
        public static string FaqTop5;
        public static string LatestEpisodeDropdownListIds;
        public static string BingSiteSearchCount;
        public static string ChatId;
        public static bool GoogleSiteSearch;
        public static bool BingSiteSearch;
        public static int CustomSearchResultCount;
        public static int CustomSearchCount;
        public static int moviesPremiumCategoryId;

        public static int FormsAuthenticationTimeout;

        public static int FreeTrial14ProductId;
        public static int FreeTrial7ProductId;
        public static bool IsFreeTrialEnabled;

        public static bool IsEWalletPaymentModeEnabled;
        public static bool IsPpcPaymentModeEnabled;
        public static bool IsCreditCardPaymentModeEnabled;
        public static bool IsPaypalPaymentModeEnabled;
        public static bool IsPpcReloadModeEnabled;
        public static bool IsCreditCardReloadModeEnabled;
        public static bool IsPaypalReloadModeEnabled;

        public static string ServiceDeskUsername;
        public static string ServiceDeskPassword;
        public static int ServiceDeskDepartmentId;
        public static string FeedbackEmail;

        public static string SupportEmail;
        public static string SmtpHost;
        public static int SmtpPort;

        public static string AllowedCountries;

        public static string SendGridUsername;
        public static string SendGridPassword;
        public static string SendGridSmtpHost;
        public static int SendGridSmtpPort;

        public static string RegistrationConfirmPage;
        public static bool IsCreateSupportTicketForLoggedInUsersEnabled;
        public static string NoReplyEmail;
        public static string LogEmail;

        public static bool IsProfilingEnabled;

        public static string TFCnowPackageIds;

        public static string TFCnowPremium;
        public static string TFCnowLite;
        public static string TFCnowMovieChannel;

        public static string TFCnowLiveStream;
        public static string TFCnowMoviePPV;

        public static string JapanCountryCode;
        public static string FreeTrialProductIds;

        public static string EmailVerificationBody;
        public static string EmailVerificationBodyTextOnly;

        public static int snippetEnd;

        public static string TFCnowBasePackageIds;

        public static int FreeTvCategoryId;

        public static string ResetPasswordBodyTextOnly;

        public static string SubscribeToProductBodyTextOnly;
        public static string ExtendSubscriptionBodyTextOnly;
        public static string GiftingSenderBodyTextOnly;
        public static string GiftingRecipientBodyTextOnly;

        public static string PaypalReloadIPNUrl;
        public static string PaypalBuyIPNUrl;
        public static string PaypalDefaultIPNUrl;

        public static bool IsCDNEnabled;

        public static string HeatMapIpWhiteList;
        public static string HeatMapUrl;

        public static string OnlinePremierePreviewCategoryIds;
        public static string OnlinePremiereFullCategoryIds;

        public static string CDNBaseUrl;
        public static string AssetsBaseUrl;
        public static bool IsAssetsEnabled;

        public static int ClickTayoMVEpisodeId;
        public static int ClickTayoMVCategoryId;
        public static int OkGoChannelId;
        public static int DigitalShortsCategoryId;
        public static int OkGoCarouselSlideId;

        public static bool IsPDTEnabled;

        public static string DBMGetUserUrl;

        public static string MultipleLoginRedirectedUrl;

        public static bool IsPreventionOfMultipleLoginEnabled;

        public static int BCWMHChannelId;

        public static string LiveStreamRestrictedCountries;
        public static int LiveStreamSpecialChannelId;
        public static string LiveStreamSpecialBannerImageUrl;
        public static bool IsLiveStreamRestrictionCheckEnabled;

        public static int AkamaiAddSeconds;

        public static string RegistrationCompleteTemplateUrl;
        public static string RegistrationCompleteSubject;

        public static bool IsCheckForEntitlementsForLiveSpecialsEnabled;
        public static string LiveSpecialsPackageIdsRestriction;
        public static string LiveSpecialsCategoryIdsRestriction;

        public static string CategoryIdsInCache;
        public static string MultipleLoginView;

        public static string SSLbaseUrl;

        public static bool IsSSLEnabled;

        public static bool IsEffectiveMeasureEnabled;
        public static bool IsChatEnabled;

        public static int KwentoNgPaskoCategoryId;
        public static string KwentoNgPaskoBannerImageUrl;

        public static int FreeTrialPackageId1;
        public static int FreeTrialPackageId2;
        public static int FreeTrialPackageId3;
        public static int FreeTrialPackageId4;

        public static string FreeTrialPackageIds;

        public static bool IsXmasSkinEnabled;

        public static int DolphyCelebrityId;

        public static int FreeTrialEarlyBirdProductId;

        public static string FreeTrialProductIdsNEW;

        public static string FreeTrialAlaCarteSubscriptionTypes;

        public static string TFCTVPackageIds;

        public static string hlsProgressivePrefixPattern;
        public static string zeriProgressivePrefixPattern;
        public static string zeriProgressiveSuffixPattern;
        public static string httpProgressivePrefixPatternMobile;
        public static string hlsProgressiveSuffixPattern;

        public static int AkamaiPMDAddSeconds;

        public static string PMDSalt;

        public static bool IsPMDPlayerEnabled;

        public static string PMDHighBitrate;
        public static string PMDLowBitrate;

        public static string TFCTvDownloadPlayerUrl;
        public static string TFCTVPlayerDownloadSalt;

        public static bool IsDownloadPlayerEnabled;

        public static int HimigHandogCategoryId;

        public static bool IsThemedSkinEnabled;

        public static int JDCEpisodeId;
        public static string PMDHDBitrate;

        public static bool IsTVERegistrationEnabled;
        public static string TVERegistrationPage;
        public static string RegistrationCompleteTVE;
        public static int ConcertsCategoryId;
        public static bool IsEarlyBirdEnabled;
        public static string RestrictedAdsCategoryIds;
        public static int TFCEverywhereParentCategoryId;
        public static string StreamingTVECategoryIds;
        public static string TVEStreamingChannelCounterpart;
        public static bool UseResponseRedirectOnPreventionOfMultipleLogin;
        public static bool UseResponseBufferOutput;
        public static bool IsAdditionalCssEnabled;
        public static int TFCkatCategoryId;
        public static int TFCkatExclusivesCategoryId;
        public static string TVEMenuIds;
        public static int OnlinePremiereCategoryId;

        public static string hlsLiveStreamPrefixPattern;
        public static string hlsLiveStreamSuffixPattern;
        public static string zeriLiveStreamPrefixPattern;
        public static string zeriLiveStreamSuffixPattern;
        public static string httpLiveStreamPrefixPatternMobile;
        public static string PackageIdsWithChannelAccess;

        public static string HalalanChannelIds;
        public static int HalalanParentCategoryId;
        public static string HalalanOrderedListIds;
        public static int HalalanNewsAlertsParentCategoryId;
        public static int HalalanAdvisoriesParentCategoryId;
        public static int HalalanLatestEpisodesFeatureId;
        public static int HalalanNewsAlertsFeatureId;
        public static int HalalanAdvisoriesFeatureId;
        public static string ElmahAllowedEmails;
        public static bool IsRecurringBillingEnabled;
        public static bool isUAT;
        public static string TVECountryWhiteList;
        public static string ExtendSubscriptionBodyWithAutoRenewTextOnly;
        public static string SubscribeToProductBodyWithAutoRenewTextOnly;
        public static bool IsProfileSiteMapEnabled;
        public static string TVEverywherePackageIds;
        public static string CanPlayExcludedPackageIds;
        public static string CanPlayIncludedPackageIds;
        public static int KapamilyaChatLiveEventEpisodeId;
        public static string CoverItLiveAltCastCode;
        public static DateTime KapamilyaChatUserRegistrationDate;
        public static int KapamilyaChatNumberOfWinners;
        public static int KapamilyaChatRelatedVideosMaxCount;
        public static int KapamilyaChatFeatureId;
        public static bool IsLiveEventEntitlementCheckEnabled;

        public static DateTime FreeTrialStartDt;
        public static bool IsMECreditCardEnabled;
        public static string MECountriesAllowedForCreditCard;
        public static int MiddleEastGomsSubsidiaryId;
        public static bool IsFlowHTML5PlayerEnabled;

        public static int ANCChannelId;
        public static bool IsANCFreeStreamEnabled;
        public static DateTime ANCFreeStreamStartDt;
        public static DateTime ANCFreeStreamEndDt;
        public static string ExcludedCountriesFromRegistrationDropDown;
        public static int SpecialElectionCoverageCategoryId;
        public static int SpecialElectionCoverageEpisodeDisplayCount;
        public static string FreeLiveEventEpisodeIds;
        public static int CoverItLiveCdnId;
        public static bool IsKapamilyaChatRedirectToLiveEnabled;
        public static string AkamaiIosTokenKey;
        public static bool IsTVEIpCheckEnabled;
        public static bool IsIosHLSCdnEnabled;
        public static string ExcludedCategoryIdsForDisplay;
        public static bool UseDaysBasedOnCacheDurationForSessionStore;
        public static string WhitelistedLiveStreamEpisodeIdFromMobileCheck;
        public static int SessionStoreCacheDurationInDays;
        public static int DZMMTVECategoryChannelId;

        public static string AkamaiBeaconIos;
        public static string AkamaiBeaconAdobeFlash;
        public static string csmaPluginPath;
        public static string TVEStreamingEpisodeCounterpart;
        public static int Live;
        public static int paymentTransactionMaximumThreshold;
        public static int reloadTransactionMaximumThreshold;
        public static bool UseProgressiveViaAdaptiveTechnology;
        public static string PMDViaAdaptiveHDBitrate;
        public static string PMDViaAdaptiveHighBitrate;
        public static string PMDViaAdaptiveLowBitrate;
        public static bool CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology;
        public static string EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology;
        public static string UAAPTeamsCelebrityIDs;
        public static string UAAPTeamsImgPath;
        public static int UAAPGreatnessNeverEndsCategoryId;
        public static int UAAPLiveStreamEpisodeId;
        public static int UAAPGamesParentId;
        public static int UAAPExclusiveFeaturesId;
        public static string UAAPEntitlementPackageID;
        public static int UAAPTeamsFeatureId;
        public static bool UseGomsSubsidiaryForTVECheck;
        public static bool IsSynapseEnabled;
        public static int PackageAndProductCacheDuration;
        public static string UAAPCoverItLiveAltCastCode;
        public static bool IsUserEntitlementViewOnPageEnabled;
        public static bool UsePayPalIPNLog;
        public static int MenuCacheDuration;
        public static bool UseCountryListInMemory;
        public static List<IPTV2_Model.Country> CountryList;
        public static int MenuShowBlurbLength;
        public static int MenuMovieBlurbLength;
        public static string LiveEventEpisodesWhereVotingIsEnabled;
        public static bool IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled;
        public static string WelcomePageEpisodeID;
        public static int SynapseGenericCacheDuration;
        public static string PackagesInProductGroup;
        public static string UrlTokenSalt;
        public static string UrlTokenPassword;
        public static bool IsSmartPitReloadEnabled;
        public static string AlaCarteBlurb;
        public static int EventCelebritiesFeatureID;
        public static string TFCkatShowExclusives;
        public static string TFCkatAltCastCode;
        public static bool UseServiceOfferingWhenCheckingEpisodeParentCategory;
        public static int GetParentCategoriesCacheDuration;
        public static int GetParentShowsForEpisodeCacheDuration;
        public static int GetAllSubCategoryIdsCacheDuration;
        public static string GigyaPromoChallengeID;
        public static int TFCtvFirstYearAnniversaryPromoId;
        public static bool IsWelcomePageOn;
        public static bool EnableNotifyLoginOnLinkAccount;
        public static string PromoRunDateString;
        public static string SocialProvidersList;
        public static bool IsForceConnectToSNSEnabled;
        public static int BCWMHWeddingLiveEventEpisodeId;
        public static int BCWMHWeddingSpecialsFeatureId;
        public static int BCWMHWeddingExclusivesFeatureId;
        public static int BCWMHWeddingCategoryId;
        public static string BCWMHCoverItLiveAltCastCode;
        public static int BalitangAmericaSagipKapamilyaEpisodeId;
        public static string BalitangAmericaSagipKapamilyaSlug;
        public static string AlertBoxURL;
        public static bool IsAlertBoxEnabled;
        public static string AlertBoxEnabledUrlList;
        public static int TulongPHConcertEpisodeId;
        public static string TulongPHConcertSlug;
        public static bool IsTFCkatVotingEnabled;
        public static bool IsTFCtvFanMobPromoEnabled;
        public static int KwentoNgPaskoSpecialsFeatureId;
        public static int KwentoNgPaskoExclusivesFeatureId;
        public static string MopayFormPostAddress;
        public static bool IsMopayReloadModeEnabled;
        public static string MopayCountryWhitelist;
        public static string MopayLoadSaltCode;
        public static DateTime TfcTvFree2StartDate;
        public static DateTime TfcTvFree2EndDate;
        public static int TfcTvFree2ProductId;
        public static string TfcTvFree2CountryWhiteList;
        public static DateTime TfcTvFree1StartDate;
        public static DateTime TfcTvFree1EndDate;
        public static string AlertBoxForItalyTaiwanUrl;
        public static string AlertBoxForGermanyUrl;
        public static int KapamilyaLoveChatEpisodeId;
        public static string KapamilyaLoveChatSlug;
        public static int TFCkatGrandWinners;
        public static int ChristmasSpecialEpisodeId;
        public static string ChristmasSpecialSlug;
        public static DateTime IsTFCkatMEVotingDisableDate;
        public static int ASAPDubai2014EpisodeId;
        public static string ASAPDubai2014Slug;
        public static string FPJThematicBundleFeatureIds;
        public static int FPJCategoryId;
        public static int FPJProductId;
        public static int FPJShowIDForSubscribeButton;
        public static int NumberOfItemsInEpisodeList;
        public static int ABSCBNFreeLiveStreamEpisodeId;
        public static bool IsABSCBNFreeLiveStreamFreeOnRegistrationEnabled;
        public static DateTime ABSCBNFreeLiveStreamStartDate;
        public static DateTime ABSCBNFreeLiveStreamEndDate;
        public static int ABSCBNFreeLiveStreamProductId;
        public static int ABSCBNFreeLiveStreamPackageId;
        public static string EmailsAllowedToBypassIPBlock;
        public static string ABSCBNFreeLiveStreamSlug;
        public static string MainMenuItems;

        public static string PaypalAPIUser;
        public static string PaypalAPIPassword;
        public static string PaypalAPISignature;
        public static string PaypalNVPUrl;
        public static string PaypalAPIVersion;
        public static string PaypalAPIMethod;
        public static string PaypalAPIAction;
        public static string ProductIdsExcludedFromRecurringBilling; // NOT USED ANYMORE
        public static bool IsPayPalRecurringBillingEnabled;
        public static string ProductIdsIncludedInRecurringBilling;
        public static bool IsTVERegionBlockingEnabled;
        public static int NumberOfItemsInEpisodeListInEpisodePage;
        public static string MopayBackURL;
        public static int PBBLiveEventEpisodeId;
        public static int BayaningFilipinoEpisodeId;
        public static string BayaningFilipinoSlug;
        public static int PBBLiveStreamFeatureId;
        public static int PBBEpisodesFeatureId;
        public static int PBBUberFeatureId;
        public static string PBBCarouselSlideIds;
        public static string PBBBlockedCountryCodes;

        public static int TFC20DefaultEpisodeId;
        public static int TFC20BayaningFilipinoCategoryId;
        public static string TFC20CoverItLiveAltCastCode;

        public static bool IsMopayListenerEnabled;
        public static string MopayCID;
        public static string MopayPassword;
        public static string MopayDeliveryConfirmationURL;

        public static bool IsMobilePreviewEnabled;
        public static int FeatureItemsPageSize;
        public static int GenericListContentSize;

        public static int FPJParentCategoryId;

        public static string ShowsCategoryIds;
        public static string NewsCategoryIds;
        public static string MoviesCategoryIds;
        public static string LiveCategoryIds;
        public static int UXHomePageCarouselId;

        public static string PremiumProductIds;
        public static string LiteProductIds;
        public static string MovieProductIds;
        public static string UXAlaCarteParentCategoryIds;
        public static string GigyaSealKey;
        public static int DefaultProductIdOffering;
        public static int UXAnonymousHomepageCarouselId;
        public static int UXReviewParentCategoryId;

        public static string MenuExCategory;
        public static string BCWMHThanksgivingConcertSlideIds;
        public static string BCWMHThanksgivingConcertBlockedCountryCodes;
        public static bool IsHDPlaybackEnabled;
        public static int SpecialsCurrentCategoryId;
        public static int PremiumTrialPromoId;
        public static int PremiumTrialPromoProductId;
        public static string LitePromoProductIds;
        public static int LiteChurnersPromoId;
        public static string TFCtvApiVideoPlaybackUri;
        public static string Html5CapableDevicesRegex;
        public static int AdultContentCategoryId;
        public static string FeaturedCelebSlug;
        public static int ProjectAirEpisodeId;
        public static int ProjectAirDisplayCategoryId;
        public static int ProjectAirCategoryId;
        public static int ProjectAirPromoId;
        public static string ProjectAirEmailVerificationBodyTemplateUrl;
        public static string ProjectAirProgramScheduleChannelIds;
        public static string USAllowedCarouselIds;
        public static string Promo2014ProductIds;
        public static int Promo201410PromoId;
        public static string AirRegistrationCompleteTemplateUrl;
        public static string premiumProductGroupIds;
        public static int Compensation2014PromoId;
        public static string SubscriptionProductIds;
        public static string SubscriptionProductIds1Month;
        public static string SubscriptionProductIds3Month;
        public static string SubscriptionProductIds12Month;
        public static int Compensation201411ProductId1Month;
        public static int Compensation201411ProductId3and12Month;
        public static int Compensation201411CategoryId1Month;
        public static int Compensation201411CategoryId3and12Month;
        public static int Compensatory3DaysProductId;
        public static int Compensatory7DaysProductId;
        public static string PreBlackPromoProductIds;
        public static int PreBlackPromoId;
        public static string CAAllowedCarouselIds;
        public static int EdsaWoolworthPromoId;
        public static string ProjectBlackPromoIds;
        public static string ProjectBlackProductIds;
        public static int PremiumComplimentary1Month;
        public static int SurveyPromoId;
        public static int KapitBisigEpisodeId;
        public static string KapitBisigSlug;
        public static int Xoom2PromoId;
        public static int Xoom2FreeProductId;
        public static bool UseJWPlayer;
        public static int TFCtvMobileAirEpisodeId;
        public static string ITEValidateURL;
        public static string ITEWhitelistedIp;
        public static int ITEPromoId;
        public static string AkamaiIosTokenKey2;
        public static string LiveEpisodeIdsToUseIosTokenKey2;
        public static int PinoyPride30EpisodeId;
        public static string PinoyPride30Slug;
        public static DateTime PinoyPride30TakeoverStartDt;
        public static DateTime PinoyPride30TakeoverEndDt;
        public static int PinoyPride30ProductId;
        public static int PacMayVODEpisodeId;
        public static int PacMayLiveEpisodeId;
        public static int PacMaySpecialsFeatureId;
        public static int PacMayLiveConcertShowId;
        public static int PacMayLatestNewsShowId;
        public static int PacMayVODShowId;
        public static int PacMayTrendingVideosCategoryId;
        public static int PacMayPlugOnTrendingVideosEpisodeId;
        public static int PacMaySubscribeCategoryId;
        public static string PacMaySubscribeProductIds;
        public static string MEPacMayCarouselIds;
        public static string MEPacMayAllowedCountryCodes;
        public static string NonMEPacMayCarouselIds;
        public static int PacMayLiveCategoryId;
        public static int PacMayTVCEpisodeId;
        public static string PacMayProductIdsForRedirect;
        public static bool IsStaticPacMayForMECarouselEnabled;
        public static int Q22015PromoId;
        public static string Q22015ProductId;
        public static string ListOfPromoIdsForEvents;
        public static int PBB747Cam1EpisodeId;
        public static int PBB747Cam2EpisodeId;
        public static string PBBHelpFragment;
        public static string VerificationTemplateUrl;
        public static string WelcomeTemplateUrl;
        public static string ReceiptTemplateUrl;
        public static string DaxAllowedParentCategories;
        public static string SiteUmbrellaCategoryIds;
        public static int Asap20InLondonEpisodeId;
        public static string Asap20InLondonAllowedDomains;
        public static string BackgroundTakeoverAllowedCountries;
        public static string AkamaiIosTokenKey3;
        public static string EpisodeIdsToUseIosTokenKey3;
        public static int TwitterUriCdnId;
        public static int TwitterWidgetCdnId;
        public static bool StreamSenseEnabled;
        public static int UAAPMainCategoryId;
        public static string hlsProtocol;
        public static string httpProtocol;
        public static string hdsFolder;
        public static string hlsFolder;
        public static int NetsuiteMaintenancePromoId;
        public static string TfctvRecommendedItemsApi;
        public static string Country14DayTrials;
        public static int AirPlusPackageId;
        public static int TFCtvPaidMobileAirEpisodeId;
        public static string ListOfAirPlusPackageIds;
        public static string AirPlusAdTypeIds;

        public static void Initialize()
        {

            offeringId = Convert.ToInt32(Settings.GetSettings("offeringId"));
            serviceId = Convert.ToInt32(Settings.GetSettings("serviceId"));

            premiumId = Convert.ToInt32(Settings.GetSettings("premiumPackageId"));
            liteId = Convert.ToInt32(Settings.GetSettings("litePackageId"));
            movieId = Convert.ToInt32(Settings.GetSettings("moviePackageId"));

            AnonymousDefaultPackageId = Convert.ToInt32(Settings.GetSettings("AnonymousDefaultPackageId"));
            LoggedInDefaultPackageId = Convert.ToInt32(Settings.GetSettings("LoggedInDefaultPackageId"));

            GSapikey = Settings.GetSettings("GSapikey");
            GSsecretkey = Settings.GetSettings("GSsecretkey");

            Visible = 1;
            Hidden = 2;
            Deleted = 0;

            Entertainment = Convert.ToInt32(Settings.GetSettings("entertainmentCategoryId"));
            Movies = Convert.ToInt32(Settings.GetSettings("moviesCategoryId"));
            News = Convert.ToInt32(Settings.GetSettings("newsCategoryId"));
            Live = Convert.ToInt32(Settings.GetSettings("liveCategoryId"));
            //Specials = Convert.ToInt32(Settings.GetSettings("specialsCategoryId"));
            //Drama = Convert.ToInt32(Settings.GetSettings("dramasCategoryId"));
            //Sport = Convert.ToInt32(Settings.GetSettings("sportsCategoryId"));

            #region Home Featured IDS

            LatestFullEpisodes = Convert.ToInt32(Settings.GetSettings("LatestFullEpisodesFeatureId"));
            FreeTV = Convert.ToInt32(Settings.GetSettings("FreeTVFeatureId"));
            MostViewed = Convert.ToInt32(Settings.GetSettings("MostViewedFeatureId"));
            LatestShows = Convert.ToInt32(Settings.GetSettings("LatestShowsFeatureId"));
            FeaturedCelebrities = Convert.ToInt32(Settings.GetSettings("FeaturedCelebritiesFeatureId"));

            #endregion Home Featured IDS

            #region Movies Featured IDS

            LatestMovies = Convert.ToInt32(Settings.GetSettings("LatestMovieFeatureId"));
            StaffPicksMovies = Convert.ToInt32(Settings.GetSettings("StaffPicksFeatureId"));
            FeaturedPersonMovies = Convert.ToInt32(Settings.GetSettings("FeaturedPersonMovieFeatureId"));
            MostPopularMovies = Convert.ToInt32(Settings.GetSettings("MostPopularMovieseFeatureId"));

            #endregion Movies Featured IDS

            #region News Featured IDS

            FeaturedJournalists = Convert.ToInt32(Settings.GetSettings("FeaturedJournalistsFeatureId"));
            LatestNewsFullEpisodes = Convert.ToInt32(Settings.GetSettings("LatestNewsFullEpisodesFeatureId"));
            NewsFreeTV = Convert.ToInt32(Settings.GetSettings("NewsFreeTVFeatureId"));
            FeaturedRegionalNews = Convert.ToInt32(Settings.GetSettings("FeaturedRegionalNewsFeatureId"));
            FeaturedCurrentAffairs = Convert.ToInt32(Settings.GetSettings("FeaturedCurrentAffairs"));

            #endregion News Featured IDS

            #region Carousel IDS

            CarouselEntertainmentId = Convert.ToInt32(Settings.GetSettings("CarouselEntertainmentId"));
            CarouselMovieId = Convert.ToInt32(Settings.GetSettings("CarouselMovieId"));
            CarouselNewsId = Convert.ToInt32(Settings.GetSettings("CarouselNewsId"));

            #endregion Carousel IDS

            #region FreeTV IDS

            FreeTVMostViewedId = Convert.ToInt32(Settings.GetSettings("FreeTVMostViewedId"));
            FreeTVMostSharedId = Convert.ToInt32(Settings.GetSettings("FreeTVMostSharedId"));
            FreeTVTopLikedId = Convert.ToInt32(Settings.GetSettings("FreeTVTopLikedId"));
            FreeTVPlayListId = Convert.ToInt32(Settings.GetSettings("FreeTVPlayListId"));
            FreeTVComedyId = Convert.ToInt32(Settings.GetSettings("FreeTVComedyId"));
            FreeTVDramaId = Convert.ToInt32(Settings.GetSettings("FreeTVDramaId"));
            FreeTVSportsId = Convert.ToInt32(Settings.GetSettings("FreeTVSportsId"));

            #endregion FreeTV IDS

            SHA1Encryption = Settings.GetSettings("pinEncryption");

            PDTToken = Settings.GetSettings("PDTToken");
            PayPalSubmitUrl = Settings.GetSettings("PayPalSubmitUrl");
            PayPalBusiness = Settings.GetSettings("PayPalBusiness");
            PayPalReturnUrl = Settings.GetSettings("PayPalReturnUrl");
            PayPalReloadReturnUrl = Settings.GetSettings("PayPalReloadReturnUrl");

            AkamaiTokenKey = Settings.GetSettings("AkamaiTokenKey");

            DefaultCurrency = Settings.GetSettings("DefaultCurrency");
            DefaultCountry = Settings.GetSettings("DefaultCountry");
            TrialCurrency = Settings.GetSettings("TrialCurrency");

            GeoIpPath = Settings.GetSettings("GeoIpPath");

            CarouselImgPath = Settings.GetSettings("CarouselImgPath");
            ShowImgPath = Settings.GetSettings("ShowImgPath");
            EpisodeImgPath = Settings.GetSettings("EpisodeImgPath");
            CelebrityImgPath = Settings.GetSettings("CelebrityImgPath");

            BlankGif = Settings.GetSettings("BlankGif");

            baseUrl = Settings.GetSettings("baseUrl");

            FAQ = Convert.ToInt32(Settings.GetSettings("FAQId"));

            SOCIAL_COMMENT = Convert.ToInt32(Settings.GetSettings("SocialComment"));
            SOCIAL_RATING = Convert.ToInt32(Settings.GetSettings("SocialRating"));
            SOCIAL_SHARE = Convert.ToInt32(Settings.GetSettings("SocialShare"));
            SOCIAL_LIKE = Convert.ToInt32(Settings.GetSettings("SocialLike"));
            SOCIAL_LOVE = Convert.ToInt32(Settings.GetSettings("SocialLove"));

            maximumDistance = Convert.ToInt32(Settings.GetSettings("maximumDistance"));

            hlsPrefixPattern = Settings.GetSettings("hlsPrefixPattern");
            hlsSuffixPattern = Settings.GetSettings("hlsSuffixPattern");
            zeriPrefixPattern = Settings.GetSettings("zeriPrefixPattern");
            zeriSuffixPattern = Settings.GetSettings("zeriSuffixPattern");
            httpPrefixPatternMobile = Settings.GetSettings("httpPrefixPatternMobile");

            LitePackageId = Convert.ToInt32(Settings.GetSettings("LitePackageId"));
            PremiumPackageId = Convert.ToInt32(Settings.GetSettings("PremiumPackageId"));
            IpWhiteList = Settings.GetSettings("IpWhitelist");
            FaqTop5 = Settings.GetSettings("FAQTop5");
            LatestEpisodeDropdownListIds = Settings.GetSettings("LatestEpisodeDropdownListIds");
            BingSiteSearchCount = Settings.GetSettings("BingSiteSearchCount");
            ChatId = Settings.GetSettings("ChatId");
            GoogleSiteSearch = Convert.ToBoolean(Settings.GetSettings("useGoogleCustomSiteSearch"));
            BingSiteSearch = Convert.ToBoolean(Settings.GetSettings("useBingSiteSearch"));
            CustomSearchResultCount = Convert.ToInt32(Settings.GetSettings("CustomSearchResultCount"));
            CustomSearchCount = Convert.ToInt32(Settings.GetSettings("CustomSearchCount"));
            moviesPremiumCategoryId = Convert.ToInt32(Settings.GetSettings("moviesPremiumCategoryId"));

            FormsAuthenticationTimeout = Convert.ToInt32(Settings.GetSettings("FormsAuthenticationTimeout"));

            FreeTrial14ProductId = Convert.ToInt32(Settings.GetSettings("FreeTrial14ProductId"));
            FreeTrial7ProductId = Convert.ToInt32(Settings.GetSettings("FreeTrial7ProductId"));
            IsFreeTrialEnabled = Convert.ToBoolean(Settings.GetSettings("IsFreeTrialEnabled"));

            IsEWalletPaymentModeEnabled = Convert.ToBoolean(Settings.GetSettings("IsEWalletPaymentModeEnabled"));
            IsPpcPaymentModeEnabled = Convert.ToBoolean(Settings.GetSettings("IsPpcPaymentModeEnabled"));
            IsCreditCardPaymentModeEnabled = Convert.ToBoolean(Settings.GetSettings("IsCreditCardPaymentModeEnabled"));
            IsPaypalPaymentModeEnabled = Convert.ToBoolean(Settings.GetSettings("IsPaypalPaymentModeEnabled"));
            IsPpcReloadModeEnabled = Convert.ToBoolean(Settings.GetSettings("IsPpcReloadModeEnabled"));
            IsCreditCardReloadModeEnabled = Convert.ToBoolean(Settings.GetSettings("IsCreditCardReloadModeEnabled"));
            IsPaypalReloadModeEnabled = Convert.ToBoolean(Settings.GetSettings("IsPaypalReloadModeEnabled"));

            ServiceDeskUsername = Settings.GetSettings("ServiceDeskUsername");
            ServiceDeskPassword = Settings.GetSettings("ServiceDeskPassword");
            ServiceDeskDepartmentId = Convert.ToInt32(Settings.GetSettings("ServiceDeskDepartmentId"));
            FeedbackEmail = Settings.GetSettings("FeedbackEmail");

            SupportEmail = Settings.GetSettings("SupportEmail");
            SmtpHost = Settings.GetSettings("SmtpHost");
            SmtpPort = Convert.ToInt32(Settings.GetSettings("SmtpPort"));

            AllowedCountries = Settings.GetSettings("AllowedCountries");

            SendGridUsername = Settings.GetSettings("SendGridUsername");
            SendGridPassword = Settings.GetSettings("SendGridPassword");
            SendGridSmtpHost = Settings.GetSettings("SendGridSmtpHost");
            SendGridSmtpPort = Convert.ToInt32(Settings.GetSettings("SendGridSmtpPort"));

            RegistrationConfirmPage = Settings.GetSettings("RegistrationConfirmPage");
            IsCreateSupportTicketForLoggedInUsersEnabled = Convert.ToBoolean(Settings.GetSettings("IsCreateSupportTicketForLoggedInUsersEnabled"));
            NoReplyEmail = Settings.GetSettings("NoReplyEmail");
            LogEmail = Settings.GetSettings("LogEmail");

            IsProfilingEnabled = Convert.ToBoolean(Settings.GetSettings("IsProfilingEnabled"));

            TFCnowPackageIds = Settings.GetSettings("TFCnowPackageIds");

            TFCnowPremium = Settings.GetSettings("TFCnowPremium");
            TFCnowLite = Settings.GetSettings("TFCnowLite");
            TFCnowMovieChannel = Settings.GetSettings("TFCnowMovieChannel");

            TFCnowLiveStream = Settings.GetSettings("TFCnowLiveStream");
            TFCnowMoviePPV = Settings.GetSettings("TFCnowMoviePPV");

            JapanCountryCode = Settings.GetSettings("JapanCountryCode");
            FreeTrialProductIds = Settings.GetSettings("FreeTrialProductIds");

            EmailVerificationBody = Settings.GetSettings("EmailVerificationBody");
            EmailVerificationBodyTextOnly = Settings.GetSettings("EmailVerificationBodyTextOnly");

            snippetEnd = Convert.ToInt32(Settings.GetSettings("snippetEnd"));

            TFCnowBasePackageIds = Settings.GetSettings("TFCnowBasePackageIds");

            FreeTvCategoryId = Convert.ToInt32(Settings.GetSettings("FreeTvCategoryId"));

            ResetPasswordBodyTextOnly = Settings.GetSettings("ResetPasswordBodyTextOnly");

            SubscribeToProductBodyTextOnly = Settings.GetSettings("SubscribeToProductBodyTextOnly");
            ExtendSubscriptionBodyTextOnly = Settings.GetSettings("ExtendSubscriptionBodyTextOnly");
            GiftingSenderBodyTextOnly = Settings.GetSettings("GiftingSenderBodyTextOnly");
            GiftingRecipientBodyTextOnly = Settings.GetSettings("GiftingRecipientBodyTextOnly");

            PaypalReloadIPNUrl = Settings.GetSettings("PaypalReloadIPNUrl");
            PaypalBuyIPNUrl = Settings.GetSettings("PaypalBuyIPNUrl");
            PaypalDefaultIPNUrl = Settings.GetSettings("PaypalDefaultIPNUrl");

            IsCDNEnabled = Convert.ToBoolean(Settings.GetSettings("IsCDNEnabled"));

            HeatMapIpWhiteList = Settings.GetSettings("HeatMapIpWhiteList");
            HeatMapUrl = Settings.GetSettings("HeatMapUrl");

            OnlinePremierePreviewCategoryIds = Settings.GetSettings("OnlinePremierePreviewCategoryIds");
            OnlinePremiereFullCategoryIds = Settings.GetSettings("OnlinePremiereFullCategoryIds");

            CDNBaseUrl = Settings.GetSettings("CDNBaseUrl");
            AssetsBaseUrl = Settings.GetSettings("AssetsBaseUrl");
            IsAssetsEnabled = Convert.ToBoolean(Settings.GetSettings("IsAssetsEnabled"));

            ClickTayoMVEpisodeId = Convert.ToInt32(Settings.GetSettings("ClickTayoMVEpisodeId"));
            ClickTayoMVCategoryId = Convert.ToInt32(Settings.GetSettings("ClickTayoMVCategoryId"));
            OkGoChannelId = Convert.ToInt32(Settings.GetSettings("OkGoChannelId"));
            DigitalShortsCategoryId = Convert.ToInt32(Settings.GetSettings("DigitalShortsCategoryId"));
            OkGoCarouselSlideId = Convert.ToInt32(Settings.GetSettings("OkGoCarouselSlideId"));

            IsPDTEnabled = Convert.ToBoolean(Settings.GetSettings("IsPDTEnabled"));

            DBMGetUserUrl = Settings.GetSettings("DBMGetUserUrl");

            MultipleLoginRedirectedUrl = Settings.GetSettings("MultipleLoginRedirectedUrl");

            IsPreventionOfMultipleLoginEnabled = Convert.ToBoolean(Settings.GetSettings("IsPreventionOfMultipleLoginEnabled"));

            BCWMHChannelId = Convert.ToInt32(Settings.GetSettings("BCWMHChannelId"));

            LiveStreamRestrictedCountries = Settings.GetSettings("LiveStreamRestrictedCountries");
            LiveStreamSpecialChannelId = Convert.ToInt32(Settings.GetSettings("LiveStreamSpecialChannelId"));
            LiveStreamSpecialBannerImageUrl = Settings.GetSettings("LiveStreamSpecialBannerImageUrl");
            IsLiveStreamRestrictionCheckEnabled = Convert.ToBoolean(Settings.GetSettings("IsLiveStreamRestrictionCheckEnabled"));

            AkamaiAddSeconds = Convert.ToInt32(Settings.GetSettings("AkamaiAddSeconds"));

            RegistrationCompleteTemplateUrl = Settings.GetSettings("RegistrationCompleteTemplateUrl");
            RegistrationCompleteSubject = Settings.GetSettings("RegistrationCompleteSubject");

            IsCheckForEntitlementsForLiveSpecialsEnabled = Convert.ToBoolean(Settings.GetSettings("IsCheckForEntitlementsForLiveSpecialsEnabled"));
            LiveSpecialsPackageIdsRestriction = Settings.GetSettings("LiveSpecialsPackageIdsRestriction");
            LiveSpecialsCategoryIdsRestriction = Settings.GetSettings("LiveSpecialsCategoryIdsRestriction");

            CategoryIdsInCache = Settings.GetSettings("CategoryIdsInCache");

            MultipleLoginView = Settings.GetSettings("MultipleLoginView");

            SSLbaseUrl = Settings.GetSettings("SSLbaseUrl");
            IsSSLEnabled = Convert.ToBoolean(Settings.GetSettings("isSSLEnabled"));

            IsEffectiveMeasureEnabled = Convert.ToBoolean(Settings.GetSettings("IsEffectiveMeasureEnabled"));
            IsChatEnabled = Convert.ToBoolean(Settings.GetSettings("IsChatEnabled"));

            KwentoNgPaskoCategoryId = Convert.ToInt32(Settings.GetSettings("KwentoNgPaskoCategoryId"));
            KwentoNgPaskoBannerImageUrl = Settings.GetSettings("KwentoNgPaskoBannerImageUrl");

            FreeTrialPackageId1 = Convert.ToInt32(Settings.GetSettings("FreeTrialPackageId1"));
            FreeTrialPackageId2 = Convert.ToInt32(Settings.GetSettings("FreeTrialPackageId2"));
            FreeTrialPackageId3 = Convert.ToInt32(Settings.GetSettings("FreeTrialPackageId3"));
            FreeTrialPackageId4 = Convert.ToInt32(Settings.GetSettings("FreeTrialPackageId4"));
            FreeTrialPackageIds = Settings.GetSettings("FreeTrialPackageIds");
            IsXmasSkinEnabled = Convert.ToBoolean(Settings.GetSettings("IsXmasSkinEnabled"));
            DolphyCelebrityId = Convert.ToInt32(Settings.GetSettings("DolphyCelebrityId"));
            FreeTrialEarlyBirdProductId = Convert.ToInt32(Settings.GetSettings("FreeTrialEarlyBirdProductId"));
            FreeTrialProductIdsNEW = Settings.GetSettings("FreeTrialProductIdsNEW");
            FreeTrialAlaCarteSubscriptionTypes = Settings.GetSettings("FreeTrialAlaCarteSubscriptionTypes");
            TFCTVPackageIds = Settings.GetSettings("TFCTVPackageIds");

            hlsProgressivePrefixPattern = Settings.GetSettings("hlsProgressivePrefixPattern");
            zeriProgressivePrefixPattern = Settings.GetSettings("zeriProgressivePrefixPattern");
            zeriProgressiveSuffixPattern = Settings.GetSettings("zeriProgressiveSuffixPattern");
            httpProgressivePrefixPatternMobile = Settings.GetSettings("httpProgressivePrefixPatternMobile");

            AkamaiPMDAddSeconds = Convert.ToInt32(Settings.GetSettings("AkamaiPMDAddSeconds"));

            PMDSalt = Settings.GetSettings("PMDSalt");

            hlsProgressiveSuffixPattern = Settings.GetSettings("hlsProgressiveSuffixPattern");
            IsPMDPlayerEnabled = Convert.ToBoolean(Settings.GetSettings("IsPMDPlayerEnabled"));

            PMDHighBitrate = Settings.GetSettings("PMDHighBitrate");
            PMDLowBitrate = Settings.GetSettings("PMDLowBitrate");
            TFCTvDownloadPlayerUrl = Settings.GetSettings("TFCTvDownloadPlayerUrl");
            TFCTVPlayerDownloadSalt = Settings.GetSettings("TFCTVPlayerDownloadSalt");

            IsDownloadPlayerEnabled = Convert.ToBoolean(Settings.GetSettings("IsDownloadPlayerEnabled"));

            HimigHandogCategoryId = Convert.ToInt32(Settings.GetSettings("HimigHandogCategoryId"));
            IsThemedSkinEnabled = Convert.ToBoolean(Settings.GetSettings("IsThemedSkinEnabled"));

            JDCEpisodeId = Convert.ToInt32(Settings.GetSettings("JDCEpisodeId"));
            PMDHDBitrate = Settings.GetSettings("PMDHDBitrate");
            IsTVERegistrationEnabled = Convert.ToBoolean(Settings.GetSettings("IsTVERegistrationEnabled"));
            TVERegistrationPage = Settings.GetSettings("TVERegistrationPage");
            RegistrationCompleteTVE = Settings.GetSettings("RegistrationCompleteTVE");
            ConcertsCategoryId = Convert.ToInt32(Settings.GetSettings("ConcertsCategoryId"));
            IsEarlyBirdEnabled = Convert.ToBoolean(Settings.GetSettings("IsEarlyBirdEnabled"));
            RestrictedAdsCategoryIds = Settings.GetSettings("RestrictedAdsCategoryIds");
            TFCEverywhereParentCategoryId = Convert.ToInt32(Settings.GetSettings("TFCEverywhereParentCategoryId"));
            StreamingTVECategoryIds = Settings.GetSettings("StreamingTVECategoryIds");
            TVEStreamingChannelCounterpart = Settings.GetSettings("TVEStreamingChannelCounterpart");
            UseResponseRedirectOnPreventionOfMultipleLogin = Convert.ToBoolean(Settings.GetSettings("UseResponseRedirectOnPreventionOfMultipleLogin"));
            UseResponseBufferOutput = Convert.ToBoolean(Settings.GetSettings("UseResponseBufferOutput"));
            IsAdditionalCssEnabled = Convert.ToBoolean(Settings.GetSettings("IsAdditionalCssEnabled"));
            TFCkatCategoryId = Convert.ToInt32(Settings.GetSettings("TFCkatCategoryId"));
            TFCkatExclusivesCategoryId = Convert.ToInt32(Settings.GetSettings("TFCkatExclusivesCategoryId"));
            TVEMenuIds = Settings.GetSettings("TVEMenuIds");
            OnlinePremiereCategoryId = Convert.ToInt32(Settings.GetSettings("OnlinePremiereCategoryId"));

            hlsLiveStreamPrefixPattern = Settings.GetSettings("hlsLiveStreamPrefixPattern");
            hlsLiveStreamSuffixPattern = Settings.GetSettings("hlsLiveStreamSuffixPattern");
            zeriLiveStreamPrefixPattern = Settings.GetSettings("zeriLiveStreamPrefixPattern");
            zeriLiveStreamSuffixPattern = Settings.GetSettings("zeriLiveStreamSuffixPattern");
            httpLiveStreamPrefixPatternMobile = Settings.GetSettings("httpLiveStreamPrefixPatternMobile");
            PackageIdsWithChannelAccess = Settings.GetSettings("PackageIdsWithChannelAccess");

            HalalanChannelIds = Settings.GetSettings("HalalanChannelIds");
            HalalanParentCategoryId = Convert.ToInt32(Settings.GetSettings("HalalanParentCategoryId"));
            HalalanOrderedListIds = Settings.GetSettings("HalalanOrderedListIds");
            HalalanAdvisoriesParentCategoryId = Convert.ToInt32(Settings.GetSettings("HalalanAdvisoriesParentCategoryId"));
            HalalanNewsAlertsParentCategoryId = Convert.ToInt32(Settings.GetSettings("HalalanNewsAlertsParentCategoryId"));
            HalalanLatestEpisodesFeatureId = Convert.ToInt32(Settings.GetSettings("HalalanLatestEpisodesFeatureId"));
            HalalanNewsAlertsFeatureId = Convert.ToInt32(Settings.GetSettings("HalalanNewsAlertsFeatureId"));
            HalalanAdvisoriesFeatureId = Convert.ToInt32(Settings.GetSettings("HalalanAdvisoriesFeatureId"));
            ElmahAllowedEmails = Settings.GetSettings("ElmahAllowedEmails");
            IsRecurringBillingEnabled = Convert.ToBoolean(Settings.GetSettings("IsRecurringBillingEnabled"));
            isUAT = Convert.ToBoolean(Settings.GetSettings("isUAT"));
            TVECountryWhiteList = Settings.GetSettings("TVECountryWhiteList");
            ExtendSubscriptionBodyWithAutoRenewTextOnly = Settings.GetSettings("ExtendSubscriptionBodyWithAutoRenewTextOnly");
            SubscribeToProductBodyWithAutoRenewTextOnly = Settings.GetSettings("SubscribeToProductBodyWithAutoRenewTextOnly");
            FreeTrialStartDt = Convert.ToDateTime(Settings.GetSettings("FreeTrialStartDt"));
            IsProfileSiteMapEnabled = Convert.ToBoolean(Settings.GetSettings("IsProfileSiteMapEnabled"));
            TVEverywherePackageIds = Settings.GetSettings("TVEverywherePackageIds");
            CanPlayExcludedPackageIds = Settings.GetSettings("CanPlayExcludedPackageIds");
            CanPlayIncludedPackageIds = Settings.GetSettings("CanPlayIncludedPackageIds");
            KapamilyaChatLiveEventEpisodeId = Convert.ToInt32(Settings.GetSettings("KapamilyaChatLiveEventEpisodeId"));
            CoverItLiveAltCastCode = Settings.GetSettings("CoverItLiveAltCastCode");
            KapamilyaChatUserRegistrationDate = Convert.ToDateTime(Settings.GetSettings("KapamilyaChatUserRegistrationDate"));
            KapamilyaChatNumberOfWinners = Convert.ToInt32(Settings.GetSettings("KapamilyaChatNumberOfWinners"));
            KapamilyaChatRelatedVideosMaxCount = Convert.ToInt32(Settings.GetSettings("KapamilyaChatRelatedVideosMaxCount"));
            KapamilyaChatFeatureId = Convert.ToInt32(Settings.GetSettings("KapamilyaChatFeatureId"));
            IsLiveEventEntitlementCheckEnabled = Convert.ToBoolean(Settings.GetSettings("IsLiveEventEntitlementCheckEnabled"));
            IsMECreditCardEnabled = Convert.ToBoolean(Settings.GetSettings("IsMECreditCardEnabled"));
            MECountriesAllowedForCreditCard = Settings.GetSettings("MECountriesAllowedForCreditCard");
            MiddleEastGomsSubsidiaryId = Convert.ToInt32(Settings.GetSettings("MiddleEastGomsSubsidiaryId"));
            IsFlowHTML5PlayerEnabled = Convert.ToBoolean(Settings.GetSettings("IsFlowHTML5PlayerEnabled"));

            IsANCFreeStreamEnabled = Convert.ToBoolean(Settings.GetSettings("IsANCFreeStreamEnabled"));
            ANCChannelId = Convert.ToInt32(Settings.GetSettings("ANCChannelId"));
            ANCFreeStreamStartDt = Convert.ToDateTime(Settings.GetSettings("ANCFreeStreamStartDt"));
            ANCFreeStreamEndDt = Convert.ToDateTime(Settings.GetSettings("ANCFreeStreamEndDt"));
            ExcludedCountriesFromRegistrationDropDown = Settings.GetSettings("ExcludedCountriesFromRegistrationDropDown");
            SpecialElectionCoverageCategoryId = Convert.ToInt32(Settings.GetSettings("SpecialElectionCoverageCategoryId"));
            SpecialElectionCoverageEpisodeDisplayCount = Convert.ToInt32(Settings.GetSettings("SpecialElectionCoverageEpisodeDisplayCount"));
            FreeLiveEventEpisodeIds = Settings.GetSettings("FreeLiveEventEpisodeIds");
            CoverItLiveCdnId = Convert.ToInt32(Settings.GetSettings("CoverItLiveCdnId"));
            IsKapamilyaChatRedirectToLiveEnabled = Convert.ToBoolean(Settings.GetSettings("IsKapamilyaChatRedirectToLiveEnabled"));
            AkamaiIosTokenKey = Settings.GetSettings("AkamaiIosTokenKey");
            IsTVEIpCheckEnabled = Convert.ToBoolean(Settings.GetSettings("IsTVEIpCheckEnabled"));
            IsIosHLSCdnEnabled = Convert.ToBoolean(Settings.GetSettings("IsIosHLSCdnEnabled"));
            ExcludedCategoryIdsForDisplay = Settings.GetSettings("ExcludedCategoryIdsForDisplay");
            UseDaysBasedOnCacheDurationForSessionStore = Convert.ToBoolean(Settings.GetSettings("UseDaysBasedOnCacheDurationForSessionStore"));
            WhitelistedLiveStreamEpisodeIdFromMobileCheck = Settings.GetSettings("WhitelistedLiveStreamEpisodeIdFromMobileCheck");
            SessionStoreCacheDurationInDays = Convert.ToInt32(Settings.GetSettings("SessionStoreCacheDurationInDays"));
            DZMMTVECategoryChannelId = Convert.ToInt32(Settings.GetSettings("DZMMTVECategoryChannelId"));
            AkamaiBeaconIos = Settings.GetSettings("AkamaiBeaconIos");
            AkamaiBeaconAdobeFlash = Settings.GetSettings("AkamaiBeaconAdobeFlash");
            csmaPluginPath = Settings.GetSettings("csmaPluginPath");
            TVEStreamingEpisodeCounterpart = Settings.GetSettings("TVEStreamingEpisodeCounterpart");
            paymentTransactionMaximumThreshold = Convert.ToInt32(Settings.GetSettings("paymentTransactionMaximumThreshold"));
            reloadTransactionMaximumThreshold = Convert.ToInt32(Settings.GetSettings("reloadTransactionMaximumThreshold"));
            UseProgressiveViaAdaptiveTechnology = Convert.ToBoolean(Settings.GetSettings("UseProgressiveViaAdaptiveTechnology"));

            PMDViaAdaptiveHighBitrate = Settings.GetSettings("PMDViaAdaptiveHighBitrate");
            PMDViaAdaptiveLowBitrate = Settings.GetSettings("PMDViaAdaptiveLowBitrate");
            PMDViaAdaptiveHDBitrate = Settings.GetSettings("PMDViaAdaptiveHDBitrate");
            CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology = Convert.ToBoolean(Settings.GetSettings("CheckForEpisodesToMakeUseOfProgessiveViaAdaptiveTechnology"));
            EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology = Settings.GetSettings("EpisodeIdsToMakeUseOfProgressiveViaAdaptiveTechnology");
            UAAPTeamsCelebrityIDs = Settings.GetSettings("UAAPTeamsCelebrityIDs");
            UAAPTeamsImgPath = Settings.GetSettings("UAAPTeamsImgPath");
            UAAPGreatnessNeverEndsCategoryId = Convert.ToInt32(Settings.GetSettings("UAAPGreatnessNeverEndsCategoryId"));
            UAAPLiveStreamEpisodeId = Convert.ToInt32(Settings.GetSettings("UAAPLiveStreamEpisodeId"));
            UAAPGamesParentId = Convert.ToInt32(Settings.GetSettings("UAAPGamesParentId"));
            UAAPExclusiveFeaturesId = Convert.ToInt32(Settings.GetSettings("UAAPExclusiveFeaturesId"));
            UAAPEntitlementPackageID = Settings.GetSettings("UAAPEntitlementPackageID");
            UAAPTeamsFeatureId = Convert.ToInt32(Settings.GetSettings("UAAPTeamsFeatureId"));
            UseGomsSubsidiaryForTVECheck = Convert.ToBoolean(Settings.GetSettings("UseGomsSubsidiaryForTVECheck"));
            IsSynapseEnabled = Convert.ToBoolean(Settings.GetSettings("IsSynapseEnabled"));
            PackageAndProductCacheDuration = Convert.ToInt32(Settings.GetSettings("PackageAndProductCacheDuration"));
            UAAPCoverItLiveAltCastCode = Settings.GetSettings("UAAPCoverItLiveAltCastCode");
            IsUserEntitlementViewOnPageEnabled = Convert.ToBoolean(Settings.GetSettings("IsUserEntitlementViewOnPageEnabled"));
            UsePayPalIPNLog = Convert.ToBoolean(Settings.GetSettings("UsePayPalIPNLog"));
            MenuCacheDuration = Convert.ToInt32(Settings.GetSettings("MenuCacheDuration"));
            UseCountryListInMemory = Convert.ToBoolean(Settings.GetSettings("UseCountryListInMemory"));
            MenuMovieBlurbLength = Convert.ToInt32(Settings.GetSettings("MenuMovieBlurbLength"));
            MenuShowBlurbLength = Convert.ToInt32(Settings.GetSettings("MenuShowBlurbLength"));
            LiveEventEpisodesWhereVotingIsEnabled = Settings.GetSettings("LiveEventEpisodesWhereVotingIsEnabled");
            IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled = Convert.ToBoolean(Settings.GetSettings("IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled"));
            WelcomePageEpisodeID = Settings.GetSettings("WelcomePageEpisodeID");
            SynapseGenericCacheDuration = Convert.ToInt32(Settings.GetSettings("SynapseGenericCacheDuration"));
            PackagesInProductGroup = Settings.GetSettings("PackagesInProductGroup");
            UrlTokenSalt = Settings.GetSettings("UrlTokenSalt");
            UrlTokenPassword = Settings.GetSettings("UrlTokenPassword");
            IsSmartPitReloadEnabled = Convert.ToBoolean(Settings.GetSettings("IsSmartPitReloadEnabled"));
            AlaCarteBlurb = Settings.GetSettings("AlaCarteBlurb");
            EventCelebritiesFeatureID = Convert.ToInt32(Settings.GetSettings("EventCelebritiesFeatureID"));
            TFCkatShowExclusives = Settings.GetSettings("TFCkatShowExclusives");
            TFCkatAltCastCode = Settings.GetSettings("TFCkatAltCastCode");
            UseServiceOfferingWhenCheckingEpisodeParentCategory = Convert.ToBoolean(Settings.GetSettings("UseServiceOfferingWhenCheckingEpisodeParentCategory"));
            GetParentCategoriesCacheDuration = Convert.ToInt32(Settings.GetSettings("GetParentCategoriesCacheDuration"));
            GetParentShowsForEpisodeCacheDuration = Convert.ToInt32(Settings.GetSettings("GetParentShowsForEpisodeCacheDuration"));
            GetAllSubCategoryIdsCacheDuration = Convert.ToInt32(Settings.GetSettings("GetAllSubCategoryIdsCacheDuration"));
            GigyaPromoChallengeID = Settings.GetSettings("GigyaPromoChallengeID");
            TFCtvFirstYearAnniversaryPromoId = Convert.ToInt32(Settings.GetSettings("TFCtvFirstYearAnniversaryPromoId"));
            IsWelcomePageOn = Convert.ToBoolean(Settings.GetSettings("IsWelcomePageOn"));
            EnableNotifyLoginOnLinkAccount = Convert.ToBoolean(Settings.GetSettings("EnableNotifyLoginOnLinkAccount"));
            PromoRunDateString = Settings.GetSettings("PromoRunDateString");
            SocialProvidersList = Settings.GetSettings("SocialProvidersList");
            IsForceConnectToSNSEnabled = Convert.ToBoolean(Settings.GetSettings("IsForceConnectToSNSEnabled"));
            BCWMHWeddingLiveEventEpisodeId = Convert.ToInt32(Settings.GetSettings("BCWMHWeddingLiveEventEpisodeId"));
            BCWMHWeddingSpecialsFeatureId = Convert.ToInt32(Settings.GetSettings("BCWMHWeddingSpecialsFeatureId"));
            BCWMHWeddingExclusivesFeatureId = Convert.ToInt32(Settings.GetSettings("BCWMHWeddingExclusivesFeatureId"));
            BCWMHWeddingCategoryId = Convert.ToInt32(Settings.GetSettings("BCWMHWeddingCategoryId"));
            BCWMHCoverItLiveAltCastCode = Settings.GetSettings("BCWMHCoverItLiveAltCastCode");
            BalitangAmericaSagipKapamilyaEpisodeId = Convert.ToInt32(Settings.GetSettings("BalitangAmericaSagipKapamilyaEpisodeId"));
            BalitangAmericaSagipKapamilyaSlug = Settings.GetSettings("BalitangAmericaSagipKapamilyaSlug");
            AlertBoxURL = Settings.GetSettings("AlertBoxURL");
            IsAlertBoxEnabled = Convert.ToBoolean(Settings.GetSettings("IsAlertBoxEnabled"));
            AlertBoxEnabledUrlList = Settings.GetSettings("AlertBoxEnabledUrlList");
            TulongPHConcertEpisodeId = Convert.ToInt32(Settings.GetSettings("TulongPHConcertEpisodeId"));
            TulongPHConcertSlug = Settings.GetSettings("TulongPHConcertSlug");
            IsTFCkatVotingEnabled = Convert.ToBoolean(Settings.GetSettings("IsTFCkatVotingEnabled"));
            IsTFCtvFanMobPromoEnabled = Convert.ToBoolean(Settings.GetSettings("IsTFCtvFanMobPromoEnabled"));
            KwentoNgPaskoSpecialsFeatureId = Convert.ToInt32(Settings.GetSettings("KwentoNgPaskoSpecialsFeatureId"));
            KwentoNgPaskoExclusivesFeatureId = Convert.ToInt32(Settings.GetSettings("KwentoNgPaskoExclusivesFeatureId"));
            MopayFormPostAddress = Settings.GetSettings("MopayFormPostAddress");
            IsMopayReloadModeEnabled = Convert.ToBoolean(Settings.GetSettings("IsMopayReloadModeEnabled"));
            MopayCountryWhitelist = Settings.GetSettings("MopayCountryWhitelist");
            MopayLoadSaltCode = Settings.GetSettings("MopayLoadSaltCode");
            TfcTvFree2StartDate = Convert.ToDateTime(Settings.GetSettings("TfcTvFree2StartDate"));
            TfcTvFree2EndDate = Convert.ToDateTime(Settings.GetSettings("TfcTvFree2EndDate"));
            TfcTvFree2ProductId = Convert.ToInt32(Settings.GetSettings("TfcTvFree2ProductId"));
            TfcTvFree2CountryWhiteList = Settings.GetSettings("TfcTvFree2CountryWhiteList");
            TfcTvFree1StartDate = Convert.ToDateTime(Settings.GetSettings("TfcTvFree1StartDate"));
            TfcTvFree1EndDate = Convert.ToDateTime(Settings.GetSettings("TfcTvFree1EndDate"));
            AlertBoxForItalyTaiwanUrl = Settings.GetSettings("AlertBoxForItalyTaiwanUrl");
            AlertBoxForGermanyUrl = Settings.GetSettings("AlertBoxForGermanyUrl");
            KapamilyaLoveChatEpisodeId = Convert.ToInt32(Settings.GetSettings("KapamilyaLoveChatEpisodeId"));
            KapamilyaLoveChatSlug = Settings.GetSettings("KapamilyaLoveChatSlug");
            TFCkatGrandWinners = Convert.ToInt32(Settings.GetSettings("TFCkatGrandWinners"));
            ChristmasSpecialEpisodeId = Convert.ToInt32(Settings.GetSettings("ChristmasSpecialEpisodeId"));
            ChristmasSpecialSlug = Settings.GetSettings("ChristmasSpecialSlug");

            PaypalAPIUser = Settings.GetSettings("PaypalAPIUser");
            PaypalAPIPassword = Settings.GetSettings("PaypalAPIPassword");
            PaypalAPISignature = Settings.GetSettings("PaypalAPISignature");
            PaypalNVPUrl = Settings.GetSettings("PaypalNVPUrl");
            PaypalAPIVersion = Settings.GetSettings("PaypalAPIVersion");
            PaypalAPIMethod = Settings.GetSettings("PaypalAPIMethod");
            PaypalAPIAction = Settings.GetSettings("PaypalAPIAction");
            ProductIdsExcludedFromRecurringBilling = Settings.GetSettings("ProductIdsExcludedFromRecurringBilling");
            IsPayPalRecurringBillingEnabled = Convert.ToBoolean(Settings.GetSettings("IsPayPalRecurringBillingEnabled"));
            ProductIdsIncludedInRecurringBilling = Settings.GetSettings("ProductIdsIncludedInRecurringBilling");
            IsTFCkatMEVotingDisableDate = Convert.ToDateTime(Settings.GetSettings("IsTFCkatMEVotingDisableDate"));
            ASAPDubai2014EpisodeId = Convert.ToInt32(Settings.GetSettings("ASAPDubai2014EpisodeId"));
            ASAPDubai2014Slug = Settings.GetSettings("ASAPDubai2014Slug");
            FPJThematicBundleFeatureIds = Settings.GetSettings("FPJThematicBundleFeatureIds");
            FPJCategoryId = Convert.ToInt32(Settings.GetSettings("FPJCategoryId"));
            FPJProductId = Convert.ToInt32(Settings.GetSettings("FPJProductId"));
            FPJShowIDForSubscribeButton = Convert.ToInt32(Settings.GetSettings("FPJShowIDForSubscribeButton"));
            NumberOfItemsInEpisodeList = Convert.ToInt32(Settings.GetSettings("NumberOfItemsInEpisodeList"));
            ABSCBNFreeLiveStreamEpisodeId = Convert.ToInt32(Settings.GetSettings("ABSCBNFreeLiveStreamEpisodeId"));
            IsABSCBNFreeLiveStreamFreeOnRegistrationEnabled = Convert.ToBoolean(Settings.GetSettings("IsABSCBNFreeLiveStreamFreeOnRegistrationEnabled"));
            ABSCBNFreeLiveStreamStartDate = Convert.ToDateTime(Settings.GetSettings("ABSCBNFreeLiveStreamStartDate"));
            ABSCBNFreeLiveStreamEndDate = Convert.ToDateTime(Settings.GetSettings("ABSCBNFreeLiveStreamEndDate"));
            ABSCBNFreeLiveStreamProductId = Convert.ToInt32(Settings.GetSettings("ABSCBNFreeLiveStreamProductId"));
            ABSCBNFreeLiveStreamPackageId = Convert.ToInt32(Settings.GetSettings("ABSCBNFreeLiveStreamPackageId"));
            EmailsAllowedToBypassIPBlock = Settings.GetSettings("EmailsAllowedToBypassIPBlock");
            ABSCBNFreeLiveStreamSlug = Settings.GetSettings("ABSCBNFreeLiveStreamSlug");
            MainMenuItems = Settings.GetSettings("MainMenuItems");
            IsTVERegionBlockingEnabled = Convert.ToBoolean(Settings.GetSettings("IsTVERegionBlockingEnabled"));
            NumberOfItemsInEpisodeListInEpisodePage = Convert.ToInt32(Settings.GetSettings("NumberOfItemsInEpisodeListInEpisodePage"));
            MopayBackURL = Settings.GetSettings("MopayBackURL");
            PBBLiveEventEpisodeId = Convert.ToInt32(Settings.GetSettings("PBBLiveEventEpisodeId"));
            BayaningFilipinoEpisodeId = Convert.ToInt32(Settings.GetSettings("BayaningFilipinoEpisodeId"));
            BayaningFilipinoSlug = Settings.GetSettings("BayaningFilipinoSlug");
            PBBLiveStreamFeatureId = Convert.ToInt32(Settings.GetSettings("PBBLiveStreamFeatureId"));
            PBBEpisodesFeatureId = Convert.ToInt32(Settings.GetSettings("PBBEpisodesFeatureId"));
            PBBUberFeatureId = Convert.ToInt32(Settings.GetSettings("PBBUberFeatureId"));
            PBBCarouselSlideIds = Settings.GetSettings("PBBCarouselSlideIds");
            PBBBlockedCountryCodes = Settings.GetSettings("PBBBlockedCountryCodes");
            TFC20DefaultEpisodeId = Convert.ToInt32(Settings.GetSettings("TFC20DefaultEpisodeId"));
            TFC20BayaningFilipinoCategoryId = Convert.ToInt32(Settings.GetSettings("TFC20BayaningFilipinoCategoryId"));
            TFC20CoverItLiveAltCastCode = Settings.GetSettings("TFC20CoverItLiveAltCastCode");
            IsMopayListenerEnabled = Convert.ToBoolean(Settings.GetSettings("IsMopayListenerEnabled"));
            IsMobilePreviewEnabled = Convert.ToBoolean(Settings.GetSettings("IsMobilePreviewEnabled"));
            FeatureItemsPageSize = Convert.ToInt32(Settings.GetSettings("FeatureItemsPageSize"));
            GenericListContentSize = Convert.ToInt32(Settings.GetSettings("GenericListContentSize"));
            FPJParentCategoryId = Convert.ToInt32(Settings.GetSettings("FPJParentCategoryId"));
            ShowsCategoryIds = Settings.GetSettings("ShowsCategoryIds");
            NewsCategoryIds = Settings.GetSettings("NewsCategoryIds");
            MoviesCategoryIds = Settings.GetSettings("MoviesCategoryIds");
            LiveCategoryIds = Settings.GetSettings("LiveCategoryIds");
            UXHomePageCarouselId = Convert.ToInt32(Settings.GetSettings("UXHomePageCarouselId"));
            PremiumProductIds = Settings.GetSettings("PremiumProductIds");
            LiteProductIds = Settings.GetSettings("LiteProductIds");
            MovieProductIds = Settings.GetSettings("MovieProductIds");
            MenuExCategory = Settings.GetSettings("MenuExCategory");
            UXAlaCarteParentCategoryIds = Settings.GetSettings("UXAlaCarteParentCategoryIds");
            GigyaSealKey = Settings.GetSettings("GigyaSealKey");
            DefaultProductIdOffering = Convert.ToInt32(Settings.GetSettings("DefaultProductIdOffering"));
            UXAnonymousHomepageCarouselId = Convert.ToInt32(Settings.GetSettings("UXAnonymousHomepageCarouselId"));
            UXReviewParentCategoryId = Convert.ToInt32(Settings.GetSettings("UXReviewParentCategoryId"));
            BCWMHThanksgivingConcertSlideIds = Settings.GetSettings("BCWMHThanksgivingConcertSlideIds");
            BCWMHThanksgivingConcertBlockedCountryCodes = Settings.GetSettings("BCWMHThanksgivingConcertBlockedCountryCodes");
            IsHDPlaybackEnabled = Convert.ToBoolean(Settings.GetSettings("IsHDPlaybackEnabled"));
            SpecialsCurrentCategoryId = Convert.ToInt32(Settings.GetSettings("SpecialsCurrentCategoryId"));
            PremiumTrialPromoId = Convert.ToInt32(Settings.GetSettings("PremiumTrialPromoId"));
            PremiumTrialPromoProductId = Convert.ToInt32(Settings.GetSettings("PremiumTrialPromoProductId"));
            LitePromoProductIds = Settings.GetSettings("LitePromoProductIds");
            LiteChurnersPromoId = Convert.ToInt32(Settings.GetSettings("LiteChurnersPromoId"));
            TFCtvApiVideoPlaybackUri = Settings.GetSettings("TFCtvApiVideoPlaybackUri");
            Html5CapableDevicesRegex = Settings.GetSettings("Html5CapableDevicesRegex");
            AdultContentCategoryId = Convert.ToInt32(Settings.GetSettings("AdultContentCategoryId"));
            FeaturedCelebSlug = Settings.GetSettings("FeaturedCelebSlug");
            ProjectAirEpisodeId = Convert.ToInt32(Settings.GetSettings("ProjectAirEpisodeId"));
            ProjectAirCategoryId = Convert.ToInt32(Settings.GetSettings("ProjectAirCategoryId"));
            ProjectAirDisplayCategoryId = Convert.ToInt32(Settings.GetSettings("ProjectAirDisplayCategoryId"));
            ProjectAirPromoId = Convert.ToInt32(Settings.GetSettings("ProjectAirPromoId"));
            ProjectAirEmailVerificationBodyTemplateUrl = Settings.GetSettings("ProjectAirEmailVerificationBodyTemplateUrl");
            ProjectAirProgramScheduleChannelIds = Settings.GetSettings("ProjectAirProgramScheduleChannelIds");
            USAllowedCarouselIds = Settings.GetSettings("USAllowedCarouselIds");
            Promo2014ProductIds = Settings.GetSettings("Promo2014ProductIds");
            Promo201410PromoId = Convert.ToInt32(Settings.GetSettings("Promo201410PromoId"));
            AirRegistrationCompleteTemplateUrl = Settings.GetSettings("AirRegistrationCompleteTemplateUrl");
            premiumProductGroupIds = Settings.GetSettings("premiumProductGroupIds");
            Compensation2014PromoId = Convert.ToInt32(Settings.GetSettings("Compensation2014PromoId"));
            SubscriptionProductIds = Settings.GetSettings("SubscriptionProductIds");
            SubscriptionProductIds1Month = Settings.GetSettings("SubscriptionProductIds1Month");
            SubscriptionProductIds3Month = Settings.GetSettings("SubscriptionProductIds3Month");
            SubscriptionProductIds12Month = Settings.GetSettings("SubscriptionProductIds12Month");
            Compensation201411ProductId1Month = Convert.ToInt32(Settings.GetSettings("Compensation201411ProductId1Month"));
            Compensation201411ProductId3and12Month = Convert.ToInt32(Settings.GetSettings("Compensation201411ProductId3and12Month"));
            Compensation201411CategoryId1Month = Convert.ToInt32(Settings.GetSettings("Compensation201411CategoryId1Month"));
            Compensation201411CategoryId3and12Month = Convert.ToInt32(Settings.GetSettings("Compensation201411CategoryId3and12Month"));
            Compensatory3DaysProductId = Convert.ToInt32(Settings.GetSettings("Compensatory3DaysProductId"));
            Compensatory7DaysProductId = Convert.ToInt32(Settings.GetSettings("Compensatory7DaysProductId"));
            PreBlackPromoProductIds = Settings.GetSettings("PreBlackPromoProductIds");
            PreBlackPromoId = Convert.ToInt32(Settings.GetSettings("PreBlackPromoId"));
            CAAllowedCarouselIds = Settings.GetSettings("CAAllowedCarouselIds");
            EdsaWoolworthPromoId = Convert.ToInt32(Settings.GetSettings("EdsaWoolworthPromoId"));
            ProjectBlackPromoIds = Settings.GetSettings("ProjectBlackPromoIds");
            ProjectBlackProductIds = Settings.GetSettings("ProjectBlackProductIds");
            PremiumComplimentary1Month = Convert.ToInt32(Settings.GetSettings("PremiumComplimentary1Month"));
            SurveyPromoId = Convert.ToInt32(Settings.GetSettings("SurveyPromoId"));
            KapitBisigEpisodeId = Convert.ToInt32(Settings.GetSettings("KapitBisigEpisodeId"));
            KapitBisigSlug = Settings.GetSettings("KapitBisigSlug");
            Xoom2PromoId = Convert.ToInt32(Settings.GetSettings("Xoom2PromoId"));
            Xoom2FreeProductId = Convert.ToInt32(Settings.GetSettings("Xoom2FreeProductId"));
            UseJWPlayer = Convert.ToBoolean(Settings.GetSettings("UseJWPlayer"));
            TFCtvMobileAirEpisodeId = Convert.ToInt32(Settings.GetSettings("TFCtvMobileAirEpisodeId"));
            ITEValidateURL = Settings.GetSettings("ITEValidateURL");
            ITEWhitelistedIp = Settings.GetSettings("ITEWhitelistedIp");
            ITEPromoId = Convert.ToInt32(Settings.GetSettings("ITEPromoId"));
            AkamaiIosTokenKey2 = Settings.GetSettings("AkamaiIosTokenKey2");
            LiveEpisodeIdsToUseIosTokenKey2 = Settings.GetSettings("LiveEpisodeIdsToUseIosTokenKey2");
            PinoyPride30EpisodeId = Convert.ToInt32(Settings.GetSettings("PinoyPride30EpisodeId"));
            PinoyPride30Slug = Settings.GetSettings("PinoyPride30Slug");
            PinoyPride30TakeoverStartDt = Convert.ToDateTime(Settings.GetSettings("PinoyPride30TakeoverStartDt"));
            PinoyPride30TakeoverEndDt = Convert.ToDateTime(Settings.GetSettings("PinoyPride30TakeoverEndDt"));
            PinoyPride30ProductId = Convert.ToInt32(Settings.GetSettings("PinoyPride30ProductId"));
            PacMayVODEpisodeId = Convert.ToInt32(Settings.GetSettings("PacMayVODEpisodeId"));
            PacMayLiveEpisodeId = Convert.ToInt32(Settings.GetSettings("PacMayLiveEpisodeId"));
            PacMaySpecialsFeatureId = Convert.ToInt32(Settings.GetSettings("PacMaySpecialsFeatureId"));
            PacMayLiveConcertShowId = Convert.ToInt32(Settings.GetSettings("PacMayLiveConcertShowId"));
            PacMayLatestNewsShowId = Convert.ToInt32(Settings.GetSettings("PacMayLatestNewsShowId"));
            PacMayVODShowId = Convert.ToInt32(Settings.GetSettings("PacMayVODShowId"));
            PacMayTrendingVideosCategoryId = Convert.ToInt32(Settings.GetSettings("PacMayTrendingVideosCategoryId"));
            PacMayPlugOnTrendingVideosEpisodeId = Convert.ToInt32(Settings.GetSettings("PacMayPlugOnTrendingVideosEpisodeId"));
            PacMaySubscribeCategoryId = Convert.ToInt32(Settings.GetSettings("PacMaySubscribeCategoryId"));
            PacMaySubscribeProductIds = Settings.GetSettings("PacMaySubscribeProductIds");
            MEPacMayCarouselIds = Settings.GetSettings("MEPacMayCarouselIds");
            MEPacMayAllowedCountryCodes = Settings.GetSettings("MEPacMayAllowedCountryCodes");
            NonMEPacMayCarouselIds = Settings.GetSettings("NonMEPacMayCarouselIds");
            PacMayLiveCategoryId = Convert.ToInt32(Settings.GetSettings("PacMayLiveCategoryId"));
            PacMayTVCEpisodeId = Convert.ToInt32(Settings.GetSettings("PacMayTVCEpisodeId"));
            PacMayProductIdsForRedirect = Settings.GetSettings("PacMayProductIdsForRedirect");
            IsStaticPacMayForMECarouselEnabled = Convert.ToBoolean(Settings.GetSettings("IsStaticPacMayForMECarouselEnabled"));
            Q22015PromoId = Convert.ToInt32(Settings.GetSettings("Q22015PromoId"));
            Q22015ProductId = Settings.GetSettings("Q22015ProductId");
            ListOfPromoIdsForEvents = Settings.GetSettings("ListOfPromoIdsForEvents");
            PBB747Cam1EpisodeId = Convert.ToInt32(Settings.GetSettings("PBB747Cam1EpisodeId"));
            PBB747Cam2EpisodeId = Convert.ToInt32(Settings.GetSettings("PBB747Cam2EpisodeId"));
            PBBHelpFragment = Settings.GetSettings("PBBHelpFragment");
            VerificationTemplateUrl = Settings.GetSettings("VerificationTemplateUrl");
            WelcomeTemplateUrl = Settings.GetSettings("WelcomeTemplateUrl");
            ReceiptTemplateUrl = Settings.GetSettings("ReceiptTemplateUrl");
            DaxAllowedParentCategories = Settings.GetSettings("DaxAllowedParentCategories");
            SiteUmbrellaCategoryIds = Settings.GetSettings("SiteUmbrellaCategoryIds");
            Asap20InLondonEpisodeId = Convert.ToInt32(Settings.GetSettings("Asap20InLondonEpisodeId"));
            Asap20InLondonAllowedDomains = Settings.GetSettings("Asap20InLondonAllowedDomains");
            BackgroundTakeoverAllowedCountries = Settings.GetSettings("BackgroundTakeoverAllowedCountries");
            AkamaiIosTokenKey3 = Settings.GetSettings("AkamaiIosTokenKey3");
            EpisodeIdsToUseIosTokenKey3 = Settings.GetSettings("EpisodeIdsToUseIosTokenKey3");
            TwitterUriCdnId = Convert.ToInt32(Settings.GetSettings("TwitterUriCdnId"));
            TwitterWidgetCdnId = Convert.ToInt32(Settings.GetSettings("TwitterWidgetCdnId"));
            StreamSenseEnabled = Convert.ToBoolean(Settings.GetSettings("StreamSenseEnabled"));
            UAAPMainCategoryId = Convert.ToInt32(Settings.GetSettings("UAAPMainCategoryId"));
            hlsProtocol = Settings.GetSettings("hlsProtocol");
            httpProtocol = Settings.GetSettings("httpProtocol");
            hdsFolder = Settings.GetSettings("hdsFolder");
            hlsFolder = Settings.GetSettings("hlsFolder");
            NetsuiteMaintenancePromoId = Convert.ToInt32(Settings.GetSettings("NetsuiteMaintenancePromoId"));
            TfctvRecommendedItemsApi = Settings.GetSettings("TfctvRecommendedItemsApi");
            Country14DayTrials = Settings.GetSettings("Country14DayTrials");
            AirPlusPackageId = Convert.ToInt32(Settings.GetSettings("AirPlusPackageId"));
            TFCtvPaidMobileAirEpisodeId = Convert.ToInt32(Settings.GetSettings("TFCtvPaidMobileAirEpisodeId"));
            ListOfAirPlusPackageIds = Settings.GetSettings("ListOfAirPlusPackageIds");
            AirPlusAdTypeIds = Settings.GetSettings("AirPlusAdTypeIds");
        }
    }

    public static class Settings
    {
        private static bool isAzure = false;

        static Settings()
        {
            isAzure = RoleEnvironment.IsAvailable;
        }

        public static string GetSettings(string settingName)
        {
            string setting = "";
            try
            {
                if (isAzure)
                    setting = RoleEnvironment.GetConfigurationSettingValue(settingName);
                else

                    setting = ConfigurationManager.AppSettings[settingName];
            }
            catch (Exception)
            {
                setting = "";
            }

            return (setting);
        }
    }

    public enum SubscriptionProductType
    {
        Episode = 1,
        Show = 2,
        Package = 3
    }

    public enum ErrorCodes
    {
        Success = 0,
        InsufficientWalletLoad = -300,
        EntityUpdateError = -301,
        UnknownError = -901,
        NotAuthenticated = -400,
        IsAlreadyAuthenticated = -401,
        UserIsNotEntitled = 402,
        IsInvalidPpc = -500,
        IsReloadPpc = -501,
        IsSubscriptionPpc = -502,
        IsProductIdInvalidPpc = -503,
        IsInvalidCombinationPpc = -504,
        IsUsedPpc = -505,
        IsExpiredPpc = -506,
        IsNotValidInCountryPpc = -507,
        IsNotValidAmountPpc = -508,
        HasNoWallet = -509,
        WallePpcCurrencyConflict = -510,
        IsProcessedPayPalTransaction = -600,
        IsExistingEmail = -200,
        IsEmailEmpty = -201,
        IsMismatchPassword = -202,
        UserDoesNotExist = -203,
        IsWrongPassword = -204,
        FailedToLinkAccount = -205,
        IsMissingRequiredFields = -206,
        IsInvalidRequest = -207,
        IsNewSiteUser = -208,
        IsNotValidEmail = -209,
        IsNotVerified = -210,
        VideoNotFound = -700,
        AkamaiCdnNotFound = -701,
        PremiumAssetNotFound = -702,
        EpisodeNotFound = -703,
        IPodPreviewNotAvailable = -704,
        WishlistNotFound = -900,
        WishlistItemExists = -901,
        CreditCardHasExpired = -1000,
        IsNotAllowedToBuyLite = -2000,
        IsNotAllowedToBuyPremium = -2001,
        ProductIsNull = -2002,
        PackageIsNotAllowedInCountry = -2003,
        HasPendingChangeCountryTransaction = -2004,
        ProductIsNotPurchaseable = -2005,
        ProductIsNotAllowedInCountry = -2006,
        IsElapsedExpiryDate = -3000,
        LimitReached = -211,
        IsNotAvailableOnMobileDevices = -705,
        MultipleLoginDetected = -4000,
        MaximumTransactionsExceeded = -800,
        IsCurrentlyEnrolledInRecurringBilling = -801,
        IsAlreadyEnrolledToSmartPit = -802,
        UnauthorizedCountry = -803
    }

    public enum MenuShow
    {
        Show = 0,
        Channel = 1
    }

    public enum MailType
    {
        TextOnly = 0,
        HtmlOnly = 1,
        Both = 2
    }

    public enum ContentSource
    {

        Site = 0,
        CDN = 1,
        Assets = 2
    }

    public enum EngagementContentType
    {

        Show = 1,
        Episode = 2,
        Celebrity = 3,
        Channel = 4,
        Youtube = 5
    }

    public enum Progressive
    {
        High = 1,
        Low = 2
    }

    public enum ProductSubscriptionType
    {
        Package = 1,
        Show = 2,
        Episode = 3
    }

    public enum PaymentError
    {
        PENDING_GOMS_CHANGE_COUNTRY = 1,
        MAXIMUM_TRANSACTION_THRESHOLD_REACHED = 2,
        USER_ENROLLED_IN_SAME_RECURRING_GROUP_PRODUCT = 3,
        INSUFFICIENT_WALLET_LOAD = 4,
        EWALLET_PAYMENT_IS_DISABLED = 5,
        UNSPECIFIED_ERROR = 6,
        PREPAID_CARD_PAYMENT_IS_DISABLED = 7,
        PAYPAL_PAYMENT_IS_DISABLED = 8,
        CREDIT_CARD_PAYMENT_IS_DISABLED = 9,
        CREDIT_CARD_IS_NOT_AVAILABLE_IN_YOUR_AREA = 10

    }

    public enum ReloadError
    {
        PENDING_GOMS_CHANGE_COUNTRY = 1,
        MAXIMUM_TRANSACTION_THRESHOLD_REACHED = 2,
        USER_IS_NOT_AUTHENTICATED = 3,
        USER_DOES_NOT_EXIST = 4,
        USER_WALLET_DOES_NOT_EXIST = 5,
        UNSPECIFIED_ERROR = 6,
        PREPAID_CARD_RELOAD_IS_DISABLED = 7,
        PAYPAL_RELOAD_IS_DISABLED = 8,
        CREDIT_CARD_RELOAD_IS_DISABLED = 9,
        CREDIT_CARD_IS_NOT_AVAILABLE_IN_YOUR_AREA = 10,
        SMARTPIT_RELOAD_IS_DISABLED = 11,
        MOPAY_RELOAD_IS_DISABLED = 12
    }

    public enum CelebrityContentType
    {
        SHOWS = 1,
        MOVIES = 2,
        EPISODES = 3
    }

    public enum VideoQualityCdnReference
    {
        StandardDefinition = 2,
        HighDefinition = 6
    }

    public enum Platform
    {
        Desktop = 0,
        Mobile = 1
    }
    public enum ITEIdType
    {
        PhoneNumber = 0,
        AccountNumber = 1
    }

    public enum ITEResponseError
    {
        IDTYPE_NOT_DEFINED = 1,
        IDTYPE_SHOULD_BE_NUMERIC = 2,
        IDTYPE_LENGTH_SHOULD_BE_1 = 3,
        ACCOUNTANI_SHOULD_BE_DEFINED = 4,
        ACCOUNTANI_SHOULD_BE_NUMERIC = 5,
        ACCOUNTANI_LENGTH_SHOULD_BE_10 = 6,
        VALIDATIONCODE_NOT_DEFINED = 7,
        VALIDAITONCODE_LENGTH_SHOULD_BE_10 = 8,
        TFCTVUSERID_NOT_DEFINED = 9,
        TFCTVUSERID_IS_BLANK = 11,
        ACCOUNTANI_NOT_FOUND = 12,
        ACTIVATION_CODE_MISMTCH = 13,
        IP_NOT_ALLOWED = 14,
        MISSING_PARAMETERS = 15,
        USER_NOT_FOUND = 16,
        UNABLE_TO_COMMIT = 17,
        ITEID_DO_NOT_MATCH = 18,
        SUCCESS = 0,
        UNKNOWN_ERROR = -1,
        ACCOUNTANI_ALREADY_USED = 100,
        REGISTRATION_REQUIRED = 101,
        TFCTV_ACCOUNT_ALREADY_ACTIVATED = 102,
        VALIDATION_SUCCESS = 200

    }

    public enum ITEPackageType
    {
        LITEPACKAGE = 868
    }
}
