namespace JsonApiDotNetCore.Models
{
    public class HasManyAttribute : RelationshipAttribute
    {
        /// <summary>
        /// Create a HasMany relational link to another entity
        /// </summary>
        /// 
        /// <param name="publicName">The relationship name as exposed by the API</param>
        /// <param name="internalName">The relationship name as defined in the entity layer, if not provided defaults to the variable name</param>
        /// <param name="documentLinks">Which links are available. Defaults to <see cref="Link.All"/></param>
        /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        /// 
        /// <example>
        /// 
        /// <code>
        /// public class Author : Identifiable 
        /// {
        ///     [HasMany("articles"]
        ///     public virtual List<Article> Articles { get; set; }
        /// }
        /// </code>
        /// 
        /// </example>
        public HasManyAttribute(string publicName, string internalName = null, Link documentLinks = Link.All, 
            bool canInclude = true)
        : base(publicName, internalName, documentLinks, canInclude)
        { }

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
