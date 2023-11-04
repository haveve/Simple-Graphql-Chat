namespace WebSocketGraphql.GraphQl.ChatTypes.Models
{
    public class ReceivedMessage
    {
        public string FromId { get; set; }

        public string Content { get; set; }

        public DateTime SentAt { get; set; }
    }
}
