using System.Collections.Concurrent;
using WebSocketGraphql.GraphQl.ChatTypes.Models;

namespace WebSocketGraphql.Repositories
{
    public interface IChat
    {
        ConcurrentStack<Message> AllMessages { get; }

        Message AddMessage(Message message);

        IObservable<Message> Messages();

        Message AddMessage(ReceivedMessage message);
    }
}
