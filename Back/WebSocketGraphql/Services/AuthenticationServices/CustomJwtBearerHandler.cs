using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace WebSocketGraphql.Services.AuthenticationServices
{
    public class CustomJwtBearerHandler : JwtBearerHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthorizationManager _authorizationManager;
        private readonly AuthHelper _helper;

        public CustomJwtBearerHandler(AuthHelper helper,IAuthorizationManager authorizationManager, IConfiguration configuration, IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
            _authorizationManager = authorizationManager;
            _helper = helper;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValues))
            {
                return AuthenticateResult.Fail("Authorization header not found.");
            }

            var authorizationHeader = authorizationHeaderValues.FirstOrDefault();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return AuthenticateResult.Fail("Bearer token not found in Authorization header.");
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            if (await _authorizationManager.IsValidToken(token))
            {
                var user = _authorizationManager.ReadJwtToken(token);
                if (_helper.IsAccess(user.Claims))
                {
                    var principal = new ClaimsPrincipal(new ClaimsIdentity(_helper.GetImmutableClaims(user.Claims), "Token"));
                    return AuthenticateResult.Success(new AuthenticationTicket(principal, "CustomJwtBearer"));
                }
            }

            return AuthenticateResult.Fail("Token validation failed.");
        }
    }

}
