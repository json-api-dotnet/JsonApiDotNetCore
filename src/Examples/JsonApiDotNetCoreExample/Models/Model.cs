using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Model : Identifiable
    {
        [Attr]
        public string DoNotExpose { get; set; }
    }
}
