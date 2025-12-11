using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ConstellationDefinition(
    IResourceGraph resourceGraph, IClientSettingsProvider clientSettingsProvider, ResourceDefinitionHitCounter hitCounter)
    : HitCountingResourceDefinition<Constellation, long>(resourceGraph, hitCounter)
{
    private readonly IClientSettingsProvider _clientSettingsProvider = clientSettingsProvider;

    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Reading;

    public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
    {
        FilterExpression? baseFilter = base.OnApplyFilter(existingFilter);

        if (_clientSettingsProvider.AreConstellationsVisibleDuringWinterHidden)
        {
            AttrAttribute visibleDuringAttribute = ResourceType.GetAttributeByPropertyName(nameof(Constellation.VisibleDuring));
            var visibleDuringChain = new ResourceFieldChainExpression(visibleDuringAttribute);
            var visibleDuringComparison = new ComparisonExpression(ComparisonOperator.Equals, visibleDuringChain, new LiteralConstantExpression(Season.Winter));
            var notVisibleDuringComparison = new NotExpression(visibleDuringComparison);

            return LogicalExpression.Compose(LogicalOperator.And, baseFilter, notVisibleDuringComparison);
        }

        return baseFilter;
    }
}
