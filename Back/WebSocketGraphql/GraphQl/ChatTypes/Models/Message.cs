namespace WebSocketGraphql.GraphQl.ChatTypes.Models
{

    public class Message
    {
        public MessageFrom From { get; set; }

        public string Content { get; set; }

        public DateTime SentAt { get; set; }
    }

}
