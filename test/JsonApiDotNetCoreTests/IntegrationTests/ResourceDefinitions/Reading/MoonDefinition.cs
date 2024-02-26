using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
// The constructor parameters will be resolved from the container, which means you can take on any dependency that is also defined in the container.
public sealed class MoonDefinition(IResourceGraph resourceGraph, IClientSettingsProvider clientSettingsProvider, ResourceDefinitionHitCounter hitCounter)
    : HitCountingResourceDefinition<Moon, int>(resourceGraph, hitCounter)
{
    private readonly IClientSettingsProvider _clientSettingsProvider = clientSettingsProvider;

    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Reading;

    public override IImmutableSet<IncludeElementExpression> OnApplyIncludes(IImmutableSet<IncludeElementExpression> existingIncludes)
    {
        base.OnApplyIncludes(existingIncludes);

        if (!_clientSettingsProvider.IsStarGivingLightToMoonAutoIncluded ||
            existingIncludes.Any(include => include.Relationship.Property.Name == nameof(Moon.IsGivenLightBy)))
        {
            return existingIncludes;
        }

        RelationshipAttribute isGivenLightByRelationship = ResourceType.GetRelationshipByPropertyName(nameof(Moon.IsGivenLightBy));

        return existingIncludes.Add(new IncludeElementExpression(isGivenLightByRelationship));
    }

    public override QueryStringParameterHandlers<Moon> OnRegisterQueryableHandlersForQueryStringParameters()
    {
        base.OnRegisterQueryableHandlersForQueryStringParameters();

        return new QueryStringParameterHandlers<Moon>
        {
            ["isLargerThanTheSun"] = FilterByRadius
        };
    }

    private static IQueryable<Moon> FilterByRadius(IQueryable<Moon> source, StringValues parameterValue)
    {
        bool isFilterOnLargerThan = bool.Parse(parameterValue.ToString());
        return isFilterOnLargerThan ? source.Where(moon => moon.SolarRadius > 1m) : source.Where(moon => moon.SolarRadius <= 1m);
    }
}
