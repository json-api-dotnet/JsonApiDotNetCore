namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public sealed class HumanContentItem
    {
        public int ContentId { get; set; }
        
        public Content Content { get; set; }

        public int HumanId { get; set; }
        
        public Human Human { get; set; }
    }
}
