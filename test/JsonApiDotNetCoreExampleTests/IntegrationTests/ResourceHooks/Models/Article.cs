using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
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
    }
}
