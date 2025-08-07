using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// The base class for parsing query string parameters, using the Recursive Descent algorithm.
/// </summary>
/// <remarks>
/// A tokenizer populates a stack of tokens from the source text, which is then recursively popped by various parsing routines. A
/// <see cref="QueryParseException" /> is expected to be thrown on invalid input.
/// </remarks>
[PublicAPI]
public abstract class QueryExpressionParser
{
    /// <summary>
    /// Contains the tokens produced from the source text, after <see cref="Tokenize" /> has been called.
    /// </summary>
    /// <remarks>
    /// The various parsing methods typically pop tokens while producing <see cref="QueryExpression" />s.
    /// </remarks>
    protected Stack<Token> TokenStack { get; private set; } = new();

    /// <summary>
    /// Contains the source text that tokens were produced from, after <see cref="Tokenize" /> has been called.
    /// </summary>
    protected string Source { get; private set; } = string.Empty;

    /// <summary>
    /// Enables derived types to throw a <see cref="QueryParseException" /> when usage of a JSON:API field inside a field chain is not permitted.
    /// </summary>
    protected virtual void ValidateField(ResourceFieldAttribute field, int position)
    {
    }

    /// <summary>
    /// Populates <see cref="TokenStack" /> from the source text using <see cref="QueryTokenizer" />.
    /// </summary>
    /// <remarks>
    /// To use a custom tokenizer, override this method and consider overriding <see cref="EatSingleCharacterToken" />.
    /// </remarks>
    protected virtual void Tokenize(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Source = source;

        var tokenizer = new QueryTokenizer(source);
        TokenStack = new Stack<Token>(tokenizer.EnumerateTokens().Reverse());
    }

    /// <summary>
    /// Parses a dot-separated path of field names into a chain of resource fields, while matching it against the specified pattern.
    /// </summary>
    protected ResourceFieldChainExpression ParseFieldChain(FieldChainPattern pattern, FieldChainPatternMatchOptions options, ResourceType resourceType,
        string? alternativeErrorMessage)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(resourceType);

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

    /// <summary>
    /// Consumes a token containing the expected text from the top of <see cref="TokenStack" />. Throws a <see cref="QueryParseException" /> if a different
    /// token kind is at the top, it contains a different text, or if there are no more tokens available.
    /// </summary>
    protected void EatText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!TokenStack.TryPop(out Token? token) || token.Kind != TokenKind.Text || token.Value != text)
        {
            int position = token?.Position ?? GetNextTokenPositionOrEnd();
            throw new QueryParseException($"{text} expected.", position);
        }
    }

    /// <summary>
    /// Consumes the expected token kind from the top of <see cref="TokenStack" />. Throws a <see cref="QueryParseException" /> if a different token kind is
    /// at the top, or if there are no more tokens available.
    /// </summary>
    protected virtual void EatSingleCharacterToken(TokenKind kind)
    {
        if (!TokenStack.TryPop(out Token? token) || token.Kind != kind)
        {
            char ch = QueryTokenizer.SingleCharacterToTokenKinds.Single(pair => pair.Value == kind).Key;
            int position = token?.Position ?? GetNextTokenPositionOrEnd();
            throw new QueryParseException($"{ch} expected.", position);
        }
    }

    /// <summary>
    /// Gets the zero-based position of the token at the top of <see cref="TokenStack" />, or the position at the end of the source text if there are no more
    /// tokens available.
    /// </summary>
    protected int GetNextTokenPositionOrEnd()
    {
        if (TokenStack.TryPeek(out Token? nextToken))
        {
            return nextToken.Position;
        }

        return Source.Length;
    }

    /// <summary>
    /// Gets the zero-based position of the last field in the specified resource field chain.
    /// </summary>
    protected int GetRelativePositionOfLastFieldInChain(ResourceFieldChainExpression fieldChain)
    {
        ArgumentNullException.ThrowIfNull(fieldChain);

        int position = 0;

        for (int index = 0; index < fieldChain.Fields.Count - 1; index++)
        {
            position += fieldChain.Fields[index].PublicName.Length + 1;
        }

        return position;
    }

    /// <summary>
    /// Throws a <see cref="QueryParseException" /> when <see cref="TokenStack" /> isn't empty. Derived types should call this when parsing has completed, to
    /// ensure all input has been processed.
    /// </summary>
    protected void AssertTokenStackIsEmpty()
    {
        if (TokenStack.Count > 0)
        {
            int position = GetNextTokenPositionOrEnd();
            throw new QueryParseException("End of expression expected.", position);
        }
    }
}
