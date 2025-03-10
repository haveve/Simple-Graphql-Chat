using System.ComponentModel.DataAnnotations;

namespace WebSocketGraphql.Configurations
{
    public class GeneralSettings
    {
        [Required]
        public int UserSubNumber { get; set; }

        [Required]
        public int MaxPictureSizeInKB { get; set; }

        [Required]
        public int SmallImageSize { get; set; }

        [Required]
        public string SmallImagePostfix { get; set; } = null!;
    }
}
