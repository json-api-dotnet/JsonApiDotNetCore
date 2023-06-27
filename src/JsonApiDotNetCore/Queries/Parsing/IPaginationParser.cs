using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Parses the JSON:API 'page' query string parameter value.
/// </summary>
public interface IPaginationParser
{
    /// <summary>
    /// Parses the specified source into a <see cref="PaginationQueryStringValueExpression" />. Throws a <see cref="QueryParseException" /> if the input is
    /// invalid.
    /// </summary>
    /// <param name="source">
    /// The source text to read from.
    /// </param>
    /// <param name="resourceType">
    /// The resource type used to lookup JSON:API fields that are referenced in <paramref name="source" />.
    /// </param>
    /// <remarks>
    /// Due to the syntax of the JSON:API pagination parameter, The returned <see cref="PaginationQueryStringValueExpression" /> is an intermediate value
    /// that gets converted into <see cref="PaginationExpression" /> by <see cref="PaginationQueryStringParameterReader" />.
    /// </remarks>
    PaginationQueryStringValueExpression Parse(string source, ResourceType resourceType);
}
