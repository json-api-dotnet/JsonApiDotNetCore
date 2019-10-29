using JsonApiDotNetCore.Models;

namespace NoEntityFrameworkExample.Models
{
    public class Tag : Identifiable
    {
        [Attr]
        public string Name { get; set; }
    }
}