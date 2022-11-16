using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Resources.Annotations
{
    public class HasManyAttribute : RelationshipAttribute
    {
        // /// <summary>
        // /// Create a HasMany relational link to another entity
        // /// </summary>
        // ///
        // /// <param name="publicName">The relationship name as exposed by the API</param>
        // /// <param name="documentLinks">Which links are available. Defaults to <see cref="LinkTypes.All"/></param>
        // /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        // /// <param name="mappedBy">The name of the entity mapped property, defaults to null</param>
        // ///
        // /// <example>
        // ///
        // /// <code>
        // /// public class Author : Identifiable
        // /// {
        // ///     [HasMany(PublicName  = "articles"]
        // ///     public virtual List&lt;Articl&gt; Articles { get; set; }
        // /// }
        // /// </code>
        // ///
        // /// </example>
        // public HasManyAttribute(string publicName = null, LinkTypes documentLinks = LinkTypes.All, bool canInclude = true, string mappedBy = null)
        // : base(publicName, documentLinks, canInclude, mappedBy)
        // { }

        /// <summary>
        /// Sets the value of the property identified by this attribute
        /// </summary>
        /// <param name="resource">The target object</param>
        /// <param name="newValue">The new property value</param>
        public override void SetValue(object resource, object newValue)
        {
            var propertyInfo = resource
                .GetType()
                .GetProperty(InternalRelationshipName);

            propertyInfo.SetValue(resource, newValue);
        }
    }
}
