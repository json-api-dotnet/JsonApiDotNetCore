namespace JsonApiDotNetCore.Queries.Internal.Parsing;

public enum TokenKind
{
    OpenParen,
    CloseParen,
    OpenBracket,
    CloseBracket,
    Period,
    Comma,
    Colon,
    Minus,
    Text,
    QuotedText
}
