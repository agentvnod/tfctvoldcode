using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using ServiceDesk.SmarterTicketServiceReference;

namespace ServiceDesk
{
    /// <summary>
    /// See origin web service at http://servicedesk.abs-cbnglobal.com/Services/svcTickets.asmx
    /// Docs at http://help.smartertools.com/SmarterTicket/v2/Topics/Admin/Misc/WebServices.aspx
    /// </summary>
    public class Ticket
    {
        public static string CreateTicket(string agentName, string agentPassword, int departmentId, string from, string subject, string body, bool isHtmlBody)
        {
            var serviceClient = new SmarterTicketServiceReference.svcTicketsSoapClient();
            return serviceClient.CreateTicket(agentName, agentPassword, departmentId, from, subject, body, isHtmlBody, false);
        }
    }
}
