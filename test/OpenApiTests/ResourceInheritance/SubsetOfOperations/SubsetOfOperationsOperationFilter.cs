using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using OpenApiTests.ResourceInheritance.Models;

namespace OpenApiTests.ResourceInheritance.SubsetOfOperations;

public sealed class SubsetOfOperationsOperationFilter : DefaultOperationFilter
{
    protected override JsonApiEndpoints? GetJsonApiEndpoints(ResourceType resourceType)
    {
        Type resourceClrType = resourceType.ClrType;

        if (resourceClrType == typeof(Residence))
        {
            return JsonApiEndpoints.Post | JsonApiEndpoints.Patch;
        }

        if (resourceClrType == typeof(FamilyHome))
        {
            return JsonApiEndpoints.GetRelationship | JsonApiEndpoints.PostRelationship;
        }

        if (resourceClrType == typeof(Mansion))
        {
            return JsonApiEndpoints.DeleteRelationship;
        }

        if (resourceClrType == typeof(Room))
        {
            return JsonApiEndpoints.Post | JsonApiEndpoints.PatchRelationship;
        }

        return JsonApiEndpoints.None;
    }
}
