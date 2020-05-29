using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Model : Identifiable
    {
        [Attr]
        public string DoNotExpose { get; set; }
    }
}
