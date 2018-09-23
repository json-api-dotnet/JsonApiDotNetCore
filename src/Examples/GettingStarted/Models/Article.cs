using JsonApiDotNetCore.Models;

namespace GettingStarted.Models
{
    public class Article : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [HasOne]
        public Person Author { get; set; }
        public int AuthorId { get; set; }
    }
}