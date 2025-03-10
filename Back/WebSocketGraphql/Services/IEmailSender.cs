using WebSocketGraphql.Models;

namespace WebSocketGraphql.Services
{
    public interface IEmailSender
    {
        Task SendResetPassEmailAsync(string code, string email);
        Task SendRegistrationEmailAsync(string code, string email);
    }
}
