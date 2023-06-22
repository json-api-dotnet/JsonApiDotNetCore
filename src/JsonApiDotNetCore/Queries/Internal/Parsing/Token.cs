using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public sealed class Token
{
    public TokenKind Kind { get; }
    public string? Value { get; }
    public int Position { get; }

    public Token(TokenKind kind, int position)
    {
        Kind = kind;
        Position = position;
    }

    public Token(TokenKind kind, string value, int position)
        : this(kind, position)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value == null ? $"{Kind} at {Position}" : $"{Kind}: '{Value}' at {Position}";
    }
}
