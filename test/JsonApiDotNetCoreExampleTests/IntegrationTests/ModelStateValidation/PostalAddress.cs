using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class PostalAddress : Identifiable
    {
        [Attr]
        [IsRequired]
        public string StreetAddress { get; set; }
        
        [Attr]
        public string AddressLine2 { get; set; }

        [Attr]
        [IsRequired]
        public string City { get; set; }
        
        [Attr]
        [IsRequired]
        public string Region { get; set; }
        
        [Attr]
        [IsRequired]
        public string ZipCode { get; set; }
    }
}
