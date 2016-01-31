using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using IPTV2_Model;
using Mvc.Mailer;
using TFCTV.Helpers;

namespace TFCTV.Mailers
{
    public class UserMailer : MailerBase, IUserMailer
    {
        public UserMailer() :
            base()
        {
            MasterName = "_Layout";
        }

        public virtual MailMessage Welcome()
        {
            var mailMessage = new MailMessage { Subject = "Welcome" };

            //mailMessage.To.Add("some-email@example.com");
            //ViewBag.Data = someObject;
            PopulateBody(mailMessage, viewName: "Welcome");

            return mailMessage;
        }

        public virtual MailMessage ForgotPassword(string to, string password)
        {
            var mailMessage = new MailMessage { Subject = "TFC.tv: Forgot Your Password" };

            mailMessage.To.Add(to);
            mailMessage.IsBodyHtml = true;
            ViewBag.NewPassword = password;
            PopulateBody(mailMessage, viewName: "ForgotPassword");

            return mailMessage;
        }

        public virtual MailMessage SendFeedback(string from, string subject, string message)
        {
            var mailMessage = new MailMessage { Subject = subject };

            mailMessage.To.Add(GlobalConfig.SupportEmail);
            mailMessage.IsBodyHtml = true;
            ViewBag.Body = message;
            PopulateBody(mailMessage, viewName: "SendFeedback");

            return mailMessage;
        }
    }
}