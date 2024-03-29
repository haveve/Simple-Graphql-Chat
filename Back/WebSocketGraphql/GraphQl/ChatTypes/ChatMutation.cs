﻿﻿using CourseWorkDB.Repositories;
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

        private const string chatPictPath = "chat_pictures";
        private const string userPictPath = "user_pictures";
        private const string messagePictPath = "message_pictures";

        private const int DefaultMaxFileSizeInKB = 150;

        private readonly int _maxFileSizeInKb;
        public ChatMutation(IChat chat, IConfiguration configuration, AuthHelper helper, IUserRepository userRepository, IUploadRepository uploadRepository)
        {
            if (Int32.TryParse(configuration["MaxPictureSizeInKB"], out int value))
                _maxFileSizeInKb = value;
            else
                _maxFileSizeInKb = DefaultMaxFileSizeInKB;

            Field<NonNullGraphType<MessageGraphType>>("addMessage")
                .Argument<NonNullGraphType<MessageInputGraphType>>("message", el => el.ApplyDirective(
                   "length", "min", MessageInputGraphType.minLength, "max", MessageInputGraphType.maxLength))
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<UploadGraphType>("image")
                .ResolveAsync(async context =>
                {

                    var img = context.GetArgument<IFormFile>("image");

                    var receivedMessage = context.GetArgument<Message>("message");
                    receivedMessage.ChatId = context.GetArgument<int>("chatId");
                    receivedMessage.FromId = helper.GetUserId(context.User!);
                    receivedMessage.NickName = helper.GetUserNickName(context.UserContext);

                    try
                    {

                        if (img is not null)
                        {
                            receivedMessage.Image = await uploadRepository.SaveImgWithSmallOneAsync(img, Path.Combine(messagePictPath, receivedMessage.ChatId.ToString()), 5, 5, maxFileSizeInKB: _maxFileSizeInKb);
                        }

                        if (!await chat.AddMessageAsync(receivedMessage))
                        {
                            throw new Exception();
                        }
                    }
                    catch
                    {
                        if (receivedMessage.Image is not null)
                            uploadRepository.DeleteFileWithSmallOne(Path.Combine(messagePictPath, receivedMessage.ChatId.ToString(), receivedMessage.Image));

                        throw;

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

                    try
                    {
                        var img = await chat.RemoveMessageAsync(receivedMessage);
                        if (img is not null)
                            uploadRepository.DeleteFileWithSmallOne(Path.Combine(messagePictPath, receivedMessage.ChatId.ToString(), img));
                    }
                    catch
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
                    if (!helper.CheckChatOwner(chatUpdate.Id, context.UserContext))
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
                    if (!helper.CheckChatOwner(chatId, context.UserContext))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    var pictName = await chat.RemoveChatAsync(chatId);
                    if (pictName is not null)
                        uploadRepository.DeleteFile(Path.Combine(chatPictPath, pictName));

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

                    if (!helper.CheckChatOwner(chatId, context.UserContext))
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

                    if (!helper.CheckChatOwner(chatId, context.UserContext))
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

                if (result is not null)
                {
                    uploadRepository.DeleteFile(Path.Combine(userPictPath, result));
                }

                var participants = helper.GetChatParticipant(context.UserContext);

                if (participants is not null)
                {
                    chat.NotifyAllChats(participants, new ChatParticipant()
                    { Id = data.Id, NickName = "DELETED", Online = false });
                }

                return data.Id;
            });

            Field<NonNullGraphType<StringGraphType>>("updateUserAvatart")
                .Argument<NonNullGraphType<UploadGraphType>>("image")
                .ResolveAsync(async context =>
                {
                    var img = context.GetArgument<IFormFile>("image");

                    var userId = helper.GetUserId(context.User!);
                    var newImg = await uploadRepository.SaveImgAsync(img, userPictPath, _maxFileSizeInKb);

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

            Field<NonNullGraphType<StringGraphType>>("updateChatAvatart")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .Argument<NonNullGraphType<UploadGraphType>>("image")
                .ResolveAsync(async context =>
                {

                    var chatId = context.GetArgument<int>("chatId");
                    if (!helper.CheckChatOwner(chatId, context.UserContext))
                    {
                        ThrowError("You does not have anough rights");
                    }

                    var img = context.GetArgument<IFormFile>("image");
                    var newImg = await uploadRepository.SaveImgAsync(img, chatPictPath, _maxFileSizeInKb);

                    try
                    {
                        var result = await chat.UpdateChatAvatarAsync(chatId, newImg);

                        if (result is not null)
                        {
                            uploadRepository.DeleteFile(Path.Combine(chatPictPath, result));
                        }
                        return newImg;
                    }
                    catch
                    {

                        uploadRepository.DeleteFile(Path.Combine(chatPictPath, newImg));
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