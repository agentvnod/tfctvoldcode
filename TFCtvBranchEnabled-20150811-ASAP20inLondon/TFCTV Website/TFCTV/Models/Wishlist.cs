using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TFCTV.Models
{
    public class Wishlist
    {
        [Required]
        [Display(Name = "UID")]
        public string UID_s { get; set; }

        [Required]
        [Display(Name = "Date")]
        public string registDt_d { get; set; }

        [Required]
        [Display(Name = "ProductId")]
        public int? ProductId_i { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName_s { get; set; }
    }

    public class WishlistModel
    {
        public string type { get; set; }

        public Wishlist data { get; set; }
    }
}