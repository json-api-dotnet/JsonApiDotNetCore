using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ErrorResponseDocument
{
    [Required]
    [JsonPropertyName("errors")]
    public IList<ErrorObject> Errors { get; set; } = new List<ErrorObject>();
}
