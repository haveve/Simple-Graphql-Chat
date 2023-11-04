using GraphQL.Types;
using WebSocketGraphql.GraphQl.ChatTypes.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class MessageFromType : ObjectGraphType<MessageFrom>
    {
        public MessageFromType()
        {
            Field(o => o.Id);
            Field(o => o.DisplayName);
        }
    }
}
