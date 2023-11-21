using GraphQL.Server.Transports.AspNetCore.WebSockets;
using GraphQL.Transport;
using GraphQL;
using System.Security.Claims;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using GraphQL.Validation;
using System.Text.Json;
using Newtonsoft.Json;
using System.ComponentModel;

namespace WebSocketGraphql.Services.AuthenticationServices
{
    class CustomWebSocketAuthenticator : IWebSocketAuthenticationService
    {
        private readonly IGraphQLSerializer _serializer;
        private readonly IAuthorizationManager _authorizationManager;
        private readonly AuthHelper _authHelper;
        private readonly ILogger<CustomWebSocketAuthenticator> _logger;

        public CustomWebSocketAuthenticator(ILogger<CustomWebSocketAuthenticator> logger,AuthHelper authHelper,IGraphQLSerializer serializer, IAuthorizationManager authorizationManager)
        {
            _serializer = serializer;
            _authorizationManager = authorizationManager;
            _authHelper = authHelper;
            _logger = logger;
        }
        
        public async Task AuthenticateAsync(IWebSocketConnection connection, string subProtocol, OperationMessage operationMessage)
        {
            var payload = _serializer.ReadNode<Inputs>(operationMessage.Payload);
            _logger.LogCritical(operationMessage.Type);
            if ((payload?.TryGetValue("authorization", out var token) ?? false) && token is string tokeString)
            {
                if (await _authorizationManager.IsValidToken(tokeString, null))
                {
                    var tokenData = _authorizationManager.ReadJwtToken(tokeString);
                    var principal = new ClaimsPrincipal(new ClaimsIdentity(_authHelper.GetImmutableClaims(tokenData.Claims), "Token"));
                    connection.HttpContext.User = principal;
                }
            }
        }
        
    }
    public static class ObjectToDictionaryHelper
    {
        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null) ThrowExceptionWhenSourceArgumentIsNull();

            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                object value = property.GetValue(source);
                if (IsOfType<T>(value))
                {
                    dictionary.Add(property.Name, (T)value);
                }
            }
            return dictionary;
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new NullReferenceException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
        }
    }
}
