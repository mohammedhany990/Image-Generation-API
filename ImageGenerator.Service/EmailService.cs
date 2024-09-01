using ImageGenerator.API.DTOs;
using ImageGenerator.API.Helpers;
using ImageGenerator.Core.EmailSettings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ImageGenerator.Service
{
    public class EmailService : IEmailService
    {
        private MailSettings _options;
        public EmailService(IOptions<MailSettings> options)
        {
            _options = options.Value;
        }

        public void SendEmail(Email email)
        {
            var mail = new MimeMessage
            {
                // To determine email that I'll send from, <from appsettings>
                Sender = MailboxAddress.Parse(_options.Email),
                Subject = email.Subject,//subject from email that passed to function
            };

            // To determine email that I'll send to
            mail.To.Add(MailboxAddress.Parse(email.To));


            // Build The Email Body 
            var builder = new BodyBuilder();
            builder.TextBody = email.Body;
            mail.Body = builder.ToMessageBody();
            // To put the DisplayName not sender's email
            mail.From.Add(new MailboxAddress(_options.DisplayName, _options.Email));

            // To Connect to Mail Provider -> smtp
            using var smtp = new SmtpClient();
            smtp.Connect(_options.Host, _options.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_options.Email, _options.Password);
            smtp.Send(mail);
            smtp.Disconnect(true);

        }
    }
}
