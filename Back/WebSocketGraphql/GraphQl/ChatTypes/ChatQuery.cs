using GraphQL.Types;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.Repositories;

namespace WebSocketGraphql.GraphQl.ChatTypes
{
    public class ChatQuery : ObjectGraphType
    {
        public ChatQuery(IChat chat)
        {
            Field<ListGraphType<MessageType>>("messages").Resolve(_ => chat.AllMessages);
        }
    }

}
