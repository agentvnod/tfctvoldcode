using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcSiteMapProvider.Extensibility;
using IPTV2_Model;

namespace TFCTV.Helpers
{
    public class CelebritySiteMap : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection()
        {
            List<DynamicNode> returnValue = null;
            var cache = DataCache.Cache;
            string cacheKey = "SMAPCELEB:O:00";
            returnValue = (List<DynamicNode>)cache[cacheKey];
            // Build value 
            if (returnValue == null)
            {
                try
                {
                    returnValue = new List<DynamicNode>();
                    var context = new IPTV2_Model.IPTV2Entities();
                    // Create a node for each celebrity 
                    var celebs = context.Celebrities.ToList();
                    foreach (var celeb in celebs)
                    {
                        DynamicNode node = new DynamicNode();
                        node.Key = "Celebrity-" + celeb.CelebrityId;
                        node.Title = "Celebrity - " + celeb.FullName;
                        //node.Controller = "Celebrity";
                        //node.Action = "Profile";
                        node.RouteValues.Add("id", celeb.CelebrityId);
                        returnValue.Add(node);
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
            return new CacheDescription("CelebritySiteMap")
            {
                SlidingExpiration = TimeSpan.FromHours(24)
            };
        }

    }
}