using GraphQL.Types;
using TimeTracker.GraphQL.Types.IdentityTipes.Models;

namespace WebSocketGraphql.GraphQl.IdentityTypes
{
    public class UserUpdateInputGraphType : InputObjectGraphType<UpdateUser>
    {
        public UserUpdateInputGraphType()
        {

            Field(l => l.Email, nullable: false)
            .Directive("length", "min", RegistrationInputGraphType.minEmailLength, "max", RegistrationInputGraphType.maxEmailLength)
            .Directive("email");
            Field(l => l.NewPassword, nullable: false)
            .Directive("length", "min", RegistrationInputGraphType.minEmailLength, "max", RegistrationInputGraphType.maxEmailLength);
            Field(l => l.OldPassword, nullable: false)
            .Directive("length", "min", RegistrationInputGraphType.minEmailLength, "max", RegistrationInputGraphType.maxEmailLength);
            Field(l => l.NickName, nullable: false)
            .Directive("length", "min", RegistrationInputGraphType.minNickNameLength, "max", RegistrationInputGraphType.maxNickNameLength);
        }
    }
}