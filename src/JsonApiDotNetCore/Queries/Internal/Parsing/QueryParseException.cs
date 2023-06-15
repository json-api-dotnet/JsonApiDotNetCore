using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public sealed class QueryParseException : Exception
{
    public QueryParseException(string message)
        : base(message)
    {
    }

    public QueryParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
