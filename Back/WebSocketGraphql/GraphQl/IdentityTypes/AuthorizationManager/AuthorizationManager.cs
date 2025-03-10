using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using WebSocketGraphql.Repositories;
using WebSocketGraphql.Services.AuthenticationServices;

namespace WebSocketGraphql.GraphQL.Types.IdentityTipes.AuthorizationManager
{
    public record class ValidateRefreshAndGetAccess(TokenResult? accessToken, bool isValid, string? erroMessage);
    public record class TokenResult(string token, DateTime expiredAt, DateTime issuedAt);

    public class AuthorizationManager : IAuthorizationManager
    {
        private const int refreshTokenExpiration = 31536000;
        private const int accessTokenExpiration = 60;

        private readonly IAuthorizationRepository _authRepo;
        private readonly IConfiguration _configuration;
        private readonly IChat _chat;
        private readonly IUserRepository _userRepo;

        public AuthorizationManager(IUserRepository userRepo, IAuthorizationRepository authRepo, IConfiguration configuration, IChat chat, AuthHelper authHelper)
        {
            _authRepo = authRepo;
            _configuration = configuration;
            _chat = chat;
            _userRepo = userRepo;

        }

        public TokenResult GetRefreshToken(int userId)
        {
            var expiredAt = DateTime.UtcNow.Add(TimeSpan.FromSeconds(refreshTokenExpiration));
            var issuedAt = DateTime.UtcNow;

            DateTimeOffset issuedAtOffset = issuedAt;

            var refreshToken = new JwtSecurityToken(
                issuer: _configuration.GetIssuer(),
                audience: _configuration.GetAudience(),
                claims: new[] {
                        new Claim("UserId", userId.ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, issuedAtOffset.ToUnixTimeSeconds().ToString()),
                        new Claim("isRefresh",true.ToString())
                },
                expires: expiredAt,
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(_configuration.GetKey()), SecurityAlgorithms.HmacSha256));

            return new(new JwtSecurityTokenHandler().WriteToken(refreshToken), expiredAt, issuedAt);
        }

        public bool IsValidToken(string token, int? chatId = null, bool refresh = false)
        {
            try
            {
                var tokenValidate = new JwtSecurityTokenHandler();

                tokenValidate.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _configuration.GetIssuer(),
                    ValidateAudience = true,
                    ValidAudience = _configuration.GetAudience(),
                    ValidateLifetime = true,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(_configuration.GetKey()),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken securityToken);

                if (refresh)
                {
                    return true;
                }

                var tokenData = ReadJwtToken(token);
                var userId = Convert.ToInt32(tokenData.Claims.First(c => c.Type == "UserId").Value);
                var participatedChats = JsonSerializer.Deserialize<IEnumerable<int>>(tokenData.Claims.First(c => c.Type == "UserParticipated").Value);
                var ownChats = JsonSerializer.Deserialize<IEnumerable<int>>(tokenData.Claims.First(c => c.Type == "UserOwn").Value);

                if (chatId is null)
                {
                    return true;
                }

                if (participatedChats!.Any(id => id == chatId) || ownChats!.Any(id => id == chatId))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public JwtSecurityToken ReadJwtToken(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

        public async Task<ValidateRefreshAndGetAccess> ValidateRefreshToken(string refreshToken)
        {

            JwtSecurityToken objRefreshToken = ReadJwtToken(refreshToken);

            int userId = int.Parse(objRefreshToken.Claims.First(c => c.Type == "UserId").Value);
            bool isRefresh = bool.Parse(objRefreshToken.Claims.First(c => c.Type == "isRefresh").Value);
            var savedToken = await _authRepo.GetRefreshTokenAsync(refreshToken, userId);

            if (savedToken == null)
            {
                return new ValidateRefreshAndGetAccess(null, false, "Refresh token is invalid");
            }

            if (!IsValidToken(refreshToken, refresh: isRefresh))
            {
                return new ValidateRefreshAndGetAccess(null, false, "Refresh token is invalid");
            }

            if (!isRefresh)
            {
                return new ValidateRefreshAndGetAccess(null, false, "Refresh token is invalid");
            }

            return new ValidateRefreshAndGetAccess(await GetAccessToken(userId), true, null);

        }

        public async Task<TokenResult> GetAccessToken(int userId)
        {
            var expiredAt = DateTime.UtcNow.Add(TimeSpan.FromSeconds(accessTokenExpiration));
            var issuedAt = DateTime.UtcNow;

            var participatedChats = await _chat.GetUserChatsAsync(userId);
            var ownChats = await _chat.GetUserCreationChatsAsync(userId);
            var user = await _userRepo.GetUserAsync(userId);

            DateTimeOffset issuedAtOffset = issuedAt;

            var newAccessToken = new JwtSecurityToken(
                issuer: _configuration.GetIssuer(),
                audience: _configuration.GetAudience(),
                claims: new[] {
                        new Claim("UserId", userId.ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, issuedAtOffset.ToUnixTimeSeconds().ToString()),
                        new Claim("UserOwn",JsonSerializer.Serialize(ownChats)),
                        new Claim("UserParticipated",JsonSerializer.Serialize(participatedChats)),
                        new Claim("UserNickName",user!.NickName),
                        new Claim("IsAccess",true.ToString())
                },
                expires: expiredAt,
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(_configuration.GetKey()), SecurityAlgorithms.HmacSha256));

            return new(new JwtSecurityTokenHandler().WriteToken(newAccessToken), expiredAt, issuedAt);

        }
    }
}
