using JsonApiDotNetCore.Models.Fluent;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class CategoryResource: ResourceMapping<Category>
    {
        public CategoryResource()
        {
            Property(x => x.Name);

            Property(x => x.Description);
            
            TopLevelLinks()      
                .DisableAll()
                .EnableSelf(true);
        }
    }
}
