using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Parses the JSON:API 'fields' query string parameter value.
/// </summary>
public interface ISparseFieldSetParser
{
    /// <summary>
    /// Parses the specified source into a <see cref="SparseFieldSetExpression" />. Throws a <see cref="QueryParseException" /> if the input is invalid.
    /// </summary>
    /// <param name="source">
    /// The source text to read from.
    /// </param>
    /// <param name="resourceType">
    /// The resource type used to lookup JSON:API fields that are referenced in <paramref name="source" />.
    /// </param>
    SparseFieldSetExpression? Parse(string source, ResourceType resourceType);
}
