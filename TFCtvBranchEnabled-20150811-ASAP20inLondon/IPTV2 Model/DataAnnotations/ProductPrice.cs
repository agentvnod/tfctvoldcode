using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace IPTV2_Model
{

    [MetadataType(typeof(ProductPriceMetaData))]
    public partial class ProductPrice
    {
    }

    public partial class ProductPriceMetaData
    {
        [Required]
        [DisplayName("Product")]
        public int ProductId { get; set; }
        [Required]
        [DisplayName("Currency Code")]
        public string CurrencyCode { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}