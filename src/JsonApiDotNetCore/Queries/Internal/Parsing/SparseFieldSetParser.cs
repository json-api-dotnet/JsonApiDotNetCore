using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public class SparseFieldSetParser : QueryExpressionParser
{
    private ResourceType? _resourceType;

    public SparseFieldSetExpression? Parse(string source, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        _resourceType = resourceType;

        Tokenize(source);

        SparseFieldSetExpression? expression = ParseSparseFieldSet();

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected SparseFieldSetExpression? ParseSparseFieldSet()
    {
        ImmutableHashSet<ResourceFieldAttribute>.Builder fieldSetBuilder = ImmutableHashSet.CreateBuilder<ResourceFieldAttribute>();

        while (TokenStack.Any())
        {
            if (fieldSetBuilder.Count > 0)
            {
                EatSingleCharacterToken(TokenKind.Comma);
            }

            ResourceFieldChainExpression nextChain = ParseFieldChain(FieldChainRequirements.EndsInAttribute, "Field name expected.");
            ResourceFieldAttribute nextField = nextChain.Fields.Single();
            fieldSetBuilder.Add(nextField);
        }

        return fieldSetBuilder.Any() ? new SparseFieldSetExpression(fieldSetBuilder.ToImmutable()) : null;
    }

    protected override IImmutableList<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
    {
        ResourceFieldAttribute field = ChainResolver.GetField(path, _resourceType!, path);

        ValidateSingleField(field, _resourceType!, path);

        return ImmutableArray.Create(field);
    }

    protected override void ValidateSingleField(ResourceFieldAttribute field, ResourceType resourceType, string path)
    {
        if (field.IsViewBlocked())
        {
            string kind = field is AttrAttribute ? "attribute" : "relationship";
            throw new QueryParseException($"Retrieving the {kind} '{field.PublicName}' is not allowed.");
        }
    }
}
