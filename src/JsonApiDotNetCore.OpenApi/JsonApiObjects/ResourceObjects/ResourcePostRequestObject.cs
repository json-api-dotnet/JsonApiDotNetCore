using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    internal sealed class ResourcePostRequestObject<TResource> : ResourceObject<TResource>
        where TResource : IIdentifiable
    {
    }
}
