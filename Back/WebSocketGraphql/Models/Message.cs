﻿using System.ComponentModel;

namespace WebSocketGraphql.Models
{
    public class Message
    {
        [Description("sent_at")]
        public DateTime SentAt { get; set; }
        [Description("content")]
        public string Content { get; set; } = null!;
        [Description("from_id")]
        public int? FromId { get; set; } = null;
        [Description("chat_id")]
        public int ChatId { get; set; } = 0;
        [Description("nick_name")]
        public string? NickName { get; set; } = null;
        [Description("image")]
        public string? Image { get; set; } = null;
    }
}
