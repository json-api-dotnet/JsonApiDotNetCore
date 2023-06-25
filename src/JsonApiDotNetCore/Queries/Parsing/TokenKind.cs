namespace JsonApiDotNetCore.Queries.Parsing;

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
