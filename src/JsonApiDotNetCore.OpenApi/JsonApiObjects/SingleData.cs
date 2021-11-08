using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

#pragma warning disable 8618 // Non-nullable member is uninitialized.

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal abstract class SingleData<TData>
        where TData : ResourceIdentifierObject
    {
        [Required]
        public TData Data { get; set; } = null!;
    }
}
