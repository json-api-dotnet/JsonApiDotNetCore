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
        ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

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
        if (!TokenStack.TryPop(out Token? token) || token.Kind != TokenKind.Text)
        {
            throw new QueryParseException("Parameter name expected.");
        }

        EatSingleCharacterToken(TokenKind.OpenBracket);

        ResourceType resourceType = ParseResourceName();

        EatSingleCharacterToken(TokenKind.CloseBracket);

        return resourceType;
    }

    private ResourceType ParseResourceName()
    {
        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text)
        {
            return GetResourceType(token.Value!);
        }

        throw new QueryParseException("Resource type expected.");
    }

    private ResourceType GetResourceType(string publicName)
    {
        ResourceType? resourceType = _resourceGraph.FindResourceType(publicName);

        if (resourceType == null)
        {
            throw new QueryParseException($"Resource type '{publicName}' does not exist.");
        }

        return resourceType;
    }

    protected override IImmutableList<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
    {
        throw new NotSupportedException();
    }
}
