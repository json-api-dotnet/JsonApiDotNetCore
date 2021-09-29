using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <inheritdoc />
    public class ResourceDataAdapter : IResourceDataAdapter
    {
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly IResourceObjectAdapter _resourceObjectAdapter;

        public ResourceDataAdapter(IResourceDefinitionAccessor resourceDefinitionAccessor, IResourceObjectAdapter resourceObjectAdapter)
        {
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));
            ArgumentGuard.NotNull(resourceObjectAdapter, nameof(resourceObjectAdapter));

            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _resourceObjectAdapter = resourceObjectAdapter;
        }

        /// <inheritdoc />
        public IIdentifiable Convert(SingleOrManyData<ResourceObject> data, ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            ArgumentGuard.NotNull(requirements, nameof(requirements));
            ArgumentGuard.NotNull(state, nameof(state));
            AssertHasData(data, state);

            using (state.Position.PushElement("data"))
            {
                AssertNoManyValue(data, state);

                (IIdentifiable resource, ResourceContext _) = ConvertResourceObject(data, requirements, state);

                // Ensure that IResourceDefinition extensibility point sees the current operation, it case it injects IJsonApiRequest.
                state.RefreshInjectables();

                _resourceDefinitionAccessor.OnDeserialize(resource);
                return resource;
            }
        }

        private static void AssertHasData(SingleOrManyData<ResourceObject> data, RequestAdapterState state)
        {
            if (data.Value == null)
            {
                throw new DeserializationException(state.Position, "The 'data' element is required.", null);
            }
        }

        private static void AssertNoManyValue(SingleOrManyData<ResourceObject> data, RequestAdapterState state)
        {
            if (data.ManyValue != null)
            {
                throw new DeserializationException(state.Position, "Expected 'data' object instead of array.", null);
            }
        }

        protected virtual (IIdentifiable resource, ResourceContext resourceContext) ConvertResourceObject(SingleOrManyData<ResourceObject> data,
            ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            return _resourceObjectAdapter.Convert(data.SingleValue, requirements, state);
        }
    }
}
