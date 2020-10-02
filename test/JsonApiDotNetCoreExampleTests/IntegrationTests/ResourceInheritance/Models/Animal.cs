using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public abstract class Animal : Identifiable
    {
        [Attr]
        public bool IsDomesticated { get; set; }
    }
}
