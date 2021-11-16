using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

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
