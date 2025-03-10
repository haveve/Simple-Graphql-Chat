using Google.Authenticator;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using WebSocketGraphql.GraphQL.Types.IdentityTipes.AuthorizationManager;
using WebSocketGraphql.Models;
using WebSocketGraphql.Services.AuthenticationServices;

namespace WebSocketGraphql.Helpers;

public static class _2fAuthHelper
{
    private const int tempTokenExpiration = 60 * 5;

    public static bool Has2fAuth(User user)
    {
        return user.Key2Auth != null;
    }

    public static bool Check2fAuth(string key, string code)
    {
        var tfa = new TwoFactorAuthenticator();
        return tfa.ValidateTwoFactorPIN(key, code);
    }

    public static string GetTemporaty2fAuthToken(User user, TokenResult refreshToken, string issuer, string audience, string key)
    {
        var expiredAt = DateTime.UtcNow.Add(TimeSpan.FromSeconds(tempTokenExpiration));

        var newAccessToken = new JwtSecurityToken(
            issuer,
            audience,
            claims: new[] {
                        new Claim("UserId", user.Id.ToString()),
                        new Claim("RefreshToken",refreshToken.token),
                        new Claim("IssuedAtRefresh",JsonSerializer.Serialize(refreshToken.issuedAt)),
                        new Claim("ExpiredAtRefresh",JsonSerializer.Serialize(refreshToken.expiredAt))

            },
            expires: expiredAt,
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(newAccessToken);
    }
}
