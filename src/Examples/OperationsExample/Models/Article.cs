using JsonApiDotNetCore.Models;

namespace OperationsExample.Models
{
    public class Article : Identifiable
    {
        [Attr("name")]
        public string Name { get; set; }
    }
}
