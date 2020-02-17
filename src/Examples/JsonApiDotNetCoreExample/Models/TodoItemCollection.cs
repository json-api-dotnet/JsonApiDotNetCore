using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    [Resource("todoCollections")]
    public class TodoItemCollection : Identifiable<Guid>
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public List<TodoItem> TodoItems { get; set; }

        [HasOne]
        public Person Owner { get; set; }

        public int? OwnerId { get; set; }
    }
}
