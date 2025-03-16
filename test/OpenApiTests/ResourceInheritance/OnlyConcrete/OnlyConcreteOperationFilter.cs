using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;

namespace OpenApiTests.ResourceInheritance.OnlyConcrete;

public sealed class OnlyConcreteOperationFilter : DefaultOperationFilter
{
    protected override JsonApiEndpoints? GetJsonApiEndpoints(ResourceType resourceType)
    {
        return resourceType.ClrType.IsAbstract ? JsonApiEndpoints.None : JsonApiEndpoints.All;
    }
}
