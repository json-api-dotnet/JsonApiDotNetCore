using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    [Resource("todoCollections")]
    public sealed class TodoItemCollection : Identifiable<Guid>
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public ISet<TodoItem> TodoItems { get; set; }

        [HasOne]
        public Person Owner { get; set; }
    }
}
