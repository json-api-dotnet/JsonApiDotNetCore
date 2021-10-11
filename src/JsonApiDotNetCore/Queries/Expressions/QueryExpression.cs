using System.Linq.Expressions;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the base data structure for immutable types that query string parameters are converted into. This intermediate structure is later
    /// transformed into system <see cref="Expression" /> trees that are handled by Entity Framework Core.
    /// </summary>
    public abstract class QueryExpression
    {
        public abstract TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument);
    }
}
