using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GOMS_TFCtv;

namespace GOMS_TFCtv_Tests
{
    public class Forex
    {
        public static void GetExchangeRates()
        {
            var context = new IPTV2_Model.IPTV2Entities();
            var gomsService = new GomsTfcTv();

            gomsService.GetExchangeRates(context, "USD");

        }
    }
}
