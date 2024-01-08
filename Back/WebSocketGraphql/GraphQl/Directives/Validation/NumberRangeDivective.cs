using GraphQL.Types;
using GraphQLParser.AST;

namespace WebSocketGraphql.GraphQl.Directives.Validation
{
    public class NumberRangeDivective : Directive
    {
        public override bool? Introspectable => true;
        public NumberRangeDivective()
            : base("constraint_number", DirectiveLocation.InputFieldDefinition, DirectiveLocation.ArgumentDefinition)
        {
            base.Description = "Used to specify the minimum and/or maximum value of number for an input field or argument.";
            base.Arguments = new QueryArguments(new QueryArgument<IntGraphType>
            {
                Name = "min",
                Description = "If specified, specifies the minimum number that the input field or argument must have."
            }, new QueryArgument<IntGraphType>
            {
                Name = "max",
                Description = "If specified, specifies the maximum number that the input field or argument must have."
            });
        }

        public override void Validate(AppliedDirective applied)
        {
            object? obj = applied.FindArgument("min")?.Value;
            object? obj2 = applied.FindArgument("max")?.Value;
            if (obj == null && obj2 == null)
            {
                throw new ArgumentException("Either 'min' or 'max' argument must be specified for @constraint_number directive.");
            }

            if (obj != null && obj2 != null && (int)obj > (int)obj2)
            {
                throw new ArgumentOutOfRangeException($"Argument 'max' must be equal or greater than 'min' argument for @constraint_number directive; min={obj}, max={obj2}");
            }
        }
    }
}