using GraphQL.Types;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class MessageInputGraphType : InputObjectGraphType<Message>
    {
        public const int maxLength = 200;
        public const int minLength = 1;

        public MessageInputGraphType()
        {
            Field(el => el.SentAt, nullable: false, typeof(DateTimeGraphType));
            Field(el => el.Content, nullable: false).Directive("length", "min", minLength, "max", maxLength);
        }
    }
}
