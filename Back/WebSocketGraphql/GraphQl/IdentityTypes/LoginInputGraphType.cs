using GraphQL.Types;
using WebSocketGraphql.GraphQL.Types.IdentityTipes.Models;

namespace WebSocketGraphql.GraphQL.Types.IdentityTipes
{
    public class LoginInputGraphType : InputObjectGraphType<Login>
    {
        public LoginInputGraphType()
        {
            Field(l => l.NickNameOrEmail, nullable: false);
            Field(l => l.Password, nullable: false);
        }
    }
}
