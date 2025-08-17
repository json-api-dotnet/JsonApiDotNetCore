using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

internal sealed class ObfuscationOperationFilter : DefaultOperationFilter
{
    protected override JsonApiEndpoints? GetJsonApiEndpoints(ResourceType resourceType)
    {
        return resourceType.ClrType.Name == nameof(ObfuscatedIdentifiable) ? JsonApiEndpoints.None : JsonApiEndpoints.All;
    }
}
