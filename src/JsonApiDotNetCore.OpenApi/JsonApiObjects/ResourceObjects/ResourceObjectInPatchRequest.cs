using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ResourceObjectInPatchRequest<TResource> : ResourceIdentifierObject
        where TResource : IIdentifiable
    {
        public AttributesInPatchRequest<TResource> Attributes { get; set; } = null!;

        public RelationshipsInPatchRequest<TResource> Relationships { get; set; } = null!;
    }
}
