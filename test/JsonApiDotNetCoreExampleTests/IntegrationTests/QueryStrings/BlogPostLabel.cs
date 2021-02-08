namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class BlogPostLabel
    {
        public int BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; }

        public int LabelId { get; set; }
        public Label Label { get; set; }
    }
}
