using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Video : ContentItem
    {
        [Attr]
        public int DurationInSeconds { get; set; }
    }
}
