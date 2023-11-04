using GraphQL.Types;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class MessageInputType : InputObjectGraphType
    {
        public MessageInputType()
        {
            Field<StringGraphType>("fromId");
            Field<StringGraphType>("content");
            Field<DateTimeGraphType>("sentAt");
        }
    }
}
