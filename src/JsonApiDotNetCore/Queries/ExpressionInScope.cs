using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// Represents an expression coming from query string. The scope determines at which depth in the <see cref="IResourceGraph" /> to apply its expression.
    /// </summary>
    [PublicAPI]
    public class ExpressionInScope
    {
        public ResourceFieldChainExpression Scope { get; }
        public QueryExpression Expression { get; }

        public ExpressionInScope(ResourceFieldChainExpression scope, QueryExpression expression)
        {
            ArgumentGuard.NotNull(expression, nameof(expression));

            Scope = scope;
            Expression = expression;
        }

        public override string ToString()
        {
            return $"{Scope} => {Expression}";
        }
    }
}
