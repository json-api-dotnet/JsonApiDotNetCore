using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public class IncludeParser : QueryExpressionParser
{
    private static readonly IncludeChainConverter IncludeChainConverter = new();

    private readonly Action<RelationshipAttribute, ResourceType, string>? _validateSingleRelationshipCallback;
    private ResourceType? _resourceTypeInScope;

    public IncludeParser(Action<RelationshipAttribute, ResourceType, string>? validateSingleRelationshipCallback = null)
    {
        _validateSingleRelationshipCallback = validateSingleRelationshipCallback;
    }

    public IncludeExpression Parse(string source, ResourceType resourceTypeInScope, int? maximumDepth)
    {
        ArgumentGuard.NotNull(resourceTypeInScope, nameof(resourceTypeInScope));

        _resourceTypeInScope = resourceTypeInScope;

        Tokenize(source);

        IncludeExpression expression = ParseInclude(maximumDepth);

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected IncludeExpression ParseInclude(int? maximumDepth)
    {
        ResourceFieldChainExpression firstChain = ParseFieldChain(FieldChainRequirements.IsRelationship, "Relationship name expected.");

        List<ResourceFieldChainExpression> chains = firstChain.AsList();

        while (TokenStack.Any())
        {
            EatSingleCharacterToken(TokenKind.Comma);

            ResourceFieldChainExpression nextChain = ParseFieldChain(FieldChainRequirements.IsRelationship, "Relationship name expected.");
            chains.Add(nextChain);
        }

        ValidateMaximumIncludeDepth(maximumDepth, chains);

        return IncludeChainConverter.FromRelationshipChains(chains);
    }

    private static void ValidateMaximumIncludeDepth(int? maximumDepth, IEnumerable<ResourceFieldChainExpression> chains)
    {
        if (maximumDepth != null)
        {
            foreach (ResourceFieldChainExpression chain in chains)
            {
                if (chain.Fields.Count > maximumDepth)
                {
                    string path = string.Join('.', chain.Fields.Select(field => field.PublicName));
                    throw new QueryParseException($"Including '{path}' exceeds the maximum inclusion depth of {maximumDepth}.");
                }
            }
        }
    }

    protected override IImmutableList<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
    {
        return ChainResolver.ResolveRelationshipChain(_resourceTypeInScope!, path, _validateSingleRelationshipCallback);
    }
}
