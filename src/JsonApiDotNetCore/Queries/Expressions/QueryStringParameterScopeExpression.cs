using System;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the scope of a query string parameter, resulting from text such as: ?filter[articles]=...
    /// </summary>
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
