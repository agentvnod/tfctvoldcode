using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GOMS_TFCtv;
using IPTV2_Model;

namespace GOMS_TFCtv_Tests
{
    class CreditCard
    {

        public static CreditCardInfo GetCreditCard()
        {
            return new CreditCardInfo
            {
                CardType = CreditCardType.American_Express,
                Name = "Eugene Paden",
                Number = "4111111111111111",
                CardSecurityCode = "123",
                ExpiryMonth = 6,
                ExpiryYear = 2012,
                StreetAddress = "150 shoreline dr",
                PostalCode = "94002"
            };
        }
        

        static public void CreditCardReload()
        {
            var context = new IPTV2_Model.IPTV2Entities();
            var gomsService = new GomsTfcTv();

            var user = context.Users.FirstOrDefault(u => u.EMail == "eugenecp@hotmail.com");
            if (user != null)
            {
                // create transaction
                var transaction = new IPTV2_Model.CreditCardReloadTransaction
                    {
                        Amount = 10,
                        Currency = user.Country.CurrencyCode,
                        Date = DateTime.Now,
                    };

                var resp = gomsService.ReloadWalletViaCreditCard(context, user.UserId, transaction, GetCreditCard());
                if (resp.IsSuccess)
                {
                    // YEY
                }

            }

        }

        static public void CreditCardPurchase()
        {
            var context = new IPTV2Entities();
            var gomsService = new GomsTfcTv();

            var user = context.Users.FirstOrDefault(u => u.EMail == "eugene_paden@abs-cbn.com");
            if (user != null)
            {

                var product = (SubscriptionProduct)context.Products.Find(1);
                var currency = user.Country.Currency;
                var productPrice = product.ProductPrices.FirstOrDefault(pp => pp.CurrencyCode == user.Country.CurrencyCode);

                var amount = productPrice.Amount;

                // create purchase item
                var purchaseItem = new PurchaseItem
                {
                    Currency = currency.Code,
                    SubscriptionProduct = product,
                    Price = productPrice.Amount,
                    User = user,
                    RecipientUserId = user.UserId
                };

                // create purchase
                var purchase = new Purchase
                {
                    Date = DateTime.Now,
                    User = user,
                    PurchaseItems = new List<PurchaseItem> { purchaseItem },                     
                };

                // create transaction
                var transaction = new CreditCardPaymentTransaction
                {
                      Date = DateTime.Now,
                      Purchase = purchase,
                      Currency = currency.Code, 
                      Amount = amount,                         
                };

                var resp = gomsService.CreateOrderViaCreditCard(context, user.UserId, transaction, GetCreditCard());
                if (resp.IsSuccess)
                {
                    // YEY
                }

            }

        }
    }
}
