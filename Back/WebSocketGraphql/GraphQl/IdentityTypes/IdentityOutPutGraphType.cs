using GraphQL.Types;
using TimeTracker.GraphQL.Types.IdentityTipes.AuthorizationManager;
using TimeTracker.GraphQL.Types.IdentityTipes.Models;

namespace TimeTracker.GraphQL.Types.IdentityTipes
{
    public class IdentityOutPutGraphType : ObjectGraphType<LoginOutput>
    {
        public IdentityOutPutGraphType()
        {
            Field(l => l.access_token, nullable: false, type: typeof(TokenResultGraphType));
            Field(l => l.user_id, nullable: false);
            Field(l => l.refresh_token, nullable: false, type: typeof(TokenResultGraphType));
        }
    }
}
