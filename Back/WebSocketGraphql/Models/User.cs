using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TimeTracker.Repositories;

namespace TimeTracker.Models
{
    public class User
    {
        [Description("id")]
        public int Id { get; set; }
        [Description("nick_name")]
        public string NickName { get; set; }
        [Description("password")]
        public string Password { get; set; } = null;
        [Description("email")]
        public string Email { get; set; }
        [Description("activate_code")]
        public string ActivateCode { get; set; } = null;
        [Description("salt")]
        public string Salt { get; set; } = null;
        [Description("key_2auth")]
        public string? Key2Auth {  get; set; }
        [Description("reset_key_2auth")]
        public string? ResetCode { get; set; }

    }
}
