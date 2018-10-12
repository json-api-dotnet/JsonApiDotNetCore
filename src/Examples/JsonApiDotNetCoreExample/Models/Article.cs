using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Article : Identifiable
    {
        [Attr("name")]
        public string Name { get; set; }

        [HasOne("author")]
        public Author Author { get; set; }
        public int AuthorId { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(ArticleTags))]
        public List<Tag> Tags { get; set; }

        public List<ArticleTag> ArticleTags { get; set; }
    }
}
