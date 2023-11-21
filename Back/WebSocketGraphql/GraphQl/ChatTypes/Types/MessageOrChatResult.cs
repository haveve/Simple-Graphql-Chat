using GraphQL.Types;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class MessageOrChatResult:UnionGraphType
    {
        public MessageOrChatResult()
        {
            Type<MessageSubscriptionGraphType>();
            Type<ChatResulSubscriptionGraphType>();
            Type<ChatParticipantSubscriptionGraphType>();
        }
    }
}
