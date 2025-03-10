using WebSocketGraphql.Models;

namespace WebSocketGraphql.ViewModels;

public class ChatResult : ChatModel
{
    public int ChatMembersCount { get; set; }
}
