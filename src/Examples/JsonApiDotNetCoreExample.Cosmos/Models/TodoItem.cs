using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreExample.Cosmos.Definitions;

namespace JsonApiDotNetCoreExample.Cosmos.Models
{
    /// <summary>
    /// Represents a to-do item that is owned by a person and that can be assigned to another person.
    /// </summary>
    [NoSqlResource]
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TodoItem : Identifiable<Guid>
    {
        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        /// <remarks>
        /// In this example, we are using a generic name for the partition key. The property must exist in all classes the instances of which are to be stored in
        /// the same container.
        /// </remarks>
        [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort)]
        public string PartitionKey
        {
            get => OwnerId.ToString();
            set => OwnerId = Guid.Parse(value);
        }

        /// <summary>
        /// Gets or sets the description of the to-do.
        /// </summary>
        [Attr]
        public string Description { get; set; } = null!;

        /// <summary>
        /// Gets or sets the priority of the to-do.
        /// </summary>
        [Attr]
        public TodoItemPriority Priority { get; set; } = TodoItemPriority.Low;

        /// <summary>
        /// Gets or sets the date and time at which the to-do was initially created.
        /// </summary>
        /// <remarks>
        /// This attribute will be set on the back end by the <see cref="TodoItemDefinition" />.
        /// </remarks>
        [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort | AttrCapabilities.AllowView)]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the to-do was last modified.
        /// </summary>
        /// <remarks>
        /// This attribute will be set on the back end by the <see cref="TodoItemDefinition" />.
        /// </remarks>
        [Attr(PublicName = "modifiedAt", Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort | AttrCapabilities.AllowView)]
        public DateTimeOffset? LastModifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the set of tags assigned to this <see cref="TodoItem" />.
        /// </summary>
        /// <remarks>
        /// Cosmos DB has the concept of owned entities, which we can make accessible as complex attributes.
        /// </remarks>
        [Attr]
        public ISet<Tag> Tags { get; set; } = new HashSet<Tag>();

        /// <summary>
        /// Gets or sets the <see cref="Person.Id" /> of the <see cref="Owner" />.
        /// </summary>
        /// <remarks>
        /// With Cosmos DB, the foreign key must at least be accessible for filtering. Making it viewable is discouraged by the JSON:API specification.
        /// </remarks>
        [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort)]
        public Guid OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Person.Id" /> of the <see cref="Assignee" />.
        /// </summary>
        /// <remarks>
        /// With Cosmos DB, the foreign key must at least be accessible for filtering. Making it viewable is discouraged by the JSON:API specification.
        /// </remarks>
        [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort)]
        public Guid? AssigneeId { get; set; }

        /// <summary>
        /// Gets or sets the owner of this <see cref="TodoItem" />.
        /// </summary>
        /// <remarks>
        /// To enable the navigation of relationships, the name of the foreign key property must be specified for the navigation properties.
        /// </remarks>
        [HasOne]
        [NoSqlHasForeignKey(nameof(OwnerId))]
        public Person Owner { get; set; } = null!;

        /// <summary>
        /// Gets or sets the optional assignee of this <see cref="TodoItem" />.
        /// </summary>
        /// <remarks>
        /// To enable the navigation of relationships, the name of the foreign key property must be specified for the navigation properties.
        /// </remarks>
        [HasOne]
        [NoSqlHasForeignKey(nameof(AssigneeId))]
        public Person? Assignee { get; set; }
    }
}
