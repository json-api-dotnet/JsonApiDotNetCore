using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ResourceObjectInResponse<TResource> : ResourceIdentifierObject
        where TResource : IIdentifiable
    {
        public AttributesInResponse<TResource> Attributes { get; set; } = null!;

        public RelationshipsInResponse<TResource> Relationships { get; set; } = null!;

        [Required]
        public LinksInResourceObject Links { get; set; } = null!;

        public IDictionary<string, object> Meta { get; set; } = null!;
    }
}
