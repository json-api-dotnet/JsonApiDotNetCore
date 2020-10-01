using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace MultiDbContextExample.Models
{
    public sealed class ResourceA : Identifiable
    {
        [Attr]
        public string NameA { get; set; }
    }
}
