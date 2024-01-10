using WebSocketGraphql.Models;
using WebSocketGraphql.Repositories;

namespace WebSocketGraphql.ViewModels
{
    public class ChatSubscription : ChatModel
    {
        public ChatResultType Type { get; set; }
        public int UserId { get; set; }
        public ChatSubscription(ChatResultType type)
        {
            Type = type;
        }
    }

    public enum ChatResultType
    {
        UPDATE,
        DELETE
    }
}
