using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebSocketGraphql.GraphQL.Types.IdentityTipes.Models
{
    public class Login
    {
        public string NickNameOrEmail { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
