using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.Resources;

#pragma warning disable 8618 // Non-nullable member is uninitialized.

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ResourceResponseObject<TResource> : ResourceObject<TResource>
        where TResource : IIdentifiable
    {
        [Required]
        public LinksInResourceObject Links { get; set; }

        public IDictionary<string, object> Meta { get; set; }
    }
}
