using GraphQL.Types;
using GraphQLParser.AST;
using WebSocketGraphql.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class MessageGraphType : ObjectGraphType<Message>
    {
        public MessageGraphType()
        {
            Field(el => el.FromId, nullable: true);
            Field(el => el.ChatId, nullable: false);
            Field(el => el.SentAt, nullable: false, typeof(DateTimeGraphType));
            Field(el => el.Content, nullable: false);
            Field(el => el.NickName, nullable: true);
        }
    }
}
