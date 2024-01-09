using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using TimeTracker.Models;
using WebSocketGraphql.Models;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.Repositories
{
    public interface IChat
    {
        void NotifyAllChats(IEnumerable<int> chatIds, object obj);

        Task<bool> AddMessageAsync(Message message);

        Task<IEnumerable<Message>> GetAllMessagesAsync(int chatId, int take, int skip, DateTime? maxDate);

        Task<IEnumerable<ChatParticipant>> GetAllChatParticipatsAsync(int chatId,string search = "");

        IObservable<object> SubscribeMessages(int chatId);

        IObservable<object> SubscribeUserNotification(int userId);

        void UnSubscribeUserNotification(int userId);

        Task<bool> SetOnile(int userId);

        void UnSubscribeMessages(int chatId);

        Task<IEnumerable<int>> SetOffline(int userId);

        Task<string?> RemoveMessageAsync(Message message);

        Task<bool> UpdateMessageAsync(Message message);

        Task<int> AddChatAsync(ChatModel chat);

        Task<string?> RemoveChatAsync(int chatId);

        Task<bool> UpdateChatAsync(int chatId, string name);

        Task<string?> UpdateChatAvatarAsync(int chatId, string avatarName);

        Task<bool> AddUserToChatAsync(int chatId, string nickNameOrEmail, string by);

        Task<bool> RemoveUserFromChatAsync(int chatId, string nickNameOrEmail, bool deletedAll = false, string? byOrSelf = null);

        Task<IEnumerable<int>> GetUserChatsAsync(int userId);

        Task<IEnumerable<int>> GetUserCreationChatsAsync(int userId);

        Task<IEnumerable<ChatModel>> GetUserChatsInstancesAsync(int userId);

        Task<ChatResult?> GetFullChatInfoAsync(int chatId, int userId);

        Task<bool> LeaveFromChatAsync(string nickName, int chatId, bool deleteMessages = false);

        Task<bool> AddTechMessageAsync(int chatId, Message message);

    }
}
