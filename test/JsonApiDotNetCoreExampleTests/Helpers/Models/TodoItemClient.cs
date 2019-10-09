using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExampleTests.Helpers.Models
{
    /// <summary>
    /// this "client" version of the <see cref="TodoItem"/> is required because the
    /// base property that is overridden here does not have a setter. For a model
    /// defind on a json:api client, it would not make sense to have an exposed attribute
    /// without a setter.
    /// </summary>
    public class TodoItemClient : TodoItem
    {
        [Attr("calculated-value")]
        public new string CalculatedValue { get; set; }
    }



    [Resource("todo-collections")]
    public class TodoItemCollectionClient : Identifiable<Guid>
    {
        [Attr("name")]
        public string Name { get; set; }
        public int OwnerId { get; set; }

        [HasMany("todo-items")]
        public virtual List<TodoItemClient> TodoItems { get; set; }

        [HasOne("owner")]
        public virtual Person Owner { get; set; }
    }
}
