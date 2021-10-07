using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class MoonDefinition : JsonApiResourceDefinition<Moon>
    {
        private readonly IClientSettingsProvider _clientSettingsProvider;
        private readonly ResourceDefinitionHitCounter _hitCounter;

        public MoonDefinition(IResourceGraph resourceGraph, IClientSettingsProvider clientSettingsProvider, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph)
        {
            // This constructor will be resolved from the container, which means
            // you can take on any dependency that is also defined in the container.

            _clientSettingsProvider = clientSettingsProvider;
            _hitCounter = hitCounter;
        }

        public override IImmutableSet<IncludeElementExpression> OnApplyIncludes(IImmutableSet<IncludeElementExpression> existingIncludes)
        {
            _hitCounter.TrackInvocation<Moon>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnApplyIncludes);

            if (!_clientSettingsProvider.IsMoonOrbitingPlanetAutoIncluded ||
                existingIncludes.Any(include => include.Relationship.Property.Name == nameof(Moon.OrbitsAround)))
            {
                return existingIncludes;
            }

            RelationshipAttribute orbitsAroundRelationship = ResourceType.GetRelationshipByPropertyName(nameof(Moon.OrbitsAround));

            return existingIncludes.Add(new IncludeElementExpression(orbitsAroundRelationship));
        }

        public override QueryStringParameterHandlers<Moon> OnRegisterQueryableHandlersForQueryStringParameters()
        {
            _hitCounter.TrackInvocation<Moon>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnRegisterQueryableHandlersForQueryStringParameters);

            return new QueryStringParameterHandlers<Moon>
            {
                ["isLargerThanTheSun"] = FilterByRadius
            };
        }

        private static IQueryable<Moon> FilterByRadius(IQueryable<Moon> source, StringValues parameterValue)
        {
            bool isFilterOnLargerThan = bool.Parse(parameterValue);
            return isFilterOnLargerThan ? source.Where(moon => moon.SolarRadius > 1m) : source.Where(moon => moon.SolarRadius <= 1m);
        }
    }
}
