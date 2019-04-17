using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class ArticleTag
    {
        public int ArticleId { get; set; }
        public Article Article { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }


    public class IdentifiableArticleTag : Identifiable
    {
        public int ArticleId { get; set; }
        [HasOne("article")]
        public Article Article { get; set; }

        public int TagId { get; set; }
        [HasOne("Tag")]
        public Tag Tag { get; set; }

        public string SomeMetaData { get; set; }
    }
}