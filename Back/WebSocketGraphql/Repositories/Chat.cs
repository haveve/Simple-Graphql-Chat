using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Message = WebSocketGraphql.Models.Message;
using TimeTracker.Repositories;
using Dapper;
using WebSocketGraphql.Models;
using System.Xml.Linq;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.Repositories
{


    public class Chat : IChat
    {
        private const int _userInOneSubjectDefault = 1000;

        private readonly int _userNumberInSubject;

        private ConcurrentDictionary<int, Subject<object>> _chats;
        private ConcurrentDictionary<int, Subject<UserNotification>> _userChatNotifyer;
        private readonly DapperContext _dapperContext;

        public Chat(DapperContext dapperContext, IConfiguration configuration)
        {
            _dapperContext = dapperContext;
            _chats = new();
            _userChatNotifyer = new ConcurrentDictionary<int, Subject<UserNotification>>();

            if (!Int32.TryParse(configuration["ChatConfigData:UserSubNumber"], out _userNumberInSubject))
            {
                _userNumberInSubject = _userInOneSubjectDefault;
            }
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

        public IObservable<UserNotification> SubscribeUserNotification(int userId)
        {

            var maxUserId = ComputeUserSubIndex(userId);

            try
            {
                var sub = _userChatNotifyer[maxUserId];
                return sub.AsObservable();
            }
            catch
            {
                var newSub = new Subject<UserNotification>();
                _userChatNotifyer[maxUserId] = newSub;
                return newSub.AsObservable();
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

        public async ValueTask<bool> AddMessageAsync(Message message)
        {
            string query = "INSERT INTO Message (chat_id,sent_at,from_id,content) VALUES(@ChatId,@SentAt,@FromId,@Content)";
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

        public async ValueTask<bool> AddUserToChatAsync(int chatId, string nickNameOrEmail, string by)
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

            try
            {
                _userChatNotifyer[maxId].OnNext(new UserNotification()
                {
                    UserId = user.UserId,
                    NotificationType = ChatNotificationType.ENROLL,
                    Name = user.ChatName,
                    Id = chatId,
                    CreatorId = user.CreatorId,
                    ChatMembersCount = user.ChatMembersCount
                });
            }
            catch
            {

            }

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

        public async Task<IEnumerable<Message>> GetAllMessagesAsync(int chatId)
        {
            string query = @"SELECT u.nick_name, m.sent_at,m.content,m.from_id,m.chat_id FROM Message as m 
                                JOIN Users as u 
                                ON u.id = m.from_id
                                WHERE m.chat_id = @chatId
                            UNION ALL
                            SELECT null,sent_at,content,null,chat_id FROM TechMessage
                            WHERE chat_id = @chatId
                            ORDER BY sent_at";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<Message>(query, new { chatId });
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

        public async ValueTask<bool> RemoveMessageAsync(Message message)
        {
            string query = @"
                DECLARE @CompareDelete DateTime2(3) = @SentAt
                DELETE Message WHERE from_id = @FromId AND sent_at = @CompareDelete AND chat_id = @ChatId";
            using var connection = _dapperContext.CreateConnection();
            bool result = await connection.ExecuteAsync(query, message).ConfigureAwait(false) > 0;
            if (result)
            {
                var subject = GetOrCreateChat(message.ChatId);
                subject.OnNext(new MessageSubscription(message) { Type = MessageType.DELETE });
            }
            return result;
        }

        public async ValueTask<bool> UpdateMessageAsync(Message message)
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

        public async ValueTask<int> AddChatAsync(ChatModel chat)
        {
            string query = "INSERT INTO Chat (name,creator) OUTPUT Inserted.id VALUES(@Name,@CreatorId)";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleAsync<int>(query, chat).ConfigureAwait(false);
        }

        public async ValueTask<bool> LeaveFromChatAsync(string nickName, int chatId, bool deleteMessages = false)
        {
            return await RemoveUserFromChatAsync(chatId, nickName, deleteMessages).ConfigureAwait(false);
        }

        public async ValueTask<bool> RemoveChatAsync(int chatId)
        {
            string query = "DELETE Chat WHERE id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            bool result = await connection.ExecuteAsync(query, new { chatId }).ConfigureAwait(false) > 0;

            if (result)
            {
                var subject = GetOrCreateChat(chatId);
                subject.OnNext(new ChatSubscription(ChatResultType.DELETE) { Id = chatId });
            }

            return result;
        }

        public async ValueTask<bool> AddTechMessageAsync(int chatId, Message message)
        {
            string query = "INSERT INTO TechMessage(chat_id,content,sent_at) VALUES(@chatId,@Content,@SentAt)";
            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteAsync(query, new { chatId, message.Content, message.SentAt }).ConfigureAwait(false) > 0;
        }

        public async ValueTask<bool> UpdateChatAsync(int chatId, string name)
        {
            string query = "UPDATE Chat SET Name = @name WHERE id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            bool result = await connection.ExecuteAsync(query, new { chatId, name }).ConfigureAwait(false) > 0;

            if (result)
            {
                var subject = GetOrCreateChat(chatId);
                subject.OnNext(new ChatSubscription(ChatResultType.UPDATE) { Id = chatId, Name = name });
            }

            return result;

        }
        public async Task<string?> UpdateChatAvatarAsync(int chatId, string avatarName)
        {
            var query = @"update Chat
                        set avatar = @avatarName OUTPUT deleted.avatar where id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleAsync<string?>(query, new { chatId, avatarName });
        }

        public async ValueTask<bool> RemoveUserFromChatAsync(int chatId, string nickNameOrEmail, bool deleteAll = false, string? byOrSelf = null)
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

            try
            {
                _userChatNotifyer[maxId].OnNext(new UserNotification()
                {
                    UserId = user.UserId,
                    NotificationType = ChatNotificationType.BANISH,
                    Name = user.ChatName,
                    Id = chatId,
                    CreatorId = user.CreatorId,
                    ChatMembersCount = user.ChatMembersCount
                });
            }
            catch
            {

            }

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


