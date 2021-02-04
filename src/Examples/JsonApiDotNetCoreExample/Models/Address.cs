using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Address : Identifiable
    {
        [Attr]
        public string Street { get; set; }

        [HasOne]
        public Country Country { get; set; }
    }
}
