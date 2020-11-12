using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite
{
    public sealed class RgbColor : Identifiable<string>
    {
        [Attr]
        public string DisplayName { get; set; }

        [HasOne]
        public WorkItemGroup Group { get; set; }
    }
}
