using JsonApiDotNetCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApiDotNetCore.Internal.Contracts
{
    /// <summary>
    /// A cache for the models in entity core
    /// </summary>
    public interface IResourceGraph
    {


        RelationshipAttribute GetInverseRelationship(RelationshipAttribute relationship);
        /// <summary>
        /// Gets the value of the navigation property, defined by the relationshipName,
        /// on the provided instance.
        /// </summary>
        /// <param name="resource">The resource instance</param>
        /// <param name="propertyName">The navigation property name.</param>
        /// <example>
        /// <code>
        /// _graph.GetRelationship(todoItem, nameof(TodoItem.Owner));
        /// </code>
        /// </example>
        /// <remarks>
        /// In the case of a `HasManyThrough` relationship, it will not traverse the relationship 
        /// and will instead return the value of the shadow property (e.g. Articles.Tags).
        /// If you want to traverse the relationship, you should use <see cref="GetRelationshipValue" />.
        /// </remarks>
        object GetRelationship<TParent>(TParent resource, string propertyName);

        /// <summary>
        /// Get the entity type based on a string
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns>The context entity from the resource graph</returns>
        ContextEntity GetEntityType(string entityName);

        /// <summary>
        /// Gets the value of the navigation property (defined by the <see cref="RelationshipAttribute" />)
        /// on the provided instance.
        /// In the case of `HasManyThrough` relationships, it will traverse the through entity and return the 
        /// value of the relationship on the other side of a join entity (e.g. Articles.ArticleTags.Tag).
        /// </summary>
        /// <param name="resource">The resource instance</param>
        /// <param name="relationship">The attribute used to define the relationship.</param>
        /// <example>
        /// <code>
        /// _graph.GetRelationshipValue(todoItem, nameof(TodoItem.Owner));
        /// </code>
        /// </example>
        object GetRelationshipValue<TParent>(TParent resource, RelationshipAttribute relationship) where TParent : IIdentifiable;

        /// <summary>
        /// Get the internal navigation property name for the specified public
        /// relationship name.
        /// </summary>
        /// <param name="relationshipName">The public relationship name specified by a <see cref="HasOneAttribute" /> or <see cref="HasManyAttribute" /></param>
        /// <example>
        /// <code>
        /// _graph.GetRelationshipName&lt;TodoItem&gt;("achieved-date");
        /// // returns "AchievedDate"
        /// </code>
        /// </example>
        string GetRelationshipName<TParent>(string relationshipName);

        /// <summary>
        /// Get the resource metadata by the DbSet property name
        /// </summary>
        ContextEntity GetContextEntity(string dbSetName);

        /// <summary>
        /// Get the resource metadata by the resource type
        /// </summary>
        ContextEntity GetContextEntity(Type entityType);

        /// <summary>
        /// Get the public attribute name for a type based on the internal attribute name.
        /// </summary>
        /// <param name="internalAttributeName">The internal attribute name for a <see cref="AttrAttribute" />.</param>
        string GetPublicAttributeName<TParent>(string internalAttributeName);

        /// <summary>
        /// Was built against an EntityFrameworkCore DbContext ?
        /// </summary>
        bool UsesDbContext { get; }

        ContextEntity GetEntityFromControllerName(string pathParsed);
    }
}
