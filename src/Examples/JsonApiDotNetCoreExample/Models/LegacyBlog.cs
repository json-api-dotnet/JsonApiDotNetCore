using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class LegacyBlog : Identifiable
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
