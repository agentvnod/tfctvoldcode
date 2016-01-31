using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    class ProductGroupTester
    {

        public static void TestGroup()
        {
            var context = new IPTV2Entities();
            var productGroup = context.ProductGroups.Find(1);
            var upgradeableFromPGs = productGroup.UpgradeableFromProductGroups();
            var packageIds = productGroup.GetPackageIds(false);
            var showIds = productGroup.GetShowIds(false);
        }
    }
}
