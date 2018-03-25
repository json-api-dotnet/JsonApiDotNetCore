using JsonApiDotNetCore.Models;

namespace OperationsExample.Models
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
