using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class NullableToOneRelationshipResponseData<TResource> : NullableSingleData<ResourceIdentifierObject<TResource>>
        where TResource : IIdentifiable
    {
        [Required]
        public LinksInRelationshipObject Links { get; set; } = null!;

        public IDictionary<string, object> Meta { get; set; } = null!;
    }
}
