using GraphQL.Types;
using GraphQLParser.AST;

namespace WebSocketGraphql.GraphQl.Directives.Validation.Directives
{
    public class EmailDirective : Directive
    {
        public EmailDirective()
            : base(ValidationDirectives.Email, DirectiveLocation.InputFieldDefinition, DirectiveLocation.ArgumentDefinition)
        {
            Description = "Used to specify email validation rules";
        }
    }
}
