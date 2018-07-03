using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    [Resource("camelCasedModels")]
    public class CamelCasedModel : Identifiable
    {
        [Attr("compoundAttr")]
        public string CompoundAttr { get; set; }
    }
}
