using Dapper;
using System.Data;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using TimeTracker.GraphQL.Types.IdentityTipes.Models;

namespace TimeTracker.Repositories
{
    public class AuthorizationRepository : IAuthorizationRepository
    {
        private readonly DapperContext _dapperContext;

        public AuthorizationRepository(DapperContext context)
        {
            _dapperContext = context;
        }

        public async Task CreateRefreshTokenAsync(TokenResult refreshToken, int userId)
        {

            string query = "INSERT INTO UserRefreshes (user_id, expiration_start, expiration_end, token) VALUES(@userId, @issuedAt, @expiredAt, @token)";
            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, new { userId, refreshToken.token, refreshToken.expiredAt, refreshToken.issuedAt }).ConfigureAwait(false);
        }

        public async Task DeleteAllRefreshTokensAsync(int userId)
        {
            string query = "DELETE UserRefreshes WHERE user_id = @userId";
            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, new { userId }).ConfigureAwait(false);
        }

        public async Task DeleteRefreshTokenAsync(string refreshToken)
        {
            string query = "DELETE UserRefreshes WHERE token = @refreshToken";
            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, new { refreshToken }).ConfigureAwait(false);
        }

        public async Task UpdateRefreshTokenAsync(string oldRefreshToken, TokenResult refreshToken, int userId)
        {
            string query = "UPDATE UserRefreshes SET token = @token, expiration_start = @issuedAt , expiration_end = @expiredAt   WHERE user_id = @userId AND token = @oldRefreshToken";
            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(query, new { userId, refreshToken.token, refreshToken.expiredAt, refreshToken.issuedAt, oldRefreshToken }).ConfigureAwait(false);
        }
        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, int userId)
        {
            string query = "SELECT token, expiration_start, expiration_end FROM UserRefreshes WHERE token = @refreshToken AND user_id = @userId";
            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<RefreshToken?>(query, new { refreshToken, userId });
        }


        public async Task<bool> Add2factorKeyAsync(int userId, string key, string resetCode)
        {
            using var connection = _dapperContext.CreateConnection();
            var sqlQuery = @"IF(EXISTS(select * from users where id = @userId and key_2auth is not distinct from null))
                            Update Users SET key_2auth = @key, reset_key_2auth = @resetCode WHERE Id = @userId";
            return await connection.ExecuteAsync(sqlQuery, new { key, userId, resetCode }).ConfigureAwait(false) > 0;
        }

        public async Task<bool> Drop2factorKeyAsync(int userId, string? resetCode)
        {
            using var connection = _dapperContext.CreateConnection();
            string sqlQuery = "Update Users SET key_2auth = @key, reset_key_2auth = @code WHERE id = @userId";
            if (resetCode is not null)
            {
                sqlQuery += " AND reset_key_2auth = @resetCode";
            }

            return await connection.ExecuteAsync(sqlQuery, new { key = (string?)null, code = (string?)null, userId, resetCode }).ConfigureAwait(false) > 0;
        }

        public async Task<string?> Get2factorKeyAsync(int userId)
        {
            using var connection = _dapperContext.CreateConnection();
            var sqlQuery = "SELECT key_2auth FROM Users WHERE Id = @userId";
            return await connection.QuerySingleAsync<string?>(sqlQuery, new { userId }).ConfigureAwait(false);
        }
    }
}
