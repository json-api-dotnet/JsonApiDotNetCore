using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal abstract class ManyData<TData>
    where TData : ResourceIdentifierObject
{
    [Required]
    public ICollection<TData> Data { get; set; } = null!;
}
