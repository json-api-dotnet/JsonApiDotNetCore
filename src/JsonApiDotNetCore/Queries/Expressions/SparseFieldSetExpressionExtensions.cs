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

        if (sparseFieldSet == null)
        {
            return null;
        }

        SparseFieldTreeBuilder builder = SparseFieldTreeBuilder.FromExpression(sparseFieldSet);

        foreach (ResourceFieldChainExpression chain in resourceGraph.GetFieldChains(fieldSelector))
        {
            builder.IncludeChain(chain.Fields);
        }

        return builder.ToExpression();
    }

    public static SparseFieldSetExpression? Including(this SparseFieldSetExpression? sparseFieldSet, ResourceFieldAttribute field)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (sparseFieldSet == null)
        {
            return null;
        }

        SparseFieldTreeBuilder builder = SparseFieldTreeBuilder.FromExpression(sparseFieldSet);
        builder.IncludeChain([field]);
        return builder.ToExpression();
    }

    public static SparseFieldSetExpression? Including(this SparseFieldSetExpression? sparseFieldSet, ResourceFieldChainExpression fieldChain)
    {
        ArgumentNullException.ThrowIfNull(fieldChain);

        if (sparseFieldSet == null)
        {
            return null;
        }

        SparseFieldTreeBuilder builder = SparseFieldTreeBuilder.FromExpression(sparseFieldSet);
        builder.IncludeChain(fieldChain.Fields);
        return builder.ToExpression();
    }

    public static SparseFieldSetExpression? Excluding<TResource>(this SparseFieldSetExpression? sparseFieldSet,
        Expression<Func<TResource, object?>> fieldSelector, IResourceGraph resourceGraph)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(fieldSelector);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        // Design tradeoff: When the sparse fieldset is null, it means all fields will be selected.
        // Adding an exclusion in that case is a no-op, which results in still retrieving the excluded field from data store.
        // But later, when serializing the response, the sparse fieldset is first populated with all fields,
        // so then the exclusion will actually be applied and the excluded field is not returned to the client.
        if (sparseFieldSet == null)
        {
            return null;
        }

        SparseFieldTreeBuilder builder = SparseFieldTreeBuilder.FromExpression(sparseFieldSet);

        foreach (ResourceFieldChainExpression chain in resourceGraph.GetFieldChains(fieldSelector))
        {
            builder.ExcludeChain(chain.Fields);
        }

        return builder.ToExpression();
    }

    public static SparseFieldSetExpression? Excluding(this SparseFieldSetExpression? sparseFieldSet, ResourceFieldAttribute field)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (sparseFieldSet == null)
        {
            return null;
        }

        SparseFieldTreeBuilder builder = SparseFieldTreeBuilder.FromExpression(sparseFieldSet);
        builder.ExcludeChain([field]);
        return builder.ToExpression();
    }

    public static SparseFieldSetExpression? Excluding(this SparseFieldSetExpression? sparseFieldSet, ResourceFieldChainExpression fieldChain)
    {
        ArgumentNullException.ThrowIfNull(fieldChain);

        if (sparseFieldSet == null)
        {
            return null;
        }

        SparseFieldTreeBuilder builder = SparseFieldTreeBuilder.FromExpression(sparseFieldSet);
        builder.ExcludeChain(fieldChain.Fields);
        return builder.ToExpression();
    }
}
