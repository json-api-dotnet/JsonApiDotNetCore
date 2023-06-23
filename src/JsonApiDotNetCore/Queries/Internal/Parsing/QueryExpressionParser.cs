using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

/// <summary>
/// The base class for parsing query string parameters, using the Recursive Descent algorithm.
/// </summary>
/// <remarks>
/// Uses a tokenizer to populate a stack of tokens, which is then manipulated from the various parsing routines for subexpressions. Implementations
/// should throw <see cref="QueryParseException" /> on invalid input.
/// </remarks>
[PublicAPI]
public abstract class QueryExpressionParser
{
    private int _endOfSourcePosition;

    protected Stack<Token> TokenStack { get; private set; } = null!;

    /// <summary>
    /// Enables derived types to throw a <see cref="QueryParseException" /> when a field in a resource field chain is not permitted.
    /// </summary>
    protected virtual void ValidateField(ResourceFieldAttribute field, int position)
    {
    }

    protected virtual void Tokenize(string source)
    {
        var tokenizer = new QueryTokenizer(source);
        TokenStack = new Stack<Token>(tokenizer.EnumerateTokens().Reverse());
        _endOfSourcePosition = source.Length;
    }

    /// <summary>
    /// Parses a dot-separated path of field names into a chain of fields, while matching it against the specified pattern.
    /// </summary>
    protected ResourceFieldChainExpression ParseFieldChain(FieldChainPattern pattern, FieldChainPatternMatchOptions options, ResourceType resourceType,
        string? alternativeErrorMessage)
    {
        ArgumentGuard.NotNull(pattern);
        ArgumentGuard.NotNull(resourceType);

        int startPosition = GetNextTokenPositionOrEnd();

        string path = EatFieldChain(alternativeErrorMessage);
        PatternMatchResult result = pattern.Match(path, resourceType, options);

        if (!result.IsSuccess)
        {
            string message = result.IsFieldChainError
                ? result.FailureMessage
                : $"Field chain on resource type '{resourceType}' failed to match the pattern: {pattern.GetDescription()}. {result.FailureMessage}";

            throw new QueryParseException(message, startPosition + result.FailurePosition);
        }

        int chainPosition = 0;

        foreach (ResourceFieldAttribute field in result.FieldChain)
        {
            ValidateField(field, startPosition + chainPosition);

            chainPosition += field.PublicName.Length + 1;
        }

        return new ResourceFieldChainExpression(result.FieldChain.ToImmutableArray());
    }

    private string EatFieldChain(string? alternativeErrorMessage)
    {
        var pathBuilder = new StringBuilder();

        while (true)
        {
            int position = GetNextTokenPositionOrEnd();

            if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text && token.Value != Keywords.Null)
            {
                pathBuilder.Append(token.Value);

                if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Period)
                {
                    EatSingleCharacterToken(TokenKind.Period);
                    pathBuilder.Append('.');
                }
                else
                {
                    return pathBuilder.ToString();
                }
            }
            else
            {
                throw new QueryParseException(alternativeErrorMessage ?? "Field name expected.", position);
            }
        }
    }

    protected CountExpression? TryParseCount(FieldChainPatternMatchOptions options, ResourceType resourceType)
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text, Value: Keywords.Count })
        {
            TokenStack.Pop();

            EatSingleCharacterToken(TokenKind.OpenParen);

            ResourceFieldChainExpression targetCollection = ParseFieldChain(BuiltInPatterns.ToOneChainEndingInToMany, options, resourceType, null);

            EatSingleCharacterToken(TokenKind.CloseParen);

            return new CountExpression(targetCollection);
        }

        return null;
    }

    protected void EatText(string text)
    {
        if (!TokenStack.TryPop(out Token? token) || token.Kind != TokenKind.Text || token.Value != text)
        {
            int position = token?.Position ?? GetNextTokenPositionOrEnd();
            throw new QueryParseException($"{text} expected.", position);
        }
    }

    protected void EatSingleCharacterToken(TokenKind kind)
    {
        if (!TokenStack.TryPop(out Token? token) || token.Kind != kind)
        {
            char ch = QueryTokenizer.SingleCharacterToTokenKinds.Single(pair => pair.Value == kind).Key;
            int position = token?.Position ?? GetNextTokenPositionOrEnd();
            throw new QueryParseException($"{ch} expected.", position);
        }
    }

    protected int GetNextTokenPositionOrEnd()
    {
        if (TokenStack.TryPeek(out Token? nextToken))
        {
            return nextToken.Position;
        }

        return _endOfSourcePosition;
    }

    protected int GetRelativePositionOfLastFieldInChain(ResourceFieldChainExpression fieldChain)
    {
        ArgumentGuard.NotNull(fieldChain);

        int position = 0;

        for (int index = 0; index < fieldChain.Fields.Count - 1; index++)
        {
            position += fieldChain.Fields[index].PublicName.Length + 1;
        }

        return position;
    }

    protected void AssertTokenStackIsEmpty()
    {
        if (TokenStack.Any())
        {
            int position = GetNextTokenPositionOrEnd();
            throw new QueryParseException("End of expression expected.", position);
        }
    }
}
