using WebSocketGraphql.Models;

namespace WebSocketGraphql.ViewModels
{
    public class UserNotification: ChatModel
    {
        public int UserId { get; set; }

        public ChatNotificationType NotificationType { get; set; }
    }

    public enum ChatNotificationType
    {
        ENROLL,
        BANISH
    }

}
