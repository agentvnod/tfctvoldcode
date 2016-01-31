using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    class EntitlementTester
    {
        public static void GetEntitlement()
        {
            var context = new IPTV2Entities();
            var user = context.Users.First(u => u.EMail == "eugenecp@gmail.com");
            var entitlement1 = Entitlement.GetUserProductEntitlement(context, user.UserId, 1);
            var entitlement2 = Entitlement.GetUserProductEntitlement(context, user.UserId, 2);

        }
    }
}
