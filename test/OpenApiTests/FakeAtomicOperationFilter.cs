using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests;

/// <summary>
/// Replaces lookups for usage of <see cref="ResourceAttribute.GenerateControllerEndpoints" /> on resource classes with code-based registrations.
/// </summary>
internal sealed class FakeAtomicOperationFilter : IAtomicOperationFilter
{
    private readonly CodeBasedAtomicOperationFilter _innerFilter = new();

    public bool IsEnabled(ResourceType resourceType, WriteOperationKind writeOperation)
    {
        return _innerFilter.IsEnabled(resourceType, writeOperation);
    }

    public void Register(ResourceType resourceType, JsonApiEndpoints endpoints)
    {
        _innerFilter.Register(resourceType, endpoints);
    }

    private sealed class CodeBasedAtomicOperationFilter : DefaultOperationFilter
    {
        private readonly Dictionary<ResourceType, JsonApiEndpoints> _endpointsPerResourceType = [];

        public void Register(ResourceType resourceType, JsonApiEndpoints endpoints)
        {
            _endpointsPerResourceType[resourceType] = endpoints;
        }

        protected override JsonApiEndpoints? GetJsonApiEndpoints(ResourceType resourceType)
        {
            return _endpointsPerResourceType.GetValueOrDefault(resourceType);
        }
    }
}
