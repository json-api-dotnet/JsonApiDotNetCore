using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class PlanetDefinition : JsonApiResourceDefinition<Planet>
    {
        private readonly IClientSettingsProvider _clientSettingsProvider;

        public PlanetDefinition(IResourceGraph resourceGraph, IClientSettingsProvider clientSettingsProvider)
            : base(resourceGraph)
        {
            // This constructor will be resolved from the container, which means
            // you can take on any dependency that is also defined in the container.

            _clientSettingsProvider = clientSettingsProvider;
        }

        public override IReadOnlyCollection<IncludeElementExpression> OnApplyIncludes(IReadOnlyCollection<IncludeElementExpression> existingIncludes)
        {
            if (_clientSettingsProvider.IsIncludePlanetMoonsBlocked &&
                existingIncludes.Any(include => include.Relationship.Property.Name == nameof(Planet.Moons)))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Including moons is not permitted."
                });
            }

            return existingIncludes;
        }

        public override FilterExpression OnApplyFilter(FilterExpression existingFilter)
        {
            if (_clientSettingsProvider.ArePlanetsWithPrivateNameHidden)
            {
                ResourceContext resourceContext = ResourceGraph.GetResourceContext<Planet>();
                AttrAttribute privateNameAttribute = resourceContext.Attributes.Single(attribute => attribute.Property.Name == nameof(Planet.PrivateName));

                FilterExpression hasNoPrivateName = new ComparisonExpression(ComparisonOperator.Equals, new ResourceFieldChainExpression(privateNameAttribute),
                    new NullConstantExpression());

                return existingFilter == null
                    ? hasNoPrivateName
                    : new LogicalExpression(LogicalOperator.And, ArrayFactory.Create(hasNoPrivateName, existingFilter));
            }

            return existingFilter;
        }
    }
}
