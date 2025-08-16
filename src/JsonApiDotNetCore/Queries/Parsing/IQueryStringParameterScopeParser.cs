using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Parser for the JSON:API 'sort' and 'filter' query string parameter names, which indicate the scope their query string parameter value applies to. The
/// value consists of an optional relationship chain ending in a to-many relationship, surrounded by brackets.
/// </summary>
public interface IQueryStringParameterScopeParser
{
    /// <summary>
    /// Parses the specified source into a <see cref="QueryStringParameterScopeExpression" />. Throws a <see cref="QueryParseException" /> if the input is
    /// invalid.
    /// </summary>
    /// <param name="source">
    /// The source text to read from.
    /// </param>
    /// <param name="resourceType">
    /// The resource type used to lookup JSON:API fields that are referenced in <paramref name="source" />.
    /// </param>
    QueryStringParameterScopeExpression Parse(string source, ResourceType resourceType);
}
