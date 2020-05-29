using System;
using JsonApiDotNetCore.Internal.Queries.Expressions;

namespace JsonApiDotNetCore.Internal.Queries
{
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
