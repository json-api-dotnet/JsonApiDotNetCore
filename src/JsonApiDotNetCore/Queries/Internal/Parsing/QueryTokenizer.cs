using System.Collections.ObjectModel;
using System.Text;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public sealed class QueryTokenizer
{
    public static readonly IReadOnlyDictionary<char, TokenKind> SingleCharacterToTokenKinds = new ReadOnlyDictionary<char, TokenKind>(
        new Dictionary<char, TokenKind>
        {
            ['('] = TokenKind.OpenParen,
            [')'] = TokenKind.CloseParen,
            ['['] = TokenKind.OpenBracket,
            [']'] = TokenKind.CloseBracket,
            ['.'] = TokenKind.Period,
            [','] = TokenKind.Comma,
            [':'] = TokenKind.Colon,
            ['-'] = TokenKind.Minus
        });

    private readonly string _source;
    private readonly StringBuilder _textBuffer = new();
    private int _sourceOffset;
    private int? _tokenStartOffset;
    private bool _isInQuotedSection;

    public QueryTokenizer(string source)
    {
        ArgumentGuard.NotNull(source);

        _source = source;
    }

    public IEnumerable<Token> EnumerateTokens()
    {
        _textBuffer.Clear();
        _isInQuotedSection = false;
        _sourceOffset = 0;
        _tokenStartOffset = null;

        while (_sourceOffset < _source.Length)
        {
            _tokenStartOffset ??= _sourceOffset;

            char ch = _source[_sourceOffset];

            if (ch == '\'')
            {
                if (_isInQuotedSection)
                {
                    char? peeked = PeekChar();

                    if (peeked == '\'')
                    {
                        _textBuffer.Append(ch);
                        _sourceOffset += 2;
                        continue;
                    }

                    _isInQuotedSection = false;

                    Token literalToken = ProduceTokenFromTextBuffer(true)!;
                    yield return literalToken;
                }
                else
                {
                    if (_textBuffer.Length > 0)
                    {
                        throw new QueryParseException("Unexpected ' outside text.", _sourceOffset);
                    }

                    _isInQuotedSection = true;
                }
            }
            else
            {
                TokenKind? singleCharacterTokenKind = _isInQuotedSection ? null : TryGetSingleCharacterTokenKind(ch);

                if (singleCharacterTokenKind != null && !IsMinusInsideText(singleCharacterTokenKind.Value))
                {
                    Token? identifierToken = ProduceTokenFromTextBuffer(false);

                    if (identifierToken != null)
                    {
                        yield return identifierToken;
                    }

                    yield return new Token(singleCharacterTokenKind.Value, _sourceOffset);

                    _tokenStartOffset = null;
                }
                else
                {
                    if (ch == ' ' && !_isInQuotedSection)
                    {
                        throw new QueryParseException("Unexpected whitespace.", _sourceOffset);
                    }

                    _textBuffer.Append(ch);
                }
            }

            _sourceOffset++;
        }

        if (_isInQuotedSection)
        {
            throw new QueryParseException("' expected.", _sourceOffset - 1);
        }

        Token? lastToken = ProduceTokenFromTextBuffer(false);

        if (lastToken != null)
        {
            yield return lastToken;
        }
    }

    private bool IsMinusInsideText(TokenKind kind)
    {
        return kind == TokenKind.Minus && _textBuffer.Length > 0;
    }

    private char? PeekChar()
    {
        return _sourceOffset + 1 < _source.Length ? _source[_sourceOffset + 1] : null;
    }

    private static TokenKind? TryGetSingleCharacterTokenKind(char ch)
    {
        return SingleCharacterToTokenKinds.TryGetValue(ch, out TokenKind tokenKind) ? tokenKind : null;
    }

    private Token? ProduceTokenFromTextBuffer(bool isQuotedText)
    {
        if (isQuotedText || _textBuffer.Length > 0)
        {
            int tokenStartOffset = _tokenStartOffset!.Value;
            string text = _textBuffer.ToString();

            _textBuffer.Clear();
            _tokenStartOffset = null;

            return new Token(isQuotedText ? TokenKind.QuotedText : TokenKind.Text, text, tokenStartOffset);
        }

        return null;
    }
}
