using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class MoonDefinition : JsonApiResourceDefinition<Moon>
    {
        public MoonDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
            // This constructor will be resolved from the container, which means
            // you can take on any dependency that is also defined in the container.
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
