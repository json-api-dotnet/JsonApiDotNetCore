using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.FilterValueConversion;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class FilterRewritingResourceDefinition<TResource, TId> : JsonApiResourceDefinition<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    public FilterRewritingResourceDefinition(IResourceGraph resourceGraph)
        : base(resourceGraph)
    {
    }

    public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
    {
        if (existingFilter != null)
        {
            var rewriter = new FilterTimeRangeRewriter();
            return (FilterExpression)rewriter.Visit(existingFilter, null)!;
        }

        return existingFilter;
    }
}
