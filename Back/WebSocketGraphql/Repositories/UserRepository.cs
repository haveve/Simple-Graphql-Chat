using Dapper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using TimeTracker.Models;
using TimeTracker.Services;
using WebSocketGraphql.Services;

namespace TimeTracker.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly DapperContext _dapperContext;
        private readonly int _iteration;

        public UserRepository(DapperContext context, IConfiguration configuration,IAuthorizationRepository authorizationRepository)
        {
            _configuration = configuration;
            _authorizationRepository = authorizationRepository;
            _dapperContext = context;
            try
            {
                _iteration = Convert.ToInt32(configuration["PasswordHashing:Iteration"]);
            }
            catch
            {
                _iteration = 0;
            }
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

        public async Task DeleteUserAsync(int id)
        {
            string query = "DELETE FROM Users WHERE id = @id";
            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, param: new {id}).ConfigureAwait(false);
        }

        public async Task<User?> GetUserAsync(int id)
        {
            string query = "SELECT id,nick_name,email FROM Users WHERE id = @id";
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

            if(password.ComparePasswords(user.Password, true,user.Salt,_iteration))
            {
                return user;
            }

            return null;
        }

        public async Task<User?> GetUserByNickNameOrEmailAsync(string nickNameOrEmail)
        {
            string query = "SELECT id,nick_name,email,password,activate_code,salt FROM Users WHERE nick_name = @nickNameOrEmail OR email = @nickNameOrEmail";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User>(query, param: new { nickNameOrEmail }).ConfigureAwait(false);
        }

        public async Task UpdateUserAsync(User user, bool withPassword = false, bool withCode = false)
        {
            string query = "UPDATE Users SET nick_name = @NickName,email = @Email";

            if (withPassword)
            {
                var salt = PasswordHasher.GenerateSalt();
                user.Password = user.Password.ComputeHash(salt,_iteration);
                query += ",password = @Password,salt = @salt";
            }

            if (withCode)
            {
                query += ",acrivate_code = @AcrivateCode";
            }

            query += "WHERE id = @Id";

            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, param: user).ConfigureAwait(false);
        }

        public async Task UpdateUserPasswordAndCodeAsync(int id, string password)
        {
            string query = "UPDATE Users SET password = @password, salt = @salt, activate_code = @code WHERE id = @id";
            var salt = PasswordHasher.GenerateSalt();
            password = password.ComputeHash(salt, _iteration);
            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, param: new { id, password, salt,code = (string?)null }).ConfigureAwait(false);
        }

        public async Task UpdateUserResetCodeByIdAsync(int id, string code)
        {
            string query = "UPDATE Users SET acrivate_code = @code WHERE id = @id";
            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, param: new { id, code }).ConfigureAwait(false);
        }
    }
}
