using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public abstract class Animal : Identifiable
    {
        [Attr]
        public bool Feline { get; set; } 
    }
}
