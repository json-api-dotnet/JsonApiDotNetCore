namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public sealed class HumanFavoriteContentItem
    {
        public int ContentId { get; set; }
        
        public Content Content { get; set; }

        public int HumanId { get; set; }
        
        public Human Human { get; set; }
    }
}
