using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class Male : Person
    {
        [Attr]
        public string MaleProperty { get; set; }
    }
}
