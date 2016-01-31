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
    
    public abstract partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.ProductPrices = new HashSet<ProductPrice>();
            this.EntitlementRequests = new HashSet<EntitlementRequest>();
            this.PpcPaymentTransactions = new HashSet<PpcPaymentTransaction>();
            this.AllowedCountries = new HashSet<ProductAllowedCountry>();
            this.BlockedCountries = new HashSet<ProductBlockedCountry>();
            this.MigrationTransactions = new HashSet<MigrationTransaction>();
            this.RecurringBillings = new HashSet<RecurringBilling>();
            this.AuditTrail = new AuditTrail();
        }
    
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OfferingId { get; set; }
        public byte StatusId { get; set; }
        public Nullable<int> GomsProductId { get; set; }
        public Nullable<int> GomsProductQuantity { get; set; }
        public bool IsForSale { get; set; }
        public Nullable<System.DateTime> BreakingDate { get; set; }
        public Nullable<int> IsRecurring { get; set; }
        public Nullable<int> RegularProductId { get; set; }
    
        public AuditTrail AuditTrail { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductPrice> ProductPrices { get; set; }
        public virtual Offering Offering { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EntitlementRequest> EntitlementRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PpcPaymentTransaction> PpcPaymentTransactions { get; set; }
        public virtual Status Status { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductAllowedCountry> AllowedCountries { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductBlockedCountry> BlockedCountries { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MigrationTransaction> MigrationTransactions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecurringBilling> RecurringBillings { get; set; }
    }
}