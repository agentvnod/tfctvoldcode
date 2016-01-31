using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    class SubscriptionProductCTester
    {
        public static void LoadProduct()
        {
            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(2);
            var lite1M = context.Products.Find(5);
            // var productC = SubscriptionProductC.Load(context, 2, 5, false);
            var x = SubscriptionProductC.LoadAll(context, 2, true, new TimeSpan(0,0,5));            
            var productCs = SubscriptionProductC.LoadAll(context, 2, false);
            var productCsInUs = productCs.Where(p => p.IsAllowed("US"));
            var show = (Show)context.CategoryClasses.Find(1515); // pintada
            var packageProductIds = show.GetPackageProductIds(offering, "JP", RightsType.Online, false);
            var showProductIds = show.GetShowProductIds(offering, "JP", RightsType.Online, false);
            var packageProducts = from product in productCs
                                  join id in packageProductIds
                                  on product.ProductId equals id
                                  where product.IsForSale
                                  select product;
            var showProducts = from product in productCs
                               join id in showProductIds
                               on product.ProductId equals id
                               where product.IsForSale
                               select product;
            var packageGroupProducts = from product in packageProducts
                                       group product by product.ProductGroupId into ProductGroup
                                       orderby ProductGroup.Key
                                       select new
                                       {
                                           GroupId = ProductGroup.Key,
                                           Products =
                                           (
                                           from product2 in ProductGroup
                                           orderby product2.DurationInDays descending
                                           select product2
                                           )
                                       };
            foreach (var g in packageGroupProducts)
            {
                Console.WriteLine(g.GroupId);
                foreach (var p in g.Products)
                {
                    Console.WriteLine(p.Name);
                }
            }
        }
    }
}
