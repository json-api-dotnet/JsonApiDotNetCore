using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Person : Identifiable<int>
    {
        public override int Id { get; set; }
        
        [Attr("first-name")]
        public string FirstName { get; set; }

        [Attr("last-name")]
        public string LastName { get; set; }

        public virtual List<TodoItem> TodoItems { get; set; }
    }
}
