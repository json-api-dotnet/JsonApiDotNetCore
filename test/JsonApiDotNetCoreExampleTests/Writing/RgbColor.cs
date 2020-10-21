using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.Writing
{
    public sealed class RgbColor : Identifiable<string>
    {
        [Attr]
        public string DisplayName { get; set; }
    }
}
