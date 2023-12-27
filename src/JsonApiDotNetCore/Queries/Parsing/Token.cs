using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Parsing;

[PublicAPI]
public class Token(TokenKind kind, int position)
{
    public TokenKind Kind { get; } = kind;
    public string? Value { get; }
    public int Position { get; } = position;

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
