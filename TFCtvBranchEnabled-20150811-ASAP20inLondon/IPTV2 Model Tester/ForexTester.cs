using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    class ForexTester
    {
        public static void ConversionTests()
        {
            var context = new IPTV2Entities();
            decimal amount = 500;
            amount = Forex.Convert(context, "USD", "AED", amount);
            amount = Forex.Convert(context, "AED", "USD", amount);
            amount = Forex.Convert(context, "MYR", "AED", amount);
            amount = Forex.Convert(context, "AED", "MYR", amount);
            
        }
    }
}
