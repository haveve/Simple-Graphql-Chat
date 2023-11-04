using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TimeTracker.Models;

namespace TimeTracker.Repositories
{
    public interface IUserRepository
    {
        public Task<User?> GetUserAsync(int id);
        public Task<User?> GetUserByCredentialsAsync(string nickNameOrEmail, string password);
        public Task<string> CreateUserAsync(User user);
        public Task UpdateUserAsync(User user, bool withPassword = false, bool withCode = false);
        public Task DeleteUserAsync(int id);
        Task<User?> GetUserByNickNameOrEmailAsync(string nickNameOrEmail);
        Task UpdateUserResetCodeByIdAsync(int id, string code);
        Task UpdateUserPasswordAndCodeAsync(int id, string password);
    }
}
