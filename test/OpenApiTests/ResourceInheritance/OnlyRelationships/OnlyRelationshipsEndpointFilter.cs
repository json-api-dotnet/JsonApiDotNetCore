using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;

namespace OpenApiTests.ResourceInheritance.OnlyRelationships;

public sealed class OnlyRelationshipsEndpointFilter : IJsonApiEndpointFilter
{
    public bool IsEnabled(ResourceType resourceType, JsonApiEndpoints endpoint)
    {
        return endpoint switch
        {
            JsonApiEndpoints.GetRelationship or JsonApiEndpoints.PostRelationship or JsonApiEndpoints.PatchRelationship or
                JsonApiEndpoints.DeleteRelationship => true,
            _ => false
        };
    }
}
