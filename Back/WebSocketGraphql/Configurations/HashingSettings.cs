using System.ComponentModel.DataAnnotations;

namespace WebSocketGraphql.Configurations
{
    public class HashingSettings
    {
        [Required]
        public int Iteration { get; set; }
    }
}
