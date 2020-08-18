using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public class KebabCasedModel : Identifiable
    {
        [Attr]
        public string CompoundAttr { get; set; }
    }
}
