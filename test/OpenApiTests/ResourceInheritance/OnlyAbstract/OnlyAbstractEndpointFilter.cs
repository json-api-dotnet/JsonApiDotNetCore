using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;

namespace OpenApiTests.ResourceInheritance.OnlyAbstract;

internal sealed class OnlyAbstractEndpointFilter : IJsonApiEndpointFilter
{
    public bool IsEnabled(ResourceType resourceType, JsonApiEndpoints endpoint)
    {
        return resourceType.ClrType.IsAbstract;
    }
}
