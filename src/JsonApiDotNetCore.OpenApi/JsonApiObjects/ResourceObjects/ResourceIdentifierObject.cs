using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

#pragma warning disable 8618 // Non-nullable member is uninitialized.

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects
{
    // ReSharper disable once UnusedTypeParameter
    internal class ResourceIdentifierObject<TResource> : ResourceIdentifierObject
        where TResource : IIdentifiable
    {
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal class ResourceIdentifierObject
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string Id { get; set; }
    }
}
