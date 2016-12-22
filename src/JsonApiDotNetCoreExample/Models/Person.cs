using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Person : IIdentifiable
    {
        public int Id { get; set; }
        public virtual List<TodoItem> TodoItems { get; set; }
    }
}
