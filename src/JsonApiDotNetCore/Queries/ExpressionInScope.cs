using System;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// Represents an expression coming from query string. The scope determines at which depth in the <see cref="IResourceGraph"/> to apply its expression.
    /// </summary>
    public class ExpressionInScope
    {
        public ResourceFieldChainExpression Scope { get; }
        public QueryExpression Expression { get; }

        public ExpressionInScope(ResourceFieldChainExpression scope, QueryExpression expression)
        {
            Scope = scope;
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public override string ToString()
        {
            return $"{Scope} => {Expression}";
        }
    }
}
