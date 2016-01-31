using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFCtvAPI_WS_Tester.TFCtvAPI;

namespace TFCtvAPI_WS_Tester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TFCtvAPISoapClient client = new TFCtvAPISoapClient();
            AuthenticationHeader header = new AuthenticationHeader() { Username = "tfctvnetsuite01", Password = "Dm07k2F54jUf09" };

            //ReqCreateTFCtvEverywhereEntitlement req = new ReqCreateTFCtvEverywhereEntitlement()
            //{
            //    EmailAddress = "istarbuxs@hotmail.com",
            //    GomsCustomerId = 1101294,
            //    GomsProductId = 17112,
            //    GomsProductQuantity = 1,
            //    GomsTFCEverywhereStartDate = Convert.ToDateTime("2013/01/01"),
            //    GomsTFCEverywhereEndDate = Convert.ToDateTime("2013/01/31"),
            //    GomsTransactionDate = Convert.ToDateTime("2013/01/01"),
            //    GomsTransactionId = 100000,
            //    Reference = "TFC.tv Everywhere (GOMS)"

            //};

            //var response = client.CreateTFCtvEverywhereEntitlement(header, req);

            ReqUnassociateTVEverywhere req = new ReqUnassociateTVEverywhere()
            {
                GomsCustomerId = 2520184,
                GomsTransactionDate = DateTime.Now,
                GomsTransactionId = 10515275,
                Reference = "UNCLAIMED",
            };
            var response = client.UnassociateTVEverywhere(header, req);
            Console.WriteLine(String.Format("Code: {0}, Message: {1}", response.Code, response.Message));

            //ReqActivatePpc req = new ReqActivatePpc()
            //{
            //    PpcStart = "MBAAD0000035",
            //    PpcEnd = "MBAAD0000045",
            //    ActivatedBy = "gomsuserid",
            //    StatusId = 0
            //};
            //var resp = client.TogglePpc(header, req);
            //Console.WriteLine(String.Format("Code: {0}, Message: {1}", resp.Code, resp.Message));
            Console.ReadLine();

            //ReqReloadWalletViaSmartPit req2 = new ReqReloadWalletViaSmartPit()
            //{
            //    Amount = 1150,
            //    GomsCustomerId = 1878159,
            //    GomsTransactionDate = Convert.ToDateTime("2012/08/01"),
            //    GomsTransactionId = 15141856,
            //    GomsWalletId = 21148
            //};
            //var resp2 = client.ReloadWalletViaSmartPit(header, req2);
            //Console.WriteLine(String.Format("Code: {0}, Message: {1}", resp2.Code, resp2.Message));

            //ReqUpdateSmartPit req3 = new ReqUpdateSmartPit()
            //{
            //    GomsCustomerId = 1100994,
            //    SmartPitCardNo = null
            //};
            //var resp3 = client.UpdateSmartPit(header, req3);
            //Console.WriteLine(String.Format("Code: {0}, Message: {1}", resp3.Code, resp3.Message));
        }
    }
}