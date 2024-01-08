using GraphQL.Types;
using GraphQL;
using GraphQL.Validation;
using GraphQLParser.AST;
using GraphQL.Validation.Errors;

namespace WebSocketGraphql.GraphQl.ValidationRules
{
    public class InputAndArgumentNumberValidationRule : IValidationRule, IVariableVisitorProvider
    {
        private sealed class FieldVisitor : BaseVariableVisitor
        {
            public static readonly FieldVisitor Instance = new FieldVisitor();

            public override ValueTask VisitFieldAsync(ValidationContext context, GraphQLVariableDefinition variable, VariableName variableName, IInputObjectGraphType type, FieldType field, object? variableValue, object? parsedValue)
            {
                AppliedDirective? appliedDirective = field.FindAppliedDirective("constraint_number");
                if (appliedDirective == null)
                {
                    return default(ValueTask);
                }

                object? obj = appliedDirective.FindArgument("min")?.Value;
                object? obj2 = appliedDirective.FindArgument("max")?.Value;
                if (parsedValue == null)
                {
                    if (obj != null || obj2 != null)
                    {
                        context.ReportError(new InputAndArgumentNumberValidationError(context, variable, variableName, null, (int?)obj, (int?)obj2));
                    }
                }
                else if (parsedValue is int number && IsValidNumber(number, (int?)obj, (int?)obj2))
                {
                    context.ReportError(new InputAndArgumentNumberValidationError(context, variable, variableName, number, (int?)obj, (int?)obj2));
                }

                return default(ValueTask);
            }
        }

        public static readonly InputAndArgumentNumberValidationRule Instance = new InputAndArgumentNumberValidationRule();

        private static readonly INodeVisitor _nodeVisitor = new NodeVisitors(new MatchingNodeVisitor<GraphQLArgument>(delegate (GraphQLArgument arg, ValidationContext context)
        {
            CheckLength(arg, arg.Value, context.TypeInfo.GetArgument(), context);
        }), new MatchingNodeVisitor<GraphQLObjectField>(delegate (GraphQLObjectField field, ValidationContext context)
        {
            if (context.TypeInfo.GetInputType(1)?.GetNamedType() is IInputObjectGraphType inputObjectGraphType)
            {
                CheckLength(field, field.Value, inputObjectGraphType.GetField(field.Name), context);
            }
        }));

        public IVariableVisitor GetVisitor(ValidationContext _)
        {
            return FieldVisitor.Instance;
        }


        public ValueTask<INodeVisitor?> ValidateAsync(ValidationContext context)
        {
            return new ValueTask<INodeVisitor>(_nodeVisitor)!;
        }

        private static void CheckLength(ASTNode node, GraphQLValue value, IProvideMetadata? provider, ValidationContext context)
        {
            AppliedDirective? appliedDirective = provider?.FindAppliedDirective("constraint_number");
            
            if (appliedDirective == null)
            {
                return;
            }

            object? min = appliedDirective.FindArgument("min")?.Value;
            object? max = appliedDirective.FindArgument("max")?.Value;
            if (value is GraphQLNullValue)
            {
                if (min != null)
                {
                    context.ReportError(new InputAndArgumentNumberValidationError(context, node, null, (int?)min, (int?)max));
                }
            }
            else if (value is GraphQLIntValue graphQLIntValue)
            {
                ValidNumberOrError(int.Parse((string)graphQLIntValue.Value));
            }
            else if (value is GraphQLVariable vRef && context.Variables != null && context.Variables.TryGetValue(vRef.Name.StringValue, out object? val))
            {
                if (val is int number)
                    ValidNumberOrError(number);
                else if (val is null && min != null )
                    context.ReportError(new InputAndArgumentNumberValidationError(context, node, null, (int?)min, (int?)max));

            }

            void ValidNumberOrError(int number)
            {
                if (!IsValidNumber(number, (int?)min, (int?)max))
                {
                    context.ReportError(new InputAndArgumentNumberValidationError(context, node, number, (int?)min, (int?)max));
                }
            }
        }

        private static bool IsValidNumber(int number, int? min = null, int? max = null)
        {
            if (min is null && max is not null)
            {
                return number <= max;
            }

            if (max is null && min is not null)
            {
                return number >= min;
            }

            return number >= min && number <= max;
        }
    }
}