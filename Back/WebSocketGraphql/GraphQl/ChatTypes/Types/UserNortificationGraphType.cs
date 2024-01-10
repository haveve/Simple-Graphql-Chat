using GraphQL.Types;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class UserNortificationGraphType : ObjectGraphType<UserNotification>
    {
        public UserNortificationGraphType()
        {
            Field(el => el.Id);
            Field(el => el.NotificationType);
            Field(el => el.Name);
            Field(el => el.CreatorId);
            Field(el => el.UserId);
            Field(el => el.Avatar, nullable: true);
        }
    }
}
