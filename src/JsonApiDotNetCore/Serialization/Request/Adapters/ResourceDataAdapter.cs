using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc cref="IResourceDataAdapter" />
public class ResourceDataAdapter : BaseAdapter, IResourceDataAdapter
{
    private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
    private readonly IResourceObjectAdapter _resourceObjectAdapter;

    public ResourceDataAdapter(IResourceDefinitionAccessor resourceDefinitionAccessor, IResourceObjectAdapter resourceObjectAdapter)
    {
        ArgumentGuard.NotNull(resourceDefinitionAccessor);
        ArgumentGuard.NotNull(resourceObjectAdapter);

        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _resourceObjectAdapter = resourceObjectAdapter;
    }

    /// <inheritdoc />
    public IIdentifiable Convert(SingleOrManyData<ResourceObject> data, ResourceIdentityRequirements requirements, RequestAdapterState state)
    {
        ArgumentGuard.NotNull(requirements);
        ArgumentGuard.NotNull(state);

        AssertHasData(data, state);

        using IDisposable _ = state.Position.PushElement("data");
        AssertDataHasSingleValue(data, false, state);

        (IIdentifiable resource, ResourceType _) = ConvertResourceObject(data, requirements, state);

        // Ensure that IResourceDefinition extensibility point sees the current operation, in case it injects IJsonApiRequest.
        state.RefreshInjectables();

        _resourceDefinitionAccessor.OnDeserialize(resource);
        return resource;
    }

    protected virtual (IIdentifiable resource, ResourceType resourceType) ConvertResourceObject(SingleOrManyData<ResourceObject> data,
        ResourceIdentityRequirements requirements, RequestAdapterState state)
    {
        return _resourceObjectAdapter.Convert(data.SingleValue!, requirements, state);
    }
}
