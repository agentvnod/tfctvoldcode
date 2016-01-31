using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    class ChannelTester
    {
        public static void ChannelTest()
        {
            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(2);
            var channels = context.Channels.Find(1);
            var packages = channels.GetPackageProductIds(offering, "US", RightsType.Online, false);
            var packagesCached = channels.GetPackageProductIds(offering, "US", RightsType.Online, true);
            var onlinePackages = channels.GetPackageProductIds(offering, "US", RightsType.Online, true);
        }
    }
}
