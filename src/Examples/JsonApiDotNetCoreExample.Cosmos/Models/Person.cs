using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Cosmos.Models
{
    [NoSqlResource]
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Person : Identifiable<Guid>
    {
        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        /// <remarks>
        /// In this example, we are using a generic name for the partition key. We could have used any other sensible name such as PersonId, for example. In any
        /// case, the property must exist in all classes the instances of which are to be stored in the same container. In our example project, both
        /// <see cref="Person" /> and <see cref="TodoItem" /> instances are stored in that container. A <see cref="Person" /> instance and all
        /// <see cref="TodoItem" /> instances owned by that person will thus be stored in the same logical partition.
        /// </remarks>
        [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort)]
        public string PartitionKey
        {
            get => Id.ToString();
            set => Id = Guid.Parse(value);
        }

        /// <summary>
        /// Gets or sets the optional first name.
        /// </summary>
        [Attr]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the required last name.
        /// </summary>
        [Attr]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the set of <see cref="TodoItem" /> instances that are owned by this <see cref="Person" />.
        /// </summary>
        /// <remarks>
        /// To enable the navigation of relationships, the name of the foreign key property must be specified for the navigation properties.
        /// </remarks>
        [HasMany]
        [NoSqlHasForeignKey(nameof(TodoItem.OwnerId))]
        public ISet<TodoItem> OwnedTodoItems { get; set; } = new HashSet<TodoItem>();

        /// <summary>
        /// Gets or sets the set of <see cref="TodoItem" /> instances that are assigned to this <see cref="Person" />.
        /// </summary>
        /// <remarks>
        /// To enable the navigation of relationships, the name of the foreign key property must be specified for the navigation properties.
        /// </remarks>
        [HasMany]
        [NoSqlHasForeignKey(nameof(TodoItem.AssigneeId))]
        public ISet<TodoItem> AssignedTodoItems { get; set; } = new HashSet<TodoItem>();
    }
}
