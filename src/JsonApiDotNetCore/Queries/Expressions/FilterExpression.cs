namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents the base type for filter functions that return a boolean value.
/// </summary>
public abstract class FilterExpression : FunctionExpression
{
    /// <summary>
    /// The CLR type this function returns, which is always <see cref="bool" />.
    /// </summary>
    public override Type ReturnType { get; } = typeof(bool);
}
