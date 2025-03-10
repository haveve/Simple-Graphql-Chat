using System.ComponentModel;

namespace WebSocketGraphql.Models
{
    public class User
    {
        [Description("id")]
        public int Id { get; set; }

        [Description("nick_name")]
        public string NickName { get; set; } = null!;

        [Description("password")]
        public string? Password { get; set; }

        [Description("email")]
        public string Email { get; set; } = null!;

        [Description("activate_code")]
        public string? ActivateCode { get; set; }

        [Description("salt")]
        public string? Salt { get; set; }

        [Description("key_2auth")]
        public string? Key2Auth { get; set; }

        [Description("reset_key_2auth")]
        public string? ResetCode { get; set; }

        [Description("avatar")]
        public string? Avatar { get; set; }
    }
}
