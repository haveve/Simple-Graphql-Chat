using GraphQL.Types;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class ExtendedChatGraphType : ObjectGraphType<ChatResult>
    {
        public ExtendedChatGraphType()
        { 
                Field(el => el.Name, nullable: false);
                Field(el => el.Id, nullable: false);
                Field(el => el.CreatorId, nullable: false);
                Field(el => el.ChatMembersCount, nullable: false);
        }
    }
}
