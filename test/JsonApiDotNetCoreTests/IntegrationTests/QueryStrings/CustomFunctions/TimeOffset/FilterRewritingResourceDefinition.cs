using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.TimeOffset;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class FilterRewritingResourceDefinition<TResource, TId>(IResourceGraph resourceGraph, TimeProvider timeProvider)
    : JsonApiResourceDefinition<TResource, TId>(resourceGraph)
    where TResource : class, IIdentifiable<TId>
{
    private readonly FilterTimeOffsetRewriter _rewriter = new(timeProvider);

    public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
    {
        if (existingFilter != null)
        {
            return (FilterExpression)_rewriter.Visit(existingFilter, null)!;
        }

        return existingFilter;
    }
}
