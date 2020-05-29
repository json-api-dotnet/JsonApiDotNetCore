using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Blog : Identifiable
    {
        [Attr] 
        public string Title { get; set; }

        [Attr]
        public string CompanyName { get; set; }

        [HasMany]
        public IList<Article> Articles { get; set; }

        [HasOne]
        public Author Owner { get; set; }
    }
}
