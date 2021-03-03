using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Comment : Identifiable
    {
        [Attr]
        public string Text { get; set; }

        [Attr]
        public DateTime CreatedAt { get; set; }

        [HasOne]
        public WebAccount Author { get; set; }

        [HasOne]
        public BlogPost Parent { get; set; }
    }
}
