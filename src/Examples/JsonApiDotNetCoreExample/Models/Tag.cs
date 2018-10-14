using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Tag : Identifiable
    {
        [Attr]
        public string Name { get; set; }
    }
}