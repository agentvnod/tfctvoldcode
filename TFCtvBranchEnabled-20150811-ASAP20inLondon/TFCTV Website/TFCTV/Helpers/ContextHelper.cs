using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IPTV2_Model;
using EngagementsModel;
using StackExchange.Profiling;
using System.Web.Security;

namespace TFCTV.Helpers
{
    public static class ContextHelper
    {
        public static UserWallet CreateWallet(decimal Balance, string CurrencyCode, DateTime registDt)
        {
            UserWallet wallet = new UserWallet()
            {
                Balance = Balance,
                Currency = CurrencyCode,
                LastUpdated = registDt,
                IsActive = true,
            };
            return wallet;
        }

        public static SubscriptionProductType GetProductType(Product product)
        {
            SubscriptionProductType type = SubscriptionProductType.Package;
            if (product is SubscriptionProduct)
            {
                SubscriptionProduct sproduct = (SubscriptionProduct)product;
                if (sproduct is ShowSubscriptionProduct)
                    type = SubscriptionProductType.Show;
                else if (sproduct is EpisodeSubscriptionProduct)
                    type = SubscriptionProductType.Episode;
                else if (sproduct is PackageSubscriptionProduct)
                    type = SubscriptionProductType.Package;
            }
            return type;
        }

        public static string GetProductName(IPTV2Entities context, Product product)
        {
            if (product is SubscriptionProduct)
            {
                SubscriptionProduct sproduct = (SubscriptionProduct)product;
                if (sproduct is ShowSubscriptionProduct)
                    return ((ShowSubscriptionProduct)sproduct).Categories.First().Show.Description;
                else if (sproduct is EpisodeSubscriptionProduct)
                    return ((EpisodeSubscriptionProduct)sproduct).Episodes.First().Episode.Description;
                else if (sproduct is PackageSubscriptionProduct)
                    return ((PackageSubscriptionProduct)sproduct).Packages.First().Package.Description;
            }
            return String.Empty;
        }

        public static int GetPackageFromProductId(int productId)
        {
            int packageId = 0;
            var context = new IPTV2Entities();
            try
            {
                var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == productId);
                if (productPackage != null)
                    packageId = productPackage.PackageId;
            }
            catch (Exception) { }
            return packageId;
        }


