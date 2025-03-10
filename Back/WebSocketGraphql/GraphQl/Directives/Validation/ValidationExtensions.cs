using GraphQL;
using GraphQL.Builders;
using GraphQL.Types;

namespace WebSocketGraphql.GraphQl.Directives.Validation
{
    public static class ValidationExtensions
    {
        #region Arguments

        public static TMetadataProvider RestrictLength<TMetadataProvider>(this TMetadataProvider argument, int minLength, int maxLength) where TMetadataProvider : IProvideMetadata
            => argument.ApplyDirective(ValidationDirectives.Length, "min", minLength, "max", maxLength);

        public static TMetadataProvider RestrictNumberRange<TMetadataProvider>(this TMetadataProvider argument, int minLength, int maxLength) where TMetadataProvider : IProvideMetadata
           => argument.ApplyDirective(ValidationDirectives.NumberRange, "min", minLength, "max", maxLength);

        public static TMetadataProvider RestrictAsEmail<TMetadataProvider>(this TMetadataProvider argument) where TMetadataProvider : IProvideMetadata
           => argument.ApplyDirective(ValidationDirectives.Email);

        #endregion

        #region FieldBuilder

        public static FieldBuilder<TSourceType, TReturnType> RestrictLength<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> fieldBuilder, int minLength, int maxLength)
           => fieldBuilder.Directive(ValidationDirectives.Length, "min", minLength, "max", maxLength);

        public static FieldBuilder<TSourceType, TReturnType> RestrictNumberRange<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> fieldBuilder, int minLength, int maxLength)
           => fieldBuilder.Directive(ValidationDirectives.NumberRange, "min", minLength, "max", maxLength);

        public static FieldBuilder<TSourceType, TReturnType> RestrictAsEmail<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> fieldBuilder)
           => fieldBuilder.Directive(ValidationDirectives.Email);

        #endregion
    }
}
