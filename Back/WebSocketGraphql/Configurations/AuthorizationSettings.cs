using System.ComponentModel.DataAnnotations;

namespace WebSocketGraphql.Configurations
{
    public class AuthorizationSettings
    {
        [Required]
        public string Key { get; set; } = null!;

        [Required]
        public string Audience { get; set; } = null!;

        [Required]
        public string Issuer { get; set; } = null!;
    }
}
