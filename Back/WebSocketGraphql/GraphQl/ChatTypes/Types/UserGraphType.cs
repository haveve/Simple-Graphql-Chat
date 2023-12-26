using GraphQL.Types;
using TimeTracker.Models;

namespace WebSocketGraphql.GraphQl.ChatTypes.Types
{
    public class UserGraphType : ObjectGraphType<User>
    {
        public UserGraphType()
        {
            Field(el => el.Id);
            Field(el => el.NickName);
            Field(el => el.Email);
            Field(el => el.Key2Auth,nullable:true);
        }
    }
}
