using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public class SparseFieldSetParser : QueryExpressionParser
{
    private readonly Action<ResourceFieldAttribute, ResourceType, string>? _validateSingleFieldCallback;
    private ResourceType? _resourceType;

    public SparseFieldSetParser(Action<ResourceFieldAttribute, ResourceType, string>? validateSingleFieldCallback = null)
    {
        _validateSingleFieldCallback = validateSingleFieldCallback;
    }

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

        _validateSingleFieldCallback?.Invoke(field, _resourceType!, path);

        return ImmutableArray.Create(field);
    }
}
