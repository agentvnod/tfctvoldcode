using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using IPTV2_Model;
using GOMS_TFCtv;

namespace GOMS_TFCtvSync
{
    public class ExchangeRate : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Forex update is executing. Time now is " + DateTime.UtcNow);
            Send();
        }

        private void Send()
        {
            try
            {
                Console.WriteLine("Updating Forex Exchange Rate...");
                var context = new IPTV2Entities();
                var service = new GomsTfcTv();
                service.GetExchangeRates(context, "USD");
                Console.WriteLine("Updating of Forex Exchange Rate Complete!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Outer Exception: " + e.Message);
            }
        }
    }
}
