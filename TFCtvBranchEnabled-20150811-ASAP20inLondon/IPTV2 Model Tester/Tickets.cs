using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model_Tester
{
    class Tickets
    {
        static public void CreateTicketTest()
        {
            string userName = "TFCtvTS";
            string password = "";
            int departmentId = 68;
            string from = "eugenecp@gmail.com";
            string subject = "Test";
            string body = "test body";
            var result = ServiceDesk.Ticket.CreateTicket(userName, password, departmentId, from, subject, body, false);
        }
    }
}
