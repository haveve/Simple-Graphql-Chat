using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.ViewModels;
using WebSocketGraphql.Services.AuthenticationServices;
using WebSocketGraphql.Helpers;

namespace WebSocketGraphql.GraphQl.ChatTypes
{
    public class ChatSubscriptions : ObjectGraphType
    {
        private readonly IChat _chat;
        private readonly AuthHelper _authHelper;
        public ChatSubscriptions(IChat chat, AuthHelper authHelper)
        {
            _chat = chat;
            _authHelper = authHelper;
            AddField(new FieldType
            {
                Name = "chatNotification",
                Type = typeof(MessageOrChatResult),
                Arguments = new QueryArguments(
                new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "chatId" }
                ),
                Resolver = new FuncFieldResolver<object>(context =>
                {
                    if (context.Source is MessageSubscription data)
                    { 
                        if (data!.NickName == authHelper.GetUserNickName(context.User!))
                        {
                            return null;
                        }
                    }

                    return context.Source;
                }),
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

            var ids = _authHelper.GetChatParticipant(context.UserContext);
            var userId = _authHelper.GetUserId(context.User!);

            var observable = new UserOnlineObservable<UserNotification>(userId, ids!, _chat, _chat.SubscribeUserNotification(userId));

            return observable;
        }

    }

}
