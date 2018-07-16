namespace JsonApiDotNetCore.Models
{
    public class HasOneAttribute : RelationshipAttribute
    {
        /// <summary>
        /// Create a HasOne relational link to another entity
        /// </summary>
        /// 
        /// <param name="publicName">The relationship name as exposed by the API</param>
        /// <param name="internalName">The relationship name as defined in the entity layer, if not provided defaults to the variable name</param>
        /// <param name="documentLinks">Which links are available. Defaults to <see cref="Link.All"/></param>
        /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        /// <param name="withForiegnKey">The foreign key property name. Defaults to <c>"{RelationshipName}Id"</c></param>
        /// 
        /// <example>
        /// Using an alternative foreign key:
        /// 
        /// <code>
        /// public class Article : Identifiable 
        /// {
        ///     [HasOne("author", withForiegnKey: nameof(AuthorKey)]
        ///     public Author Author { get; set; }
        ///     public int AuthorKey { get; set; }
        /// }
        /// </code>
        /// 
        /// </example>
        public HasOneAttribute(string publicName, string internalName = null, Link documentLinks = Link.All, 
            bool canInclude = true, string withForiegnKey = null)
        : base(publicName, internalName, documentLinks, canInclude)
        {
            _explicitIdentifiablePropertyName = withForiegnKey;
        }

        private readonly string _explicitIdentifiablePropertyName;

        /// <summary>
        /// The independent entity identifier.
        /// </summary>
        public string IdentifiablePropertyName => string.IsNullOrWhiteSpace(_explicitIdentifiablePropertyName)
            ? $"{InternalRelationshipName}Id"
            : _explicitIdentifiablePropertyName;

        /// <summary>
        /// Sets the value of the property identified by this attribute
        /// </summary>
        /// <param name="entity">The target object</param>
        /// <param name="newValue">The new property value</param>
        public override void SetValue(object entity, object newValue)
        {
            var propertyName = (newValue?.GetType() == Type)
                ? InternalRelationshipName
                : IdentifiablePropertyName;

            var propertyInfo = entity
                .GetType()
                .GetProperty(propertyName);

            propertyInfo.SetValue(entity, newValue);
        }

        // HACK: this will likely require boxing
        // we should be able to move some of the reflection into the ContextGraphBuilder
        /// <summary>
        /// Gets the value of the independent identifier (e.g. Article.AuthorId)
        /// </summary>
        /// 
        /// <param name="entity">
        /// An instance of dependent resource
        /// </param>
        /// 
        /// <returns>
        /// The property value or null if the property does not exist on the model.
        /// </returns>
        internal object GetIdentifiablePropertyValue(object entity) => entity
                .GetType()
                .GetProperty(IdentifiablePropertyName)
                ?.GetValue(entity);
    }
}
