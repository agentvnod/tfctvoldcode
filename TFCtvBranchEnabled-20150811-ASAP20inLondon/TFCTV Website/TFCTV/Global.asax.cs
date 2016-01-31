using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using IPTV2_Model;
using MvcSiteMapProvider.Web;
using TFCTV.Helpers;

namespace TFCTV
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode,
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "GetLatestFullEpisodes",
                url: "Ajax/GetLatestFullEpisodes/{id}",
                defaults: new { controller = "Ajax", action = "GetListing", id = GlobalConfig.LatestFullEpisodes }
            );
            routes.MapRoute(
                name: "GetFreeTV",
                url: "Ajax/GetFreeTV",
                defaults: new { controller = "Ajax", action = "GetListing", id = GlobalConfig.FreeTV }
            );
            routes.MapRoute(
                name: "GetMostViewed",
                url: "Ajax/GetMostViewed",
                defaults: new { controller = "Ajax", action = "GetListing", id = GlobalConfig.MostViewed }
            );

            // News
            routes.MapRoute(
              name: "GetLatestNewsFullEpisodes",
              url: "Ajax/GetLatestNewsFullEpisodes",
              defaults: new { controller = "Ajax", action = "GetListing", id = GlobalConfig.LatestNewsFullEpisodes }
            );
            routes.MapRoute(
              name: "GetNewsFreeTV",
              url: "Ajax/GetNewsFreeTV",
              defaults: new { controller = "Ajax", action = "GetListing", id = GlobalConfig.NewsFreeTV }
            );

            routes.MapRoute(
              name: "GetRegionalNews",
              url: "Ajax/GetRegionalNews",
              defaults: new { controller = "Ajax", action = "GetListing", id = GlobalConfig.FeaturedRegionalNews }
            );

            routes.MapRoute(
             name: "GetCurrentAffairs",
             url: "Ajax/GetCurrentAffairs",
             defaults: new { controller = "Ajax", action = "GetListing", id = GlobalConfig.FeaturedCurrentAffairs }
            );

            routes.MapRoute(
                name: "GetFeaturedJournalist",
                url: "Ajax/GetFeaturedJournalist",
                defaults: new { controller = "Ajax", action = "GetFeaturedPerson", id = GlobalConfig.FeaturedJournalists }
            );

            // Movies
            routes.MapRoute(
                name: "GetFeaturedPersonMovies",
                url: "Ajax/GetFeaturedPersonMovies",
                defaults: new { controller = "Ajax", action = "GetFeaturedPerson", id = GlobalConfig.FeaturedPersonMovies }
            );
            routes.MapRoute(
                name: "GetLatestMovies",
                url: "Ajax/GetLatestMovies",
                defaults: new { controller = "Ajax", action = "GetListing", id = GlobalConfig.LatestMovies }
            );

            routes.MapRoute(
               name: "OnlinePremiere",
               url: "OnlinePremiere",
               defaults: new { controller = "Promo", action = "OnlinePremiere", id = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "GoPremium",
                url: "GoPremium",
                defaults: new { controller = "Packages", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Compare",
                url: "Subcription/GoCompare",
                defaults: new { controller = "Packages", action = "Compare" }
            );

            routes.MapRoute(
                name: "Subscription",
                url: "Subscription",
                defaults: new { controller = "Packages", action = "Subscription", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Alacarte",
                url: "Alacarte/{action}/{id}",
                defaults: new { controller = "Packages", action = "PerSerye", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "EditProfile", //Route name
                "editprofile", //URL with parameters
                new { controller = "User", action = "EditProfile" }
            );

            routes.MapRoute(
               "Subscribe", // Route name
               "Subscribe/Product/{id}", // URL with parameters
               new { controller = "Subscribe", action = "Create", id = UrlParameter.Optional } // Parameter defaults
           );

            routes.MapRoute(
                "Profile", // Route name
                "Profile/{id}/{slug}", // URL with parameters
                new { controller = "Profile", action = "Index", id = UrlParameter.Optional, slug = UrlParameter.Optional } // Parameter defaults
            );

            /* routes.MapRoute(
                 "SocialEngagement", // Route name
                 "SocialEngagement/{id,userStatus}", // URL with parameters
                 new { controller = "Profile", action = "Index", id = UrlParameter.Optional,userStatus = UrlParameter.Optional } // Parameter defaults
             );*/

            routes.MapRoute(
               "Gifting", // Route name
               "Gift/{action}/{id}/{wid}", // URL with parameters
               new { controller = "Home", action = "Index", id = UrlParameter.Optional, wishlistId = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
               "Halalan2013", // Route name
               "Halalan2013/{action}/{id}/{slug}", // URL with parameters
               new { controller = "Halalan", action = "Index", id = UrlParameter.Optional, slug = UrlParameter.Optional } // Parameter defaults
           );

            routes.MapRoute(
                "FreeTV", // Route name
                "FreeTV/{id}/{episodeid}", // URL with parameters
                new { controller = "FreeTV", action = "Index", id = UrlParameter.Optional, episodeid = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                name: "FindADealer",
                url: "Find-A-Dealer",
                defaults: new { controller = "StoreLocator", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "LiveSpecials",
               url: "LiveSpecials",
               defaults: new { controller = "Promo", action = "LiveSpecials", id = UrlParameter.Optional }
           );

            routes.MapRoute(
               name: "HimigHandog",
               url: "HimigHandog/{id}",
               defaults: new { controller = "Promo", action = "HimigHandog", id = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "HaloHaloList",
                url: "HaloHalo/List",
                defaults: new { controller = "HaloHalo", action = "List", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "HaloHaloLMC",
                url: "HaloHalo/LoadMoreContent/{page}/{pageSize}",
                defaults: new { controller = "HaloHalo", action = "LoadMoreContent", page = UrlParameter.Optional, pageSize = UrlParameter.Optional }
            );

            routes.MapRoute(
                 name: "KuwentongPaskoList",
                 url: "KwentoNgPasko/List/{id}",
                 defaults: new { controller = "KwentoNgPasko", action = "List", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Dolphy",
                url: "Dolphy",
                defaults: new { controller = "Celebrity", action = "Profile", id = GlobalConfig.DolphyCelebrityId }
            );

            //routes.MapRoute(
            //    name: "BalitangAmericaSagipKapamilya",
            //    url: "SagipKapamilya",
            //    defaults: new { controller = "Live", action = "Details", id = GlobalConfig.BalitangAmericaSagipKapamilyaEpisodeId, slug = GlobalConfig.BalitangAmericaSagipKapamilyaSlug }
            //);

            routes.MapRoute(
                name: "SK",
                url: "SK",
                defaults: new { controller = "Live", action = "Details", id = GlobalConfig.BalitangAmericaSagipKapamilyaEpisodeId, slug = GlobalConfig.BalitangAmericaSagipKapamilyaSlug }
            );

            routes.MapRoute(
                name: "KLC",
                url: "KLC",
                defaults: new { controller = "Live", action = "Details", id = GlobalConfig.KapamilyaLoveChatEpisodeId, slug = GlobalConfig.KapamilyaLoveChatSlug }
            );

            routes.MapRoute(
                name: "mayweather-vs-pacquiao-may-3",
                url: "mayweather-vs-pacquiao-may-3",
                defaults: new { controller = "PacMay", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "PinoyPride",
                url: "PinoyPride",
                defaults: new { controller = "Live", action = "Details", id = GlobalConfig.PinoyPride30EpisodeId, slug = GlobalConfig.PinoyPride30Slug }
            );

            routes.MapRoute(
                name: "KapitBisig",
                url: "kapitbisig",
                defaults: new { controller = "Live", action = "Details", id = GlobalConfig.KapitBisigEpisodeId, slug = GlobalConfig.KapitBisigSlug }
            );

            //routes.MapRoute(
            //    name: "AsapDubai2014",
            //    url: "ASAPDubai2014",
            //    defaults: new { controller = "Live", action = "Details", id = GlobalConfig.ASAPDubai2014EpisodeId, slug = GlobalConfig.ASAPDubai2014Slug }
            //);

            routes.MapRoute(
            name: "PBB1", // Route name
            url: "pinoy-big-brother", // URL with parameters
            defaults: new { controller = "PinoyBigBrother", action = "PBB737", id = UrlParameter.Optional }
            );

            routes.MapRoute(
              name: "PBB2", // Route name
              url: "pbb", // URL with parameters
              defaults: new { controller = "PinoyBigBrother", action = "PBB737", id = UrlParameter.Optional }
            );

            routes.MapRoute(
              name: "PBB3", // Route name
              url: "pinoybigbrother", // URL with parameters
              defaults: new { controller = "PinoyBigBrother", action = "PBB737", id = UrlParameter.Optional }
            );

            routes.MapRoute(
              name: "PBB4", // Route name
              url: "pbb737", // URL with parameters
              defaults: new { controller = "PinoyBigBrother", action = "PBB737", id = UrlParameter.Optional }
            );

            routes.MapRoute(
              name: "ChristmasSpecial",
              url: "ChristmasSpecial",
              defaults: new { controller = "Live", action = "Details", id = GlobalConfig.ChristmasSpecialEpisodeId, slug = GlobalConfig.ChristmasSpecialSlug }
            );

            routes.MapRoute(
               name: "KapamilyaLoveChat",
               url: "KapamilyaLoveChat",
               defaults: new { controller = "Live", action = "Details", id = GlobalConfig.KapamilyaLoveChatEpisodeId, slug = GlobalConfig.KapamilyaLoveChatSlug }
            );

            routes.MapRoute(
                name: "featured-celebrities",
                url: "featured-celebrities",
                defaults: new { controller = "Celebrity", action = "List", id = GlobalConfig.FeaturedCelebrities, slug = GlobalConfig.FeaturedCelebSlug }
            );


            routes.MapRoute(
               name: "BayaningFilipino",
               url: "bayaning-filipino",
               defaults: new { controller = "Live", action = "Details", id = GlobalConfig.BayaningFilipinoEpisodeId, slug = GlobalConfig.BayaningFilipinoSlug }
           );
            routes.MapRoute(
   name: "Events",
   url: "Events/{id}",
   defaults: new { controller = "Events", action = "Index", id = UrlParameter.Optional }
);
            //routes.MapRoute(
            //   name: "TulongPH",
            //   url: "TulongPH",
            //   defaults: new { controller = "Live", action = "Details", id = GlobalConfig.TulongPHConcertEpisodeId, slug = GlobalConfig.TulongPHConcertSlug }
            //);

            //routes.MapRoute(
            //    name: "TFCSagipKapamilya",
            //    url: "TFCSagipKapamilya",
            //    defaults: new { controller = "Live", action = "Details", id = GlobalConfig.BalitangAmericaSagipKapamilyaEpisodeId, slug = GlobalConfig.BalitangAmericaSagipKapamilyaSlug }
            //);
            routes.MapRoute(
                name: "ABSCBNFreeLiveStream",
                url: "abscbnlive",
                defaults: new { controller = "Live", action = "Details", id = GlobalConfig.ABSCBNFreeLiveStreamEpisodeId, slug = GlobalConfig.ABSCBNFreeLiveStreamSlug }
            );

            routes.MapRoute(
                name: "KuwentoNgPasko",
                url: "KwentoNgPasko/{id}/{slug}",
                defaults: new { controller = "KwentoNgPasko", action = "Details", id = UrlParameter.Optional, slug = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "KuwentoNgPasko1",
                url: "KwentoNgPasko",
                defaults: new { controller = "KwentoNgPasko", action = "Details" }
            );

            routes.MapRoute(
                name: "TFC20",
                url: "TFC20/{id}/{slug}",
                defaults: new { controller = "TFC20", action = "Details", id = UrlParameter.Optional, slug = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "TFC20LMC",
                url: "TFC20/GetMoreTFC20Content/{page}/{pageSize}",
                defaults: new { controller = "TFC20", action = "GetMoreTFC20Content", page = UrlParameter.Optional, pageSize = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "HaloHalo",
                url: "HaloHalo/{id}/{slug}",
                defaults: new { controller = "HaloHalo", action = "Video", id = UrlParameter.Optional, slug = UrlParameter.Optional }
            );


            routes.MapRoute(
               name: "TFCChannel",
               url: "TFCChannel/{id}",
               defaults: new { controller = "Packages", action = "TVE", id = UrlParameter.Optional }
           );

            //   routes.MapRoute(
            //    name: "Free",
            //    url: "Free/{promocode}/{key}",
            //    defaults: new { controller = "Free", action = "Index", promocode = UrlParameter.Optional, key = UrlParameter.Optional }
            //);

            routes.MapRoute(
              name: "PromoOffer",
              url: "Offer/{id}",
              defaults: new { controller = "Offer", action = "Index", id = UrlParameter.Optional }
          );

            routes.MapRoute(
                name: "TFCEverywhere",
                url: "TFCEverywhere",
                defaults: new { controller = "User", action = "TVEverywhereMain", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "OneKapamilyaGo",
                url: "OneKapamilyaGo",
                defaults: new { controller = "Promo", action = "OKGo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "OnlineConcerts",
                url: "Concerts",
                defaults: new { controller = "Promo", action = "Concerts", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ClickTayoMV",
                url: "ClickTayo",
                defaults: new { controller = "Promo", action = "ClickTayoMV", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "HeatMap",
                url: "ClickTayoWorldwide",
                defaults: new { controller = "Promo", action = "HeatMap", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "WatchLive",
                url: "air/{id}",
                defaults: new { controller = "Air", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "WatchNow",
               url: "WatchNow/{id}",
               defaults: new { controller = "Air", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "LogOut",
                url: "LogOut",
                defaults: new { controller = "User", action = "LogOut", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "sitemap", // Route name
                "sitemap.xml", // URL with parameters
                new { controller = "XmlSiteMap", action = "Index", page = 0 } // Parameter defaults
            );

            routes.MapRoute(
                "sitemapPage", // Route name
                "sitemap-{page}.xml", // URL with parameters
                new { controller = "XmlSiteMap", action = "Index", page = 0 } // Parameter defaults
            );

            routes.MapRoute(
                "Bayanihan", // Route name
                "Bayanihan", // URL with parameters
                new { controller = "Events", action = "Index", id= "KR2" } // Parameter defaults
            );

            //routes.MapRoute(
            //    "SerChiefMayaWedding",
            //    "Ser-Chief-Maya-Wedding/{action}/{id}/{slug}",
            //     new { controller = "Wedding", action = "Index", id = UrlParameter.Optional, slug = UrlParameter.Optional } // Parameter defaults
            //);

            routes.MapRoute(name: "SiteRoot", url: "",
                    defaults: new { controller = "Home", action = "Index" });

            //Show with Slug
            routes.MapRoute(
                "ShowDetailsSlug", // Route name
                "{controller}/{action}/{id}/{slug}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional, slug = "" } // Parameter defaults
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            // make sitemap.xml available
            //XmlSiteMapController.RegisterRoutes(RouteTable.Routes);

            //instantiate show list
            //CategoryTreeHelper.GetShowsOnCurrentOffering();
            if (GlobalConfig.UseCountryListInMemory)
                ContextHelper.InitializeCountryList();

        }

        protected void Application_BeginRequest()
        {
            var isAllowed = false;

            if (GlobalConfig.IsProfilingEnabled)
            {
                if (Request.IsLocal)
                    isAllowed = true;
                else
                {
                    //if (HttpContext.Current.User.Identity.IsAuthenticated)
                    //{
                    //    string list = GlobalConfig.AllowedUserIdsForProfiling;
                    //    string[] listOfUserIds = list.Split(';');
                    //    if (listOfUserIds.Contains(HttpContext.Current.User.Identity.Name))
                    //        isAllowed = true;
                    //}

                    if (HttpContext.Current.Request.Cookies["miniprofiler"] != null)
                    {
                        HttpCookie cookie = HttpContext.Current.Request.Cookies["miniprofiler"];
                        isAllowed = Convert.ToBoolean(cookie.Value);
                    }
                }
            }

            if (isAllowed)
                StackExchange.Profiling.MiniProfiler.Start();
        }

        protected void Application_EndRequest()
        {
            //var isAllowed = false;

            //if (GlobalConfig.IsProfilingEnabled)
            //{
            //    if (Request.IsLocal)
            //        isAllowed = true;
            //    else
            //    {
            //        if (HttpContext.Current.Request.Cookies["miniprofiler"] != null)
            //        {
            //            HttpCookie cookie = HttpContext.Current.Request.Cookies["miniprofiler"];
            //            isAllowed = Convert.ToBoolean(cookie.Value);
            //        }
            //    }
            //}

            //if (isAllowed)
            StackExchange.Profiling.MiniProfiler.Stop();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            //removed as it is throwing duplicate errors into elmah
            //log error and chug along
            //Exception exception = Server.GetLastError();
            //MyUtility.LogException(exception);
            //Server.ClearError();

            Exception ex = Server.GetLastError();
            //if (ex is Microsoft.ApplicationServer.Caching.DataCacheException)
            if (ex.GetType() == typeof(Microsoft.ApplicationServer.Caching.DataCacheException))
            {
                DataCache.Refresh();
                // Clear DistributedCacheSessionStateStoreProvider._staticInternalProvider
                var providerType = typeof(Microsoft.Web.DistributedCache.DistributedCacheSessionStateStoreProvider);
                var providerField = providerType.GetField("_staticInternalProvider", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                providerField.SetValue(null, null);
            }
            Server.ClearError();
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Request.Url.AbsolutePath.ToLowerInvariant().Contains("tfctv-site-monitoring.axd"))
            {
                if (HttpContext.Current.User != null)
                {
                    if (MyUtility.isUserLoggedIn())
                    {
                        var userId = new Guid(User.Identity.Name);
                        var context = new IPTV2Entities();
                        var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            var allowedEmails = GlobalConfig.ElmahAllowedEmails.Split(',');
                            if (!allowedEmails.Contains(user.EMail.ToLower()))
                                HttpContext.Current.Response.StatusCode = 404;
                        }
                        else
                            HttpContext.Current.Response.StatusCode = 404;
                    }
                    else
                        HttpContext.Current.Response.StatusCode = 404;
                }
                else
                    HttpContext.Current.Response.StatusCode = 404;
            }
            else if (Request.Url.AbsolutePath.ToLowerInvariant().Contains("tfctv-site-monitoring"))
                HttpContext.Current.Response.StatusCode = 404;
        }
    }
}
