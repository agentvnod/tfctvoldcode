using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class Product : IComparable<Product>
    {
        public int CompareTo(Product otherProduct)
        {
            return (ProductId.CompareTo(otherProduct.ProductId));
        }

        /// <summary>
        /// Checks whether product can be sold in Country
        /// </summary>
        /// <param name="country">Country to validate against</param>
        /// <returns>True if allowed for country</returns>
        public bool IsAllowed(Country country)
        {
            return ((country == null) ? false : IsAllowed(country.Code));
        }

        /// <summary>
        /// Checks whether product can be sold in Country
        /// </summary>
        /// <param name="countryCode">CountryCode to validate against</param>
        /// <returns>True if allowed for countryCode</returns>
        public bool IsAllowed(string countryCode)
        {
            int acCount = AllowedCountries.Where(c => c.StatusId == 1).Count();
            int bcCount = BlockedCountries.Where(c => c.StatusId == 1).Count();

            // nothing defined, so implicitly allowed
            if ((acCount == 0) && (bcCount == 0))
                return (true);

            // explicitly blocked
            if (BlockedCountries.FirstOrDefault(c => String.Compare(c.CountryCode, countryCode, true) == 0 && c.StatusId == 1) != null)
                return (false);

            // nothing defined (implicitly allow)  or explicitly allowed
            if ((acCount == 0) || (AllowedCountries.FirstOrDefault(c => String.Compare(c.CountryCode, countryCode, true) == 0 && c.StatusId == 1) != null))
                return (true);

            return (false);
        }


        /// <summary>
        /// Check if product has a price for a specific country
        /// </summary>
        /// <param name="country">Country to check price for</param>
        /// <returns>True if price is defined for the country</returns>
        public bool HasPrice(Country country)
        {
            return (HasPrice(country.CurrencyCode));
        }

        /// <summary>
        /// Check if product has a price for a specific currency
        /// </summary>
        /// <param name="currencyCode">Currency to check price for</param>
        /// <returns>True if price is defined for the currencyCode</returns>
        public bool HasPrice(string currencyCode)
        {
            return (ProductPrices.FirstOrDefault(pp => pp.CurrencyCode == currencyCode) != null);
        }

        /// <summary>
        /// Get product's price for a particular country.
        /// </summary>
        /// <param name="country">Country to get price for</param>
        /// <returns>-1 if product is not allowed in country or no price defined for country, otherwise Product's price is returned</returns>
        public decimal GetPrice(Country country)
        {
            decimal price = -1;
            if (IsAllowed(country) && HasPrice(country))
                price = ProductPrices.FirstOrDefault(pp => pp.CurrencyCode == country.CurrencyCode).Amount;
            return price;
        }
    }
}
