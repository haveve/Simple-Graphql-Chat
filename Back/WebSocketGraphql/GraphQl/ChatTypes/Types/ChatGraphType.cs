using GraphQL.Types;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class ChatGraphType : ObjectGraphType<ChatModel>
    {
        public ChatGraphType()
        {
            Field(el => el.Name, nullable: false);
            Field(el => el.Id, nullable: false);
            Field(el => el.CreatorId, nullable: false);
        }
    }
}
