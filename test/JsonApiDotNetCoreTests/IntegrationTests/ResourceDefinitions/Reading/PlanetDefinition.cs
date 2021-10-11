#nullable disable

using System.Collections.Immutable;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class PlanetDefinition : JsonApiResourceDefinition<Planet, int>
    {
        private readonly IClientSettingsProvider _clientSettingsProvider;
        private readonly ResourceDefinitionHitCounter _hitCounter;

        public PlanetDefinition(IResourceGraph resourceGraph, IClientSettingsProvider clientSettingsProvider, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph)
        {
            // This constructor will be resolved from the container, which means
            // you can take on any dependency that is also defined in the container.

            _clientSettingsProvider = clientSettingsProvider;
            _hitCounter = hitCounter;
        }

        public override IImmutableSet<IncludeElementExpression> OnApplyIncludes(IImmutableSet<IncludeElementExpression> existingIncludes)
        {
            _hitCounter.TrackInvocation<Planet>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnApplyIncludes);

            if (_clientSettingsProvider.IsIncludePlanetMoonsBlocked &&
                existingIncludes.Any(include => include.Relationship.Property.Name == nameof(Planet.Moons)))
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
                {
                    Title = "Including moons is not permitted."
                });
            }

            return existingIncludes;
        }

        public override FilterExpression OnApplyFilter(FilterExpression existingFilter)
        {
            _hitCounter.TrackInvocation<Planet>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnApplyFilter);

            if (_clientSettingsProvider.ArePlanetsWithPrivateNameHidden)
            {
                AttrAttribute privateNameAttribute = ResourceType.GetAttributeByPropertyName(nameof(Planet.PrivateName));

                FilterExpression hasNoPrivateName = new ComparisonExpression(ComparisonOperator.Equals, new ResourceFieldChainExpression(privateNameAttribute),
                    NullConstantExpression.Instance);

                return existingFilter == null ? hasNoPrivateName : new LogicalExpression(LogicalOperator.And, hasNoPrivateName, existingFilter);
            }

            return existingFilter;
        }
    }
}
