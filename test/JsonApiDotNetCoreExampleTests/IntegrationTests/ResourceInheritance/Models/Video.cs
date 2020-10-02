using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public class Video : ContentItem
    {
        [Attr]
        public int Duration { get; set; }
    }
}
