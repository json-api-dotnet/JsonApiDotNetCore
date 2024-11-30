using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;

namespace OpenApiTests.ResourceInheritance.New_OnlyAbstract;

public sealed class OnlyAbstractOperationFilter : DefaultOperationFilter
{
    protected override JsonApiEndpoints? GetJsonApiEndpoints(ResourceType resourceType)
    {
        return resourceType.ClrType.IsAbstract ? JsonApiEndpoints.All : JsonApiEndpoints.None;
    }
}
