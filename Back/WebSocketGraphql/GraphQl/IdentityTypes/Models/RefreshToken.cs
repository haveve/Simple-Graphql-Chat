using System;
using System.ComponentModel;

namespace WebSocketGraphql.GraphQL.Types.IdentityTipes.Models
{
    public class RefreshToken
    {
        [Description("expiration_start")]
        public DateTime ExpiresStart { get; set; }
        [Description("expiration_end")]
        public DateTime ExpiresEnd { get; set; }
        [Description("token")]
        public string Token { get; set; } = null!;
    }
}
