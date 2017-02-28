using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItem : Identifiable<int>
    {
        
        [Attr("description")]
        public string Description { get; set; }

        [Attr("ordinal")]
        public long Ordinal { get; set; }
        
        public int? OwnerId { get; set; }
        public virtual Person Owner { get; set; }
    }
}
