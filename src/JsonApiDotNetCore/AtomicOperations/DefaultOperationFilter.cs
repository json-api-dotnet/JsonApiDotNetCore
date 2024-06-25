using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.AtomicOperations;

/// <inheritdoc />
internal sealed class DefaultOperationFilter : IAtomicOperationFilter
{
    /// <inheritdoc />
    public bool IsEnabled(ResourceType resourceType, WriteOperationKind writeOperation)
    {
        var resourceAttribute = resourceType.ClrType.GetCustomAttribute<ResourceAttribute>();
        return resourceAttribute != null && Contains(resourceAttribute.GenerateControllerEndpoints, writeOperation);
    }

    private static bool Contains(JsonApiEndpoints endpoints, WriteOperationKind writeOperation)
    {
        return writeOperation switch
        {
            WriteOperationKind.CreateResource => endpoints.HasFlag(JsonApiEndpoints.Post),
            WriteOperationKind.UpdateResource => endpoints.HasFlag(JsonApiEndpoints.Patch),
            WriteOperationKind.DeleteResource => endpoints.HasFlag(JsonApiEndpoints.Delete),
            WriteOperationKind.SetRelationship => endpoints.HasFlag(JsonApiEndpoints.PatchRelationship),
            WriteOperationKind.AddToRelationship => endpoints.HasFlag(JsonApiEndpoints.PostRelationship),
            WriteOperationKind.RemoveFromRelationship => endpoints.HasFlag(JsonApiEndpoints.DeleteRelationship),
            _ => false
        };
    }
}
