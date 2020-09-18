using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public class Student : Person
    {
        [Attr]
        public string StudentProperty { get; set; }
    }
}
