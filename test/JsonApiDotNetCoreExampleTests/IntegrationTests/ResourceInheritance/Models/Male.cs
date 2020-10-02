using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public sealed class Male : Human
    {
        [Attr]
        public bool HasBeard { get; set; }
    }
}
