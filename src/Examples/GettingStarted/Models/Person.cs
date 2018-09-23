using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace GettingStarted.Models
{
    public class Person : Identifiable
    {
        [Attr("name")]
        public string Name { get; set; }

        [HasMany("articles")]
        public List<Article> Articles { get; set; }
    }
}