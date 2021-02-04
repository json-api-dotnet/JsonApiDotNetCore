using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public class Passport : Identifiable
    {
        [Attr]
        public bool IsLocked { get; set; }

        [HasOne]
        public Person Person { get; set; }
    }
}
