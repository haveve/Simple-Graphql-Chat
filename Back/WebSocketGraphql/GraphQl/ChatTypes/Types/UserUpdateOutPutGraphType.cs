using GraphQL.Types;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.GraphQl.IdentityTypes
{
    public class UserUpdateOutPutGraphType : ObjectGraphType<UpdateUser>
    {
        public UserUpdateOutPutGraphType()
        {
            Field(l => l.Id);
            Field(l => l.Email, nullable: false);
            Field(l => l.NickName, nullable: false);
        }
    }
}