using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    internal sealed class ResourceObjectInPostRequest<TResource> : ResourceObject<TResource>
        where TResource : IIdentifiable
    {
    }
}
