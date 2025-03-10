using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Message = WebSocketGraphql.Models.Message;
using WebSocketGraphql.Repositories;
using Dapper;
using WebSocketGraphql.Models;
using System.Xml.Linq;
using WebSocketGraphql.ViewModels;
using Microsoft.Extensions.Options;
using WebSocketGraphql.Configurations;

namespace WebSocketGraphql.Repositories
{
    public class Chat : IChat
    {
        private readonly int _userNumberInSubject;

        private readonly ConcurrentDictionary<int, Subject<object>> _chats = new();
        private readonly ConcurrentDictionary<int, Subject<object>> _userChatNotifyer = new();
        private readonly DapperContext _dapperContext;

        public Chat(DapperContext dapperContext, IOptions<GeneralSettings> generalSettings)
        {
            _dapperContext = dapperContext;
            _userNumberInSubject = generalSettings.Value.UserSubNumber;
        }

        private int ComputeUserSubIndex(int userId)
        {
            return (int)Math.Ceiling((decimal)userId / _userNumberInSubject) * _userNumberInSubject;
        }

        public void NotifyAllChats(IEnumerable<int> chatIds, object obj)
        {
            foreach (int i in chatIds)
            {
                try
                {
                    _chats[i].OnNext(obj);
                }
                catch
                {

                }
            }
        }

        public IObservable<object> SubscribeUserNotification(int userId)
        {

            var maxUserId = ComputeUserSubIndex(userId);

            try
            {
                var sub = _userChatNotifyer[maxUserId];
                return sub.AsObservable();
            }
            catch
            {
                var newSub = new Subject<object>();
                _userChatNotifyer[maxUserId] = newSub;
                return newSub.AsObservable();
            }

        }

        private Subject<object>? GetUserNotificationSubject(int userId)
        {
            try
            {
                var sub = _userChatNotifyer[userId];
                return sub;
            }
            catch
            {
                return null;
            }
        }

        public void UnSubscribeUserNotification(int userId)
        {
            var maxUserId = ComputeUserSubIndex(userId);

            try
            {
                var sub = _userChatNotifyer[maxUserId];
                if (!sub.HasObservers)
                {
                    _userChatNotifyer.Remove(maxUserId, out _);
                }
            }
            catch
            {

            }
        }

        public void UnSubscribeMessages(int chatId)
        {
            try
            {
                var chat = _chats[chatId];
                if (!chat.HasObservers)
                {
                    _chats.Remove(chatId, out _);
                }
            }
            catch
            {

            }
        }

        public async Task<bool> AddMessageAsync(Message message)
        {
            string query = "INSERT INTO Message (chat_id,sent_at,from_id,content,image) VALUES(@ChatId,@SentAt,@FromId,@Content,@Image)";
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
            public string ChatName { get; set; } = null!;
            public int UserId { get; set; }
            public string NickName { get; set; } = null!;
            public int CreatorId { get; set; } = 0;
            public string? Avatar { get; set; }
        }

