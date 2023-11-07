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

namespace WebSocketGraphql.Repositories
{
    public class Chat : IChat
    {
        private ConcurrentDictionary<int, ISubject<Message>> _chats;
        private readonly DapperContext _dapperContext;

        public Chat(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
            _chats = new();
        }

        public async Task<bool> AddMessageAsync(Message message)
        {
            string query = "INSERT INTO Message (chat_id,sent_at,fromt_id,content) VALUES(@ChatId,@SentAt,@FromId,@Content)";
            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteAsync(query, message).ConfigureAwait(false) > 0;
        }

        public async Task<bool> AddUserToChatAsync(int chatId, string nickNameOrEmail)
        {
            string query = @"
            BEGIN TRANSACTION
            Declare @userId INT 
            SELECT @userId = id FROM Users WHERE nick_name = @nickNameOrEmail OR email = @nickNameOrEmail
            
            IF(ROWCOUNT_BIG() = 0)
			    BEGIN
				    PRINT 'There is no user with that id'
				    ROLLBACK
			    END
			ELSE
            BEGIN
                INSERT INTO Users_Chat_Keys (user_id,chat_id) VALUES(@userId,@chatId)
				COMMIT TRANSACTION
            END";

            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteAsync(query, new {chatId,nickNameOrEmail}).ConfigureAwait(false) > 0;

        }

        public IObservable<Message> Messages(int chatId)
        {
            try
            {
                return _chats[chatId].AsObservable();
            }
            catch
            {
                var subject = new Subject<Message>();
                _chats.TryAdd(chatId, subject);
                return subject.AsObservable();
            }
        }

        public async Task<IEnumerable<Message>> GetAllMessages(int chatId)
        {
            string query = "SELECT sent_at,content,from_id,chat_id FROM MESSAGE WHERE chat_id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<Message>(query,new {chatId});
        }

        public async Task<bool> RemoveMessageAsync(int fromId, DateTime sentAt)
        {
            string query = "DELETE Message WHERE from_id = @fromId AND sent_at = @sentAt";
            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteAsync(query, new { fromId , sentAt }).ConfigureAwait(false) > 0;
        }

        public async Task<bool> UpdateMessageAsync(Message message)
        {
            string query = "UPDATE Message SET content = @content WHERE sent_at = @SentAt AND from_id = @FromId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteAsync(query, message).ConfigureAwait(false) > 0;
        }

        public async Task<bool> AddChatAsync(ChatModel chat)
        {
            string query = "INSERT INTO Chat (name,creator) VALUEST(@Name,@CreatorId)";
            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteAsync(query, chat).ConfigureAwait(false) > 0;
        }

        public async Task<bool> RemoveChatAsync(int chatId)
        {
            string query = "DELETE Chat WHERE chat_id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteAsync(query, new { chatId}).ConfigureAwait(false) > 0;
        }

        public async Task<bool> UpdateChatAsync(int chatId, string name)
        {
            string query = "UPDATE Chat SET Name = @name WHERE chat_id = @chatId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteAsync(query, new { chatId, name }).ConfigureAwait(false) > 0;
        }

        public async Task<bool> RemoveUserFromChatAsync(int chatId, string nickNameOrEmail)
        {
            string query = @"
            BEGIN TRANSACTION
            Declare @userId INT 
            SELECT @userId = id FROM Users WHERE nick_name = @nickNameOrEmail OR email = @nickNameOrEmail
            
            IF(ROWCOUNT_BIG() = 0)
			    BEGIN
				    PRINT 'There is no user with that id'
				    ROLLBACK
			    END
			ELSE
            BEGIN
                DELETE Users_Chat_Keys WHERE user_id = @userId AND chat_id = @chatId
				COMMIT TRANSACTION
            END";

            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteAsync(query, new { chatId, nickNameOrEmail }).ConfigureAwait(false) > 0;
        }
    }
}
