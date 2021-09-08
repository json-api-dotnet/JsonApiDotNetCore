using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ResourcePatchRequestDocument<TResource> : SingleData<ResourcePatchRequestObject<TResource>>
        where TResource : IIdentifiable
    {
    }
}
