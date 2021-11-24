using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace SourceGeneratorDebugger.Models
{
    public sealed class Customer : Identifiable<long>
    {
        [Attr]
        public string Name { get; set; } = null!;
    }
}
