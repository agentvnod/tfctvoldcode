using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    class StoreFrontTester
    {
        public static void GetStores()
        {
            double latitude = 37.52411;
            double longitude = -122.25914;

            var loc = new GeoLocation(latitude, longitude);

            int offeringId = 2;
            var context = new IPTV2Entities();
            var stores = StoreFront.GetNearestStores(context, offeringId, loc, false);

        }
    }
}
