using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Parses the JSON:API 'filter' query string parameter value.
/// </summary>
public interface IFilterParser
{
    /// <summary>
    /// Parses the specified source into a <see cref="FilterExpression" />. Throws a <see cref="QueryParseException" /> if the input is invalid.
    /// </summary>
    /// <param name="source">
    /// The source text to read from.
    /// </param>
    /// <param name="resourceType">
    /// The resource type used to lookup JSON:API fields that are referenced in <paramref name="source" />.
    /// </param>
    FilterExpression Parse(string source, ResourceType resourceType);
}
