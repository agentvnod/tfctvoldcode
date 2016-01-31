using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class Forex
    {
        static string DefaultCurrency = "USD";  // Use for cross conversion

        public static decimal Convert(IPTV2Entities context, string baseCurrency, string targetCurrency, decimal amount)
        {
            decimal  newAmount = amount;

            if (baseCurrency == targetCurrency)
                return amount;

            var thisRate = context.Forexes.Find(new string[] { baseCurrency, targetCurrency });
            if (thisRate != null)
            {
                return (decimal)((double)amount * thisRate.ExchangeRate);
            }
            else
            {
                thisRate = context.Forexes.Find(new string[] { targetCurrency, baseCurrency });
                if (thisRate != null)
                {
                    return (decimal)((double)amount / thisRate.ExchangeRate);
                }
                else
                {
                    // convert baseCurrency to DefaultCurrency first
                    thisRate = context.Forexes.Find(new string[] { DefaultCurrency, baseCurrency });
                    if (thisRate == null) throw new Exception("Cannot convert from base currency.");
                    newAmount = (decimal)((double)amount / thisRate.ExchangeRate);

                    // convert DefaultCurrency to targetCurrency
                    thisRate = context.Forexes.Find(new string[] { DefaultCurrency, targetCurrency });
                    if (thisRate == null) throw new Exception("Cannot convert to target currency.");
                    newAmount = (decimal)((double)newAmount * thisRate.ExchangeRate);
                }
            }

            return (newAmount);
        }
    }
}
