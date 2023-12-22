using GraphQL.Validation;
using GraphQL;
using GraphQLParser.AST;

namespace WebSocketGraphql.GraphQl.IdentityTypes.Errors
{
    public class ExtendedInvalidVariableError : ValidationError
    {
        private const string NUMBER = "5.8";

        /// <summary>
        /// Initializes a new instance with the specified properties.
        /// </summary>
        public ExtendedInvalidVariableError(ValidationContext context, ASTNode node,string fieldOrArgue ,string message)
            : base(context.Document.Source, NUMBER, $"'${fieldOrArgue}' is invalid. {message}", node)
        {
            Code = "INVALID_VALUE";
        }

    }

}