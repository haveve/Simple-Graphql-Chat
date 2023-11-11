using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.ViewModels;
using WebSocketGraphql.Services.AuthenticationServices;

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
                Type = typeof(MessageOrChatResult),
                Arguments = new QueryArguments(
                new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "chatId" }
                ),
                StreamResolver = new SourceStreamResolver<object>(Subscribe),
            });
        }

        private IObservable<object> Subscribe(IResolveFieldContext context)
        {
            int chatId = context.GetArgument<int>("chatId");
            return _chat.SubscribeMessages(chatId);
        }

    }

}
