using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCore.Models.Operations
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OperationCode
    {
        get = 1,
        add = 2,
        update = 3,
        remove = 4
    }
}
