using GraphQL;
using GraphQL.Types;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.Models;
using WebSocketGraphql.Repositories;

namespace WebSocketGraphql.GraphQl.ChatTypes
{
    public class ChatMutation : ObjectGraphType
    {
        public ChatMutation(IChat chat)
        {
            Field<BooleanGraphType>("addMessage")
                .Argument<MessageInputType>("message")
                .ResolveAsync(async context =>
                {
                    var receivedMessage = context.GetArgument<Message>("message");
                    return await chat.AddMessageAsync(receivedMessage);
                });
        }
    }
}
