using System;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Models
{
    public class HasManyAttribute : RelationshipAttribute
    {
        /// <summary>
        /// Create a HasMany relational link to another entity
        /// </summary>
        /// 
        /// <param name="publicName">The relationship name as exposed by the API</param>
        /// <param name="relationshipLinks">Which links are available. Defaults to <see cref="Link.All"/></param>
        /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        /// <param name="mappedBy">The name of the entity mapped property, defaults to null</param>
        /// 
        /// <example>
        /// 
        /// <code>
        /// public class Author : Identifiable 
        /// {
        ///     [HasMany("articles"]
        ///     public virtual List&lt;Articl&gt; Articles { get; set; }
        /// }
        /// </code>
        /// 
        /// </example>
        public HasManyAttribute(string publicName = null, Link relationshipLinks = Link.All, bool canInclude = true, string mappedBy = null, string inverseNavigationProperty = null)
        : base(publicName, relationshipLinks, canInclude, mappedBy)
        {
            InverseNavigation = inverseNavigationProperty;
        }

        /// <summary>
        /// Gets the value of the navigation property, defined by the relationshipName,
        /// on the provided instance.
        /// </summary>

        public override object GetValue(object entity)
        {
           return entity?.GetType()?
                .GetProperty(InternalRelationshipName)?
                .GetValue(entity);
        }


        /// <summary>
        /// Sets the value of the property identified by this attribute
        /// </summary>
        /// <param name="entity">The target object</param>
        /// <param name="newValue">The new property value</param>
        public override void SetValue(object entity, object newValue)
        {
            var propertyInfo = entity
                .GetType()
                .GetProperty(InternalRelationshipName);

            propertyInfo.SetValue(entity, newValue);
        }
    }
}
