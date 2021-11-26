using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    internal sealed class ResourceObjectInPatchRequest<TResource> : ResourceObject<TResource>
        where TResource : IIdentifiable
    {
    }
}
