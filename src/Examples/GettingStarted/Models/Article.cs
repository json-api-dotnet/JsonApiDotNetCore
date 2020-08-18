using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

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
