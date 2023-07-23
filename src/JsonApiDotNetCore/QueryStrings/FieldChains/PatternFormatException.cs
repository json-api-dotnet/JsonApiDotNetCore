using JetBrains.Annotations;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// The exception that is thrown when the format of a <see cref="FieldChainPattern" /> is invalid.
/// </summary>
[PublicAPI]
public sealed class PatternFormatException : FormatException
{
    /// <summary>
    /// Gets the text of the invalid pattern.
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    /// Gets the zero-based error position in <see cref="Pattern" />, or at its end.
    /// </summary>
    public int Position { get; }

    public PatternFormatException(string pattern, int position, string message)
        : base(message)
    {
        Pattern = pattern;
        Position = position;
    }
}
