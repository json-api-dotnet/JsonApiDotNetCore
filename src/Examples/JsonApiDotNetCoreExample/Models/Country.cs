using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public class Country : Identifiable
    {
        [Attr]
        public string Name { get; set; }
    }
}
