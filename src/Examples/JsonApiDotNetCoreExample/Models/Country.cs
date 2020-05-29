using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public class Country : Identifiable
    {
        [Attr]
        public string Name { get; set; }
    }
}
