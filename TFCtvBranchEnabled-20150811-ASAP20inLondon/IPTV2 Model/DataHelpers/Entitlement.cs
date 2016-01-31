using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class Entitlement
    {
        public static Entitlement GetUserProductEntitlement(IPTV2Entities context, System.Guid userId, int productId)
        {
            var user = context.Users.Find(userId);
            if (user == null)
            {
                throw new Exception(String.Format("Invalid user ID {0}", userId));
            }

            var product = context.Products.Find(productId);
            if (product == null)
            {
                throw new Exception(String.Format("Invalid product ID {0}", productId));
            }

            // look for user's entitlement of purchased product
            Entitlement userEntitlement = null;
            if (product is PackageSubscriptionProduct)
            {
                var pProduct = (PackageSubscriptionProduct)product;
                foreach (var p in pProduct.Packages)
                {
                    userEntitlement = user.PackageEntitlements.FirstOrDefault(pp => pp.PackageId == p.PackageId);
                }
            }
            else if (product is ShowSubscriptionProduct)
            {
                var sProduct = (ShowSubscriptionProduct)product;
                foreach (var s in sProduct.Categories)
                {
                    userEntitlement = user.ShowEntitlements.FirstOrDefault(ss => ss.CategoryId == s.CategoryId);
                }

            }
            else if (product is EpisodeSubscriptionProduct)
            {
                var eProduct = (EpisodeSubscriptionProduct)product;
                foreach (var e in eProduct.Episodes)
                {
                    userEntitlement = user.EpisodeEntitlements.FirstOrDefault(ee => ee.EpisodeId == e.EpisodeId);
                }

            }
            else
            {
                throw new Exception(String.Format("Invalid product type for product Id {0}-{1}.", product.ProductId, product.Name));
            }

            return (userEntitlement);
        }

    }
}
