using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    [Serializable]
    public partial class ProductGroupC : IComparable<ProductGroupC>
    {
        public ProductGroupC()
        {

        }

        public int ProductGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<int> PackageIds { get; set; }
        public ICollection<int> ShowIds { get; set; }

        public int CompareTo(ProductGroupC other)
        {
            return (ProductGroupId.CompareTo(other.ProductGroupId));
        }

    }
}
