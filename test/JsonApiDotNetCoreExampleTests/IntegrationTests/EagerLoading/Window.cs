using JetBrains.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Window
    {
        public int Id { get; set; }
        public int HeightInCentimeters { get; set; }
        public int WidthInCentimeters { get; set; }
    }
}
