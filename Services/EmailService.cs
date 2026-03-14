using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace FurnitureStoreWeb.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var smptSettings = _config.GetSection("EmailSettings");
            var mailMessage = new MailMessage
            {
                From = new MailAddress(smptSettings["SenderEmail"]!, smptSettings["SenderName"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            using var smtpClient = new SmtpClient(smptSettings["SmtpServer"])
            {
                Port = int.Parse(smptSettings["Port"]!),
                Credentials = new NetworkCredential(smptSettings["SenderEmail"], smptSettings["SenderPassword"]),
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
