using System;
using System.Threading;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a JSON:API to-one relationship (https://jsonapi.org/format/#document-resource-object-relationships).
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// public class Article : Identifiable
    /// {
    ///     [HasOne]
    ///     public Author Author { get; set; }
    /// }
    /// ]]></code>
    /// </example>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HasOneAttribute : RelationshipAttribute
    {
        private readonly Lazy<bool> _lazyIsOneToOne;

        /// <summary>
        /// Inspects <see cref="RelationshipAttribute.InverseNavigationProperty" /> to determine if this is a one-to-one relationship.
        /// </summary>
        internal bool IsOneToOne => _lazyIsOneToOne.Value;

        public HasOneAttribute()
        {
            _lazyIsOneToOne = new Lazy<bool>(EvaluateIsOneToOne, LazyThreadSafetyMode.PublicationOnly);
        }

        private bool EvaluateIsOneToOne()
        {
            if (InverseNavigationProperty != null)
            {
                Type? elementType = CollectionConverter.TryGetCollectionElementType(InverseNavigationProperty.PropertyType);
                return elementType == null;
            }

            return false;
        }
    }
}
