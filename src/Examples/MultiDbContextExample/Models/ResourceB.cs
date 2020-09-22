using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace MultiDbContextExample.Models
{
    public sealed class ResourceB : Identifiable
    {
        [Attr]
        public string NameB { get; set; }
    }
}
