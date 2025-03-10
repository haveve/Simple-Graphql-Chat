using GraphQL.Server.Transports.AspNetCore.WebSockets;
using GraphQL.Transport;
using GraphQL;
using System.Security.Claims;
using WebSocketGraphql.GraphQL.Types.IdentityTipes.AuthorizationManager;
using System.ComponentModel;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.Services.AuthenticationServices
{
    class CustomWebSocketAuthenticator : IWebSocketAuthenticationService
    {
        private readonly IGraphQLSerializer _serializer;
        private readonly IAuthorizationManager _authorizationManager;
        private readonly AuthHelper _authHelper;

        public CustomWebSocketAuthenticator(ILogger<CustomWebSocketAuthenticator> logger, AuthHelper authHelper, IGraphQLSerializer serializer, IAuthorizationManager authorizationManager)
        {
            _serializer = serializer;
            _authorizationManager = authorizationManager;
            _authHelper = authHelper;
        }

        public Task AuthenticateAsync(IWebSocketConnection connection, string subProtocol, OperationMessage operationMessage)
        {
            var payload = _serializer.ReadNode<Inputs>(operationMessage.Payload);
            if ((payload?.TryGetValue("authorization", out var token) ?? false) && token is string tokeString)
            {
                if (_authorizationManager.IsValidToken(tokeString, null))
                {
                    var tokenData = _authorizationManager.ReadJwtToken(tokeString);
                    if (_authHelper.IsAccess(tokenData.Claims))
                    {
                        var principal = new ClaimsPrincipal(new ClaimsIdentity(_authHelper.GetImmutableClaims(tokenData.Claims), "Token"));
                        connection.HttpContext.User = principal;
                    }
                }
            }
            return Task.CompletedTask;
        }

    }
}
