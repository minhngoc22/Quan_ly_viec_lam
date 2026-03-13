using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace DoanWeb
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var fromEmail = _config["EmailSettings:From"];
            var appPassword = _config["EmailSettings:AppPassword"];

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true,
            };

            smtpClient.Send(fromEmail, toEmail, subject, body);
        }
    }
}
