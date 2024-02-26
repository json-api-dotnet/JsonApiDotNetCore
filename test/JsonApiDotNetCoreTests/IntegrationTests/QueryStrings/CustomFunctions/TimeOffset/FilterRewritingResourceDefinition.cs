using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.TimeOffset;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class FilterRewritingResourceDefinition<TResource, TId>(IResourceGraph resourceGraph, ISystemClock systemClock)
    : JsonApiResourceDefinition<TResource, TId>(resourceGraph)
    where TResource : class, IIdentifiable<TId>
{
    private readonly FilterTimeOffsetRewriter _rewriter = new(systemClock);

    public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
    {
        if (existingFilter != null)
        {
            return (FilterExpression)_rewriter.Visit(existingFilter, null)!;
        }

        return existingFilter;
    }
}
