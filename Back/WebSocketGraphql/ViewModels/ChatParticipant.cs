﻿using System.ComponentModel;

namespace WebSocketGraphql.ViewModels
{
    public class ChatParticipant
    {
        [Description("id")]
        public int Id { get; set; }
        [Description("nick_name")]
        public string NickName { get; set; } = null!;
        [Description("online")]
        public bool Online { get; set; }

        [Description("avatar")]
        public string? Avatar { get; set; }
    }
}
