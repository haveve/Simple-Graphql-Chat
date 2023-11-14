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

        public ChatSubscriptions(IChat chat, AuthHelper authHelper)
        {
            _chat = chat;
            AddField(new FieldType
            {
                Name = "chatNotification",
                Type = typeof(MessageOrChatResult),
                Arguments = new QueryArguments(
                new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "chatId" }
                ),
                StreamResolver = new SourceStreamResolver<object>(SubscribeChatNotification),
            });

            AddField(new FieldType
            {
                Name = "userNotification",
                Type = typeof(UserNortificationGraphType),
                Resolver = new FuncFieldResolver<UserNotification>(context =>
                {
                    var data = context.Source as UserNotification;
                    if(data!.UserId != authHelper.GetUserId(context.User!))
                    {
                        return null;
                    }
                    return data;
                }),
                StreamResolver = new SourceStreamResolver<UserNotification>(SubscribeUserNotification),
            });
        }

        private IObservable<object> SubscribeChatNotification(IResolveFieldContext context)
        {
            int chatId = context.GetArgument<int>("chatId");
            return _chat.SubscribeMessages(chatId);
        }


        private IObservable<UserNotification> SubscribeUserNotification(IResolveFieldContext context)
        {
            return _chat.SubscribeUserNotification();
        }

    }

}
