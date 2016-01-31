using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace IPTV2_Model
{

    [MetadataType(typeof(PackageSubscriptionProductMetaData))]
    public partial class PackageSubscriptionProduct
    {
    }

    public partial class PackageSubscriptionProductMetaData
    {


        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [DisplayName("Offering")]
        public int OfferingId { get; set; }
        [Required]
        [DisplayName("Status")]
        public byte StatusId { get; set; }
        [Required]
        public int Duration { get; set; }
        [Required]
        [DisplayName("Duration Type")]
        public string DurationType { get; set; }
        [DisplayName("GOMS Product Id")]
        public string GomsProductId { get; set; }
        [DisplayName("GOMS Product Quantity")]
        public string GomsProductQuantity { get; set; }
        [Required]
        [DisplayName("Product Group")]
        public int ProductGroupId { get; set; }

    }

    [MetadataType(typeof(ShowSubscriptionProductMetaData))]
    public partial class ShowSubscriptionProduct
    {
    }

    public partial class ShowSubscriptionProductMetaData
    {


        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [DisplayName("Offering")]
        public int OfferingId { get; set; }
        [Required]
        [DisplayName("Status")]
        public byte StatusId { get; set; }
        [Required]
        public int Duration { get; set; }
        [Required]
        [DisplayName("Duration Type")]
        public string DurationType { get; set; }
        [DisplayName("GOMS Product Id")]
        public string GomsProductId { get; set; }
        [DisplayName("GOMS Product Quantity")]
        public string GomsProductQuantity { get; set; }
        [Required]
        [DisplayName("Product Group")]
        public int ProductGroupId { get; set; }
        [DisplayName("A La Carte Subscription Type")]
        public int ALaCarteSubscriptionTypeId { get; set; }


    }

    [MetadataType(typeof(EpisodeSubscriptionProductMetaData))]
    public partial class EpisodeSubscriptionProduct
    {
    }

    public partial class EpisodeSubscriptionProductMetaData
    {


        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [DisplayName("Offering")]
        public int OfferingId { get; set; }
        [Required]
        [DisplayName("Status")]
        public byte StatusId { get; set; }
        [Required]
        public int Duration { get; set; }
        [Required]
        [DisplayName("Duration Type")]
        public string DurationType { get; set; }
        [DisplayName("GOMS Product Id")]
        public string GomsProductId { get; set; }
        [DisplayName("GOMS Product Quantity")]
        public string GomsProductQuantity { get; set; }
        [Required]
        [DisplayName("Product Group")]
        public int ProductGroupId { get; set; }

    }

}