using System.ComponentModel;

namespace WebSocketGraphql.Models
{
    public class Message
    {
        [Description("sent_at")]
        public DateTime SentAt { get; set; }
        [Description("content")]
        public string Content { get; set; }
        [Description("from_id")]
        public int FromId { get; set; }
        [Description("chat_id")]
        public int ChatId { get; set; }
     }
}