        public async Task<bool> AddUserToChatAsync(int chatId, string nickNameOrEmail, string by)
        {
            string query = @"
            BEGIN TRANSACTION
            Declare @userId INT 
            Declare @nickName NVARCHAR(18)

            SELECT @userId = id, @nickName = nick_name FROM Users WHERE nick_name = @nickNameOrEmail OR email = @nickNameOrEmail
            
            IF(ROWCOUNT_BIG() = 0)
			    BEGIN
				    PRINT 'There is no user with that id'
				    ROLLBACK
			    END
			ELSE
            BEGIN
                INSERT INTO Users_Chat_Keys (user_id,chat_id) VALUES(@userId,@chatId)
                SELECT @userId as UserId,creator as CreatorId , @nickName as NickName,name as ChatName, avatar as Avatar FROM Chat WHERE id = @chatId
				COMMIT TRANSACTION
            END";

            using var connection = _dapperContext.CreateConnection();
            ChatOperationUser? user = await connection.QuerySingleOrDefaultAsync<ChatOperationUser?>(query, new { chatId, nickNameOrEmail }).ConfigureAwait(false);

            if (user is null)
            {
                return false;
            }

            var subject = GetOrCreateChat(chatId);

            var message = new MessageSubscription(new()
            {
                ChatId = chatId,
                Content = $"{user.NickName} was added to chat by {by}",
                FromId = user.UserId,
                SentAt = DateTime.UtcNow,
                NickName = user.NickName
            })
            { Type = MessageType.USER_ADD };

            var result = await AddTechMessageAsync(chatId, message);

            subject.OnNext(message);

            var maxId = ComputeUserSubIndex(user.UserId);

            GetUserNotificationSubject(maxId)?.OnNext(new UserNotification()
            {
                UserId = user.UserId,
                NotificationType = ChatNotificationType.ENROLL,
                Name = user.ChatName,
                Id = chatId,
                CreatorId = user.CreatorId,
                Avatar = user.Avatar
            });

            return result;

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

        public async Task<IEnumerable<ChatParticipant>> GetAllChatParticipatsAsync(int chatId, string search = "")
        {
            string query = @"
                            SELECT u.nick_name,u.id, u.online,u.avatar FROM Users as u
                            JOIN Chat as ch
                            ON ch.id = @chatId AND u.id = ch.creator
                            WHERE u.nick_name LIKE @search + '%'
                            UNION ALL
                            SELECT u.nick_name,u.id, u.online, u.avatar FROM Users as u
                            JOIN Users_Chat_Keys as uck
                            ON u.id = uck.user_id AND uck.chat_id = @chatId
                            WHERE u.nick_name LIKE @search + '%'";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<ChatParticipant>(query, new { chatId, search });
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync(int chatId, int take, int skip, DateTime? maxDate)
        {

            string skipMaxDate = maxDate is not null ? "sent_at < @maxDate AND" : string.Empty;

            string query = @$"SELECT u.nick_name, m.sent_at,m.content,m.from_id,m.chat_id,m.image FROM Message as m 
                                JOIN Users as u 
                                ON {skipMaxDate} u.id = m.from_id AND m.chat_id = @chatId 
                            UNION ALL
                            SELECT null,sent_at,content,null,chat_id,null FROM TechMessage
                            WHERE {skipMaxDate} chat_id = @chatId
                            ORDER BY sent_at desc
							OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<Message>(query, new { chatId, skip, take, maxDate });
        }

        public async Task<IEnumerable<ChatModel>> GetUserChatsInstancesAsync(int userId)
        {
            string query = @"SELECT ch.creator,ch.id,ch.name,ch.avatar FROM Chat AS ch
                            JOIN  Users_Chat_Keys as uck
                            ON ch.id = uck.chat_id AND uck.user_id = @userId 
                            UNION ALL
                            SELECT ch.creator,ch.id,ch.name,ch.avatar FROM Chat AS ch 
                            WHERE ch.creator = @userId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<ChatModel>(query, new { userId }).ConfigureAwait(false);
        }

        public async Task<ChatResult?> GetFullChatInfoAsync(int chatId, int userId)
        {
            string query = @"SELECT ch.creator,ch.id,ch.name,ch.avatar, (SELECT Count(*) FROM Users_Chat_Keys WHERE chat_id = ch.id) AS ChatMembersCount FROM Chat AS ch
                            JOIN  Users_Chat_Keys as uck
                            ON ch.id = @chatId AND uck.chat_id = @chatId AND uck.user_id = @userId
                            UNION ALL
                            SELECT ch.creator,ch.id,ch.name,ch.avatar,(SELECT Count(*) FROM Users_Chat_Keys WHERE chat_id = ch.id) AS ChatMembersCount FROM Chat AS ch 
                            WHERE ch.id = @chatId and ch.creator = @userId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ChatResult>(query, new { userId, chatId }).ConfigureAwait(false);
        }

        public async Task<string?> RemoveMessageAsync(Message message)
        {
            string query = @"
                DECLARE @CompareDelete DateTime2(3) = @SentAt
                DELETE Message OUTPUT deleted.image WHERE from_id = @FromId AND sent_at = @CompareDelete AND chat_id = @ChatId";
            using var connection = _dapperContext.CreateConnection();
            string? img = await connection.QuerySingleAsync<string?>(query, message).ConfigureAwait(false);

            var subject = GetOrCreateChat(message.ChatId);
            subject.OnNext(new MessageSubscription(message) { Type = MessageType.DELETE });

            return img;
        }

        public async Task<bool> UpdateMessageAsync(Message message)
        {
            string query = @"DECLARE @CompareDelete DateTime2(3) = @SentAt
            UPDATE Message SET content = @content WHERE sent_at = @CompareDelete AND from_id = @FromId AND chat_id = @ChatId";
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

        public async Task<bool> LeaveFromChatAsync(string nickName, int chatId, bool deleteMessages = false)
        {
            return await RemoveUserFromChatAsync(chatId, nickName, deleteMessages).ConfigureAwait(false);
        }

        public async Task<string?> RemoveChatAsync(int chatId)
        {
            string query = @"DECLARE @chatPicture nvarchar(46)
                            DECLARE @MyTableVar TABLE (
                             userId INT null);

							INSERT INTO @MyTableVar SELECT user_id from Users_Chat_Keys where chat_id = @chatId
                            UNION ALL Select creator from chat where id = @chatId

                            SELECT @chatPicture = avatar from Chat where id = @chatId
                            DELETE Chat WHERE id = @chatId
                            select null as avatar, userId from @MyTableVar
                            UNION ALL select @chatPicture, null";

            string? returnResult = null;

            using var connection = _dapperContext.CreateConnection();
            var users = (await connection.QueryAsync<string?, int?, int>(query, (avatar, userId) =>
            {
                if (userId is null)
                {
                    returnResult = avatar;
                }
                return userId ?? 0;
            }, new { chatId }, splitOn: "userId").ConfigureAwait(false)).SkipLast(1);

            if (users.Any())
            {
                foreach (var userId in users)
                {

                    var subject = GetUserNotificationSubject(ComputeUserSubIndex(chatId));
                    subject?.OnNext(new ChatSubscription(ChatResultType.DELETE) { Id = chatId, UserId = userId });
                }
            }

            return returnResult;
        }

        public async Task<bool> AddTechMessageAsync(int chatId, Message message)
        {
            string query = "INSERT INTO TechMessage(chat_id,content,sent_at) VALUES(@chatId,@Content,@SentAt)";
            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteAsync(query, new { chatId, message.Content, message.SentAt }).ConfigureAwait(false) > 0;
        }

        public async Task<bool> UpdateChatAsync(int chatId, string name)
        {
            string query = @"set NOCOUNT ON UPDATE 
                            Chat SET Name = @name WHERE id = @chatId
                            SELECT user_id from Users_Chat_Keys where chat_id = @chatId
                            UNION ALL Select creator from chat where id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            var users = await connection.QueryAsync<int>(query, new { chatId, name }).ConfigureAwait(false);

            var result = users.Any();
            if (result)
            {
                foreach (var userId in users)
                {
                    var subject = GetUserNotificationSubject(ComputeUserSubIndex(chatId));
                    subject?.OnNext(new ChatSubscription(ChatResultType.UPDATE) { Id = chatId, Name = name, UserId = userId });
                }
            }

            return result;

        }
        public async Task<string?> UpdateChatAvatarAsync(int chatId, string avatarName)
        {
            var query = @"DECLARE @MyTableVar TABLE (
                        avatar nvarchar(46) null);

                        update Chat set avatar = @avatarName OUTPUT deleted.avatar into @MyTableVar where id = @chatId

                        SELECT null as avatar, user_id from Users_Chat_Keys where chat_id = @chatId
                        UNION ALL Select null,creator from chat where id = @chatId
                        UNION ALL SELECT avatar,null from @MyTableVar";

            string? returnResult = null;

            using var connection = _dapperContext.CreateConnection();
            var users = (await connection.QueryAsync<string?, int?, int>(query, (avatar, userId) =>
            {
                if (userId is null)
                {
                    returnResult = avatar;
                }
                return userId ?? 0;
            }, new { chatId, avatarName }, splitOn: "user_id").ConfigureAwait(false)).SkipLast(1);


            if (users.Any())
            {
                foreach (var userId in users)
                {

                    var subject = GetUserNotificationSubject(ComputeUserSubIndex(chatId));
                    subject?.OnNext(new ChatSubscription(ChatResultType.UPDATE) { Id = chatId, Avatar = avatarName, UserId = userId });
                }
            }

            return returnResult;
        }

        public async Task<bool> RemoveUserFromChatAsync(int chatId, string nickNameOrEmail, bool deleteAll = false, string? byOrSelf = null)
        {
            string query = @"
            BEGIN TRANSACTION
            Declare @userId INT 
            Declare @nickName NVARCHAR(18)

            SELECT @userId = id, @nickName = nick_name FROM Users WHERE nick_name = @nickNameOrEmail OR email = @nickNameOrEmail
            
            IF(ROWCOUNT_BIG() = 0)
			    BEGIN
				    PRINT 'There is no user with that id'
				    ROLLBACK
			    END
			ELSE
            BEGIN
                DELETE Users_Chat_Keys WHERE user_id = @userId AND chat_id = @chatId
                IF(ROWCOUNT_BIG() = 0)
			    BEGIN
				    PRINT 'There is no user in chat with that id'
				    ROLLBACK
			    END
                SELECT @userId as UserId, @nickName as NickName, name as ChatName FROM Chat WHERE id = @chatId
				IF(@deleteAll = 1)
                DELETE FROM Message
                where from_id = @userId AND chat_id = @chatId
                COMMIT TRANSACTION
            END";

            using var connection = _dapperContext.CreateConnection();
            ChatOperationUser? user = await connection.QuerySingleOrDefaultAsync<ChatOperationUser?>(query, new { chatId, nickNameOrEmail, deleteAll }).ConfigureAwait(false);

            if (user is null)
            {
                return false;
            }

            var subject = GetOrCreateChat(chatId);

            string leaveOrRemove = byOrSelf is null ? "leave" : "removed";
            string byOrNothing = byOrSelf is null ? string.Empty : $"by {byOrSelf}";

            var message = new MessageSubscription(new()
            {
                ChatId = chatId,
                Content = $"{user.NickName} was {leaveOrRemove} from chat {byOrNothing}",
                FromId = user.UserId,
                SentAt = DateTime.UtcNow,
            })
            { Type = MessageType.USER_REMOVE, DeleteAll = deleteAll };

            var result = await AddTechMessageAsync(chatId, message);
            subject.OnNext(message);

            var maxId = ComputeUserSubIndex(user.UserId);


            GetUserNotificationSubject(maxId)?.OnNext(new UserNotification()
            {
                UserId = user.UserId,
                NotificationType = ChatNotificationType.BANISH,
                Name = user.ChatName,
                Id = chatId,
                CreatorId = user.CreatorId,
                Avatar = user.Avatar
            });

            return result;
        }

        public async Task<IEnumerable<int>> GetUserChatsAsync(int userId)
        {
            string query = "SELECT chat_id FROM Users_Chat_Keys WHERE user_id = @userId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<int>(query, new { userId }).ConfigureAwait(false);
        }


        public async Task<IEnumerable<int>> GetUserCreationChatsAsync(int userId)
        {
            string query = "SELECT id FROM Chat where creator = @userId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<int>(query, new { userId }).ConfigureAwait(false);
        }

        public Task<bool> SetOnile(int userId)
        {
            var task = Task.Run(() =>
            {
                string query = @"UPDATE Users 
                SET online = 1
                WHERE id = @userId";
                using var connection = _dapperContext.CreateConnection();
                return connection.Execute(query, new { userId }) > 0;
            });
            return task;
        }

        public Task<IEnumerable<int>> SetOffline(int userId)
        {
            string query = @"SET NOCOUNT ON
            UPDATE Users 
            SET online = 0
            WHERE id = @userId

            SELECT chat_id FROM Users_Chat_Keys
            UNION ALL
            SELECT id From Chat";


            var task = Task.Run(() =>
            {
                using var connection = _dapperContext.CreateConnection();
                return connection.Query<int>(query, new { userId });
            });

            return task;
        }

    }
}


