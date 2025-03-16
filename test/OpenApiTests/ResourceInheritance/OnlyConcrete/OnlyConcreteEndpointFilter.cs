using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;

namespace OpenApiTests.ResourceInheritance.OnlyConcrete;

internal sealed class OnlyConcreteEndpointFilter : IJsonApiEndpointFilter
{
    public bool IsEnabled(ResourceType resourceType, JsonApiEndpoints endpoint)
    {
        return !resourceType.ClrType.IsAbstract;
    }
}
