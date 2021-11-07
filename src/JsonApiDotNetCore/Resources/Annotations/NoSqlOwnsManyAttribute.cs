using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to provide additional information for a JSON:API relationship (https://jsonapi.org/format/#document-resource-object-relationships).
    /// </summary>
    /// <remarks>
    /// With Cosmos DB, for example, an entity can own one or many other entity types. In the example below, an Author entity owns many Article entities,
    /// which is also reflected in the Entity Framework Core model. This means that all owned Article entities will be returned with the Author. To represent
    /// Article entities as a to-many relationship, the [HasMany] and [NoSqlOwnsMany] annotations can be combined to indicate to the NoSQL resource service
    /// that those Article resources do not have to be fetched. To include the Article resources into the response, the request will have to add an include
    /// expression such as "?include=articles". Otherwise, the Article resources will not be returned.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// public class Author : Identifiable<Guid>
    /// {
    ///     [HasMany]
    ///     [NoSqlOwnsMany]
    ///     public ICollection<Article> Articles { get; set; }
    /// }
    /// ]]></code>
    /// </example>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public class NoSqlOwnsManyAttribute : Attribute
    {
    }
}
