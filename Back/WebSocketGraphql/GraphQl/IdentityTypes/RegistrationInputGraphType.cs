using GraphQL.Types;
using WebSocketGraphql.GraphQl.IdentityTypes.Models;

namespace WebSocketGraphql.GraphQl.IdentityTypes
{
    public class RegistrationInputGraphType : InputObjectGraphType<Registration>
    {
        public const int maxNickNameLength = 75;
        public const int minNickNameLength = 3;

        public const int maxEmailLength = 100;
        public const int minEmailLength = 5;


        public RegistrationInputGraphType()
        {
            Field(el => el.NickName, nullable:false).Directive("length", "min", minNickNameLength, "max", maxNickNameLength);
            Field(el => el.Email, nullable: false)
                .Directive("length", "min", minEmailLength, "max", maxEmailLength)
                .Directive("email");
        }
    }
}
