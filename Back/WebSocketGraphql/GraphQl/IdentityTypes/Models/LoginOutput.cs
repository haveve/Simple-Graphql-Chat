using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using TimeTracker.Models;

namespace TimeTracker.GraphQL.Types.IdentityTipes.Models
{
    public class LoginOutput
    {
        public int user_id { get; set; }
        public TokenResult access_token { get; set; } = null!;
        public TokenResult refresh_token { get; set; } = null!;
        public string? redirect_url { get; set; } 
    }
}
