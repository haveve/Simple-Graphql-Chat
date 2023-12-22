using GraphQL.Types;
using TimeTracker.GraphQL.Queries;
using WebSocketGraphql.GraphQl.Directives.Validation;
using WebSocketGraphql.GraphQl.IdentityTypes;

namespace TimeTracker.GraphQL.Schemas
{
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
}
