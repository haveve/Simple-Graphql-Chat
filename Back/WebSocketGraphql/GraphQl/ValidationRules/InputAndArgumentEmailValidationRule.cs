﻿using GraphQL.Types;
using GraphQL;
using GraphQL.Validation;
using GraphQLParser.AST;
using WebSocketGraphql.GraphQl.IdentityTypes.Errors;

namespace WebSocketGraphql.GraphQl.ValidationRules
{
    public class InputAndArgumentEmailValidationRule : IValidationRule, IVariableVisitorProvider
    {

        private const string error = "Email format is invalid";

        //For variables data validation. For example - variables{ someVariable:12 }
        private sealed class FieldVisitor : BaseVariableVisitor
        {
            public static readonly FieldVisitor Instance = new();

            public override ValueTask VisitFieldAsync(ValidationContext context, GraphQLVariableDefinition variable, VariableName variableName, IInputObjectGraphType type, FieldType field, object? variableValue, object? parsedValue)
            {
                var lengthDirective = field?.FindAppliedDirective("email");
                if (lengthDirective == null)
                    return default;

                if (parsedValue == null)
                {
                    return default;
                }
                else if (parsedValue is string str && !IsValidEmail(str))
                {
                    context.ReportError(new InputAndArgumentEmailValidationError(context, variable, variableName, error));
                }

                return default;
            }
        }

        public IVariableVisitor GetVisitor(ValidationContext _) => FieldVisitor.Instance;

		//for validation directly assigned values. For example - someInputFieldOrArgument:12

        private static readonly INodeVisitor _nodeVisitor = new NodeVisitors(
        new MatchingNodeVisitor<GraphQLArgument>((arg, context) => CheckEmail(arg, arg.Value, context.TypeInfo.GetArgument(), context, $"Argument {arg}")),
        new MatchingNodeVisitor<GraphQLObjectField>((field, context) =>
        {
            if (context.TypeInfo.GetInputType(1)?.GetNamedType() is IInputObjectGraphType input)
                CheckEmail(field, field.Value, input.GetField(field.Name), context, $"Field {field}");
        })
        );

        private static void CheckEmail(ASTNode node, GraphQLValue value, IProvideMetadata? provider, ValidationContext context, string nameWithDefinition)
        {
            var directive = provider?.FindAppliedDirective("email");

            if(directive is null){
                return;
            }

            if (value is GraphQLNullValue)
            {
                return;
            }
            if (value is GraphQLStringValue strLiteral && !IsValidEmail(strLiteral.Value.ToString()))
            {
                context.ReportError(new InputAndArgumentEmailValidationError(context, node, nameWithDefinition, error));
            }
            
            else if (value is GraphQLVariable vRef && context.Variables != null && context.Variables.TryGetValue(vRef.Name.StringValue, out object? val)) //ISSUE:allocation
            {
                if (val is string strVariable && !IsValidEmail(strVariable))
                  context.ReportError(new InputAndArgumentEmailValidationError(context,node, nameWithDefinition, error));
            }
        }

        public ValueTask<INodeVisitor?> ValidateAsync(ValidationContext context)
        {
            return new(_nodeVisitor);
        }


        private static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
    }
}
