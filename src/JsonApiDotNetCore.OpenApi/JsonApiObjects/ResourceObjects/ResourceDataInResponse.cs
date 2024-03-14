using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceDataInResponse<TResource> : ResourceData
    where TResource : IIdentifiable
{
    [JsonPropertyName("attributes")]
    public AttributesInResponse<TResource> Attributes { get; set; } = null!;

    [JsonPropertyName("relationships")]
    public RelationshipsInResponse<TResource> Relationships { get; set; } = null!;

    // This would normally be { "self": "/people/5" } for GET /todoItems/1/assignee, but it is null when PeopleController is unavailable.
    [JsonPropertyName("links")]
    public LinksInResourceData Links { get; set; } = null!;

    [JsonPropertyName("meta")]
    public IDictionary<string, object> Meta { get; set; } = null!;
}
