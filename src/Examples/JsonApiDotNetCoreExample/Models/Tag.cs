using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Tag : Identifiable
    {
        [Attr]
        public string Name { get; set; }
    }
}
