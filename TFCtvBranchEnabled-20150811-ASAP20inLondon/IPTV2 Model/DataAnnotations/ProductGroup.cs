using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace IPTV2_Model
{

    [MetadataType(typeof(ProductGroupMetaData))]
    public partial class ProductGroup
    {
    }

    public partial class ProductGroupMetaData
    {
        [Required]
        [DisplayName("Product Group")]
        public int ProductGroupId { get; set; }
        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }
        [DisplayName("Description")]
        public string Description { get; set; }
        [Required]
        [DisplayName("Offering")]
        public int OfferingId { get; set; }
    }
}