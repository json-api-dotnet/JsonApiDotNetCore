using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class CamelCasedModel : Identifiable
    {
        [Attr("compoundAttr")]
        public string CompoundAttr { get; set; }
    }
}
