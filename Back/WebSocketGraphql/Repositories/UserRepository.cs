using Dapper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using TimeTracker.Models;
using TimeTracker.Services;
using WebSocketGraphql.Services;
using WebSocketGraphql.Helpers;
using Microsoft.AspNetCore.Mvc;
using WebSocketGraphql.Repositories;

namespace TimeTracker.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly DapperContext _dapperContext;
        private readonly int _iteration;

        public UserRepository(DapperContext context, IConfiguration configuration, IAuthorizationRepository authorizationRepository)
        {
            _configuration = configuration;
            _authorizationRepository = authorizationRepository;
            _dapperContext = context;
            _iteration = configuration.GetIteration();
        }

        public async Task<string> CreateUserAsync(User user)
        {
            string code = EmailSendHelper.GetUniqueCode();
            string query = "INSERT INTO Users (nick_name,email,activate_code) VALUES(@NickName,@Email,@ActivateCode)";
            user.ActivateCode = code;
            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, param: user);
            return code;
        }

        public async Task<string?> DeleteUserAsync(RemoveUser data)
        {
            var salt = await GetUserSalt(data.Id);
            string query = @"DECLARE @userPicture nvarchar(46)
                            SELECT @userPicture = avatar from Users WHERE id = @Id
                            DELETE Users WHERE id = @Id AND password = @Password
                            IF(@@ROWCOUNT > 0)
								select @userPicture
							ELSE 
								THROW 60000,'invalid data',1";
            data.Password = data.Password.ComputeHash(salt, _iteration);
            using var connection = _dapperContext.CreateConnection();
            try
            {
                return await connection.QuerySingleAsync<string?>(query, data).ConfigureAwait(false);
            }
            catch
            {
                throw new InvalidDataException("Incorrect data");
            }
        }

        public async Task<UpdateUser> UpdateUserAsync(UpdateUser data)
        {
            using var connection = _dapperContext.CreateConnection();
            var salt = await GetUserSalt(data.Id);
            var dotNotChangePassword = data.NewPassword.Equals(data.OldPassword);

            salt = dotNotChangePassword ? salt : PasswordHasher.GenerateSalt();
            data.OldPassword = data.OldPassword.ComputeHash(salt, _iteration);
            data.NewPassword = dotNotChangePassword ?
                               data.OldPassword :
                               data.NewPassword.ComputeHash(salt, _iteration);
            data.Salt = salt;

            string query = @"UPDATE Users SET email = @Email, nick_name = @NickName,password = @NewPassword, salt = @Salt 
                             WHERE id = @Id AND password = @OldPassword";
            return await connection.ExecuteAsync(query, data) > 0 ? data : throw new InvalidDataException("uncorrect data");
        }

        public async Task<string?> UpdateUserAvatarAsync(int userId, string avatarName)
        {
            var query = @"update users
                        set avatar = @avatarName OUTPUT deleted.avatar where id = @userId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleAsync<string?>(query, new { userId, avatarName });
        }

        public async Task<User?> GetUserAsync(int id)
        {
            string query = "SELECT id,nick_name,email,reset_key_2auth,key_2auth,avatar FROM Users WHERE id = @id";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User>(query, param: new { id }).ConfigureAwait(false);
        }

        public async Task<User?> GetUserByCredentialsAsync(string nickNameOrEmail, string password)
        {
            var user = await GetUserByNickNameOrEmailAsync(nickNameOrEmail).ConfigureAwait(false);

            if (user is null)
            {
                return user;
            }

            if (password.ComparePasswords(user.Password, true, user.Salt, _iteration))
            {
                return user;
            }

            return null;
        }

        public async Task<User?> GetUserByNickNameOrEmailAsync(string nickNameOrEmail)
        {
            string query = "SELECT id,nick_name,email,password,activate_code,salt, key_2auth, reset_key_2auth  FROM Users WHERE nick_name = @nickNameOrEmail OR email = @nickNameOrEmail";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User>(query, param: new { nickNameOrEmail }).ConfigureAwait(false);
        }

        public async Task UpdateUserPasswordAndCodeAsync(int id, string password)
        {
            string query = "UPDATE Users SET password = @password, salt = @salt, activate_code = @code WHERE id = @id";
            var salt = PasswordHasher.GenerateSalt();
            password = password.ComputeHash(salt, _iteration);
            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, param: new { id, password, salt, code = (string?)null }).ConfigureAwait(false);
        }

        public async Task UpdateUserResetCodeByIdAsync(int id, string code)
        {
            string query = "UPDATE Users SET activate_code = @code WHERE id = @id";
            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, param: new { id, code }).ConfigureAwait(false);
        }

        private async Task<string> GetUserSalt(int id)
        {
            using var connection = _dapperContext.CreateConnection();
            string queryUserSalt = "SELECT salt from Users where id = @id";
            return await connection.QuerySingleAsync<string>(queryUserSalt, new { id });
        }
    }
}
