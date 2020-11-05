using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public class Tag : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public TagColor Color { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(ArticleTags))]
        public ISet<Article> Articles { get; set; }
        public ISet<ArticleTag> ArticleTags { get; set; }
    }
}
