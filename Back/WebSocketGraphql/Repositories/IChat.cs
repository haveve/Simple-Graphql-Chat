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

        ValueTask<bool> AddMessageAsync(Message message);

        Task<IEnumerable<Message>> GetAllMessagesAsync(int chatId);

        Task<IEnumerable<ChatParticipant>> GetAllChatParticipatsAsync(int chatId,string search = "");

        IObservable<object> SubscribeMessages(int chatId);

        IObservable<UserNotification> SubscribeUserNotification(int userId);

        void UnSubscribeUserNotification(int userId);

        Task<bool> SetOnile(int userId);

        void UnSubscribeMessages(int chatId);

        Task<IEnumerable<int>> SetOffline(int userId);

        ValueTask<bool> RemoveMessageAsync(Message message);

        ValueTask<bool> UpdateMessageAsync(Message message);

        ValueTask<int> AddChatAsync(ChatModel chat);

        ValueTask<bool> RemoveChatAsync(int chatId);

        ValueTask<bool> UpdateChatAsync(int chatId, string name);

        Task<string?> UpdateChatAvatarAsync(int chatId, string avatarName);

        ValueTask<bool> AddUserToChatAsync(int chatId, string nickNameOrEmail, string by);

        ValueTask<bool> RemoveUserFromChatAsync(int chatId, string nickNameOrEmail, bool deletedAll = false, string? byOrSelf = null);

        Task<IEnumerable<int>> GetUserChatsAsync(int userId);

        Task<IEnumerable<int>> GetUserCreationChatsAsync(int userId);

        Task<IEnumerable<ChatModel>> GetUserChatsInstancesAsync(int userId);

        Task<ChatResult?> GetFullChatInfoAsync(int chatId, int userId);

        ValueTask<bool> LeaveFromChatAsync(string nickName, int chatId, bool deleteMessages = false);

        ValueTask<bool> AddTechMessageAsync(int chatId, Message message);

    }
}
