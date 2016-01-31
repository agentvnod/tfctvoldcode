using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public class StoreFrontDistance : IComparable<StoreFrontDistance>
    {
        public double Distance;
        public StoreFront Store;

        public StoreFrontDistance(double distance, StoreFront store)
        {
            Distance = distance;
            Store = store;
        }

        public int CompareTo(StoreFrontDistance other)
        {
            return (Distance.CompareTo(other.Distance));
        }

    }

}
