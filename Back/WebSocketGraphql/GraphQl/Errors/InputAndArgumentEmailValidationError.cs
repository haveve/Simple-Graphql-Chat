using GraphQL.Validation;
using GraphQL;
using GraphQLParser.AST;

namespace WebSocketGraphql.GraphQl.IdentityTypes.Errors
{
    public class InputAndArgumentEmailValidationError : ValidationError
    {
        private const string NUMBER = "5.6.1";

        public InputAndArgumentEmailValidationError(ValidationContext context, ASTNode node, string fieldOrArgue, string message)
            : base(context.Document.Source, NUMBER, $"'${fieldOrArgue}' is invalid. {message}", node)
        {
            Code = "INVALID_VALUE";
        }

        public InputAndArgumentEmailValidationError(ValidationContext context, GraphQLVariableDefinition node, VariableName variableName, string message)
    : base(context.Document.Source, NUMBER, $"Variable '${variableName}' is invalid. {message}", node)
        {
        }

    }

}