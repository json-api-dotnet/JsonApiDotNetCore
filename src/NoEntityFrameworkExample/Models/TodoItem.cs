using JsonApiDotNetCore.Models;

namespace NoEntityFrameworkExample.Models
{
    public class TodoItem : Identifiable
    {
        [Attr("description")]
        public string Description { get; set; }
    }
}
