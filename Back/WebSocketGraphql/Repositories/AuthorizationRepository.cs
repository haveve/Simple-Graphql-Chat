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

            string query = "INSERT INTO UserRefreshes (UserId, ExpiresStart, ExpiresEnd, Token) VALUES(@userId, @issuedAt, @expiredAt, @token)";
            using var connection = _dapperContext.CreateConnection();
            connection.Execute(query, new { userId, refreshToken.token, refreshToken.expiredAt, refreshToken.issuedAt });
        }

        void IAuthorizationRepository.DeleteAllRefreshTokens(int userId)
        {
            string query = "DELETE UserRefreshes WHERE UserId = @userId";
            using var connection = _dapperContext.CreateConnection();
            connection.Execute(query, new { userId });
        }

        void IAuthorizationRepository.DeleteRefreshToken(string refreshToken)
        {
            string query = "DELETE UserRefreshes WHERE Token = @refreshToken";
            using var connection = _dapperContext.CreateConnection();
            connection.Execute(query, new {refreshToken });
        }

        void IAuthorizationRepository.UpdateRefreshToken(string oldRefreshToken, TokenResult refreshToken, int userId)
        {
            string query = "UPDATE UserRefreshes SET Token = @token, ExpiresStart = @issuedAt , ExpiresEnd = @expiredAt   WHERE UserId = @userId AND Token = @oldRefreshToken";
            using var connection = _dapperContext.CreateConnection();
            connection.Execute(query, new { userId, refreshToken.token,refreshToken.expiredAt,refreshToken.issuedAt, oldRefreshToken });
        }
        public RefreshToken? GetRefreshToken(string refreshToken)
        {
            string query = "SELECT * FROM UserRefreshes WHERE Token = @refreshToken";
            using var connection = _dapperContext.CreateConnection();
            return connection.QuerySingleOrDefault<RefreshToken?>(query, new {refreshToken });
        }
        public DateTime? GetLastDateOfUserChanging(int userId)
        {
            using var connection = _dapperContext.CreateConnection();
            var sqlQuery = "SELECT LastChanged FROM Users WHERE Id = @userId";
            return connection.QuerySingle<DateTime?>(sqlQuery, new { userId });
        }
    }
}
