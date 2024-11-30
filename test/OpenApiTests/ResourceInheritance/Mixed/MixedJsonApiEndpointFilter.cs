using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using OpenApiTests.ResourceInheritance.Models;

namespace OpenApiTests.ResourceInheritance.Mixed;

internal sealed class MixedJsonApiEndpointFilter : IJsonApiEndpointFilter
{
    public bool IsEnabled(ResourceType resourceType, JsonApiEndpoints endpoint)
    {
        return resourceType.ClrType.Name switch
        {
            nameof(District) => endpoint == JsonApiEndpoints.GetCollection,
            nameof(Building) => endpoint is JsonApiEndpoints.Post or JsonApiEndpoints.Patch,
            nameof(FamilyHome) => endpoint is JsonApiEndpoints.GetRelationship,
            _ => false
        };
    }
}
