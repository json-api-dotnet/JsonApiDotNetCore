using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Tag : Identifiable
    {
        public string Name { get; set; }
    }
}