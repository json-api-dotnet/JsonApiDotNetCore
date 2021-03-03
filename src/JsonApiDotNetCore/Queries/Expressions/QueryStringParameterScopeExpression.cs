using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the scope of a query string parameter, resulting from text such as: ?filter[articles]=...
    /// </summary>
    [PublicAPI]
    public class QueryStringParameterScopeExpression : QueryExpression
    {
        public LiteralConstantExpression ParameterName { get; }
        public ResourceFieldChainExpression Scope { get; }

        public QueryStringParameterScopeExpression(LiteralConstantExpression parameterName, ResourceFieldChainExpression scope)
        {
            ArgumentGuard.NotNull(parameterName, nameof(parameterName));

            ParameterName = parameterName;
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (QueryStringParameterScopeExpression)obj;

            return ParameterName.Equals(other.ParameterName) && Equals(Scope, other.Scope);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ParameterName, Scope);
        }
    }
}
