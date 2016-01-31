using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcSiteMapProvider.Extensibility;
using IPTV2_Model;

namespace TFCTV.Helpers
{
    public class ShowSiteMap : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection()
        {
            List<DynamicNode> returnValue = null;
            var cache = DataCache.Cache;
            string cacheKey = "SMAPSHOW:O:00";
            returnValue = (List<DynamicNode>)cache[cacheKey];
            // Build value
            if (returnValue == null)
            {
                try
                {
                    returnValue = new List<DynamicNode>();
                    var context = new IPTV2_Model.IPTV2Entities();
                    var service = context.PackageTypes.Find(GlobalConfig.serviceId);
                    var showIds = service.GetAllIptvShowIds("US");
                    // Create a node for each show
                    foreach (var i in showIds)
                    {
                        var show = (IPTV2_Model.Show)context.CategoryClasses.Find(i);
                        DynamicNode node = new DynamicNode();
                        node.Key = "Show-" + show.CategoryId;
                        node.Title = "Show - " + show.CategoryName;
                        node.Controller = "Show";
                        node.Action = "Details";
                        node.RouteValues.Add("id", show.CategoryId);
                        var dbSlug = MyUtility.GetSlug(show.Description);
                        node.RouteValues.Add("slug", dbSlug);
                        returnValue.Add(node);

                        //// get episodes of show
                        //foreach (var episode in show.Episodes.Where(e => e.Episode.OnlineStatusId == 1))
                        //{
                        //    DynamicNode episodeNode = new DynamicNode();
                        //    episodeNode.Key = "Episodes-" + episode.Episode.EpisodeId;
                        //    episodeNode.Title = "Episode - " + episode.Episode.EpisodeName;
                        //    episodeNode.Controller = "Episode";
                        //    episodeNode.Action = "Details";
                        //    episodeNode.ParentKey = node.Key;
                        //    episodeNode.RouteValues.Add("id", episode.Episode.EpisodeId);
                        //    returnValue.Add(episodeNode);
                        //}                   
                    }
                    var cacheDuration = new TimeSpan(2, 0, 0);
                    cache.Put(cacheKey, returnValue, cacheDuration);
                }
                catch (Exception) { returnValue = new List<DynamicNode>(); }

            }
            // Return
            return returnValue;
        }

        public override CacheDescription GetCacheDescription()
        {
            return new CacheDescription("ShowSiteMap")
            {
                SlidingExpiration = TimeSpan.FromHours(24)
            };
        }
    }
}