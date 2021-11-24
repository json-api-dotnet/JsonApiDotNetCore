using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
    public sealed class Man : Human
    {
        [Attr]
        public bool HasBeard { get; set; }
    }
}
