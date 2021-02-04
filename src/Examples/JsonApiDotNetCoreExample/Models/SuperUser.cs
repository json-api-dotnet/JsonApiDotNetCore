using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class SuperUser : User
    {
        [Attr]
        public int SecurityLevel { get; set; }
    }
}
