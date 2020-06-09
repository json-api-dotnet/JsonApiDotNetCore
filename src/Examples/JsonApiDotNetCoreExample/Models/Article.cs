using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.CustomValidators;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Article : Identifiable
    {
        [Attr]
        [Required(AllowEmptyStrings = true)]
        public string Name { get; set; }

        [HasOne]      
        public Author Author { get; set; }
        public int AuthorId { get; set; }

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
