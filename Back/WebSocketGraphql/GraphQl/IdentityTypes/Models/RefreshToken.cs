using System;

namespace TimeTracker.GraphQL.Types.IdentityTipes.Models
{
    public class RefreshToken
    {
        public DateTime ExpiresStart { get; set; }
        public DateTime ExpiresEnd { get; set; }
        public string Token { get; set; } = null!;
    }
}
