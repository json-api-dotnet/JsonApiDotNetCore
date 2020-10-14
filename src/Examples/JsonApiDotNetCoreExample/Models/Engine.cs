using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Engine : Identifiable
    {
        [Attr]
        public string SerialCode { get; set; }

        [HasOne] 
        public Car Car { get; set; }
    }
}
