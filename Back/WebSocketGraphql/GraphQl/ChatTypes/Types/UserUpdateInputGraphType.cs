using GraphQL.Types;
using WebSocketGraphql.GraphQl.Directives.Validation;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.IdentityTypes;

public class UserUpdateInputGraphType : InputObjectGraphType<UpdateUser>
{
    public UserUpdateInputGraphType()
    {
        Field(l => l.Email, nullable: false)
            .RestrictLength(RegistrationInputGraphType.minEmailLength, RegistrationInputGraphType.maxEmailLength)
            .RestrictAsEmail();

        Field(l => l.NewPassword, nullable: false)
            .RestrictLength(IdentityMutation.minPasswordLength, IdentityMutation.maxPasswordLength);

        Field(l => l.OldPassword, nullable: false)
            .RestrictLength(IdentityMutation.minPasswordLength, IdentityMutation.maxPasswordLength);

        Field(l => l.NickName, nullable: false)
            .RestrictLength(RegistrationInputGraphType.minNickNameLength, RegistrationInputGraphType.maxNickNameLength);
    }
}