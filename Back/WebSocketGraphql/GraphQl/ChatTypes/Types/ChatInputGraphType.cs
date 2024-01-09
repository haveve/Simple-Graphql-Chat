using GraphQL.Types;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class ChatInputGraphType : InputObjectGraphType<ChatModel>
    {
        public const int minChatNameLength = 1;
        public const int maxChatNameLength = 100;

        public ChatInputGraphType()
        {
            Field(el => el.Name, nullable: false).Directive("length", "min", minChatNameLength, "max", maxChatNameLength); ;
            Field(el => el.Id, nullable: false);
        }
    }
}
