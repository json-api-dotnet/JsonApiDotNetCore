using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class Female : Person
    {
        [Attr]
        public string FemaleProperty { get; set; }
    }
}
