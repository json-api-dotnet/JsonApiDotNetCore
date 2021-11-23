using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal abstract class ResourceObject<TResource> : ResourceIdentifierObject<TResource>
        where TResource : IIdentifiable
    {
        public IDictionary<string, object> Attributes { get; set; } = null!;

        public IDictionary<string, object> Relationships { get; set; } = null!;
    }
}
