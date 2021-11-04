using System.Collections.Immutable;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class PlanetDefinition : HitCountingResourceDefinition<Planet, int>
    {
        private readonly IClientSettingsProvider _clientSettingsProvider;

        protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Reading;

        public PlanetDefinition(IResourceGraph resourceGraph, IClientSettingsProvider clientSettingsProvider, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, hitCounter)
        {
            // This constructor will be resolved from the container, which means
            // you can take on any dependency that is also defined in the container.

            _clientSettingsProvider = clientSettingsProvider;
        }

        public override IImmutableSet<IncludeElementExpression> OnApplyIncludes(IImmutableSet<IncludeElementExpression> existingIncludes)
        {
            base.OnApplyIncludes(existingIncludes);

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

        public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
        {
            base.OnApplyFilter(existingFilter);

            if (_clientSettingsProvider.ArePlanetsWithPrivateNameHidden)
            {
                AttrAttribute privateNameAttribute = ResourceType.GetAttributeByPropertyName(nameof(Planet.PrivateName));

                FilterExpression hasNoPrivateName = new ComparisonExpression(ComparisonOperator.Equals, new ResourceFieldChainExpression(privateNameAttribute),
                    NullConstantExpression.Instance);

                return LogicalExpression.Compose(LogicalOperator.And, hasNoPrivateName, existingFilter);
            }

            return existingFilter;
        }
    }
}
