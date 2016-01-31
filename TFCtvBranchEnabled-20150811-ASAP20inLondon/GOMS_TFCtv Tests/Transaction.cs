using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTV2_Model;
using GOMS_TFCtv;

namespace GOMS_TFCtv_Tests
{
    class Transaction
    {
        public static void PpcReload()
        {
            var context = new IPTV2Entities();
            var service = new GomsTfcTv();

            // get transaction
            // List<PpcReloadTransaction> ts = context.Transactions.Where(t => t is PpcReloadTransaction && t.GomsTransactionId == null).ToList();
            var trans = context.Transactions.OfType<PpcReloadTransaction>().FirstOrDefault(t => t.GomsTransactionId == null && t.ReloadPpc.PpcType.GomsSubsidiaryId == t.User.GomsSubsidiaryId);
            var resp = service.ReloadWallet(context, trans.User.UserId, trans.TransactionId);
        }

        public static void ProcessAllGomsPendingTransactions()
        {
            var context = new IPTV2Entities();
            var service = new GomsTfcTv();
            var offering = context.Offerings.Find(2);
            service.ProcessAllPendingTransactionsInGoms(context, offering);
        }


        public static void ProcessAllGomsPendingTransactionsForUser()
        {

            var context = new IPTV2Entities();
            var service = new GomsTfcTv();
            var offering = context.Offerings.Find(2);
            var user = context.Users.FirstOrDefault(u => u.EMail == "littleantonia@gmail.com");            
            service.ProcessAllPendingTransactionsInGoms(context, offering, user);
        }


    }
}
