using JsonApiDotNetCore.Models.Fluent;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class ProductResource: ResourceMapping<Product>
    {
        public ProductResource()
        {
            Property(x => x.Name);
                
            Property(x => x.Description);

            Property(x => x.Price);

            HasMany(x => x.Categories);

            TopLevelLinks()
                .EnableAll();

            ResourceLinks()
                .EnableAll();

            RelationshipLinks()
                .EnableAll();
        }
    }
}
