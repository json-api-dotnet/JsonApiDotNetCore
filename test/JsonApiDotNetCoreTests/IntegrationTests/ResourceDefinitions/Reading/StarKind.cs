#nullable disable

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum StarKind
    {
        Other,
        RedDwarf,
        MainSequence,
        RedGiant
    }
}
