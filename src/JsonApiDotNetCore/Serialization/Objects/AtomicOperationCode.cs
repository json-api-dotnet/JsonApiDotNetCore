using System.Text.Json.Serialization;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// See "op" in https://jsonapi.org/ext/atomic/#operation-objects.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AtomicOperationCode
{
    Add,
    Update,
    Remove
}
