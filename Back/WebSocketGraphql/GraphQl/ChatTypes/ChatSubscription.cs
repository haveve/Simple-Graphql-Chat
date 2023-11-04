using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.GraphQl.ChatTypes.Models;
using System.Linq;
using System.Reactive.Linq;

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

}
