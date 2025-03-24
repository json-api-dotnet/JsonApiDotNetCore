using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;

namespace OpenApiTests.ResourceInheritance.NoOperations;

public sealed class NoOperationsEndpointFilter : IJsonApiEndpointFilter
{
    public bool IsEnabled(ResourceType resourceType, JsonApiEndpoints endpoint)
    {
        return true;
    }
}
