using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StarKind
    {
        Other,
        RedDwarf,
        MainSequence,
        RedGiant
    }
}
