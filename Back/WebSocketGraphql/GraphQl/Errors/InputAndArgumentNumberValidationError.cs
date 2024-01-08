using System;
using GraphQL;
using GraphQL.Validation;
using GraphQLParser.AST;
public class InputAndArgumentNumberValidationError : ValidationError
{
    private const string NUMBER = "5.6.1";
    public InputAndArgumentNumberValidationError(ValidationContext context, ASTNode node, int? value, int? min, int? max)
        : base(context.Document.Source, NUMBER, BadValueMessage(node, value, min, max), node)
    {
    }

    public InputAndArgumentNumberValidationError(ValidationContext context, GraphQLVariableDefinition node, VariableName variableName, int? value, int? min, int? max)
        : base(context.Document.Source, NUMBER, BadValueMessage(variableName, value, min, max), node)
    {
    }

    private static string BadValueMessage(ASTNode node, int? number, int? minLength, int? maxLength)
    {
        string value = number is not null ? number!.ToString()! : "null";
        string value2 = minLength.GetValueOrDefault().ToString();
        string value3 = maxLength is not null ? maxLength!.ToString()! : "unrestricted";
        return $"{node.Kind} '{((INamedNode)node).Name}' has invalid value ({value}). Value must be in range [{value2}, {value3}].";
    }

    private static string BadValueMessage(VariableName variableName, int? number, int? minLength, int? maxLength)
    {
        string value = number is not null ? number!.ToString()! : "null";
        string value2 = minLength.GetValueOrDefault().ToString();
        string value3 = maxLength is not null ? maxLength!.ToString()! : "unrestricted";
        return $"Variable '{variableName}' has invalid value ({value}). Value must be in range [{value2}, {value3}].";
    }
}