using GraphQL.Types;
using WebSocketGraphql.GraphQl.Directives.Validation;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.IdentityTypes;

public class UserRemoveInputGraphType : InputObjectGraphType<RemoveUser>
{
    public UserRemoveInputGraphType()
    {
        Field(l => l.Password, nullable: false).RestrictLength(RegistrationInputGraphType.minEmailLength, RegistrationInputGraphType.maxEmailLength);
    }
}