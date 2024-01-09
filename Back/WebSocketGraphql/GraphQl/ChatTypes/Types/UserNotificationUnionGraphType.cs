using GraphQL.Types;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class UserNotificationUnionGraphType:UnionGraphType
    {
        public UserNotificationUnionGraphType()
        {
            Type<UserNortificationGraphType>();
            Type<ChatResulSubscriptionGraphType>();
        }
    }
}