using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager
{
    public interface IAuthorizationManager
    {
        public const int RefreshTokenExpiration = 31536000;
        public const int AccessTokenExpiration = 60;

        public TokenResult GetRefreshToken(int userId);
        public JwtSecurityToken ReadJwtToken(string accessToken);
        public Task<ValidateRefreshAndGetAccess> ValidateRefreshToken(string refreshToken);
        public bool IsValidToken(string token, int? chatId = null, bool refresh = false);
        public Task<TokenResult> GetAccessToken(int userId);
    }
}
