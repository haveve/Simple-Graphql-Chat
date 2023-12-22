using GraphQL.Types;
using GraphQLParser.AST;

namespace WebSocketGraphql.GraphQl.Directives.Validation
{
    public class EmailDirective : Directive
    {
        public EmailDirective()
            : base("email", DirectiveLocation.InputFieldDefinition, DirectiveLocation.ArgumentDefinition)
        {
            Description = "Used to specify email validation rules";
        }
    }
}
