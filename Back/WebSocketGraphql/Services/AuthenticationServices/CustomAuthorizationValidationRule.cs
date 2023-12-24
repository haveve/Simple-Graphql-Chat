using GraphQL;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Validation;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;

namespace WebSocketGraphql.Services.AuthenticationServices
{
    public class CustomAuthorizationValidationRule : IValidationRule
    {

        public virtual async ValueTask<INodeVisitor?> ValidateAsync(ValidationContext context)
        {

            var variables = context.Variables;
            var provider = context.RequestServices;
            var authService = provider!.GetService<IAuthorizationService>();
            var helper = provider!.GetService<AuthHelper>();
            var _authorizationManager = provider!.GetService<IAuthorizationManager>();

            if ((variables?.TryGetValue("authorization", out var token) ?? false) && token is string tokeString)
            {
                int? chatId = null;
                if ((variables?.TryGetValue("chatId", out var id) ?? false) && id is int intChatId)
                {
                    chatId = intChatId;
                }

                if (await _authorizationManager!.IsValidToken(tokeString, chatId))
                {
                    var tokenData = _authorizationManager.ReadJwtToken(tokeString);
                    if (helper.IsAccess(tokenData.Claims))
                    {
                        var identity = new ClaimsIdentity(tokenData.Claims, "Token");
                        var principal = new ClaimsPrincipal(identity);
                        var visitor = new AuthorizationVisitor(context, principal, authService);
                        foreach (var el in tokenData.Claims)
                        {
                            context.UserContext[el.Type] = el.Value;
                        }
                        return visitor;
                    }
                }
            }

            if (context.Schema.IsAuthorizationRequired())
                context.ReportError(new ValidationError("Access denied"));

            return null;
        }
    }
}
