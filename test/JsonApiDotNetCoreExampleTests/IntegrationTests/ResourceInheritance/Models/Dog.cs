using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public class Dog : Animal
    {
        [Attr]
        public bool IsGuardDog { get; set; }
    }
}
