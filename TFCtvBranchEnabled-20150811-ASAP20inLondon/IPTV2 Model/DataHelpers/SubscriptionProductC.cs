using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    [Serializable]
    public partial class SubscriptionProductC : IComparable<SubscriptionProductC>
    {
        public SubscriptionProductC()
        {
            this.ProductPrices = new SortedSet<ProductPrice>();
            this.AllowedCountries = new HashSet<string>();
            this.BlockedCountries = new HashSet<string>();
        }

        public static SortedSet<SubscriptionProductC> LoadAll(IPTV2Entities context, int offeringId)
        {
            return LoadAll(context, offeringId, true);
        }

        public static SortedSet<SubscriptionProductC> LoadAll(IPTV2Entities context, int offeringId, bool useCache)
        {
            return LoadAll(context, offeringId, useCache, DataCache.CacheDuration);
        }

        public static SortedSet<SubscriptionProductC> LoadAll(IPTV2Entities context, int offeringId, bool useCache, TimeSpan cacheDuration)
        {
            SortedSet<SubscriptionProductC> productCs = null;
            string cacheKey = "SPC:O:" + offeringId.ToString();
            var cache = DataCache.Cache;

            if (useCache)
            {
                productCs = (SortedSet<SubscriptionProductC>)cache[cacheKey];
            }

            if (productCs == null)
            {
                var products = context.Products.Where(p => p.OfferingId == offeringId && p is SubscriptionProduct && p.StatusId == 1);
                if (products.Count() > 0)
                {
                    productCs = new SortedSet<SubscriptionProductC>();
                    foreach (var product in products)
                    {
                        productCs.Add(SubscriptionProductC.Load(context, offeringId, product.ProductId, false));
                    }
                    if (useCache)
                    {
                        cache.Put(cacheKey, productCs, cacheDuration);
                    }
                }

            }

            return productCs;
        }

        public static SubscriptionProductC Load(IPTV2Entities context, int offeringId, int productId)
        {
            return Load(context, offeringId, productId, true);
        }

        public static SubscriptionProductC Load(IPTV2Entities context, int offeringId, int productId, bool useCache)
        {
            SubscriptionProductC productC = null;
            string cacheKey = "SPC:O:" + offeringId.ToString() + ";P:" + productId.ToString();
            var cache = DataCache.Cache;

            if (useCache)
            {
                productC = (SubscriptionProductC)cache[cacheKey];
            }

            if (productC == null)
            {
                var product = context.Products.FirstOrDefault(p => p.ProductId == productId && p.OfferingId == offeringId && p is SubscriptionProduct && p.StatusId == 1);
                if (product != null)
                {
                    var p = (SubscriptionProduct)product;

                    if (p is EpisodeSubscriptionProduct)
                    {
                        productC = new EpisodeSubscriptionProductC();
                    }
                    else if (p is ShowSubscriptionProduct)
                    {
                        productC = new ShowSubscriptionProductC();
                    }
                    else if (p is PackageSubscriptionProduct)
                    {
                        productC = new PackageSubscriptionProductC();
                    }
                    else
                    {
                        throw new Exception("Invalide subscription product.");
                    }

                    productC.ProductId = p.ProductId;
                    productC.Name = p.Name;
                    productC.Description = p.Description;
                    productC.OfferingId = p.OfferingId;
                    productC.StatusId = p.StatusId;
                    productC.IsForSale = p.IsForSale;
                    productC.ProductGroupId = p.ProductGroupId;
                    productC.Duration = p.Duration;
                    productC.DurationType = p.DurationType;

                    switch (p.DurationType.ToUpper())
                    {
                        case "D":
                            {
                                productC.DurationInDays = p.Duration;
                                break;
                            }
                        case "M":
                            {
                                productC.DurationInDays = p.Duration * 30;
                                break;
                            }
                        case "Y":
                            {
                                productC.DurationInDays = p.Duration * 365;
                                break;
                            }
                        default:
                            {
                                throw new Exception("Invalid duration type.");
                            }

                    }

                    // fill prices
                    var prices = context.ProductPrices.Where(pp => pp.ProductId == p.ProductId);
                    foreach (var price in prices)
                    {
                        productC.ProductPrices.Add(new ProductPrice { CurrencyCode = price.CurrencyCode, Amount = price.Amount, Symbol = price.Currency.Symbol, IsLeft = price.Currency.IsLeft });
                    }
                    if (useCache)
                    {
                        cache.Put(cacheKey, productC, DataCache.CacheDuration);
                    }

                    // fill allowed/blocked countries
                    var countries = context.Countries.ToList();
                    foreach (var country in countries)
                    {
                        if (p.IsAllowed(country.Code))
                        {
                            productC.AllowedCountries.Add(country.Code);
                        }
                        else
                        {
                            productC.BlockedCountries.Add(country.Code);
                        }
                    }
                }
                else
                    productC = null;
            }
            return productC;
        }

        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OfferingId { get; set; }
        public byte StatusId { get; set; }
        public bool IsForSale { get; set; }
        public int Duration { get; set; }
        public string DurationType { get; set; }
        public int ProductGroupId { get; set; }
        public int DurationInDays { get; set; }

        public ICollection<ProductPrice> ProductPrices { get; set; }
        public ICollection<string> AllowedCountries { get; set; }
        public ICollection<string> BlockedCountries { get; set; }

        public int CompareTo(SubscriptionProductC other)
        {
            return (ProductId.CompareTo(other.ProductId));
        }

        public decimal GetPrice(string currencyCode)
        {
            var price = ProductPrices.FirstOrDefault(p => p.CurrencyCode == currencyCode);
            return price == null ? -1 : price.Amount;
        }

        public ProductPrice GetProductPrice(string currencyCode)
        {
            return ProductPrices.FirstOrDefault(p => p.CurrencyCode == currencyCode);            
        }

        public bool IsAllowed(string countryCode)
        {
            return AllowedCountries.Contains(countryCode);
        }

        [Serializable]
        public class ProductPrice : IComparable<ProductPrice>
        {
            public string CurrencyCode { get; set; }
            public decimal Amount { get; set; }
            public string Symbol { get; set; }
            public bool IsLeft { get; set; }

            public int CompareTo(ProductPrice other)
            {
                return (CurrencyCode.CompareTo(other.CurrencyCode));
            }
        }

        public class PackageSubscriptionProductC : SubscriptionProductC
        {
        }

        public class ShowSubscriptionProductC : SubscriptionProductC
        {
        }

        public class EpisodeSubscriptionProductC : SubscriptionProductC
        {
        }


    }


}
