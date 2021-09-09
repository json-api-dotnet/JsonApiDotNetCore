using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LabelColor
    {
        Red,
        Green,
        Blue
    }
}
