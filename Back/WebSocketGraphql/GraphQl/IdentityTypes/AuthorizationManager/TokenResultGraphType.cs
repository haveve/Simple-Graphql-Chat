using GraphQL.Types;

namespace WebSocketGraphql.GraphQL.Types.IdentityTipes.AuthorizationManager
{
    public class TokenResultGraphType:ObjectGraphType<TokenResult>
    {
        public TokenResultGraphType()
        {
            Field(tr => tr.issuedAt,nullable:false);
            Field(tr => tr.token, nullable: false);
            Field(tr => tr.expiredAt, nullable: false);
        }
    }
}
