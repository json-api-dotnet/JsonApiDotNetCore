using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ToManyRelationshipInResponse<TResource> : ManyData<ResourceIdentifierObject<TResource>>
    where TResource : IIdentifiable
{
    [Required]
    public LinksInRelationshipObject Links { get; set; } = null!;

    public IDictionary<string, object> Meta { get; set; } = null!;
}
