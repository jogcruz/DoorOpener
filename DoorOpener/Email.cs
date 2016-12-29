using System;
using System.Net;
using System.Net.Mail;

namespace DoorOpener
{
    class Email
    {
        public static Boolean SendGmail(string receivers, string subject, string body)
        {
            bool Result = false;
            string Sender = "appsenderemail@gmail.com";
            const string SenderPassword = "app;sender#";

            try
            {
                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(Sender, SenderPassword),
                    Timeout = 3000
                };

                // MailMessage represents a mail message. 
                // The constructor accepts a comma (',') separated list of addresses.  
                // it is 4 parameters(From,TO,subject,body)

                MailMessage message = new MailMessage(Sender, receivers, subject, body);
                message.IsBodyHtml = true;
                //message.Attachments.Add(new Attachment("C:\\file.zip"));

                smtp.Send(message);
                Result = true;

            }
            catch (Exception ex)
            {
                
            }

            return Result;
        }
    }
}
