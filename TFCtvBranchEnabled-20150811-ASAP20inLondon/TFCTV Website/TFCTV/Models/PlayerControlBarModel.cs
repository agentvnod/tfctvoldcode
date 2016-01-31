using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFCTV.Models
{
    public class PlayerControlBarModel
    {
        public bool IsUserEntitled { get; set; }
        public string TFCTvDownloadPlayerFullUrl { get; set; }
        public int EpisodeId { get; set; }
        public bool? IsUsingSmallPlayer { get; set; }
        public IPTV2_Model.Episode Episode { get; set; }
        public bool HasHD { get; set; }
        public bool HasSD { get; set; }
        public bool IgnoreCheckForMobileDevices { get; set; }
        public bool IsDeviceMp4Capable { get; set; }
    }
}