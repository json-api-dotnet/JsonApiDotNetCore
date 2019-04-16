using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TodoItemResource : ResourceDefinition<TodoItem>
    {
        public override IEnumerable<TodoItem> BeforeCreate(IEnumerable<TodoItem> entities, ResourceAction actionSource) { return entities; }
        public override IEnumerable<TodoItem> AfterCreate(IEnumerable<TodoItem> entities, ResourceAction actionSource) { return entities; }

        public override void BeforeDelete(IEnumerable<TodoItem> entities, ResourceAction actionSource) { }
        public override void AfterDelete(IEnumerable<TodoItem> entities, bool succeeded, ResourceAction actionSource) { }
    }
}
