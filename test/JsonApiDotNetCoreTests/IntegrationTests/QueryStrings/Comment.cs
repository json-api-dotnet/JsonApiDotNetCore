using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Comment : Identifiable<int>
    {
        [Attr]
        public string Text { get; set; } = null!;

        [Attr]
        public DateTime CreatedAt { get; set; }

        [HasOne]
        public WebAccount Author { get; set; } = null!;

        [HasOne]
        public BlogPost Parent { get; set; } = null!;
    }
}
