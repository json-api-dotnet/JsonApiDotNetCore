#nullable disable

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum LabelColor
    {
        Red,
        Green,
        Blue
    }
}
