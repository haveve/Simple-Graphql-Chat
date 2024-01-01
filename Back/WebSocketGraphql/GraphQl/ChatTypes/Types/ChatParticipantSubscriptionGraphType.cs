using GraphQL.Types;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class ChatParticipantSubscriptionGraphType : ObjectGraphType<ChatParticipant>
    {
        public ChatParticipantSubscriptionGraphType()
        {
            Field(el => el.Id, nullable: false);
            Field(el => el.Online, nullable: false);
            Field(el => el.NickName, nullable: true);
        }
    }
}
