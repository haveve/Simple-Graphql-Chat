using GraphQL.Types;
using WebSocketGraphql.GraphQl.IdentityTypes.Models;

namespace WebSocketGraphql.GraphQl.IdentityTypes
{
    public class RegistrationInputGraphType : InputObjectGraphType<Registration>
    {
        public RegistrationInputGraphType()
        {
            Field(el => el.NickName, nullable:false);
            Field(el => el.Email, nullable: false);
        }
    }
}
