using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

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

        public int? OwnerId { get; set; }
    }
}
