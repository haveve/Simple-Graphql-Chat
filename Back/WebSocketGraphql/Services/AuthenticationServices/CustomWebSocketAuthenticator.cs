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

        public CustomWebSocketAuthenticator(IGraphQLSerializer serializer, IAuthorizationManager authorizationManager)
        {
            _serializer = serializer;
            _authorizationManager = authorizationManager;
        }

        public async Task AuthenticateAsync(IWebSocketConnection connection, string subProtocol, OperationMessage operationMessage)
        {
            var payload = _serializer.ReadNode<Inputs>(operationMessage.Payload);
            if ((payload?.TryGetValue("authorization", out var token) ?? false) && token is string tokeString)
            {

                int? chatId = null;
                if(payload?.TryGetValue("variables", out var variables)??false)
                {
                        var json = JsonConvert.SerializeObject(variables);
                        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        var chatData = dictionary?.FirstOrDefault(el => el.Key == "chatId");
                    try
                    {
                        chatId = chatData is null ? null : Convert.ToInt32(chatData.Value.Value);
                    }
                    catch
                    {
                        chatId = null;
                    }
                }

                if (await _authorizationManager.IsValidToken(tokeString, chatId))
                {
                    var tokenData = _authorizationManager.ReadJwtToken(tokeString);
                    var principal = new ClaimsPrincipal(new ClaimsIdentity(tokenData.Claims, "Token"));
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
