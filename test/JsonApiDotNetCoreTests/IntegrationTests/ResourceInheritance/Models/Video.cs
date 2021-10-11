#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Video : ContentItem
    {
        [Attr]
        public int Duration { get; set; }
    }
}
