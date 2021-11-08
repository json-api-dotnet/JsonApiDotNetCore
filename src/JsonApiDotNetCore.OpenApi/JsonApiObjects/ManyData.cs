using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

#pragma warning disable 8618 // Non-nullable member is uninitialized.

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal abstract class ManyData<TData>
        where TData : ResourceIdentifierObject
    {
        [Required]
        public ICollection<TData> Data { get; set; }
    }
}
