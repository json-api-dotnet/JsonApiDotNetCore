namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class LiteraturePerson
    {
        public int LiteratureId { get; set; }
        
        public Literature Literature { get; set; }

        public int PersonId { get; set; }
        
        public Person Person { get; set; }
    }
}
