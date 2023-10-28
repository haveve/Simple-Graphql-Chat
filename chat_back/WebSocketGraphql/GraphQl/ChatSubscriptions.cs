using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.Tests.Subscription;

public class ChatSchema : Schema
{
    public ChatSchema(IServiceProvider service):base(service)
    {
        Query = service.GetRequiredService<ChatQuery>();
        Mutation = service.GetRequiredService<ChatMutation>();
        Subscription = service.GetRequiredService<ChatSubscriptions>();
    }
}

public class ChatSubscriptions : ObjectGraphType
{
    private readonly IChat _chat;

    public ChatSubscriptions(IChat chat)
    {
        _chat = chat;
        AddField(new FieldType
        {
            Name = "messageAdded",
            Type = typeof(MessageType),
            StreamResolver = new SourceStreamResolver<Message>(Subscribe)
        });

        AddField(new FieldType
        {
            Name = "messageAddedByUser",
            Arguments = new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" }
            ),
            Type = typeof(MessageType),
            StreamResolver = new SourceStreamResolver<Message>(SubscribeById)
        });
    }
    private IObservable<Message> SubscribeById(IResolveFieldContext context)
    {
        string id = context.GetArgument<string>("id");

        var messages = _chat.Messages();

        return messages.Where(message => message.From.Id == id);
    }

   
    private IObservable<Message> Subscribe(IResolveFieldContext context)
    {
        return _chat.Messages();
    }

}

public class ChatMutation : ObjectGraphType<object>
{
    public ChatMutation(IChat chat)
    {
        Field<StringGraphType>("addMessage")
            .Argument<MessageInputType>("message")
            .Resolve(context =>
            {
                var receivedMessage = context.GetArgument<ReceivedMessage>("message");
                chat.AddMessage(receivedMessage);
                return "OK";
            });
    }
}

public class ChatQuery : ObjectGraphType
{
    public ChatQuery(IChat chat)
    {
        Field<ListGraphType<MessageType>>("messages").Resolve(_ => chat.AllMessages);
    }
}

public class MessageType : ObjectGraphType<Message>
{
    public MessageType()
    {
        Field(o => o.Content);
        Field(o => o.SentAt);
        Field(o => o.From, false, typeof(MessageFromType)).Resolve(ResolveFrom);
    }

    private MessageFrom ResolveFrom(IResolveFieldContext<Message> context)
    {
        var message = context.Source;
        return message.From;
    }
}

public class MessageInputType : InputObjectGraphType
{
    public MessageInputType()
    {
        Field<StringGraphType>("fromId");
        Field<StringGraphType>("content");
        Field<DateGraphType>("sentAt");
    }
}

public class MessageFromType : ObjectGraphType<MessageFrom>
{
    public MessageFromType()
    {
        Field(o => o.Id);
        Field(o => o.DisplayName);
    }
}

public class Message
{
    public MessageFrom From { get; set; }

    public string Content { get; set; }

    public DateTime SentAt { get; set; }
}

public class MessageFrom
{
    public string Id { get; set; }

    public string DisplayName { get; set; }
}

public class ReceivedMessage
{
    public string FromId { get; set; }

    public string Content { get; set; }

    public DateTime SentAt { get; set; }
}

public interface IChat
{
    ConcurrentStack<Message> AllMessages { get; }

    Message AddMessage(Message message);

    IObservable<Message> Messages();

    Message AddMessage(ReceivedMessage message);
}

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