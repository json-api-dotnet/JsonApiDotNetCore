namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class PlaceholderPerson
    {
        public int PlaceHolderId { get; set; }
        
        public Placeholder PlaceHolder { get; set; }

        public int PersonId { get; set; }
        
        public Person Person { get; set; }
    }
}
