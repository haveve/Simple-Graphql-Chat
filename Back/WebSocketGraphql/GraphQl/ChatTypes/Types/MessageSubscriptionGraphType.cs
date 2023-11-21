using GraphQL.Types;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class MessageSubscriptionGraphType:ObjectGraphType<MessageSubscription>
    {
        public MessageSubscriptionGraphType() 
        {
            Field(el => el.FromId, nullable: true);
            Field(el => el.ChatId, nullable: false);
            Field(el => el.SentAt, nullable: false, typeof(DateTimeGraphType));
            Field(el => el.Content, nullable: false);
            Field(el => el.Type, nullable: false);
            Field(el => el.NickName, nullable: true);
            Field(el => el.DeleteAll, nullable: true);
        }
    }
}
