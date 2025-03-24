using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;

namespace OpenApiTests.ResourceInheritance.OnlyOperations;

public sealed class OnlyOperationsOperationFilter : DefaultOperationFilter
{
    protected override JsonApiEndpoints? GetJsonApiEndpoints(ResourceType resourceType)
    {
        return JsonApiEndpoints.All;
    }
}
