using CourseWorkDB.Repositories;
using GraphQL;
using GraphQL.Types;
using GraphQL.Upload.AspNetCore;
using TimeTracker.Repositories;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.GraphQl.IdentityTypes;
using WebSocketGraphql.Models;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.Services.AuthenticationServices;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.ChatTypes
{
    public class ChatMutation : ObjectGraphType
    {

        public ChatMutation(IChat chat, AuthHelper helper, IUserRepository userRepository, IUploadRepository uploadRepository)
        {
            Field<NonNullGraphType<MessageGraphType>>("addMessage")
                .Argument<NonNullGraphType<MessageInputGraphType>>("message", el => el.ApplyDirective(
                   "length", "min", MessageInputGraphType.minLength, "max", MessageInputGraphType.maxLength))
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .ResolveAsync(async context =>
                {
                    var receivedMessage = context.GetArgument<Message>("message");
                    receivedMessage.ChatId = context.GetArgument<int>("chatId");
                    receivedMessage.FromId = helper.GetUserId(context.User!);
                    receivedMessage.NickName = helper.GetUserNickName(context.UserContext);

                    if (!await chat.AddMessageAsync(receivedMessage))
                    {
                        ThrowError("Message cannot be created because of some reasons");
                    }

                    return receivedMessage;
                });
            Field<NonNullGraphType<MessageGraphType>>("removeMessage")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<NonNullGraphType<MessageInputGraphType>>("message")
                .ResolveAsync(async context =>
                {
                    var receivedMessage = context.GetArgument<Message>("message");
                    receivedMessage.ChatId = context.GetArgument<int>("chatId");
                    receivedMessage.FromId = helper.GetUserId(context.User!);
                    receivedMessage.NickName = helper.GetUserNickName(context.UserContext);


                    if (!await chat.RemoveMessageAsync(receivedMessage))
                    {
                        ThrowError("Message cannot be removed because of some reasons");
                    }
                    return receivedMessage;
                });

            Field<NonNullGraphType<MessageGraphType>>("updateMessage")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<NonNullGraphType<MessageInputGraphType>>("message")
                .ResolveAsync(async context =>
                {
                    var receivedMessage = context.GetArgument<Message>("message");
                    receivedMessage.ChatId = context.GetArgument<int>("chatId");
                    receivedMessage.FromId = helper.GetUserId(context.User!);
                    receivedMessage.NickName = helper.GetUserNickName(context.UserContext);


                    if (!await chat.UpdateMessageAsync(receivedMessage))
                    {
                        ThrowError("Message cannot be updated because of some reasons");
                    }

                    return receivedMessage;
                });

            Field<NonNullGraphType<ExtendedChatGraphType>>("createChat")
                .Argument<NonNullGraphType<StringGraphType>>("name",
                data => data.ApplyDirective("length", "min", ChatInputGraphType.minChatNameLength,
                "max", ChatInputGraphType.maxChatNameLength))
                .ResolveAsync(async (context) =>
                {
                    var chatData = new ChatResult()
                    {
                        Name = context.GetArgument<string>("name"),
                        CreatorId = helper.GetUserId(context.User!),
                        ChatMembersCount = 0
                    };
                    chatData.Id = await chat.AddChatAsync(chatData);

                    return chatData;
                });

            Field<NonNullGraphType<ChatGraphType>>("updateChat")
                .Argument<NonNullGraphType<ChatInputGraphType>>("chat")
                .ResolveAsync(async (context) =>
                {
                    var chatUpdate = context.GetArgument<ChatModel>("chat");
                    if (!await helper.CheckChatOwner(helper.GetUserId(context.User!), chatUpdate.Id, context.UserContext))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    if (!await chat.UpdateChatAsync(chatUpdate.Id, chatUpdate.Name))
                    {
                        ThrowError("Chat cannot be updated because of some reasons");
                    }

                    return chatUpdate;
                });


            Field<NonNullGraphType<IntGraphType>>("removeChat")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .ResolveAsync(async (context) =>
                {
                    var chatId = context.GetArgument<int>("chatId");
                    if (!await helper.CheckChatOwner(helper.GetUserId(context.User!), chatId, context.UserContext))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    if (!await chat.RemoveChatAsync(chatId))
                    {
                        ThrowError("Chat cannot be updated because of some reasons");
                    }

                    return chatId;
                });

            Field<NonNullGraphType<StringGraphType>>("addUserToChat")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<NonNullGraphType<StringGraphType>>("user", el => el.ApplyDirective(
                   "length", "min", RegistrationInputGraphType.minNickNameLength, "max", RegistrationInputGraphType.maxEmailLength))
                .ResolveAsync(async (context) =>
                {
                    var chatId = context.GetArgument<int>("chatId");
                    var user = context.GetArgument<string>("user");

                    if (!await helper.CheckChatOwner(helper.GetUserId(context.User!), chatId, context.UserContext))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    if (!await chat.AddUserToChatAsync(chatId, user, helper.GetUserNickName(context.UserContext)))
                    {
                        ThrowError("User cannot be added to chat because of some reasons");
                    }

                    return "Ok";
                });

            Field<NonNullGraphType<StringGraphType>>("removeUserFromChat")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<NonNullGraphType<StringGraphType>>("user", el => el.ApplyDirective(
                   "length", "min", RegistrationInputGraphType.minNickNameLength, "max", RegistrationInputGraphType.maxEmailLength))
                .Argument<BooleanGraphType>("deleteAll")
                .ResolveAsync(async (context) =>
                {
                    var chatId = context.GetArgument<int>("chatId");
                    var user = context.GetArgument<string>("user");
                    var deleteAll = context.GetArgument<bool?>("deleteAll");

                    if (!await helper.CheckChatOwner(helper.GetUserId(context.User!), chatId, context.UserContext))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    if (!await chat.RemoveUserFromChatAsync(chatId, user, deleteAll ?? false, helper.GetUserNickName(context.UserContext)))
                    {
                        ThrowError("User cannot be removed from chat because of some reasons");
                    }

                    return "Ok";
                });

            Field<NonNullGraphType<StringGraphType>>("leaveFromChat")
    .Argument<NonNullGraphType<IntGraphType>>("chatId")
    .Argument<BooleanGraphType>("deleteAll")
    .ResolveAsync(async (context) =>
    {
        var chatId = context.GetArgument<int>("chatId");
        var user = helper.GetUserNickName(context.UserContext);
        var deleteAll = context.GetArgument<bool?>("deleteAll");

        if (!await chat.LeaveFromChatAsync(user, chatId, deleteAll ?? false))
        {
            ThrowError("User cannot be removed from chat because of some reasons");
        }

        return "Ok";
    });

            Field<NonNullGraphType<UserUpdateOutPutGraphType>>("updateUser")
        .Argument<NonNullGraphType<UserUpdateInputGraphType>>("data")
        .ResolveAsync(async context =>
        {
            var data = context.GetArgument<UpdateUser>("data");
            data.Id = helper.GetUserId(context.User!);

            var result = await userRepository.UpdateUserAsync(data);

            var participants = helper.GetChatParticipant(context.UserContext);

            if (participants is not null)
            {
                chat.NotifyAllChats(participants, new ChatParticipant()
                { Id = data.Id, NickName = data.NickName, Online = true });
            }
            return result;
        });

            Field<NonNullGraphType<IntGraphType>>("deleteUser")
            .Argument<NonNullGraphType<UserRemoveInputGraphType>>("data")
            .ResolveAsync(async context =>
            {
                var data = context.GetArgument<RemoveUser>("data");
                data.Id = helper.GetUserId(context.User!);
                var result = await userRepository.DeleteUserAsync(data);

                var participants = helper.GetChatParticipant(context.UserContext);

                if (participants is not null)
                {
                    chat.NotifyAllChats(participants, new ChatParticipant()
                    { Id = data.Id, NickName = "DELETED", Online = false });
                }

                return result;
            });

            Field<NonNullGraphType<StringGraphType>>("updateUserAvatart")
                .Argument<NonNullGraphType<UploadGraphType>>("image")
                .ResolveAsync(async context =>
                {
                    var userPictPath = "user_pictures";
                    var img = context.GetArgument<IFormFile>("image");
                    var userId = helper.GetUserId(context.User!);
                    var newImg = await uploadRepository.SaveImgAsync(img, userPictPath);

                    try
                    {
                        var result = await userRepository.UpdateUserAvatarAsync(userId, newImg);

                        if (result is not null)
                        {
                            uploadRepository.DeleteFile(Path.Combine(userPictPath, result));
                        }
                        return newImg;
                    }
                    catch
                    {

                        uploadRepository.DeleteFile(Path.Combine(userPictPath, newImg));
                        throw;
                    }
                });

        }

        private void ThrowError(string message)
        {
            throw new InvalidDataException(message);
        }
    }
}
