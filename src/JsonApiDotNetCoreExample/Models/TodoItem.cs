using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItem : Identifiable<int>
    {
        public override int Id { get; set; }
        
        [Attr("description")]
        public string Description { get; set; }
        
        public int OwnerId { get; set; }
        public virtual Person Owner { get; set; }
    }
}
