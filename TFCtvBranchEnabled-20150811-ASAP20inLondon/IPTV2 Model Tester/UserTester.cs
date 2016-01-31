using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    public class UserTester
    {
        public static void EntitlePackage()
        {
            var context = new IPTV2Entities();
            var user = context.Users.First(u => u.EMail == "eugene_paden@abs-cbn.com");
            var product = context.Products.Find(5);
            // user.Entitle((PackageSubscriptionProduct)product);
        }

        public static void GetPackageProducts()
        {
            var context = new IPTV2Entities();
            var user = context.Users.First(u => u.EMail == "eugene_paden@abs-cbn.com");
            int offeringId = 2;
            var offering = context.Offerings.Find(offeringId);
            var subscribedProducts = user.GetSubscribedProducts(offering);
            var subscribedProductGroups = user.GetSubscribedProductGroups(offering);
            var pg = context.ProductGroups.Find(5);
            var upgradeProductGroups = pg.UpgradeableToProductGroups();
            var upgradeProducts = pg.UpgradeableToProducts();
        }

        public static void GetEntitlements()
        {
            var context = new IPTV2Entities();
            var user = context.Users.First(u => u.EMail == "albin.lim@gmail.com");
            int offeringId = 2;
            var offering = context.Offerings.Find(offeringId);
            var pe = user.GetProductEntitlements(offering);
            var pge = user.GetProductGroupEntitlements(offering);

            var premiumGroup = context.ProductGroups.Find(1);
            var premiumExpiration = user.GetProductGroupExpiration(premiumGroup);
            var liteGroup = context.ProductGroups.Find(5);
            var liteExpiration = user.GetProductGroupExpiration(liteGroup);
            var mmkGroup = context.ProductGroups.Find(10);
            var mmkExpiration = user.GetProductGroupExpiration(mmkGroup);
        }
    }
}
