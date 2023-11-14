using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Message = WebSocketGraphql.Models.Message;
using TimeTracker.Repositories;
using Dapper;
using WebSocketGraphql.Models;
using GraphQL.Types.Relay.DataObjects;
using System.Xml.Linq;
using System;
using static GraphQL.Validation.Rules.OverlappingFieldsCanBeMerged;
using TimeTracker.Models;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.Repositories
{
    public class Chat : IChat
    {
        private ConcurrentDictionary<int, ISubject<object>> _chats;
        private ISubject<UserNotification> _userChatNotifyer;
        private readonly DapperContext _dapperContext;

        public Chat(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
            _chats = new();
            _userChatNotifyer = new Subject<UserNotification>();
        }

        public IObservable<UserNotification> SubscribeUserNotification()
        {
            return _userChatNotifyer.AsObservable();
        }

        public async Task<bool> AddMessageAsync(Message message)
        {
            string query = "INSERT INTO Message (chat_id,sent_at,fromt_id,content) VALUES(@ChatId,@SentAt,@FromId,@Content)";
            using var connection = _dapperContext.CreateConnection();
            bool result = await connection.ExecuteAsync(query, message).ConfigureAwait(false) > 0;
            if (result)
            {
                var subject = GetOrCreateChat(message.ChatId);
                subject.OnNext(new MessageSubscription(message) { Type = MessageType.CREATE });
            }
            return result;
        }

        private class ChatOperationUser
        {
            public string ChatName { get; set; }
            public int UserId { get; set; }
            public string NickName { get; set; }
            public int ChatMembersCount { get; set; } = 0;
            public int CreatorId { get; set; } = 0;
        }

        public async Task<bool> AddUserToChatAsync(int chatId, string nickNameOrEmail)
        {
            string query = @"
            BEGIN TRANSACTION
            Declare @userId INT 
            Declare @nickName NVARCHAT(75)

            SELECT @userId = id, @nickName = nick_name FROM Users WHERE nick_name = @nickNameOrEmail OR email = @nickNameOrEmail
            
            IF(ROWCOUNT_BIG() = 0)
			    BEGIN
				    PRINT 'There is no user with that id'
				    ROLLBACK
			    END
			ELSE
            BEGIN
                INSERT INTO Users_Chat_Keys (user_id,chat_id) VALUES(@userId,@chatId)
                SELECT @userId as UserId,creator as CreatorId , @nickName as NickName,name as ChatName,(SELECT Count(*) FROM Users_Chat_Keys WHERE chat_id = @chatId) AS ChatMembersCount FROM Chat WHERE id = @chatId
				COMMIT TRANSACTION
            END";

            using var connection = _dapperContext.CreateConnection();
            ChatOperationUser? user = await connection.QuerySingleOrDefaultAsync<ChatOperationUser?>(query, new { chatId, nickNameOrEmail }).ConfigureAwait(false);

            if (user is null)
            {
                return false;
            }

            var subject = GetOrCreateChat(chatId);

            subject.OnNext(new MessageSubscription(new()
            {
                ChatId = chatId,
                Content = $"{user.NickName} was added to chat",
                FromId = user.UserId,
                SentAt = DateTime.UtcNow
            })
            { Type = MessageType.USER_ADD });

            _userChatNotifyer.OnNext(new UserNotification()
            {
                UserId = user.UserId,
                NotificationType = ChatNotificationType.ENROLL,
                Name = user.ChatName,
                Id = chatId,
                CreatorId = user.CreatorId,
                ChatMembersCount = user.ChatMembersCount
            });

            return true;

        }

        private ISubject<object> GetOrCreateChat(int chatId)
        {
            try
            {
                return _chats[chatId];
            }
            catch
            {
                var subject = new Subject<object>();
                _chats.TryAdd(chatId, subject);
                return subject;
            }
        }

        public IObservable<object> SubscribeMessages(int chatId)
        {
            return GetOrCreateChat(chatId).AsObservable();
        }

        public async Task<IEnumerable<ChatParticipant>> GetAllChatParticipatsAsync(int chatId)
        {
            string query = @"SELECT u.nick_name,u.id FROM Users as u
                            JOIN Users_Chat_Keys as uck
                            ON u.id = uck.user_id
                            WHERE uck.chat_id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<ChatParticipant>(query, new { chatId });
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync(int chatId)
        {
            string query = "SELECT sent_at,content,from_id,chat_id FROM MESSAGE WHERE chat_id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<Message>(query, new { chatId });
        }

        public async Task<IEnumerable<ChatResult>> GetUserChatsInstances(int userId)
        {
            string query = @"SELECT ch.creator,ch.id,ch.name, (SELECT Count(*) FROM Users_Chat_Keys WHERE chat_id = ch.id) AS ChatMembersCount FROM Chat AS ch
                            JOIN  Users_Chat_Keys as uck
                            ON ch.id = uck.chat_id AND uck.user_id = @userId 
                            UNION ALL
                            SELECT ch.creator,ch.id,ch.name FROM Chat AS ch 
                            WHERE ch.creator = @userId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<ChatResult>(query, new { userId }).ConfigureAwait(false);
        }

        public async Task<bool> RemoveMessageAsync(Message message)
        {
            string query = "DELETE Message WHERE from_id = @FromId AND sent_at = @SentAt AND chat_id = @ChatId";
            using var connection = _dapperContext.CreateConnection();
            bool result = await connection.ExecuteAsync(query, new { message }).ConfigureAwait(false) > 0;
            if (result)
            {
                var subject = GetOrCreateChat(message.ChatId);
                subject.OnNext(new MessageSubscription(message) { Type = MessageType.DELETE });
            }
            return result;
        }

        public async Task<bool> UpdateMessageAsync(Message message)
        {
            string query = "UPDATE Message SET content = @content WHERE sent_at = @SentAt AND from_id = @FromId";
            using var connection = _dapperContext.CreateConnection();
            bool result = await connection.ExecuteAsync(query, message).ConfigureAwait(false) > 0;
            if (result)
            {
                var subject = GetOrCreateChat(message.ChatId);
                subject.OnNext(new MessageSubscription(message) { Type = MessageType.UPDATE });
            }
            return result;
        }

        public async Task<int> AddChatAsync(ChatModel chat)
        {
            string query = "INSERT INTO Chat (name,creator) OUTPUT Inserted.id VALUES(@Name,@CreatorId)";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleAsync<int>(query, chat).ConfigureAwait(false);
        }

        public async Task<bool> RemoveChatAsync(int chatId)
        {
            string query = "DELETE Chat WHERE chat_id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            bool result =  await connection.ExecuteAsync(query, new { chatId }).ConfigureAwait(false) > 0;

            if (result)
            {
                var subject = GetOrCreateChat(chatId);
                subject.OnNext(new ChatSubscription(ChatResultType.DELETE) { Id = chatId});
            }

            return result;
        }

        public async Task<bool> UpdateChatAsync(int chatId, string name)
        {
            string query = "UPDATE Chat SET Name = @name WHERE chat_id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            bool result = await connection.ExecuteAsync(query, new { chatId, name }).ConfigureAwait(false) > 0;

            if (result)
            {
                var subject = GetOrCreateChat(chatId);
                subject.OnNext(new ChatSubscription(ChatResultType.UPDATE) { Id = chatId, Name = name });
            }

            return result;

        }

        public async Task<bool> RemoveUserFromChatAsync(int chatId, string nickNameOrEmail)
        {
            string query = @"
            BEGIN TRANSACTION
            Declare @userId INT 
            Declare @nickName NVARCHAT(75)

            SELECT @userId = id, @nickName = nick_name FROM Users WHERE nick_name = @nickNameOrEmail OR email = @nickNameOrEmail
            
            IF(ROWCOUNT_BIG() = 0)
			    BEGIN
				    PRINT 'There is no user with that id'
				    ROLLBACK
			    END
			ELSE
            BEGIN
                DELETE Users_Chat_Keys WHERE user_id = @userId AND chat_id = @chatId
                SELECT @userId as UserId, @nickName as NickName, name as ChatName FROM Chat WHERE id = @chatId
				COMMIT TRANSACTION
            END";

            using var connection = _dapperContext.CreateConnection();
            ChatOperationUser? user = await connection.QuerySingleOrDefaultAsync<ChatOperationUser?>(query, new { chatId, nickNameOrEmail }).ConfigureAwait(false);

            if (user is null)
            {
                return false;
            }

            var subject = GetOrCreateChat(chatId);
            subject.OnNext(new MessageSubscription(new()
            {
                ChatId = chatId,
                Content = $"{user.NickName} was removed from chat",
                FromId = user.UserId,
                SentAt = DateTime.UtcNow
            })
            { Type = MessageType.USER_REMOVE });

            _userChatNotifyer.OnNext(new UserNotification()
            {
                UserId = user.UserId,
                NotificationType = ChatNotificationType.BANISH,
                Name = user.ChatName,
                Id = chatId,
                CreatorId = user.CreatorId,
                ChatMembersCount = user.ChatMembersCount
            });

            return true;
        }

        public async Task<IEnumerable<int>> GetUserChats(int userId)
        {
            string query = "SELECT chat_id FROM Users_Chat_Keys WHERE user_id = @userId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<int>(query, new { userId }).ConfigureAwait(false);
        }


        public async Task<IEnumerable<int>> GetUserCreationChats(int userId)
        {
            string query = "SELECT creator FROM Chat where creator = @userId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<int>(query, new { userId }).ConfigureAwait(false);
        }

        public async Task<bool> CheckPrecentUserInChat(int userId, int chatId)
        {
            string query = @"IF(EXISTS( SELECT * FROM Chat as c 
		  full join Users_Chat_Keys as u
		  on c.id = u.chat_id where c.creator = @userId 
		  OR (u.chat_id = @chatId AND u.user_id = @userId) ))
            SELECT 1
          ELSE
            SELECT 0";

            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleAsync<bool>(query, new { userId, chatId }).ConfigureAwait(false);
        }

        public async Task<bool> CheckUserOwnChat(int userId, int chatId)
        {
            string query = @"IF(EXISTS( SELECT * FROM Chat WHERE creator = @userId AND id = @chatId))
                    SELECT 1
                    ELSE
                    SELECT 0";

            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleAsync<bool>(query, new { userId, chatId }).ConfigureAwait(false);

        }

    }
}


