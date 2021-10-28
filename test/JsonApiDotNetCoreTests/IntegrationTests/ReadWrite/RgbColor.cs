using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class RgbColor : Identifiable<string>
    {
        [Attr]
        public string DisplayName { get; set; } = null!;

        [HasOne]
        public WorkItemGroup? Group { get; set; }
    }
}
