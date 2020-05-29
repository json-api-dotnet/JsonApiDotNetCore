using System;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class QueryStringParameterScopeExpression : QueryExpression
    {
        public LiteralConstantExpression ParameterName { get; }
        public ResourceFieldChainExpression Scope { get; }

        public QueryStringParameterScopeExpression(LiteralConstantExpression parameterName, ResourceFieldChainExpression scope)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            Scope = scope;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitQueryStringParameterScope(this, argument);
        }

        public override string ToString()
        {
            return Scope == null ? ParameterName.ToString() : $"{ParameterName}: {Scope}";
        }
    }
}
