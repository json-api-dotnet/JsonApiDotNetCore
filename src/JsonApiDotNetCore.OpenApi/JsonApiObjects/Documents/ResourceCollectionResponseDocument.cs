using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ResourceCollectionResponseDocument<TResource> : ManyData<ResourceResponseObject<TResource>>
        where TResource : IIdentifiable
    {
        public IDictionary<string, object> Meta { get; set; }

        public JsonapiObject Jsonapi { get; set; }

        [Required]
        public LinksInResourceCollectionDocument Links { get; set; }
    }
}
