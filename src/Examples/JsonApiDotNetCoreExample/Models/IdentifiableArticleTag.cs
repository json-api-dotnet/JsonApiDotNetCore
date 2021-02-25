using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class IdentifiableArticleTag : Identifiable
    {
        public int ArticleId { get; set; }

        [HasOne]
        public Article Article { get; set; }

        public int TagId { get; set; }

        [HasOne]
        public Tag Tag { get; set; }
    }
}
