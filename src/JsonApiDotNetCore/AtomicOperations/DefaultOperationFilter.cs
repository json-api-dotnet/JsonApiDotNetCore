using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.AtomicOperations;

/// <inheritdoc />
public class DefaultOperationFilter : IAtomicOperationFilter
{
    /// <inheritdoc />
    public virtual bool IsEnabled(ResourceType resourceType, WriteOperationKind writeOperation)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        // To match the behavior of non-operations controllers:
        // If an operation is enabled on a base type, it is implicitly enabled on all derived types.
        ResourceType currentResourceType = resourceType;

        while (true)
        {
            JsonApiEndpoints? endpoints = GetJsonApiEndpoints(currentResourceType);
            bool isEnabled = endpoints != null && Contains(endpoints.Value, writeOperation);

            if (isEnabled || currentResourceType.BaseType == null)
            {
                return isEnabled;
            }

            currentResourceType = currentResourceType.BaseType;
        }
    }

    protected virtual JsonApiEndpoints? GetJsonApiEndpoints(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        var resourceAttribute = resourceType.ClrType.GetCustomAttribute<ResourceAttribute>();
        return resourceAttribute?.GenerateControllerEndpoints;
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
