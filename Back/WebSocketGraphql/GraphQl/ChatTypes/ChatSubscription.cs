using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using System.Linq;
using System.Reactive.Linq;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes
{
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
        }

        private IObservable<Message> Subscribe(IResolveFieldContext context)
        {
            int chatId = context.GetArgument<int>("chatId");
            return _chat.Messages(chatId);
        }

    }

}
