using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using WebSocketGraphql.Configurations;

namespace WebSocketGraphql.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        private readonly string _frontUrl;

        public EmailSender(IOptions<SpaSettings> spaSettings, IOptions<EmailSettings> emailSettings)
        {
            _settings = emailSettings.Value;
            _frontUrl = spaSettings.Value.Url;
        }

        public async Task SendResetPassEmailAsync(string code, string email)
        {
            var subject = "Password recovery MyAwesomeChat";
            var body = "To reset your password, follow the following link = {0}";

            await SendSetPasswordEmail(subject, body, code, email);
        }

        public async Task SendRegistrationEmailAsync(string code, string email)
        {
            var subject = "MyAwesomeChat Registration";
            var body = "To register, follow the link = {0}";

            await SendSetPasswordEmail(subject, body, code, email);
        }

        private async Task SendSetPasswordEmail(string subject, string body, string code, string email)
        {
            var client = new SmtpClient(_settings.ServiceDomain, _settings.Port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.From, _settings.Password)
            };
            string resetUrl = $"{_frontUrl}/set-password";

            string url = $"{resetUrl}?code={code}&email={email}";

            var mail = new MailMessage(_settings.From, email);
            mail.Subject = subject;
            mail.Body = string.Format(body, url);

            await client.SendMailAsync(mail);
        }
    }
}
