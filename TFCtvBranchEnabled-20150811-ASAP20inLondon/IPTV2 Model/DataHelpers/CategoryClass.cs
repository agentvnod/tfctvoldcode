using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class CategoryClass : IComparable<CategoryClass>
    {
        public int CompareTo(CategoryClass other)
        {
            return (CategoryId.CompareTo(other.CategoryId));
        }

    }
}
