using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace IPTV2_Model
{
    /// <summary>
    /// GeoLocation computation
    /// Source: http://www.codeproject.com/Articles/18108/Store-Locator-Help-customers-find-you-with-Google
    /// Get Latitude/Longitude: http://universimmedia.pagesperso-orange.fr/geo/loc.htm 
    /// </summary>
    public struct GeoLocation
    {
        public double Latitude;
        public double Longitude;

        const double EarthRadiusInKms = 6371;
        const double EarthRadiusInMiles = 3959.0; 

        public GeoLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return "Latitude: " + Latitude.ToString() + " Longitude: " + Longitude.ToString();
        }

        public static double Distance(GeoLocation loc1, GeoLocation loc2, bool inKilometers)
        {
            /*
            The Haversine formula according to Dr. Math.
            http://mathforum.org/library/drmath/view/51879.html
                
            dlon = lon2 - lon1
            dlat = lat2 - lat1
            a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
            c = 2 * atan2(sqrt(a), sqrt(1-a)) 
            d = R * c
                
            Where
                * dlon is the change in longitude
                * dlat is the change in latitude
                * c is the great circle distance in Radians.
                * R is the radius of a spherical Earth.
                * The locations of the two points in 
                    spherical coordinates (longitude and 
                    latitude) are lon1,lat1 and lon2, lat2.
            */

            double dDistance = Double.MinValue;
            double dLat1InRad = loc1.Latitude * (Math.PI / 180.0);
            double dLong1InRad = loc1.Longitude * (Math.PI / 180.0);
            double dLat2InRad = loc2.Latitude * (Math.PI / 180.0);
            double dLong2InRad = loc2.Longitude * (Math.PI / 180.0);

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Asin(Math.Sqrt(a));

            dDistance = c * (inKilometers ? EarthRadiusInKms : EarthRadiusInMiles);

            return dDistance;
        }

        public static double DistanceInKilometers(GeoLocation loc1, GeoLocation loc2)
        {
            return Distance(loc1, loc2, true);
        }

        public static double DistanceInMiles(GeoLocation loc1, GeoLocation loc2)
        {
            return Distance(loc1, loc2, false);
        }
    }
}
