using GraphQL;
using GraphQL.Types;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.Models;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.Services.AuthenticationServices;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.ChatTypes
{
    public class ChatQuery : ObjectGraphType
    {
        public ChatQuery(IChat chat, AuthHelper helper)
        {
            Field<ListGraphType<MessageGraphType>>("messages")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .ResolveAsync(async context =>
                {
                    int chatId = context.GetArgument<int>("chatId");
                    return await chat.GetAllMessagesAsync(chatId);
                });

            Field<ListGraphType<ExtendedChatGraphType>>("chats")
                .ResolveAsync(async (context) =>
                {
                    int userId = helper.GetUserId(context.User!);
                    return await chat.GetUserChatsInstances(userId);
                });

            Field<ListGraphType<ChatParticipantGraphType>>("participats")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .ResolveAsync(async context =>
                {
                    int chatId = context.GetArgument<int>("chatId");

                    if (!await helper.CheckChatOwner(helper.GetUserId(context.User!), chatId, context.User!))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    return await chat.GetAllChatParticipatsAsync(chatId);
                });

        }

        private void ThrowError(string message)
        {
            throw new InvalidDataException(message);
        }
    }

}
