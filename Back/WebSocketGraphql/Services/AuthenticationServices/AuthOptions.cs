using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebSocketGraphql.Services.AuthenticationServices
{
    public static class AuthOptions
    {
        public static SymmetricSecurityKey GetSymmetricSecurityKey(string KEY) =>
            new (Encoding.UTF8.GetBytes(KEY));

        public static string GetKey(this IConfiguration configuration)
        {
            return configuration["Authorization:Key"];
        }

        public static string GetIssuer(this IConfiguration configuration)
        {
            return configuration["Authorization:Issuer"];
        }

        public static string GetAudience(this IConfiguration configuration)
        {
            return configuration["Authorization:Audience"];
        }
    }
}
