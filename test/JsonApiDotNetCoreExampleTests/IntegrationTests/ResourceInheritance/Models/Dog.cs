using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public class Dog : Animal
    {
        [Attr]
        public Trainability Trainability  { get; set; }
    }

    public enum Trainability
    {
        None,
        Easy,
        Moderate,
        Difficult
    }
}
