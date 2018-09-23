using JsonApiDotNetCore.Models;

namespace GettingStarted.Models
{
    public class Article : Identifiable
    {
        [Attr("title")]
        public string Title { get; set; }

        [HasOne("author")]
        public Person Author { get; set; }
        public int AuthorId { get; set; }
    }
}