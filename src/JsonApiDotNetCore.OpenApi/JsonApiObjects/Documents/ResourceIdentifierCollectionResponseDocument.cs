using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

#pragma warning disable 8618 // Non-nullable member is uninitialized.

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ResourceIdentifierCollectionResponseDocument<TResource> : ManyData<ResourceIdentifierObject<TResource>>
        where TResource : IIdentifiable
    {
        public IDictionary<string, object> Meta { get; set; }

        public JsonapiObject Jsonapi { get; set; }

        [Required]
        public LinksInResourceIdentifierCollectionDocument Links { get; set; }
    }
}
