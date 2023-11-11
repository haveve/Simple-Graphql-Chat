using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;

namespace WebSocketGraphql.Services.AuthenticationServices
{
    public class CustomJwtBearerHandler : JwtBearerHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthorizationManager _authorizationManager;

        public CustomJwtBearerHandler(IAuthorizationManager authorizationManager, IConfiguration configuration, IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
            _authorizationManager = authorizationManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return AuthenticateResult.Fail("Auhtorization through HTTP(S) was disabled");
        }
    }

}
