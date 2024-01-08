using GraphQL;
using GraphQL.Types;
using TimeTracker.Repositories;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.Services.AuthenticationServices;

namespace WebSocketGraphql.GraphQl.ChatTypes
{
    public class ChatQuery : ObjectGraphType
    {
        public ChatQuery(IChat chat, IUserRepository user, AuthHelper helper)
        {
            Field<ListGraphType<MessageGraphType>>("messages")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<NonNullGraphType<IntGraphType>>("take",el => el.ApplyDirective("constraint_number","min",1))
                .Argument<NonNullGraphType<IntGraphType>>("skip",el => el.ApplyDirective("constraint_number","min",0))
                .Argument<DateTimeGraphType>("maxDate")
                .ResolveAsync(async context =>
                {
                    int chatId = context.GetArgument<int>("chatId");
                    int take = context.GetArgument<int>("take");
                    int skip = context.GetArgument<int>("skip");
                    DateTime? maxDate = context.GetArgument<DateTime?>("maxDate");

                    return await chat.GetAllMessagesAsync(chatId, take, skip, maxDate);
                });

            Field<ListGraphType<ChatGraphType>>("chats")
                .ResolveAsync(async (context) =>
                {
                    int userId = helper.GetUserId(context.User!);
                    return await chat.GetUserChatsInstancesAsync(userId);
                });

            Field<ListGraphType<ChatParticipantGraphType>>("participants")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<StringGraphType>("search")
                .ResolveAsync(async context =>
                {
                    int chatId = context.GetArgument<int>("chatId");
                    string search = context.GetArgument<string>("search") ?? string.Empty;
                    return await chat.GetAllChatParticipatsAsync(chatId, search);
                });

            Field<ExtendedChatGraphType>("chatFullInfo")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .ResolveAsync(async context =>
                {
                    int chatId = context.GetArgument<int>("chatId");
                    int userId = helper.GetUserId(context.User!);

                    return await chat.GetFullChatInfoAsync(chatId, userId);
                });

            Field<NonNullGraphType<UserGraphType>>("user")
                .ResolveAsync(async context =>
                {
                    var id = helper.GetUserId(context.User!);
                    var userData = await user.GetUserAsync(id);
                    userData!.Key2Auth = userData.Key2Auth is not null ? "key" : null;
                    return userData;
                });

            Field<NonNullGraphType<StringGraphType>>("ping")
                .Resolve(_ => "pong");
        }

        private void ThrowError(string message)
        {
            throw new InvalidDataException(message);
        }
    }

}
