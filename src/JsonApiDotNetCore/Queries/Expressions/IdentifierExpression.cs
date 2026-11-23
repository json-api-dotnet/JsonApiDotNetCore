namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents the base type for an identifier, such as a JSON:API attribute/relationship name, a constant value between quotes, <c>null</c> or the
/// <c>_</c> placeholder for the current element of a scalar collection.
/// </summary>
public abstract class IdentifierExpression : QueryExpression;
