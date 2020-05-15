using JsonApiDotNetCore.Models;

namespace GettingStarted.Models
{
    public sealed class Article : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [HasOne]
        public Person Author { get; set; }
    }
}
