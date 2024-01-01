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
        public Task<UpdateUser> UpdateUserAsync(UpdateUser data);
        public Task<User?> GetUserByCredentialsAsync(string nickNameOrEmail, string password);
        public Task<string> CreateUserAsync(User user);
        public Task<int> DeleteUserAsync(RemoveUser data);
        Task<User?> GetUserByNickNameOrEmailAsync(string nickNameOrEmail);
        Task UpdateUserResetCodeByIdAsync(int id, string code);
        Task UpdateUserPasswordAndCodeAsync(int id, string password);
    }
}
