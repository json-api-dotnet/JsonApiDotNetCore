using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using OpenApiTests.ResourceInheritance.Models;

namespace OpenApiTests.ResourceInheritance.SubsetOfVarious;

public sealed class SubsetOfVariousOperationFilter : DefaultOperationFilter
{
    protected override JsonApiEndpoints? GetJsonApiEndpoints(ResourceType resourceType)
    {
        Type resourceClrType = resourceType.ClrType;

        if (resourceClrType == typeof(District))
        {
            return JsonApiEndpoints.GetCollection;
        }

        if (resourceClrType == typeof(Building))
        {
            return JsonApiEndpoints.Post | JsonApiEndpoints.Patch;
        }

        if (resourceClrType == typeof(FamilyHome))
        {
            return JsonApiEndpoints.GetRelationship;
        }

        if (resourceClrType == typeof(CyclePath))
        {
            return JsonApiEndpoints.GetSingle;
        }

        return JsonApiEndpoints.None;
    }
}
