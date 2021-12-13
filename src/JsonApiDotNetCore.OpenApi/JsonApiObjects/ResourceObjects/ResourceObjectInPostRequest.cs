using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceObjectInPostRequest<TResource> : ResourceIdentifierObject
    where TResource : IIdentifiable
{
    public AttributesInPostRequest<TResource> Attributes { get; set; } = null!;

    public RelationshipsInPostRequest<TResource> Relationships { get; set; } = null!;
}
