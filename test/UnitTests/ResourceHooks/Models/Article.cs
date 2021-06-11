using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.ResourceHooks.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Article : Identifiable
    {
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
