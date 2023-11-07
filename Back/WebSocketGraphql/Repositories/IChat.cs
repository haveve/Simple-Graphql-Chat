using System.Collections.Concurrent;
using System.Reactive.Subjects;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.Repositories
{
    public interface IChat
    {

        Task<bool> AddMessageAsync(Message message);

        Task<IEnumerable<Message>> GetAllMessages(int chatId);

        IObservable<Message> Messages(int chatId);

        Task<bool> RemoveMessageAsync(int fromId, DateTime sentAt);

        Task<bool> UpdateMessageAsync(Message message);

        Task<bool> AddChatAsync(ChatModel chat);

        Task<bool> RemoveChatAsync(int chatId);

        Task<bool> UpdateChatAsync(int chatId, string name);

        Task<bool> AddUserToChatAsync(int chatId, string nickNameOrEmail);

        Task<bool> RemoveUserFromChatAsync(int chatId, string nickNameOrEmail);

    }
}
