using WebSocketGraphql.Models;

namespace WebSocketGraphql.ViewModels;

public class MessageSubscription : Message
{
    public MessageType Type { get; set; }

    public bool DeleteAll { get; set; }

    public MessageSubscription(Message message)
    {
        ChatId = message.ChatId;
        FromId = message.FromId;
        Content = message.Content;
        SentAt = message.SentAt;
        NickName = message.NickName;
        Image = message.Image;
    }
}

public enum MessageType
{
    CREATE,
    UPDATE,
    DELETE,
    USER_ADD,
    USER_REMOVE
}
