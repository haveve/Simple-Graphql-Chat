using GraphQL;
using GraphQL.Types;
using WebSocketGraphql.GraphQl.ChatTypes;

namespace WebSocketGraphql.GraphQl.Schemes
{
    public class ChatSchema : Schema
    {
        public ChatSchema(IServiceProvider service) : base(service)
        {
            Metadata.Add(AuthorizationExtensions.AUTHORIZE_KEY, true);

            Query = service.GetRequiredService<ChatQuery>();
            Mutation = service.GetRequiredService<ChatMutation>();
            Subscription = service.GetRequiredService<ChatSubscriptions>();
        }
    }
}
