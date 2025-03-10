using System.ComponentModel.DataAnnotations;

namespace WebSocketGraphql.Configurations
{
    public class EmailSettings
    {
        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string From { get; set; } = null!;

        [Required]
        public bool EnableSsl { get; set; }

        [Required]
        public string ServiceDomain { get; set; } = null!;

        [Required]
        public int Port { get; set; }
    }
}
