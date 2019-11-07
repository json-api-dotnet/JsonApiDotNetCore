using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class KebabCasedModel : Identifiable
    {
        [Attr]
        public string CompoundAttr { get; set; }
    }
}
