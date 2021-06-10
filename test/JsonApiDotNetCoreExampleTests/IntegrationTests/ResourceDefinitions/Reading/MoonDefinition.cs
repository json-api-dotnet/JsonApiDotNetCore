using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class MoonDefinition : JsonApiResourceDefinition<Moon>
    {
        private readonly IClientSettingsProvider _clientSettingsProvider;

        public MoonDefinition(IResourceGraph resourceGraph, IClientSettingsProvider clientSettingsProvider)
            : base(resourceGraph)
        {
            // This constructor will be resolved from the container, which means
            // you can take on any dependency that is also defined in the container.

            _clientSettingsProvider = clientSettingsProvider;
        }

        public override IReadOnlyCollection<IncludeElementExpression> OnApplyIncludes(IReadOnlyCollection<IncludeElementExpression> existingIncludes)
        {
            if (!_clientSettingsProvider.IsMoonOrbitingPlanetAutoIncluded ||
                existingIncludes.Any(include => include.Relationship.Property.Name == nameof(Moon.OrbitsAround)))
            {
                return existingIncludes;
            }

            ResourceContext resourceContext = ResourceGraph.GetResourceContext<Moon>();

            RelationshipAttribute orbitsAroundRelationship =
                resourceContext.Relationships.Single(relationship => relationship.Property.Name == nameof(Moon.OrbitsAround));

            return new List<IncludeElementExpression>(existingIncludes)
            {
                new IncludeElementExpression(orbitsAroundRelationship)
            };
        }

        public override QueryStringParameterHandlers<Moon> OnRegisterQueryableHandlersForQueryStringParameters()
        {
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
