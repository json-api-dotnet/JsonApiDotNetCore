using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Address : Identifiable
    {
        [Attr]
        public string Street { get; set; }

        [Attr]
        public string ZipCode { get; set; }

        [HasOne]
        public Country Country { get; set; }
    }
}
