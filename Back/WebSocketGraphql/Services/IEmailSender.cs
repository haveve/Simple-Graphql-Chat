using TimeTracker.Models;

namespace TimeTracker.Services
{
    public interface IEmailSender
    {
        Task SendResetPassEmailAsync(string code, string email);
        Task SendRegistrationEmailAsync(string code, string email);
    }
}
