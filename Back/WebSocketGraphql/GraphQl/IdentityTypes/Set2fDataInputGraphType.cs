using GraphQL.Types;
using WebSocketGraphql.GraphQl.IdentityTypes.Models;

namespace WebSocketGraphql.GraphQl.IdentityTypes
{
    public class Set2fDataInputGraphType : InputObjectGraphType<Set2fData>
    {
        public Set2fDataInputGraphType()
        {
            Field(el => el.Code);
            Field(el => el.Key);
        }
    }
}