using GraphQL.Types;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class ChatResulSubscriptionGraphType : ObjectGraphType<ChatSubscription>
    {
        public ChatResulSubscriptionGraphType()
        {
            Field(el => el.Id, nullable: false);
            Field(el => el.Name, nullable: false);
            Field(el => el.CreatorId, nullable: false);
            Field(el => el.ChatMembersCount, nullable: false);
            Field(el => el.Type, nullable: false);
            Field(el => el.UserId, nullable: false);
            Field(el => el.Avatar, nullable: true);
        }
    }
}
