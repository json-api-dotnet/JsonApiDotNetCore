using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

#pragma warning disable 8618 // Non-nullable member is uninitialized.

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal abstract class ResourceObject<TResource> : ResourceIdentifierObject<TResource>
        where TResource : IIdentifiable
    {
        public IDictionary<string, object> Attributes { get; set; }

        public IDictionary<string, object> Relationships { get; set; }
    }
}
