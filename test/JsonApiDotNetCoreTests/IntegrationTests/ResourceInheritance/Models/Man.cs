#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Man : Human
    {
        [Attr]
        public bool HasBeard { get; set; }
    }
}
