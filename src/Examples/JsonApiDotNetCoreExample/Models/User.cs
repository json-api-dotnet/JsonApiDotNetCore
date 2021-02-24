using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class User : Identifiable
    {
        [Attr]
        public string UserName { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        public string Password { get; set; }
    }
}
