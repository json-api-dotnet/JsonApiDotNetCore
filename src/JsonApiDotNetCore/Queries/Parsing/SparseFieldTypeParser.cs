using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <inheritdoc cref="ISparseFieldTypeParser" />
[PublicAPI]
public class SparseFieldTypeParser : QueryExpressionParser, ISparseFieldTypeParser
{
    private readonly IResourceGraph _resourceGraph;

    public SparseFieldTypeParser(IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(resourceGraph);

        _resourceGraph = resourceGraph;
    }

    /// <inheritdoc />
    public ResourceType Parse(string source)
    {
        Tokenize(source);

        ResourceType resourceType = ParseSparseFieldType();

        AssertTokenStackIsEmpty();

        return resourceType;
    }

    protected virtual ResourceType ParseSparseFieldType()
    {
        int position = GetNextTokenPositionOrEnd();

        if (!TokenStack.TryPop(out Token? token) || token.Kind != TokenKind.Text)
        {
            throw new QueryParseException("Parameter name expected.", position);
        }

        EatSingleCharacterToken(TokenKind.OpenBracket);

        ResourceType resourceType = ParseResourceType();

        EatSingleCharacterToken(TokenKind.CloseBracket);

        return resourceType;
    }

    private ResourceType ParseResourceType()
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text)
        {
            return GetResourceType(token.Value!, token.Position);
        }

        throw new QueryParseException("Resource type expected.", position);
    }

    private ResourceType GetResourceType(string publicName, int position)
    {
        ResourceType? resourceType = _resourceGraph.FindResourceType(publicName);

        if (resourceType == null)
        {
            throw new QueryParseException($"Resource type '{publicName}' does not exist.", position);
        }

        return resourceType;
    }
}
