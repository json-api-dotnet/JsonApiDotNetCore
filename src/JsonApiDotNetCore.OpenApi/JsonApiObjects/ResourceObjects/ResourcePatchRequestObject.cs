using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    internal sealed class ResourcePatchRequestObject<TResource> : ResourceObject<TResource>
        where TResource : IIdentifiable
    {
    }
}
