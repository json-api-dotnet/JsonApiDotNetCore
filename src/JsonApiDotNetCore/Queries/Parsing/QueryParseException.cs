using System.Text;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// The error that is thrown when parsing a query string parameter fails.
/// </summary>
[PublicAPI]
public sealed class QueryParseException : Exception
{
    /// <summary>
    /// Gets the zero-based position in the text of the query string parameter name/value, or at its end, where the failure occurred, or -1 if unavailable.
    /// </summary>
    public int Position { get; }

    public QueryParseException(string message, int position)
        : base(message)
    {
        Position = position;
    }

    public QueryParseException(string message, int position, Exception innerException)
        : base(message, innerException)
    {
        Position = position;
    }

    public string GetMessageWithPosition(string sourceText)
    {
        ArgumentGuard.NotNull(sourceText);

        if (Position < 0)
        {
            return Message;
        }

        StringBuilder builder = new();
        builder.Append(Message);
        builder.Append($" Failed at position {Position + 1}: {sourceText[..Position]}^{sourceText[Position..]}");
        return builder.ToString();
    }
}
