using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <inheritdoc cref="IResourceDataInOperationsRequestAdapter" />
    public sealed class ResourceDataInOperationsRequestAdapter : ResourceDataAdapter, IResourceDataInOperationsRequestAdapter
    {
        public ResourceDataInOperationsRequestAdapter(IResourceDefinitionAccessor resourceDefinitionAccessor, IResourceObjectAdapter resourceObjectAdapter)
            : base(resourceDefinitionAccessor, resourceObjectAdapter)
        {
        }

        protected override (IIdentifiable resource, ResourceType resourceType) ConvertResourceObject(SingleOrManyData<ResourceObject> data,
            ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            // This override ensures that we enrich IJsonApiRequest before calling into IResourceDefinition, so it is ready for consumption there.

            (IIdentifiable resource, ResourceType resourceType) = base.ConvertResourceObject(data, requirements, state);

            state.WritableRequest!.PrimaryResourceType = resourceType;
            state.WritableRequest.PrimaryId = resource.StringId;

            return (resource, resourceType);
        }
    }
}
