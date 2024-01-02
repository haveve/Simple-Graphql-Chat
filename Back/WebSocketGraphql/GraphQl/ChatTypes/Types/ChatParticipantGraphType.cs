using GraphQL.Types;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class ChatParticipantGraphType : ObjectGraphType<ChatParticipant>
    {
        public ChatParticipantGraphType()
        {
            Field(el => el.Id, nullable: false);
            Field(el => el.NickName, nullable: false);
            Field(el => el.Online, nullable: false);
            Field(el => el.Avatar, nullable: true);
        }
    }
}
