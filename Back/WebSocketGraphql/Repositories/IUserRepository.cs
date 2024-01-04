using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TimeTracker.Models;

namespace TimeTracker.Repositories
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
