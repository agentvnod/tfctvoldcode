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
    
    public partial class ProgramSchedule
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProgramSchedule()
        {
            this.AuditTrail = new AuditTrail();
        }
    
        public int ProgramScheduleId { get; set; }
        public int ChannelId { get; set; }
        public string ShowName { get; set; }
        public Nullable<short> SortOrder { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public bool Deleted { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public string Blurb { get; set; }
    
        public AuditTrail AuditTrail { get; set; }
    
        public virtual Channel Channel { get; set; }
    }
}
