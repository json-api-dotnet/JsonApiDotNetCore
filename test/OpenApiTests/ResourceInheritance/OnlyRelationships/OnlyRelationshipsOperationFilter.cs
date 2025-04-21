using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;

namespace OpenApiTests.ResourceInheritance.OnlyRelationships;

public sealed class OnlyRelationshipsOperationFilter : DefaultOperationFilter
{
    protected override JsonApiEndpoints? GetJsonApiEndpoints(ResourceType resourceType)
    {
        return JsonApiEndpoints.GetRelationship | JsonApiEndpoints.PostRelationship | JsonApiEndpoints.PatchRelationship | JsonApiEndpoints.DeleteRelationship;
    }
}
