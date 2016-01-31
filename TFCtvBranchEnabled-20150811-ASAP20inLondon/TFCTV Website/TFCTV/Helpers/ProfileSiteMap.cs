using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcSiteMapProvider.Extensibility;
using IPTV2_Model;

namespace TFCTV.Helpers
{
    public class ProfileSiteMap : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection()
        {
            if (GlobalConfig.IsProfileSiteMapEnabled)
            {
                List<DynamicNode> returnValue = null;
                var cache = DataCache.Cache;
                string cacheKey = "SMAPPROFILE:O:00";
                returnValue = (List<DynamicNode>)cache[cacheKey];
                // Build value  
                if (returnValue == null)
                {
                    try
                    {
                        returnValue = new List<DynamicNode>();
                        var context = new IPTV2_Model.IPTV2Entities();
                        // Create a node for each show
                        var users = context.Users.Where(u => u.StatusId == 1).ToList();
                        foreach (var user in users)
                        {
                            DynamicNode node = new DynamicNode();
                            node.Key = "User-" + user.UserId.ToString();
                            node.Title = "User - " + user.FirstName + " " + user.LastName;
                            node.RouteValues.Add("id", user.UserId.ToString());
                            returnValue.Add(node);
                        }
                        var cacheDuration = new TimeSpan(2, 0, 0);
                        cache.Put(cacheKey, returnValue, cacheDuration);
                    }
                    catch (Exception)
                    {
                        returnValue = new List<DynamicNode>();
                    }
                }
                // Return 
                return returnValue;
            }
            else return new List<DynamicNode>();
        }

        public override CacheDescription GetCacheDescription()
        {
            return new CacheDescription("ProfileSiteMap")
            {
                SlidingExpiration = TimeSpan.FromHours(24)
            };
        }


    }
}