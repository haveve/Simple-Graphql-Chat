using System.IdentityModel.Tokens.Jwt;

namespace WebSocketGraphql.GraphQL.Types.IdentityTipes.AuthorizationManager
{
    public interface IAuthorizationManager
    {
        public TokenResult GetRefreshToken(int userId);
        public JwtSecurityToken ReadJwtToken(string accessToken);
        public Task<ValidateRefreshAndGetAccess> ValidateRefreshToken(string refreshToken);
        public bool IsValidToken(string token, int? chatId = null, bool refresh = false);
        public Task<TokenResult> GetAccessToken(int userId);
    }
}
