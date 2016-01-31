using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace IPTV2_Model
{

    [MetadataType(typeof(CarouselSlideMetaData))]
    public partial class CarouselSlide
    {
    }

    public partial class CarouselSlideMetaData
    {

        public int CarouselSlideId { get; set; }
        [Required]
        [DisplayName("Slide Name")]
        public string Name { get; set; }
        //[Required]
        [DisplayName("Banner Image URL")]
        public string BannerImageUrl { get; set; }
        //[Required]
        [DisplayName("Thumbnail URL")]
        public string ThumbnailUrl { get; set; }
        //[Required]
        public string Header { get; set; }
        //[Required]
        public string Blurb { get; set; }
        //[Required]
        [DisplayName("Button Label")]
        public string ButtonLabel { get; set; }
        //[Required]
        [DisplayName("Target URL")]
        public string TargetUrl { get; set; }
        [Required]
        [DisplayName("Status")]
        public byte StatusId { get; set; }
        [Required]
        [DisplayName("Mobile Status")]
        public byte MobileStatusId { get; set; }
        [Required]
        [DisplayName("IPTV Status")]
        public byte IptvStatusId { get; set; }

    }

}