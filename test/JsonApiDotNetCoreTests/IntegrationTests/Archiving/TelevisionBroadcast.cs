using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Archiving")]
    public sealed class TelevisionBroadcast : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; } = null!;

        [Attr]
        public DateTimeOffset AiredAt { get; set; }

        [Attr]
        public DateTimeOffset? ArchivedAt { get; set; }

        [HasOne]
        public TelevisionStation? AiredOn { get; set; }

        [HasMany]
        public ISet<BroadcastComment> Comments { get; set; } = new HashSet<BroadcastComment>();
    }
}
