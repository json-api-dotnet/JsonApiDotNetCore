using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Authentication;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.TimeOffset;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class FilterRewritingResourceDefinition<TResource, TId> : JsonApiResourceDefinition<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly FilterTimeOffsetRewriter _rewriter;

    public FilterRewritingResourceDefinition(IResourceGraph resourceGraph, ISystemClock systemClock)
        : base(resourceGraph)
    {
        _rewriter = new FilterTimeOffsetRewriter(systemClock);
    }

    public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
    {
        if (existingFilter != null)
        {
            return (FilterExpression)_rewriter.Visit(existingFilter, null)!;
        }

        return existingFilter;
    }
}
