using GraphQL.Types;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class ChatResulSubscriptionGraphType:ObjectGraphType<ChatSubscription>
    {
        public ChatResulSubscriptionGraphType()
        {
            Field(el => el.Id, nullable: false);
            Field(el => el.Name, nullable: true);  
            Field(el => el.CreatorId, nullable: true);
            Field(el => el.ChatMembersCount, nullable: true);
            Field(el => el.Type, nullable: false);
        }
    }
}
