namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// The exception that is thrown when the format of a dot-separated resource field chain is invalid.
/// </summary>
internal sealed class FieldChainFormatException : FormatException
{
    /// <summary>
    /// Gets the zero-based error position in the field chain, or at its end.
    /// </summary>
    public int Position { get; }

    public FieldChainFormatException(int position, string message)
        : base(message)
    {
        Position = position;
    }
}
