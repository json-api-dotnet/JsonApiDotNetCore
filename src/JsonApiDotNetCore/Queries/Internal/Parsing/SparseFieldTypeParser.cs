using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public class SparseFieldTypeParser : QueryExpressionParser
{
    private readonly IResourceGraph _resourceGraph;

    public SparseFieldTypeParser(IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(resourceGraph);

        _resourceGraph = resourceGraph;
    }

    public ResourceType Parse(string source)
    {
        Tokenize(source);

        ResourceType resourceType = ParseSparseFieldTarget();

        AssertTokenStackIsEmpty();

        return resourceType;
    }

    private ResourceType ParseSparseFieldTarget()
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

    protected override IImmutableList<ResourceFieldAttribute> OnResolveFieldChain(string path, int position, FieldChainRequirements chainRequirements)
    {
        throw new NotSupportedException();
    }
}
