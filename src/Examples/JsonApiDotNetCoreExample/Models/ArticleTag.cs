using System;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Data;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class ArticleTag
    {
        public int ArticleId { get; set; }
        public Article Article { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }

        public ArticleTag(AppDbContext appDbContext)
        {
            if (appDbContext == null) throw new ArgumentNullException(nameof(appDbContext));
        }
    }

    public class IdentifiableArticleTag : Identifiable
    {
        public int ArticleId { get; set; }
        [HasOne]
        public Article Article { get; set; }

        public int TagId { get; set; }
        [HasOne]
        public Tag Tag { get; set; }

        public string SomeMetaData { get; set; }
    }
}
