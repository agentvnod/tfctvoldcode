using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    public class ProductTester
    {
        public static void TestProducts()
        {
            var context = new IPTV2Entities();
            var lite1M = context.Products.Find(5);
            var lite3M = context.Products.Find(6);
            var lite12M = context.Products.Find(7);

            var is1MUsAllowed = lite1M.IsAllowed("US"); // false
            var is3MUsAllowed = lite3M.IsAllowed("US"); // false
            var is12MUsAllowed = lite12M.IsAllowed("US"); //false

            var is1MJpAllowed = lite1M.IsAllowed("JP"); // true
            var is3MJpAllowed = lite3M.IsAllowed("JP"); // true
            var is12MJpAllowed = lite12M.IsAllowed("JP"); //true


            var prem1M = context.Products.Find(1);
            var prem3M = context.Products.Find(3);
            var prem12M = context.Products.Find(4);
            var prem10D = context.Products.Find(2);

            var isPrem1M = prem1M.IsAllowed("US"); // true
            var isPrem3M = prem3M.IsAllowed("US"); // true
            var isPrem12M = prem12M.IsAllowed("US");  // true
            var isPrem10D = prem10D.IsAllowed("US");  // true

            var ae = context.Countries.Find("AE");
            var aePricePrem1M = prem1M.GetPrice(ae);

            var us = context.Countries.Find("US");
            var usPriceLite1M = lite1M.GetPrice(us);

        }
    }
}

