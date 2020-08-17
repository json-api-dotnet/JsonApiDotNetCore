using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public class KebabCasedModel : Identifiable
    {
        [Attr]
        public string CompoundAttr { get; set; }
    }
}
