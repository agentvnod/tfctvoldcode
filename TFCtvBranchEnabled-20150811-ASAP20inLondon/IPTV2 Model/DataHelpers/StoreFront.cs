using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class StoreFront
    {

        public static SortedSet<StoreFrontDistance> GetNearestStoresInKilometers(IPTV2Entities context, int offeringId, GeoLocation loc)
        {
            return GetNearestStores(context, offeringId, loc, true);
        }


        public static SortedSet<StoreFrontDistance> GetNearestStoresInKilometers(IPTV2Entities context, int offeringId, GeoLocation loc, double maximumDistance)
        {
            return GetNearestStores(context, offeringId, loc, true, maximumDistance);
        }


        public static SortedSet<StoreFrontDistance> GetNearestStoresInMiles(IPTV2Entities context, int offeringId, GeoLocation loc)
        {
            return GetNearestStores(context, offeringId, loc, false);
        }

        public static SortedSet<StoreFrontDistance> GetNearestStoresInMiles(IPTV2Entities context, int offeringId, GeoLocation loc, double maximumDistance)
        {
            return GetNearestStores(context, offeringId, loc, false, maximumDistance);
        }

        public static SortedSet<StoreFrontDistance> GetNearestStores(IPTV2Entities context, int offeringId, GeoLocation loc, bool inKilometers)
        {
            return GetNearestStores(context, offeringId, loc, inKilometers, double.MaxValue);
        }

        /// <summary>
        /// GetNearestStores to a specific location, order by distance
        /// </summary>
        /// <param name="context">Database Context</param>
        /// <param name="offeringId">Offering Id</param>
        /// <param name="loc">Origin Location</param>
        /// <param name="inKilometers">In Kilometers or Miles</param>
        /// <param name="maximumDistance">Maximum distance from origin</param>
        /// <returns></returns>
        public static SortedSet<StoreFrontDistance> GetNearestStores(IPTV2Entities context, int offeringId, GeoLocation loc, bool inKilometers, double maximumDistance)
        {
            var stores = new SortedSet<StoreFrontDistance>();
            var allStores = context.StoreFronts.Where(sf => sf.OfferingId == offeringId && sf.StatusId == 1);
            foreach (var s in allStores)
            {
                try
                {
                    double distance = GeoLocation.Distance(loc, new GeoLocation((double)s.Latitude, (double)s.Longitude), inKilometers);
                    if (distance <= maximumDistance)
                    {
                        stores.Add(new StoreFrontDistance(distance, s));
                    }
                }
                catch (Exception)
                {
                }
            }
            return stores;
        }
    }

}
