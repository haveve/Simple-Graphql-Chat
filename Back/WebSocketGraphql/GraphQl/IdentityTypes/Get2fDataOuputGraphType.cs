using GraphQL.Types;
using WebSocketGraphql.GraphQl.IdentityTypes.Models;

namespace WebSocketGraphql.GraphQl.IdentityTypes
{
    public class Get2fDataOuputGraphType : ObjectGraphType<Get2fData>
    {
        public Get2fDataOuputGraphType()
        {
            Field(el => el.Key);
            Field(el => el.ManualEntry);
            Field(el => el.QrUrl);
        }
    }
}