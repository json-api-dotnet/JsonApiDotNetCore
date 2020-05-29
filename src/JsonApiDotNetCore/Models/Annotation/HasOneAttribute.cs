using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Models.Annotation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HasOneAttribute : RelationshipAttribute
    {
        private readonly string _explicitIdentifiablePropertyName;

        /// <summary>
        /// The independent resource identifier.
        /// </summary>
        public string IdentifiablePropertyName =>
            string.IsNullOrWhiteSpace(_explicitIdentifiablePropertyName)
                ? JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(Property.Name)
                : _explicitIdentifiablePropertyName;

        /// <summary>
        /// Create a HasOne relational link to another entity
        /// </summary>
        /// 
        /// <param name="publicName">The relationship name as exposed by the API</param>
        /// <param name="links">Enum to set which links should be outputted for this relationship. Defaults to <see cref="Link.NotConfigured"/> which means that the configuration in
        /// <see cref="ILinksConfiguration"/> or <see cref="ResourceContext"/> is used.</param>
        /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        /// <param name="withForeignKey">The foreign key property name. Defaults to <c>"{RelationshipName}Id"</c></param>
        /// <param name="inverseNavigationProperty"></param>
        /// <example>
        /// Using an alternative foreign key:
        /// 
        /// <code>
        /// public class Article : Identifiable 
        /// {
        ///     [HasOne("author", withForeignKey: nameof(AuthorKey)]
        ///     public Author Author { get; set; }
        ///     public int AuthorKey { get; set; }
        /// }
        /// </code>
        /// </example>
        public HasOneAttribute(string publicName = null, Link links = Link.NotConfigured, bool canInclude = true, string withForeignKey = null, string inverseNavigationProperty = null)
        : base(publicName, links, canInclude)
        {
            _explicitIdentifiablePropertyName = withForeignKey;
            InverseNavigation = inverseNavigationProperty;
        }

        /// <inheritdoc />
        public override void SetValue(object entity, object newValue, IResourceFactory resourceFactory)
        {
            // If we're deleting the relationship (setting it to null), we set the foreignKey to null.
            // We could also set the actual property to null, but then we would first need to load the
            // current relationship, which requires an extra query.

            var propertyName = newValue == null ? IdentifiablePropertyName : Property.Name;
            var resourceType = entity.GetType();

            var propertyInfo = resourceType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                // we can't set the FK to null because there isn't any.
                propertyInfo = resourceType.GetProperty(RelationshipPath);
            }

            propertyInfo.SetValue(entity, newValue);
        }
    }
}
