using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Parses the JSON:API 'sort' and 'filter' query string parameter names, which contain a resource field chain that indicates the scope its query string
/// parameter value applies to.
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
    /// <param name="pattern">
    /// The pattern that the field chain in <paramref name="source" /> must match.
    /// </param>
    /// <param name="options">
    /// The match options for <paramref name="pattern" />.
    /// </param>
    QueryStringParameterScopeExpression Parse(string source, ResourceType resourceType, FieldChainPattern pattern, FieldChainPatternMatchOptions options);
}
