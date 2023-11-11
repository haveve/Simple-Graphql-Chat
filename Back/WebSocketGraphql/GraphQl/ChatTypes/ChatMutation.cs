using GraphQL;
using GraphQL.Types;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.Models;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.Services.AuthenticationServices;

namespace WebSocketGraphql.GraphQl.ChatTypes
{
    public class ChatMutation : ObjectGraphType
    {
        public ChatMutation(IChat chat, AuthHelper helper)
        {
            Field<NonNullGraphType<MessageGraphType>>("addMessage")
                .Argument<NonNullGraphType<MessageInputGraphType>>("message")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .ResolveAsync(async context =>
                {
                    var receivedMessage = context.GetArgument<Message>("message");
                    receivedMessage.ChatId = context.GetArgument<int>("chatId");
                    receivedMessage.FromId = helper.GetUserId(context.User!);

                    if (!await chat.AddMessageAsync(receivedMessage))
                    {
                        ThrowError("Message cannot be created because of some reasons");
                    }

                    return receivedMessage;
                });
            Field<NonNullGraphType<StringGraphType>>("removeMessage")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<NonNullGraphType<MessageInputGraphType>>("message")
                .ResolveAsync(async context =>
                {
                    var receivedMessage = context.GetArgument<Message>("message");
                    receivedMessage.ChatId = context.GetArgument<int>("chatId");
                    receivedMessage.FromId = helper.GetUserId(context.User!);

                    if (!await chat.RemoveMessageAsync(receivedMessage))
                    {
                        ThrowError("Message cannot be removed because of some reasons");
                    }

                    return "Ok";
                });

            Field<NonNullGraphType<MessageGraphType>>("updateMessage")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<NonNullGraphType<MessageInputGraphType>>("message")
                .ResolveAsync(async context =>
                {
                    var receivedMessage = context.GetArgument<Message>("message");
                    receivedMessage.ChatId = context.GetArgument<int>("chatId");
                    receivedMessage.FromId = helper.GetUserId(context.User!);


                    if (!await chat.UpdateMessageAsync(receivedMessage))
                    {
                        ThrowError("Message cannot be updated because of some reasons");
                    }

                    return receivedMessage;
                });

            Field<ChatGraphType>("createChat")
                .Argument<NonNullGraphType<StringGraphType>>("name")
                .ResolveAsync(async (context) =>
                {
                    var chatData = new ChatModel()
                    {
                        Name = context.GetArgument<string>("name"),
                        CreatorId = helper.GetUserId(context.User!)
                    };
                    chatData.Id = await chat.AddChatAsync(chatData);

                    return chatData;
                });

            Field<NonNullGraphType<IntGraphType>>("updateChat")
                .Argument<NonNullGraphType<ChatInputGraphType>>("chat")
                .ResolveAsync(async (context) =>
                {
                    var chatUpdate = context.GetArgument<ChatModel>("chat");
                    if (!await helper.CheckChatOwner(helper.GetUserId(context.User!), chatUpdate.Id, context.User!))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    if(!await chat.UpdateChatAsync(chatUpdate.Id, chatUpdate.Name))
                    {
                        ThrowError("Chat cannot be updated because of some reasons");
                    }

                    return chatUpdate;
                });


            Field<NonNullGraphType<StringGraphType>>("removeChat")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .ResolveAsync(async (context) =>
                {
                    var chatId = context.GetArgument<int>("chatId");
                    if (!await helper.CheckChatOwner(helper.GetUserId(context.User!), chatId, context.User!))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    if (!await chat.RemoveChatAsync(chatId))
                    {
                        ThrowError("Chat cannot be updated because of some reasons");
                    }

                    return "Ok";
                });

            Field<NonNullGraphType<StringGraphType>>("addUserToChat")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<NonNullGraphType<StringGraphType>>("user")
                .ResolveAsync(async (context) =>
                {
                    var chatId = context.GetArgument<int>("chatId");
                    var user = context.GetArgument<string>("user");

                    if (!await helper.CheckChatOwner(helper.GetUserId(context.User!), chatId, context.User!))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    if (!await chat.AddUserToChatAsync(chatId, user))
                    {
                        ThrowError("User cannot be added to chat because of some reasons");
                    }

                    return "Ok";
                });

            Field<NonNullGraphType<StringGraphType>>("removeUserFromChat")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<NonNullGraphType<StringGraphType>>("user")
                .ResolveAsync(async (context) =>
                {
                    var chatId = context.GetArgument<int>("chatId");
                    var user = context.GetArgument<string>("user");

                    if (!await helper.CheckChatOwner(helper.GetUserId(context.User!), chatId, context.User!))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    if (!await chat.RemoveUserFromChatAsync(chatId, user))
                    {
                        ThrowError("User cannot be removed from chat because of some reasons");
                    }

                    return "Ok";
                });

        }

        private void ThrowError(string message)
        {
            throw new InvalidDataException(message);
        }
    }
}
