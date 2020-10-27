using System;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a json:api to-one relationship (https://jsonapi.org/format/#document-resource-object-relationships).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HasOneAttribute : RelationshipAttribute
    {
        private string _identifiablePropertyName;

        /// <summary>
        /// The foreign key property name. Defaults to <c>"{RelationshipName}Id"</c>.
        /// </summary>
        /// <example>
        /// Using an alternative foreign key:
        /// <code>
        /// public class Article : Identifiable 
        /// {
        ///     [HasOne(PublicName = "author", IdentifiablePropertyName = nameof(AuthorKey)]
        ///     public Author Author { get; set; }
        ///     public int AuthorKey { get; set; }
        /// }
        /// </code>
        /// </example>
        public string IdentifiablePropertyName
        {
            get => _identifiablePropertyName ?? JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(Property.Name);
            set => _identifiablePropertyName = value;
        }

        public HasOneAttribute()
        {
            Links = LinkTypes.NotConfigured;
        }

        /// <inheritdoc />
        public override void SetValue(object resource, object newValue)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            // TODO: Given recent changes, does the following code still need access to foreign keys, or can this be handled by the caller now?

            // If we're deleting the relationship (setting it to null), we set the foreignKey to null.
            // We could also set the actual property to null, but then we would first need to load the
            // current relationship, which requires an extra query.

            var propertyName = newValue == null ? IdentifiablePropertyName : Property.Name;
            var resourceType = resource.GetType();

            var propertyInfo = resourceType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                // we can't set the FK to null because there isn't any.
                propertyInfo = resourceType.GetProperty(RelationshipPath);
            }

            propertyInfo.SetValue(resource, newValue);
        }
    }
}
