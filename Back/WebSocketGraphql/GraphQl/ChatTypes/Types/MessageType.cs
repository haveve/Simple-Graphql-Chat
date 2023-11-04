using GraphQL.Types;
using WebSocketGraphql.GraphQl.ChatTypes.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class MessageType : ObjectGraphType<Message>
    {
        public MessageType()
        {
            Field(o => o.Content);
            Field(o => o.SentAt);
            Field(o => o.From, false, typeof(MessageFromType));
        }
    }
}
