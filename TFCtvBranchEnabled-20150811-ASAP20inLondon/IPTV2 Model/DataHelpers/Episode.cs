using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class Episode : IComparable<Episode>
    {
        public int CompareTo(Episode other)
        {
            return (EpisodeId.CompareTo(other.EpisodeId));
        }

        public SortedSet<EpisodeSubscriptionProduct> GetOnlineEpisodeProducts(Offering offering, String countryCode)
        {
            var products = new SortedSet<EpisodeSubscriptionProduct>();

            // check if at least one show is allowed for country
            bool allowedInCountry = false;
            foreach (var s in EpisodeCategories)
            {
                if (s.Show.IsOnlineAllowed(countryCode))
                {
                    allowedInCountry = true;
                    break;
                }
            }

            if (allowedInCountry)
            {
                var epProducts = Products.Where(p => (p.Product.StatusId == 1) & (p.Product.OfferingId == offering.OfferingId));

                foreach (var p in epProducts)
                {
                    products.Add(p.Product);
                }
            }
            return (products.Count > 0 ? products : null);
        }

        public SortedSet<EpisodeSubscriptionProduct> GetMobileEpisodeProducts(Offering offering, String countryCode)
        {
            var products = new SortedSet<EpisodeSubscriptionProduct>();

            // check if at least one show is allowed for country
            bool allowedInCountry = false;
            foreach (var s in EpisodeCategories)
            {
                if (s.Show.IsMobileAllowed(countryCode))
                {
                    allowedInCountry = true;
                    break;
                }
            }

            if (allowedInCountry)
            {
                var epProducts = Products.Where(p => (p.Product.StatusId == 1) & (p.Product.OfferingId == offering.OfferingId));

                foreach (var p in epProducts)
                {
                    products.Add(p.Product);
                }
            }
            return (products.Count > 0 ? products : null);
        }

        public SortedSet<EpisodeSubscriptionProduct> GetIptvEpisodeProducts(Offering offering, String countryCode)
        {
            var products = new SortedSet<EpisodeSubscriptionProduct>();

            // check if at least one show is allowed for country
            bool allowedInCountry = false;
            foreach (var s in EpisodeCategories)
            {
                if (s.Show.IsIptvAllowed(countryCode))
                {
                    allowedInCountry = true;
                    break;
                }
            }

            if (allowedInCountry)
            {
                var epProducts = Products.Where(p => (p.Product.StatusId == 1) & (p.Product.OfferingId == offering.OfferingId));

                foreach (var p in epProducts)
                {
                    products.Add(p.Product);
                }
            }
            return (products.Count > 0 ? products : null);
        }


        public SortedSet<ShowSubscriptionProduct> GetOnlineShowProducts(Offering offering, String countryCode)
        {
            var products = new SortedSet<ShowSubscriptionProduct>();

            foreach (var s in this.EpisodeCategories)
            {
                if (s.Show.StatusId == 1)
                {
                    var showProducts = s.Show.GetOnlineShowProducts(offering, countryCode);
                    if (showProducts != null)
                    {
                        products.UnionWith(showProducts);
                    }
                }
            }
            return (products.Count > 0 ? products : null);
        }

        public SortedSet<ShowSubscriptionProduct> GetMobileShowProducts(Offering offering, String countryCode)
        {
            var products = new SortedSet<ShowSubscriptionProduct>();

            foreach (var s in this.EpisodeCategories)
            {
                if (s.Show.StatusId == 1)
                {
                    var showProducts = s.Show.GetMobileShowProducts(offering, countryCode);
                    if (showProducts != null)
                    {
                        products.UnionWith(showProducts);
                    }
                }
            }
            return (products.Count > 0 ? products : null);

        }

        public SortedSet<ShowSubscriptionProduct> GetIptvShowProducts(Offering offering, String countryCode)
        {
            var products = new SortedSet<ShowSubscriptionProduct>();

            foreach (var s in this.EpisodeCategories)
            {
                if (s.Show.StatusId == 1)
                {
                    var showProducts = s.Show.GetIptvShowProducts(offering, countryCode);
                    if (showProducts != null)
                    {
                        products.UnionWith(showProducts);
                    }
                }
            }
            return (products.Count > 0 ? products : null);
        }

        public SortedSet<PackageSubscriptionProduct> GetOnlinePackageProducts(Offering offering, String countryCode)
        {
            var products = new SortedSet<PackageSubscriptionProduct>();

            foreach (var s in this.EpisodeCategories)
            {
                if (s.Show.StatusId == 1)
                {
                    var packageProducts = s.Show.GetOnlinePackageProducts(offering, countryCode);
                    if (packageProducts != null)
                    {
                        products.UnionWith(packageProducts);
                    }
                }
            }
            return (products.Count > 0 ? products : null);
        }

        public SortedSet<PackageSubscriptionProduct> GetMobilePackageProducts(Offering offering, String countryCode)
        {
            var products = new SortedSet<PackageSubscriptionProduct>();

            foreach (var s in this.EpisodeCategories)
            {
                if (s.Show.StatusId == 1)
                {
                    var packageProducts = s.Show.GetMobilePackageProducts(offering, countryCode);
                    if (packageProducts != null)
                    {
                        products.UnionWith(packageProducts);
                    }
                }
            }
            return (products.Count > 0 ? products : null);
        }

        public SortedSet<PackageSubscriptionProduct> GetIptvPackageProducts(Offering offering, String countryCode)
        {
            var products = new SortedSet<PackageSubscriptionProduct>();

            foreach (var s in this.EpisodeCategories)
            {
                if (s.Show.StatusId == 1)
                {
                    var packageProducts = s.Show.GetIptvPackageProducts(offering, countryCode);
                    if (packageProducts != null)
                    {
                        products.UnionWith(packageProducts);
                    }
                }
            }
            return (products.Count > 0 ? products : null);
        }

        public bool HasHlsPremiumAssets()
        {
            var hlsAssets = from a in PremiumAssets
                            from ac in a.Asset.AssetCdns
                            where ac.CdnId == 2
                            select ac;

            return (hlsAssets.Count() > 0);
        }

        public int GetStatusId(RightsType rightsType)
        {
            int statusId = 0;
            switch (rightsType)
            {
                case RightsType.Online:
                    statusId = OnlineStatusId;
                    break;
                case RightsType.Mobile:
                    statusId = MobileStatusId;
                    break;
                case RightsType.IPTV:
                    statusId = StatusId;
                    break;
                case RightsType.IPTV3:
                    statusId = Iptv3StatusId;
                    break;
                default:
                    throw new Exception("Invalid rightstype.");
            }
            return (statusId);
        }

        public SortedSet<int> GetParentShows(TimeSpan? CacheDuration = null)
        {
            SortedSet<int> parentShowList = null;
            var cache = DataCache.Cache;
            string cacheKey = "GAPRNTSHO:Epi:" + EpisodeId;
            parentShowList = (SortedSet<int>)cache[cacheKey];
            if (parentShowList == null)
            {
                parentShowList = new SortedSet<int>();
                var categoryParentIds = EpisodeCategories.Select(e => e.CategoryId);
                parentShowList.UnionWith(categoryParentIds);
                cache.Put(cacheKey, parentShowList, CacheDuration != null ? (TimeSpan)CacheDuration : DataCache.CacheDuration);
            }
            return (parentShowList);
        }

    }
}
