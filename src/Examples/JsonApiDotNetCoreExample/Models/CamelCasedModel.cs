using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    [Resource("camelCasedModels")]
    public class CamelCasedModel : Identifiable
    {
        [Attr]
        public string CompoundAttr { get; set; }
    }
}
