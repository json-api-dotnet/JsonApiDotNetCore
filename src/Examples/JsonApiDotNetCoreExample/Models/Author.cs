using JsonApiDotNetCore.Models;
using System.Collections.Generic;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Author : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public List<Article> Articles { get; set; }
    }
}

