using System.Collections.Concurrent;
using System.Reactive.Subjects;
using TimeTracker.Models;
using WebSocketGraphql.Models;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.Repositories
{
    public interface IChat
    {

        Task<bool> AddMessageAsync(Message message);

        Task<IEnumerable<Message>> GetAllMessagesAsync(int chatId);

        Task<IEnumerable<ChatParticipant>> GetAllChatParticipatsAsync(int chatId);

        IObservable<object> SubscribeMessages(int chatId);

        Task<bool> RemoveMessageAsync(Message message);

        Task<bool> UpdateMessageAsync(Message message);

        Task<int> AddChatAsync(ChatModel chat);

        Task<bool> RemoveChatAsync(int chatId);

        Task<bool> UpdateChatAsync(int chatId, string name);

        Task<bool> AddUserToChatAsync(int chatId, string nickNameOrEmail);

        Task<bool> RemoveUserFromChatAsync(int chatId, string nickNameOrEmail);

        Task<IEnumerable<int>> GetUserChats(int userId);

        Task<IEnumerable<int>> GetUserCreationChats(int userId);

        Task<bool> CheckPrecentUserInChat(int userId, int chatId);

        Task<bool> CheckUserOwnChat(int userId, int chatId);

        Task<IEnumerable<ChatResult>> GetUserChatsInstances(int userId);
  
        }
    }
