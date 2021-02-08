using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Article : Identifiable
    {
        [Attr]
        public string Caption { get; set; }

        [Attr]
        public string Url { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(ArticleTags))]
        public ISet<Tag> Tags { get; set; }
        public ISet<ArticleTag> ArticleTags { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(IdentifiableArticleTags))]
        public ICollection<Tag> IdentifiableTags { get; set; }
        public ICollection<IdentifiableArticleTag> IdentifiableArticleTags { get; set; }
    }
}
