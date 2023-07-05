namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents the base type for functions that return a value.
/// </summary>
public abstract class FunctionExpression : QueryExpression
{
    /// <summary>
    /// The CLR type this function returns.
    /// </summary>
    public abstract Type ReturnType { get; }
}
