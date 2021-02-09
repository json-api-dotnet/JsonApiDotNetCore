using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class Label : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public LabelColor Color { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(BlogPostLabels))]
        public ISet<BlogPost> Posts { get; set; }
        public ISet<BlogPostLabel> BlogPostLabels { get; set; }
    }
}
