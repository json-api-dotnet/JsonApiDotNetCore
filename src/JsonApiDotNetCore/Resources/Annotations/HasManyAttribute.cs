#nullable disable

using System;
using System.Threading;
using JetBrains.Annotations;

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
    ///     [HasMany]
    ///     public ISet<Article> Articles { get; set; }
    /// }
    /// ]]></code>
    /// </example>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HasManyAttribute : RelationshipAttribute
    {
        private readonly Lazy<bool> _lazyIsManyToMany;

        /// <summary>
        /// Inspects <see cref="RelationshipAttribute.InverseNavigationProperty" /> to determine if this is a many-to-many relationship.
        /// </summary>
        internal bool IsManyToMany => _lazyIsManyToMany.Value;

        public HasManyAttribute()
        {
            _lazyIsManyToMany = new Lazy<bool>(EvaluateIsManyToMany, LazyThreadSafetyMode.PublicationOnly);
        }

        private bool EvaluateIsManyToMany()
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
