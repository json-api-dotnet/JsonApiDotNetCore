using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public class Teacher : Person
    {
        [Attr]
        public string TeacherProperty { get; set; }
    }
}
