using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public sealed class Token
{
    public TokenKind Kind { get; }
    public string? Value { get; }

    public Token(TokenKind kind)
    {
        Kind = kind;
    }

    public Token(TokenKind kind, string value)
        : this(kind)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value == null ? Kind.ToString() : $"{Kind}: {Value}";
    }
}
