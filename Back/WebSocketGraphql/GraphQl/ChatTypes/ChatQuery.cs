using GraphQL;
using GraphQL.Types;
using WebSocketGraphql.GraphQl.ChatTypes.Types;
using WebSocketGraphql.Repositories;

namespace WebSocketGraphql.GraphQl.ChatTypes
{
    public class ChatQuery : ObjectGraphType
    {
        public ChatQuery(IChat chat)
        {
            Field<ListGraphType<MessageType>>("messages")
                .Argument<NonNullGraphType<IntGraphType>>("chatId")
                .ResolveAsync(async context =>
                {
                    int chatId = context.GetArgument<int>("chatId");
                    return await chat.GetAllMessages(chatId);
                });
        }
    }

}
