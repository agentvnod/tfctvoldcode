using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace IPTV2_Model
{

    [MetadataType(typeof(EpisodeCelebrityRoleMetaData))]
    public partial class EpisodeCelebrityRole
    {
    }

    public partial class EpisodeCelebrityRoleMetaData
    {
        public int CelebirtyRoleId { get; set; }
        [Required]
        [DisplayName("Cast")]
        public int CelebrityId { get; set; }
        [Required]
        [DisplayName("Role")]
        public int RoleTypeId { get; set; }
        [Required]
        [DisplayName("Episode")]
        public int EpisodeId { get; set; }

    }

    [MetadataType(typeof(ShowCelebrityRoleMetaData))]
    public partial class ShowCelebrityRole
    {
    }


    public partial class ShowCelebrityRoleMetaData
    {
        public int CelebirtyRoleId { get; set; }
        [Required]
        [DisplayName("Cast")]
        public int CelebrityId { get; set; }
        [Required]
        [DisplayName("Role")]
        public int RoleTypeId { get; set; }
        [Required]
        [DisplayName("Show")]
        public int CategoryId { get; set; }

    }

}