        public static string GetProductName(int productId)
        {
            var context = new IPTV2Entities();
            var product = context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                if (product is SubscriptionProduct)
                {
                    SubscriptionProduct sproduct = (SubscriptionProduct)product;
                    if (sproduct is ShowSubscriptionProduct)
                        return ((ShowSubscriptionProduct)sproduct).Categories.First().Show.Description;
                    else if (sproduct is EpisodeSubscriptionProduct)
                        return ((EpisodeSubscriptionProduct)sproduct).Episodes.First().Episode.Description;
                    else if (sproduct is PackageSubscriptionProduct)
                        return ((PackageSubscriptionProduct)sproduct).Packages.First().Package.PackageName;
                }
            }
            return String.Empty;
        }

        public static string GetCurrentSubscribeProduct(IPTV2Entities context, Product product, System.Guid userId, Offering offering)
        {
            if (product is SubscriptionProduct)
            {
                SubscriptionProduct sproduct = (SubscriptionProduct)product;
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                var subscribedproducts = user.GetSubscribedProductGroups(offering);
                foreach (var item in subscribedproducts)
                {
                    return item.Name;
                    //if (item.ProductGroup.UpgradeableFromProductGroups().Contains(sproduct.ProductGroup))
                    //{
                    //    //Product CurrentProduct = context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                    //    //return CurrentProduct.Name;
                    //    return item.Name;
                    //}
                }
            }

            return String.Empty;
        }

        public static bool HasSocialEngagement(Guid userId, int reactionTypeId, object idx, EngagementContentType type)
        {
            bool retVal = false;
            try
            {
                var context = new EngagementsEntities();
                switch (type)
                {
                    case EngagementContentType.Show:
                        retVal = context.ShowReactions.Count(i => i.CategoryId == (int)idx && i.ReactionTypeId == reactionTypeId && i.UserId == userId) > 0;
                        break;
                    case EngagementContentType.Episode:
                        retVal = context.EpisodeReactions.Count(i => i.EpisodeId == (int)idx && i.ReactionTypeId == reactionTypeId && i.UserId == userId) > 0;
                        break;
                    case EngagementContentType.Celebrity:
                        retVal = context.CelebrityReactions.Count(i => i.CelebrityId == (int)idx && i.ReactionTypeId == reactionTypeId && i.UserId == userId) > 0;
                        break;
                    case EngagementContentType.Channel:
                        retVal = context.ChannelReactions.Count(i => i.ChannelId == (int)idx && i.ReactionTypeId == reactionTypeId && i.UserId == userId) > 0;
                        break;
                    case EngagementContentType.Youtube:
                        retVal = context.YouTubeReactions.Count(i => i.YouTubeId == (string)idx && i.ReactionTypeId == reactionTypeId && i.UserId == userId) > 0;
                        break;
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return retVal;
        }

        public static int GetConvertedDaysFromFreeTrial(User user)
        {
            DateTime registDt = DateTime.Now;
            var freeTrialPackageIds = MyUtility.StringToIntList(GlobalConfig.FreeTrialPackageIds);
            var freeTrialPackage = user.PackageEntitlements.FirstOrDefault(p => freeTrialPackageIds.Contains(p.PackageId) && p.EndDate > registDt);
            if (freeTrialPackage != null)
            {
                DateTime tempDt = DateTime.Parse(registDt.ToShortDateString());
                int convertedDays = freeTrialPackage.EndDate.Subtract(tempDt).Days;
                if (convertedDays <= 1)
                    return 0;
                return convertedDays;
            }
            return 0;
        }


        public static PackageEntitlement CreatePackageEntitlement(EntitlementRequest request, PackageSubscriptionProduct subscription, ProductPackage package, DateTime endDate, int offeringId)
        {
            PackageEntitlement entitlement = new PackageEntitlement()
            {
                EndDate = endDate,
                Package = (IPTV2_Model.Package)package.Package,
                OfferingId = offeringId,
                LatestEntitlementRequest = request
            };
            return entitlement;
        }



        public static EntitlementRequest CreateEntitlementRequest(DateTime registDt, DateTime endDate, Product product, string source, string reference)
        {
            EntitlementRequest request = new EntitlementRequest()
            {
                DateRequested = registDt,
                EndDate = endDate,
                Product = product,
                Source = source,
                ReferenceId = reference
            };
            return request;
        }

        public static PurchaseItem CreatePurchaseItem(System.Guid userId, Product product, ProductPrice priceOfProduct)
        {
            PurchaseItem item = new PurchaseItem()
            {
                RecipientUserId = userId,
                ProductId = product.ProductId,
                Price = priceOfProduct.Amount,
                Currency = priceOfProduct.CurrencyCode,
                Remarks = product.Name
            };
            return item;
        }

        public static Purchase CreatePurchase(DateTime registDt, string remarks)
        {
            Purchase purchase = new Purchase()
            {
                Date = registDt,
                Remarks = remarks
            };
            return purchase;
        }

        public static bool CanPlayVideo(IPTV2Entities context, Offering offering, Episode episode, Asset asset, System.Security.Principal.IPrincipal thisUser, HttpRequestBase req)
        {
            bool IsUserEntitled = false;

            var profiler = MiniProfiler.Current;

            using (profiler.Step("ContextHelper.CanPlayVideo"))
            {
                string CountryCode = GlobalConfig.DefaultCountry;
                try
                {
                    CountryCode = MyUtility.getCountry(req.GetUserHostAddressFromCloudflare()).getCode();
                }
                catch (Exception) { }

                var packageId = GlobalConfig.AnonymousDefaultPackageId;

                if (!IsUserEntitled)
                    IsUserEntitled = User.IsAssetEntitled(context, offering.OfferingId, packageId, episode.EpisodeId, asset.AssetId, CountryCode, RightsType.Online);
                else
                    IsUserEntitled = true;

                // check user's access rights
                if (!IsUserEntitled && thisUser.Identity.IsAuthenticated)
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == new System.Guid(thisUser.Identity.Name));
                    if (user != null)
                    {
                        // check access from default logged in user package
                        packageId = GlobalConfig.LoggedInDefaultPackageId;
                        IsUserEntitled = User.IsAssetEntitled(context, offering.OfferingId, packageId, episode.EpisodeId, asset.AssetId, user.CountryCode, RightsType.Online);

                        if (!IsUserEntitled)
                        {
                            if (GlobalConfig.IsTVERegionBlockingEnabled)
                            {
                                var userCountryCode = MyUtility.GetCountryCodeViaIp();
                                int GomsSubsidiaryId = 0;
                                if (GlobalConfig.UseGomsSubsidiaryForTVECheck)
                                    GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                else
                                    GomsSubsidiaryId = -1;
                                //var GomsSubsidiaryId = ContextHelper.GetGomsSubsidiaryIdOfCountry(context, userCountryCode);
                                //IsUserEntitled = user.CanPlayVideo(offering, episode, asset, RightsType.Online); // check if user has entitlements that can play the video
                                var IncludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayIncludedPackageIds);
                                var ExcludePackageIds = MyUtility.StringToIntList(GlobalConfig.CanPlayExcludedPackageIds);
                                IsUserEntitled = user.CanPlayVideo(offering, episode, asset, RightsType.Online, ExcludePackageIds, IncludePackageIds, userCountryCode, GomsSubsidiaryId);
                                if (GlobalConfig.IsTVEIpCheckEnabled)
                                {
                                    try
                                    {
                                        string ip = GlobalConfig.IpWhiteList;
                                        string[] IpAddresses = ip.Split(';');
                                        if (IpAddresses.Contains(req.GetUserHostAddressFromCloudflare()))
                                            IsUserEntitled = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                                    }
                                    catch (Exception e) { MyUtility.LogException(e, "ContextHelper IsUserEntitled IP Whitelisting"); }
                                }
                            }
                            else
                                IsUserEntitled = user.CanPlayVideo(offering, episode, asset, RightsType.Online);
                        }
                        else
                            IsUserEntitled = true;
                    }
                }
            }

            ////Check for subclips
            //int snippetStart = 0;
            //int snippetEnd = 0;
            //AkamaiFlowPlayerPluginClipDetails clipDetails = null;

            //if (!IsUserEntitled)
            //{
            //    if ((asset.SnippetStart != null) && (asset.SnippetEnd != null) && (asset.SnippetEnd > asset.SnippetStart))
            //    {
            //        snippetStart = Convert.ToInt32(asset.SnippetStart.Value.TotalSeconds);
            //        snippetEnd = Convert.ToInt32(asset.SnippetEnd.Value.TotalSeconds);
            //    }
            //    else
            //    {
            //        snippetStart = 0;
            //        snippetEnd = GlobalConfig.snippetEnd;
            //    }
            //    clipDetails.PromptToSubscribe = true;
            //}
            //else
            //    clipDetails.PromptToSubscribe = false;

            //clipDetails.SubClip = (snippetStart + snippetEnd > 0) ? new SubClip { Start = snippetStart, End = snippetEnd } : null;
            //if (clipDetails.SubClip != null)
            //    IsUserEntitled = false;
            //else
            //    IsUserEntitled = true;

            return IsUserEntitled;
        }

        public static bool HasSocialEngagementRecordOnEpisode(Guid userId, int reactionTypeId, int episodeId)
        {
            var context = new EngagementsEntities();
            var episode = context.EpisodeReactions.FirstOrDefault(s => s.EpisodeId == episodeId && s.ReactionTypeId == reactionTypeId && s.UserId == userId);
            if (episode == null)
                return false;
            return true;
        }

        public static string GetShowParentCategories(int categoryId, bool useDescription = false, bool getSingleParent = false, bool isUnion = false)
        {
            string result = String.Empty;
            try
            {
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == categoryId);
                if (category != null)
                {
                    var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentCategoriesCacheDuration, 0);
                    var eCacheDuration = new TimeSpan(0, GlobalConfig.GetAllSubCategoryIdsCacheDuration, 0);
                    if (category is Show)
                    {
                        #region category is show
                        var show = (Show)category;
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                        var parentCategories = show.GetAllParentCategories(CacheDuration);
                        var serviceSubCategories = service.GetAllSubCategoryIds(eCacheDuration);
                        if (isUnion)
                            serviceSubCategories.UnionWith(parentCategories);
                        else
                            serviceSubCategories.IntersectWith(parentCategories);
                        result = string.Join(",", serviceSubCategories);
                        try
                        {
                            if (useDescription)
                            {
                                if (getSingleParent)
                                {
                                    int tempId = serviceSubCategories.ElementAt(0);
                                    if (serviceSubCategories.Count() > 1)
                                        tempId = serviceSubCategories.ElementAt(1);
                                    var tempCategory = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == tempId);
                                    if (tempCategory != null)
                                        result = tempCategory.Description;
                                }
                                else
                                {
                                    var listOfCategoryNames = context.CategoryClasses.Where(c => serviceSubCategories.Contains(c.CategoryId)).Select(c => c.Description);
                                    result = string.Join(",", listOfCategoryNames);
                                }
                            }
                        }
                        catch (Exception) { }
                        #endregion
                    }
                    else if (category is Category)
                    {
                        #region category is category
                        var cat = (Category)category;
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                        var parentCategories = cat.GetAllParentCategories(CacheDuration);
                        var serviceSubCategories = service.GetAllSubCategoryIds(eCacheDuration);
                        if (isUnion)
                            serviceSubCategories.UnionWith(parentCategories);
                        else
                            serviceSubCategories.IntersectWith(parentCategories);
                        result = string.Join(",", serviceSubCategories);
                        try
                        {
                            if (useDescription)
                            {
                                var listOfCategoryNames = context.CategoryClasses.Where(c => serviceSubCategories.Contains(c.CategoryId)).Select(c => c.Description);
                                result = string.Join(",", listOfCategoryNames);
                            }
                        }
                        catch (Exception) { }
                        #endregion
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return result;
        }

        public static string GetEpisodeParentCategories(int episodeId, bool useDescription = false)
        {
            string result = String.Empty;
            try
            {
                var context = new IPTV2Entities();
                var episode = context.Episodes.FirstOrDefault(e => e.EpisodeId == episodeId);
                if (episode != null)
                {
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                    var CacheDuration = new TimeSpan(0, GlobalConfig.GetParentShowsForEpisodeCacheDuration, 0);
                    var parentShows = episode.GetParentShows(CacheDuration);
                    result = string.Join(",", parentShows);
                    if (useDescription)
                    {
                        var listOfCategoryNames = context.CategoryClasses.Where(c => parentShows.Contains(c.CategoryId)).Select(c => c.Description);
                        result = string.Join(",", listOfCategoryNames);
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return result;
        }

        public static bool DoesEpisodeHaveIosCdnReferenceBasedOnAsset(Episode episode)
        {
            try
            {
                //var premiumAsset = episode.PremiumAssets.FirstOrDefault();
                var premiumAsset = episode.PremiumAssets.LastOrDefault();
                if (premiumAsset != null)
                {
                    var asset = premiumAsset.Asset;
                    return asset.AssetCdns.Count(a => a.CdnId == 5) > 0;
                }
            }
            catch (Exception) { }
            return false;
        }

        public static bool DoesEpisodeHaveIosCdnReferenceBasedOPremiumnAsset(EpisodePremiumAsset premiumAsset)
        {
            try
            {
                if (premiumAsset != null)
                {
                    var asset = premiumAsset.Asset;
                    return asset.AssetCdns.Count(a => a.CdnId == 5) > 0;
                }
            }
            catch (Exception) { }
            return false;
        }

        public static bool DoesEpisodeHaveIosCdnReferenceBasedOAsset(object asset)
        {
            try
            {
                if (asset is EpisodePremiumAsset)
                {
                    var premiumAsset = (EpisodePremiumAsset)asset;
                    if (premiumAsset != null)
                    {
                        var obj = premiumAsset.Asset;
                        return obj.AssetCdns.Count(a => a.CdnId == 5) > 0;
                    }
                }
                else if (asset is EpisodePreviewAsset)
                {
                    var previewAsset = (EpisodePreviewAsset)asset;
                    if (previewAsset != null)
                    {
                        var obj = previewAsset.Asset;
                        return obj.AssetCdns.Count(a => a.CdnId == 5) > 0;
                    }
                }
            }
            catch (Exception) { }
            return false;
        }

        //public static bool IsCategoryAllowedInCountry(int id)
        //{
        //    var context = new IPTV2Entities();
        //    var offering = context.Offerings.FirstOrDefault(o => o.OfferingId == GlobalConfig.offeringId);
        //    var service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
        //    SortedSet<int> showIds = service.GetAllOnlineShowIds(MyUtility.GetCurrentCountryCodeOrDefault());
        //    return showIds.Contains(id);
        //}

        public static bool IsCategoryAllowedInCountry(Show show)
        {
            var countryCodes = show.OnlineAllowedCountries.Where(o => o.StatusId == GlobalConfig.Visible).Select(o => o.CountryCode);
            if (countryCodes.Count() > 0)
            {
                if (countryCodes.Contains("--"))
                    return true;
                else
                    return countryCodes.Contains(MyUtility.GetCurrentCountryCodeOrDefault());
            }
            return true;
        }

        public static bool IsCategoryBlockedInCountry(Show show)
        {
            var countryCodes = show.OnlineBlockedCountries.Where(o => o.StatusId == GlobalConfig.Visible).Select(o => o.CountryCode);
            if (countryCodes.Count() > 0)
            {
                if (countryCodes.Contains("--"))
                    return true;
                else
                    return countryCodes.Contains(MyUtility.GetCurrentCountryCodeOrDefault());
            }
            return false;
        }

        public static bool IsCategoryViewableInUserCountry(Show show)
        {
            if (IsCategoryBlockedInCountry(show))
                return false;
            else
            {
                return IsCategoryAllowedInCountry(show);
                //if (IsCategoryAllowedInCountry(show))
                //    return true;
                //else
                //    return false;
            }
        }

        public static int GetGomsSubsidiaryIdOfCountry(IPTV2Entities context, string CountryCode)
        {
            var profiler = MiniProfiler.Current;

            int SubsidiaryId = 0;
            try
            {
                using (profiler.Step("GetGomsSubsidiaryIdOfCountry"))
                {
                    Country country = null;
                    if (GlobalConfig.UseCountryListInMemory)
                    {
                        if (GlobalConfig.CountryList != null)
                            country = GlobalConfig.CountryList.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);
                        else
                            country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);
                    }
                    else
                        country = context.Countries.FirstOrDefault(c => String.Compare(c.Code, CountryCode, true) == 0);

                    if (country != null)
                    {
                        if (country.GomsSubsidiary != null)
                            return country.GomsSubsidiary.GomsSubsidiaryId;
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return SubsidiaryId;
        }

        public static Celebrity GetCelebrity(int id, IPTV2Entities context)
        {
            return context.Celebrities.Find(id);
        }

        //public static ShowPackageProductPrices GetShowPackageProductPrices(int categoryId, string countryCode)
        //{
        //    ShowPackageProductPrices showPackageProductPrices = new ShowPackageProductPrices();
        //    try
        //    {
        //        showPackageProductPrices = showPackageProductPrices.LoadAllPackageAndProduct(categoryId, countryCode, true);
        //    }
        //    catch (Exception e) { MyUtility.LogException(e); }
        //    return showPackageProductPrices;
        //}

        public static void InitializeCountryList()
        {
            //initialize country list
            var context = new IPTV2_Model.IPTV2Entities();
            GlobalConfig.CountryList = context.Countries.ToList();
        }

        public static bool SaveSessionInDatabase(IPTV2Entities context, User user)
        {
            if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
            {
                try
                {
                    HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    user.SessionId = authCookie.Value;
                    user.SessionLoggedDate = DateTime.Now;
                    return context.SaveChanges() > 0;
                }
                catch (Exception) { }
            }
            return false;
        }

        public static ProductGroupType GetProductGroupType(Product2 product)
        {
            ProductGroupType productGroupType = new ProductGroupType() { productSubscriptionType = ProductSubscriptionType.Package, type = 1 };
            try
            {
                var context = new IPTV2Entities();
                var productGroup = context.ProductGroups.FirstOrDefault(p => p.ProductGroupId == product.ProductGroupId);
                if (productGroup != null)
                {
                    productGroupType.type = productGroup.ProductGroupId;
                    if (productGroup.ProductSubscriptionTypeId != null)
                    {
                        productGroupType.productSubscriptionType = ProductSubscriptionType.Show;
                        productGroupType.type = (int)productGroup.ProductSubscriptionTypeId;
                    }
                }
            }
            catch (Exception) { }
            return productGroupType;
        }

        public static bool IsProductAllowedInCountry(Product product)
        {
            var countryCodes = product.AllowedCountries.Where(o => o.StatusId == GlobalConfig.Visible).Select(o => o.CountryCode);
            if (countryCodes.Count() > 0)
            {
                if (countryCodes.Contains("--"))
                    return true;
                else
                    return countryCodes.Contains(MyUtility.GetCurrentCountryCodeOrDefault());
            }
            return true;
        }

        public static bool IsProductBlockedInCountry(Product product)
        {
            var countryCodes = product.BlockedCountries.Where(o => o.StatusId == GlobalConfig.Visible).Select(o => o.CountryCode);
            if (countryCodes.Count() > 0)
            {
                if (countryCodes.Contains("--"))
                    return true;
                else
                    return countryCodes.Contains(MyUtility.GetCurrentCountryCodeOrDefault());
            }
            return false;
        }

        public static bool IsProductViewableInUserCountry(Product product)
        {
            if (IsProductBlockedInCountry(product))
                return false;
            else
                return IsProductAllowedInCountry(product);
        }

        public static string GetTransactionType(Transaction transaction)
        {
            string result = String.Empty;
            if (transaction is PaymentTransaction)
            {
                if (transaction is CreditCardPaymentTransaction)
                    result = "Credit Card";
                else if (transaction is PaypalPaymentTransaction)
                    result = "PayPal";
                else if (transaction is PpcPaymentTransaction)
                    result = "Prepaid Card/ePIN";
                else if (transaction is WalletPaymentTransaction)
                    result = "E-Wallet";
            }
            else if (transaction is ReloadTransaction)
            {
                if (transaction is CreditCardReloadTransaction)
                    result = "Credit Card";
                else if (transaction is PaypalReloadTransaction)
                    result = "PayPal";
                else if (transaction is PpcReloadTransaction)
                    result = "Prepaid Card/ePIN";
                else if (transaction is SmartPitReloadTransaction)
                    result = "SmartPit";
            }
            return result;

        }

        public static bool IsCategoryAllowedInCountry(Show show, string CountryCode)
        {
            var countryCodes = show.OnlineAllowedCountries.Where(o => o.StatusId == GlobalConfig.Visible).Select(o => o.CountryCode);
            if (countryCodes.Count() > 0)
            {
                if (countryCodes.Contains("--"))
                    return true;
                else
                    return countryCodes.Contains(CountryCode);
            }
            return true;
        }

        public static bool IsCategoryBlockedInCountry(Show show, string CountryCode)
        {
            var countryCodes = show.OnlineBlockedCountries.Where(o => o.StatusId == GlobalConfig.Visible).Select(o => o.CountryCode);
            if (countryCodes.Count() > 0)
            {
                if (countryCodes.Contains("--"))
                    return true;
                else
                    return countryCodes.Contains(CountryCode);
            }
            return false;
        }

        public static bool IsCategoryViewableInUserCountry(Show show, string CountryCode)
        {
            if (IsCategoryBlockedInCountry(show, CountryCode))
                return false;
            else
                return IsCategoryAllowedInCountry(show, CountryCode);
        }

        public static bool HasSocialEngagementRecordOnShow(Guid userId, int reactionTypeId, int categoryId)
        {
            try
            {
                var context = new EngagementsEntities();
                return context.ShowReactions.Count(s => s.CategoryId == categoryId && s.ReactionTypeId == reactionTypeId && s.UserId == userId) > 0;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return false;
        }

        public static CheckSubscriptionReturnObject HasActiveSubscriptionBasedOnCategoryId(User user, int categoryId, DateTime? registDt = null, string CountryCodeViaIp = null)
        {
            var returnObject = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
            try
            {
                SortedSet<int> completeCategoryIds = new SortedSet<int>();
                if (registDt == null)
                    registDt = DateTime.Now;
                if (user != null)
                {
                    if (user.Entitlements != null)
                    {
                        if (user.Entitlements.Count(e => e.EndDate > registDt) > 0)
                        {
                            foreach (var entitlement in user.Entitlements)
                            {
                                if (entitlement.EndDate > registDt)
                                {
                                    if (entitlement is PackageEntitlement)
                                    {
                                        var pkgEntitlement = (PackageEntitlement)entitlement;
                                        var showIds = pkgEntitlement.Package.GetAllOnlineShowIds(user.CountryCode);
                                        completeCategoryIds.UnionWith(showIds);
                                    }
                                    else if (entitlement is ShowEntitlement)
                                    {
                                        var showEntitlement = (ShowEntitlement)entitlement;
                                        completeCategoryIds.Add(showEntitlement.CategoryId);
                                    }

                                    if (completeCategoryIds.Contains(categoryId))
                                    {
                                        returnObject.HasSubscription = true;
                                        if (returnObject.SubscriptionEndDate == null)
                                            returnObject.SubscriptionEndDate = entitlement.EndDate;
                                        else if (returnObject.SubscriptionEndDate < entitlement.EndDate)
                                            returnObject.SubscriptionEndDate = entitlement.EndDate;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //after checking entitlements and user still has no subscription
                    //fallback is to  check if shows are available on anonymous and default logged in packages
                    if (!returnObject.HasSubscription)
                    {
                        SortedSet<int> listOfPackageIds = new SortedSet<int>();
                        listOfPackageIds.Add(GlobalConfig.LoggedInDefaultPackageId);
                        listOfPackageIds.Add(GlobalConfig.AnonymousDefaultPackageId);

                        var context = new IPTV2Entities();
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var packages = offering.Packages.Where(p => listOfPackageIds.Contains(p.PackageId));
                        if (packages != null)
                        {
                            if (packages.Count() > 0)
                            {
                                foreach (var package in packages)
                                {
                                    completeCategoryIds.UnionWith(package.GetAllOnlineShowIds(user.CountryCode));
                                }
                            }
                            returnObject.HasSubscription = completeCategoryIds.Contains(categoryId);
                        }
                    }
                    if (returnObject.SubscriptionEndDate != null && registDt != null)
                    {
                        if (returnObject.SubscriptionEndDate.Value.Subtract((DateTime)registDt).TotalDays <= 3)
                        {
                            returnObject.NumberOfDaysLeft = Convert.ToInt32(returnObject.SubscriptionEndDate.Value.Subtract((DateTime)registDt).TotalDays);
                            returnObject.Within5DaysOrLess = true;
                        }
                    }

                }
                else //Logged out, check default anonymous package
                {
                    var context = new IPTV2Entities();
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var package = offering.Packages.FirstOrDefault(p => p.PackageId == GlobalConfig.AnonymousDefaultPackageId);
                    var listOfShows = package.GetAllOnlineShowIds(CountryCodeViaIp);
                    returnObject.HasSubscription = listOfShows.Contains(categoryId);
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return returnObject;
        }

        public static CheckSubscriptionReturnObject HasActiveSubscriptionBasedOnCategoryId(User user, IEnumerable<int> categoryIds, DateTime? registDt = null, string CountryCodeViaIp = null)
        {
            var returnObject = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false, IsFreeEntitlement = false };
            try
            {
                SortedSet<int> completeCategoryIds = new SortedSet<int>();
                if (registDt == null)
                    registDt = DateTime.Now;
                if (user != null)
                {
                    if (user.Entitlements != null)
                    {
                        if (user.Entitlements.Count(e => e.EndDate > registDt) > 0)
                        {
                            foreach (var entitlement in user.Entitlements)
                            {
                                if (entitlement.EndDate > registDt)
                                {
                                    if (entitlement is PackageEntitlement)
                                    {
                                        var pkgEntitlement = (PackageEntitlement)entitlement;
                                        var showIds = pkgEntitlement.Package.GetAllOnlineShowIds(user.CountryCode);
                                        completeCategoryIds.UnionWith(showIds);
                                    }
                                    else if (entitlement is ShowEntitlement)
                                    {
                                        var showEntitlement = (ShowEntitlement)entitlement;
                                        completeCategoryIds.Add(showEntitlement.CategoryId);
                                    }

                                    if (completeCategoryIds.Intersect(categoryIds).Count() > 0)
                                    {
                                        returnObject.HasSubscription = true;
                                        if (returnObject.SubscriptionEndDate == null)
                                            returnObject.SubscriptionEndDate = entitlement.EndDate;
                                        else if (returnObject.SubscriptionEndDate < entitlement.EndDate)
                                            returnObject.SubscriptionEndDate = entitlement.EndDate;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //after checking entitlements and user still has no subscription
                    //fallback is to  check if shows are available on anonymous and default logged in packages
                    if (!returnObject.HasSubscription)
                    {
                        SortedSet<int> listOfPackageIds = new SortedSet<int>();
                        listOfPackageIds.Add(GlobalConfig.LoggedInDefaultPackageId);
                        listOfPackageIds.Add(GlobalConfig.AnonymousDefaultPackageId);

                        var context = new IPTV2Entities();
                        var offering = context.Offerings.Find(GlobalConfig.offeringId);
                        var packages = offering.Packages.Where(p => listOfPackageIds.Contains(p.PackageId));
                        if (packages != null)
                        {
                            if (packages.Count() > 0)
                            {
                                foreach (var package in packages)
                                {
                                    completeCategoryIds.UnionWith(package.GetAllOnlineShowIds(user.CountryCode));
                                }
                            }
                            returnObject.HasSubscription = completeCategoryIds.Intersect(categoryIds).Count() > 0;
                            returnObject.IsFreeEntitlement = returnObject.HasSubscription;
                        }
                    }

                    if (returnObject.SubscriptionEndDate != null && registDt != null)
                    {
                        if (returnObject.SubscriptionEndDate.Value.Subtract((DateTime)registDt).TotalDays <= 3)
                        {
                            returnObject.NumberOfDaysLeft = Convert.ToInt32(returnObject.SubscriptionEndDate.Value.Subtract((DateTime)registDt).TotalDays);
                            returnObject.Within5DaysOrLess = true;
                        }
                    }

                }
                else //Logged out, check default anonymous package
                {
                    var context = new IPTV2Entities();
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var package = offering.Packages.FirstOrDefault(p => p.PackageId == GlobalConfig.AnonymousDefaultPackageId);
                    var listOfShows = package.GetAllOnlineShowIds(CountryCodeViaIp);
                    returnObject.HasSubscription = listOfShows.Intersect(categoryIds).Count() > 0;
                    returnObject.IsFreeEntitlement = returnObject.HasSubscription;
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return returnObject;
        }

        public static List<string> GetPackageFeatures(string CountryCode, ProductPackage package)
        {
            List<string> list = null;
            string jsonString = String.Empty;
            try
            {
                var cache = DataCache.Cache;
                //modify cache duration
                var CacheDuration = new TimeSpan(0, GlobalConfig.PackageAndProductCacheDuration, 0);
                string cacheKey = "GPKGFEAT:P:" + package.PackageId + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    list = new List<string>();
                    var context = new IPTV2Entities();
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(o => o.PackageId == GlobalConfig.serviceId);
                    SortedSet<int> listOfShowIds = new SortedSet<int>();

                    foreach (var category in package.Package.Categories)
                    {
                        listOfShowIds.UnionWith(service.GetAllOnlineShowIds(CountryCode, category.Category));
                        if (category.Category is Category)
                        {
                            var item = (Category)category.Category;
                            var CategoryShowIds = service.GetAllOnlineShowIds(CountryCode, item);
                            if (CategoryShowIds.Count() > 1000)
                                list.Add(String.Format("{0}+ in {1}", CategoryShowIds.Count().Floor(100), item.Description));
                            else if (CategoryShowIds.Count() > 100)
                                list.Add(String.Format("{0}+ in {1}", CategoryShowIds.Count().Floor(10), item.Description));
                            else if (CategoryShowIds.Count() > 10)
                                list.Add(String.Format("{0}+ in {1}", CategoryShowIds.Count().Floor(10), item.Description));
                            else
                                list.Add(String.Format("{0} in {1}", CategoryShowIds.Count(), item.Description));
                        }
                    }
                    list.Add(String.Format("{0}+ Titles", listOfShowIds.Count().Floor(10)));
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                    cache.Put(cacheKey, jsonString, CacheDuration);
                }
                else
                    list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return list;
        }

        public static List<string> GetPackageFeaturesViaPackage(string CountryCode, Package package)
        {
            List<string> list = null;
            string jsonString = String.Empty;
            try
            {
                var cache = DataCache.Cache;
                //modify cache duration
                var CacheDuration = new TimeSpan(0, GlobalConfig.PackageAndProductCacheDuration, 0);
                string cacheKey = "GPKGFEAT:P:" + package.PackageId + ";C:" + CountryCode;
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { }
                if (String.IsNullOrEmpty(jsonString))
                {
                    list = new List<string>();
                    var context = new IPTV2Entities();
                    var offering = context.Offerings.Find(GlobalConfig.offeringId);
                    var service = offering.Services.FirstOrDefault(o => o.PackageId == GlobalConfig.serviceId);
                    SortedSet<int> listOfShowIds = new SortedSet<int>();

                    foreach (var category in package.Categories)
                    {
                        listOfShowIds.UnionWith(service.GetAllOnlineShowIds(CountryCode, category.Category));
                        if (category.Category is Category)
                        {
                            var item = (Category)category.Category;
                            var CategoryShowIds = service.GetAllOnlineShowIds(CountryCode, item);
                            if (CategoryShowIds.Count() > 1000)
                                list.Add(String.Format("{0}+ in {1}", CategoryShowIds.Count().Floor(100), item.Description));
                            else if (CategoryShowIds.Count() > 100)
                                list.Add(String.Format("{0}+ in {1}", CategoryShowIds.Count().Floor(10), item.Description));
                            else if (CategoryShowIds.Count() > 10)
                                list.Add(String.Format("{0}+ in {1}", CategoryShowIds.Count().Floor(10), item.Description));
                            else
                                list.Add(String.Format("{0} in {1}", CategoryShowIds.Count(), item.Description));
                        }
                    }
                    list.Add(String.Format("{0}+ Titles", listOfShowIds.Count().Floor(10)));
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                    cache.Put(cacheKey, jsonString, CacheDuration);
                }
                else
                    list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(jsonString);
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return list;
        }

        public static IEnumerable<int> GetAllShowsBasedOnCountryCode(IPTV2Entities context, string CountryCode, bool addExclusion = false, Offering offering = null, Service service = null, Category category = null)
        {
            SortedSet<int> list = null;
            try
            {
                if (offering == null)
                    offering = context.Offerings.Find(GlobalConfig.offeringId);
                if (service == null)
                    service = offering.Services.FirstOrDefault(s => s.PackageId == GlobalConfig.serviceId);
                if (category != null)
                    list = service.GetAllOnlineShowIds(CountryCode, category);
                else
                    list = service.GetAllOnlineShowIds(CountryCode);
                if (addExclusion)
                {
                    if (list != null)
                    {
                        var excludedCategoryIds = MyUtility.StringToIntList(GlobalConfig.ExcludedCategoryIdsForDisplay);
                        var result = list.Except(excludedCategoryIds);
                        return result;
                    }
                }
                if (list != null)
                    return list.ToList();
            }
            catch (Exception) { }
            return null;
        }

        public static bool IsUserPartOfPromo(int PromoId, Guid UserId)
        {
            try
            {
                if (UserId != Guid.Empty)
                {
                    var context = new IPTV2Entities();
                    var registDt = DateTime.Now;
                    var user = context.Users.FirstOrDefault(u => u.UserId == UserId);
                    if (user != null)
                    {
                        var promo = context.Promos.FirstOrDefault(p => p.PromoId == PromoId && p.StatusId == GlobalConfig.Visible);
                        if (promo != null)
                            if (promo.StartDate < registDt && promo.EndDate > registDt) //check if promo is within promo date
                                return user.UserPromos.Count(u => u.PromoId == promo.PromoId) > 0;
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return false;
        }

        public static bool isPromoEnabled(int id)
        {
            bool isEnabled = false;
            try
            {
                var context = new IPTV2Entities();
                var registDt = DateTime.Now;
                var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.TFCtvFirstYearAnniversaryPromoId);
                if (promo != null)
                    if ((promo.StartDate < registDt && promo.EndDate > registDt) && promo.StatusId == GlobalConfig.Visible)
                        isEnabled = true;
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return isEnabled;
        }

        public static string GetCategoryDescription(int id)
        {
            try
            {
                var context = new IPTV2Entities();
                var category = context.CategoryClasses.FirstOrDefault(c => c.CategoryId == id);
                if (category != null)
                    return category.Description;
            }
            catch (Exception) { }
            return String.Empty;
        }

        public static CheckSubscriptionReturnObject HasActiveSubscriptionBasedOnProducts(IPTV2Entities context, User user, Offering offering, Product product)
        {
            var returnObject = new CheckSubscriptionReturnObject() { HasSubscription = false, Within5DaysOrLess = false };
            try
            {
                var subProducts = user.GetSubscribedProducts(offering);
                if (product != null)
                {
                    if (product is SubscriptionProduct)
                    {
                        if (product is PackageSubscriptionProduct)
                        {
                            var packageProduct = (PackageSubscriptionProduct)product;
                            returnObject.SubscriptionEndDate = user.PackageEntitlements.FirstOrDefault(u => u.PackageId == packageProduct.Packages.First().PackageId).EndDate;
                        }
                        else if (product is ShowSubscriptionProduct)
                        {
                            var showProduct = (ShowSubscriptionProduct)product;
                            returnObject.SubscriptionEndDate = user.ShowEntitlements.FirstOrDefault(u => u.CategoryId == showProduct.Categories.First().CategoryId).EndDate;
                        }
                        returnObject.HasSubscription = subProducts.Select(s => s.ProductId).Contains(product.ProductId);
                    }
                }

            }
            catch (Exception e) { MyUtility.LogException(e); }
            return returnObject;
        }

        public static RecurringBillingReturnValue CheckIfUserIsEnrolledToSameRecurringProductGroup(IPTV2Entities context, Offering offering, User user, Product product)
        {
            RecurringBillingReturnValue returnValue = new RecurringBillingReturnValue()
            {
                container = null,
                value = false
            };
            var profiler = MiniProfiler.Current;
            using (profiler.Step("Check Recurring Enrolment"))
            {
                try
                {

                    if (product is SubscriptionProduct)
                    {
                        // check if user is part of recurring
                        var subscriptionProduct = (SubscriptionProduct)product;
                        //Get user's recurring productGroups
                        var recurringProductGroups = user.GetRecurringProductGroups(offering);
                        if (recurringProductGroups.Contains(subscriptionProduct.ProductGroup))
                        {
                            var productPackage = context.ProductPackages.FirstOrDefault(p => p.ProductId == product.ProductId);
                            if (productPackage != null)
                            {
                                var entitlement = user.PackageEntitlements.FirstOrDefault(p => p.PackageId == productPackage.PackageId);
                                if (entitlement != null)
                                {
                                    var container = new RecurringBillingContainer()
                                    {
                                        user = user,
                                        product = product,
                                        entitlement = entitlement,
                                        package = (Package)productPackage.Package
                                    };
                                    returnValue.value = true;
                                    returnValue.container = container;
                                }
                            }
                        }
                    }
                }
                catch (Exception e) { MyUtility.LogException(e); }
            }
            return returnValue;
        }

        public static bool IsProductPurchaseable(IPTV2Entities context, Product product, User user, Offering offering)
        {
            PackageSubscriptionProduct subscription = (PackageSubscriptionProduct)product;
            var subscribedProducts = user.GetSubscribedProductGroups(offering);
            foreach (var item in subscribedProducts)
            {
                //checks if product to be bought and subscribed product belong to the same group
                if (item.ProductGroupId != subscription.ProductGroupId) //does not belong to same group
                {
                    if (item.UpgradeableFromProductGroups().Contains(subscription.ProductGroup))
                        return false;
                }
            }
            return true;
        }

        public static bool DoesEpisodeHaveAkamaiHDCdnReferenceBasedOnAsset(Episode episode)
        {
            try
            {
                if (episode != null)
                {
                    var premiumAsset = episode.PremiumAssets.LastOrDefault();
                    if (premiumAsset != null)
                    {
                        var asset = premiumAsset.Asset;
                        return asset.AssetCdns.Count(a => a.CdnId == 6) > 0;
                    }
                }
            }
            catch (Exception) { }
            return false;
        }

        public static bool DoesEpisodeHaveAkamaiCdnReferenceBasedOnAsset(Episode episode)
        {
            try
            {
                if (episode != null)
                {
                    var premiumAsset = episode.PremiumAssets.LastOrDefault();
                    if (premiumAsset != null)
                    {
                        var asset = premiumAsset.Asset;
                        return asset.AssetCdns.Count(a => a.CdnId == 2) > 0;
                    }
                }
            }
            catch (Exception) { }
            return false;
        }

        public static bool IsXoomEligible(IPTV2Entities context, User user)
        {
            var registDt = DateTime.Now;
            try
            {
                var promo = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.Xoom2PromoId);
                if (promo != null)
                    if (promo.StatusId == GlobalConfig.Visible && promo.StartDate < registDt && promo.EndDate > registDt)
                        return context.UserPromos.Count(u => u.UserId == user.UserId && u.PromoId == promo.PromoId && u.AuditTrail.UpdatedOn == null) > 0;

            }
            catch (Exception) { }
            return false;
        }

        public static bool SaveSessionInDatabase(IPTV2Entities context, User user, string value)
        {
            if (GlobalConfig.IsPreventionOfMultipleLoginUsingSessionDatabaseEnabled)
            {
                try
                {
                    user.SessionId = value;
                    user.SessionLoggedDate = DateTime.Now;
                    return context.SaveChanges() > 0;
                }
                catch (Exception) { }
            }
            return false;
        }

        public static string BuildDax(string parentCategoryIds, string title, DateTime? DateAired)
        {
            try
            {
                var tfcParentIds = MyUtility.StringToIntList(GlobalConfig.SiteUmbrellaCategoryIds);
                var parentIds = MyUtility.StringToIntList(parentCategoryIds);
                var context = new IPTV2Entities();

                IEnumerable<int> subCategoryIds = Enumerable.Empty<int>();
                List<int> subCatIds = null;
                var cache = DataCache.Cache;
                string jsonString = String.Empty;
                string cacheKey = "DAX:U:SubCats";
                try { jsonString = (string)cache[cacheKey]; }
                catch (Exception) { DataCache.Refresh(); }

                if (String.IsNullOrEmpty(jsonString))
                {
                    subCategoryIds = tfcParentIds;
                    var tfcParents = context.CategoryClasses.Where(c => tfcParentIds.Contains(c.CategoryId));
                    if (tfcParents != null)
                    {
                        foreach (var cat in tfcParents)
                        {
                            subCategoryIds = subCategoryIds.Union(cat.CategoryClassSubCategories.Select(c => c.CategoryId));
                        }
                    }

                    subCatIds = subCategoryIds.ToList();
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(subCatIds);
                    cache.Put(cacheKey, jsonString, DataCache.CacheDuration);
                }
                else
                    subCatIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(jsonString);

                var allowedParentIds = parentIds.Intersect(subCatIds);
                var cats = context.CategoryClasses.Where(c => allowedParentIds.Contains(c.CategoryId) && c.StatusId == GlobalConfig.Visible);
                if (cats != null)
                {
                    if (cats.Count() >= 2)
                    {
                        //get first item
                        var first = cats.FirstOrDefault();
                        if (first != null)
                            if (!tfcParentIds.Contains(first.CategoryId))
                                cats = cats.OrderByDescending(c => c.CategoryId);
                        var categories = cats.Take(2).ToList();
                        var daxDetail = string.Join("|", categories.Select(c => c.Description));
                        daxDetail = MyUtility.GetDaxSlug(daxDetail);
                        string airdate = String.Empty;
                        if (DateAired != null)
                            airdate = ((DateTime)DateAired).ToString("yyyyMMdd");
                        else
                            return String.Format("tfc-tv:{0}:{1}", daxDetail.ToLower(), MyUtility.GetDaxSlug(title));
                        return String.Format("tfc-tv:{0}:{1}:{2}", daxDetail.ToLower(), MyUtility.GetDaxSlug(title), airdate);
                    }
                }
            }
            catch (Exception e) { MyUtility.LogException(e); }
            return String.Empty;
        }

        public static NetsuiteReturnObj IsNetsuiteUnderMaintenance()
        {
            var registDt = DateTime.Now;
            var obj = new NetsuiteReturnObj()
            {
                IsUnderMaintenance = false,
                StatusMessage = String.Empty
            };
            try
            {
                using (var context = new IPTV2Entities())
                {
                    var setting = context.Promos.FirstOrDefault(p => p.PromoId == GlobalConfig.NetsuiteMaintenancePromoId && p.StatusId == GlobalConfig.Visible);
                    if (setting != null)
                    {
                        if (setting.StartDate < registDt && setting.EndDate > registDt)
                        {
                            obj.IsUnderMaintenance = true;
                            obj.StatusMessage = setting.Description;
                        }

                    }
                }
            }
            catch (Exception) { }
            return obj;
        }

        public static StreamSenseObj CreateStreamSenseObject(Episode episode, Show show)
        {
            string show_name = show.Description.Trim();
            var streamSenseObj = new StreamSenseObj()
            {
                dateaired = episode.DateAired,
                id = episode.EpisodeId,
                playlist = String.Format("{0} episodes", show_name),
                program = show_name,
                IsEpisode = (show is IPTV2_Model.Movie || show is IPTV2_Model.LiveEvent) ? false : true
            };

            if (show is IPTV2_Model.Movie)
            {
                streamSenseObj.playlist = String.Format("{0} movie", show_name);
            }
            else if (show is IPTV2_Model.LiveEvent)
            {
                streamSenseObj.playlist = String.Format("{0} live event", show_name);
            }
            return streamSenseObj;
        }
    }
}
