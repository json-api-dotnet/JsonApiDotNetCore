using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItem : IIdentifiable
    {
        public int Id { get; set; }
        
        [Attr("description")]
        public string Description { get; set; }
        
        public virtual Person Owner { get; set; }
    }
}
