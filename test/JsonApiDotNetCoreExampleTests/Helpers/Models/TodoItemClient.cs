using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExampleTests.Helpers.Models
{
    /// <summary>
    /// this "client" version of the <see cref="TodoItem"/> is required because the
    /// base property that is overridden here does not have a setter. For a model
    /// defined on a json:api client, it would not make sense to have an exposed attribute
    /// without a setter.
    /// </summary>
    public class TodoItemClient : TodoItem
    {
        [Attr]
        public new string CalculatedValue { get; set; }
    }

    [Resource("todoCollections")]
    public sealed class TodoItemCollectionClient : Identifiable<Guid>
    {
        [Attr]
        public string Name { get; set; }
        public int OwnerId { get; set; }

        [HasMany]
        public ISet<TodoItemClient> TodoItems { get; set; }

        [HasOne]
        public Person Owner { get; set; }
    }
}
