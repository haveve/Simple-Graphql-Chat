﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
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

            if (_authorizationManager.IsValidToken(token))
            {
                var tokenData = _authorizationManager.ReadJwtToken(token);
                var principal = new ClaimsPrincipal(new ClaimsIdentity(tokenData.Claims, "Token"));
                return AuthenticateResult.Success(new AuthenticationTicket(principal, "CustomJwtBearer"));
            }

            return AuthenticateResult.Fail("Token validation failed.");
        }
    }

}