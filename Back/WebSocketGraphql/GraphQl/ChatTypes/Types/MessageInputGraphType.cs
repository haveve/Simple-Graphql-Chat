using GraphQL.Types;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class MessageInputGraphType : InputObjectGraphType<Message>
    {
        public MessageInputGraphType()
        {
            Field(el => el.SentAt, nullable: false, typeof(DateTimeGraphType));
            Field(el => el.Content, nullable: false);
        }
    }
}
