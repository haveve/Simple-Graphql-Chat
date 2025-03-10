using System.ComponentModel;

namespace WebSocketGraphql.Models
{
    public class Message
    {
        [Description("sent_at")]
        public DateTime SentAt { get; set; }
        
        [Description("content")]
        public string Content { get; set; } = null!;

        [Description("from_id")]
        public int? FromId { get; set; }

        [Description("chat_id")]
        public int ChatId { get; set; }

        [Description("nick_name")]
        public string? NickName { get; set; }

        [Description("image")]
        public string? Image { get; set; }
    }
}
