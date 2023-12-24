using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using TimeTracker.GraphQL.Types.IdentityTipes.Models;

namespace TimeTracker.Repositories
{
    public interface IAuthorizationRepository
    {
        public void CreateRefreshToken(TokenResult refreshToken,int userId);
        public void UpdateRefreshToken(string oldRefreshToken, TokenResult refreshToken, int userId);
        public void DeleteRefreshToken(string refreshToken);
        public void DeleteAllRefreshTokens(int userId);
        public RefreshToken? GetRefreshToken(string refreshToken, int userId);
        public bool Add2factorKey(int userId, string key,string resetCode);
        public string? Get2factorKey(int userId);
        public bool Drop2factorKey(int userId, string? resetCode);
    }
}
