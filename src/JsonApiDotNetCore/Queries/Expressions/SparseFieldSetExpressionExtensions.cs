using System.Collections.Immutable;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

[PublicAPI]
public static class SparseFieldSetExpressionExtensions
{
    public static SparseFieldSetExpression? Including<TResource>(this SparseFieldSetExpression? sparseFieldSet,
        Expression<Func<TResource, object?>> fieldSelector, IResourceGraph resourceGraph)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(fieldSelector);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        SparseFieldSetExpression? newSparseFieldSet = sparseFieldSet;

        foreach (ResourceFieldChainExpression chain in resourceGraph.GetFieldChains(fieldSelector))
        {
            newSparseFieldSet = IncludeFieldChain(newSparseFieldSet, chain);
        }

        return newSparseFieldSet;
    }

    public static SparseFieldSetExpression? Including(this SparseFieldSetExpression? sparseFieldSet,
        ResourceFieldChainExpression fieldChain)
    {
        ArgumentNullException.ThrowIfNull(fieldChain);

        return IncludeFieldChain(sparseFieldSet, fieldChain);
    }

    private static SparseFieldSetExpression? IncludeField(SparseFieldSetExpression? sparseFieldSet, ResourceFieldAttribute fieldToInclude)
    {
        return IncludeFieldChain(sparseFieldSet, new ResourceFieldChainExpression(fieldToInclude));
    }

    private static SparseFieldSetExpression? IncludeFieldChain(SparseFieldSetExpression? sparseFieldSet, ResourceFieldChainExpression chainToInclude)
    {
        if (sparseFieldSet == null || sparseFieldSet.FieldChains.Contains(chainToInclude))
        {
            return sparseFieldSet;
        }

        IImmutableSet<ResourceFieldChainExpression> newFieldChains = sparseFieldSet.FieldChains.Add(chainToInclude);
        return new SparseFieldSetExpression(newFieldChains);
    }

    public static SparseFieldSetExpression? Excluding<TResource>(this SparseFieldSetExpression? sparseFieldSet,
        Expression<Func<TResource, object?>> fieldSelector, IResourceGraph resourceGraph)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(fieldSelector);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        SparseFieldSetExpression? newSparseFieldSet = sparseFieldSet;

        foreach (ResourceFieldChainExpression chain in resourceGraph.GetFieldChains(fieldSelector))
        {
            newSparseFieldSet = ExcludeFieldChain(newSparseFieldSet, chain);
        }

        return newSparseFieldSet;
    }

    public static SparseFieldSetExpression? Excluding(this SparseFieldSetExpression? sparseFieldSet,
        ResourceFieldChainExpression fieldChain)
    {
        ArgumentNullException.ThrowIfNull(fieldChain);

        return ExcludeFieldChain(sparseFieldSet, fieldChain);
    }

    private static SparseFieldSetExpression? ExcludeField(SparseFieldSetExpression? sparseFieldSet, ResourceFieldAttribute fieldToExclude)
    {
        return ExcludeFieldChain(sparseFieldSet, new ResourceFieldChainExpression(fieldToExclude));
    }

    private static SparseFieldSetExpression? ExcludeFieldChain(SparseFieldSetExpression? sparseFieldSet, ResourceFieldChainExpression chainToExclude)
    {
        // Design tradeoff: When the sparse fieldset is empty, it means all fields will be selected.
        // Adding an exclusion in that case is a no-op, which results in still retrieving the excluded field from data store.
        // But later, when serializing the response, the sparse fieldset is first populated with all fields,
        // so then the exclusion will actually be applied and the excluded field is not returned to the client.

        if (sparseFieldSet == null)
        {
            return null;
        }

        ImmutableHashSet<ResourceFieldChainExpression> newFieldChains = sparseFieldSet.FieldChains
            .Where(chain => !IsPrefixOrEqual(chainToExclude, chain))
            .ToImmutableHashSet();

        if (newFieldChains.Count == sparseFieldSet.FieldChains.Count)
        {
            return sparseFieldSet;
        }

        return newFieldChains.IsEmpty ? null : new SparseFieldSetExpression(newFieldChains);
    }

    private static bool IsPrefixOrEqual(ResourceFieldChainExpression prefix, ResourceFieldChainExpression target)
    {
        if (prefix.Fields.Count > target.Fields.Count)
        {
            return false;
        }

        for (int index = 0; index < prefix.Fields.Count; index++)
        {
            if (!prefix.Fields[index].Equals(target.Fields[index]))
            {
                return false;
            }
        }

        return true;
    }
}
