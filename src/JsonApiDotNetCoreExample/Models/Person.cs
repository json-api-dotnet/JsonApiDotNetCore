using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Person : IIdentifiable
    {
        public int Id { get; set; }
        
        [Attr("firstName")]
        public string FirstName { get; set; }

        [Attr("lastName")]
        public string LastName { get; set; }

        public virtual List<TodoItem> TodoItems { get; set; }
    }
}
