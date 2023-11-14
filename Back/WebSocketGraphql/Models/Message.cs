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
        public int FromId { get; set; } = 0;
        [Description("chat_id")]
        public int ChatId { get; set; } = 0;
        [Description("nick_name")]
        public string NickName { get; set; } = string.Empty;
     }
}
