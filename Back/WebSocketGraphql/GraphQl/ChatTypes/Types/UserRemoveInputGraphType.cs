using GraphQL.Types;
using TimeTracker.GraphQL.Types.IdentityTipes.Models;

namespace WebSocketGraphql.GraphQl.IdentityTypes
{
    public class UserRemoveInputGraphType : InputObjectGraphType<RemoveUser>
    {
        public UserRemoveInputGraphType()
        {
            Field(l => l.Password, nullable: false)
            .Directive("length", "min", RegistrationInputGraphType.minEmailLength, "max", RegistrationInputGraphType.maxEmailLength);
        }
    }
}