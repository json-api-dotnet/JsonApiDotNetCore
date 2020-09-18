using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public abstract class Person : Identifiable
    {
        [Attr]
        public string InheritedProperty { get; set; }
        
        [HasOne]
        public Article ReviewItem { get; set; }
        
        public int? ReviewItemId { get; set; } 
    }
}
