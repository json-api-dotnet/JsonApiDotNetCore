using System.Linq.Expressions;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents the base data structure for immutable types that query string parameters are converted into. This intermediate structure is later
/// transformed into System.Linq <see cref="Expression" /> trees that are handled by Entity Framework Core.
/// </summary>
public abstract class QueryExpression
{
    /// <summary>
    /// Dispatches the current query expression to the specified visitor with the given argument.
    /// </summary>
    /// <typeparam name="TArgument">
    /// The type of the argument passed to the visitor.
    /// </typeparam>
    /// <typeparam name="TResult">
    /// The type of the result returned by the visitor.
    /// </typeparam>
    /// <param name="visitor">
    /// A visitor that processes the query expression.
    /// </param>
    /// <param name="argument">
    /// An argument to pass to the visitor.
    /// </param>
    /// <returns>
    /// The result produced by the visitor.
    /// </returns>
    public abstract TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument);

    /// <summary>
    /// Returns a string representation that includes all details of the expression.
    /// </summary>
    public abstract string ToFullString();
}
