using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ResourceObjectInResponse<TResource> : ResourceObject<TResource>
        where TResource : IIdentifiable
    {
        [Required]
        public LinksInResourceObject Links { get; set; } = null!;

        public IDictionary<string, object> Meta { get; set; } = null!;
    }
}
