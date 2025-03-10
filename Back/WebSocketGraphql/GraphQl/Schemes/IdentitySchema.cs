using GraphQL.Types;
using WebSocketGraphql.GraphQL.Queries;
using WebSocketGraphql.GraphQl.Directives.Validation.Directives;
using WebSocketGraphql.GraphQl.IdentityTypes;

namespace WebSocketGraphql.GraphQL.Schemas;

public class IdentitySchema : Schema
{
    public IdentitySchema(IServiceProvider provider) : base(provider)
    {
        Directives.Register(new LengthDirective(),
                            new EmailDirective());

        Query = provider.GetRequiredService<IdentityQuery>();
        Mutation = provider.GetRequiredService<IdentityMutation>();
    }
}