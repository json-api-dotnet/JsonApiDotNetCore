using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Parses the JSON:API 'include' query string parameter value.
/// </summary>
public interface IIncludeParser
{
    /// <summary>
    /// Parses the specified source into an <see cref="IncludeExpression" />. Throws a <see cref="QueryParseException" /> if the input is invalid.
    /// </summary>
    /// <param name="source">
    /// The source text to read from.
    /// </param>
    /// <param name="resourceType">
    /// The resource type used to lookup JSON:API fields that are referenced in <paramref name="source" />.
    /// </param>
    IncludeExpression Parse(string source, ResourceType resourceType);
}
