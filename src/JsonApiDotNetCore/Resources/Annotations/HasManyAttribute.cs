using System;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a JSON:API to-many relationship
    /// (https://jsonapi.org/format/#document-resource-object-relationships).
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// public class Author : Identifiable
    /// {
    ///     [HasMany(PublicName = "articles")]
    ///     public List<Article> Articles { get; set; }
    /// }
    /// ]]></code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property)]
    public class HasManyAttribute : RelationshipAttribute
    {
        private static readonly CollectionConverter CollectionConverter = new();

        /// <summary>
        /// Inspects <see cref="RelationshipAttribute.InverseNavigationProperty" /> to determine if this is a many-to-many relationship.
        /// </summary>
        public virtual bool IsManyToMany
        {
            get
            {
                if (InverseNavigationProperty != null)
                {
                    Type elementType = CollectionConverter.TryGetCollectionElementType(InverseNavigationProperty.PropertyType);
                    return elementType != null;
                }

                return false;
            }
        }
    }
}
