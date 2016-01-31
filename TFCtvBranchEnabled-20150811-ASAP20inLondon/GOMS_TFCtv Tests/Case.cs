using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GOMS_TFCtv;
using IPTV2_Model;

namespace GOMS_TFCtv_Tests
{
    class Case
    {
        public static void CreateCase()
        {
            var context = new IPTV2Entities();
            var gomsService = new GomsTfcTv();

            // var email = "eugene_paden@abs-cbn.com";
            var email = "eugenecp@hotmail.com";
            var user = context.Users.FirstOrDefault(u => u.EMail == email);
            var agent = (GomsCaseAgent)context.GomsReferences.FirstOrDefault(r => r is GomsCaseAgent);
            var caseIssueType = (GomsCaseIssueType)context.GomsReferences.FirstOrDefault(r => r is GomsCaseIssueType);
            var caseSubIssueType = (GomsCaseSubIssueType)context.GomsReferences.FirstOrDefault(r => r is GomsCaseSubIssueType);
            var resp = gomsService.CreateSupportCase(context, user.UserId, "test case", "hope this works, please help...", agent, caseIssueType, caseSubIssueType);


        }
    }
}
