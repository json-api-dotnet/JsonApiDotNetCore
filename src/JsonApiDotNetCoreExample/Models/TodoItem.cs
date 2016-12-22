using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItem : IIdentifiable
    {
        public int Id { get; set; }
        public virtual Person Owner { get; set; }
    }
}
