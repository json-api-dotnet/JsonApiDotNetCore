using JsonApiDotNetCore.Models;

namespace NoEntityFrameworkExample.Models
{
    [Resource("camelCasedModels")]
    public class CamelCasedModel : Identifiable
    {
        [Attr("compoundAttr")]
        public string CompoundAttr { get; set; }
    }
}
