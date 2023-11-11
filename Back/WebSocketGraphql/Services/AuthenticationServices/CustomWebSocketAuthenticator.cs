using GraphQL.Server.Transports.AspNetCore.WebSockets;
using GraphQL.Transport;
using GraphQL;
using System.Security.Claims;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using GraphQL.Validation;

namespace WebSocketGraphql.Services.AuthenticationServices
{
    class CustomWebSocketAuthenticator : IWebSocketAuthenticationService
    {
        private readonly IGraphQLSerializer _serializer;
        private readonly IAuthorizationManager _authorizationManager;

        public CustomWebSocketAuthenticator(IGraphQLSerializer serializer, IAuthorizationManager authorizationManager)
        {
            _serializer = serializer;
            _authorizationManager = authorizationManager;
        }

        public async Task AuthenticateAsync(IWebSocketConnection connection, string subProtocol, OperationMessage operationMessage)
        {
            var payload = _serializer.ReadNode<Inputs>(operationMessage.Payload);
            var variables = _serializer.ReadNode<Variables>(operationMessage.Payload);
            if ((payload?.TryGetValue("authorization", out var token) ?? false) && token is string tokeString)
            {

                var chatId = variables?.FirstOrDefault(el => el.Name == "chatId")?.Value;
                if (await _authorizationManager.IsValidToken(tokeString, chatId is int id ? id: null ))
                {
                    var tokenData = _authorizationManager.ReadJwtToken(tokeString);
                    var principal = new ClaimsPrincipal(new ClaimsIdentity(tokenData.Claims, "Token"));
                    connection.HttpContext.User = principal;
                }
            }
        }
    }
}
