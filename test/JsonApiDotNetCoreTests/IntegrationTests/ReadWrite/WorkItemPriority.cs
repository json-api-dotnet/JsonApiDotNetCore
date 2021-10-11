#nullable disable

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum WorkItemPriority
    {
        Low,
        Medium,
        High
    }
}
