using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using WebSocketGraphql.GraphQl.ChatTypes.Models;

namespace WebSocketGraphql.Repositories
{
    public class Chat : IChat
    {
        private readonly ISubject<Message> _messageStream = new Subject<Message>();

        public Chat()
        {
            AllMessages = new ConcurrentStack<Message>();
            Users = new ConcurrentDictionary<string, string>
            {
                ["1"] = "developer",
                ["2"] = "tester"
            };
        }

        public ConcurrentDictionary<string, string> Users { get; set; }

        public ConcurrentStack<Message> AllMessages { get; }

        public Message AddMessage(ReceivedMessage message)
        {
            if (!Users.TryGetValue(message.FromId, out string displayName))
            {
                displayName = "(unknown)";
            }

            return AddMessage(new Message
            {
                Content = message.Content,
                SentAt = message.SentAt,
                From = new MessageFrom
                {
                    DisplayName = displayName,
                    Id = message.FromId
                }
            });
        }

        public Message AddMessage(Message message)
        {
            AllMessages.Push(message);
            _messageStream.OnNext(message);
            var l = new List<Message>(AllMessages);
            return message;
        }

        public IObservable<Message> Messages()
        {
            return _messageStream.AsObservable();
        }
    }
}
