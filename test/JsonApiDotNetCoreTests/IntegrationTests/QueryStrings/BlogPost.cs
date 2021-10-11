#nullable disable

using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class BlogPost : Identifiable<int>
    {
        [Attr]
        public string Caption { get; set; }

        [Attr]
        public string Url { get; set; }

        [HasOne]
        public WebAccount Author { get; set; }

        [HasOne]
        public WebAccount Reviewer { get; set; }

        [HasMany]
        public ISet<Label> Labels { get; set; }

        [HasMany]
        public ISet<Comment> Comments { get; set; }

        [HasOne(CanInclude = false)]
        public Blog Parent { get; set; }
    }
}
