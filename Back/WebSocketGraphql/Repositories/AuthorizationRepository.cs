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

        void IAuthorizationRepository.CreateRefreshToken(TokenResult refreshToken, int userId)
        {

            string query = "INSERT INTO UserRefreshes (user_id, expiration_start, expiration_end, token) VALUES(@userId, @issuedAt, @expiredAt, @token)";
            using var connection = _dapperContext.CreateConnection();
            connection.Execute(query, new { userId, refreshToken.token, refreshToken.expiredAt, refreshToken.issuedAt });
        }

        void IAuthorizationRepository.DeleteAllRefreshTokens(int userId)
        {
            string query = "DELETE UserRefreshes WHERE user_id = @userId";
            using var connection = _dapperContext.CreateConnection();
            connection.Execute(query, new { userId });
        }

        void IAuthorizationRepository.DeleteRefreshToken(string refreshToken)
        {
            string query = "DELETE UserRefreshes WHERE token = @refreshToken";
            using var connection = _dapperContext.CreateConnection();
            connection.Execute(query, new {refreshToken });
        }

        void IAuthorizationRepository.UpdateRefreshToken(string oldRefreshToken, TokenResult refreshToken, int userId)
        {
            string query = "UPDATE UserRefreshes SET token = @token, expiration_start = @issuedAt , expiration_end = @expiredAt   WHERE user_id = @userId AND token = @oldRefreshToken";
            using var connection = _dapperContext.CreateConnection();
            connection.Execute(query, new { userId, refreshToken.token,refreshToken.expiredAt,refreshToken.issuedAt, oldRefreshToken });
        }
        public RefreshToken? GetRefreshToken(string refreshToken, int userId)
        {
            string query = "SELECT token, expiration_start, expiration_end FROM UserRefreshes WHERE token = @refreshToken AND user_id = @userId";
            using var connection = _dapperContext.CreateConnection();
            return connection.QuerySingleOrDefault<RefreshToken?>(query, new {refreshToken, userId });
        }


        public bool Add2factorKey(int userId, string key, string resetCode)
        {
            using var connection = _dapperContext.CreateConnection();
            var sqlQuery = @"IF(EXISTS(select * from users where id = @userId and key_2auth is not distinct from null))
                            Update Users SET key_2auth = @key, reset_key_2auth = @resetCode WHERE Id = @userId";
            return connection.Execute(sqlQuery, new { key, userId, resetCode }) > 0;
        }

        public bool Drop2factorKey(int userId,string? resetCode)
        {
            using var connection = _dapperContext.CreateConnection();
            string sqlQuery = "Update Users SET key_2auth = @key WHERE id = @userId";
            if(resetCode is not null)
            {
                sqlQuery += "AND reset_key_2auth = @resetCode";
            }

            return connection.Execute(sqlQuery, new { key = (string?)null, userId, resetCode }) > 0;
        }

        public string? Get2factorKey(int userId)
        {
            using var connection = _dapperContext.CreateConnection();
            var sqlQuery = "SELECT key_2auth FROM Users WHERE Id = @userId";
            return connection.QuerySingle<string?>(sqlQuery, new { userId });
        }
    }
}
