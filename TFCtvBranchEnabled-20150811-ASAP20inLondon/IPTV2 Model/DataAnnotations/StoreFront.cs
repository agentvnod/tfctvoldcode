using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;


namespace IPTV2_Model
{

    [MetadataType(typeof(StoreFrontMetaData))]
    public partial class StoreFront
    {
    }

    public partial class StoreFrontMetaData
    {

        public int StoreFrontId { get; set; }
        [DisplayName("Offering")]
        public int OfferingId { get; set; }
        [Required]
        [DisplayName("Business Name")]
        public string BusinessName { get; set; }
        [DisplayName("Contact Person")]
        public string ContactPerson { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        [DisplayName("Zip Code")]
        public string ZipCode { get; set; }
        [DisplayName("Country")]
        public string CountryCode { get; set; }
        [DisplayName("Business Phone")]
        public string BusinessPhone { get; set; }
        [DisplayName("Email Address")]
        public string EMailAddress { get; set; }
        [DisplayName("Web Site Url")]
        public string WebSiteUrl { get; set; }
        [DisplayName("Mobile Phone")]
        public string MobilePhone { get; set; }
        public Nullable<double> Latitude { get; set; }
        public Nullable<double> Longitude { get; set; }
        [DisplayName("Status")]
        public int StatusId { get; set; }
        
    }

}