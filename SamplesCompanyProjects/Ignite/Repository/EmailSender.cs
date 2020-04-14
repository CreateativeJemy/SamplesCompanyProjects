using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Ignite.Areas.Office.Repository
{
    public class EmailSender : IEmailSender

    {
        public async Task SendEmailAsync(string To_Email_Address, string subject, string message)
        {
            try
            {
                var to = new MailAddress(To_Email_Address);

                var from = new MailAddress("mohamed.farid@seedersmarket.com");

                var mail = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                var smtp = new SmtpClient
                {
                    Host = "smtp.zoho.com",
                    Port = 587,
                    Timeout = 60000,
                    Credentials = new NetworkCredential("", ""),
                    EnableSsl = true
                };
                await smtp.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
               // throw ex;
            }
        }
    }
}
