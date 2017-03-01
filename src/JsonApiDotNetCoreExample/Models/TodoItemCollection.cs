using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItemCollection : Identifiable
    {
        public string Name { get; set; }
        public virtual List<TodoItem> TodoItems { get; set; }
        public int OwnerId { get; set; }
        public virtual Person Owner { get; set; }
    }
}