using GraphQL.Types;
using TimeTracker.GraphQL.Queries;

namespace TimeTracker.GraphQL.Schemas
{
        public class IdentitySchema : Schema
        {
            public IdentitySchema(IServiceProvider provider) : base(provider)
            {
                Query = provider.GetRequiredService<IdentityQuery>();
            }
        }
}
