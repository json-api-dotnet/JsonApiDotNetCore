using JsonApiDotNetCore.Models;

namespace NoEntityFrameworkExample.Models
{
    public class MyModel : Identifiable
    {
        [Attr("description")]
        public string Description { get; set; }
    }
}
