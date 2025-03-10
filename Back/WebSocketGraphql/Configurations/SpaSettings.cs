using System.ComponentModel.DataAnnotations;

namespace WebSocketGraphql.Configurations
{
    public record SpaSettings
    {
        [Required]
        public string Url { get; set; } = null!;
    }
}
