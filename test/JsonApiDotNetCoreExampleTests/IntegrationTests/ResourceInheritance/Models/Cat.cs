using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public class Cat : Animal
    {
        [Attr]
        public bool UsesLitterBox { get; set; }
    }
}
