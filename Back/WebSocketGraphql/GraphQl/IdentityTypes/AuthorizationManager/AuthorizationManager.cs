using GraphQLParser;
using GraphQLParser.AST;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Security.Cryptography;
using TimeTracker.GraphQL.Types.IdentityTipes.Models;
using TimeTracker.Models;
using TimeTracker.Repositories;
using WebSocketGraphql.Services.AuthenticationServices;

namespace TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager
{
    public record class ValidateRefreshAndGetAccess(TokenResult accessToken, bool isValid, string? erroMessage);
    public record class TokenResult(string token, DateTime expiredAt, DateTime issuedAt);

    public class AuthorizationManager : IAuthorizationManager
    {
        public const int RefreshTokenExpiration = 31536000;
        public const int AccessTokenExpiration = 60;

        public readonly IAuthorizationRepository _authRepo;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        public AuthorizationManager(IUserRepository userRepository, IAuthorizationRepository authRepo, IConfiguration configuration)
        {
            _authRepo = authRepo;
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public TokenResult GetRefreshToken(int userId)
        {
            var expiredAt = DateTime.UtcNow.Add(TimeSpan.FromSeconds(IAuthorizationManager.RefreshTokenExpiration));
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

        public bool IsValidToken(string token)
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
                return true;
            }
            catch
            {
                return false;
            }
        }

        public JwtSecurityToken ReadJwtToken(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

        public ValidateRefreshAndGetAccess ValidateRefreshAndGetAccessToken(string refreshToken)
        {

            JwtSecurityToken objRefreshToken = ReadJwtToken(refreshToken);

            int userId = int.Parse(objRefreshToken.Claims.First(c => c.Type == "UserId").Value);
            bool isRefresh = bool.Parse(objRefreshToken.Claims.First(c => c.Type == "isRefresh").Value);
            var savedToken = _authRepo.GetRefreshToken(refreshToken);

            if (savedToken == null)
            {
                return new ValidateRefreshAndGetAccess(null, false, "Refresh token is invalid");
            }

            if (!IsValidToken(refreshToken))
            {
                return new ValidateRefreshAndGetAccess(null, false, "Refresh token is invalid");
            }

            if (!isRefresh)
            {
                return new ValidateRefreshAndGetAccess(null, false, "Refresh token is invalid");
            }

            return new ValidateRefreshAndGetAccess(GetAccessToken(userId), true, null);

        }

        public TokenResult GetAccessToken(int userId)
        {
            var expiredAt = DateTime.UtcNow.Add(TimeSpan.FromSeconds(IAuthorizationManager.AccessTokenExpiration));
            var issuedAt = DateTime.UtcNow;

            DateTimeOffset issuedAtOffset = issuedAt;

            var newAccessToken = new JwtSecurityToken(
    issuer: _configuration.GetIssuer(),
    audience: _configuration.GetAudience(),
    claims: new[] {
            new Claim("UserId", userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, issuedAtOffset.ToUnixTimeSeconds().ToString())
    },
    expires: expiredAt,
    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(_configuration.GetKey()), SecurityAlgorithms.HmacSha256));

            return new(new JwtSecurityTokenHandler().WriteToken(newAccessToken), expiredAt, issuedAt);

        }
    }
}
