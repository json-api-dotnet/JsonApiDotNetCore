namespace JsonApiDotNetCore.Queries.Internal.Parsing;

public enum TokenKind
{
    OpenParen,
    CloseParen,
    OpenBracket,
    CloseBracket,
    Comma,
    Colon,
    Minus,
    Text,
    QuotedText
}
