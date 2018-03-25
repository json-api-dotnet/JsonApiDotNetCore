using JsonApiDotNetCore.Models;
using System.Collections.Generic;

namespace OperationsExample.Models
{
    public class Author : Identifiable
    {
        [Attr("name")]
        public string Name { get; set; }

        [HasMany("articles")]
        public List<Article> Articles { get; set; }
    }
}
