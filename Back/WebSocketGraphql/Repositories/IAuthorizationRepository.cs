using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using TimeTracker.GraphQL.Types.IdentityTipes.Models;

namespace TimeTracker.Repositories
{
    public interface IAuthorizationRepository
    {
        public Task CreateRefreshTokenAsync(TokenResult refreshToken,int userId);
        public Task UpdateRefreshTokenAsync(string oldRefreshToken, TokenResult refreshToken, int userId);
        public Task DeleteRefreshTokenAsync(string refreshToken);
        public Task DeleteAllRefreshTokensAsync(int userId);
        public Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, int userId);
        public Task<bool> Add2factorKeyAsync(int userId, string key,string resetCode);
        public Task<string?> Get2factorKeyAsync(int userId);
        public Task<bool> Drop2factorKeyAsync(int userId, string? resetCode);
    }
}
