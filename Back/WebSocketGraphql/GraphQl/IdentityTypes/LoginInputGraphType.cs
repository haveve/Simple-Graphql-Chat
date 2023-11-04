using GraphQL.Types;
using TimeTracker.GraphQL.Types.IdentityTipes.Models;

namespace TimeTracker.GraphQL.Types.IdentityTipes
{
    public class LoginInputGraphType:InputObjectGraphType<Login>
    {
        public LoginInputGraphType() 
        {
            Field(l => l.NickNameOrEmail,nullable:false);
            Field(l => l.Password, nullable: false);
        }
    }
}
