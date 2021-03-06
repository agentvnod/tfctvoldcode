﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class IPTV2Entities : DbContext
    {
        public IPTV2Entities()
            : base("name=IPTV2Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AssetCdn> AssetCdns { get; set; }
        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<CategoryClass> CategoryClasses { get; set; }
        public virtual DbSet<CategoryRelationship> CategoryRelationships { get; set; }
        public virtual DbSet<CDN> CDNs { get; set; }
        public virtual DbSet<Channel> Channels { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<EpisodeAsset> EpisodeAssets { get; set; }
        public virtual DbSet<EpisodeCategory> EpisodeCategories1 { get; set; }
        public virtual DbSet<Episode> Episodes { get; set; }
        public virtual DbSet<PackageCategory> PackageCategories { get; set; }
        public virtual DbSet<PackageChannel> PackageChannels { get; set; }
        public virtual DbSet<PackageType> PackageTypes { get; set; }
        public virtual DbSet<ProgramSchedule> ProgramSchedules { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<Celebrity> Celebrities { get; set; }
        public virtual DbSet<CelebrityRole> CelebrityRoles { get; set; }
        public virtual DbSet<RoleType> RoleTypes { get; set; }
        public virtual DbSet<CategoryCountryRestriction> CategoryCountryRestrictions { get; set; }
        public virtual DbSet<Carousel> Carousels { get; set; }
        public virtual DbSet<CarouselSlide> CarouselSlides { get; set; }
        public virtual DbSet<ProductPrice> ProductPrices { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductPackage> ProductPackages { get; set; }
        public virtual DbSet<Offering> Offerings { get; set; }
        public virtual DbSet<ProductShow> ProductShows { get; set; }
        public virtual DbSet<ProductEpisode> ProductEpisodes { get; set; }
        public virtual DbSet<Entitlement> Entitlements { get; set; }
        public virtual DbSet<EntitlementRequest> EntitlementRequests { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Feature> Features { get; set; }
        public virtual DbSet<FeatureItem> FeatureItems { get; set; }
        public virtual DbSet<UserWallet> UserWallets { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<PurchaseItem> PurchaseItems { get; set; }
        public virtual DbSet<Purchase> Purchases { get; set; }
        public virtual DbSet<Ppc> Ppcs { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<GomsPaymentMethod> GomsPaymentMethods { get; set; }
        public virtual DbSet<PpcType> PpcTypes { get; set; }
        public virtual DbSet<BetaTester> BetaTesters { get; set; }
        public virtual DbSet<GomsSubsidiary> GomsSubsidiaries { get; set; }
        public virtual DbSet<StoreFront> StoreFronts { get; set; }
        public virtual DbSet<Forex> Forexes { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<PpcTypeCurrency> PpcTypeCurrencies { get; set; }
        public virtual DbSet<ProductCountryRestriction> ProductCountryRestrictions { get; set; }
        public virtual DbSet<ProductGroup> ProductGroups { get; set; }
        public virtual DbSet<ProductGroupUpgrade> ProductGroupUpgrades { get; set; }
        public virtual DbSet<GomsReference> GomsReferences { get; set; }
        public virtual DbSet<ChannelCountryRestriction> ChannelCountryRestrictions { get; set; }
        public virtual DbSet<ChannelCdn> ChannelCdns { get; set; }
        public virtual DbSet<RecurringBilling> RecurringBillings { get; set; }
        public virtual DbSet<CreditCard> CreditCards { get; set; }
        public virtual DbSet<PaypalIPNLog> PaypalIPNLogs { get; set; }
        public virtual DbSet<Promo> Promos { get; set; }
        public virtual DbSet<UserPromo> UserPromos { get; set; }
        public virtual DbSet<MopayButton> MopayButtons { get; set; }
        public virtual DbSet<MopayTransactionRequest> MopayTransactionRequests { get; set; }
        public virtual DbSet<SearchResult> SearchResults { get; set; }
        public virtual DbSet<CountryBitrate> CountryBitrates { get; set; }
        public virtual DbSet<ITEDetail> ITEDetails { get; set; }
        public virtual DbSet<PacMayLogs> PacMayLogs { get; set; }
        public virtual DbSet<CategoryRestrictions> CategoryRestrictions { get; set; }
        public virtual DbSet<OnlineEvent> OnlineEvents { get; set; }
    
        public virtual ObjectResult<Nullable<bool>> CheckCategoryIfGeoAllowed(Nullable<int> categoryId, string country, string region, string city, string zipCode)
        {
            var categoryIdParameter = categoryId.HasValue ?
                new ObjectParameter("categoryId", categoryId) :
                new ObjectParameter("categoryId", typeof(int));
    
            var countryParameter = country != null ?
                new ObjectParameter("country", country) :
                new ObjectParameter("country", typeof(string));
    
            var regionParameter = region != null ?
                new ObjectParameter("region", region) :
                new ObjectParameter("region", typeof(string));
    
            var cityParameter = city != null ?
                new ObjectParameter("city", city) :
                new ObjectParameter("city", typeof(string));
    
            var zipCodeParameter = zipCode != null ?
                new ObjectParameter("zipCode", zipCode) :
                new ObjectParameter("zipCode", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<bool>>("CheckCategoryIfGeoAllowed", categoryIdParameter, countryParameter, regionParameter, cityParameter, zipCodeParameter);
        }
    }
}
