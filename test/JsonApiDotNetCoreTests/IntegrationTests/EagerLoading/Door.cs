using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Door
    {
        public int Id { get; set; }
        public string Color { get; set; } = null!;
    }
}
