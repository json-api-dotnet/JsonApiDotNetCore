using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class NullableResourceIdentifierResponseDocument<TResource> : NullableSingleData<ResourceIdentifierObject<TResource>>
        where TResource : IIdentifiable
    {
        public IDictionary<string, object> Meta { get; set; } = null!;

        public JsonapiObject Jsonapi { get; set; } = null!;

        [Required]
        public LinksInResourceIdentifierDocument Links { get; set; } = null!;
    }
}
