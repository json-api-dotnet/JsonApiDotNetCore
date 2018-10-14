using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Tag : Identifiable
    {
        [Attr("name")]
        public string Name { get; set; }
    }
}