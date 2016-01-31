using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;

namespace IPTV2_Model_Tester
{
    public class PpcTester
    {
        public static void PpcTypeTester()
        {
            var context = new IPTV2Entities();
            var regularPpc = (SubscriptionPpc)context.Ppcs.FirstOrDefault(p => p.PpcProductId == 1002);
            var trialPpc = (SubscriptionPpc)context.Ppcs.FirstOrDefault(p => p.PpcProductId == 1090);
            var compensatoryPpc = (SubscriptionPpc)context.Ppcs.FirstOrDefault(p => p.PpcProductId == 1093);
            var complimentaryPpc = (SubscriptionPpc)context.Ppcs.FirstOrDefault(p => p.PpcProductId == 1094); 
        }

        public static void TestValidate()
        {
            var context = new IPTV2Entities();
            var returnCode = Ppc.Validate(context, "AABJL0000001", "982A765329EDBD9A389B50A8BC54616282F5ED8F", "USD"); // success
            var amount = Ppc.GetPpcAmount(context, "AABJL0000001", "982A765329EDBD9A389B50A8BC54616282F5ED8F", "USD");

            var ppc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == "AABJL0000001");
            returnCode = ppc.Validate("982A765329EDBD9A389B50A8BC54616282F5ED8F", "USD");
            amount = ppc.GetAmount("USD");

            var aedSerialNumber = "LABJL0000001";
            var aedPin = "1EB89BC82C5538A40B8C854F8F1A25E4FBA79431";
            var aedError = Ppc.Validate(context, aedSerialNumber, aedPin, "AED"); // success
            var bhdError = Ppc.Validate(context, aedSerialNumber, aedPin, "BHD"); // success
            var jodError = Ppc.Validate(context, aedSerialNumber, aedPin, "JOD"); // success
            var kwdError = Ppc.Validate(context, aedSerialNumber, aedPin, "KWD"); // success
            var lbpError = Ppc.Validate(context, aedSerialNumber, aedPin, "LBP"); // success
            var omrError = Ppc.Validate(context, aedSerialNumber, aedPin, "OMR"); // success
            var qarError = Ppc.Validate(context, aedSerialNumber, aedPin, "QAR"); // success
            var usdAedError = Ppc.Validate(context, aedSerialNumber, aedPin, "USD"); // fail, currency

            var aedAmount = Ppc.GetPpcAmount(context, aedSerialNumber, aedPin, "AED");
            var bhdAmount = Ppc.GetPpcAmount(context, aedSerialNumber, aedPin, "BHD");
            var jodAmount = Ppc.GetPpcAmount(context, aedSerialNumber, aedPin, "JOD");
            var kwdAmount = Ppc.GetPpcAmount(context, aedSerialNumber, aedPin, "KWD");
            var lbpAmount = Ppc.GetPpcAmount(context, aedSerialNumber, aedPin, "LBP");
            var omrAmount = Ppc.GetPpcAmount(context, aedSerialNumber, aedPin, "OMR");
            var qarAmount = Ppc.GetPpcAmount(context, aedSerialNumber, aedPin, "QAR");

            var aedPpc = context.Ppcs.FirstOrDefault(p => p.SerialNumber == aedSerialNumber);
            var pAedAmount = aedPpc.GetAmount("AED");
            var pBhdAmount = aedPpc.GetAmount("BHD");
            var pJodAmount = aedPpc.GetAmount("JOD");
            var pKwdAmount = aedPpc.GetAmount("KWD");
            var pLbpAmount = aedPpc.GetAmount("LBP");
            var pOmrAmount = aedPpc.GetAmount("OMR");
            var pQarAmount = aedPpc.GetAmount("QAR");

            var currencyError = Ppc.Validate(context, "AABJL0000001", "982A765329EDBD9A389B50A8BC54616282F5ED8F", "CAD"); // currency
            var serialError = Ppc.Validate(context, "AABJL0000000", "982A765329EDBD9A389B50A8BC54616282F5ED8F", "USD"); // serial
            var pinError = Ppc.Validate(context, "AABJL0000001", "982A765329EDBD9A389B50A8BC54616282F5ED8G", "USD"); // pin
            var usedError = Ppc.Validate(context, "BCBJL0000001", "77C0A0CDB1FC8B8477B716F9239CCC3D546EE694", "CAD"); // used
            var notAReloadError = Ppc.ValidateReloadPpc(context, "BEBJL0000020", "5D56A3F7ACB4BF363AAD4AE3B0904F9306A28D2D", "CAD"); // not a reload

            var product = context.Products.Find(3);
            returnCode = Ppc.ValidateSubscriptionPpc(context, "BDBJL0000001", "109FE3B9C2948C39B564889B3C8288122B1CEAD2", "CAD", product); // success

            var wrongProduct = context.Products.Find(1);
            var wrongProductError = Ppc.ValidateSubscriptionPpc(context, "BDBJL0000001", "109FE3B9C2948C39B564889B3C8288122B1CEAD2", "CAD", wrongProduct); // wrong product

            var notASubscriptionError = Ppc.ValidateSubscriptionPpc(context, "AABJL0000001", "982A765329EDBD9A389B50A8BC54616282F5ED8F", "USD", wrongProduct); // not a subscription
        }
    }
}
