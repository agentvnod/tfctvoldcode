using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace IPTV2_Model
{

    [MetadataType(typeof(CelebrityMetaData))]
    public partial class Celebrity
    {
    }

    public partial class CelebrityMetaData
    {

        public int CelebrityId { get; set; }
        [DisplayName("Individual")]
        public bool IsIndividual { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [Required]
        [DisplayName("Full Name")]
        public string FullName { get; set; }
        public string Description { get; set; }
        public string Birthplace { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Birthday { get; set; }
        [DisplayName("Zodiac Sign")]
        public string ZodiacSign { get; set; }
        [DisplayName("Chinese Year")]
        public string ChineseYear { get; set; }
        [Required]
        [DisplayName("Status")]
        public byte StatusId { get; set; }
        [DisplayName("Image")]
        public byte ImageUrl { get; set; }

    }

}