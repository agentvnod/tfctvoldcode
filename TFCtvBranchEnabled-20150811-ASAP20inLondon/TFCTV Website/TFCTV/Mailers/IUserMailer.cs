using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Mvc.Mailer;

namespace TFCTV.Mailers
{
    public interface IUserMailer
    {
        MailMessage Welcome();

        MailMessage ForgotPassword(string to, string password);

        MailMessage SendFeedback(string from, string subject, string message);
    }
}