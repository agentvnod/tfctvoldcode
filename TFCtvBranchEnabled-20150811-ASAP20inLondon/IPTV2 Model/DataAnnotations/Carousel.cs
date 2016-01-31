using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace IPTV2_Model
{

    [MetadataType(typeof(CarouselMetaData))]
    public partial class Carousel
    {
    }

    public partial class CarouselMetaData
    {

        public int CarouselId { get; set; }
        [Required]
        [DisplayName("Carousel Name")]
        public string Name { get; set; }
        [Required]
        [DisplayName("Offering")]
        public int OfferingId { get; set; }
        public int Transition { get; set; }
        public int Drop { get; set; }
        [DisplayName("Status")]
        public byte StatusId { get; set; }

    }

    [MetadataType(typeof(CategoryCarouselMetaData))]
    public partial class CategoryCarousel
    {
    }

    public partial class CategoryCarouselMetaData
    {
        [Required]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [Required]
        [DisplayName("Offering")]
        public int OfferingId { get; set; }
        public int CarouselId { get; set; }
        [Required]
        [DisplayName("Carousel Name")]
        public string Name { get; set; }
        [Required]
        [DisplayName("Status")]
        public byte StatusId { get; set; }
    }

    [MetadataType(typeof(PackageCarouselMetaData))]
    public partial class PackageCarousel
    {
    }

    public partial class PackageCarouselMetaData
    {
        [Required]
        [DisplayName("Package")]
        public int PackageId { get; set; }
        [Required]
        [DisplayName("Offering")]
        public int OfferingId { get; set; }
        public int CarouselId { get; set; }
        [Required]
        [DisplayName("Carousel Name")]
        public string Name { get; set; }
        [Required]
        [DisplayName("Status")]
        public byte StatusId { get; set; }
    }

}