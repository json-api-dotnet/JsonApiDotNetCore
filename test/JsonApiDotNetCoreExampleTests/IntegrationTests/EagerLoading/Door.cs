using JetBrains.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Door
    {
        public int Id { get; set; }
        public string Color { get; set; }
    }
}
