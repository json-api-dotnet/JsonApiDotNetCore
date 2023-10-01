using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Parses the JSON:API 'fields' query string parameter name.
/// </summary>
public interface ISparseFieldTypeParser
{
    /// <summary>
    /// Parses the specified source into a <see cref="ResourceType" />. Throws a <see cref="QueryParseException" /> if the input is invalid.
    /// </summary>
    /// <param name="source">
    /// The source text to read from.
    /// </param>
    ResourceType Parse(string source);
}
