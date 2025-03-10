using WebSocketGraphql.ViewModels;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserAsync(int id);
        Task<string?> UpdateUserAvatarAsync(int userId, string avatarName);
        Task<UpdateUser> UpdateUserAsync(UpdateUser data);
        Task<User?> GetUserByCredentialsAsync(string nickNameOrEmail, string password);
        Task<string> CreateUserAsync(User user);
        Task<string?> DeleteUserAsync(RemoveUser data);
        Task<User?> GetUserByNickNameOrEmailAsync(string nickNameOrEmail);
        Task UpdateUserResetCodeByIdAsync(int id, string code);
        Task UpdateUserPasswordAndCodeAsync(int id, string password);
    }
}
