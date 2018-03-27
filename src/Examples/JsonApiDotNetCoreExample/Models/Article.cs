using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Article : Identifiable
    {
        [Attr("name")]
        public string Name { get; set; }

        [HasOne("author")]
        public Author Author { get; set; }
        public int AuthorId { get; set; }
    }
}
