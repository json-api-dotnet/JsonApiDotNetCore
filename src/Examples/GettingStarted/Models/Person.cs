using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models
{
    public sealed class Person : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public ICollection<Book> Books { get; set; }
    }
}
