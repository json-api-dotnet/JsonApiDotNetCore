using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class ArticleTag : Identifiable
    {
        public int ArticleId { get; set; }
        public Article Article { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}