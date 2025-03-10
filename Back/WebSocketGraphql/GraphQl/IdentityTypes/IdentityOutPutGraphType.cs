using GraphQL.Types;
using WebSocketGraphql.GraphQL.Types.IdentityTipes.AuthorizationManager;
using WebSocketGraphql.GraphQL.Types.IdentityTipes.Models;

namespace WebSocketGraphql.GraphQL.Types.IdentityTipes;

public class IdentityOutPutGraphType : ObjectGraphType<LoginOutput>
{
    public IdentityOutPutGraphType()
    {
        Field(l => l.access_token, nullable: false, type: typeof(TokenResultGraphType));
        Field(l => l.user_id, nullable: false);
        Field(l => l.refresh_token, nullable: false, type: typeof(TokenResultGraphType));
        Field(l => l.redirect_url, nullable: true);
    }
}
