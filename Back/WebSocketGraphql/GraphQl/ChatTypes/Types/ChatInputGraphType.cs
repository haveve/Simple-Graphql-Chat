using GraphQL.Types;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class ChatInputGraphType : InputObjectGraphType<ChatModel>
    {
        public ChatInputGraphType()
        {
            Field(el => el.Name, nullable: false);
            Field(el => el.Id, nullable: false);
        }
    }
}
