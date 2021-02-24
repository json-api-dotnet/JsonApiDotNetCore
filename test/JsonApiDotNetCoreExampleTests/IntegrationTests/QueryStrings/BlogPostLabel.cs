using JetBrains.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class BlogPostLabel
    {
        public int BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; }

        public int LabelId { get; set; }
        public Label Label { get; set; }
    }
}
