using GraphQL.Types;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class MessageType : ObjectGraphType<Message>
    {
        public MessageType()
        {
            Field(el => el.FromId, nullable: false);
            Field(el => el.ChatId, nullable: false);
            Field(el => el.SentAt, nullable: false);
            Field(el => el.Content, nullable: false);
        }
    }
}
