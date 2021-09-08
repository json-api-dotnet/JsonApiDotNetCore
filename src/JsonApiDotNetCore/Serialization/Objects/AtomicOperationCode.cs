using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See "op" in https://jsonapi.org/ext/atomic/#operation-objects.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AtomicOperationCode
    {
        Add,
        Update,
        Remove
    }
}
