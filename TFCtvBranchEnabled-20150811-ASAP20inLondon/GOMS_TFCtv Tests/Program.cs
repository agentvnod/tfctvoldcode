using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GOMS_TFCtv;
using IPTV2_Model;
using Xunit;

namespace GOMS_TFCtv_Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // TestConnectivity();
            // UserRegistration.TestRegisterUser();
            // UserRegistration.TestRegisterAllUsers();
            // TestGetUser();  //passed
            // PpcTests.LoadUatPpcs();
            // PpcTests.LoadMoreUatPpcs();
            // PpcTests.LoadUatTrialPpcs();
            // PpcTests.ExportAllPpcs();
            // PpcTests.GomsExportAll();
            //PpcTests.ExportPpc("AEBJL0000001", "AEBJL0000020");
            //PpcTests.ExportPpc("AABJL0000001", "AABJL0000020");

            // CreditCard.CreditCardReload();
            // CreditCard.CreditCardPurchase();

            // Forex.GetExchangeRates();

            // Case.CreateCase();

            // Transaction.PpcReload();
            // Transaction.ProcessAllGomsPendingTransactions();
            //Transaction.ProcessAllGomsPendingTransactionsForUser();

            //int offeringid = 2;
            //string email = "istarbuxs@gmail.com";
            //var service = new GomsTfcTv();
            //var context = new IPTV2Entities();
            //var offering = context.Offerings.Find(offeringid);
            //var user = context.Users.FirstOrDefault(u => u.EMail == email);
            //service.ProcessAllPendingTransactionsInGoms(context, offering, user);
            //Console.ReadLine();
            //TestGetWallet();

            int offeringid = 2;
            string emails = "roa_emma@yahoo.com,kitty.calalang3@gmail.com,istarbuxs@gmail.com,tfctv099@gmail.com";
            emails = "joanna.choa@gmail.com";
            var list = emails.Split(',');
            var service = new GomsTfcTv();
            var context = new IPTV2Entities();
            var offering = context.Offerings.Find(offeringid);
            Console.WriteLine("Process start.");
            foreach (var email in list)
            {
                var user = context.Users.FirstOrDefault(u => u.EMail == email);
                Console.Write(String.Format("Processing user {0}...", email));
                service.ProcessAllPendingTransactionsInGoms(context, offering, user);
                Console.WriteLine("DONE.");
            }
            Console.WriteLine("Process finish.");
            //Console.ReadLine();


            //int offeringid = 2;
            //var service = new GomsTfcTv();
            //var context = new IPTV2Entities();
            //var offering = context.Offerings.Find(offeringid);
            //Console.WriteLine("Process start.");
            //// Update Forex
            //service.GetExchangeRates(context, "USD");
            //// Process Transactions
            //service.ProcessAllPendingTransactionsInGoms(context, offering);
            //Console.WriteLine("Process finish.");
            
            // Console.ReadLine();
        }

        [Fact]
        public void MyTest()
        {
            Assert.Equal(2, 2);
        }

        private static void TestConnectivity()
        {
            var service = new GomsTfcTv();
            service.TestConnect();
        }

        private static void TestGetUser()
        {
            var service = new GomsTfcTv();
            int userId = 577615;
            string email = "eugene_paden@abs-cbn.com";
            var resp = service.GetUser(userId, email);
            if (resp.IsSuccess)
            {
                var context = new IPTV2_Model.IPTV2Entities();
                var user = context.Users.FirstOrDefault(u => u.EMail == email);
                if (user != null)
                {
                    user.GomsCustomerId = resp.CustomerId;
                    user.GomsSubsidiaryId = resp.SubsidiaryId;
                    var wallet = user.UserWallets.FirstOrDefault(w => w.Currency == user.Country.CurrencyCode);
                    if (wallet == null)
                    {
                        wallet = new IPTV2_Model.UserWallet
                        {
                            Currency = user.Country.CurrencyCode,
                            Balance = 0,
                            IsActive = true,
                            LastUpdated = DateTime.Now
                        };
                        user.UserWallets.Add(wallet);
                        context.SaveChanges();
                    }
                }
            }
        }

        private static void TestGetWallet()
        {
            var service = new GomsTfcTv();
            int userId = 577615;
            string email = "eugenecp@gmail.com";
            int walletId = 10;
            var resp = service.GetWallet(userId, email, walletId);
        }
    }
}