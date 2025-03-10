using System.ComponentModel;

namespace WebSocketGraphql.Models
{
    public class ChatModel
    {
        [Description("id")]
        public int Id { get; set; }

        [Description("name")]
        public string Name { get; set; } = string.Empty;

        [Description("creator")]
        public int CreatorId { get; set; }

        [Description("avatar")]
        public string? Avatar { get; set; }
    }
}
