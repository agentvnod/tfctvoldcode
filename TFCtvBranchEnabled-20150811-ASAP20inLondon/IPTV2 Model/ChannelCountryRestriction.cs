//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IPTV2_Model
{
    using System;
    using System.Collections.Generic;
    
    public abstract partial class ChannelCountryRestriction
    {
        public int ChannelRestrictionId { get; set; }
        public string CountryCode { get; set; }
        public int ChannelId { get; set; }
        public byte StatusId { get; set; }
        public Nullable<System.Guid> CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<System.Guid> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
    }
}