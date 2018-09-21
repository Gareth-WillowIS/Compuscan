using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace CompuscanUtils
{
    public class Utils
    {
        public static void sendMail(string emailAddress, string subject, string mailBody)
        {


            MailMessage mail = new MailMessage(ConfigurationManager.AppSettings["errorMailEmailAddress"], emailAddress);
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = ConfigurationManager.AppSettings["smtp"];
            mail.Subject = subject;
            mail.Body = mailBody;

            client.Send(mail);
        }


        public static void sendErrorMail(string page, string exception, string innerException, string subject, string additionalInfo = null)
        {
            string mailBody = "An error occurred at " + DateTime.Now + " on " + page + ":" + Environment.NewLine + "Exception:" + exception;
            if (innerException != "")
            {
                mailBody += Environment.NewLine + "Inner Exception: " + innerException;
            }

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                mailBody += Environment.NewLine + additionalInfo;
            }

            MailMessage mail = new MailMessage(ConfigurationManager.AppSettings["errorMailEmailAddress"], ConfigurationManager.AppSettings["errorMailEmailAddress"]);
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = ConfigurationManager.AppSettings["smtp"];
            mail.Subject = subject;
            mail.Body = mailBody;

            client.Send(mail);
        }
    }
}