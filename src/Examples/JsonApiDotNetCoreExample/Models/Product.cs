using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Product : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [HasOne] 
        public Category Category { get; set; }

        public int? CountryId { get; set; }
        
        public string ShopId { get; set; }
    }
}
