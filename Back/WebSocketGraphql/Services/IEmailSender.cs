using TimeTracker.Models;

namespace TimeTracker.Services
{
    public interface IEmailSender
    {
        void SendResetPassEmail(string code, string email);
        void SendRegistrationEmail(string code, string email);
    }
}
