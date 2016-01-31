using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace IPTV2_Model
{

    [MetadataType(typeof(FeatureMetaData))]
    public partial class Feature
    {
    }

    public partial class FeatureMetaData
    {


        [Required]
        [DisplayName("Feature")]
        public int FeatureId { get; set; }
        [Required]
        [DisplayName("Offering")]
        public int OfferingId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        [DisplayName("Status")]
        public byte StatusId { get; set; }
    
       
    }

}