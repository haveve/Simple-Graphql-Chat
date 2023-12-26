using System.Net;
using System.Net.Mail;
using System.Text;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public class EmailSender : IEmailSender
    {
        private const int _port = 587;

        private readonly string _emailFrom = null!;
        private readonly string _emailPassword = null!;
        private readonly string _serverUrl = "smtp.office365.com";
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration) 
        { 
            _configuration = configuration;
            _emailFrom = configuration["EmailSender:From"];
            _emailPassword = configuration["EmailSender:Password"];
        }

        public async Task SendResetPassEmailAsync(string code, string email)
        {
            var client = new SmtpClient(_serverUrl, _port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailFrom, _emailPassword)
            };
            string resetUrl = $"{_configuration["FrontUrl"]}/set-password";

            string url = $"{resetUrl}?code={code}&email={email}";

            var mail = new MailMessage(_emailFrom, email);
            mail.Subject = "Password recovery MyAwesomeChat";
            mail.Body = $"To reset your password, follow the following link = {url}";

            await client.SendMailAsync(mail);
        }
        public async Task SendRegistrationEmailAsync(string code, string email)
        {
            var client = new SmtpClient(_serverUrl, _port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailFrom, _emailPassword)
            };
            string resetUrl = $"{_configuration["FrontUrl"]}/set-password";

            string url = $"{resetUrl}?code={code}&email={email}";

            var mail = new MailMessage(_emailFrom, email);
            mail.Subject = "MyAwesomeChat Registration";
            mail.Body = $"To register, follow the link = {url}";

            await client.SendMailAsync(mail);
        }
    }
}
