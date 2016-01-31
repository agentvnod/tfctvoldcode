using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace TFCtv_Background_Cache__Updater
{
    class SubscriptionProductCCacheRefresher
    {
        public void FillCache(IPTV2Entities context, int offeringId, RightsType rightsType, TimeSpan cacheDuration)
        {
            SubscriptionProductC.LoadAll(context, offeringId, true, cacheDuration);            
        }
    }
}
