using GraphQL;
using GraphQL.Types;
using WebSocketGraphql.GraphQl.ChatTypes.Models;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.Repositories;

namespace WebSocketGraphql.GraphQl.ChatTypes
{
    public class ChatMutation : ObjectGraphType<object>
    {
        public ChatMutation(IChat chat)
        {
            Field<StringGraphType>("addMessage")
                .Argument<MessageInputType>("message")
                .Resolve(context =>
                {
                    var receivedMessage = context.GetArgument<ReceivedMessage>("message");
                    chat.AddMessage(receivedMessage);
                    return "OK";
                });
        }
    }

}
