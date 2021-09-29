using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <inheritdoc cref="IOperationResourceDataAdapter" />
    public sealed class OperationResourceDataAdapter : ResourceDataAdapter, IOperationResourceDataAdapter
    {
        public OperationResourceDataAdapter(IResourceDefinitionAccessor resourceDefinitionAccessor, IResourceObjectAdapter resourceObjectAdapter)
            : base(resourceDefinitionAccessor, resourceObjectAdapter)
        {
        }

        protected override (IIdentifiable resource, ResourceContext resourceContext) ConvertResourceObject(SingleOrManyData<ResourceObject> data,
            ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            // This override ensures that we enrich IJsonApiRequest before calling into IResourceDefinition, so it is ready for consumption there.

            (IIdentifiable resource, ResourceContext resourceContext) = base.ConvertResourceObject(data, requirements, state);

            state.WritableRequest.PrimaryResource = resourceContext;
            state.WritableRequest.PrimaryId = resource.StringId;

            return (resource, resourceContext);
        }
    }
}
