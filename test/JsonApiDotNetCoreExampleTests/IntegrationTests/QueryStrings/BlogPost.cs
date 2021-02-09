using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class BlogPost : Identifiable
    {
        [Attr]
        public string Caption { get; set; }

        [Attr]
        public string Url { get; set; }

        [HasOne]
        public WebAccount Author { get; set; }

        [HasOne]
        public WebAccount Reviewer { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(BlogPostLabels))]
        public ISet<Label> Labels { get; set; }
        public ISet<BlogPostLabel> BlogPostLabels { get; set; }

        [HasMany]
        public ISet<Comment> Comments { get; set; }

        [HasOne(CanInclude = false)]
        public Blog Parent { get; set; }
    }
}
