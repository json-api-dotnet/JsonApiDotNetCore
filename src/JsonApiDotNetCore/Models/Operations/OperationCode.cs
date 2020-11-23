using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCore.Models.Operations
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OperationCode
    {
        add,
        update,
        remove
    }
}
