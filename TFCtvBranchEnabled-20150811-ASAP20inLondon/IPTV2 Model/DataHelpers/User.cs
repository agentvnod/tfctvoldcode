using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    partial class User : IComparable<User>
    {
        /// <summary>
        /// Compare if the user is equal to, less than or greater than another userr
        /// </summary>
        /// <param name="other">user to compare this user to</param>
        /// <returns></returns>
        public int CompareTo(User other)
        {
            return (UserId.CompareTo(other.UserId));
        }

        /// <summary>
        /// Determine if user is already registered in GOMS
        /// </summary>
        public bool IsGomsRegistered
        {
            get { return (GomsCustomerId != null && GomsCustomerId != 0); }
        }

        /// <summary>
        /// Check if user has a pending ChangeCountryTransaction in GOMS
        /// </summary>
        /// <param name="offering">Offering to check agains</param>
        /// <returns>True if a ChangeCountryTransaction is not yet updated in GOMS</returns>
        public bool HasPendingGomsChangeCountryTransaction(Offering offering)
        {
            //var trans = Transactions.OfType<ChangeCountryTransaction>().Where(t => t.OfferingId == offering.OfferingId && (t.GomsTransactionId == null || t.GomsTransactionId == 0));
            //var c = Transactions.OfType<ChangeCountryTransaction>().Count(t => t.OfferingId == offering.OfferingId && (t.GomsTransactionId == null || t.GomsTransactionId == 0));
            return Transactions.OfType<ChangeCountryTransaction>().Count(t => t.OfferingId == offering.OfferingId && (t.GomsTransactionId == null || t.GomsTransactionId == 0)) > 0;
        }

        /// <summary>
        /// Check if user has a pending transactions in GOMS
        /// </summary>
        /// <param name="offering">Offering to check agains</param>
        /// <returns>True if a ChangeCountryTransaction is not yet updated in GOMS</returns>
        public bool HasOtherPendingGomsTransaction(Offering offering)
        {
            //var trans = Transactions.OfType<ChangeCountryTransaction>().Where(t => t.OfferingId == offering.OfferingId && (t.GomsTransactionId == null || t.GomsTransactionId == 0));
            //var c = Transactions.OfType<ChangeCountryTransaction>().Count(t => t.OfferingId == offering.OfferingId && (t.GomsTransactionId == null || t.GomsTransactionId == 0));
            return Transactions.Count(t => t.GetType() != typeof(ChangeCountryTransaction) && t.OfferingId == offering.OfferingId && (t.GomsTransactionId == null || t.GomsTransactionId == 0) && t.Date < DateTime.Now) > 0;
        }

        /// <summary>
        /// Determine if user can play a specific channel
        /// </summary>
        /// <param name="offering">offering to check rights against</param>        
        /// <param name="channel">channel where to check for playback rights</param>        
        /// <param name="rightsType">type of right/platform to check agains(iptv, online or mobile)</param>
        /// <returns>True - can play</returns>
        public bool CanPlayLiveStream(Offering offering, Channel channel, RightsType rightsType)
        {
            return (IsChannelEntitled(offering, channel, rightsType));
        }

        /// <summary>
        /// Determine if user can play a specific video
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="episode">episode where video to check for playback rights belongs to</param>
        /// <param name="asset">specific episode asset/video that is checked for playback rights</param>
        /// <param name="rightsType">type of right/platform to check agains(iptv, online or mobile)</param>
        /// <returns>True - can play</returns>
        public bool CanPlayVideo(Offering offering, Episode episode, Asset asset, RightsType rightsType)
        {
            return (IsEpisodeEntitled(offering, episode, asset, rightsType));
        }

        /// <summary>
        /// Check if the specific episode/asset/rightsType combination is available for playback
        /// </summary>
        /// <param name="episode">episode where video/assset belongs to</param>
        /// <param name="asset">specific episode asset for checking</param>
        /// <param name="rightsType">type of right/platform to check agains(iptv, online or mobile)</param>
        /// <returns>True - episode is available for specific rightsTye</returns>
        public bool ValidateEpisodeAsset(Episode episode, Asset asset, RightsType rightsType)
        {
            return ((episode.GetStatusId(rightsType) == 1) && (episode.AllAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null));
        }

        /// <summary>
        /// Check if Anonymous User is entitled to a specific asset
        /// </summary>
        /// <param name="context">DB Context to use</param>
        /// <param name="offeringId">offering to check rights against</param>
        /// <param name="packageId">package to check rights against</param>
        /// <param name="episodeId">episode where the asset belongs to</param>
        /// <param name="assetId">specific asset to check rights of</param>
        /// <param name="countryCode">country where entitlement is to be checked against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <returns>True - anonymous user is entitled to this asset</returns>
        static public bool IsAssetEntitled(IPTV2Entities context, int offeringId, int packageId, int episodeId, int assetId, String countryCode, RightsType rightsType)
        {
            bool canPlay = false;

            var offering = context.Offerings.Find(offeringId);
            var package = offering.Packages.FirstOrDefault(p => p.PackageId == packageId);
            var episode = context.Episodes.Find(episodeId);

            if ((package != null) && (episode != null) && (package.StatusId == 1) && (episode.GetStatusId(rightsType) == 1))
            {
                // check if free clip
                canPlay = (episode.FreeAssets.FirstOrDefault(e => e.AssetId == assetId) != null);

                // check if preview clip
                if (!canPlay)
                {
                    canPlay = (episode.PreviewAssets.FirstOrDefault(e => e.AssetId == assetId) != null);
                }

                // check access for premium content
                if (!canPlay)
                {
                    canPlay = (episode.PremiumAssets.FirstOrDefault(e => e.AssetId == assetId) != null) && (IsEpisodeEntitled(context, offeringId, packageId, episodeId, countryCode, rightsType));
                }
            }

            return (canPlay);
        }

        /// <summary>
        /// Check if Anonymous User is entitled to a specific episode
        /// </summary>
        /// <param name="context">DB Context to use</param>
        /// <param name="offeringId">offering to check rights against</param>
        /// <param name="packageId">package to check rights against</param>
        /// <param name="episodeId">episode to check rights against</param>
        /// <param name="countryCode">country where entitlement is to be checked against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <returns>True - anonymous user is entitled to this asset</returns>
        static public bool IsEpisodeEntitled(IPTV2Entities context, int offeringId, int packageId, int episodeId, String countryCode, RightsType rightsType)
        {
            bool canPlay = false;

            var offering = context.Offerings.Find(offeringId);
            var package = offering.Packages.FirstOrDefault(p => p.PackageId == packageId);
            var episode = context.Episodes.Find(episodeId);

            if ((package != null) && (episode != null) && (package.StatusId == 1) && (episode.GetStatusId(rightsType) == 1))
            {
                foreach (var s in episode.EpisodeCategories)
                {
                    canPlay = IsShowEntitled(context, offeringId, packageId, s.CategoryId, countryCode, rightsType);
                    if (canPlay)
                    {
                        break;
                    }
                }
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if Anonymous User is entitled to a specific show
        /// </summary>
        /// <param name="context">DB Context to use</param>
        /// <param name="offeringId">offering to check rights against</param>
        /// <param name="packageId">package to check rights against</param>
        /// <param name="showId"></param>
        /// <param name="countryCode">country where entitlement is to be checked against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <returns>True - anonymous user is entitled to this asset</returns>
        static public bool IsShowEntitled(IPTV2Entities context, int offeringId, int packageId, int showId, String countryCode, RightsType rightsType)
        {
            bool canPlay = false;

            var offering = context.Offerings.Find(offeringId);
            var package = offering.Packages.FirstOrDefault(p => p.PackageId == packageId && p.StatusId == 1);

            if (package != null)
            {
                var packageShowList = package.GetAllShowIds(countryCode, rightsType);
                canPlay = packageShowList.Contains(showId);
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is Entitled to a specific episode
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="episode">episode where video to check for playback rights belongs to</param>
        /// <param name="asset">specific episode asset/video that is checked for playback rights</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <returns>True - user has rights for the episode</returns>
        public bool IsEpisodeEntitled(Offering offering, Episode episode, Asset asset, RightsType rightsType)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;

            if (ValidateEpisodeAsset(episode, asset, rightsType))
            {
                // check if in preview assets
                canPlay = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);

                if (!canPlay)
                {
                    // check if in free assets
                    canPlay = (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                }

                if (!canPlay)
                {
                    // Check episode entitlement
                    canPlay = (EpisodeEntitlements.FirstOrDefault(c => (c.OfferingId == offering.OfferingId) && (c.EpisodeId == episode.EpisodeId) && ((c.EndDate >= currentDate))) != null);
                }

                if (!canPlay)
                {
                    // check show entitlement
                    canPlay = IsShowEntitled(offering, episode, asset, rightsType);
                }
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is entitled to specific show
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="episode">episode where video to check for playback rights belongs to</param>
        /// <param name="asset">specific episode asset/video that is checked for playback rights</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <returns>True - user has rights for the show</returns>
        public bool IsShowEntitled(Offering offering, Episode episode, Asset asset, RightsType rightsType)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;
            foreach (var s in episode.EpisodeCategories)
            {
                canPlay = IsShowEntitled(offering, s.Show, rightsType);
                if (canPlay)
                {
                    break;
                }
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is entitled to a specific show
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="show">show to check rights against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <returns>True - user has rights for the show</returns>
        public bool IsShowEntitled(Offering offering, Show show, RightsType rightsType)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;
            canPlay = (ShowEntitlements.FirstOrDefault(se => (se.OfferingId == offering.OfferingId) & (se.EndDate >= currentDate) & (se.CategoryId == show.CategoryId) & (show.StatusId == 1)) != null);
            if (!canPlay)
            {
                canPlay = IsPackageEntitled(offering, show, rightsType);
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is entitled to package, given a show
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="show">show to check rights against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <returns>True - user has rights for the show</returns>
        private bool IsPackageEntitled(Offering offering, Show show, RightsType rightsType)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;

            foreach (var p in PackageEntitlements)
            {
                // validate if package has not yet expired
                if ((p.OfferingId == offering.OfferingId) & (p.EndDate >= currentDate))
                {
                    SortedSet<int> packageShowList = p.Package.GetAllShowIds(CountryCode, rightsType);
                    canPlay = packageShowList.Contains(show.CategoryId);
                }

                if (canPlay)
                {
                    break;
                }
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is entitled to a specific channel
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="channel">channel to check rights against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <returns>True - user has rights for the channel</returns>
        public bool IsChannelEntitled(Offering offering, Channel channel, RightsType rightsType)
        {
            bool canPlay = false;
            canPlay = IsPackageEntitled(offering, channel, rightsType);
            return (canPlay);
        }


        /// <summary>
        /// Check if user is entitled to package, given a channel
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="channel">channel to check rights against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <returns>True - user has rights for the channel</returns>
        private bool IsPackageEntitled(Offering offering, Channel channel, RightsType rightsType)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;

            foreach (var p in PackageEntitlements)
            {
                // validate if package has not yet expired
                if ((p.OfferingId == offering.OfferingId) & (p.EndDate >= currentDate))
                {
                    SortedSet<int> packageChannelList = p.Package.GetAllChannelIds(CountryCode, rightsType);
                    canPlay = packageChannelList.Contains(channel.ChannelId);
                }

                if (canPlay)
                {
                    break;
                }
            }
            return (canPlay);

        }

        private DateTime AddDate(DateTime startDate, string durationType, int duration)
        {
            var endDate = startDate;
            switch (durationType.ToUpper())
            {
                case "D":
                    {
                        endDate = startDate.AddDays(duration);
                        break;
                    }
                case "M":
                    {
                        endDate = startDate.AddMonths(duration);
                        break;
                    }
                case "Y":
                    {
                        endDate = startDate.AddYears(duration);
                        break;
                    }
                default:
                    {
                        throw new Exception("Invalid duration type.");
                    }
            }
            return (endDate);
        }

        /// <summary>
        /// Entitle user to a particular episode
        /// </summary>
        /// <param name="req">Entitlement Request</param>
        /// <param name="episode">Episode to be entitled for</param>
        /// <param name="offering">Offering where user is to be entitled to</param>
        /// <param name="durationType">Type of duration to grant entitlement for: D-ays, M-onths, Y-ears</param>
        /// <param name="duration">Amount of DurationType to grant entitlement for</param>
        /// <returns>Granted EpisodeEntitlement details</returns>
        public EpisodeEntitlement EntitleEpisode(EntitlementRequest req, Episode episode, Offering offering, string durationType, int duration)
        {
            var entitlement = this.EpisodeEntitlements.SingleOrDefault(ep => ep.EpisodeId == episode.EpisodeId);
            var startDate = DateTime.Now;
            if (entitlement == null)
            {
                entitlement = new EpisodeEntitlement
                {
                    Episode = episode,
                    Offering = offering,
                    LatestEntitlementRequest = req,
                };
                this.EpisodeEntitlements.Add(entitlement);
            }
            else
            {
                entitlement.LatestEntitlementRequest = req;
                startDate = (startDate < entitlement.EndDate) ? entitlement.EndDate : startDate;
            }
            entitlement.EndDate = AddDate(startDate, durationType, duration);
            return (entitlement);
        }

        /// <summary>
        /// Entitle user to all episodes of an EpisodeSubscriptionProduct
        /// </summary>
        /// <param name="req">Entitlement Request</param>
        /// <param name="product">EpisodeSubscriptionProduct to grant entitlement for</param>
        /// <returns>List of EpisodeEntitlements granted </returns>
        public List<Entitlement> EntitleEpisodeSubscriptionProduct(EntitlementRequest req, EpisodeSubscriptionProduct product)
        {
            var entitlements = new List<Entitlement>();
            foreach (var e in product.Episodes)
            {
                entitlements.Add(EntitleEpisode(req, e.Episode, product.Offering, product.DurationType, product.Duration));
            }
            return (entitlements);
        }

        /// <summary>
        /// Entitle user to a particular show
        /// </summary>
        /// <param name="req">Entitlement Request</param>
        /// <param name="show">Show to be entitled for</param>
        /// <param name="offering">Offering where user is to be entitled to</param>
        /// <param name="durationType">Type of duration to grant entitlement for: D-ays, M-onths, Y-ears</param>
        /// <param name="duration">Amount of DurationType to grant entitlement for</param>
        /// <returns>Granted ShowEntitlement details</returns>
        public ShowEntitlement EntitleShow(EntitlementRequest req, Show show, Offering offering, string durationType, int duration)
        {
            var entitlement = this.ShowEntitlements.SingleOrDefault(e => e.CategoryId == show.CategoryId);
            var startDate = DateTime.Now;
            if (entitlement == null)
            {
                entitlement = new ShowEntitlement
                {
                    Show = show,
                    Offering = offering,
                    LatestEntitlementRequest = req,
                };
                this.ShowEntitlements.Add(entitlement);
            }
            else
            {
                entitlement.LatestEntitlementRequest = req;
                startDate = (startDate < entitlement.EndDate) ? entitlement.EndDate : startDate;
            }
            entitlement.EndDate = AddDate(startDate, durationType, duration);
            return (entitlement);
        }

        /// <summary>
        /// Entitle user to all shows of a ShowSubscriptionProduct
        /// </summary>
        /// <param name="req">Entitle Request</param>
        /// <param name="product">ShowSubscriptionProduct to grant entitlement for</param>
        /// <returns>List of ShowEntitlements granted</returns>
        public List<Entitlement> EntitleShowSubscriptionProduct(EntitlementRequest req, ShowSubscriptionProduct product)
        {
            var entitlements = new List<Entitlement>();
            foreach (var c in product.Categories)
            {
                entitlements.Add(EntitleShow(req, c.Show, product.Offering, product.DurationType, product.Duration));
            }
            return (entitlements);
        }

        /// <summary>
        /// Entitle user to a pacticular package
        /// </summary>
        /// <param name="req">Entitlement Request</param>
        /// <param name="package">Package to be entitled for</param>
        /// <param name="offering">Offering where user is to be entitled to</param>
        /// <param name="durationType">Type of duration to grant entitlement for: D-ays, M-onths, Y-ears</param>
        /// <param name="duration">Amount of DurationType to grant entitlement for</param>
        /// <returns>Granted PackageEntitlement details</returns>
        public PackageEntitlement EntitlePackage(EntitlementRequest req, Package package, Offering offering, string durationType, int duration)
        {
            var entitlement = this.PackageEntitlements.SingleOrDefault(pe => pe.PackageId == package.PackageId);
            var startDate = DateTime.Now;
            if (entitlement == null)
            {
                entitlement = new PackageEntitlement
                {
                    Package = package,
                    Offering = offering,
                    LatestEntitlementRequest = req,
                };
                this.PackageEntitlements.Add(entitlement);
            }
            else
            {
                entitlement.LatestEntitlementRequest = req;
                startDate = (startDate < entitlement.EndDate) ? entitlement.EndDate : startDate;
            }
            entitlement.EndDate = AddDate(startDate, durationType, duration);
            return (entitlement);
        }

        /// <summary>
        /// Entitle user to all packages of a PackageSubscriptionProduct
        /// </summary>
        /// <param name="req">Entitlement Request</param>
        /// <param name="product">PackageSubscriptionProduct to grant entitlement for</param>
        /// <returns>List of PackageEntitlements granted</returns>
        public List<Entitlement> EntitlePackageSubscriptionProduct(EntitlementRequest req, PackageSubscriptionProduct product)
        {
            var entitlements = new List<Entitlement>();
            foreach (var p in product.Packages)
            {
                entitlements.Add(EntitlePackage(req, (Package)p.Package, product.Offering, product.DurationType, product.Duration));
            }
            return (entitlements);
        }

        /// <summary>
        /// Entitle user based on details of a Subscriptionproduct
        /// </summary>
        /// <param name="req">Entitlement Request</param>
        /// <param name="product">SubscriptionProduct to be entitled for</param>
        /// <returns>List of Entitlements granted</returns>
        public List<Entitlement> EntitleProduct(EntitlementRequest req, SubscriptionProduct product)
        {
            List<Entitlement> entitlements = null;
            if (product != null && product is SubscriptionProduct)
            {
                if (product is EpisodeSubscriptionProduct)
                {
                    entitlements = EntitleEpisodeSubscriptionProduct(req, (EpisodeSubscriptionProduct)product);
                }
                else if (product is ShowSubscriptionProduct)
                {
                    entitlements = EntitleShowSubscriptionProduct(req, (ShowSubscriptionProduct)product);
                }
                else if (product is PackageSubscriptionProduct)
                {
                    entitlements = EntitlePackageSubscriptionProduct(req, (PackageSubscriptionProduct)product);
                }
                else
                {
                    // throw new Exception("Invalid subscription product type.");
                    // ignore product type
                }
            }
            else
            {
                // throw new Exception("Invalid Entitlement Request.");
            }
            return (entitlements);
        }

        /// <summary>
        /// Process an EntitlementRequest, granting entitlements based on all the SubscriptionProducts that are part of the request
        /// </summary>
        /// <param name="entitlementRequest">Entitlement Request</param>
        /// <returns>List of Entitlements granted</returns>
        public List<Entitlement> ProcessEntitlementRequest(EntitlementRequest entitlementRequest)
        {
            List<Entitlement> entitlements = null;

            // process product
            var product = entitlementRequest.Product;
            if (product != null && product is SubscriptionProduct)
            {
                entitlements = EntitleProduct(entitlementRequest, (SubscriptionProduct)product);
            }

            return (entitlements);
        }

        /// <summary>
        /// Get the user's list of active subscribed ProductGroups
        /// </summary>
        /// <param name="offering">Offering to get subscription details from</param>
        /// <returns>List of ProductGroups user has active subscriptions to</returns>
        public List<ProductGroup> GetSubscribedProductGroups(Offering offering)
        {
            List<ProductGroup> productGroups = new List<ProductGroup>();

            var subscribedProducts = GetSubscribedProducts(offering);
            foreach (var p in subscribedProducts)
            {
                if (!productGroups.Contains(p.ProductGroup))
                {
                    productGroups.Add(p.ProductGroup);
                }
            }
            return (productGroups);
        }

        public class ProductGroupEntitlement
        {
            public int CompareTo(ProductGroupEntitlement other)
            {
                return (ProductGroup.ProductGroupId.CompareTo(other.ProductGroup.ProductGroupId));
            }
            public ProductGroup ProductGroup;
            public Entitlement Entitlement;
            public ProductGroupEntitlement(ProductGroup p, Entitlement e)
            {
                ProductGroup = p;
                Entitlement = e;
            }
        }


        public class ProductEntitlement
        {
            public int CompareTo(ProductEntitlement other)
            {
                return (Entitlement.EntitlementId.CompareTo(other.Entitlement.EntitlementId));
            }

            public SubscriptionProduct Product;
            public Entitlement Entitlement;

            public ProductEntitlement(SubscriptionProduct p, Entitlement e)
            {
                Product = p;
                Entitlement = e;
            }
        }


        /// <summary>
        /// Get the user's list of active productGroupEntitlements
        /// </summary>
        /// <param name="offering">Offering to get subscription details from</param>
        /// <returns>List of ProductGroupEntitlements user has</returns>
        public List<ProductGroupEntitlement> GetProductGroupEntitlements(Offering offering)
        {
            List<ProductGroupEntitlement> productGroupEntitlements = new List<ProductGroupEntitlement>();

            var productEntitlements = GetProductEntitlements(offering);
            foreach (var pe in productEntitlements)
            {
                var pge = productGroupEntitlements.FirstOrDefault(p => p.ProductGroup.ProductGroupId == pe.Product.ProductGroupId);
                if (pge == null)
                {
                    productGroupEntitlements.Add(new ProductGroupEntitlement(pe.Product.ProductGroup, pe.Entitlement));
                }
                else
                {
                    if (pe.Entitlement.EndDate > pge.Entitlement.EndDate)
                    {
                        pge.Entitlement = pe.Entitlement;
                    }
                }
            }
            return (productGroupEntitlements);
        }

        /// <summary>
        /// Get user's list of active product entitlements
        /// </summary>
        /// <param name="offering">Offering to get subscription details from</param>
        /// <returns>List of user's active product entitlements</returns>
        public List<ProductEntitlement> GetProductEntitlements(Offering offering)
        {
            List<ProductEntitlement> productEntitlements = new List<ProductEntitlement>();
            var activeEntitlements = Entitlements.Where(e => e.OfferingId == offering.OfferingId && e.EndDate > DateTime.Now);
            foreach (var e in activeEntitlements)
            {
                if (e.LatestEntitlementRequestId != null)
                {
                    if (e.LatestEntitlementRequest.Product is SubscriptionProduct)
                    {
                        productEntitlements.Add(new ProductEntitlement((SubscriptionProduct)e.LatestEntitlementRequest.Product, e));
                    }
                }
                else
                {
                    // just default to first found product
                    SubscriptionProduct defaultProduct = null;
                    if (e is PackageEntitlement)
                    {
                        var pe = (PackageEntitlement)e;
                        var packageProduct = pe.Package.Products.OrderBy(p => p.Product.Duration).FirstOrDefault();
                        if (packageProduct != null)
                        {
                            defaultProduct = packageProduct.Product;
                        }
                    }
                    else if (e is ShowEntitlement)
                    {
                        var se = (ShowEntitlement)e;
                        var showProduct = se.Show.Products.OrderBy(p => p.Product.Duration).FirstOrDefault();
                        if (showProduct != null)
                        {
                            defaultProduct = showProduct.Product;
                        }
                    }
                    else if (e is EpisodeEntitlement)
                    {
                        var ee = (EpisodeEntitlement)e;
                        var episodeProduct = ee.Episode.Products.OrderBy(p => p.Product.Duration).FirstOrDefault();
                        if (episodeProduct != null)
                        {
                            defaultProduct = episodeProduct.Product;
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid entitlement type.");
                    }

                    if (defaultProduct != null)
                        productEntitlements.Add(new ProductEntitlement(defaultProduct, e));
                }
            }

            return (productEntitlements);
        }

        /// <summary>
        /// Get user's list of active Subscriptions
        /// </summary>
        /// <param name="offering">Offering to get subscription details from</param>
        /// <returns>List of user's active SubscriptionProducts</returns>
        public List<SubscriptionProduct> GetSubscribedProducts(Offering offering)
        {
            List<SubscriptionProduct> products = new List<SubscriptionProduct>();
            var activeEntitlements = Entitlements.Where(e => e.OfferingId == offering.OfferingId && e.EndDate > DateTime.Now);
            foreach (var e in activeEntitlements)
            {
                if (e.LatestEntitlementRequestId != null)
                {
                    if (e.LatestEntitlementRequest.Product is SubscriptionProduct)
                    {
                        products.Add((SubscriptionProduct)e.LatestEntitlementRequest.Product);
                    }
                }
                else
                {
                    // just default to first found product
                    SubscriptionProduct defaultProduct = null;
                    if (e is PackageEntitlement)
                    {
                        var pe = (PackageEntitlement)e;
                        var packageProduct = pe.Package.Products.OrderBy(p => p.Product.Duration).FirstOrDefault();
                        if (packageProduct != null)
                        {
                            defaultProduct = packageProduct.Product;
                        }
                    }
                    else if (e is ShowEntitlement)
                    {
                        var se = (ShowEntitlement)e;
                        var showProduct = se.Show.Products.OrderBy(p => p.Product.Duration).FirstOrDefault();
                        if (showProduct != null)
                        {
                            defaultProduct = showProduct.Product;
                        }
                    }
                    else if (e is EpisodeEntitlement)
                    {
                        var ee = (EpisodeEntitlement)e;
                        var episodeProduct = ee.Episode.Products.OrderBy(p => p.Product.Duration).FirstOrDefault();
                        if (episodeProduct != null)
                        {
                            defaultProduct = episodeProduct.Product;
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid entitlement type.");
                    }

                    if (defaultProduct != null)
                        products.Add(defaultProduct);
                }
            }

            return (products);
        }


        public DateTime GetProductGroupExpiration(ProductGroup productGroup)
        {
            DateTime packageExpiration = DateTime.MinValue;
            DateTime showExpiration = DateTime.MinValue;

            var packageIds = productGroup.GetPackageIds();
            var packageEntitlements = from entitlement in PackageEntitlements
                                      join id in packageIds
                                      on entitlement.PackageId equals id
                                      where entitlement.EndDate > DateTime.Now
                                      select entitlement;
            if (packageEntitlements.Count() > 0)
            {
                packageExpiration = packageEntitlements.Max(e => e.EndDate);
            }

            var showIds = productGroup.GetShowIds();
            var showEntitlements = from entitlement in ShowEntitlements
                                   join id in showIds
                                   on entitlement.CategoryId equals id
                                   where entitlement.EndDate > DateTime.Now
                                   select entitlement;
            if (showEntitlements.Count() > 0)
            {
                showExpiration = showEntitlements.Max(e => e.EndDate);
            }

            return (packageExpiration > showExpiration ? packageExpiration : showExpiration);
        }

        public bool IsFirstTimeSubscriber(Offering offering, bool CheckViaEntitlementRequest = false, IEnumerable<int> freeTrialPackageIds = null, IPTV2Entities context = null)
        {
            try
            {
                if (CheckViaEntitlementRequest)
                {
                    try
                    {
                        if (Entitlements.Count() > 0)
                        {
                            var entitlementRequestId = Entitlements.OrderByDescending(e => e.EndDate).First().LatestEntitlementRequestId;
                            var productId = context.EntitlementRequests.FirstOrDefault(e => e.EntitlementRequestId == entitlementRequestId).ProductId;
                            var freeTrialProductIds = context.ProductPackages.Where(p => freeTrialPackageIds.Contains(p.PackageId)).Select(p => p.ProductId);
                            if (freeTrialProductIds.Contains(productId))
                                return true;
                            else
                                return false;
                        }
                        else
                            return false; // First Time Registrant
                    }
                    catch (Exception) // If it fails, check for payment transaction instead
                    {
                        return !(Transactions.OfType<PaymentTransaction>().Count(t => t.OfferingId == offering.OfferingId && t.Amount > 0) > 0);
                    }

                }
                else
                    return !(Transactions.OfType<PaymentTransaction>().Count(t => t.OfferingId == offering.OfferingId && t.Amount > 0) > 0);
            }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// Get the user's list of active recurring ProductGroups
        /// </summary>
        /// <param name="offering">Offering to get subscription details from</param>
        /// <returns>List of ProductGroups user has recurring subscriptions to</returns>
        public List<ProductGroup> GetRecurringProductGroups(Offering offering)
        {
            List<ProductGroup> productGroups = new List<ProductGroup>();
            var subscribedProducts = GetRecurringProducts(offering);
            foreach (var p in subscribedProducts)
            {
                if (!productGroups.Contains(p.ProductGroup))
                {
                    productGroups.Add(p.ProductGroup);
                }
            }
            return (productGroups);
        }

        /// <summary>
        /// Get user's list of recurring SubscriptionProducts
        /// </summary>        
        /// <returns>List of user's recurring SubscriptionProducts</returns>
        public List<SubscriptionProduct> GetRecurringProducts(Offering offering)
        {
            List<SubscriptionProduct> products = new List<SubscriptionProduct>();
            var recurringProducts = RecurringBillings.Where(r => r.StatusId == 1 && r.OfferingId == offering.OfferingId);
            foreach (var e in recurringProducts)
            {
                if (e.Product is SubscriptionProduct)
                    products.Add((SubscriptionProduct)e.Product);
            }
            return (products);
        }

        public bool HasTVEverywhereEntitlement(IEnumerable<int> packageIds, Offering offering)
        {
            var currentDate = DateTime.Now;
            return PackageEntitlements.Count(p => packageIds.Contains(p.PackageId) & p.EndDate > currentDate & p.Offering == offering) > 0;
        }

        /// <summary>
        /// Check if user has active recurring billing
        /// </summary>
        /// <param name="offering"></param>
        /// <returns></returns>
        public bool HasActiveRecurringProducts(Offering offering)
        {
            var currentDate = DateTime.Now;
            return RecurringBillings.Count(p => p.EndDate > currentDate & p.Offering == offering && p.StatusId == 1) > 0;
        }

        /// <summary>
        /// Check if user is entitled to package, given a show
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="show">show to check rights against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <param name="TVEverywherePackageId">TV Everywhere PackageId</param>
        /// <returns>True - user has rights for the show</returns>
        private bool IsPackageEntitled(Offering offering, Show show, RightsType rightsType, int TVEverywherePackageId)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;

            foreach (var p in PackageEntitlements)
            {
                // validate if package has not yet expired
                if ((p.OfferingId == offering.OfferingId) & (p.EndDate >= currentDate) & (p.PackageId != TVEverywherePackageId))
                {
                    SortedSet<int> packageShowList = p.Package.GetAllShowIds(CountryCode, rightsType);
                    canPlay = packageShowList.Contains(show.CategoryId);
                }

                if (canPlay)
                {
                    break;
                }
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is entitled to package, given a channel
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="channel">channel to check rights against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <param name="TVEverywherePackageId">TV Everywhere PackageId</param>
        /// <returns>True - user has rights for the channel</returns>
        ///                 
        private bool IsPackageEntitled(Offering offering, Channel channel, RightsType rightsType, int TVEverywherePackageId)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;

            foreach (var p in PackageEntitlements)
            {
                // validate if package has not yet expired
                if ((p.OfferingId == offering.OfferingId) & (p.EndDate >= currentDate) & (p.PackageId != TVEverywherePackageId))
                {
                    SortedSet<int> packageChannelList = p.Package.GetAllChannelIds(CountryCode, rightsType);
                    canPlay = packageChannelList.Contains(channel.ChannelId);
                }

                if (canPlay)
                {
                    break;
                }
            }
            return (canPlay);
        }

        /// <summary>
        /// Determine if user can play a specific video
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="episode">episode where video to check for playback rights belongs to</param>
        /// <param name="asset">specific episode asset/video that is checked for playback rights</param>
        /// <param name="rightsType">type of right/platform to check agains(iptv, online or mobile)</param>        
        /// <param name="TVEverywherePackageId">TV Everywhere Package Id</param>
        /// <param name="IPCountryCode">Client IP's country code</param>
        /// <returns>True - can play</returns>
        public bool CanPlayVideo(Offering offering, Episode episode, Asset asset, RightsType rightsType, IEnumerable<int> excludePackageIds, IEnumerable<int> includePackageIds, string IPCountryCode, int GomsSubsidiaryId)
        {

            //if (IsEpisodeEntitledWithExcludePackageIdsFilter(offering, episode, asset, rightsType, excludePackageIds))
            if (IsEpisodeEntitledWithIncludePackageIdsFilter(offering, episode, asset, rightsType, includePackageIds))
                return true;
            else
            {
                //if (IsEpisodeEntitledWithIncludePackageIdsFilter(offering, episode, asset, rightsType, includePackageIds))
                if (IsEpisodeEntitledWithExcludePackageIdsFilter(offering, episode, asset, rightsType, excludePackageIds))
                {
                    //if (String.Compare(this.CountryCode, IPCountryCode, true) == 0)
                    //    return true;
                    //else
                    //    return false;
                    if (GomsSubsidiaryId == -1)
                        return String.Compare(this.CountryCode, IPCountryCode, true) == 0;
                    return GomsSubsidiaryId == this.Country.GomsSubsidiaryId;

                }
                else
                    return false;
            }
            //if (IsEpisodeEntitled(offering, episode, asset, rightsType, ))
            //{
            //    return true;
            //}
            //else
            //{
            //    if (IsEpisodeEntitled(offering, episode, asset, rightsType, TVEverywherePackageId))
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        if (String.Compare(this.CountryCode, IPCountryCode, true) == 0)
            //            return true;
            //        else
            //            return false;
            //    }
            //}
        }

        /// <summary>
        /// Check if user is Entitled to a specific episode
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="episode">episode where video to check for playback rights belongs to</param>
        /// <param name="asset">specific episode asset/video that is checked for playback rights</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <param name="TVEverywherePackageId">TV Everywhere Package Id</param>
        /// <returns>True - user has rights for the episode</returns>
        public bool IsEpisodeEntitled(Offering offering, Episode episode, Asset asset, RightsType rightsType, int TVEverywherePackageId)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;

            if (ValidateEpisodeAsset(episode, asset, rightsType))
            {
                // check if in preview assets
                canPlay = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);

                if (!canPlay)
                {
                    // check if in free assets
                    canPlay = (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                }

                if (!canPlay)
                {
                    // Check episode entitlement
                    canPlay = (EpisodeEntitlements.FirstOrDefault(c => (c.OfferingId == offering.OfferingId) && (c.EpisodeId == episode.EpisodeId) && ((c.EndDate >= currentDate))) != null);
                }

                if (!canPlay)
                {
                    // check show entitlement
                    canPlay = IsShowEntitled(offering, episode, asset, rightsType, TVEverywherePackageId);
                }
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is entitled to specific show
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="episode">episode where video to check for playback rights belongs to</param>
        /// <param name="asset">specific episode asset/video that is checked for playback rights</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <param name="TVEverywherePackageId">TV Everywhere Package Id</param>
        /// <returns>True - user has rights for the show</returns>
        public bool IsShowEntitled(Offering offering, Episode episode, Asset asset, RightsType rightsType, int TVEverywherePackageId)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;
            foreach (var s in episode.EpisodeCategories)
            {
                canPlay = IsShowEntitled(offering, s.Show, rightsType, TVEverywherePackageId);
                if (canPlay)
                {
                    break;
                }
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is entitled to a specific show
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="show">show to check rights against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <param name="TVEverywherePackageId">TV Everywhere Package Id</param>
        /// <returns>True - user has rights for the show</returns>
        public bool IsShowEntitled(Offering offering, Show show, RightsType rightsType, int TVEverywherePackageId)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;
            canPlay = (ShowEntitlements.FirstOrDefault(se => (se.OfferingId == offering.OfferingId) & (se.EndDate >= currentDate) & (se.CategoryId == show.CategoryId) & (show.StatusId == 1)) != null);
            if (!canPlay)
            {
                canPlay = IsPackageEntitled(offering, show, rightsType, TVEverywherePackageId);
            }
            return (canPlay);
        }


        /// <summary>
        /// Check if user is Entitled to a specific episode
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="episode">episode where video to check for playback rights belongs to</param>
        /// <param name="asset">specific episode asset/video that is checked for playback rights</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <param name="TVEverywherePackageId">TV Everywhere Package Id</param>
        /// <returns>True - user has rights for the episode</returns>
        //public bool IsEpisodeEntitled(Offering offering, Episode episode, Asset asset, RightsType rightsType, IEnumerable<int> packageIds, bool isExclusion)
        public bool IsEpisodeEntitled(Offering offering, Episode episode, Asset asset, RightsType rightsType, IEnumerable<int> packageIds)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;

            if (ValidateEpisodeAsset(episode, asset, rightsType))
            {
                // check if in preview assets
                canPlay = (episode.PreviewAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);

                if (!canPlay)
                {
                    // check if in free assets
                    canPlay = (episode.FreeAssets.FirstOrDefault(a => a.AssetId == asset.AssetId) != null);
                }

                if (!canPlay)
                {
                    // Check episode entitlement
                    canPlay = (EpisodeEntitlements.FirstOrDefault(c => (c.OfferingId == offering.OfferingId) && (c.EpisodeId == episode.EpisodeId) && ((c.EndDate >= currentDate))) != null);
                }

                if (!canPlay)
                {
                    // check show entitlement
                    //canPlay = IsShowEntitled(offering, episode, asset, rightsType, packageIds, isExclusion);
                    canPlay = IsShowEntitled(offering, episode, asset, rightsType, packageIds);
                }
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is entitled to specific show
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="episode">episode where video to check for playback rights belongs to</param>
        /// <param name="asset">specific episode asset/video that is checked for playback rights</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <param name="TVEverywherePackageId">TV Everywhere Package Id</param>
        /// <returns>True - user has rights for the show</returns>
        //public bool IsShowEntitled(Offering offering, Episode episode, Asset asset, RightsType rightsType, IEnumerable<int> packageIds, bool isExclusion)
        public bool IsShowEntitled(Offering offering, Episode episode, Asset asset, RightsType rightsType, IEnumerable<int> packageIds)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;
            foreach (var s in episode.EpisodeCategories)
            {
                //canPlay = IsShowEntitled(offering, s.Show, rightsType, packageIds, isExclusion);
                //added exception handling for episodes which have categories as parents (iptv)
                try { canPlay = IsShowEntitled(offering, s.Show, rightsType, packageIds); }
                catch (Exception) { }
                if (canPlay)
                {
                    break;
                }
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is entitled to a specific show
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="show">show to check rights against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <param name="TVEverywherePackageId">TV Everywhere Package Id</param>
        /// <returns>True - user has rights for the show</returns>
        //public bool IsShowEntitled(Offering offering, Show show, RightsType rightsType, IEnumerable<int> packageIds, bool isExclusion)
        public bool IsShowEntitled(Offering offering, Show show, RightsType rightsType, IEnumerable<int> packageIds)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;
            canPlay = (ShowEntitlements.FirstOrDefault(se => (se.OfferingId == offering.OfferingId) & (se.EndDate >= currentDate) & (se.CategoryId == show.CategoryId) & (show.StatusId == 1)) != null);
            if (!canPlay)
            {
                //canPlay = IsPackageEntitled(offering, show, rightsType, packageIds, isExclusion);
                canPlay = IsPackageEntitled(offering, show, rightsType, packageIds);
            }
            return (canPlay);
        }

        /// <summary>
        /// Check if user is entitled to package, given a show
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="show">show to check rights against</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <param name="TVEverywherePackageId">TV Everywhere PackageId</param>
        /// <returns>True - user has rights for the show</returns>
        //private bool IsPackageEntitled(Offering offering, Show show, RightsType rightsType, IEnumerable<int> packageIds, bool isExclusion)
        private bool IsPackageEntitled(Offering offering, Show show, RightsType rightsType, IEnumerable<int> packageIds)
        {
            bool canPlay = false;
            var currentDate = DateTime.Now;

            foreach (var p in PackageEntitlements)
            {
                // validate if package has not yet expired

                //if (isExclusion)
                //{
                //    if ((p.OfferingId == offering.OfferingId) & (p.EndDate >= currentDate) & (packageIds.Contains(p.PackageId)))
                //    {
                //        SortedSet<int> packageShowList = p.Package.GetAllShowIds(CountryCode, rightsType);
                //        canPlay = packageShowList.Contains(show.CategoryId);
                //    }
                //}
                //else
                //{
                //    if ((p.OfferingId == offering.OfferingId) & (p.EndDate >= currentDate) & (packageIds.Contains(p.PackageId)))
                //    {
                //        SortedSet<int> packageShowList = p.Package.GetAllShowIds(CountryCode, rightsType);
                //        canPlay = packageShowList.Contains(show.CategoryId);
                //    }
                //}

                if ((p.OfferingId == offering.OfferingId) & (p.EndDate >= currentDate) & (packageIds.Contains(p.PackageId)))
                {
                    SortedSet<int> packageShowList = p.Package.GetAllShowIds(CountryCode, rightsType);
                    canPlay = packageShowList.Contains(show.CategoryId);
                }

                if (canPlay)
                {
                    break;
                }
            }
            return (canPlay);
        }


        /// <summary>
        /// Check if user is Entitled to a specific episode with package id filters (to be included)
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="episode">episode where video to check for playback rights belongs to</param>
        /// <param name="asset">specific episode asset/video that is checked for playback rights</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>
        /// <param name="packageIds">list of package ids to be included</param>
        /// <returns>True - user has rights for the episode</returns>
        public bool IsEpisodeEntitledWithIncludePackageIdsFilter(Offering offering, Episode episode, Asset asset, RightsType rightsType, IEnumerable<int> packageIds)
        {
            //return IsEpisodeEntitled(offering, episode, asset, rightsType, packageIds, false);
            return IsEpisodeEntitled(offering, episode, asset, rightsType, packageIds);
        }

        /// <summary>
        /// Check if user is Entitled to a specific episode with package id filters (to be excluded)
        /// </summary>
        /// <param name="offering">offering to check rights against</param>
        /// <param name="episode">episode where video to check for playback rights belongs to</param>
        /// <param name="asset">specific episode asset/video that is checked for playback rights</param>
        /// <param name="rightsType">type of right/platform to check against(iptv, online or mobile)</param>        
        /// <param name="packageIds">list of package ids to be excluded</param>        
        /// <returns>True - user has rights for the episode</returns>
        public bool IsEpisodeEntitledWithExcludePackageIdsFilter(Offering offering, Episode episode, Asset asset, RightsType rightsType, IEnumerable<int> packageIds)
        {
            //return IsEpisodeEntitled(offering, episode, asset, rightsType, packageIds, true);
            return IsEpisodeEntitled(offering, episode, asset, rightsType, packageIds);
        }

        public bool HasExceededMaximumPaymentTransactionsForTheDay(int maximumThreshold, DateTime registDt)
        {
            var startDt = registDt.Date;
            var endDt = startDt.AddDays(1);
            return Transactions.Count(t => t is PaymentTransaction && t.Date > startDt && t.Date < endDt && t.Amount > 0) >= maximumThreshold;
        }


        public bool HasExceededMaximumReloadTransactionsForTheDay(int maximumThreshold, DateTime registDt)
        {
            var startDt = registDt.Date;
            var endDt = startDt.AddDays(1);
            return Transactions.Count(t => t is ReloadTransaction && t.Date > startDt && t.Date < endDt && t.Amount > 0) >= maximumThreshold;
        }

        public List<Package> GetSubscribedPackages(int offeringId)
        {
            List<Package> list = null;
            var activeEntitlements = Entitlements.Where(e => e.OfferingId == offeringId && e.EndDate > DateTime.Now && e is PackageEntitlement);
            if (activeEntitlements != null)
            {
                list = new List<Package>();
                foreach (var e in activeEntitlements)
                {
                    try
                    {
                        if (e is PackageEntitlement && e.LatestEntitlementRequestId != null && e.LatestEntitlementRequest.Product is SubscriptionProduct)
                            list.Add(((PackageEntitlement)e).Package);

                    }
                    catch (Exception) { }
                }
            }
            return list;
        }

        public bool HasActiveSubscriptions()
        {
            return PackageEntitlements.Count(p => p.EndDate > DateTime.Now) > 0;
        }

        public bool IsFirstTimePurchaser(Offering offering)
        {
            try
            {
                return Transactions.OfType<PaymentTransaction>().Count(t => t.OfferingId == offering.OfferingId && t.Amount > 0) <= 1;
            }
            catch (Exception) { return false; }
        }

        public bool HasClaimedFreeTrialProduct(int productId)
        {
            try
            {
                return EntitlementRequests.Count(e => e.ProductId == productId) > 0;
            }
            catch (Exception) { return false; }
        }
    }
}