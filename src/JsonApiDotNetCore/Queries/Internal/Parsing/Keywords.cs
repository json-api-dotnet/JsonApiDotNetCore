using JetBrains.Annotations;

#pragma warning disable AV1008 // Class should not be static
#pragma warning disable AV1010 // Member hides inherited member

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    [PublicAPI]
    public static class Keywords
    {
        public const string Null = "null";
        public const string Not = "not";
        public const string And = "and";
        public const string Or = "or";
        public new const string Equals = "equals";
        public const string GreaterThan = "greaterThan";
        public const string GreaterOrEqual = "greaterOrEqual";
        public const string LessThan = "lessThan";
        public const string LessOrEqual = "lessOrEqual";
        public const string Contains = "contains";
        public const string StartsWith = "startsWith";
        public const string EndsWith = "endsWith";
        public const string Any = "any";
        public const string Count = "count";
        public const string Has = "has";
    }
}
